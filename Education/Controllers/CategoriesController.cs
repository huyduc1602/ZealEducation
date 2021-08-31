using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Education.BLL;
using Education.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Education.Controllers
{
    public class CategoriesController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<Blog> blogRepository;
        private IRepository<Category> categoryRepository;
        private IRepository<Tag> tagRepository;
        private IPaginationService paginationService;
        public CategoriesController()
        {
            ctx = new EducationManageDbContext();
            blogRepository = new DbRepository<Blog>();
            categoryRepository = new DbRepository<Category>();
            tagRepository = new DbRepository<Tag>();
            paginationService = new DbPaginationService();
        }
        // GET: Categories
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetData(int Id,int CurrentPage, int Limit, string Key)
        {
            var blog = blogRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                blog = blog.Where(x => x.CategoryId == Id &&( x.Name.ToLower().Contains(Key.ToLower()) ||
                                    x.Detail.ToLower().Contains(Key.ToLower())));
            }
            Pagination pagination = paginationService.getInfoPaginate(blog.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(blog.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            IEnumerable<Blog> blogs = blogRepository.Get(x => x.CategoryId == id);

            if (blogs == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var blogRecent = blogRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(3);
            ViewBag.BlogRecent = blogRecent;
            ViewBag.Categories = categoryRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(20);
            ViewBag.Tags = tagRepository.Get().OrderByDescending(x => x.Id).Take(20);
            ViewBag.CategoryName = categoryRepository.FindById(id).Name;
            ViewBag.Id = id;
            return View(blogs);
        }
    }
}