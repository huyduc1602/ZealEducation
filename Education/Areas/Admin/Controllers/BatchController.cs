using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Education.Areas.Admin.Data.BusinessModel.Interface;
using Education.Areas.Admin.Data.DataModel;
using Education.Areas.Admin.Data.ViewModel;
using Education.BLL;
using Education.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;

namespace Education.Areas.Admin.Controllers
{
    /*[CustomizeAuthorize]*/
    public class BatchController : Controller
    {
        private IPaginationService paginationService;
        private IRepository<ClassRoom> batchRepository;
        private IRepository<Course> courseRepository;
        private IRepository<Faulty> faultyRepository;
        private IRepository<User> userRepository;
        private IRepository<Candicate> candicateRepository;
        private IRepository<ClassRoomFaulty> roomFaultyRepository;
        private IRepository<Exam> examRepository;
        private IRepository<LearningInfo> learningInfoRepository;
        private IBatchModelService batchModelService;
        private ILearningInfoService LearningInfoService;
        public BatchController()
        {
            paginationService = new DbPaginationService();
            batchRepository = new DbRepository<ClassRoom>();
            courseRepository = new DbRepository<Course>();
            faultyRepository = new DbRepository<Faulty>();
            userRepository = new DbRepository<User>();
            roomFaultyRepository = new DbRepository<ClassRoomFaulty>();
            batchModelService = new DbBatchModelService();
            candicateRepository = new DbRepository<Candicate>();
            examRepository = new DbRepository<Exam>();
            LearningInfoService = new DbLearningInfoService();
            learningInfoRepository = new DbRepository<LearningInfo>();
        }
        // GET: Admin/Batch
        public ActionResult Index(string message)
        {
            if (message != null)
            {
                ViewBag.Info = message;
            }
            var batchs = batchRepository.Get();
            IEnumerable<BatchView> view = batchModelService.convertListBatchView(batchs);
            return View(view);
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key = null)
        {
            var batchs = batchModelService.convertListBatchView(batchRepository.Get());
            if (!string.IsNullOrEmpty(Key))
            {
                batchs = batchs.Where(x => x.Code.Contains(Key)
                                        || x.Name.Contains(Key)
                                        || x.Course.Contains(Key)
                                        || x.Faulty.Contains(Key)
                                        || x.FaultyOld.Contains(Key)
                                        || x.User.Contains(Key)
                                      );
            }
            Pagination pagination = paginationService.getInfoPaginate(batchs.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(batchs.Skip((CurrentPage - 1) * Limit).Take(Limit));
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
                    List<BatchModel> batchModels = new List<BatchModel>();
                    User user = (User)Session["user"];
                    var UserId = user.Id;
                    for (int i = 2; i <= range.Rows.Count; i++)
                    {
                        string batchName = ((Excel.Range)range.Cells[i, 1]).Text;
                        string FaultyCode = ((Excel.Range)range.Cells[i, 2]).Text;
                        string CourseCode = ((Excel.Range)range.Cells[i, 3]).Text;
                        int CourseId = 0;
                        int FaultyId = 0;
                        if (courseRepository.Get(c => c.Code.Equals(CourseCode)).Count() > 0)
                        {
                            CourseId = courseRepository.Get(c => c.Code.Equals(CourseCode)).FirstOrDefault().Id;
                        }
                        if (faultyRepository.Get(c => c.Code.Equals(FaultyCode)).Count() > 0)
                        {
                            FaultyId = faultyRepository.Get(c => c.Code.Equals(FaultyCode)).FirstOrDefault().Id;
                        }
                        int status = int.Parse(((Excel.Range)range.Cells[i, 4]).Text);
                        if (batchRepository.CheckDuplicate(
                            x => x.Name == batchName
                        ))
                        {
                            message += "Class room " + batchName + " exists. ";
                        }
                        else if (CourseId == 0)
                        {
                            message += "Course Code " + CourseCode + " not exists. ";
                        }
                        else if (FaultyId == 0)
                        {
                            message += "Faulty Code " + FaultyCode + " not exists. ";
                        }
                        else
                        {
                            BatchModel model = new BatchModel();
                            model.Name = batchName;
                            model.StartDate = DateTime.Parse(((Excel.Range)range.Cells[i, 5]).Text);
                            model.FaultyId = FaultyId;
                            model.CourseId = CourseId;
                            model.Status = status;
                            batchModels.Add(model);
                        }
                    }
                    foreach (var model in batchModels)
                    {
                        ClassRoom info = batchModelService.convertBatch(model, UserId);
                        batchRepository.Add(info);
                        ClassRoomFaulty classRoomFaulty = new ClassRoomFaulty
                        {
                            RoomId = batchRepository.Get(x => x.Name.Equals(model.Name)).First().Id,
                            FaultyId = model.FaultyId,
                            Status = true,
                        };
                        roomFaultyRepository.Add(classRoomFaulty);
                    }
                    message += "Create Batch Successfully!";
                }
                else
                {
                    message = "Please select a excel file";
                }
            }
            return RedirectToAction("Index", new { message = message });
        }
        // GET: Admin/Candicates/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BatchView view = batchModelService.convertBatchView(batchRepository.FindById(id));
            if (view == null)
            {
                return HttpNotFound();
            }
            return View(view);
        }

        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(courseRepository.Get(), "Id", "Name");
            ViewBag.FaultyId = new SelectList(faultyRepository.Get(), "Id", "Name");
            List<object> status = new List<Object> {
                        new { value = 1, text = "New" },
                        new { value = 2, text = "Classes are starting" },
                        new { value = 3, text = "The class has ended." },
                    };
            ViewBag.Status = new SelectList(status.ToList(), "value", "text");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BatchModel model)
        {
            DateTime now = DateTime.Today;
            if (batchRepository.CheckDuplicate(x => x.Name.Equals(model.Name)))
            {
                ModelState.AddModelError("Name", "Batch name available");
            }
            else if (model.StartDate < now)
            {
                ModelState.AddModelError("StartDate", "Admission date must be the future day");
            }
            if (ModelState.IsValid)
            {
                User user = (User)Session["user"];
                var UserId = user.Id;
                ClassRoom batch = batchModelService.convertBatch(model, UserId);
                if (batchRepository.Add(batch))
                {
                    ClassRoomFaulty classRoomFaulty = new ClassRoomFaulty
                    {
                        RoomId = batchRepository.Get(x => x.Code.Equals(batch.Code)).First().Id,
                        FaultyId = model.FaultyId,
                        Status = true,
                    };
                    roomFaultyRepository.Add(classRoomFaulty);
                    return RedirectToAction("Index");
                }
            }
            ViewBag.CourseId = new SelectList(courseRepository.Get(), "Id", "Name", model.CourseId);
            ViewBag.FaultyId = new SelectList(faultyRepository.Get(), "Id", "Name", model.FaultyId);
            List<object> status = new List<Object> {
                        new { value = 1, text = "New" },
                        new { value = 2, text = "Classes are starting" },
                        new { value = 3, text = "The class has ended." },
                    };
            ViewBag.Status = new SelectList(status.ToList(), "value", "text", model.Status);
            return View(model);
        }
        public ActionResult Edit(int? id)
        {
            ClassRoom batch = batchRepository.FindById(id);
            int? FaultyId = batch.ClassRoomFaulties.Where(x => x.Status).First().FaultyId;
            BatchModel model = batchModelService.convertBatchModel(batch, FaultyId);
            ViewBag.CourseId = new SelectList(courseRepository.Get(), "Id", "Name", batch.CourseId);
            ViewBag.FaultyId = new SelectList(faultyRepository.Get(), "Id", "Name", FaultyId);
            List<object> status = new List<Object> {
                        new { value = 1, text = "New" },
                        new { value = 2, text = "Classes are starting" },
                        new { value = 3, text = "The class has ended." },
                    };
            ViewBag.Status = new SelectList(status.ToList(), "value", "text", model.Status);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BatchModel model)
        {
            DateTime now = DateTime.Today;
            if (batchRepository.CheckDuplicate(x => x.Name.Equals(model.Name)) && batchRepository.Get(x => x.Name.Equals(model.Name)).First().Id != model.Id)
            {
                ModelState.AddModelError("Name", "Batch name available");
            }
            else if (model.StartDate < now)
            {
                ModelState.AddModelError("StartDate", "Admission date must be the future day");
            }
            if (ModelState.IsValid)
            {
                User user = (User)Session["user"];
                var UserId = user.Id;
                ClassRoom old = batchRepository.FindById(model.Id);
                int? FaultyId = old.ClassRoomFaulties.Where(x => x.Status).First().FaultyId;
                ClassRoom batch = batchModelService.convertEditBatch(model, old);
                batch.UserId = UserId;
                ClassRoomFaulty classRoom = old.ClassRoomFaulties.Where(x => x.FaultyId == model.FaultyId).FirstOrDefault();
                if (classRoom != null && classRoom.Status)
                {
                    classRoom.Status = true;
                    roomFaultyRepository.Edit(classRoom);
                }
                else if (classRoom != null && classRoom.Status == false)
                {
                    ClassRoomFaulty roomFaulty = old.ClassRoomFaulties.Where(x => x.Status).FirstOrDefault();
                    roomFaulty.Status = false;
                    roomFaultyRepository.Edit(roomFaulty);
                    classRoom.Status = true;
                    roomFaultyRepository.Edit(classRoom);
                }
                else if (classRoom == null)
                {
                    ClassRoomFaulty classRoomFaultyOld = old.ClassRoomFaulties.Where(x => x.Status).First();
                    classRoomFaultyOld.Status = false;
                    roomFaultyRepository.Edit(classRoomFaultyOld);
                    ClassRoomFaulty classRoomFaulty = new ClassRoomFaulty
                    {
                        RoomId = model.Id,
                        FaultyId = model.FaultyId,
                        Status = true,
                    };
                    roomFaultyRepository.Add(classRoomFaulty);
                }
                batchRepository.Edit(batch);
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(courseRepository.Get(), "Id", "Name", model.CourseId);
            ViewBag.FaultyId = new SelectList(faultyRepository.Get(), "Id", "Name", model.FaultyId);
            List<object> status = new List<Object> {
                        new { value = 1, text = "New" },
                        new { value = 2, text = "Classes are starting" },
                        new { value = 3, text = "The class has ended." },
                    };
            ViewBag.Status = new SelectList(status.ToList(), "value", "text", model.Status);
            return View(model);
        }
        public ActionResult Remove(int id)
        {
            ClassRoom classRoom = batchRepository.FindById(id);
            if (classRoom.LearningInfos.Count() >0 || classRoom.ClassRoomFaulties.Count() > 0)
            {
                return Json(new { StatusCode = 400, message = "Delete classrooms failed. Classrooms are having students!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (batchRepository.Remove(classRoom))
                {
                    return Json(new
                    {
                        StatusCode = 200,
                        message = "Delete successful classes",
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { StatusCode = 400, message = "Delete classrooms failed. Classrooms are having students!" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddStudent(int id)
        {
            List<object> status = new List<object> {
                        new { value = 1, text = "Join the class" },
                        new { value = 0, text = "Change class" },
                    };
            ViewBag.Status = new SelectList(status.ToList(), "value", "text");
            ViewBag.CandicateId = new SelectList(candicateRepository.Get(), "Id", "Name");
            ViewBag.ExamId = new SelectList(examRepository.Get(), "Id", "Name");
            Course course = batchRepository.FindById(id).Course;
            var price = course.SalePrice;
            LearningInfoModel infoModel = new LearningInfoModel();
            infoModel.BatchId = id;
            infoModel.BatchName = batchRepository.FindById(id).Name;
            infoModel.Tuition = price;
            return View(infoModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStudent(LearningInfoModel model)
        {
            var list = learningInfoRepository.Get(x => x.RoomId == model.BatchId);
            if (list.Where(x => x.CandicateId == model.CandicateId).Count() > 0)
            {
                ModelState.AddModelError("CandicateId", "Students have been in class, invite add other students");
            }
            if (model.TuitionPaid > 0 && model.TuitionPaid > model.Tuition)
            {
                ModelState.AddModelError("TuitionPaid", "Enter tuition paid fees are not greater than tuition");
            }
            if (ModelState.IsValid)
            {
                User user = (User)Session["user"];
                var UserId = user.Id;
                LearningInfo info = LearningInfoService.convertLearningInfo(model);
                info.UserId = UserId;
                if (learningInfoRepository.Add(info))
                {
                    return RedirectToAction("Index");
                }
            }
            List<object> status = new List<object> {
                        new { value = 1, text = "Join the class" },
                        new { value = 0, text = "Change class" },
                    };
            ViewBag.Status = new SelectList(status.ToList(), "value", "text");
            ViewBag.CandicateId = new SelectList(candicateRepository.Get(), "Id", "Name");
            ViewBag.ExamId = new SelectList(examRepository.Get(), "Id", "Name");
            return View(model);
        }
        
    }
}