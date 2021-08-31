using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
    [CustomizeAuthorize]
    public class CategoriesController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<Category> categoryRepository;
        private IRepository<Blog> blogRepository;
        private IPaginationService paginationService;
        public CategoriesController()
        {
            ctx = new EducationManageDbContext();
            categoryRepository = new DbRepository<Category>();
            blogRepository = new DbRepository<Blog>();
            paginationService = new DbPaginationService();
        }

        // GET: Admin/Categories
        public ActionResult Index()
        {
            var categories = categoryRepository.Get();
            return View(categories);
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key)
        {
            var course = categoryRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                course = course.Where(x => x.Name.Contains(Key));
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
        // GET: Admin/Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = categoryRepository.FindById(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name");
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CategoryModel category)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Dashboard");
            }
            else
            {
                category.UserId = ((User)Session["user"]).Id;
            }
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(category.ParentId.ToString()))
                {
                    category.ParentId = 0;
                }
                Category categoryAdd = new Category()
                {
                    Name = category.Name,
                    ParentId = category.ParentId,
                    CreatedAt = DateTime.Today,
                    UpdatedAt = DateTime.Today
                };
                if (categoryRepository.Add(categoryAdd))
                {
                    return RedirectToAction("Index");
                }

            }
            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name", category.ParentId);
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = categoryRepository.FindById(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name", category.ParentId);
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                Category categoryEdit = categoryRepository.FindById(category.Id);
                if (String.IsNullOrEmpty(category.ParentId.ToString()))
                {
                    category.ParentId = 0;
                }
                categoryEdit.Name = category.Name;
                categoryEdit.ParentId = category.ParentId;
                categoryEdit.UpdatedAt = DateTime.Now;
                if (categoryRepository.Edit(categoryEdit))
                {
                    return RedirectToAction("Index");
                }
            }
            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name", category.ParentId);
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete category failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            Category category = categoryRepository.FindById(id);
            if (category == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete category failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in blogRepository.Get())
            {
                if (item.CategoryId == id)
                {
                    return Json(new
                    {
                        StatusCode = 400,
                        Message = "Can't delete because the category has posts",
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            if (categoryRepository.Remove(category))
            {
                return Json(new
                {
                    StatusCode = 200,
                    Message = "Delete category failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                StatusCode = 500,
                message = "Delete category failed!",
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
