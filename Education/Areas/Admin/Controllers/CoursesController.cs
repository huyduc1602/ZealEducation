using System;
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
using Education.Areas.Admin.Data.DataModel;
using Education.BLL;
using Education.DAL;
using Newtonsoft.Json;

namespace Education.Areas.Admin.Controllers
{
    public class CoursesController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<Course> courseRepository;
        private IPaginationService paginationService;
        public CoursesController()
        {
            ctx = new EducationManageDbContext();
            courseRepository = new DbRepository<Course>();
            paginationService = new DbPaginationService();
        }

        // GET: Admin/Courses
        public ActionResult Index()
        {
            var courses = courseRepository.Get();
            return View(courses);
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key)
        {
            var course = courseRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                course = course.Where(x => x.Name.Contains(Key)
                                            || x.Code.Contains(Key)
                                            || x.Detail.Contains(Key));
            }
            Pagination pagination = paginationService.getInfoPaginate(course.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(course.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }
        private static Random random = new Random();
        public static string GenerateCode(int length)
        {
            const string chars = "0123456789";
            return "CO" + new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        // GET: Admin/Courses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = courseRepository.FindById(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // GET: Admin/Courses/Create
        public ActionResult Create()
        {
            string courseCode = GenerateCode(7);
            if (courseRepository.CheckDuplicate(x => x.Code.Equals(courseCode)))
            {
                courseCode = GenerateCode(7);
            }
            ViewBag.CourseCode = courseCode;
            return View();
        }

        // POST: Admin/Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(CourseModel course)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Dashboard");
            }

            else
            {
                course.UserId = ((User)Session["user"]).Id;
            }
            if (courseRepository.Get(x => x.Code.Equals(course.Code)).Count() > 0)
            {
                course.Code = GenerateCode(7);
                ModelState.AddModelError("Code", "Course code available");
            }
            else if (courseRepository.Get(x => x.Slug.Equals(course.Slug)).Count() > 0)
            {
                ModelState.AddModelError("Slug", "Course slug available");
            }
            else if (course.StudyTime < 1 && course.StudyTime > 36)
            {
                ModelState.AddModelError("StudyTime", "Study time must be from 1 to 36");
            }
            else if (course.Price < 1)
            {
                ModelState.AddModelError("Price", "Course price must be greater than 1");
            }
            else if (course.SalePrice < 0)
            {
                ModelState.AddModelError("SalePrice", "Course sale price must be greater than or equal to 0");
            }
            else if (course.MaximumCandicate < 1)
            {
                ModelState.AddModelError("MaximumCandicate", "The maximum number of candicates must be greater than or equal to 1");
            }
            else if (course.Image == null)
            {
                ModelState.AddModelError("Image", "Course images are not empty");
            }
            if (ModelState.IsValid)
            {
                if (course.Image != null && course.Image.ContentLength > 0)
                {
                    string fileName = "Course " + course.Code + DateTime.Today.ToString("ddmmyyyy") + System.IO.Path.GetExtension(course.Image.FileName);
                    string image = "/Areas/Admin/Content/assets/img/course/" + fileName;
                    Course courseAdd = new Course
                    {
                        Code = course.Code,
                        Name = course.Name,
                        Slug = course.Slug,
                        StudyTime = course.StudyTime,
                        Price = course.Price,
                        SalePrice = course.SalePrice,
                        MaximumCandicate = course.MaximumCandicate,
                        Detail = course.Detail,
                        Image = image,
                        CreatedAt = DateTime.Today,
                        UpdatedAt = DateTime.Today,
                        UserId = course.UserId,
                    };
                    courseRepository.Add(courseAdd);
                    string path = Path.Combine(Server.MapPath("~/Areas/Admin/Content/assets/img/course"), fileName);
                    course.Image.SaveAs(path);
                    return RedirectToAction("Index");
                }
            }

            ViewBag.CourseCode = course.Code;
            return View(course);
        }

        // GET: Admin/Courses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = courseRepository.FindById(id);
            CourseModel courseModel = new CourseModel()
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Slug = course.Slug,
                StudyTime = course.StudyTime,
                MaximumCandicate = course.MaximumCandicate,
                Price = course.Price,
                SalePrice = course.SalePrice,
                Detail = course.Detail,
                UserId = course.Id,
            };
            if (course == null)
            {
                return HttpNotFound();
            }
            ViewBag.Image = course.Image;
            return View(courseModel);
        }

        // POST: Admin/Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(CourseModel course)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Dashboard");
            }
            else
            {
                course.UserId = ((User)Session["user"]).Id;

            }

            if (courseRepository.Get(x => x.Slug.Equals(course.Slug)).Count() > 1)
            {
                ModelState.AddModelError("Slug", "Course slug available");
            }
            else if (course.StudyTime < 1 && course.StudyTime > 36)
            {
                ModelState.AddModelError("StudyTime", "Study time must be from 1 to 36");
            }
            else if (course.Price < 1)
            {
                ModelState.AddModelError("Price", "Course price must be greater than or equal to 1");
            }
            else if (course.SalePrice < 0)
            {
                ModelState.AddModelError("SalePrice", "Course sale price must be greater than or equal to 0");
            }
            else if (course.MaximumCandicate < 1)
            {
                ModelState.AddModelError("MaximumCandicate", "The maximum number of candicates must be greater than or equal to 1");
            }
            Course courseEdit = courseRepository.FindById(course.Id);
            string fileName = "";
            if (course.Image != null)
            {
                fileName = "Course " + course.Code + DateTime.Today.ToString("ddmmyyyy") + System.IO.Path.GetExtension(course.Image.FileName);
                string image = "/Areas/Admin/Content/assets/img/course/" + fileName;
                courseEdit.Image = image;
            }

            if (ModelState.IsValid)
            {

                courseEdit.Name = course.Name;
                courseEdit.Slug = course.Slug;
                courseEdit.StudyTime = course.StudyTime;
                courseEdit.Price = course.Price;
                courseEdit.SalePrice = course.SalePrice;
                courseEdit.MaximumCandicate = course.MaximumCandicate;
                courseEdit.UpdatedAt = DateTime.Today;
                if (courseRepository.Edit(courseEdit))
                {
                    if (course.Image != null)
                    {
                        string path = Path.Combine(Server.MapPath("~/Areas/Admin/Content/assets/img/course"), fileName);
                        course.Image.SaveAs(path);
                    }
                    return RedirectToAction("Index");
                }

            }
            return View(course);
        }

        // GET: Admin/Courses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete course failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            Course course = courseRepository.FindById(id);
            if (course == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete course failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            if (courseRepository.Remove(course))
            {
                return Json(new
                {
                    StatusCode = 200,
                    Message = "Delete the course successfully!",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                StatusCode = 500,
                message = "Delete course failed!",
            }, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ctx.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
