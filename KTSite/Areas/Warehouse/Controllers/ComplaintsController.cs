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

namespace KTSite.Areas.Warehouse.Controllers
{
    [Area("Warehouse")]
    [Authorize(Roles = SD.Role_Warehouse)]
    public class ComplaintsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ComplaintsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var complaints = _unitOfWork.Complaints.GetAll();
            ViewBag.getStore =
               new Func<string, string>(getStore);
            return View(complaints);
        }
        public string getUserName(string unameId)
        {
            return _unitOfWork.ApplicationUser.GetAll().Where(a => a.Id == unameId).Select(a => a.Name).FirstOrDefault();
        }
        public string getStore(string orderId)
        {
             int storeId = _unitOfWork.Order.GetAll().Where(a => a.Id == Convert.ToInt64(orderId)).Select(a => a.StoreNameId).FirstOrDefault();
             return _unitOfWork.UserStoreName.GetAll().Where(a => a.Id == storeId).Select(a => a.StoreName).FirstOrDefault();
        }
        public IActionResult UpdateComplaint(long Id)
        {
            bool IsAdmin = _unitOfWork.Complaints.GetAll().Where(a => a.Id == Id).Select(a => a.IsAdmin).FirstOrDefault();
            //string uNameId = _unitOfWork.Complaints.GetAll().Where(a => a.Id == Id).Select(a => a.UserNameId).FirstOrDefault();
            ComplaintsVM complaintsVM;
                complaintsVM = new ComplaintsVM()
                {
                    complaints = _unitOfWork.Complaints.GetAll().Where(a => a.Id == Id).FirstOrDefault(),
                    OrdersList = _unitOfWork.Order.GetAll().Where(a => a.OrderStatus == SD.OrderStatusDone).
                     Select(i => new SelectListItem
                     {
                         Text = i.CustName + "- Id: " + i.Id,
                         Value = i.Id.ToString()
                     })
                };
            ViewBag.IsAdmin = IsAdmin;
            return View(complaintsVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateComplaint(ComplaintsVM complaintsVM)
        {

            if (ModelState.IsValid)
            {
                complaintsVM.complaints.HandledBy =
                    (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Name)).FirstOrDefault();
                //if tracking number is not empty, complaint is solved
                if(complaintsVM.complaints.NewTrackingNumber != null)
                {
                    complaintsVM.complaints.Solved = true;
                }
                _unitOfWork.Complaints.update(complaintsVM.complaints);
                _unitOfWork.Save();
                ViewBag.ShowMsg = 1;
                //return RedirectToAction(nameof(Index));
            }
            ComplaintsVM complaintsVM2;
                complaintsVM2 = new ComplaintsVM()
                {
                    complaints = _unitOfWork.Complaints.GetAll().Where(a => a.Id == complaintsVM.complaints.Id).FirstOrDefault(),
                    OrdersList = _unitOfWork.Order.GetAll().Where(a => a.IsAdmin).Where(a => a.OrderStatus == SD.OrderStatusDone).
                    Select(i => new SelectListItem
                    {
                        Text = i.CustName + "- Id: " + i.Id,
                        Value = i.Id.ToString()
                    })
                };
            
            ViewBag.IsAdmin = complaintsVM2.complaints.IsAdmin;
            return View(complaintsVM2);
        }
        public string returnUserNameId()
        {
            return (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Id)).FirstOrDefault();
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Product.GetAll(includePoperties:"Category");
            return Json(new { data = allObj });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Product.Get(id);
            if(objFromDb == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            _unitOfWork.Product.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfull" });
        }

        #endregion
    }
}
