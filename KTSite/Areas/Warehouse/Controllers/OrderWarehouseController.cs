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
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Collections;
using KTSite.Areas.Warehouse.Views.OrderWarehouse;
using ExcelDataReader;
using CsvHelper;

namespace KTSite.Areas.Warehouse.Controllers
{
    [Area("Warehouse")]
    [Authorize(Roles = SD.Role_Warehouse)]
    public class OrderWarehouseController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderWarehouseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var orderList = _unitOfWork.Order.GetAll();
            dynamic myModel = new System.Dynamic.ExpandoObject();
            myModel.Order = _unitOfWork.Order.GetAll();
            myModel.ordercsv = new List<KTSite.Areas.Warehouse.Views.OrderWarehouse.CSVOrderLine>();
            ViewBag.getProductName =
               new Func<int, string>(returnProductName);
            ViewBag.getStoreName =
               new Func<int, string>(returnStoreName);
            ViewBag.getCost =
                          new Func<int, double ,double>(returnCost);
            ViewBag.errSaveInProgress = false;
            return View(myModel);

        }
        public void setInProgressStatus(long Id)
        {
            Order order = _unitOfWork.Order.GetAll().Where(a => a.Id == Id).FirstOrDefault();
            order.OrderStatus = SD.OrderStatusInProgress;
            _unitOfWork.Order.update(order);
            _unitOfWork.Save();
        }
        public string getProductName(int productId)
        {
            return _unitOfWork.Product.GetAll().Where(a => a.Id == productId).Select(a => a.ProductName).FirstOrDefault();
        }
        public string getWeight(int productId)
        {
            double weight = _unitOfWork.Product.GetAll().Where(a => a.Id == productId).Select(a => a.Weight).FirstOrDefault();
            return weight.ToString();
        }
        public IActionResult AddTrackingManually(long id)
        {
            OrderVM orderVM = new OrderVM()
            {

                Orders = new Order()
                
            };
            
            orderVM.Orders = _unitOfWork.Order.Get(id);
            if (orderVM.Orders == null)
            {
                return NotFound();
            }
            ViewBag.emptyTracking = false;
            return View(orderVM);
        }
        public string returnProductName(int productId)
        {
            return (_unitOfWork.Product.GetAll().Where(q => q.Id == productId).Select(q => q.ProductName)).FirstOrDefault();
        }
        public string returnStoreName(int storeId)
        {
            return (_unitOfWork.UserStoreName.GetAll().Where(q => q.Id == storeId).Select(q => q.StoreName)).FirstOrDefault();
        }
        public double returnCost(int productId, double quantity)
        {
            double productCost = (_unitOfWork.Product.GetAll().Where(q => q.Id == productId).Select(q => q.Cost)).FirstOrDefault();
            return Convert.ToDouble(String.Format("{0:0.00}", (productCost * quantity))); 
        }

        [HttpPost]
        public JsonResult ChangeInProgress()
        {
            var orderList  = _unitOfWork.Order.GetAll().Where(a => a.OrderStatus == SD.OrderStatusInProgress);
            foreach(Order order in orderList)
            {
                order.OrderStatus = SD.OrderStatusAccepted;
                _unitOfWork.Order.update(order);
                _unitOfWork.Save();
            }
            return Json(new { });

        }
        [HttpPost]
        public JsonResult ExportData()
        {
            var orderList = _unitOfWork.Order.GetAll().Where(a => a.OrderStatus == SD.OrderStatusAccepted).OrderBy(a => a.Id);
            string path = SD.PathToCreateCSV + "_" + DateTime.Now.DayOfWeek + "_HH" + DateTime.Now.Hour + "_MI" + DateTime.Now.Minute+".csv";
            ViewBag.errSaveInProgress = false;
            string excep = "No Orders In Status Accepted left.";
            int success=0;
            int lineCounter = 0;
            if(orderList.Count() == 0)
            {
                return Json(new { excep, success });
            }
            bool existInProgress = _unitOfWork.Order.GetAll().Any(a => a.OrderStatus == SD.OrderStatusInProgress);
            if(existInProgress)
            {
                excep = "Cannot create file, some orders are already in status In Progress.";
                success = 0;
                return Json(new { excep, success });
            }
            try
            {
                using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(true)))
                using (CsvWriter cw = new CsvWriter(sw, System.Globalization.CultureInfo.CurrentCulture))
                {
                    cw.WriteHeader<CSVColumnOrders>();
                    cw.NextRecord();
                    foreach (Order order in orderList)
                    {
                        if(lineCounter >= 300)
                        {
                            break;
                        }
                        CSVColumnOrders csvColumn = new CSVColumnOrders();
                        csvColumn.Product = getProductName(order.ProductId) + "- " + order.Quantity;
                        csvColumn.Name = order.CustName.Replace(",", "").Replace("\"", "");
                        csvColumn.Address1 = order.CustStreet1.Replace(",", "").Replace("\"", "");
                        csvColumn.Address2 = order.CustStreet2.Replace(",", "").Replace("\"", "");
                        csvColumn.City = order.CustCity;
                        csvColumn.State = order.CustState;
                        csvColumn.Zip = order.CustZipCode;
                        csvColumn.Country = "US";
                        if (order.CustPhone != null)
                        {
                            csvColumn.Phone = order.CustPhone;
                        }
                        else
                        {
                            csvColumn.Phone = "999-999-9999";
                        }
                        csvColumn.Quantity = order.Quantity.ToString();
                        csvColumn.Weight = getWeight(order.ProductId);

                        cw.WriteRecord<CSVColumnOrders>(csvColumn);
                        try
                        {
                            setInProgressStatus(order.Id);
                        }
                        catch
                        {
                            ViewBag.errSaveInProgress = true;
                        }
                        cw.NextRecord();
                        lineCounter++;
                    }
                }
                excep = "File Created Successfully in : " + path;
                success = 1;
            }
            catch (IOException e)
            {
                excep = "An Error Occured,could not create File.";
                success = 0;
            }
            return Json(new { excep ,success});
        }
        [HttpPost]
        public JsonResult Submit(IFormFile CSVFile)
        {
            int success = 0;
            string excep = "";
            var result = new StringBuilder();
            try
            {
                using (var reader = new StreamReader(CSVFile.OpenReadStream()))
                {
                    if(reader.Peek() <= 1)
                    {
                        success = 0;
                        excep = "File is Empty";
                        return Json(new { excep, success });
                    }
                    while (reader.Peek() >= 0)
                        result.AppendLine(reader.ReadLine());
                }
                string[] lines = result.ToString().Split(Environment.NewLine.ToCharArray());
                lines = lines.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                foreach (String line in lines)
                {
                    string[] columns = line.Split(',');
                    if (columns[51] != null)
                    {
                        string custName = columns[51].Replace("\"", "");
                        string custZip = columns[57].Replace("\"", "");
                        Order order =
                           _unitOfWork.Order.GetAll().Where(a => a.OrderStatus == SD.OrderStatusInProgress)
                                                     .Where(a => a.CustName.Replace(",", "").Replace("\"", "") == custName)
                                                     .Where(a => a.CustZipCode.Substring(0,5) == custZip.Substring(0, 5))//compare first 5
                                                     .Where(a=>a.TrackingNumber == null)
                                                     .FirstOrDefault();
                        if (order != null)
                        {
                            order.TrackingNumber = columns[1].Replace("\"", "").Replace("=", "");
                            order.Carrier = columns[2].Replace("\"", "");
                            order.OrderStatus = SD.OrderStatusDone;
                            _unitOfWork.Order.update(order);
                            _unitOfWork.Save();
                        }
                    }
                }
                success = 1;
                excep = "Tracking Updated Succesfully!";
            }
            catch
            {
                success = 0;
                excep = "There was an Error, some orders were not updated!";
            }
            return Json(new { excep, success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTrackingManually(OrderVM orderVM)
        {
            if (ModelState.IsValid)
            {
                ViewBag.ShowMsg = 1;
                // if(isStoreAuthenticated(orderVM) && orderVM.Orders.UsDate <= DateTime.Now)
                //            {
                if (orderVM.Orders.TrackingNumber == null)
                {
                    ViewBag.emptyTracking = true;
                }
                else
                {
                    orderVM.Orders.OrderStatus = SD.OrderStatusDone;
                    _unitOfWork.Order.update(orderVM.Orders);
                    _unitOfWork.Save();
                    ViewBag.failed = false;
                    ViewBag.emptyTracking = false;
                }
            }
            return View(orderVM);
        }

        public bool isStoreAuthenticated(OrderVM orderVM)
        {
            //get userName id based on store
            bool isAdmin = _unitOfWork.UserStoreName.GetAll().Where
                (a => a.Id == orderVM.Orders.StoreNameId).
                Select(a => a.IsAdminStore).FirstOrDefault();
            if (isAdmin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string returnUserNameId()
        {
            return (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Id)).FirstOrDefault();
        }
        public int getProductIdByName(string productName)
        {
            return _unitOfWork.Product.GetAll().Where(a => a.ProductName.Equals(productName,StringComparison.InvariantCultureIgnoreCase))
                .Select(a => a.Id).FirstOrDefault();
        }
        public int getStoreNameId(string storeName)
        {
            return _unitOfWork.UserStoreName.GetAll().Where(a => a.StoreName.Equals(storeName, StringComparison.InvariantCultureIgnoreCase))
                .Select(a => a.Id).FirstOrDefault();
        }
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Order.GetAll();
            return Json(new { data = allObj });
        }
        [HttpGet]
        public IActionResult GetAllWithoutTracking()
        {
            var allObj = _unitOfWork.Order.GetAll().Where(a => a.TrackingNumber == null);
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
