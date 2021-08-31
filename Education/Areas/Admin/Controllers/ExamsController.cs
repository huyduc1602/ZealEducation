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
    public class ExamsController : Controller
    {
        private EducationManageDbContext ctx;
        private IRepository<Exam> examRepository;
        private IRepository<Candicate> candicateRepository;
        private IRepository<ClassRoom> batchRepository;
        private IPaginationService paginationService;
        public ExamsController()
        {
            ctx = new EducationManageDbContext();
            examRepository = new DbRepository<Exam>();
            candicateRepository = new DbRepository<Candicate>();
            batchRepository = new DbRepository<ClassRoom>();
            paginationService = new DbPaginationService();
        }

        // GET: Admin/Exams
        public ActionResult Index()
        {
            var exams = examRepository.Get();
            return View(exams);
        }
        // GET: Admin/Exams/GetData
        public ActionResult GetData(int CurrentPage, int Limit, string Key)
        {
            var exam = examRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                exam = exam.Where(x => x.Name.Contains(Key)
                                            || x.Note.Contains(Key));
            }
            Pagination pagination = paginationService.getInfoPaginate(exam.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(exam.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }
        [NonAction]
        // GET: Admin/Exams/GetDataCandicate
        public ActionResult GetDataCandicate(int CurrentPage, int Limit, string Key, int BatchId)
        {
            var candicates = candicateRepository.Get();

            if (!String.IsNullOrEmpty(Key))
            {
                candicates = candicates.Where(x => x.Name.Contains(Key)
                                            || x.Code.Contains(Key));
            }
            //if (!String.IsNullOrEmpty(batchId.ToString()))
            //{
            //    candicates = candicates.Where(x => x.BatchId.Equals(batchId));
            //}

            Pagination pagination = paginationService.getInfoPaginate(candicates.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(candicates.Skip((CurrentPage - 1) * Limit).Take(Limit));
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
            return "EX" + new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        // GET: Admin/Exams/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exam exam = examRepository.FindById(id);
            if (exam == null)
            {
                return HttpNotFound();
            }
            return View(exam);
        }

        // GET: Admin/Exams/Create
        public ActionResult Create()
        {
            string courseCode = GenerateCode(7);
            if (examRepository.CheckDuplicate(x => x.Code.Equals(courseCode)))
            {
                courseCode = GenerateCode(7);
            }
            ViewBag.CourseCode = courseCode;
            return View();
        }

        // POST: Admin/Exams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExamModel exam)
        {
            DateTime now = DateTime.Today;
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Dashboard");
            }
            else
            {
                exam.UserId = ((User)Session["user"]).Id;
            }
            if (examRepository.Get(x => x.Code.Equals(exam.Code)).Count() > 0)
            {
                exam.Code = GenerateCode(7);
                ModelState.AddModelError("Code", "Exam code available");
            }
            else if (exam.EventDate < now)
            {
                ModelState.AddModelError("EventDate", "Exam date must be the future day");
            }
            if (ModelState.IsValid)
            {
                Exam examAdd = new Exam
                {
                    Code = exam.Code,
                    Name = exam.Name,
                    Note = exam.Note,
                    EventDate = exam.EventDate,
                    StartTime = exam.StartTime,
                    EndTime = exam.EndTime,
                    CreatedAt = DateTime.Today,
                    UpdatedAt = DateTime.Today,
                    UserId = exam.UserId,
                };
                examRepository.Add(examAdd);
                return RedirectToAction("Index");
            }
            ViewBag.CourseCode = exam.Code;
            return View(exam);
        }

        // GET: Admin/Exams/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Exam exam = examRepository.FindById(id);
            ExamModel examModel = new ExamModel()
            {
                Code = exam.Code,
                Name = exam.Name,
                Note = exam.Note,
                EventDate = exam.EventDate,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                UserId = exam.Id
            };
            if (exam == null)
            {
                return HttpNotFound();
            }
            return View(examModel);
        }

        // POST: Admin/Exams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExamModel exam)
        {
            DateTime now = DateTime.Today;
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Dashboard");
            }
            else
            {
                exam.UserId = ((User)Session["user"]).Id;
            }
            if (examRepository.Get(x => x.Code.Equals(exam.Code)).Count() > 1)
            {
                exam.Code = GenerateCode(7);
                ModelState.AddModelError("Code", "Exam code available");
            }
            else if (exam.EventDate < now)
            {
                ModelState.AddModelError("EventDate", "Exam date must be the future day");
            }
            if (ModelState.IsValid)
            {
                Exam examEdit = examRepository.FindById(exam.Id);
                examEdit.Name = exam.Name;
                examEdit.Note = exam.Note;
                examEdit.EventDate = exam.EventDate;
                examEdit.StartTime = exam.StartTime;
                examEdit.EndTime = exam.EndTime;
                examEdit.UpdatedAt = DateTime.Today;
                if (examRepository.Edit(examEdit))
                {
                    return RedirectToAction("Index");
                }

            }
            return View(exam);
        }
        public ActionResult EnterExamScores()
        {
            List<ExamScore> examScores = new List<ExamScore>();
            var candicates = candicateRepository.Get();
            foreach (var item in candicates)
            {
                ExamScore examScore = new ExamScore();
                examScore.CandicateName = item.Name;
                examScore.CandicateCode = item.Code;
                examScore.Score = 0;
                examScore.Note = "";
                examScores.Add(examScore);
            }
            ViewBag.ExamId = new SelectList(examRepository.Get(), "Id", "Code");
            ViewBag.BatchId = new SelectList(batchRepository.Get(), "Id", "Code");
            return View(examScores);
        }
        // GET: Admin/Exams/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete exam failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            Exam exam = examRepository.FindById(id);
            if (exam == null)
            {
                return Json(new
                {
                    StatusCode = 404,
                    Message = "Delete exam failed!",
                }, JsonRequestBehavior.AllowGet);
            }
            if (examRepository.Remove(exam))
            {
                return Json(new
                {
                    StatusCode = 200,
                    Message = "Delete the exam successfully!",
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                StatusCode = 500,
                message = "Delete exam failed!",
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
