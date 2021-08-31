using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Education.BLL;
using Education.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Education.Controllers
{
    public class HomeController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<Course> courseRepository;
        private IRepository<Blog> blogRepository;
        private IRepository<Category> categoryRepository;
        private IRepository<Tag> tagRepository;
        private IPaginationService paginationService;
        public HomeController()
        {
            ctx = new EducationManageDbContext();
            courseRepository = new DbRepository<Course>();
            categoryRepository = new DbRepository<Category>();
            tagRepository = new DbRepository<Tag>();
            paginationService = new DbPaginationService();
            blogRepository = new DbRepository<Blog>();
        }
        public ActionResult Index()
        {
            var courses = courseRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(6).ToList();
            ViewBag.Courses = courses;
            var blogs = blogRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(6).ToList();
            ViewBag.Blogs = blogs;
            return View();
        }
        public ActionResult GetDataCourse(int CurrentPage, int Limit, string Key)
        {
            var course = courseRepository.Get();
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
        public ActionResult GetDataBlog(int CurrentPage, int Limit, string Key)
        {
            var blog = blogRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                blog = blog.Where(x => x.Name.ToLower().Contains(Key.ToLower()) ||
                                    x.Detail.ToLower().Contains(Key.ToLower()));
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
        public ActionResult Courses()
        {
            var courses = courseRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(9);
            ViewBag.TotalCourse = courses.Count();
            return View(courses);
        }
        public ActionResult Blogs()
        {
            var blogs = blogRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(9);
            var blogRecent = blogRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(3);
            ViewBag.BlogRecent = blogRecent;
            ViewBag.TotalBlog = blogs.Count();
            ViewBag.Categories = categoryRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(20);
            ViewBag.Tags = tagRepository.Get().OrderByDescending(x => x.Id).Take(20);
            return View(blogs);
        }
        public ActionResult FAQS()
        {
            return View();
        }
        public ActionResult Search(string Key)
        {
            var courses = courseRepository.Get().OrderByDescending(x => x.UpdatedAt).Take(9);
            ViewBag.TotalCourse = courses.Count();
            ViewBag.Key = Key;
            return View(courses);
        }
        public ActionResult Instructor()
        {
            return View();
        }

        public ActionResult About()
        {

            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Contact(Contact contact)
        {
            if (!contact.EAgree)
            {
                ModelState.AddModelError("EAgree", "You have not accepted the terms and conditions");
            }
            if (ModelState.IsValid)
            {
                string mailAdmin = "zealeducationaptech@gmail.com ";
                SendContactEmail(mailAdmin, contact.Name, contact.Email, contact.Subject, contact.Message);
                ViewBag.Message = "Send contact information successfully";
                return View();
            }
            return View(contact);
        }
        public ActionResult SignIn()
        {

            return View();
        }
        public ActionResult SignUp()
        {

            return View();
        }
        public ActionResult NotFound()
        {
            ViewBag.Message = "404 Not Found page.";

            return View();
        }
        public bool IsValidEmail(string source)
        {
            return new EmailAddressAttribute().IsValid(source);
        }
        [HttpPost]
        public ActionResult Subcribe(string email)
        {
            if (IsValidEmail(email))
            {
                string mailAdmin = "zealeducationaptech@gmail.com ";
                SendSubscribeEmail(mailAdmin, email);
                return Json(new
                {
                    StatusCode = 200,
                    Message = "Subcribe successfully!",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                StatusCode = 500,
                Message = "Subcribe failed!",
            }, JsonRequestBehavior.AllowGet);
        }
        public void SendContactEmail(string mailAdmin, string name, string email, string subject, string message)
        {

            var fromEmail = new MailAddress("no.reply.email.1602@gmail.com ", name);
            var toEmail = new MailAddress(mailAdmin);
            var fromEmailPassword = "xuoabdaptnuvkgcy"; // Replace with actual password

            string body = "";
            body = "<br/><h1>Contact Info</h1>" +
                   "<h5>Name:<h5>" + name +
                   "<h5>Email:<h5>  " + email +
                   "<h5>Message:<h5> " + message;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var messageContent = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(messageContent);
        }
        public void SendSubscribeEmail(string mailAdmin, string email)
        {

            var fromEmail = new MailAddress("no.reply.email.1602@gmail.com ", "Zeal Education");
            var toEmail = new MailAddress(mailAdmin);
            var fromEmailPassword = "xuoabdaptnuvkgcy"; // Replace with actual password

            string subject = "Subscribe";
            string body = "";
            body = "<h4>Hi,Admin</h4>" + "<h4>There is a new subscription</h4>" + "<h4>Email:  " + email + "<h4>";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var messageContent = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(messageContent);
        }
    }
}