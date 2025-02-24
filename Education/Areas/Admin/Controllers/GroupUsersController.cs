﻿using Education.Areas.Admin.Data;
using Education.Areas.Admin.Data.BusinessModel;
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
    public class GroupUsersController : Controller
    {
        // GET: Admin/Business
        private IRepository<Bussiness> businessRepository;
        private IRepository<GroupUser> userRepository;
        private IRepository<Permission> permissRepository;
        private IRepository<GroupPermission> groupRepository;
        private IPaginationService paginationService;
        public GroupUsersController()
        {
            businessRepository = new DbRepository<Bussiness>();
            userRepository = new DbRepository<GroupUser>();
            permissRepository = new DbRepository<Permission>();
            groupRepository = new DbRepository<GroupPermission>();
            paginationService = new DbPaginationService();
        }
        // GET: Admin/UserGroup
        public ActionResult Index()
        {
            var data = userRepository.Get();
            return View(data);
        }
        public ActionResult GetData(int CurrentPage, int Limit, string Key)
        {
            var groupUser = userRepository.Get();
            if (!String.IsNullOrEmpty(Key))
            {
                groupUser = groupUser.Where(x => x.Name.Contains(Key));
            }
            Pagination pagination = paginationService.getInfoPaginate(groupUser.Count(), Limit, CurrentPage);
            var json = JsonConvert.SerializeObject(groupUser.Skip((CurrentPage - 1) * Limit).Take(Limit));
            var data = json;
            return Json(new
            {
                paginate = pagination,
                data = data,
                key = Key,
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(GroupUser groupUser)
        {
            if (userRepository.Add(groupUser))
            {
                return RedirectToAction("Index");
            }
            return View(groupUser);

        }
        public ActionResult Edit(int id)
        {
            GroupUser groupUser = userRepository.FindById(id);
            return View(groupUser);
        }
        [HttpPost]
        public ActionResult Edit(GroupUser groupUser)
        {
            if (userRepository.Edit(groupUser))
            {
                return RedirectToAction("Index");
            }
            return View(groupUser);

        }
        public ActionResult GrantPermission()
        {
            ViewBag.UserGroup = new SelectList(userRepository.Get(), "Id", "Name");
            ViewBag.Business = new SelectList(businessRepository.Get(), "Id", "Name");
            return View();
        }

        public ActionResult GetPermission(int groupId, string businessId)
        {
            var permission = permissRepository.Get().Where(x => x.BussinessId.Equals(businessId))
                                .Select(x => new PermissionViewModel
                                {
                                    Id = x.Id,
                                    BusinessId = x.BussinessId,
                                    Description = x.Description,
                                    IsGranted = false,
                                    Name = x.Name
                                }).ToList();
            foreach (var item in permission)
            {
                if (groupRepository.CheckDuplicate(x => x.GroupUserId == groupId && x.Id == item.Id))
                {
                    item.IsGranted = true;
                }
            }
            return Json(permission, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangePermission(int grId, int perId)
        {
            var groupPermission = groupRepository.Get().SingleOrDefault(x => x.GroupUserId == grId
                                    && x.Id == perId);
            string msg = "";
            if (groupPermission == null)
            {
                groupRepository.Add(new GroupPermission { GroupUserId = grId, Id = perId });
                msg = "Gán quyền thành công!";
            }
            else
            {
                groupRepository.Remove(groupPermission);
                msg = "Hủy quyền thành công!";
            }
            return Json(new { msg = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}