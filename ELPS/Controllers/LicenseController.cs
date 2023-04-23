using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ELPS.Models;
using ELPS.Crawler;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ELPS.Controllers
{
    [Authorize]
    public class LicenseController : Controller
    {
        #region Repository
        ILicenseRepository _licenseRep;
        IAppIdentityRepository _appIdentity;
        IApplicationRepository _appRep;
        IPermitRepository _permiTRep;
        IvPermitRepository _vPermitRep;
        ICompanyRepository _compRep;
        IvExpiringLicenseRepository _vExpLicenseRep;
        IExpiringNotificationRepository _expNotifRep;
        IPermitCategoryRepository _permitCatRep;
        IStateRepository _stateRep;
        IvZoneRepository _vZoneRep;
        IvBranchRepository _vBranchRep;
        IvZoneStateRepository _vZoneStateRep;
        IMailReceiptRepository _mailRecRep;
        IvPermit_with_amountRepository _vPermitReportRep;
        IDivisionRepo _categoryRepo;
        IPortalToDivision _portalToCategory;
        #endregion


        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;


        public LicenseController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
            //SignInManager = signInManager;
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        public LicenseController(ILicenseRepository license, IAppIdentityRepository appId, IApplicationRepository appRep, ICompanyRepository compRep, IvExpiringLicenseRepository vExpLicenseRep,
            IExpiringNotificationRepository expNotifRep, IPermitRepository permiTRep, IvPermitRepository vPermitRep, IStateRepository stateRep,
            IPermitCategoryRepository permitCatRep, IvZoneRepository vZoneRep, IvBranchRepository vBranchRep, IvZoneStateRepository vZoneStateRep,
            IMailReceiptRepository mailRecRep, IvPermit_with_amountRepository vPermitReportRep, IDivisionRepo categoryRepo, IPortalToDivision portalToCategory)
        {
            _vPermitReportRep = vPermitReportRep;
            _mailRecRep = mailRecRep;
            _vZoneStateRep = vZoneStateRep;
            _vZoneRep = vZoneRep;
            _vBranchRep = vBranchRep;
            _stateRep = stateRep;
            _permitCatRep = permitCatRep;
            _vPermitRep = vPermitRep;
            _permiTRep = permiTRep;
            _expNotifRep = expNotifRep;
            _compRep = compRep;
            _appRep = appRep;
            _appIdentity = appId;
            _licenseRep = license;
            _vExpLicenseRep = vExpLicenseRep;
            _categoryRepo = categoryRepo;
            _portalToCategory = portalToCategory;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,ITAdmin,LicenseAdmin")]
        public ActionResult Index()
        {
            if (User.IsInRole("LicenseAdmin"))
            {
                var em = User.Identity.Name;
                var li = _appIdentity.FindBy(a => a.Email == em).FirstOrDefault();
                return RedirectToAction("EditLicense", new { id = li.Id });
            }
            var licenses = _appIdentity.GetAll().ToList();
            return View(licenses);
        }




        [Authorize(Roles = "Admin,ITAdmin,LicenseAdmin")]
        public ActionResult AddLicense()
        {
            ViewBag.Categories = _categoryRepo.GetAll().ToList();
            var viewModel = new AppIdentity
            {
                AppId = PaymentRef.RefrenceCode()
            };
            //why generate payment referenceCode before sending to the view??
            //return View(new AppIdentity() { AppId = PaymentRef.RefrenceCode() });
            return View(viewModel);
        }


        [HttpPost]
        [Authorize(Roles = "Admin,ITAdmin,LicenseAdmin")]
        public ActionResult AddLicense(AppIdentity model)
        {

            ViewBag.Categories = _categoryRepo.GetAll().ToList();
            if (ModelState.IsValid)
            {
                
                var user = new ApplicationUser {UserName = model.Email, Email = model.Email, EmailConfirmed = true };


                var result = UserManager.Create(user, model.ShortName + ".123");

                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "LicenseAdmin");
                }
                else
                {
                    string er = "";
                    foreach (var item in result.Errors)
                    {
                        er += ": " + item;
                    }
                    ModelState.AddModelError("", "Error: " + er);
                    return View(model);
                }


                
                model.PublicKey = Guid.NewGuid();
                model.DateAdded = DateTime.Now;


                _appIdentity.Add(model);
                _appIdentity.Save(User.Identity.Name, Request.UserHostAddress);


                try
                {
                    _portalToCategory.AddPortalToDivisions(model.Id,(List<int>) model.CategoryId);
                    ViewBag.Message = "New License added to system successfully.";
                    ViewBag.MsgType = "pass";
                    return RedirectToAction("Index");
                }
                catch
                {
                    //drop every added field
                    var JustAdded= _appIdentity.FindBy(m => m.Id == model.Id).FirstOrDefault();
                    _appIdentity.Delete(JustAdded);
                }

           
            }

            ViewBag.Message = "Please Complete all the mandatory fields";
            ViewBag.MsgType = "fail";
            return View(model);

        }


        //[HttpPost]
        //[Authorize(Roles = "Admin,ITAdmin,LicenseAdmin")]
        //public ActionResult AddLicense(AppIdentity model)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        model.DateAdded = DateTime.Now;


        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };

        //        var result = UserManager.Create(user, model.ShortName + ".123");
        //        if (result.Succeeded)
        //        {
        //            UserManager.AddToRole(user.Id, "LicenseAdmin");

        //        }
        //        else
        //        {
        //            string er = "";
        //            foreach (var item in result.Errors)
        //            {
        //                er += ": " + item;
        //            }
        //            ModelState.AddModelError("", "Error: " + er);
        //            return View(model);
        //        }


        //        //_licenseRep.Add(model);
        //        //_licenseRep.Save(User.Identity.Name, Request.UserHostAddress);

        //        //var app = new AppIdentity();
        //        //app.AppId = PaymentRef.RefrenceCode();
        //        //app.
        //        //app.IsActive = false;
        //        //app.LinceseId = model.Id;
        //        //app.Name = model.LicenseShortName;
        //        //app.Url = model.Url;
        //        model.PublicKey = Guid.NewGuid();


        //        _appIdentity.Add(model);
        //        _appIdentity.Save(User.Identity.Name, Request.UserHostAddress);

        //        foreach (var eachId in model.CategoryId)
        //        {
        //            _portalToCategory.Add(new PortalToDivision { PortalId = model.Id, DivisionId = eachId });
        //            _portalToCategory.Save();
        //        }


        //        ViewBag.Message = "New License added to system successfully.";
        //        ViewBag.MsgType = "pass";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        ViewBag.Message = "Please Complete all the mandatory fields";
        //        ViewBag.MsgType = "fail";
        //        return View(model);
        //    }
        //}

        [Authorize(Roles = "Admin,ITAdmin,LicenseAdmin")]
        public ActionResult EditLicense(int id)
        {
            var license = _appIdentity.FindBy(a => a.Id == id).FirstOrDefault();

            license.CategoryId = _portalToCategory.FindBy(m => m.PortalId == id).Select(m => m.DivisionId).ToList();

            ViewBag.Categories = _categoryRepo.GetAll().ToList();

            //if(license != null)
            //    license.AppIdentity = _appIdentity.FindBy(a => a.LinceseId == license.Id).FirstOrDefault();



            return View(license);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ITAdmin,LicenseAdmin")]
        public ActionResult EditLicense(AppIdentity model, string bpendpoint, string active)
        {
            if (ModelState.IsValid)
            {
                // var app= 
                //_licenseRep.Edit(model);
                //_licenseRep.Save(User.Identity.Name, Request.UserHostAddress);

                var appId = _appIdentity.FindBy(a => a.Id == model.Id).FirstOrDefault();
                appId.ShortName = model.ShortName;
                appId.LicenseName = model.LicenseName;
                appId.BankPaymentEndPoint = model.BankPaymentEndPoint;
                appId.BaseUrl = model.BaseUrl;
                appId.Description = model.Description;
                appId.LoginRedirect = model.LoginRedirect;
                appId.PermitLink = model.PermitLink;
                appId.ReceiptCode = model.ReceiptCode;
                appId.BankPaymentEndPoint = model.BankPaymentEndPoint;// bpendpoint;
                appId.IsActive = !string.IsNullOrEmpty(active) ? true : false;
                appId.LoginByPass = model.LoginByPass;
                _appIdentity.Edit(appId);
                _appIdentity.Save(User.Identity.Name, Request.UserHostAddress);

                _portalToCategory.DeletePortal(appId.Id);

                _portalToCategory.AddPortalToDivisions(appId.Id, (List<int>)model.CategoryId);

              

                ViewBag.Message = model.LicenseName + " successfully updated on the system.";
                ViewBag.MsgType = "pass";
                return RedirectToAction("Index");
            }

            var categories = 
            ViewBag.Categories = _categoryRepo.GetAll().ToList(); 
            ViewBag.Message = "Please Complete all the mandatory fields";
            ViewBag.MsgType = "fail";
            return View(model);

        }

        public ActionResult ViewLicense(int id)
        {
            var userEmail = (User.Identity.Name).ToLower();
            var lice = _appIdentity.FindBy(a => a.Id == id).FirstOrDefault();
            var myCoy = _compRep.FindBy(a => a.User_Id.ToLower() == userEmail).FirstOrDefault();
            var myApps = _appRep.FindBy(a => a.LicenseId == id && a.CompanyId == myCoy.Id).ToList();
            var model = new CompanyLicenseDashboard();
            model.License = lice;
            model.Applications = myApps;
            model.Company = myCoy;
            model.Licenses = _permiTRep.FindBy(a => a.Company_Id == myCoy.Id).ToList();
            return View(model);
        }

        public int DocCount()
        {
            var docs = _appIdentity.FindBy(a => a.IsActive).ToList();
            return docs.Count();
        }

        #region EXPIRING / EXPIRED LICENSES

        public ActionResult Expiring(string startDate, string endDate, int? license, string category, string filterby, int? filterparam)
        {
            if (TempData["alert"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alert"];
            }
            //bool dateFilter = false;
            //DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            //DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            

            

            int p = filterparam.GetValueOrDefault(0);
            ViewBag.param = p;
            ViewBag.by = filterby;

            BranchFilterModel filter = new BranchFilterModel();
            filter.States = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            filter.Zones = _vZoneRep.GetAll().ToList();
            filter.Branches = _vBranchRep.GetAll().ToList();
            ViewBag.Filter = filter;

            var mm = "Showing result for ";
            if (!string.IsNullOrEmpty(filterby))
            {
                switch (filterby.ToLower())
                {
                    case "zn":
                        var z = filter.Zones.Where(a => a.Id == p).FirstOrDefault();
                        mm += "Expiring Licenses/Permits in " + z.Name;
                        break;
                    case "fd":
                        var f = filter.Branches.Where(a => a.Id == p).FirstOrDefault();
                        mm += "Expiring Licenses/Permits in " + f.Name;
                        break;
                    default:
                        mm += "All Expiring Licenses/Permits";
                        break;
                }
            }
            else
                mm += "All Expiring Licenses/Permits";

            ViewBag.ResultTitle = mm;

            var _license = new List<AppIdentity> { new AppIdentity { Id = 0, ShortName = "All Licenses" } };
            _license.AddRange(_appIdentity.FindBy(a => a.IsActive && (!a.OfficeUse.HasValue || !a.OfficeUse.Value)).ToList());
            ViewBag.licenses = new SelectList(_license, "Id", "ShortName", license);
            ViewBag.license = license == null ? 0 : license;
            var categories = new List<PermitCategory> { new PermitCategory { AppIdentityId = 0, Id = 0, Name = "All Categories" } };
            if (license != null && license > 0)
            {
                categories.AddRange(_permitCatRep.FindBy(a => a.AppIdentityId > 0 && a.AppIdentityId == license.Value).ToList());
            }
            else
            {
                categories.AddRange(_permitCatRep.FindBy(a => a.AppIdentityId > 0).ToList());
            }
            ViewBag.categories = new SelectList(categories, "Name", "Name", category);
            ViewBag.category = category;
            
            return View();
        }

        public ActionResult AjaxifyExpiring(JQueryDataTableParamModel param, string startDate, string endDate, int license, string category, string filterby, int? filterparam)
        {
            //DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            //DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            
            var exps = _vExpLicenseRep.GetAll();
            IEnumerable<vExpiringLicense> allPermits;
            if (license == 0)
            {
                if (string.IsNullOrEmpty(category))
                {
                    allPermits = exps;
                }
                else
                {
                    allPermits = exps.Where(a => a.CategoryName.ToLower() == category.ToLower());
                }
            }
            else
            {
                if (string.IsNullOrEmpty(category))
                {
                    allPermits = exps.Where(a => a.LicenseId == license);
                }
                else
                {
                    allPermits = exps.Where(a => a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower());
                }
            }

            if (!string.IsNullOrEmpty(filterby))
            {
                switch (filterby.ToLower())
                {
                    case "zn":
                        {
                            var zn = _vZoneRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                            if (zn != null)
                            {
                                var fds = _vZoneStateRep.FindBy(a => a.ZoneId == zn.Id).ToList();
                                switch (fds.Count())
                                {
                                    case 1:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName);
                                        break;
                                    case 2:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower());
                                        break;
                                    case 3:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower());
                                        break;
                                    case 4:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower());
                                        break;
                                    case 5:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower());
                                        break;
                                    case 6:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower());
                                        break;
                                    case 7:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower());
                                        break;
                                    case 8:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower());
                                        break;
                                    case 9:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower());
                                        break;
                                    case 10:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower());
                                        break;
                                    case 11:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower());
                                        break;
                                    case 12:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower());
                                        break;
                                    case 13:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower() || a.StateName.ToLower() == fds[12].StateName.ToLower());
                                        break;
                                    case 14:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower() || a.StateName.ToLower() == fds[12].StateName.ToLower() || a.StateName.ToLower() == fds[13].StateName.ToLower());
                                        break;
                                    default:
                                        allPermits = null;
                                        break;
                                }
                            }
                            else
                            {
                                allPermits = null;
                            }
                            break;
                        }
                    case "fd":
                        {
                            var brch = _vBranchRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                            if (brch != null)
                            {
                                allPermits = allPermits.Where(a => a.StateName.ToLower() == brch.StateName.ToLower());
                            }
                            else
                            {
                                allPermits = null;
                            }
                            break;
                        }
                    case "st":
                        {
                            var st = _stateRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                            allPermits = allPermits.Where(a => a.StateName.ToLower() == st.Name.ToLower());
                            break;
                        }
                    default:
                        break;
                }
            }

            IEnumerable<vExpiringLicense> filteredPermit;
            var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<vExpiringLicense, string> orderFunction = (c => sortColIndex == 1 ? c.Permit_No :
               sortColIndex == 2 ? c.OrderId : sortColIndex == 3 ? c.CompanyName
                 : sortColIndex == 4 ? c.contact_firstname : sortColIndex == 5 ? c.Date_Issued.ToString()
                : sortColIndex == 6 ? c.Date_Expire.ToString() : c.LicenseShortName.ToString());

            var sortDirection = Request["sSortDir_0"];
            List<vExpiringLicense> returnedPermit = new List<vExpiringLicense>();

            #region Select and Sort
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var ps = param.sSearch.ToLower();
                filteredPermit = allPermits.Where(a => a.Permit_No.ToLower().Contains(ps) || a.CompanyName.ToLower().Contains(ps) ||
                    a.LicenseShortName.ToLower().Contains(ps) || a.CategoryName.ToLower().Contains(ps) || a.Date_Issued.ToString().ToLower().Contains(ps) ||
                    a.Date_Expire.ToString().ToLower().Contains(ps) || a.LicenseShortName.ToLower().Contains(ps));

                if (sortDirection.ToLower() == "asc")
                {
                    if(sortColIndex == 5)
                        filteredPermit = filteredPermit.OrderBy(a => a.Date_Issued);
                    else if (sortColIndex == 6)
                        filteredPermit = filteredPermit.OrderBy(a => a.Date_Expire);
                    else
                        filteredPermit = filteredPermit.OrderBy(orderFunction);
                }
                else
                {
                    if (sortColIndex == 5)
                        filteredPermit = filteredPermit.OrderByDescending(a => a.Date_Issued);
                    else if (sortColIndex == 6)
                        filteredPermit = filteredPermit.OrderByDescending(a => a.Date_Expire);
                    else
                        filteredPermit = filteredPermit.OrderByDescending(orderFunction);
                }
            }
            else
            {
                filteredPermit = allPermits;
                if (sortDirection.ToLower() == "asc")
                {
                    if (sortColIndex == 5)
                        filteredPermit = filteredPermit.OrderBy(a => a.Date_Issued);
                    else if (sortColIndex == 6)
                        filteredPermit = filteredPermit.OrderBy(a => a.Date_Expire);
                    else
                        filteredPermit = filteredPermit.OrderBy(orderFunction);
                }
                else
                {
                    if (sortColIndex == 5)
                        filteredPermit = filteredPermit.OrderByDescending(a => a.Date_Issued);
                    else if (sortColIndex == 6)
                        filteredPermit = filteredPermit.OrderByDescending(a => a.Date_Expire);
                    else
                        filteredPermit = filteredPermit.OrderByDescending(orderFunction);
                }
            }
            #endregion

            returnedPermit = filteredPermit.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            int count = 0;
            var result = from c in returnedPermit
                         select new[] {
                             (++count).ToString(),
                             c.Permit_No,
                             c.OrderId,
                             c.CompanyName,
                             c.contact_firstname,
                             c.Date_Issued.ToShortDateString(),
                             c.Date_Expire.ToShortDateString(),
                             c.LicenseShortName,
                             c.NotificationCounter.GetValueOrDefault(0).ToString(),
                             c.Id.ToString()
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allPermits.Count(),
                iTotalDisplayRecords = filteredPermit.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Expired(string startDate, string endDate, int? license, string category, string filterby, int? filterparam)
        {
            if (TempData["alert"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alert"];
            }

            int p = filterparam.GetValueOrDefault(0);
            ViewBag.param = p;
            ViewBag.by = filterby;

            BranchFilterModel filter = new BranchFilterModel();
            filter.States = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            filter.Zones = _vZoneRep.GetAll().ToList();
            filter.Branches = _vBranchRep.GetAll().ToList();
            ViewBag.Filter = filter;

            var mm = "Showing result for ";
            if (!string.IsNullOrEmpty(filterby))
            {
                switch (filterby.ToLower())
                {
                    case "zn":
                        var z = filter.Zones.Where(a => a.Id == p).FirstOrDefault();
                        mm += "Expired License/Permits in " + z.Name;
                        break;
                    case "fd":
                        var f = filter.Branches.Where(a => a.Id == p).FirstOrDefault();
                        mm += "Expired License/Permits in " + f.Name;
                        break;
                    default:
                        mm += "All Expired License/Permits";
                        break;
                }
            }
            else
                mm += "All Expired License/Permits";

            ViewBag.ResultTitle = mm;


            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            //ViewBag.licenses = new SelectList(_appIdentity.GetAll().ToList(), "Id", "ShortName", license);
            //ViewBag.license = license == null ? 0 : license;
            //if (license != null && license > 0)
            //{
            //    ViewBag.categories = new SelectList(_permitCatRep.FindBy(a => a.AppIdentityId == license.Value).ToList(), "Name", "Name", category);
            //}
            //else
            //{
            //    ViewBag.categories = new SelectList(_permitCatRep.GetAll().ToList(), "Name", "Name");
            //}

            var _license = new List<AppIdentity> { new AppIdentity { Id = 0, ShortName = "All Licenses" } };
            _license.AddRange(_appIdentity.FindBy(a => a.IsActive && (!a.OfficeUse.HasValue || !a.OfficeUse.Value)).ToList());
            ViewBag.licenses = new SelectList(_license, "Id", "ShortName", license);
            ViewBag.license = license == null ? 0 : license;
            var categories = new List<PermitCategory> { new PermitCategory { AppIdentityId = 0, Id = 0, Name = "All Categories" } };
            if (license != null && license > 0)
            {
                categories.AddRange(_permitCatRep.FindBy(a => a.AppIdentityId > 0 && a.AppIdentityId == license.Value).ToList());
            }
            else
            {
                categories.AddRange(_permitCatRep.FindBy(a => a.AppIdentityId > 0).ToList());
            }
            ViewBag.categories = new SelectList(categories, "Name", "Name", category);
            ViewBag.category = category;
            //ViewBag.states = new SelectList(_stateRep.GetAll().ToList(), "Name", "Name");
            //ViewBag.location = location;
            ViewBag.SD = sd;
            ViewBag.ED = ed;

            List<vPermit> permits = new List<vPermit>();

            return View(permits);
        }

        public ActionResult AjaxifyExpired(JQueryDataTableParamModel param, string startDate, string endDate, int license, string category, string filterby, int? filterparam)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            IEnumerable<vPermit_with_amount> allPermits;

            if (license == 0)
            {
                if (string.IsNullOrEmpty(category))
                {
                    allPermits = _vPermitReportRep.FindBy(a => a.Date_Expire >= sd && a.Date_Expire <= ed);
                }
                else
                {
                    allPermits = _vPermitReportRep.FindBy(a => a.Date_Expire >= sd && a.Date_Expire <= ed && a.CategoryName.ToLower() == category.ToLower());
                }
            }
            else
            {
                if (string.IsNullOrEmpty(category))
                {
                    allPermits = _vPermitReportRep.FindBy(a => a.Date_Expire >= sd && a.Date_Expire <= ed && a.LicenseId == license);
                }
                else
                {
                    allPermits = _vPermitReportRep.FindBy(a => a.Date_Expire >= sd && a.Date_Expire <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower());
                }
            }

            switch (filterby.ToLower())
            {
                case "zn":
                    {
                        var zn = _vZoneRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                        if (zn != null)
                        {
                            var fds = _vZoneStateRep.FindBy(a => a.ZoneId == zn.Id).ToList();
                            switch (fds.Count())
                            {
                                case 1:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName);
                                    break;
                                case 2:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower());
                                    break;
                                case 3:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower());
                                    break;
                                case 4:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower());
                                    break;
                                case 5:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower());
                                    break;
                                case 6:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower());
                                    break;
                                case 7:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower());
                                    break;
                                case 8:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[7].StateName.ToLower());
                                    break;
                                case 9:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower());
                                    break;
                                case 10:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower());
                                    break;
                                case 11:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower());
                                    break;
                                case 12:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[11].StateName.ToLower());
                                    break;
                                case 13:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[11].StateName.ToLower() || a.StateName.ToLower() == fds[12].StateName.ToLower());
                                    break;
                                case 14:
                                    allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                    || a.StateName.ToLower() == fds[11].StateName.ToLower() || a.StateName.ToLower() == fds[12].StateName.ToLower() || a.StateName.ToLower() == fds[13].StateName.ToLower());
                                    break;
                                default:
                                    allPermits = null;
                                    break;
                            }
                        }
                        else
                        {
                            allPermits = null;
                        }
                        break;
                    }
                case "fd":
                    {
                        var brch = _vBranchRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                        if (brch != null)
                        {
                            allPermits = allPermits.Where(a => a.StateName.ToLower() == brch.StateName.ToLower());
                        }
                        else
                        {
                            allPermits = null;
                        }
                        break;
                    }
                case "st":
                    {
                        var st = _stateRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                        allPermits = allPermits.Where(a => a.StateName.ToLower() == st.Name.ToLower());
                        break;
                    }
                default:
                    break;
            }


            IEnumerable<vPermit_with_amount> filteredPermit;
            var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<vPermit_with_amount, string> orderFunction = (c => sortColIndex == 1 ? c.Permit_No :
               sortColIndex == 2 ? c.RRR : sortColIndex == 3 ? c.CompanyName
                 : sortColIndex == 4 ? c.LicenseShortName : sortColIndex == 5 ? c.CategoryName.Trim()
                : sortColIndex == 6 ? c.Date_Issued.ToString() : c.Date_Expire.ToString());

            var sortDirection = Request["sSortDir_0"];
            List<vPermit_with_amount> returnedPermit = new List<vPermit_with_amount>();

            #region Select and Sort
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var ps = param.sSearch.ToLower();
                filteredPermit = allPermits.Where(a => a.Permit_No.ToLower().Contains(ps) || a.CompanyName.ToLower().Contains(ps) ||
                    a.LicenseShortName.ToLower().Contains(ps) || a.CategoryName.ToLower().Contains(ps) || a.Date_Issued.ToString().ToLower().Contains(ps) ||
                    a.Date_Expire.ToString().ToLower().Contains(ps)).ToList();

                if (sortDirection.ToLower() == "asc")
                {
                    filteredPermit = filteredPermit.OrderBy(orderFunction);
                }
                else
                {
                    filteredPermit = filteredPermit.OrderByDescending(orderFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filteredPermit = allPermits.OrderBy(orderFunction);
                }
                else
                {
                    filteredPermit = allPermits.OrderByDescending(orderFunction);
                }
            }
            #endregion

            returnedPermit = filteredPermit.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            var result = from c in returnedPermit
                         select new[] {
                             c.Permit_No,                           // 0
                             c.RRR,                                 // 1
                             c.CompanyName,                         // 2
                             c.LicenseShortName,                    // 3
                             c.Date_Issued.ToShortDateString(),     // 4
                             c.Date_Expire.ToShortDateString(),     // 5
                             c.Fee.ToString("N2"),                  // 6
                             c.OtherFees.ToString("N2"),            // 7
                             c.Id.ToString(),                       // 8
                             c.Expired.ToString()                   // 9
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allPermits.Count(),
                iTotalDisplayRecords = filteredPermit.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Expired(string coyName)
        {
            ViewBag.licenses = new SelectList(_appIdentity.GetAll().ToList(), "Id", "ShortName");
            ViewBag.categories = new SelectList(_permitCatRep.GetAll().ToList(), "Name", "Name");
            ViewBag.states = new SelectList(_stateRep.GetAll().ToList(), "Name", "Name");
            ViewBag.CompanyName = coyName;
            ViewBag.ByCompany = true;

            BranchFilterModel filter = new BranchFilterModel();
            filter.States = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            filter.Zones = _vZoneRep.GetAll().ToList();
            filter.Branches = _vBranchRep.GetAll().ToList();
            ViewBag.Filter = filter;


            ViewBag.ResultTitle = $"Showing {coyName}'s Expired Licenses/Permits";

            List<vPermit> permits = new List<vPermit>();
            return View(permits);
        }

        public ActionResult AjaxifyExpiredByCompany(JQueryDataTableParamModel param, string coyName)
        {
            IEnumerable<vPermit> allPermits;
            var dt = UtilityHelper.CurrentTime;
            allPermits = _vPermitRep.FindBy(a => a.CompanyName.ToLower().Contains(coyName.ToLower()) && a.Date_Expire < dt);

            IEnumerable<vPermit> filteredPermit;
            var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<vPermit, string> orderFunction = (c => sortColIndex == 1 ? c.Permit_No :
               sortColIndex == 2 ? c.RRR : sortColIndex == 3 ? c.CompanyName
                 : sortColIndex == 4 ? c.LicenseShortName : sortColIndex == 5 ? c.CategoryName.Trim()
                : sortColIndex == 6 ? c.Date_Issued.ToString() : c.Date_Expire.ToString()); // sortColIndex == 6 ? : c.Invoice_open_date.ToString());

            var sortDirection = Request["sSortDir_0"];
            List<vPermit> returnedPermit = new List<vPermit>();

            #region Select and Sort
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var ps = param.sSearch.ToLower();//_vPermitRep.FindBy(
                filteredPermit = allPermits.Where(a => a.Permit_No.ToLower().Contains(ps) || a.CompanyName.ToLower().Contains(ps) ||
                    a.LicenseShortName.ToLower().Contains(ps) || a.CategoryName.ToLower().Contains(ps) || a.Date_Issued.ToString().ToLower().Contains(ps) ||
                    a.Date_Expire.ToString().ToLower().Contains(ps)).ToList();

                if (sortDirection.ToLower() == "asc")
                {
                    filteredPermit = filteredPermit.OrderBy(orderFunction);
                }
                else
                {
                    filteredPermit = filteredPermit.OrderByDescending(orderFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filteredPermit = allPermits.OrderBy(orderFunction);
                }
                else
                {
                    filteredPermit = allPermits.OrderByDescending(orderFunction);
                }
            }
            #endregion

            returnedPermit = filteredPermit.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            var result = from c in returnedPermit
                         select new[] {
                             c.Permit_No,
                             c.RRR,
                             c.CompanyName,
                             c.LicenseShortName,
                             c.CategoryName,
                             c.Date_Issued.ToShortDateString(),
                             c.Date_Expire.ToShortDateString(),
                             c.Id.ToString(),
                             c.Expired.ToString()
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allPermits.Count(),
                iTotalDisplayRecords = filteredPermit.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult Notify(string permit)
        {
            AlertModel alert = new AlertModel();

            if (string.IsNullOrEmpty(permit))
            {
                alert = new AlertModel { AlertType = "fail", Message = "Please specify Permit/License to send Expiry notification to.", Title = "Error" };
                //return $"Please specify Permit/License to send Expiry notification to.";
            }
            else
            {
                #region send Mail
                int pass = 0;
                if (permit.ToLower() == "all")
                {
                    var exps = _vExpLicenseRep.FindBy(a => a.NotificationCounter == null || a.NotificationCounter.Value < 2).ToList();
                    UtilityHelper.LogMessage($"{exps.Count()} Expiring found");
                    foreach (var item in exps)
                    {
                        if (DoNotify(item, User.Identity.Name, Request.UserHostAddress))
                        {
                            pass++;
                        }
                    }

                    alert = new AlertModel { AlertType = "pass", Message = $"Notification has been sent to {pass} of {exps.Count()}", Title = "Notification Sent" };
                }
                else
                {
                    var license = _vExpLicenseRep.FindBy(a => a.Permit_No.ToLower() == permit.ToLower()).FirstOrDefault();
                    if (DoNotify(license, User.Identity.Name, Request.UserHostAddress))
                    {
                        alert = new AlertModel { AlertType = "pass", Message = $"Expiry Notification has been sent to {license.CompanyEmail} successfully", Title = "Notification Sent" };
                    }
                    else
                        alert = new AlertModel { AlertType = "fail", Message = $"Unable to send Notification to the Client with the license/permit no {license.Permit_No}", Title = "Error" };
                }
                #endregion
            }

            TempData["alert"] = alert;
            return RedirectToAction("Expiring");
        }

        private bool DoNotify(vExpiringLicense license, string user, string ip, bool auto = true)
        {
            if (license != null)
            {
                try
                {
                    var body = "";

                    using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "GeneralFormat.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    var limit = auto ? 3 : 2;
                    var notice = _expNotifRep.FindBy(a => a.Permit_No.ToLower() == license.Permit_No.ToLower()).FirstOrDefault();
                    if (notice == null || notice.NotificationCounter < limit)
                    {
                        var typ = license.LicenseShortName.ToUpper() == "OGISP" ? "Permit" : "License";
                        int diff = Convert.ToInt16((license.Date_Expire - UtilityHelper.CurrentTime).TotalDays);
                        var msgDetails = string.Format($"<p>Dear {license.contact_firstname},</p><p>This is to notify you that your {license.LicenseShortName} {typ} is expiring in the next {diff} days. See details below</p>"
                            + $"<table><tr><td>{typ} Number:</td><td>{license.Permit_No}</td></tr><tr><td>Issue Date:</td><td>{license.Date_Issued}</td></tr><tr><td>Expiry Date:</td><td>{license.Date_Expire}</td></tr></table>"
                            + $"<p>Please login to our client area on the {license.LicenseShortName} portal to renew your {typ}.");

                        string subject = $"NUPRC \"{license.LicenseShortName}\" {typ} about to Expire";
                        var msgbody = string.Format(body, $"{license.Permit_No}", msgDetails);
                        MailHelper.SendEmail(license.CompanyEmail, subject, msgbody);//, new Attachment(new MemoryStream(binary), "Elps-Payment-Receipt.pdf"));

                        // Log notice
                        if (notice == null)
                            _expNotifRep.Add(new ExpiringNotification { DateLastNotified = UtilityHelper.CurrentTime, NotificationCounter = 1, Permit_No = license.Permit_No });
                        else
                        {
                            notice.NotificationCounter += 1;
                            notice.DateLastNotified = UtilityHelper.CurrentTime;
                            _expNotifRep.Edit(notice);
                        }
                        _expNotifRep.Save(user, ip);

                        MailReceipt mail = new MailReceipt() { EntityId = notice.Id, Email = license.CompanyEmail, Subject = subject, DateSent = UtilityHelper.CurrentTime };
                        _mailRecRep.Add(mail);
                        _mailRecRep.Save(user, ip);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    UtilityHelper.LogMessage("Error While Nofitying Client for License Expiry: " + license.Permit_No);
                }
            }
            return false;
        }

        public ActionResult Schedulers()
        {

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> StartExpiry()
        {
            PaymentCrawler.StartLicenseExpiryCrawler(User.Identity.Name, Request.UserHostAddress);
            //var tx = Convert.ToInt32(ConfigurationManager.AppSettings["LicenseExpiryTimer"]);
            //Execute_Thread(Exe_HandleExpiry, tx);

            return View("Schedulers");
        }

        [HttpPost]
        public ActionResult StartExpiryReport()
        {
            PaymentCrawler.StartLicenseExpiryReportCrawler(User.Identity.Name, Request.UserHostAddress);

            //var tx = Convert.ToInt32(ConfigurationManager.AppSettings["LicenseExpiryReportTimer"]);
            //Execute_Thread(Exe_HandleExpiryReport, tx);
            return View("Schedulers");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult HandleExpiry(string e, string i, int? page, int? size)
        {
            string msg = string.Empty;
            var resp = _HandleExpiry(e, i, out msg, page, size);
            if(resp == -1)
            {
                return Json(new { code = -1, msg = "User not Found! Operation not allowed" });
            }
            else if(resp == -2)
            {
                return Json(new { code = -1, msg = "User does not have permission to perform operation" });
            }
            else if(resp == 0)
            {
                return Json(new { code = 0, msg = msg });
            }
            else
            {
                return Json(new { code = -3, msg = "Error occured" });
            }
        }

        private int _HandleExpiry(string e, string i, out string msg, int? page, int? size)
        {
            //UtilityHelper.LogExpiryTrack("EXP running....... ");
            msg = "";
            var user = UserManager.FindByEmail(e);
            if (user == null)
            {
                //Return User not Found
                return -1;
            }

            var userRole = UserManager.IsInRole(user.Id, "Admin");
            if (!userRole)
            {
                //Return Not an Admin error
                return -2;
            }

            int pass30 = 0, exp30 = 0;
            int pass15 = 0, exp15 = 0;
            int pass5 = 0, exp5 = 0;
            var exps = new List<vExpiringLicense>();
            page = page.HasValue ? page.Value : 1;
            size = size.HasValue ? size.Value : 100;

            try
            {
                begin:
                //if (page != null && size != null)
                //{
                //    exps = _vExpLicenseRep.GetAll().Skip((page.Value - 1) * size.Value).Take(size.Value).ToList();
                //}
                //else
                //{
                //    exps = _vExpLicenseRep.GetAll().ToList();
                //}

                exps = _vExpLicenseRep.GetAll().Where(x => !x.Permit_No.Contains("NMDPRA") && !x.LicenseShortName.Equals("CVL")).OrderBy(a => a.Id).Skip((page.Value - 1) * size.Value).Take(size.Value).ToList();

                foreach (var item in exps)
                {
                    var expIn = Convert.ToInt16((item.Date_Expire - UtilityHelper.CurrentTime).TotalDays);

                    if (expIn > 15 && expIn <= 30)
                    {
                        exp30++;
                        if (item.NotificationCounter == null || item.NotificationCounter.Value == 0)
                        {
                            if (DoNotify(item, e, i))
                                pass30++;
                        }
                    }
                    else if (expIn > 5 && expIn <= 15)
                    {
                        exp15++;
                        if (item.NotificationCounter.HasValue && item.NotificationCounter.Value == 1)
                        {
                            if (DoNotify(item, e, i))
                                pass15++;
                        }
                    }
                    else if (expIn <= 5)
                    {
                        exp5++;
                        if (item.NotificationCounter.HasValue && item.NotificationCounter.Value <= 2)
                        {
                            if (DoNotify(item, e, i))
                                pass5++;
                        }
                    }

                }

                if (exps.Any())
                {
                    page++;
                    goto begin;
                }
                msg = $"Notification has been sent to companies: Exp in 30 days = {exp30}, Notif = {pass30}; Exp in 15 days = {exp15}, Notif = {pass15}; Exp in 5 days = {exp5}, Notif = {pass5}; " +
                    $" Total {exps.Count()} ==> Pages: {page}; Page Size: {size}";
                if (pass30 > 0 || pass15 > 0 || pass5 > 0){
                    var rgaEmail = ConfigurationManager.AppSettings["LicenseExpiryReportEmail"];
                    // ---> MailHelper.SendEmail(rgaEmail, "Auto Notification Crawler", $"{msg}");
                }

                UtilityHelper.LogExpiryTrack("EXP Notification: " + msg);
                return 0;
            }
            catch (Exception ex)
            {
                var mm = ex.InnerException == null ? ex.Message : (ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message);
                UtilityHelper.LogExpiryTrack("EXP Notification ERROR: " + mm);

                return -3;
            }
        }

        //private void Exe_HandleExpiry()
        //{
        //    string mm = string.Empty;
        //    _HandleExpiry("admin@example.com", ":1", out mm, 0, 100);
        //}
        //private void Exe_HandleExpiryReport()
        //{
        //    string mm = string.Empty;
        //    _HandleExpiry("admin@example.com", ":1", out mm, 0, 100);
        //}
        //private void Exe_HandlePaymentTracker()
        //{
        //    string mm = string.Empty;
        //    _HandleExpiry("admin@example.com", ":1", out mm, 0, 100);
        //}

        [HttpPost]
        [AllowAnonymous]
        public ActionResult HandleExpiryReport(string e, string i, bool doNow = false)
        {
            //UtilityHelper.LogExpiryTrack("EXP running....... ");
            var thisMt = DateTime.Now.Month;
            var thisYr = DateTime.Now.Year;
            var day1 = DateTime.Parse($"{thisMt}/1/{thisYr}").Date.AddHours(0).AddMinutes(0).AddSeconds(1);
            var dayLast = GetLastmonthDay(day1, thisMt).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            //var lastDay = day1.AddMonths(1).AddDays(-1);
            int hr = Convert.ToInt32(ConfigurationManager.AppSettings["LicenseExpiryReportHour"]);
            if (doNow || (DateTime.Now.Date == dayLast.Date && DateTime.Now.Hour == hr))
            {
                var user = UserManager.FindByEmail(e);
                if (user == null)
                {
                    //Return User not Found
                    UtilityHelper.LogExpiryTrack("EXP Report: User not Found! Operation not allowed");
                    return Json(new { code = -1, msg = "User not Found! Operation not allowed" });
                }

                var userRole = UserManager.IsInRole(user.Id, "Admin");
                if (!userRole)
                {
                    //Return Not an Admin error
                    UtilityHelper.LogExpiryTrack("EXP Report: User does not have permission to perform operation");
                    return Json(new { code = -1, msg = "User does not have permission to perform operation" });
                }
                int pass = 0;
                var exps = new List<vExpiringLicense>();

                var mth = UtilityHelper.GetMonthName(thisMt);
                var expThisMonth = _vPermitRep.FindBy(a => a.Date_Expire >= day1 && a.Date_Expire <= dayLast).ToList();
                string body = "";
                using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ExpReportFormat.txt"))
                {
                    body = sr.ReadToEnd();
                }
                var groups = expThisMonth.GroupBy(a => a.LicenseShortName);
                var msgDetails = string.Empty;

                foreach (var item in groups)
                {
                    msgDetails += $"<tr><td style=\"text-align: right; padding: 0 10px;\">{item.Key}:</td>" +
                        $"<td style=\"text-align: left; padding: 0 10px; \">{item.Count()}</td></tr>";
                }
                var rgaEmail = ConfigurationManager.AppSettings["LicenseExpiryReportEmail"];
                string subject = $"NUPRC License/Permit Expired in {mth}";
                var msgbody = string.Format(body, $"{mth} Report", expThisMonth.Count(), msgDetails, thisYr);
               // MailHelper.SendEmail(rgaEmail, subject, msgbody);//, new Attachment(new MemoryStream(binary), "Elps-Payment-Receipt.pdf"));

                var mm = $"Notification has been sent to {rgaEmail} of {expThisMonth.Count()} expired this month";
                UtilityHelper.LogExpiryTrack("EXP Report: " + mm);
                return Json(new { code = 0, msg = mm });
            }
            else
            {
                UtilityHelper.LogExpiryTrack("EXP Report: Cannot run this command now.");
                return Json(new { code = -1, msg = $"Cannot run this command now." });
            }
        }


        private DateTime GetLastmonthDay(DateTime day1, int mth)
        {
            var last = day1.AddMonths(1).AddDays(-1);
            return last;
        }

        #endregion

        #region New Thread
        public async Task Execute_Thread(Action action, int timeoutInMinutes)
        {
            await Task.Delay(TimeSpan.FromMinutes(timeoutInMinutes));
            action();
        }

        //public async Task Execute_Expiry_Report(Action action, int timeoutInMilliseconds)
        //{
            
        //    await Task.Delay(TimeSpan.FromMinutes(tx));
        //    action();
        //}
        #endregion

        #region Mail Receipt
        [AllowAnonymous]
        public ActionResult ProcessMail(string response)
        {
            var models = JsonConvert.DeserializeObject<SendGridResponseModel>(response);
            foreach (var item in models.Responses)
            {
                if(item.Event == MailEvent.Opened)
                {

                }
                else if(item.Event == MailEvent.Delivered)
                {

                }
            }

            return Json(new { obj = models }, JsonRequestBehavior.AllowGet);
        }


        #endregion
    }

    public static class MailEvent
    {
        public static string Opened { get { return "open"; } }
        public static string Delivered { get { return "delivered"; } }
    }

    public class SendGridResponseModel
    {
        public SendGridResponse[] Responses { get; set; }
    }

    public class SendGridResponse
    {
        public string Email { get; set; }
        public int Timestamp { get; set; }
        public string Smtpid { get; set; }
        public string Event { get; set; }
        public string Category { get; set; }
        public string Sg_event_id { get; set; }
        public string Sg_message_id { get; set; }
        public string Response { get; set; }
        public string Useragent { get; set; }
        public string Ip { get; set; }
    }

}
