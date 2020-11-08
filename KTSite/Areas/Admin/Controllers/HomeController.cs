using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KTSite.Models;
using KTSite.DataAccess.Repository.IRepository;
using KTSite.Utility;

namespace KTSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            if(User.IsInRole(SD.Role_Admin))
            {
                return View();
            }
            else if (User.IsInRole(SD.Role_Users))
            {
                return Redirect("UserRole/Home");
            }
            else if (User.IsInRole(SD.Role_VAs))
            {
                return Redirect("VAs/Home");
            }
            else if (User.IsInRole(SD.Role_Warehouse))
            {
                return Redirect("Warehouse/Home");
            }
            //string uNameId = (_unitOfWork.ApplicationUser.GetAll().Where(q => q.UserName == User.Identity.Name).Select(q => q.Id)).FirstOrDefault();
            //if (uNameId == null)
            // {
            return Redirect("Identity/Account/Login");
            // }

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
