﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Education.Areas.Admin.Data.BusinessModel.Interface;
using Education.Areas.Admin.Data.DataModel;
using Education.Areas.Admin.Data.ViewModel;
using Education.BLL;
using Education.DAL;
using Newtonsoft.Json;
using Excel = Microsoft.Office.Interop.Excel;

namespace Education.Areas.Admin.Controllers
{
    [CustomizeAuthorize]
    public class FaultiesController : Controller
    {
        private IRepository<Faulty> faultyRepository;
        private IPaginationService paginationService;
        private IFaultyModelService faultyModelService;
        private IRepository<User> userRepository;
        private IRepository<ClassRoomFaulty> roomFaultyrepository;
        private IUserService userService;

        public FaultiesController()
        {
            userRepository = new DbRepository<User>();
            faultyRepository = new DbRepository<Faulty>();
            roomFaultyrepository = new DbRepository<ClassRoomFaulty>();
            paginationService = new DbPaginationService();
            faultyModelService = new DbFaultyModelService();
            userService = new DbUserSevice();
        }
        // GET: Admin/Faulties
        public ActionResult Index(string message)
        {
            if (message != null)
            {
                ViewBag.Info = message;
            }
            IEnumerable<FaultyView> faultyView = faultyModelService.ConvertListFaultyView(faultyRepository.Get().ToList());
            return View(faultyView);
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key = null)
        {
            IEnumerable<FaultyView> faulty = faultyModelService.ConvertListFaultyView(faultyRepository.Get().ToList());
            if (!string.IsNullOrEmpty(Key))
            {
                faulty = faulty.Where(x => x.Name.Contains(Key)
                                            || x.Code.Contains(Key)
                                            || x.Email.Contains(Key)
                                            || x.Email.Contains(Key)
                                            || x.Phone.Contains(Key));
            }
            Pagination pagination = paginationService.getInfoPaginate(faulty.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(faulty.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Faulties/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faulty faulty = faultyRepository.FindById(id);
            User u = userRepository.FindById(faulty.UserId);
            FaultyModel model = faultyModelService.ConvertFaultyModel(faulty, u);
            if (faulty == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }
        // Import data with excel
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
                    List<FaultyModel> faultyModels = new List<FaultyModel>();
                    for (int i = 2; i <= range.Rows.Count; i++)
                    {
                        FaultyModel model = new FaultyModel();
                        model.Code = ((Excel.Range)range.Cells[i, 1]).Text;
                        model.Name = ((Excel.Range)range.Cells[i, 2]).Text;
                        model.Email = ((Excel.Range)range.Cells[i, 3]).Text;
                        model.Phone = ((Excel.Range)range.Cells[i, 4]).Text;
                        model.ImageDisplay = ((Excel.Range)range.Cells[i, 5]).Text;
                        model.Gender = int.Parse(((Excel.Range)range.Cells[i, 6]).Text);
                        model.Address = ((Excel.Range)range.Cells[i, 7]).Text;
                        model.Birthday = DateTime.Parse(((Excel.Range)range.Cells[i, 8]).Text);
                        model.Salary = double.Parse(((Excel.Range)range.Cells[i, 9]).Text);
                        model.Qualification = ((Excel.Range)range.Cells[i, 10]).Text;
                        model.FullName = ((Excel.Range)range.Cells[i, 2]).Text;
                        model.UserName = ((Excel.Range)range.Cells[i, 11]).Text;
                        model.Password = ((Excel.Range)range.Cells[i, 12]).Text;
                        if (userRepository.CheckDuplicate(x => x.UserName.Equals(model.UserName)))
                        {
                            message += "Username " + model.UserName + "available. ";
                        }
                        else if (faultyRepository.CheckDuplicate(x => x.Code.Equals(model.Code)))
                        {
                            message += "Code " + model.Code + "available. ";
                        }
                        else
                        {
                            faultyModels.Add(model);
                        }
                    }
                    foreach (var model in faultyModels)
                    {
                        // Create User candicate
                        User user = faultyModelService.ConvertUser(model);
                        user.Password = userService.GetMD5(user.Password);
                        if (userRepository.Add(user))
                        {
                            User u = userRepository.Get(x => x.UserName.Equals(model.UserName)).FirstOrDefault();
                            Faulty faulty = faultyModelService.ConvertFaulty(model);
                            faulty.Image = model.ImageDisplay;
                            faulty.UserId = u.Id;
                            faultyRepository.Add(faulty);
                        }
                    }
                    message += "Create Faulty Successfully!";
                }
                else
                {
                    message = "Please select a excel file";
                }
            }
            return RedirectToAction("Index", new { message = message });
        }

