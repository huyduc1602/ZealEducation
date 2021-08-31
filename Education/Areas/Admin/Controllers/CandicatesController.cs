using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Education.BLL;
using Education.DAL;
using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Newtonsoft.Json;
using Education.Areas.Admin.Data.DataModel;
using System.IO;
using Education.Areas.Admin.Data.BusinessModel.Interface;
using Excel = Microsoft.Office.Interop.Excel;

namespace Education.Areas.Admin.Controllers
{
    [CustomizeAuthorize]
    public class CandicatesController : Controller
    {
        protected EducationManageDbContext context;
        private IRepository<Candicate> candicateRepository;
        private IRepository<User> userRepository;
        private IRepository<GroupUser> groupUserRepository;
        private IRepository<Exam> examRepository;
        private IRepository<ClassRoom> classRoomUserRepository;
        private IPaginationService paginationService;
        private ICandicateModelService CandicateModelService;
        private IUserService userService;
        public CandicatesController()
        {
            candicateRepository = new DbRepository<Candicate>();
            userRepository = new DbRepository<User>();
            examRepository = new DbRepository<Exam>();
            groupUserRepository = new DbRepository<GroupUser>();
            classRoomUserRepository = new DbRepository<ClassRoom>();
            paginationService = new DbPaginationService();
            CandicateModelService = new DbCandicateModelService();
            userService = new DbUserSevice();
        }

        // GET: Admin/Candicates
        public ActionResult Index(string message)
        {
            if (message != null)
            {
                ViewBag.Info = message;
            }
            var candicates = candicateRepository.Get();
            return View(candicates);
        }

        public ActionResult GetData(int CurrentPage, int Limit, string Key = null)
        {
            var cadicate = candicateRepository.Get();
            if (!string.IsNullOrEmpty(Key))
            {
                cadicate = cadicate.Where(x => x.Name.Contains(Key)
                                            || x.Code.Contains(Key)
                                            || x.ParentName.Contains(Key)
                                            || x.ParentPhone.Contains(Key)
                                            || x.Email.Contains(Key)
                                            || x.Phone.Contains(Key));
            }
            Pagination pagination = paginationService.getInfoPaginate(cadicate.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(cadicate.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }
        // GET: Admin/Candicates/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Candicate candicate = candicateRepository.FindById(id);
            User u = userRepository.FindById(candicate.UserId);
            CandicateModel model = CandicateModelService.ConvertCandicateModel(candicate, u);
            if (candicate == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // GET: Admin/Candicates/Create
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
                    List<CandicateModel> candicateModels = new List<CandicateModel>();
                    for (int i = 2; i <= range.Rows.Count; i++)
                    {
                        CandicateModel model = new CandicateModel();
                        model.Code = ((Excel.Range)range.Cells[i, 1]).Text;
                        model.Name = ((Excel.Range)range.Cells[i, 2]).Text;
                        model.ParentName = ((Excel.Range)range.Cells[i, 3]).Text;
                        model.Email = ((Excel.Range)range.Cells[i, 4]).Text;
                        model.Phone = ((Excel.Range)range.Cells[i, 5]).Text;
                        model.ParentPhone = ((Excel.Range)range.Cells[i, 6]).Text;
                        model.ImageDisplay = ((Excel.Range)range.Cells[i, 7]).Text;
                        model.Gender = int.Parse(((Excel.Range)range.Cells[i, 8]).Text);
                        model.Address = ((Excel.Range)range.Cells[i, 9]).Text;
                        model.Birthday = DateTime.Parse(((Excel.Range)range.Cells[i, 10]).Text);
                        model.JoiningDate = DateTime.Parse(((Excel.Range)range.Cells[i, 11]).Text);
                        model.FullName = ((Excel.Range)range.Cells[i, 2]).Text;
                        model.UserName = ((Excel.Range)range.Cells[i, 12]).Text;
                        model.Password = ((Excel.Range)range.Cells[i, 13]).Text;
                        if (userRepository.CheckDuplicate(x => x.UserName.Equals(model.UserName)))
                        {
                            message += "Username" + model.UserName + "available. ";
                        }
                        else if (candicateRepository.CheckDuplicate(x => x.Code.Equals(model.Code)))
                        {
                            message += "Code " + model.Code + "available. ";
                        }
                        else
                        {
                            candicateModels.Add(model);
                        }
                    }
                    foreach (var model in candicateModels)
                    {
                        // Create User candicate
                        User user = CandicateModelService.ConvertUser(model);
                        user.Password = userService.GetMD5(user.Password);
                        if (userRepository.Add(user))
                        {
                            User u = userRepository.Get(x => x.UserName.Equals(model.UserName)).FirstOrDefault();
                            Candicate student = CandicateModelService.ConvertCandicate(model);
                            student.Image = model.ImageDisplay;
                            student.UserId = u.Id;
                            candicateRepository.Add(student);
                        }
                    }
                    message += "Create Candicate Successfully!";
                }
                else
                {
                    message = "Please select a excel file";
                }
            }
            return RedirectToAction("Index", new { message = message });
        }
        public ActionResult Create()
        {
            List<object> gender = new List<object> {
                        new { value = 2, text = "Select Gender" },
                        new { value = 0, text = "Female" },
                        new { value = 1, text = "Male" },
                    };
            SelectList listItems = new SelectList(gender.ToList(), "value", "text");
            ViewBag.Gender = listItems;
            return View();
        }

