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
    public class TagsController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<Blog> blogRepository;
        private IRepository<Category> categoryRepository;
        private IRepository<Tag> tagRepository;
        private IRepository<TagBlog> tagBlogRepository;
        private IPaginationService paginationService;
        public TagsController()
        {
            ctx = new EducationManageDbContext();
            blogRepository = new DbRepository<Blog>();
            categoryRepository = new DbRepository<Category>();
            tagRepository = new DbRepository<Tag>();
            tagBlogRepository = new DbRepository<TagBlog>();
            paginationService = new DbPaginationService();
        }
        // GET: Tags
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetData(int Id, int CurrentPage, int Limit, string Key)
        {
            List<Blog> blog = new List<Blog>();
            
            if (!String.IsNullOrEmpty(Key))
            {
                blog = blogRepository.Get().Where(x => x.Name.ToLower().Contains(Key.ToLower()) ||x.Detail.ToLower().Contains(Key.ToLower())).ToList();
            }
            else
            {
                List<TagBlog> tagBlogList = tagBlogRepository.Get(x => x.TagId == Id).ToList();
                blog = new List<Blog>();
                foreach (var item in tagBlogList)
                {
                    blog.Add(blogRepository.FindById(item.BlogId));
                }
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
            List<TagBlog> tagBlogList = tagBlogRepository.Get(x => x.TagId == id).ToList();
            List<Blog> blogs = new List<Blog>();
            foreach (var item in tagBlogList)
            {
                blogs.Add(blogRepository.FindById(item.BlogId));
            }

            if (blogs == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var blogRecent = blogRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(3);
            ViewBag.BlogRecent = blogRecent;
            ViewBag.Categories = categoryRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(20);
            ViewBag.Tags = tagRepository.Get().OrderByDescending(x => x.Id).Take(20);
            ViewBag.TagName = tagRepository.FindById(id).Content;
            ViewBag.Id = id;
            return View(blogs);
        }
    }
}