        // GET: Admin/Faulties/Create
        public ActionResult Create()
        {
            List<object> gender = new List<object> {
                        new { value = 2, text = "Select Gender" },
                        new { value = 0, text = "Female" },
                        new { value = 1, text = "Male" },
                    };
            ViewBag.Gender = new SelectList(gender.ToList(), "value", "text");
            return View();
        }

        // POST: Admin/Faulties/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FaultyModel model)
        {
            DateTime now = DateTime.Now;
            // Check Unique Data 
            if (faultyRepository.CheckDuplicate(x => x.Code.Equals(model.Code)))
            {
                ModelState.AddModelError("Code", "Faulty code available");
            }
            else if (faultyRepository.CheckDuplicate(x => x.Email.Equals(model.Email)))
            {
                ModelState.AddModelError("Email", "Email available");
            }
            else if (userRepository.CheckDuplicate(x => x.UserName.Equals(model.UserName)))
            {
                ModelState.AddModelError("UserName", "Username available");
            }
            else if (model.Gender == 2)
            {
                ModelState.AddModelError("Gender", "You must choose the student's gender");
            }
            else if (model.Image == null) // check null image
            {
                ModelState.AddModelError("Image", "Faulty images are not empty");
            }
            else if ((now.Year - model.Birthday.Year) < 22)
            {
                ModelState.AddModelError("Birthday", "Faulty must be over 22 years old");
            }
            if (ModelState.IsValid)
            {
                if (model.Image != null && model.Image.ContentLength > 0)
                {
                    // Create User fautly
                    User user = faultyModelService.ConvertUser(model);
                    user.Password = userService.GetMD5(user.Password);
                    if (userRepository.Add(user))
                    {
                        User u = userRepository.Get(x => x.UserName.Equals(model.UserName)).SingleOrDefault();
                        // Create info fautly
                        string lastName = model.Image.FileName;
                        string[] words = lastName.Split('.');
                        int size = words.Count();
                        string fileName = model.UserName + "-" + DateTime.Now.ToString("H-m-dd-M-yyyy") + "." + words[size - 1];
                        Faulty faulty = faultyModelService.ConvertFaulty(model);
                        faulty.Image = fileName;
                        faulty.UserId = u.Id;
                        if (faultyRepository.Add(faulty))
                        {
                            // save image candicate
                            string path = Path.Combine(Server.MapPath("~/Areas/Admin/Content/assets/img/faulty"), fileName);
                            model.Image.SaveAs(path);
                            return RedirectToAction("Index");
                        }
                    }
                }
            }
            else
            {
                List<object> gender = new List<object> {
                        new { value = 2, text = "Select Gender" },
                        new { value = 0, text = "Female" },
                        new { value = 1, text = "Male" },
                    };
                ViewBag.Gender = new SelectList(gender.ToList(), "value", "text", model.Gender);
            }
            return View(model);
        }

        // GET: Admin/Faulties/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faulty faulty = faultyRepository.FindById(id);
            User u = userRepository.FindById(faulty.UserId);
            FaultyModel model = faultyModelService.ConvertFaultyModel(faulty, u);
            if (faulty == null)
            {
                return HttpNotFound();
            }
            List<object> gender = new List<object> {
                        new { value = 2, text = "Select Gender" },
                        new { value = 0, text = "Female" },
                        new { value = 1, text = "Male" },
                    };
            ViewBag.Gender = new SelectList(gender.ToList(), "value", "text", model.Gender);
            return View(model);
        }

