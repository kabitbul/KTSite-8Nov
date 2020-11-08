﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KTSite.DataAccess.Repository.IRepository;
using KTSite.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using KTSite.Utility;

namespace KTSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotificationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Notification> notification = _unitOfWork.Notification.GetAll();
            ViewBag.getName = new Func<string, string>(getUName);
            return View(notification);
        }
        public string getUNameId()
        {
            return (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Id)).FirstOrDefault();
        }
        public string getUName(string userNameId)
        {
            return (_unitOfWork.ApplicationUser.GetAll().Where(q => q.Id == userNameId).Select(q => q.Name)).FirstOrDefault();
        }
        public IActionResult UpsertNotification(long? id)
        {
            ViewBag.ShowMsg = 0;
            ViewBag.NameId = getUNameId();
            Notification notification = new Notification();
            if(id == null)//create
            {
                return View(notification);
            }
            notification = _unitOfWork.Notification.Get(id.GetValueOrDefault());
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpsertNotification(Notification notification)
        {
            if (ModelState.IsValid)
            {
                if (notification.Id == 0)
                {
                  _unitOfWork.Notification.Add(notification);
                }
                else
                {
                    _unitOfWork.Notification.update(notification);
                }
                ViewBag.ShowMsg = 1;
                _unitOfWork.Save();
                return View(notification);
            }
            ViewBag.ShowMsg = 1;
            return View(notification);
        }
        #region API CALLS
        [HttpDelete]
        public IActionResult Delete(long id)
        {
            var objFromDb = _unitOfWork.Notification.GetAll().Where(a => a.Id == id).FirstOrDefault();
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            _unitOfWork.Notification.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfull" });
        }

        #endregion
    }
}
