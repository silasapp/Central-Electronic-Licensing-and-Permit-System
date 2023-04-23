using ELPS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    public class HomeController : Controller
    {
        ICompanyRepository _compRep;
        IAppIdentityRepository _appIDRep;

        public HomeController(ICompanyRepository coy,IAppIdentityRepository appIDRep)
        {
            _appIDRep = appIDRep;
            _compRep = coy;
        }

        
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                if (User.IsInRole("Admin")|| User.IsInRole("Account") || User.IsInRole("ITAdmin")  ||
                     User.IsInRole("ManagerObserver") || User.IsInRole("Support") ||
                    User.IsInRole("LicenseAdmin") || User.IsInRole("Staff"))
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                if (User.IsInRole("FacilityManager"))
                {
                  var app = _appIDRep.FindBy(a => a.ShortName == "ADS").FirstOrDefault();
                    TempData["AppData"] = app;
                    return RedirectToAction("ProcessAppData","Account");
                }
                else
                {
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}