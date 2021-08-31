using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Education.Areas.Admin.Data.DataModel;
using Education.BLL;
using Education.BLL.Repositories.DbHandler;
using Education.DAL;
using Newtonsoft.Json;

namespace Education.Areas.Admin.Controllers
{
    [CustomizeAuthorize]
    public class BlogsController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<Blog> blogRepository;
        private IRepository<Category> categoryRepository;
        private IRepository<Tag> tagRepository;
        private IRepository<TagBlog> tagBlogRepository;
        private TagRepository tagsRepository;
        private IPaginationService paginationService;
        public BlogsController()
        {
            ctx = new EducationManageDbContext();
            blogRepository = new DbRepository<Blog>();
            categoryRepository = new DbRepository<Category>();
            tagRepository = new DbRepository<Tag>();
            tagBlogRepository = new DbRepository<TagBlog>();
            tagsRepository = new TagRepository();
            paginationService = new DbPaginationService();
        }


        // GET: Admin/Blogs
        public ActionResult Index()
        {
            var blogs = blogRepository.Get();
            return View(blogs);
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key)
        {
            var blog = blogRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                blog = blog.Where(x => x.Name.Contains(Key)
                                            || x.Sumary.Contains(Key)
                                            || x.Detail.Contains(Key));
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
        // GET: Admin/Blogs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = blogRepository.FindById(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }
        public string GenerateSumary(string detail, int length)
        {
            if (detail.Length > length)
            {
                return detail.Substring(0, length) + "...";
            }
            return detail;
        }
        public static string HTMLToText(string HTMLCode)
        {
            // Remove new lines since they are not visible in HTML
            HTMLCode = HTMLCode.Replace("\n", " ");

            // Remove tab spaces
            HTMLCode = HTMLCode.Replace("\t", " ");

            // Remove multiple white spaces from HTML
            HTMLCode = Regex.Replace(HTMLCode, "\\s+", " ");

            // Remove HEAD tag
            HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", ""
                                , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any JavaScript
            HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", ""
              , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Replace special characters like &, <, >, " etc.
            StringBuilder sbHTML = new StringBuilder(HTMLCode);
            // Note: There are many more special characters, these are just
            // most common. You can add new characters in this arrays if needed
            string[] OldWords = {"&nbsp;", "&amp;", "&quot;", "&lt;",
   "&gt;", "&reg;", "&copy;", "&bull;", "&trade;","&#39;"};
            string[] NewWords = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢", "\'" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }

            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML.Replace("<br>", "\n<br>");
            sbHTML.Replace("<br ", "\n<br ");
            sbHTML.Replace("<p ", "\n<p ");

            // Finally, remove all HTML tags and return plain text
            return System.Text.RegularExpressions.Regex.Replace(
              sbHTML.ToString(), "<[^>]*>", "");
        }
        [NonAction]
        //private void AddTag(Blog blog,string tagStr)
        //{
        //    //Add list Tag
        //    List<Tag> Tags = new List<Tag>();
        //    string[] TagContent = tagStr.Split(',');
        //    foreach (var item in TagContent)
        //    {
        //        //Check tag exits
        //        Tag tagExits = null;
        //        var ListTag = tagRepository.Get(x => x.Content.Equals(item));
        //        if (ListTag.Count()>0)
        //        {
        //            tagExits = ListTag.First();
        //            tagExits.Blogs.Add(blog);
        //            //tagRepository.Edit(tagExits);
        //        }
        //        else
        //        {
        //            tagExits = new Tag();
        //            tagExits.Content = item;
        //            tagExits.Blogs = new List<Blog>();
        //            tagExits.Blogs.Add(blog);
        //        }
        //        Tags.Add(tagExits);
        //        blog.Tags = Tags;
        //    }
        //}
        //[NonAction]
        //private void DeleteRelationship(int blogID, long tagID)
        //{
        //    // return one instance each entity by primary key
        //    var blog = blogRepository.FindById(blogID);
        //    var tag = tagRepository.FindById(blogID);

        //    // call Remove method from navigation property for any instance
        //    // tag.Blogs.Remove(blog);
        //    // also works
        //    blog.Tags.Remove(tag);
        //    // call SaveChanges from context
        //    ctx.SaveChanges();
        //}
        // GET: Admin/Blogs/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name");
            return View();
        }
        public ActionResult Add()
        {
            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name");
            return View();
        }

