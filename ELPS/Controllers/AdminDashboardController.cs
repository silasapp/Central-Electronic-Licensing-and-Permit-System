using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using ELPS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    [Authorize]
    public class AdminDashboardController : Controller
    {
        #region Repositories
        IStateRepository _stateRep;
        IvCompanyRepository _vCompRep;
        ICompanyNameHistoryRepository _compHistRep;
        IAppIdentityRepository _appIdRep;
        ILicenseRepository _licenseRep;
        IPermitRepository _permitRep;
        ICompanyRepository _coyRep;
        ICompany_DirectorRepository _coyDirRep;
        IPayment_TransactionRepository _payTransRep;
        IvCompanyFileRepository _vCoyFileRep;
        IvCompanyDirectorRepository _vCoyDirRep;
        IMessageRepository _msgRep;
        IApplicationRepository _appRep;
        IvZoneRepository _vZoneRep;
        IvZoneStateRepository _vZoneStateRep;
        IvBranchRepository _vBranchRep;

        CompanyHelper coyHelper;
        #endregion

        private ApplicationUserManager _userManager;
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

        public AdminDashboardController()
        {
        }

        public AdminDashboardController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public AdminDashboardController(ILicenseRepository license, IPermitRepository permit, ICompanyRepository coy, IMessageRepository msg,
            IvCompanyFileRepository vCoyFile, ICompany_DirectorRepository coyDir, IvCompanyDirectorRepository vCoyDirRep, IvCompanyRepository vCompRep,
            IPayment_TransactionRepository payTrans, IAppIdentityRepository appId, IApplicationRepository apprep, ICompanyNameHistoryRepository compHistRep,
            IStateRepository stateRep, IvZoneRepository vZoneRep, IvBranchRepository vBranchRep, IvZoneStateRepository vZoneStateRep)
        {
            _vZoneStateRep = vZoneStateRep;
            _vBranchRep = vBranchRep;
            _vZoneRep = vZoneRep;
            _stateRep = stateRep;
            _vCompRep = vCompRep;
            _compHistRep = compHistRep;
            _appRep = apprep;
            _appIdRep = appId;
            _payTransRep = payTrans;
            _vCoyDirRep = vCoyDirRep;
            _coyDirRep = coyDir;
            _msgRep = msg;
            _vCoyFileRep = vCoyFile;
            _licenseRep = license;
            _permitRep = permit;
            _coyRep = coy;

            coyHelper = new CompanyHelper(coy, apprep);
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            var userRoles = UserManager.GetRoles(User.Identity.GetUserId());
            if (userRoles.Contains("Admin") || User.IsInRole("ITAdmin") || User.IsInRole("ITAdmin") ||
                    User.IsInRole("Account") || User.IsInRole("ManagerObserver") || User.IsInRole("Support"))
            {
                //This is For Admin
                var dashVM = new DashboardViewModel();
                var licenses = _appIdRep.FindBy(a => a.IsActive).ToList();

                foreach (var license in licenses)
                {
                    //var xx = license.BaseUrl;
                    license.MyPermits = _permitRep.FindBy(a => a.LicenseId == license.Id).Count();
                    license.LicensesInProcessing = _appRep.FindBy(a => a.LicenseId == license.Id && a.Status.ToLower() != "payment pending" && a.Status.ToLower() != "paymentpending" && a.Status.ToLower() != "payment completed" && a.Status.ToLower() != "approved").Count();// 0; // coyHelper.AppsInProcessing(0);
                }
                dashVM.Licenses = licenses;

                //get all the applications

                //var apps = _appRep.FindBy(a => a.Status != "PaymentPending").ToList();
                //ViewBag.paidApp = apps.Count;
                //ViewBag.subbmited = apps.Count(a => a.Status != "PaymentCompleted");
                //dashVM.Company = myCoy;
                //dashVM.Documents = _vCoyFileRep.FindBy(a => a.Id == myCoy.Id).Take(5).ToList();
                //dashVM.Messages = _msgRep.FindBy(a => a.Company_Id == myCoy.Id).OrderByDescending(a => a.Date).Take(10).ToList();
                return View(dashVM);
            }
            else if (User.IsInRole("LicenseAdmin"))
            {
                var dashVM = new DashboardViewModel();
                var em = User.Identity.Name;
                var license = _appIdRep.FindBy(a => a.IsActive && a.Email == em).FirstOrDefault();
                var lic = new List<AppIdentity>();
                license.MyPermits = _permitRep.FindBy(a => a.LicenseId == license.Id).Count();
                license.LicensesInProcessing = _appRep.FindBy(a => a.LicenseId == license.Id && a.Status != "PaymentPending" && a.Status != "Approved").Count();// 0; // coyHelper.AppsInProcessing(0);
                lic.Add(license);
                dashVM.Licenses = lic;
                return View(dashVM);
            }
            else if (userRoles.Contains("Staff"))
            {
                return RedirectToAction("StaffDashboard");
            }
            else
            {
                return View("NotAllowed");
            }
        }

        [Authorize(Roles = "Admin, Account, Support, ITAdmin")]
        public ActionResult Companies(string filterby, int? filterparam)
        {
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
                        mm += "Companies in " + z.Name;
                        break;
                    case "fd":
                        var f = filter.Branches.Where(a => a.Id == p).FirstOrDefault();
                        mm += "Companies in " + f.Name;
                        break;
                    default:
                        mm += "All Compaies";
                        break;
                }
            }
            else
                mm += "All Compaies";

            ViewBag.ResultTitle = mm;
            var companies = new List<vCompany>();// _coyRep.GetAll().ToList();
            return View(companies);
        }

        [Authorize(Roles = "Admin, Account, Support, ITAdmin")]
        public ActionResult LazyLoadCompaniesQuery(JQueryDataTableParamModel param, string filterby, int filterparam)
        {
            IEnumerable<vCompany> allCompanies;
            var srch = !string.IsNullOrEmpty(param.sSearch) ? param.sSearch.ToLower() : "";
            if (string.IsNullOrEmpty(srch))
            {
                allCompanies = _vCompRep.GetAll();
            }
            else
            {
                allCompanies = _vCompRep.FindBy(C => C.Name.ToLower().Contains(srch) || C.Business_Type.ToLower().Contains(srch)
                || C.User_Id.ToLower().Contains(srch) || C.Contact_FirstName.ToLower().Contains(srch) ||
                C.Contact_Phone.ToLower().Contains(srch) || C.StateName.ToLower().Contains(srch));
            }

            switch (filterby.ToLower())
            {
                case "zn":
                    {
                        var zn = _vZoneRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                        if(zn != null)
                        {
                            var fds = _vZoneStateRep.FindBy(a => a.ZoneId == zn.Id).ToList();
                            switch (fds.Count())
                            {
                                case 1:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId);
                                    break;
                                case 2:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId);
                                    break;
                                case 3:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId);
                                    break;
                                case 4:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId 
                                    || a.StateId == fds[3].StateId);
                                    break;
                                case 5:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId 
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId);
                                    break;
                                case 6:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId 
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId);
                                    break;
                                case 7:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId 
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId || a.StateId == fds[6].StateId);
                                    break;
                                case 8:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId 
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId || a.StateId == fds[6].StateId 
                                    || a.StateId == fds[7].StateId);
                                    break;
                                case 9:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId 
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId || a.StateId == fds[6].StateId
                                    || a.StateId == fds[7].StateId || a.StateId == fds[8].StateId);
                                    break;
                                case 10:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId 
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId || a.StateId == fds[6].StateId
                                    || a.StateId == fds[7].StateId || a.StateId == fds[8].StateId || a.StateId == fds[9].StateId);
                                    break;
                                case 11:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId || a.StateId == fds[6].StateId
                                    || a.StateId == fds[7].StateId || a.StateId == fds[8].StateId || a.StateId == fds[9].StateId || a.StateId == fds[10].StateId);
                                    break;
                                case 12:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId || a.StateId == fds[6].StateId
                                    || a.StateId == fds[7].StateId || a.StateId == fds[8].StateId || a.StateId == fds[9].StateId || a.StateId == fds[10].StateId
                                    || a.StateId == fds[11].StateId);
                                    break;
                                case 13:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId || a.StateId == fds[6].StateId
                                    || a.StateId == fds[7].StateId || a.StateId == fds[8].StateId || a.StateId == fds[9].StateId || a.StateId == fds[10].StateId
                                    || a.StateId == fds[11].StateId || a.StateId == fds[12].StateId);
                                    break;
                                case 14:
                                    allCompanies = allCompanies.Where(a => a.StateId == fds[0].StateId || a.StateId == fds[1].StateId || a.StateId == fds[2].StateId
                                    || a.StateId == fds[3].StateId || a.StateId == fds[4].StateId || a.StateId == fds[5].StateId || a.StateId == fds[6].StateId
                                    || a.StateId == fds[7].StateId || a.StateId == fds[8].StateId || a.StateId == fds[9].StateId || a.StateId == fds[10].StateId 
                                    || a.StateId == fds[11].StateId || a.StateId == fds[12].StateId || a.StateId == fds[13].StateId);
                                    break;
                                default:
                                    allCompanies = null;
                                    break;
                            }
                        }
                        else
                        {
                            allCompanies = null;
                        }
                        break;
                    }
                case "fd":
                    {
                        var brch = _vBranchRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                        if(brch != null)
                        {
                            allCompanies = allCompanies.Where(a => a.StateId == brch.StateId);
                        }
                        else
                        {
                            allCompanies = null;
                        }
                        break;
                    }
                case "st":
                    {
                        allCompanies = allCompanies.Where(a => a.StateId == filterparam);
                        break;
                    }
                default:
                    break;
            }

            IEnumerable<vCompany> filteredCompanies;
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            List<vCompany> displayedCompanies = new List<vCompany>();

            if (sortDirection == "asc")
            {
                //filteredCompanies = allCompanies.OrderBy(orderingFunction);
                switch (sortColumnIndex)
                {
                    case 1:
                        filteredCompanies = allCompanies.OrderBy(a => a.Name);
                        break;
                    case 2:
                        filteredCompanies = allCompanies.OrderBy(a => a.Business_Type);
                        break;
                    case 3:
                        filteredCompanies = allCompanies.OrderBy(a => a.User_Id);
                        break;
                    case 4:
                        filteredCompanies = allCompanies.OrderBy(a => a.Contact_FirstName);
                        break;
                    case 5:
                        filteredCompanies = allCompanies.OrderBy(a => a.Contact_Phone);
                        break;
                    case 6:
                        filteredCompanies = allCompanies.OrderBy(a => a.StateName);
                        break;
                    default:
                        filteredCompanies = allCompanies.OrderBy(a => a.Name);
                        break;
                }
            }
            else
            {
                //filteredCompanies = allCompanies.OrderByDescending(orderingFunction);
                switch (sortColumnIndex)
                {
                    case 1:
                        filteredCompanies = allCompanies.OrderByDescending(a => a.Name);
                        break;
                    case 2:
                        filteredCompanies = allCompanies.OrderByDescending(a => a.Business_Type);
                        break;
                    case 3:
                        filteredCompanies = allCompanies.OrderByDescending(a => a.User_Id);
                        break;
                    case 4:
                        filteredCompanies = allCompanies.OrderByDescending(a => a.Contact_FirstName);
                        break;
                    case 5:
                        filteredCompanies = allCompanies.OrderByDescending(a => a.Contact_Phone);
                        break;
                    case 6:
                        filteredCompanies = allCompanies.OrderByDescending(a => a.StateName);
                        break;
                    default:
                        filteredCompanies = allCompanies.OrderByDescending(a => a.Name);
                        break;
                }
            }

            if (param.iDisplayLength == -1)
            {
                displayedCompanies = filteredCompanies.ToList();
            }
            else
            {
                displayedCompanies = filteredCompanies.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            }
            var result = from c in displayedCompanies
                         select new[] { c.Name,c.Business_Type,
                          c.User_Id, c.Contact_FirstName,  c.Contact_Phone, c.StateName, Convert.ToString(c.Id) };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allCompanies.Count(),
                iTotalDisplayRecords = filteredCompanies.Count(),
                aaData = result
            },
            JsonRequestBehavior.AllowGet);

        }

        public ActionResult CompanyView(int id)
        {
            ViewBag.CompanyId = id;
            return View();
        }

        [Authorize(Roles = "Admin,ITAdmin, Support")]
        public ActionResult ChangeCompanyName(int id)
        {
            ViewBag.CompanyId = id;
            var comp = _coyRep.FindBy(a => a.Id == id).FirstOrDefault();
            if (comp != null)
            {
                ViewBag.bizType = new SelectList(BussinessType.GetBizType().Select(x => new KeyValuePair<string, string>(x, x)), "Key", "Value");
                return View(comp);
            }
            ViewBag.Error = "Company does not Exist";
            return View("Error");
        }

        [Authorize(Roles = "Admin,ITAdmin, Support"), HttpPost]
        public ActionResult ChangeCompanyName(Company model)
        {
            if (model != null)
            {
                var comp = _coyRep.FindBy(a => a.Id == model.Id).FirstOrDefault();
                if (comp != null)
                {
                    //record company change of name history 
                    if (comp.Name != model.Name)
                    {
                        var cn = new CompanyNameHistory();
                        cn.CompanyId = comp.Id;
                        cn.Date = DateTime.Now;
                        cn.NewName = "Company New Name: " + model.Name + (comp.User_Id != model.User_Id ? "New Login Email: " + model.User_Id : "");
                        cn.OldName = "Company Old Name: " + comp.Name + (comp.User_Id != model.User_Id ? "Old login email: " + comp.User_Id : "");
                        cn.EditedBy = User.Identity.Name;

                        _compHistRep.Add(cn);
                        _compHistRep.Save(User.Identity.Name, Request.UserHostAddress);


                        // and then change and save the Company Name
                        comp.Name = model.Name;
                    }

                    string oldEmail = comp.User_Id;
                    bool emailChanged = false;
                    if (!string.IsNullOrEmpty(model.User_Id) && model.User_Id.ToLower() != comp.User_Id.ToLower())
                    {
                        comp.User_Id = model.User_Id;
                        emailChanged = true;
                    }
                    comp.Business_Type = model.Business_Type;
                    comp.Contact_FirstName = model.Contact_FirstName;
                    comp.Contact_LastName = model.Contact_LastName;
                    comp.Contact_Phone = model.Contact_Phone;
                    comp.RC_Number = model.RC_Number;
                    comp.Tin_Number = model.Tin_Number;
                    comp.Year_Incorporated = model.Year_Incorporated;

                    _coyRep.Edit(comp);
                    _coyRep.Save(User.Identity.Name, Request.UserHostAddress);

                    if (emailChanged)
                    {
                        var cn = new CompanyNameHistory();
                        cn.CompanyId = comp.Id;
                        cn.Date = DateTime.Now;
                        cn.NewName = $"Company New Email: {model.User_Id}";
                        cn.OldName = "Company Old Email: " + oldEmail;
                        cn.EditedBy = User.Identity.Name;
                        _compHistRep.Add(cn);
                        _compHistRep.Save(User.Identity.Name, Request.UserHostAddress);

                        UtilityHelper.LogMessage($"Email changed... updating the User Account from {oldEmail} TO {model.User_Id}");
                        var user = UserManager.FindByEmail(oldEmail);
                        if (user != null)
                        {
                            //update user
                            user.Email = comp.User_Id;
                            user.UserName = comp.User_Id;
                            user.EmailConfirmed = false;

                            UserManager.Update(user);
                            var result = UserManager.AddPassword(user.Id, $"{user.Email}1");
                        }
                        else
                        {
                            //create new user
                            #region create new user
                            user = new ApplicationUser { UserName = model.User_Id, Email = model.User_Id, PhoneNumber = comp.Contact_Phone, PhoneNumberConfirmed = true };
                            var result = UserManager.Create(user, $"{model.User_Id}1");
                            if (result.Succeeded)
                            {
                                var x = UserManager.AddToRole(user.Id, "Company");
                            }
                            #endregion
                        }
                        
                        #region send Re-Activation Link
                        var code = UserManager.GenerateEmailConfirmationToken(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, appId = Guid.Empty.ToString(), code = code }, protocol: Request.Url.Scheme);
                        var body = "";
                        //Read template file from the App_Data folder
                        using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmEmailChange.txt"))
                        {
                            body = sr.ReadToEnd();
                        }
                        var msgBody = string.Format(body, model.Name, callbackUrl, "Confirm Your NUPRC ELPS Account");


                        //send activation link
                        //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = resp.userId, code = code, appId = ELPSAPIHelper.PublicKey }, protocol: Request.Url.Scheme);
                        //var callbackUrl = string.Format("{0}/Account/ConfirmEmail?userId={1}&code={2}&appId={3}", ConfigurationManager.AppSettings["myBaseUrl"], user.Id, code.Replace("+", "%2B"), app.PublicKey);
                        //var body = "";
                        //using (var sr = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath(@"\\App_Data\\Templates\") + "ComfirmEmailChange.txt"))
                        //{
                        //    body = sr.ReadToEnd();
                        //}
                        //var msgBody = string.Format(body, model.Name, callbackUrl, "Confirm Your DPR ELPS Account");
                        MailHelper.SendEmail(model.User_Id, "Confirm Your NUPRC ELPS Account", msgBody);

                        var msg = new Message();
                        msg.Company_Id = comp.Id;
                        msg.Content = msgBody;
                        msg.Date = UtilityHelper.CurrentTime;
                        msg.Read = 0;
                        msg.Subject = "Confirm Your NUPRC ELPS Account";
                        msg.Sender_Id = "Application";

                        _msgRep.Add(msg);
                        _msgRep.Save(model.User_Id, Request.UserHostAddress);
                        #endregion


                        //return Ok(new { responseCode = 1 });
                    }


                    return RedirectToAction("CompanyView", new { id = comp.Id });

                }
                ViewBag.Error = "Item does not Exist";
                return View("Error");

            }
            ViewBag.Error = "Model is Null";
            return View("Error");
        }
        public ActionResult CompanyDetail(int id)
        {
            var company = _coyRep.FindBy(a => a.Id == id).FirstOrDefault();
            return View(company);
        }

        public ActionResult CompanyDirector(int id)
        {
            var companyDir = _vCoyDirRep.FindBy(a => a.Company_Id == id).ToList();
            return View(companyDir);
        }

        public ActionResult StaffDashboard()
        {
            return View();
        }

        public ActionResult CompanyHistory(int id)
        {
            var ch = _compHistRep.FindBy(a => a.CompanyId == id).ToList();
            return View(ch);
        }
        public ActionResult PaymentTransactions()
        {
            var transactions = _payTransRep.GetAll().ToList();
            return View(transactions);
        }

        #region ACCOUNT

        #endregion

    }
}