        // POST: Admin/Candicates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CandicateModel candicate)
        {
            DateTime now = DateTime.Today;
            // Check Unique Data 
            if (candicateRepository.CheckDuplicate(x => x.Code.Equals(candicate.Code)))
            {
                ModelState.AddModelError("Code", "Student code available");
            }
            else if (candicateRepository.CheckDuplicate(x => x.Email.Equals(candicate.Email)))
            {
                ModelState.AddModelError("Email", "Email available");
            }
            else if (userRepository.CheckDuplicate(x => x.UserName.Equals(candicate.UserName)))
            {
                ModelState.AddModelError("UserName", "Username available");
            }
            else if (candicate.Gender == 2)
            {
                ModelState.AddModelError("Gender", "You must choose the student's gender");
            }
            else if (candicate.Image == null) // check null image
            {
                ModelState.AddModelError("Image", "Student images are not empty");
            }
            else if ((now.Year - candicate.Birthday.Year) < 16)
            {
                ModelState.AddModelError("Birthday", "Students must be over 16 years old");
            }
            if (ModelState.IsValid)
            {
                if (candicate.Image != null && candicate.Image.ContentLength > 0)
                {
                    // Create User candicate
                    User user = CandicateModelService.ConvertUser(candicate);
                    user.Password = userService.GetMD5(user.Password);
                    if (userRepository.Add(user))
                    {
                        User u = userRepository.Get(x => x.UserName.Equals(candicate.UserName)).SingleOrDefault();
                        // Create info candicate
                        string lastName = candicate.Image.FileName;
                        string[] words = lastName.Split('.');
                        int size = words.Count();
                        string fileName = candicate.UserName + "-" + DateTime.Now.ToString("H-m-dd-M-yyyy") + "." + words[size - 1];
                        Candicate student = CandicateModelService.ConvertCandicate(candicate);
                        student.Image = fileName;
                        student.UserId = u.Id;
                        if (candicateRepository.Add(student))
                        {
                            // save image candicate
                            string path = Path.Combine(Server.MapPath("~/Areas/Admin/Content/assets/img/student/"), fileName);
                            candicate.Image.SaveAs(path);
                            return RedirectToAction("Index");
                        }
                    }
                }
            }
            List<object> gender = new List<Object> {
                        new { value = 2, text = "Select Gender" },
                        new { value = 0, text = "Female" },
                        new { value = 1, text = "Male" },
                    };
            SelectList listItems = new SelectList(gender.ToList(), "value", "text", candicate.Gender);
            ViewBag.Gender = listItems;
            return View(candicate);
        }

        // GET: Admin/Candicates/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Candicate candicate = candicateRepository.FindById(id);
            User u = userRepository.FindById(candicate.UserId);
            CandicateModel model = CandicateModelService.ConvertCandicateModel(candicate, u);
            if (candicate == null)
            {
                return HttpNotFound();
            }
            List<object> gender = new List<object> {
                        new { value = 2, text = "Select Gender" },
                        new { value = 0, text = "Female" },
                        new { value = 1, text = "Male" },
                    };
            ViewBag.Gender = new SelectList(gender.ToList(), "value", "text", model.Gender);
            //ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", candicate.UserId);
            return View(model);
        }

