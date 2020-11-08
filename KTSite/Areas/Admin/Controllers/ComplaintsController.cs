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
    public class ComplaintsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ComplaintsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var complaints = _unitOfWork.Complaints.GetAll().Where(a=>a.IsAdmin);
            ViewBag.getStore =
               new Func<string, string>(getStore);
            ViewBag.IsAdmin = new Func<string, bool>(returnIsAdmin);
            return View(complaints);
        }
        public bool returnIsAdmin(string OrderId)
        {
            return _unitOfWork.Order.GetAll().Where(a => a.Id == Convert.ToInt64(OrderId)).Select(a => a.IsAdmin).FirstOrDefault();
        }
        public IActionResult ShowUserComplaint()
        {
            ViewBag.getUserName =
                   new Func<string, string>(getUserName);
            var complaints = _unitOfWork.Complaints.GetAll().Where(a =>!a.IsAdmin);
            ViewBag.Refunded = new Func<string, bool>(returnIsRefunded);
            return View(complaints);
        }
        public bool returnIsRefunded(string OrderId)
        {
            Refund refund = _unitOfWork.Refund.GetAll().Where(a => a.OrderId == Convert.ToInt64(OrderId)).FirstOrDefault();
            if (refund != null)
            {
                return true;
            }
            return false;
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
        public IActionResult AddComplaint(long? Id)
        {
            ComplaintsVM complaintsVM;
            string uNameId = "";
            string uName = "";
            uNameId = returnUserNameId();
            uName = (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.UserName)).FirstOrDefault();
            //if (Id == null)
            //{
                complaintsVM = new ComplaintsVM()
                {
                    complaints = new Complaints(),
                    OrdersList = _unitOfWork.Complaints.getAllOrdersForAdmin().
                    Select(i => new SelectListItem
                    {
                        Text = i.CustName + "- Id: " + i.Id,
                        Value = i.Id.ToString()
                    })
                };
            if (Id != null)
            {
                complaintsVM.complaints.OrderId = Id;
            }

            ViewBag.UNameId = uNameId;
            ViewBag.sysDate = DateTime.Now;
            ViewBag.ShowMsg = 0;
            ViewBag.failed = false;
            return View(complaintsVM);
        }
        public IActionResult UpdateComplaint(long Id)
        {
            bool IsAdmin = _unitOfWork.Complaints.GetAll().Where(a => a.Id == Id).Select(a => a.IsAdmin).FirstOrDefault();
            Complaints comp = _unitOfWork.Complaints.GetAll().Where(a => a.Id == Id).FirstOrDefault();
            string uNameId = comp.UserNameId;
            ComplaintsVM complaintsVM;
            complaintsVM = new ComplaintsVM()
            {
                complaints = _unitOfWork.Complaints.GetAll().Where(a => a.Id == Id).FirstOrDefault(),
                OrdersList = _unitOfWork.Order.GetAll().Where(a => a.Id == comp.OrderId).
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
        public IActionResult AddComplaint(ComplaintsVM complaintsVM)
        {
            if (ModelState.IsValid)
            {
                complaintsVM.complaints.IsAdmin = _unitOfWork.Order.GetAll().Where(a => a.Id == complaintsVM.complaints.OrderId).
                    Select(a => a.IsAdmin).FirstOrDefault();
                if (complaintsVM.complaints.Id == 0)
                {
                    _unitOfWork.Complaints.Add(complaintsVM.complaints);
                }
                else
                {
                    _unitOfWork.Complaints.update(complaintsVM.complaints);
                }

                _unitOfWork.Save();
                ViewBag.ShowMsg = 1;


                //return RedirectToAction(nameof(Index));
            }
            ComplaintsVM complaintsVM2 = new ComplaintsVM()
            {
                complaints = new Complaints(),
                OrdersList = _unitOfWork.Order.GetAll().Where(a => a.UserNameId == returnUserNameId()).Where(a => a.OrderStatus == SD.OrderStatusDone).
    Select(i => new SelectListItem
    {
        Text = i.CustName + "- Id: " + i.Id,
        Value = i.Id.ToString()
    })
            };
            return View(complaintsVM2);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateComplaint(ComplaintsVM complaintsVM)
        {

            if (ModelState.IsValid)
            {
                complaintsVM.complaints.HandledBy =
                    (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Name)).FirstOrDefault();
                _unitOfWork.Complaints.update(complaintsVM.complaints);
                _unitOfWork.Save();
                ViewBag.ShowMsg = 1;
                //return RedirectToAction(nameof(Index));
            }
            ComplaintsVM complaintsVM2;
            if (complaintsVM.complaints.IsAdmin)
            {
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
            }
            else
            {
                complaintsVM2 = new ComplaintsVM()
                {
                    complaints = _unitOfWork.Complaints.GetAll().Where(a => a.Id == complaintsVM.complaints.Id).FirstOrDefault(),
                    OrdersList = _unitOfWork.Order.GetAll().Where(a => !a.IsAdmin).Where(a => a.OrderStatus == SD.OrderStatusDone).
                    Select(i => new SelectListItem
                    {
                        Text = i.CustName + "- Id: " + i.Id,
                        Value = i.Id.ToString()
                    })
                };
            }
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
