using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Education.BLL;
using Education.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Education.Controllers
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
        // GET: Courses
        public ActionResult Index()
        {
            return RedirectToAction("Courses", "Home");
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key)
        {
            var course = courseRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                course = course.Where(x => x.Name.Contains("/" + Key + "/"));
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
        public ActionResult Details(string slug)
        {
            if (slug == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            Course course = courseRepository.Get(x => x.Slug.EndsWith(slug)).First();

            if (course == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            if (course.SalePrice == 0)
            {
                ViewBag.Discount = 100;
            }
            else
            {
                var discount = (course.Price - course.SalePrice) / course.Price;
                ViewBag.Discount = Math.Round(discount * 100);
            }
            var coureRelateds = courseRepository.Get(x => x.Id != course.Id).OrderByDescending(x => x.UpdatedAt);
            var relatedSidebar = courseRepository.Get(x => x.Id != course.Id).OrderByDescending(x => x.UpdatedAt).Take(3);
            ViewBag.CoureReleateds = coureRelateds;
            ViewBag.RelatedSidebar = relatedSidebar;
            return View(course);
        }
    }
}