        // POST: Admin/Faulties/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FaultyModel faulty)
        {
            DateTime now = DateTime.Now;
            if (faultyRepository.CheckDuplicate(x => x.Code.Equals(faulty.Code)) && faultyRepository.Get(x => x.Code.Equals(faulty.Code)).First().Id != faulty.Id)
            {
                ModelState.AddModelError("Code", "Faulty code available");
            }
            else if (faultyRepository.CheckDuplicate(x => x.Email.Equals(faulty.Email)) && faultyRepository.Get(x => x.Email.Equals(faulty.Email)).First().Id != faulty.Id)
            {
                ModelState.AddModelError("Email", "Email available");
            }
            else if (userRepository.CheckDuplicate(x => x.UserName.Equals(faulty.UserName)) && faultyRepository.FindById(faulty.Id).UserId != userRepository.Get(x => x.UserName.Equals(faulty.UserName)).First().Id)
            {
                ModelState.AddModelError("UserName", "Username available");
            }
            else if (faulty.Gender == 2)
            {
                ModelState.AddModelError("Gender", "You must choose the Faulty's gender");
            }
            else if ((now.Year - faulty.Birthday.Year) < 16)
            {
                ModelState.AddModelError("Birthday", "Students must be over 16 years old");
            }
            if (ModelState.IsValid)
            {
                Faulty old = faultyRepository.FindById(faulty.Id);
                // Update User Candicate
                User uOld = userRepository.FindById(old.UserId);
                User user = faultyModelService.ConvertUser(faulty, uOld);
                user.Password = userService.GetMD5(user.Password);
                if (userRepository.Edit(user))
                {
                    // Update info candicate
                    Faulty model = faultyModelService.ConvertFaulty(faulty, old);
                    if (faulty.Image != null)
                    {
                        string lastName = faulty.Image.FileName;
                        string[] words = lastName.Split('.');
                        int size = words.Count();
                        string fileName = faulty.UserName + "-" + DateTime.Now.ToString("H-m-dd-M-yyyy") + "." + words[size - 1];
                        // Delete Image Old
                        string file = Server.MapPath("~/Areas/Admin/Content/assets/img/faulty/" + old.Image);
                        FileInfo f = new FileInfo(file);
                        if (f.Exists)
                        {
                            f.Delete();
                        }
                        // save image candicate
                        string path = Path.Combine(Server.MapPath("~/Areas/Admin/Content/assets/img/faulty"), fileName);
                        faulty.Image.SaveAs(path);
                        model.Image = fileName;
                    }
                    faultyRepository.Edit(model);
                    return RedirectToAction("Index");
                }
            }
            List<object> gender = new List<Object> {
                        new { value = 2, text = "Select Gender" },
                        new { value = 0, text = "Female" },
                        new { value = 1, text = "Male" },
                    };
            ViewBag.Gender = new SelectList(gender.ToList(), "value", "text", faulty.Gender);
            return View(faulty);
        }

        public ActionResult Remove(int id)
        {
            Faulty faulty = faultyRepository.FindById(id);
            if (faulty.ClassRoomFaulties.Count() > 0)
            {
                return Json(new { StatusCode = 400, message = "Deleting lecturers failed. Lecturers are receiving classes!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                faultyRepository.Remove(faulty);
                // Delete Image Old
                string file = Server.MapPath("~/Areas/Admin/Content/assets/img/faulty/" + faulty.Image);
                FileInfo f = new FileInfo(file);
                if (f.Exists)
                {
                    f.Delete();
                }
                userRepository.Remove(faulty.UserId);
                return Json(new
                {
                    StatusCode = 200,
                    message = "Delete successful lecturers!",
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DetailsFaulty()
        {
            var faultyId = (int)Session["FaultyId"];
            Faulty faulty = faultyRepository.FindById(faultyId);
            IEnumerable<ClassRoomFaulty> roomFaulties = roomFaultyrepository.Get(x => x.FaultyId == faultyId);
            foreach (var item in roomFaulties)
            {
                item.Faulty = faulty;
            }
            return View(roomFaulties);
        }
        public ActionResult GetDetailsFaulty(int CurrentPage, int Limit, string Key = null)
        {
            var faultyId = (int)Session["FaultyId"];
            Faulty faulty = faultyRepository.FindById(faultyId);
            IEnumerable<ClassRoomFaulty> roomFaulties = roomFaultyrepository.Get(x => x.FaultyId == faultyId);
            foreach (var item in roomFaulties)
            {
                item.Faulty = faulty;
            }
            if (!string.IsNullOrEmpty(Key))
            {
                if (Key.Equals("Teaching"))
                {
                    roomFaulties = roomFaulties.Where(x => x.Status);
                }
                else if (Key.Equals("Finished"))
                {
                    roomFaulties = roomFaulties.Where(x => x.Status == false);
                }
                else
                {
                    roomFaulties = roomFaulties.Where(x => x.ClassRoom.Name.Contains(Key));
                }
            }
            Pagination pagination = paginationService.getInfoPaginate(roomFaulties.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(roomFaulties.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            var Salary = faulty.Salary;
            return Json(new
            {
                paginate = pagination,
                data = data,
                Salary = Salary,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
