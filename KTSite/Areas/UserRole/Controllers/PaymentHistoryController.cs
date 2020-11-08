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

namespace KTSite.Areas.UserRole.Controllers
{
    [Area("UserRole")]
    [Authorize(Roles = SD.Role_Users)]
    public class PaymentHistoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentHistoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
              ViewBag.getPaymentAddress =
                  new Func<int, string>(getPaymentAddress);
              ViewBag.getPaymentType =
                new Func<int, string>(getPaymentType);
            string UNameId = (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Id)).FirstOrDefault();
            var PaymentHistory = _unitOfWork.PaymentHistory.GetAll().Where(a => a.UserNameId == UNameId);
            return View(PaymentHistory);
        }
        public IActionResult UpsertPayment(int? Id)
        {
            string uNameId = (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Id)).FirstOrDefault();
            ViewBag.uNameId = uNameId;
            PaymentHistoryVM paymentHistoryVM = new PaymentHistoryVM()
            {
                PaymentHistory = new PaymentHistory(),
                PaymentAddress = _unitOfWork.PaymentSentAddress.GetAll().Where(a=> a.UserNameId == uNameId).Select(i => new SelectListItem
                {
                    Text = i.PaymentTypeAddress + " - " + i.PaymentType,
                    Value = i.Id.ToString()
                })
            };
            if (Id != null)
            {
                paymentHistoryVM.PaymentHistory = _unitOfWork.PaymentHistory.GetAll().Where(a => a.Id == Id).FirstOrDefault();
            }
            ViewBag.ShowMsg = 0;
            ViewBag.payoneermsg = false;
            return View(paymentHistoryVM);
        }
        public string getPaymentAddress(int Id)
        {
            return _unitOfWork.PaymentSentAddress.GetAll().Where(a => a.Id == Id).Select(a => a.PaymentTypeAddress).FirstOrDefault();
        }
        public string getPaymentType(int Id)
        {
            return _unitOfWork.PaymentSentAddress.GetAll().Where(a => a.Id == Id).Select(a => a.PaymentType).FirstOrDefault();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpsertPayment(PaymentHistoryVM paymentHistoryVM)
        {
            string uNameId = (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Id)).FirstOrDefault();
            paymentHistoryVM.PaymentAddress = _unitOfWork.PaymentSentAddress.GetAll().Where(a => a.UserNameId == uNameId).Select(i => new SelectListItem
            {
                Text = i.PaymentTypeAddress + " - " + i.PaymentType,
                Value = i.Id.ToString()
            });
            if (ModelState.IsValid)
            {
                PaymentSentAddress paymentSentAddress =
                 _unitOfWork.PaymentSentAddress.GetAll().Where(a => a.Id == paymentHistoryVM.PaymentHistory.SentFromAddressId).FirstOrDefault();
                if (paymentSentAddress.PaymentType == SD.PaymentPaypal)//then fees will apply
                {
                    paymentHistoryVM.PaymentHistory.Amount = paymentHistoryVM.PaymentHistory.Amount - SD.paypalOneTimeFee -
                        (paymentHistoryVM.PaymentHistory.Amount * SD.paypalPercentFees / 100);
                }
                if (paymentHistoryVM.PaymentHistory.Id == 0)
                {
                    _unitOfWork.PaymentHistory.Add(paymentHistoryVM.PaymentHistory);
                }
                else
                {
                    _unitOfWork.PaymentHistory.update(paymentHistoryVM.PaymentHistory);
                }
                    _unitOfWork.Save();
                ViewBag.ShowMsg = 1;
                return View(paymentHistoryVM);
                //return RedirectToAction(nameof(Index));
            }
            return View(paymentHistoryVM);
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.PaymentHistory.GetAll();
            return Json(new { data = allObj });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.PaymentHistory.Get(id);
            if(objFromDb == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            _unitOfWork.PaymentHistory.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfull" });
        }

        #endregion
    }
}