        // POST: Admin/Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(BlogModel blog)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Dashboard");
            }
            else
            {
                blog.UserId = ((User)Session["user"]).Id;
            }
            if (blogRepository.Get(x => x.Slug.Equals(blog.Slug)).Count() > 0)
            {
                ModelState.AddModelError("Slug", "Blog slug available");
            }
            else if (blogRepository.Get(x => x.Name.Equals(blog.Name)).Count() > 0)
            {
                ModelState.AddModelError("Name", "Blog name available");
            }
            else if (blog.Image == null)
            {
                ModelState.AddModelError("Image", "Blog images are not empty");
            }

            if (ModelState.IsValid)
            {
                if (blog.Image != null && blog.Image.ContentLength > 0)
                {
                    string fileName = "Blog" + blog.Slug + DateTime.Today.ToString("ddmmyyyy") + System.IO.Path.GetExtension(blog.Image.FileName);
                    string image = "/Areas/Admin/Content/assets/img/blog/" + fileName;
                    if (blog.Sumary == null)
                    {
                        blog.Sumary = GenerateSumary(HTMLToText(blog.Detail), 100);
                    }

                    Blog blogAdd = new Blog
                    {
                        Name = blog.Name,
                        Slug = blog.Slug,
                        Detail = blog.Detail,
                        Sumary = blog.Sumary,
                        CategoryId = blog.CategoryId,
                        Image = image,
                        CreatedAt = DateTime.Today,
                        UpdatedAt = DateTime.Today,
                        UserId = blog.UserId,
                    };
                    //add tag
                    var tagStr = blog.Tag;
                    var tagList = tagsRepository.AddTags(tagStr);
                    //blogAdd.Tags = tagList;
                    if (blogRepository.Add(blogAdd))
                    {
                        Blog blogEdit = blogRepository.Get(x => x.Slug == blogAdd.Slug).FirstOrDefault();
                        foreach (var item in tagList)
                        {
                            var tagId = tagsRepository.Get(x => x.Content.Equals(item.Content)).FirstOrDefault().Id;
                            TagBlog tagBlog = new TagBlog
                            {
                                BlogId = blogEdit.Id,
                                TagId = tagId,
                            };
                            tagBlogRepository.Add(tagBlog);
                        }


                        string path = Path.Combine(Server.MapPath("~/Areas/Admin/Content/assets/img/blog"), fileName);
                        blog.Image.SaveAs(path);
                        return RedirectToAction("Index");
                    }

                }
            }

            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name", blog.CategoryId);
            return View(blog);
        }

        // GET: Admin/Blogs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = blogRepository.FindById(id);
            BlogModel blogModel = new BlogModel()
            {
                Name = blog.Name,
                Slug = blog.Slug,
                Detail = blog.Detail,
                Sumary = blog.Sumary,
                CategoryId = blog.CategoryId,
                UserId = blog.Id,
            };
            if (blog == null)
            {
                return HttpNotFound();
            }
            if (blog.TagBlogs.Count() > 0)
            {
                string strTag = "";
                HashSet<TagBlog> tagBlogList = blog.TagBlogs.ToHashSet();
                List<Tag> tagList = new List<Tag>();
                foreach (var item in tagBlogList)
                {
                    tagList.Add(tagRepository.FindById(item.TagId));
                }
                List<string> tagstrings = new List<string>();
                foreach (var item in tagList)
                {
                    tagstrings.Add(item.Content);
                }
                strTag = String.Join(",", tagstrings);
                blogModel.Tag = strTag;
            }


            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name", blog.CategoryId);
            ViewBag.Image = blog.Image;
            return View(blogModel);
        }

        // POST: Admin/Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(BlogModel blog)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Dashboard");
            }
            else
            {
                blog.UserId = ((User)Session["user"]).Id;
            }
            if (blogRepository.Get(x => x.Slug.Equals(blog.Slug)).Count() > 1)
            {
                ModelState.AddModelError("Slug", "Blog slug available");
            }
            else if (blogRepository.Get(x => x.Name.Equals(blog.Name)).Count() > 1)
            {
                ModelState.AddModelError("Name", "Blog name available");
            }
            Blog blogEdit = blogRepository.FindById(blog.Id);
            string fileName = "";
            if (blog.Image != null)
            {
                fileName = "Blog" + blog.Slug + DateTime.Today.ToString("ddmmyyyy") + System.IO.Path.GetExtension(blog.Image.FileName);
                string image = "/Areas/Admin/Content/assets/img/blog/" + fileName;
                blogEdit.Image = image;
            }

            if (ModelState.IsValid)
            {
                if (blog.Sumary == null)
                {
                    blog.Sumary = GenerateSumary(HTMLToText(blog.Detail), 100);
                }

                blogEdit.Name = blog.Name;
                blogEdit.Slug = blog.Slug;
                blogEdit.Detail = blog.Detail;
                blogEdit.Sumary = blog.Sumary;
                blogEdit.CategoryId = blog.CategoryId;
                blogEdit.UpdatedAt = DateTime.Today;
                //Remove old tag
                foreach (var item in tagBlogRepository.Get(x => x.BlogId == blogEdit.Id))
                {
                    tagBlogRepository.Remove(item);
                }
                //Add list Tag
                var tagStr = blog.Tag;
                List<Tag> tagList = tagsRepository.AddTags(tagStr);

                if (blogRepository.Edit(blogEdit))
                {
                    foreach (var item in tagList)
                    {
                        var tagId = tagsRepository.Get(x => x.Content.Equals(item.Content)).FirstOrDefault().Id;
                        TagBlog tagBlog = new TagBlog
                        {
                            BlogId = blogEdit.Id,
                            TagId = tagId,
                        };
                        tagBlogRepository.Add(tagBlog);
                    }

                    if (blog.Image != null)
                    {
                        string path = Path.Combine(Server.MapPath("~/Areas/Admin/Content/assets/img/blog"), fileName);
                        blog.Image.SaveAs(path);
                    }
                    return RedirectToAction("Index");
                }

            }
            ViewBag.CategoryId = new SelectList(categoryRepository.Get(), "Id", "Name", blog.CategoryId);
            return View(blog);
        }

        // GET: Admin/Blogs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    message = "Delete fail blog!",
                }, JsonRequestBehavior.AllowGet);
            }
            Blog blog = blogRepository.FindById(id);
            if (blog == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete fail blog!",
                }, JsonRequestBehavior.AllowGet);
            }
            if (blogRepository.Remove(blog))
            {
                return Json(new
                {
                    StatusCode = 200,
                    Message = "Delete successful course!",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                StatusCode = 500,
                message = "Delete successful course!",
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
