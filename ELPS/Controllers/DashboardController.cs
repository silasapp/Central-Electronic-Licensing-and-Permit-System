using ELPS.Domain.Abstract;
using ELPS.Helpers;
using ELPS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        #region Repositories
        ILicenseRepository _licenseRep;
        IPermitRepository _permitRep;
        ICompanyRepository _coyRep;
        IvCompanyFileRepository _vCoyFileRep;
        IMessageRepository _msgRep;
        IAppIdentityRepository _appIdRep;
        IApplicationRepository _appRep;
        CompanyHelper coyHelper;
        #endregion

        public DashboardController(ILicenseRepository license, IPermitRepository permit, ICompanyRepository coy, IMessageRepository msg,
            IvCompanyFileRepository vCoyFile, IAppIdentityRepository appId, IApplicationRepository appRep)
        {
            _appRep = appRep;
            _appIdRep = appId;
            _msgRep = msg;
            _vCoyFileRep = vCoyFile;
            _licenseRep = license;
            _permitRep = permit;
            _coyRep = coy;

            coyHelper = new CompanyHelper(coy, appRep);
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            var dashVM = new DashboardViewModel();
            var licenses = _appIdRep.FindBy(a => a.IsActive && a.OfficeUse == null || a.OfficeUse.Value == false).ToList();
            var myCoy = coyHelper.MyCompany(User.Identity.Name);

            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }


            foreach (var license in licenses)
            {
                //var xx = license.BaseUrl;
                license.MyPermits = _permitRep.FindBy(a => a.LicenseId == license.Id && a.Company_Id == myCoy.Id).Count();
                license.LicensesInProcessing = coyHelper.AppsInProcessing(myCoy.Id, license.Id);
            }
            
            dashVM.Licenses = licenses;
            dashVM.Company = myCoy;
            dashVM.Documents = _vCoyFileRep.FindBy(a => a.Id == myCoy.Id).Take(5).ToList();
            dashVM.Messages = _msgRep.FindBy(a => a.Company_Id == myCoy.Id).OrderByDescending(a => a.Date).Take(10).ToList();
            return View(dashVM);
        }

        public ActionResult LicenseList()
        {
            var licenses = _appIdRep.FindBy(a => a.IsActive && a.OfficeUse == null || a.OfficeUse.Value == false).ToList();
            var myCoy = coyHelper.MyCompany(User.Identity.Name);
            foreach (var license in licenses)
            {
                //var xx = license.BaseUrl;
                license.MyPermits = _permitRep.FindBy(a => a.LicenseId == license.Id && a.Company_Id == myCoy.Id).Count();
                license.LicensesInProcessing = coyHelper.AppsInProcessing(myCoy.Id, license.Id);
            }

            return View(licenses);
        }

        public ActionResult GetLicenses()
        {
            var licenses = _appIdRep.FindBy(a => a.IsActive && a.OfficeUse == null || a.OfficeUse.Value == false).ToList().OrderBy(a => a.ShortName);

            return Json(licenses, JsonRequestBehavior.AllowGet);
        }

    }
}