        // POST: Admin/Candicates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CandicateModel candicate)
        {
            DateTime now = DateTime.Today;
            // Check Unique Data 
            if (candicateRepository.CheckDuplicate(x => x.Code.Equals(candicate.Code)) && candicateRepository.Get(x => x.Code.Equals(candicate.Code)).First().Id != candicate.Id)
            {
                ModelState.AddModelError("Code", "Student code available");
            }
            else if (candicateRepository.CheckDuplicate(x => x.Email.Equals(candicate.Email)) && candicateRepository.Get(x => x.Email.Equals(candicate.Email)).First().Id != candicate.Id)
            {
                ModelState.AddModelError("Email", "Email available");
            }
            else if (userRepository.CheckDuplicate(x => x.UserName.Equals(candicate.UserName)) && candicateRepository.FindById(candicate.Id).UserId != userRepository.Get(x => x.UserName.Equals(candicate.UserName)).First().Id)
            {
                ModelState.AddModelError("UserName", "Username available");
            }
            else if (candicate.Gender == 2)
            {
                ModelState.AddModelError("Gender", "You must choose the student's gender");
            }
            else if ((now.Year - candicate.Birthday.Year) < 16)
            {
                ModelState.AddModelError("Birthday", "Students must be over 16 years old");
            }
            if (ModelState.IsValid)
            {
                Candicate old = candicateRepository.FindById(candicate.Id);
                // Update User Candicate
                User uOld = userRepository.FindById(old.UserId);
                User user = CandicateModelService.ConvertEditUser(candicate, uOld);
                user.Password = userService.GetMD5(user.Password);
                if (userRepository.Edit(user))
                {
                    // Update info candicate
                    Candicate student = CandicateModelService.ConvertEditCandicate(candicate, old);
                    if (candicate.Image != null)
                    {
                        string lastName = candicate.Image.FileName;
                        string[] words = lastName.Split('.');
                        int size = words.Count();
                        string fileName = candicate.UserName + "-" + DateTime.Now.ToString("H-m-dd-M-yyyy") + "." + words[size - 1];
                        string file = Server.MapPath("~/Areas/Admin/Content/assets/img/student/" + old.Image);
                        FileInfo f = new FileInfo(file);
                        if (f.Exists)
                        {
                            f.Delete();
                        }
                        // save image candicate
                        string path = Path.Combine(Server.MapPath("~/Areas/Admin/Content/assets/img/student/"), fileName);
                        candicate.Image.SaveAs(path);
                        student.Image = fileName;
                    }
                    candicateRepository.Edit(student);
                    return RedirectToAction("Index");
                }
            }
            List<object> gender = new List<Object> {
                        new { value = 2, text = "Select Gender" },
                        new { value = 0, text = "Female" },
                        new { value = 1, text = "Male" },
                    };
            ViewBag.Gender = new SelectList(gender.ToList(), "value", "text", candicate.Gender);
            return View(candicate);
        }

        public ActionResult Remove(int id)
        {
            Candicate candicate = candicateRepository.FindById(id);
            if (candicate.LearningInfos.Count() > 0)
            {
                return Json(new { StatusCode = 400, message = "Deleting students unsuccessful. Students entered classes!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                candicateRepository.Remove(candicate);
                // Delete Image Old
                string file = Server.MapPath("~/Areas/Admin/Content/assets/img/student/" + candicate.Image);
                FileInfo f = new FileInfo(file);
                if (f.Exists)
                {
                    f.Delete();
                }
                userRepository.Remove(candicate.UserId);
                return Json(new
                {
                    StatusCode = 200,
                    message = "Delete successful candicate!",
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DetailsCandicate()
        {
            var candicateId = (int)Session["CandicateId"];
            Candicate candicate = candicateRepository.FindById(candicateId);
            IEnumerable<LearningInfo> learningInfo = candicate.LearningInfos;
            return View(learningInfo);
        }
        public ActionResult GetDetailsCandicate(int CurrentPage, int Limit, string Key = null)
        {
            var candicateId = (int)Session["CandicateId"];
            Candicate candicate = candicateRepository.FindById(candicateId);
            IEnumerable<LearningInfo> learningInfo = candicate.LearningInfos;

            foreach (var item in learningInfo.ToList())
            {
                item.Candicate = candicateRepository.FindById(item.CandicateId);
                item.User = userRepository.FindById(item.UserId);
                item.Exam = examRepository.FindById(item.ExamId);
                item.ClassRoom = classRoomUserRepository.FindById(item.RoomId);
            }
            if (!string.IsNullOrEmpty(Key))
            {
                learningInfo = learningInfo.Where(x => x.Exam.Name.Contains(Key)
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
    }
}
