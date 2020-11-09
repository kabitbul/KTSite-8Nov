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
using System.Runtime.InteropServices;

namespace KTSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class InventoryAnalysisController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public InventoryAnalysisController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<InventoryAnalysis> InventoryNeedRestock = new List<InventoryAnalysis>();
            ViewBag.getProductName = new Func<int, string>(returnProductName);
            var product = _unitOfWork.Product.GetAll().Where(a => a.ReStock);
            foreach(Product prod in product)
            {
                double NumOfAvgSell = 0;
                int Count = _unitOfWork.Order.GetAll().Where(a => a.ProductId == prod.Id &&
                              a.OrderStatus != SD.OrderStatusCancelled &&
                              a.OrderStatus != SD.OrderStatusRefunded &&
                              a.UsDate >= DateTime.Now.AddDays((3 - 1) * (-1)) &&
                              a.UsDate >= DateTime.Now.AddDays(-1)).Sum(a => a.Quantity);
                if(Count > 0)
                {
                    NumOfAvgSell = Count / 3;
                }
                
                if((prod.MadeIn == SD.MadeInChina &&(NumOfAvgSell *100) > (prod.InventoryCount + prod.OnTheWayInventory))||
                    (prod.MadeIn == SD.MadeInUSA && (NumOfAvgSell * 20) > prod.InventoryCount + prod.OnTheWayInventory))
                {
                    InventoryAnalysis InvObj = new InventoryAnalysis();
                    InvObj.ProductId = prod.Id;
                    if (prod.MadeIn == SD.MadeInChina)
                    {
                        InvObj.MissingQuantity = (NumOfAvgSell * 100) - (prod.InventoryCount+prod.OnTheWayInventory);
                    }
                    else
                    {
                        InvObj.MissingQuantity = (NumOfAvgSell * 20) - (prod.InventoryCount + prod.OnTheWayInventory);
                    }
                    InvObj.Cost = prod.Cost;
                    InvObj.InventoryCount = prod.InventoryCount;
                    InvObj.OnTheWay = prod.OnTheWayInventory;
                    InvObj.OwnByWarehouse = prod.OwnByWarehouse;
                    InventoryNeedRestock.Add(InvObj);
                }
            }
            return View(InventoryNeedRestock);
        }
        public string returnProductName(int productId)
        {
            return (_unitOfWork.Product.GetAll().Where(q => q.Id == productId).Select(q => q.ProductName)).FirstOrDefault();
        }
        public IActionResult ShowResult()
        {
            var refund = _unitOfWork.Refund.GetAll();
            //ViewBag.getStore =
            //   new Func<string, string>(getStore);
            return View(refund);
        }
        public string getUserName(string unameId)
        {
            return _unitOfWork.ApplicationUser.GetAll().Where(a => a.Id == unameId).Select(a => a.Name).FirstOrDefault();
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
