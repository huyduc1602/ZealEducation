using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Education.Areas.Admin.Data.BusinessModel.Interface;
using Education.Areas.Admin.Data.DataModel;
using Education.BLL;
using Education.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;

namespace Education.Areas.Admin.Controllers
{
    [CustomizeAuthorize]
    public class LearningInfoController : Controller
    {
        private IPaginationService paginationService;
        private IRepository<LearningInfo> learningInfoRepository;
        private ILearningInfoService LearningInfoService;
        private IRepository<Exam> examRepository;
        private IRepository<ClassRoom> batchRepository;
        private IRepository<Candicate> candicateRepository;
        private IRepository<User> userRepository;
        private IBatchModelService batchModelService;
        public LearningInfoController()
        {
            paginationService = new DbPaginationService();
            learningInfoRepository = new DbRepository<LearningInfo>();
            LearningInfoService = new DbLearningInfoService();
            examRepository = new DbRepository<Exam>();
            batchRepository = new DbRepository<ClassRoom>();
            candicateRepository = new DbRepository<Candicate>();
            userRepository = new DbRepository<User>();
            batchModelService = new DbBatchModelService();

        }
        public ActionResult Details(int id)
        {
            LearningInfo view = learningInfoRepository.FindById(id);
            view.Candicate = candicateRepository.FindById(view.CandicateId);
            view.User = userRepository.FindById(view.UserId);
            view.Exam = examRepository.FindById(view.ExamId);
            view.ClassRoom = batchRepository.FindById(view.RoomId);
            return View(view);
        }
        // GET: Admin/LearningInfo
        public ActionResult Index(string message)
        {
            var learningInfo = learningInfoRepository.Get();
            if (message != null)
            {
                ViewBag.Info = message;
            }
            return View(learningInfo);
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key = null)
        {
            var learningInfo = learningInfoRepository.Get();

            foreach (var item in learningInfo)
            {
                item.Candicate = candicateRepository.FindById(item.CandicateId);
                item.User = userRepository.FindById(item.UserId);
                item.Exam = examRepository.FindById(item.ExamId);
                item.ClassRoom = batchRepository.FindById(item.RoomId);
            }
            if (!string.IsNullOrEmpty(Key))
            {
                learningInfo = learningInfo.Where(x => x.Candicate.Name.Contains(Key)
                                        || x.ClassRoom.Name.Contains(Key)
                                      );
            }
            Pagination pagination = paginationService.getInfoPaginate(learningInfo.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(learningInfo.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImportData(HttpPostedFileBase excelFile)
        {
            var message = "";
            if (null == excelFile)
            {
                message = "Please select a excel file";
            }
            else
            {
                if (excelFile.FileName.EndsWith("csv")
                    || excelFile.FileName.EndsWith("xls")
                    || excelFile.FileName.EndsWith("xlsx")
                    )
                {
                    string path = Server.MapPath("~/Areas/Admin/Content/assets/excel/" + excelFile.FileName);
                    FileInfo f = new FileInfo(path);
                    if (!f.Exists)
                    {
                        excelFile.SaveAs(path);
                    }
                    Excel.Application application = new Excel.Application();
                    Excel.Workbook workbook = application.Workbooks.Open(path);
                    Excel.Worksheet worksheet = workbook.ActiveSheet;
                    Excel.Range range = worksheet.UsedRange;
                    List<LearningInfoModel> learningInfoModel = new List<LearningInfoModel>();
                    for (int i = 2; i <= range.Rows.Count; i++)
                    {
                        string batchCode = ((Excel.Range)range.Cells[i, 1]).Text;
                        int batchId = 0;
                        if (batchRepository.Get(r => r.Code.Equals(batchCode)).Count() > 0)
                        {
                            batchId = batchRepository.Get(r => r.Code.Equals(batchCode)).FirstOrDefault().Id;
                        }
                        string candicateCode = ((Excel.Range)range.Cells[i, 2]).Text;
                        int candicateId = 0;
                        if (candicateRepository.Get(c => c.Code.Equals(candicateCode)).Count() > 0)
                        {
                            candicateId = candicateRepository.Get(c => c.Code.Equals(candicateCode)).FirstOrDefault().Id;
                        }
                        int status = int.Parse(((Excel.Range)range.Cells[i, 3]).Text);
                        if (learningInfoRepository.CheckDuplicate(
                            x => x.RoomId == batchId
                            && x.CandicateId == candicateId
                        ))
                        {
                            message += "Students " + candicateRepository.FindById(candicateId).Code + " have been in class, invite add other students";
                        }
                        else if (batchId == 0)
                        {
                            message += "Batch Code " + batchCode + " does not exist. ";
                        }
                        else if (candicateId == 0)
                        {
                            message += "Candicate code " + candicateCode + " does not exist. ";
                        }
                        else
                        {
                            LearningInfoModel model = new LearningInfoModel();
                            model.BatchId = batchId;
                            model.CandicateId = candicateId;
                            model.Status = status;
                            model.Tuition = batchRepository.FindById(batchId).Course.SalePrice;
                            learningInfoModel.Add(model);
                        }
                    }
                    User user = (User)Session["user"];
                    var UserId = user.Id;
                    foreach (var model in learningInfoModel)
                    {
                        LearningInfo info = LearningInfoService.convertLearningInfo(model);
                        info.UserId = UserId;
                        learningInfoRepository.Add(info);
                    }
                    message += "Add candicate to class room successfully!";
                }
                else
                {
                    message = "Please select a excel file";
                }
            }
            return RedirectToAction("Index", new { message = message });
        }
        public ActionResult Edit(int id)
        {
            LearningInfoModel infoModel = LearningInfoService.convertLearningModel(learningInfoRepository.FindById(id));
            infoModel.BatchName = batchRepository.FindById(infoModel.BatchId).Name;
            infoModel.CandicateName = candicateRepository.FindById(infoModel.CandicateId).Name;
            List<object> status = new List<object> {
                        new { value = 1, text = "Join the class" },
                        new { value = 0, text = "Change class" },
                    };
            ViewBag.Status = new SelectList(status.ToList(), "value", "text", infoModel.Status);
            ViewBag.ExamId = new SelectList(examRepository.Get(), "Id", "Name", infoModel.ExamId);
            return View(infoModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LearningInfoModel model)
        {
            if (model.Tuition > 0 && model.Tuition > batchRepository.FindById(model.BatchId).Course.Price)
            {
                ModelState.AddModelError("Tuition", "Enter tuition fees are not greater than total money courses");
            }
            if (model.Tuition > 0 && model.TuitionPaid > model.Tuition)
            {
                ModelState.AddModelError("TuitionPaid", "Enter tuition paid fees are not greater than tuition");
            }
            if (ModelState.IsValid)
            {
                User user = (User)Session["user"];
                var UserId = user.Id;
                LearningInfo info = LearningInfoService.convertEditLearningInfo(model);
                info.UserId = UserId;
                if (learningInfoRepository.Edit(info))
                {
                    return RedirectToAction("Index");
                }
            }
            List<object> status = new List<object> {
                        new { value = 1, text = "Join the class" },
                        new { value = 0, text = "Change class" },
                    };
            ViewBag.Status = new SelectList(status.ToList(), "value", "text", model.Status);
            ViewBag.ExamId = new SelectList(examRepository.Get(), "Id", "Name", model.ExamId);
            return View(model);
        }
        public ActionResult Remove(int id)
        {
            LearningInfo learning = learningInfoRepository.FindById(id);
            if (learningInfoRepository.Remove(learning))
            {
                return Json(new
                {
                    StatusCode = 200,
                    message = "Delete successful student in class",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { StatusCode = 400, message = "Delete student in class failed!" }, JsonRequestBehavior.AllowGet);
        }
    }
}