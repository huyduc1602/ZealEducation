using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
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
    public class UsersController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<User> userRepository;
        private IRepository<GroupUser> groupUserRepository;
        private IPaginationService paginationService;
        public UsersController()
        {
            ctx = new EducationManageDbContext();
            userRepository = new DbRepository<User>();
            groupUserRepository = new DbRepository<GroupUser>();
            paginationService = new DbPaginationService();
        }
        // GET: Admin/Users
        public ActionResult Index()
        {
            var users = userRepository.Get();
            return View(users);
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key)
        {
            var user = userRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                user = user.Where(x => x.UserName.Contains(Key)
                                            || x.FullName.Contains(Key));
            }
            Pagination pagination = paginationService.getInfoPaginate(user.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(user.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = userRepository.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            ViewBag.GroupUserId = new SelectList(groupUserRepository.Get(), "Id", "Name");
            return View();
        }

        // POST: Admin/Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserModel user)
        {
            if (userRepository.Get(x => x.UserName.Equals(user.UserName)).Count() > 0)
            {
                ModelState.AddModelError("UserName", "User Name available");
            }
            if (ModelState.IsValid)
            {
                user.Password = GetMD5(user.Password);
                User userAdd = new User
                {
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Password = user.Password,
                    GroupUserId = user.GroupUserId,
                    CreatedAt = DateTime.Today,
                    UpdatedAt = DateTime.Today
                };
                if (userRepository.Add(userAdd))
                {
                    return RedirectToAction("Index");
                }
            }

            ViewBag.GroupUserId = new SelectList(groupUserRepository.Get(), "Id", "Name", user.GroupUserId);
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = userRepository.FindById(id);
            UserModel userModel = new UserModel()
            {
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                GroupUserId = user.GroupUserId,
                Password = user.Password
            };
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.GroupUserId = new SelectList(groupUserRepository.Get(), "Id", "Name", user.GroupUserId);
            return View(userModel);
        }

        // POST: Admin/Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserModel user)
        {
            if (ModelState.IsValid)
            {

                User userEdit = userRepository.FindById(user.Id);
                userEdit.UserName = user.UserName;
                userEdit.FullName = user.FullName;
                userEdit.Email = user.Email;
                if (userRepository.Edit(userEdit))
                {
                    return RedirectToAction("Index");
                }

            }
            ViewBag.GroupUserId = new SelectList(groupUserRepository.Get(), "Id", "Name", user.GroupUserId);
            return View(user);
        }
        public ActionResult ChangePassword(int? UserId)
        {
            ViewBag.UserId = UserId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(PasswordModel passwordModel)
        {
            if (ModelState.IsValid)
            {
                User u = userRepository.FindById(passwordModel.UserId);
                u.Password = GetMD5(passwordModel.Password);
                if (userRepository.Edit(u))
                {
                    return RedirectToAction("Logout", "Dashboard");
                }
            }
            return View(passwordModel);
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
        {
            //Verify Email ID
            //Generate Reset password link 
            //Send Email 
            string message = "";
            var account = userRepository.Get(a => a.Email == EmailID).FirstOrDefault();
            if (account != null)
            {
                //Send email for reset password
                string resetCode = Guid.NewGuid().ToString();
                SendVerificationLinkEmail(account.Email, resetCode, "ResetPassword");
                account.ResetPasswordCode = resetCode;
                //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 
                //in our model class in part 1
                userRepository.Edit(account);
                message = "Reset password link has been sent to your email id.";
            }
            else
            {
                message = "Account not found";
            }

            ViewBag.Message = message;
            return View();
        }
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/Admin/Users/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("no.reply.email.1602@gmail.com ", "Zeal Education Admin");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "xuoabdaptnuvkgcy"; // Replace with actual password

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account is successfully created!";
                body = "<br/>We are excited to tell you that your Dotnet Awesome account is" +
                    " successfully created. Please click on the below link to verify your account" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi,<br/>We got request for reset your account password. Please click on the below link to reset your password" +
                    "<br/><br/><a href=" + link + ">Reset Password link</a>";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }
            var user = userRepository.Get(a => a.ResetPasswordCode == id).FirstOrDefault();
            if (user != null)
            {
                ResetPasswordModel model = new ResetPasswordModel();
                model.ResetCode = id;
                return View(model);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                var user = userRepository.Get(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                if (user != null)
                {
                    user.Password = GetMD5(model.NewPassword);
                    user.ResetPasswordCode = "";
                    if (userRepository.Edit(user))
                    {
                        message = "New password updated successfully";
                        return RedirectToAction("Login", "Dashboard");
                    }
                }

            }
            else
            {
                message = "Something invalid";
            }
            ViewBag.Message = message;
            return View(model);
        }
        // GET: Admin/Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete user failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            User user = userRepository.FindById(id);
            if (user == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete user failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            if (userRepository.Remove(user))
            {
                return Json(new
                {
                    StatusCode = 200,
                    Message = "Delete user successfully!",
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
        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }
    }
}
