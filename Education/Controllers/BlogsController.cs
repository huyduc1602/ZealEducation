using Education.BLL;
using Education.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Education.Controllers
{
    public class BlogsController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<Blog> blogRepository;
        private IRepository<Category> categoryRepository;
        private IRepository<Tag> tagRepository;
        public BlogsController()
        {
            ctx = new EducationManageDbContext();
            blogRepository = new DbRepository<Blog>();
            categoryRepository = new DbRepository<Category>();
            tagRepository = new DbRepository<Tag>();
        }
        // GET: Blogs
        public ActionResult Index()
        {
            return RedirectToAction("Courses", "Home");
        }
        public ActionResult Details(string slug)
        {
            if (slug == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            Blog blog = blogRepository.Get(x => x.Slug.EndsWith(slug)).First();

            if (blog == null)
            {
                return RedirectToAction("NotFound", "Home");
            }
            var blogRecent = blogRepository.Get(x => x.Id != blog.Id).OrderByDescending(x => x.UpdatedAt).Take(3);
            ViewBag.BlogRecent = blogRecent;
            var blogRelated = blogRepository.Get(x => x.CategoryId != blog.CategoryId && x.Id != blog.Id).OrderByDescending(x => x.UpdatedAt).Take(10);
            ViewBag.BlogRelated = blogRelated;
            ViewBag.Categories = categoryRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(20);
            ViewBag.Tags = tagRepository.Get().OrderByDescending(x => x.Id).Take(20);
            return View(blog);
        }
        public ActionResult ReleatedBlog(int? id)
        {
            return View();
        }
    }
}