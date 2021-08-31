using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
using Education.Areas.Admin.Data.BusinessModel.Interface;
using Education.Areas.Admin.Data.ViewModel;
using Education.BLL;
using Education.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Education.Areas.Admin.Controllers
{
    [CustomizeAuthorize]
    public class ReportController : Controller
    {
        private IRepository<Candicate> candicateRepository;
        private IRepository<LearningInfo> learningRepository;
        private IPaginationService paginationService;
        private IRepository<ClassRoom> batchRepository;
        private IRepository<User> userRepository;
        private IRepository<Exam> examRepository;
        private IBatchModelService batchModelService;
        public ReportController()
        {
            candicateRepository = new DbRepository<Candicate>();
            learningRepository = new DbRepository<LearningInfo>();
            paginationService = new DbPaginationService();
            batchRepository = new DbRepository<ClassRoom>();
            batchModelService = new DbBatchModelService();
            userRepository = new DbRepository<User>();
            examRepository = new DbRepository<Exam>();
        }
        // GET: Admin/Report
        public ActionResult ReportStudentByMonthAndYear()
        {
            var student = candicateRepository.Get();
            List<object> Month = new List<Object> {
                        new { value = 1, text = "January" },
                        new { value = 2, text = "February" },
                        new { value = 3, text = "March" },
                        new { value = 4, text = "April" },
                        new { value = 5, text = "May" },
                        new { value = 6, text = "June" },
                        new { value = 7, text = "July" },
                        new { value = 8, text = "August" },
                        new { value = 9, text = "September" },
                        new { value = 10, text = "October" },
                        new { value = 11, text = "November" },
                        new { value = 12, text = "December" },
                    };
            int monthNow = DateTime.Today.Month;
            List<int> Years = new List<int>();
            int yearNow = DateTime.Today.Year;
            Years.Add(yearNow);
            for (int i = 1; i <= 20; i++)
            {
                var Year1 = yearNow - i;
                var Year2 = yearNow + i;
                Years.Add(Year1);
                Years.Add(Year2);
            }
            Years.Sort();
            ViewBag.Month = new SelectList(Month.ToList(), "value", "text", monthNow);
            ViewBag.Year = new SelectList(Years, yearNow);
            return View(student);
        }
        public ActionResult GetReportMonthAndYear(int CurrentPage, int Limit, int Month, int Year)
        {
            var student = candicateRepository.Get();
            List<Candicate> list = new List<Candicate>();
            foreach (var item in student)
            {
                var JoinDate = item.JoiningDate;
                int yearJoin = JoinDate.Year;
                int monthJoin = JoinDate.Month;
                if (yearJoin == Year && Month == monthJoin)
                {
                    list.Add(item);
                }
            }
            Pagination pagination = paginationService.getInfoPaginate(list.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(list.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult StillGoBackToSchool()
        {
            var learning = learningRepository.Get(x => x.Status == false);
            var Candicate = new HashSet<Candicate>();
            foreach (var item in learning)
            {

                if (learningRepository.Get(x => x.CandicateId == item.CandicateId).Where(x => x.Status).Count() > 0)
                {
                    Candicate.Add(candicateRepository.FindById(item.CandicateId));
                }
            }
            return View(Candicate);
        }

        public ActionResult GetReportStillGoBackToSchoolr(int CurrentPage, int Limit)
        {
            var learning = learningRepository.Get(x => x.Status == false);
            var Candicate = new HashSet<Candicate>();
            foreach (var item in learning)
            {

                if (learningRepository.Get(x => x.CandicateId == item.CandicateId).Where(x => x.Status).Count() > 0)
                {
                    Candicate.Add(candicateRepository.FindById(item.CandicateId));
                }
            }
            Pagination pagination = paginationService.getInfoPaginate(Candicate.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(Candicate.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReportStudentByClassRoom()
        {
            var room = batchRepository.Get();
            ViewBag.Room = new SelectList(room, "Id", "Code");
            var learning = learningRepository.Get();
            return View(learning);
        }
        public ActionResult GetReportStudentByClassRoom(int CurrentPage, int Limit, int Room)
        {
            var learning = learningRepository.Get();
            if (Room > 0)
            {
                learning = learningRepository.Get(x => x.RoomId == Room);
            }
            List<LearningInfo> list = new List<LearningInfo>();
            foreach (var item in learning)
            {
                Candicate candicate = candicateRepository.FindById(item.CandicateId);
                item.Candicate = candicate;
                list.Add(item);
            }
            Pagination pagination = paginationService.getInfoPaginate(list.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(list.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CompleteCertificate()
        {
            var learning = learningRepository.Get(x => x.Status == false);
            var Candicate = new HashSet<Candicate>();
            foreach (var item in learning)
            {
                Candicate.Add(candicateRepository.FindById(item.CandicateId));
            }
            return View(Candicate);
        }

        public ActionResult GetCompleteCertificate(int CurrentPage, int Limit)
        {
            var learning = learningRepository.Get(x => x.Status == false);
            var Candicate = new HashSet<Candicate>();
            foreach (var item in learning)
            {
                Candicate.Add(candicateRepository.FindById(item.CandicateId));
            }
            Pagination pagination = paginationService.getInfoPaginate(Candicate.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(Candicate.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TheClassHasEnded()
        {
            var room = batchRepository.Get(x => x.Status == 3);
            IEnumerable<BatchView> view = batchModelService.convertListBatchView(room);
            return View(view);
        }

        public ActionResult GetTheClassHasEnded(int CurrentPage, int Limit, string Key = null)
        {
            var room = batchRepository.Get(x => x.Status == 3);
            IEnumerable<BatchView> view = batchModelService.convertListBatchView(room);
            if (!string.IsNullOrEmpty(Key))
            {
                view = view.Where(x => x.Code.Contains(Key)
                                        || x.Name.Contains(Key)
                                        || x.Course.Contains(Key)
                                        || x.Faulty.Contains(Key)
                                        || x.FaultyOld.Contains(Key)
                                        || x.User.Contains(Key)
                                      );
            }
            Pagination pagination = paginationService.getInfoPaginate(view.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(view.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ClassesAreStarting()
        {
            var room = batchRepository.Get(x => x.Status == 2);
            IEnumerable<BatchView> view = batchModelService.convertListBatchView(room);
            return View(view);
        }

        public ActionResult GetClassesAreStarting(int CurrentPage, int Limit, string Key = null)
        {
            var room = batchRepository.Get(x => x.Status == 2);
            IEnumerable<BatchView> view = batchModelService.convertListBatchView(room);
            if (!string.IsNullOrEmpty(Key))
            {
                view = view.Where(x => x.Code.Contains(Key)
                                        || x.Name.Contains(Key)
                                        || x.Course.Contains(Key)
                                        || x.Faulty.Contains(Key)
                                        || x.FaultyOld.Contains(Key)
                                        || x.User.Contains(Key)
                                      );
            }
            Pagination pagination = paginationService.getInfoPaginate(view.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(view.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RemainingTuition()
        {
            var learnings = learningRepository.Get(x => x.TuitionPaid < x.Tuition || x.TuitionPaid == null);
            foreach (var item in learnings)
            {
                item.Candicate = candicateRepository.FindById(item.CandicateId);
                item.User = userRepository.FindById(item.UserId);
                item.Exam = examRepository.FindById(item.ExamId);
                item.ClassRoom = batchRepository.FindById(item.RoomId);
            }
            return View(learnings);
        }

        public ActionResult GetRemainingTuition(int CurrentPage, int Limit, string Key = null)
        {
            var learnings = learningRepository.Get(x => x.TuitionPaid < x.Tuition || x.TuitionPaid == null);
            foreach (var item in learnings)
            {
                item.Candicate = candicateRepository.FindById(item.CandicateId);
                item.User = userRepository.FindById(item.UserId);
                item.Exam = examRepository.FindById(item.ExamId);
                item.ClassRoom = batchRepository.FindById(item.RoomId);
            }
            if (!string.IsNullOrEmpty(Key))
            {
                learnings = learnings.Where(x => x.Candicate.Name.Contains(Key)
                                        || x.ClassRoom.Name.Contains(Key)
                                      );
            }
            Pagination pagination = paginationService.getInfoPaginate(learnings.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(learnings.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}