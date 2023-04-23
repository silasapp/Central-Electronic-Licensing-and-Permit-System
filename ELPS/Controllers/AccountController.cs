using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using ELPS.Models;
using System.Collections.Generic;
using ELPS.Domain.Entities;
using ELPS.Domain.Abstract;
using ELPS.Helpers;
using System.IO;
using System.Transactions;
using System.Data.Entity;
using ELPS.Domain.ViewDTOs;
using ELPS.Domain.Helper;
using AutoMapper;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace ELPS.Controllers
{
    // [Authorize]
    public class AccountController : Controller
    {
        #region Repository
        ICompanyRepository _compRep;
        IMessageRepository _msgRep;
        ILicenseRepository _licenseRep;
        IAppIdentityRepository _appIDRep;
        ILockedOutUserRepository _lockOutRep;
        ExtApplicationHelper extAppHelper;
        IAspNetUserRepository _usrRep;
        IvLockedOutUserRepository _vLockOutRep;
        IAffiliateRepository _affRep;
        IDivisionRepo _PortalCat;
        IPortalToDivision _portalToCategory;
        IStaffRepository _staffRep;
        #endregion

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) //, UserManager<ApplicationUser> userManager2)
        {
            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("One");
            userManager.UserTokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider<ApplicationUser>(provider.Create("EmailConfirmation"));
            UserManager = userManager;

            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };

            //UserManager = userManager;
            SignInManager = signInManager;
        }

        public AccountController(ICompanyRepository coy, IMessageRepository msg, ILicenseRepository license, IAppIdentityRepository appIDRep, ILockedOutUserRepository lockOutRep,
            IAspNetUserRepository usrRep, IvLockedOutUserRepository vLockOutRep, IAffiliateRepository affRep, IDivisionRepo portalCat, IPortalToDivision portalToCategory, IStaffRepository staffRep)
        {
            _affRep = affRep;
            _portalToCategory = portalToCategory;
            _vLockOutRep = vLockOutRep;
            _usrRep = usrRep;
            _lockOutRep = lockOutRep;
            _compRep = coy;
            _msgRep = msg;
            _licenseRep = license;
            _PortalCat = portalCat;
            _appIDRep = appIDRep;
            _staffRep = staffRep;
            extAppHelper = new ExtApplicationHelper(coy, license, appIDRep);
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
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

        public ActionResult SideBar()
        {
            if (User.IsInRole("Admin") || User.IsInRole("ITAdmin"))
            {
                return View("_AdminSidebar");
            }
            else if (User.IsInRole("Account"))
            {
                return View("_AccountSidebar");
            }
            else if (User.IsInRole("ManagerObserver"))
            {
                return View("_manObserverSidebar");
            }
            else if (User.IsInRole("Support"))
            {
                return View("_SupportSidebar");
            }
            else if (User.IsInRole("LicenseAdmin"))
            {
                return View("_LicenseAdminSidebar");
            }
            else
            {
                return View("_StaffSidebar");
            }
        }


        [HttpGet]
        public ActionResult Login(string returnUrl, string appId)
        {

            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }

            //if the request is not aunthenticated
            if (!Request.IsAuthenticated)
            {
                List<Division> divisions = null;    //used to hold the division and their respective portals
                List<AppIdentity> allActivePortals = null;   //used to hold all the portals without repitition for the search
                AppIdentity selectedPortal = null;  //used to hold the selected portal if there is any

                //this method is responsible for grouping Portals
                GroupPortals(out allActivePortals, out selectedPortal, out divisions, appId);

                if (!String.IsNullOrEmpty(appId) && selectedPortal == null)
                {
                    ViewBag.Alert = new AlertModel
                    {
                        AlertType = "failure",
                        Message = "The requested Portal could not be found, you can however select from the available portals",
                        Title = "Portal Unavailable"
                    };
                }

                ViewBag.appId = appId;
                ViewBag.returnUrl = returnUrl;
                return View("LoginView", new AccountLoginDTO { Divisions = divisions, SelectedPortal = selectedPortal, ActivePortals = allActivePortals });
            }

            //handle when the request is authenticated from an external portal

            if (!string.IsNullOrEmpty(appId))
            {

                //Guid pk = Guid.Parse(appId);
                var app = _appIDRep.FindBy(m => m.PublicKey.ToString() == appId).FirstOrDefault();

                if (app != null)
                {
                    TempData["AppData"] = app;
                    //trigger this to enable redirect from portal
                    return RedirectToAction("ProcessAppData");
                }
                //this is needed
                return View("NotFound");
            }

            var userId = User.Identity.GetUserId() ;
            //redirect to the appropriate dashboard
            var url = ReturnBasedOnRole(userId);
            return RedirectToLocal(url);

        }

        //this is used for loading the loginPage faster once the page has been loaded to avoid the stress of grouping portals again
        [HttpGet]
        public ActionResult loginForm(string returnUrl, string appId)
        {
            ViewBag.returnUrl = returnUrl;
            ViewBag.appId = appId;
            return View("LoginForm");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(AccountLoginDTO model, string returnUrl, string appId)
        {
            //summary of Logic: All commented portion
            ViewBag.appId = appId;
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                TempData["ErrorList"] = new List<string> { "Please fill out the required field(s) correctly" };
                return PartialView("LoginForm", model);
            }

            if (UtilityHelper.IsDPRStaff(model.Email))
            {
                TempData["ErrorList"] = new List<string> { "Please login by clicking 'Staff Login' below" };
                return PartialView("LoginForm", model);
            }

            var usr = await UserManager.FindByEmailAsync(model.Email);

            if (usr != null)
            {
                AppIdentity app = null;

                // Check to make sure call is from our clients;
                if (!string.IsNullOrEmpty(appId))
                {
                    //Guid pk = Guid.Parse(appId);
                    if (Request.IsAuthenticated)
                    {

                    }
                 
                    app = _appIDRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                    if (app == null)
                    {
                        //this is needed since a redirect needs to be triggered with the correct Id
                        return HttpNotFound("Portal cannot be found");
                    }
                }

                //check if the email has been confirmed
                if (!await UserManager.IsEmailConfirmedAsync(usr.Id))
                {
                    //var Message = "Please check your mail to confirm your Account before you can Login";
                    return Json(new Response() { Result=Result.PageDefined.ToString(),
                        Message="MailNotification",
                        ReturnUrl=Url.Action("ResendMailConfimation", "Account",new {Email=usr.Email,appId=appId}) }
                    );
                }

                //check if the User have been locked out
                if (_lockOutRep.IsUserLockedOut(usr.Id))
                {
                    TempData["ErrorList"] = new List<string> { "Sorry but you have been locked out of NUPRC Services, Please contact the Support Center for for details" };
                    return PartialView("LoginForm");
                }

                //To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

                switch (result)
                {
                    case SignInStatus.Success:
                        MvcApplication.CurrentUser = new CurrentLog() { IsAuthenticated = true, Name = model.Email };

                        if (app != null)
                        {
                            TempData["AppData"] = app;
                            //var loggedInUser = User.Identity.Name;
                            //var hashStr = $"{app.PublicKey}.{loggedInUser}.{app.AppId}";
                            //var code = HashManager.GetHash(hashStr.ToUpper());

                            return Json(new Response() { 
                                Result=Result.Success.ToString(),
                                ReturnUrl=Url.Action("ProcessAppData","Account",new { appId=appId})
                            });
                        }
                        if (returnUrl == null)
                        {
                            var user = UserManager.FindByEmail(model.Email);
                            returnUrl = ReturnBasedOnRole(user.Id);
                           
                        }
                        return Json(new Response() { Result = Result.Success.ToString(), ReturnUrl = returnUrl });
                        

                    case SignInStatus.LockedOut:
                        TempData["ErrorList"] = new List<string> { "Sorry but you have been locked out of NUPRC Services, Please contact the Support Center for for details" };
                        return PartialView("LoginForm");

                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                    case SignInStatus.Failure:
                    default:
                        break;
                }
            }
            
            TempData["ErrorList"] = new List<string> {"Invalid Email/Passcode"};
            return PartialView("LoginForm",model);

        }

        [HttpGet]
        public ActionResult Register(string appId)
        {
  
            ViewBag.appId = appId;

            List<Division> divisions = null;    //used to hold the division and their respective portals
            List<AppIdentity> allActivePortals = null;   //used to hold all the portals without repitition
            AppIdentity selectedPortal = null;  //used to hold the selected portal if there is any


            //this handles the grouping of the portals
            GroupPortals(out allActivePortals, out selectedPortal, out divisions, appId);
            if (!String.IsNullOrEmpty(appId) && selectedPortal == null)
            {
                ViewBag.Alert = new AlertModel
                {
                    AlertType = "failure",
                    Message = "The requested Portal could not be found, you can however select from the available portals",
                    Title = "Portal Unavailable"
                };
            }
            ViewBag.businessType = BussinessType.GetBizType().ToList();
            return View("RegisterView", new AccountRegisterDTO { Divisions = divisions, SelectedPortal = selectedPortal, ActivePortals = allActivePortals });
        }

        //this is used for loading the registerPage faster once the page has been loaded to avoid the stress of grouping portals again
        [HttpGet]
        public ActionResult RegisterForm(string appId)
        {
            ViewBag.appId = appId;
            ViewBag.businessType = BussinessType.GetBizType().ToList();
            return View("RegisterForm");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(AccountRegisterDTO model, string appId)
        {
            //return Json(model);
            ViewBag.appId = appId;
            ViewBag.businessType = BussinessType.GetBizType().ToList();
            if (!ModelState.IsValid)
            {
                TempData["ErrorList"] = new List<string>() { "Please fill out the required field(s) correctly" };
                return View("RegisterForm", model);
            }


            AppIdentity app = null;

            //Check to make sure call is from our clients;
            if (!string.IsNullOrEmpty(appId))
            {
                //Guid pk = Guid.Parse(appId);
                app = _appIDRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                if (app == null)
                {
                    return HttpNotFound("Portal not recognised");
                }
            }

            using (var trans = new TransactionScope()) // TransactionScopeAsyncFlowOption.Enabled))
            {
                var user = new ApplicationUser();
                try
                {

                    List<string> outError = null;
                    Company existingCompany = null;

                    if (ProcessRegistration(model, appId, out outError, out existingCompany))
                    {
                        trans.Complete();

                        //ViewBag.Email = model.Email;
                        return Json(new Response() { 
                            Result=Result.Notification.ToString(),
                            ResultType=ResultType.Success.ToString(),
                            Title="Account Created",
                            Message="Your account has been created succesfully, kindly proceed to your registration mail for account Verification"
                        });
                    }

                    //a false process registration can only be due to an existing company or model generate error.
                    if (outError.Any())
                    {
                        var errorList = new List<string>();
                        foreach (var errors in outError)
                        {
                            if (errors.ToLower().Contains("password"))
                                ModelState.AddModelError("Password", errors);
                            else if (errors.ToLower().Contains("email"))
                                ModelState.AddModelError("Email", errors);
                            else
                                errorList.Add(errors);
                        }

                        if (!errorList.Any())
                        {
                            errorList.Add("Please fill out the required field(s) correctly");
                        }

                        TempData["ErrorList"] = errorList;
                        return View("RegisterForm", model);
                    }
                    //if there is no error in the outerror, it means false processing was trigered because companyExists
                    return Json(new Response() { 
                        Result = Result.PageDefined.ToString(),
                        Message = "CompanyExist" 
                    });


                }
                catch (Exception ex)
                {
                    //throw ex; for testing
                    trans.Dispose();
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    //return HttpNotFound();
                    throw ex;

                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterAffiliate(AccountRegisterDTO model, string appId)
        {
            ViewBag.businessType = BussinessType.GetBizType().ToList();
            ViewBag.Affilate = "Affilate";
            if (!ModelState.IsValid)
            {
                TempData["ErrorList"] = new List<string>() { "Please fill out the required fields(s) correctly" };
                return PartialView("RegisterForm", model);
            }


            try
            {
                //create an application user
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, PhoneNumberConfirmed = true };

                var result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    var x = UserManager.AddToRole(user.Id, "Affiliate");

                    var parentComp = _compRep.GetCompany(model.CompanyName, model.RegistrationNumber);

                    //Create the Affiliate in the company table
                    var comp = new Company()
                    {
                        User_Id = model.Email,
                        Name = model.CompanyName,
                        Business_Type = model.BusinessType,
                        Contact_Phone = model.PhoneNumber,
                        RC_Number = parentComp.RC_Number,
                        Tin_Number = parentComp.Tin_Number,
                        Year_Incorporated = parentComp.Year_Incorporated,
                        Nationality = parentComp.Nationality,
                        Date = UtilityHelper.CurrentTime,
                        ParentCompanyId = parentComp.Id
                    };

                    _compRep.Add(comp);
                    _compRep.Save(model.Email, Request.UserHostAddress);

                    //Create the Affiliate details in the affilliate table
                    var _code = (Guid.NewGuid()).ToString().Split('-')[0];
                    var cc = PaymentRef.getHash(comp.Id.ToString() + "_" + comp.User_Id + "_" + parentComp.Id.ToString() + "_" + _code, true);
                    var aff = new Affiliate() { ChildId = comp.Id, DateAdded = UtilityHelper.CurrentTime, ParentId = parentComp.Id, UniqueId = Guid.NewGuid(), Code = cc };

                    _affRep.Add(aff);
                    _affRep.Save(model.Email, Request.UserHostAddress);

                    //Send a mail to the affillate Company
                    var code = UserManager.GenerateEmailConfirmationToken(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, appId = appId, code = code }, protocol: Request.Url.Scheme);
                    SendMailToAffiliate(callbackUrl, model.Email, appId);

                    //send a mail to the Parent Company
                    var callbacklink = Url.Action("ConfirmAffiliate", "Account", new { code = aff.UniqueId, em = parentComp.User_Id }, protocol: Request.Url.Scheme);
                    SendMailToParentCompany(callbacklink, parentComp, comp, _code);

                    return Json(new Response()
                    {
                        Result = Result.Notification.ToString(),
                        ResultType = ResultType.Success.ToString(),
                        Title = "Affiliate Account Created",
                        Message = "Your account has been created succesfully, kindly proceed to your registration mail for account Verification"
                    });
                }

                //check if failure was caused by email clash
                var ExistingUser = UserManager.FindByEmail(model.Email);
                if (ExistingUser != null)
                {
                    TempData["ErrorList"] = new List<string>() { "Input Error, Please fill out the required input(s) correctly" };
                    ModelState.AddModelError("Email", "Email has already been taken");
                    return PartialView("RegisterForm", model);
                }

                TempData["ErrorList"] = result.Errors;
                return PartialView("RegisterForm",model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult ResendMailConfimation(string Email, string appId)
        {
            var user = UserManager.FindByName(Email);
            if (user == null || UserManager.IsEmailConfirmed(user.Id))
            {
                return Json(new Response
                {
                    Result = Result.Notification.ToString(),
                    Title = "Mail Notification",
                    Message = "A verification mail has been sent to your registered Mail: " + Email + ", kindly proceed to your mail to complete verification process",
                    ResultType = ResultType.Success.ToString()
                });
            }
            var model = _compRep.FindBy(m => m.User_Id == Email).FirstOrDefault();
            var code = UserManager.GenerateEmailConfirmationToken(user.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, appId = appId, code = code }, protocol: Request.Url.Scheme);

            //send this as a mail to the user
            var body = "";

            //Read template file from the App_Data folder
            using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
            {
                body = sr.ReadToEnd();
            }
            var msgBody = string.Format(body, model.Name, callbackUrl, "Confirm Your NUPRC ELPS Account");
            MailHelper.SendEmail(Email, "Confirm Your NUPRC ELPS Account", msgBody);
            return Json(new Response { Result = Result.Notification.ToString(),
                Title="Mail Notification",
                Message="A verification mail has been sent to your registered Mail: "+Email+", kindly proceed to your mail to complete verification process",
                ResultType=ResultType.Success.ToString() 
            });
        }


        [HttpGet]
        public ActionResult ForgotPassword(string appId)
        {
            ViewBag.appId = appId;
            if (!string.IsNullOrEmpty(appId))
            {
                var app = _appIDRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                if (app != null)
                {
                    ViewBag.ShortName = app.ShortName;
                    ViewBag.FullName = app.LicenseName;
                }
               
            }
            return View("ForgotPasswordView");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model, string appId)
        {
            if (ModelState.IsValid)
            {
                UtilityHelper.LogMessage("Forgot password by " + model.Email);
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    //return View("ForgotPasswordConfirmation");
                    TempData["alertModel"] = new AlertModel()
                    {
                        AlertType = "success",
                        Title = "Password Reset Confirmation",
                        Message = "A Password Reset Link has been sent to your mail. Please check your email to continue your password recovery process."
                    };
                    return RedirectToAction("Login", "Account", new { appId = appId });
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { Email = user.Email, code = code, appId = appId }, protocol: Request.Url.Scheme);

                var body = "";
                //Read template file from the App_Data folder
                using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ForgotPassword.txt"))
                {
                    body = sr.ReadToEnd();
                }
                var msgBody = string.Format(body, model.Email, callbackUrl);
                MailHelper.SendEmail(user.Email, "Password Reset Request", msgBody);
                TempData["alertModel"] = new AlertModel() { AlertType = "success",
                    Title = "Password Reset Confirmation", 
                    Message = "Reset Password Link has been sent to your Email. Please check your email to reset your password." 
                };
                return RedirectToAction("Login", "Account", new { appId = appId });
            }
            if (!string.IsNullOrEmpty(appId))
            {
                var app = _appIDRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                if (app != null)
                {
                    ViewBag.ShortName = app.ShortName;
                    ViewBag.FullName = app.LicenseName;
                }

            }
            // If we got this far, something failed, redisplay form
            ViewBag.appId = appId;
            TempData["ErrorList"] = new List<string>() { "Please fill out the required field(s) correctly" };
            return View("ForgotPasswordView",model);
        }

        [HttpGet]
        public ActionResult ResetPassword(string code, string appId, string Email)
        {
            ViewBag.Email = Email;
            ViewBag.appId = appId;

            var modelDTO = new ResetPasswordViewModel { Code = code, Email = Email };
            return code == null||Email==null ? View("NotFound") : View("ResetPasswordView",modelDTO);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model, string appId)
        {
            ViewBag.Email = model.Email;
            ViewBag.appId = appId;
            if (!ModelState.IsValid)
            {
                TempData["ErrorList"] = new List<string>() { "Please fill out the required field(s) correctly" };
                return View("ResetPasswordView", model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                TempData["alertModel"] = new AlertModel()
                {
                    AlertType = "success",
                    Title = "Password Reset",
                    Message = "Your password have been successfully updated, You can now login with your new password."
                };
                // Don't reveal that the user does not exist
                return RedirectToAction("Login", "Account", new { appId = appId });
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                TempData["alertModel"] = new AlertModel()
                {
                    AlertType = "success",
                    Title = "Password Reset",
                    Message = "Your password have been successfully updated, You can now login with your new password."
                };
                return RedirectToAction("Login", "Account", new { appId = appId });
            }
            TempData["ErrorList"] = result.Errors;
            return View("ResetPasswordView",model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProcessAppData(string appId)
        {
            AppIdentity app = null;
            if (TempData["AppData"] != null)
            {
                app = (AppIdentity)TempData["AppData"];
            }
            else if (!string.IsNullOrEmpty(appId))
            {
                //var _key = Guid.Parse(appId);
                app = _appIDRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
            }
            else
            {
                return View("Error");
            }
            var usr = TempData["StaffUsers"] != null ? TempData["StaffUsers"].ToString() :  User.Identity.Name;
            var hashStr = $"{app.PublicKey}.{usr}.{app.AppId}";
            var code = HashManager.GetHash(hashStr.ToUpper());

            var viewDTO = new AccountProcessDataDTO { Code = code, Email = usr, Url = app.LoginRedirect };

            return View(viewDTO);

            //var frm = "<form action='" + app.LoginRedirect + "' id='frmTest' method='post'>" +
            //    "<input type='hidden' name='email' value='" + usr + "' />" +
            //    "<input type='hidden' name='code' value='" + code + "' />" +
            //    "</form>" +
            //    "<script>document.getElementById('frmTest').submit();</script>";
            //return Content(frm);
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string userId, string appId, string code)
        {
            ViewBag.appId = appId;

            if (userId == null || code == null)
            {
                return View("NotFound");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
           

            if (result.Succeeded)
            {
                TempData["alertModel"] = new AlertModel() { 
                    AlertType = "success", 
                    Title = "Account Confirmed", 
                    Message = "Your account has been successfully verified. You can login now." 
                };
                return RedirectToAction("Login", "Account", new { appId = appId });
          
            }
            return View("Error");
        }

        [HttpGet]
        public ActionResult Error()
        {
            return View("Error");
        }

        [HttpGet]
        public ActionResult NotFound()
        {
            return View("NotFound");
        }

        public ActionResult DisplayEmail()
        {
            return View();
        }


 
        [HttpGet]
        public ActionResult Affiliate()
        {
            ViewBag.CoyName = "Test Company";
            return View();
        }

        public ActionResult ConfirmAffiliate(string code, string em)
        {
           
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(em))
            {
                var myCoy = _compRep.FindBy(a => a.User_Id.ToLower() == em.ToLower()).FirstOrDefault();
                if (myCoy != null)
                {
                    var unid = Guid.Parse(code);
                    var aff = _affRep.FindBy(a => a.UniqueId == unid).FirstOrDefault();
                    if (aff != null && aff.Approved == null && aff.ParentId == myCoy.Id)
                    {
                        ViewBag.Company = _compRep.FindBy(a => a.Id == aff.ChildId).FirstOrDefault();
                        return View(aff);
                    }
                }
            }

            return View("Error");
        }

        [HttpPost]
        public ActionResult ConfirmAffiliate(string code, string action, string scode)
        {
            try
            {
                var unid = Guid.Parse(code);
                var aff = _affRep.FindBy(a => a.UniqueId == unid).FirstOrDefault();

                if (aff != null && aff.Approved == null)
                {
                    var child = _compRep.FindBy(a => a.Id == aff.ChildId).FirstOrDefault();
                    var parent = _compRep.FindBy(a => a.Id == aff.ParentId).FirstOrDefault();

                    var cc = PaymentRef.getHash(child.Id.ToString() + "_" + child.User_Id + "_" + parent.Id.ToString() + "_" + scode.Trim(), true);
                    if (cc == aff.Code)
                    {
                        aff.DateConfirmed = UtilityHelper.CurrentTime;
                        if (action.ToLower() == "confirm")
                        {
                            aff.Approved = true;
                            ViewBag.Result = "done";
                        }
                        else
                        {
                            aff.Approved = false;
                            ViewBag.Result = "not done";
                            //Lock User out
                            var usr = UserManager.FindByEmail(child.User_Id);
                            var lo = _lockOutRep.FindBy(a => a.UserId == usr.Id).FirstOrDefault();
                            if (lo == null)
                            {
                                lo = new LockedOutUser()
                                {
                                    Reason = "[" + parent.Name + "] Decline the company as her Affiliate/Daughter company",
                                    Resolved = false,
                                    UserId = usr.Id
                                };

                                _lockOutRep.Add(lo);
                            }
                            else
                            {
                                lo.Reason = "[" + parent.Name + "] Decline the company as her Affiliate/Daughter company";
                                lo.Resolved = false;
                                _lockOutRep.Edit(lo);
                            }
                            _lockOutRep.Save(parent.User_Id, Request.UserHostAddress);
                        }
                        _affRep.Edit(aff);
                        _affRep.Save(parent.User_Id, Request.UserHostAddress);

                        return View();
                    }
                }
                throw new ArgumentException("");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Support, Support2")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "fail";
                return RedirectToAction("AdminForgotPassword", "Account");
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                TempData["Message"] = "pass";
            }
            else
            {
                //AddErrors(result);
                UtilityHelper.LogMessage(result.ToString());
                TempData["Message"] = "fail";
            }

            return RedirectToAction("AdminForgotPassword", "Account");
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ChangeEmail(string newemail, string email, string apiHash, CompanyChangeModel model)//(IEnumerable<HttpPostedFileBase> files)
        {
            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { responseCode = 0, message = "App UserName cannot be empty" }, JsonRequestBehavior.AllowGet);

            }
            //check if app is registered
            var app = _appIDRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {
                return Json(new { responseCode = 0, message = "App has been denied Access, Contact NUPRC Dev" }, JsonRequestBehavior.AllowGet);
            }

            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                return Json(new { responseCode = 0, message = "App has been denied Access, Contact NUPRC Dev" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(newemail) || string.IsNullOrEmpty(newemail))
            {
                // Err: 400
                return Json(new { responseCode = 0, message = "Please check the entered values and try again." }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            try
            {
                var comp = _compRep.FindBy(a => a.Id == model.CompanyId).FirstOrDefault();
                bool change = false;
                if (!string.IsNullOrEmpty(model.Name) && model.Name != comp.Name)
                {
                    comp.Name = model.Name;
                    change = true;
                }
                if (!string.IsNullOrEmpty(model.RC_Number) && model.RC_Number != comp.RC_Number)
                {
                    comp.RC_Number = model.RC_Number;
                    change = true;
                }
                if (!string.IsNullOrEmpty(model.Business_Type) && model.Business_Type != comp.Business_Type)
                {
                    comp.Business_Type = model.Business_Type;
                    change = true;
                }


                if (comp.User_Id.ToLower() != newemail.ToLower())
                {
                    var user = UserManager.FindByEmail(comp.User_Id);
                    if (user != null)
                    {
                        //update user
                        user.Email = newemail;
                        user.UserName = newemail;
                        user.EmailConfirmed = false;

                        UserManager.Update(user);
                        if (UserManager.HasPassword(user.Id))
                        {
                            UserManager.RemovePassword(user.Id);
                        }
                        var result = UserManager.AddPassword(user.Id, newemail + 1);
                    }
                    else
                    {
                        //create new user
                        #region create new user
                        user = new ApplicationUser { UserName = newemail, Email = newemail, PhoneNumber = comp.Contact_Phone, PhoneNumberConfirmed = true };
                        var result = UserManager.Create(user, newemail + 1.ToString());
                        if (result.Succeeded)
                        {
                            var x = UserManager.AddToRole(user.Id, "Company");
                        }
                        #endregion
                    }

                    var code = UserManager.GenerateEmailConfirmationToken(user.Id);

                    #region send Re-Activation Link

                    //send activation link
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code, appId = app.PublicKey }, protocol: Request.Url.Scheme);
                    //var callbackUrl = string.Format("{0}/Account/ConfirmEmail?userId={1}&code={2}&appId={3}", ConfigurationManager.AppSettings["myBaseUrl"], user.Id, code, app.PublicKey);
                    var body = "";
                    using (var sr = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath(@"\\App_Data\\Templates\") + "ComfirmEmailChange.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    var msgBody = string.Format(body, model.Name, callbackUrl, "Confirm Your NUPRC ELPS Account");
                    MailHelper.SendEmail(newemail, "Confirm Your NUPRC ELPS Account", msgBody);

                    var msg = new Message();
                    msg.Company_Id = comp.Id;
                    msg.Content = msgBody;
                    msg.Date = UtilityHelper.CurrentTime;
                    msg.Read = 0;
                    msg.Subject = "Confirm Your NUPRC ELPS Account";
                    msg.Sender_Id = "Application";

                    _msgRep.Add(msg);
                    _msgRep.Save(newemail, Request.UserHostAddress);
                    #endregion

                    comp.User_Id = newemail;
                    _compRep.Edit(comp);
                    _compRep.Save(newemail, Request.UserHostAddress);

                    return Json(new { responseCode = 1 }, JsonRequestBehavior.AllowGet);
                }
                else if (change)
                {
                    _compRep.Edit(comp);
                    _compRep.Save(newemail, Request.UserHostAddress);
                    return Json(new { responseCode = 1 }, JsonRequestBehavior.AllowGet);
                }
                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                return Json(new { responseCode = 0, message = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    



 

       


        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Support")]
        public ActionResult AdminForgotPassword()
        {
            if (TempData["Message"] != null)
            {
                if (TempData["Message"].ToString() == "pass")
                    ViewBag.Message = "pass";
                else
                    ViewBag.Message = "fail";

                TempData["Message"] = "";
            }
            return View();
        }

        [Authorize(Roles = "Admin, Support")]
        [HttpPost]
        public ActionResult AdminForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                #region  Pushed to method below

                //var user = await UserManager.FindByNameAsync(model.Email);
                //if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                //{
                //    // Don't reveal that the user does not exist or is not confirmed
                //    ViewBag.report = "User does not Exist!";
                //    return View("ForgotPasswordConfirmation");
                //}

                //var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                #endregion

                //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                var uid = "";
                var code = GetResetPasswordToken(model.Email, out uid);
                //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = uid, code = code }, protocol: Request.Url.Scheme);
                if (code.ToLower() != "fail".ToLower())
                {
                    ViewBag.Link = code;
                    return View("_AdminResetPassword");
                }
            }

            // If we got this far, something failed, redisplay form
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        #region Lock out
        [Authorize(Roles = "Admin")]
        public ActionResult Lockout(string Id)
        {
            //id= email
            var usr = UserManager.FindByEmail(Id);
            if (usr != null)
            {
                var lo = _lockOutRep.FindBy(a => a.UserId == usr.Id).FirstOrDefault();

                if (lo != null && !lo.Resolved)
                {
                    ViewBag.locked = true;

                    ModelState.AddModelError("", "The User is alreadyy Locked out");
                    return View(lo);
                }

                ViewBag.locked = false;
                lo = new LockedOutUser();
                lo.UserId = usr.Id;
                return View(lo);
            }
            ViewBag.error = "Sorry, Something went wrong";
            return View("Error");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Lockout(LockedOutUser model)
        {
            if (model != null)
            {
                model.Resolved = false;
                _lockOutRep.Add(model);
                _lockOutRep.Save(User.Identity.Name, Request.UserHostAddress);

                return RedirectToAction("Lockout");
            }
            ViewBag.error = "Sorry, Something went wrong";
            return View("Error");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult LockedOut()
        {
            var los = _vLockOutRep.FindBy(a => a.Resolved == false).ToList();
            return View(los);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Resolve(int Id)
        {
            var lo = _lockOutRep.FindBy(a => a.Id == Id).FirstOrDefault();
            lo.Resolved = true;
            _lockOutRep.Edit(lo);
            _lockOutRep.Save(User.Identity.Name, Request.UserHostAddress);

            return RedirectToAction("LookedOut");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllUsers()
        {
            // var usrs = _usrRep.GetAll();
            return View();
        }

        public ActionResult LazyLoadAllUserQuery(JQueryDataTableParamModel param)
        {
            var allPayments = _usrRep.GetAll();
            IEnumerable<AspNetUser> filteredPayments;
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<AspNetUser, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Email : c.PhoneNumber
                 );

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            List<AspNetUser> displayedTransactions = new List<AspNetUser>();

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var s = param.sSearch.ToLower();
                filteredPayments = allPayments.Where(C => C.Email.Trim().ToLower().Contains(s) ||
                        C.PhoneNumber.Trim().ToLower().Contains(s));

                if (sortDirection == "asc")
                {
                    filteredPayments = filteredPayments.OrderBy(orderingFunction);
                }
                else
                {
                    filteredPayments = filteredPayments.OrderByDescending(orderingFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filteredPayments = allPayments.OrderBy(orderingFunction);
                }
                else
                {
                    filteredPayments = allPayments.OrderByDescending(orderingFunction);
                }
            }

            displayedTransactions = filteredPayments.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            var result = from c in displayedTransactions
                         select new[] { c.Email, c.PhoneNumber, c.Id };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allPayments.Count(),
                iTotalDisplayRecords = filteredPayments.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        public ActionResult ResendEmailActivation(string appId)
        {
            ViewBag.appId = appId;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ResendEmailActivation(string Email, string appid)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return View("Error");
            }

            var usr = await UserManager.FindByEmailAsync(Email);

            if (usr != null)
            {
                var comp = _compRep.FindBy(a => a.User_Id == usr.Email).FirstOrDefault();
                if (comp == null)
                {
                    ViewBag.UserNotExist = "Not Allowed!";
                    TempData["alertModel"] = new AlertModel() { AlertType = "warn", Title = "Alert", Message = ViewBag.UserNotExist };
                    //return View("DisplayEmailResend");
                    return RedirectToAction("Login", "Account", new { appId = appid });

                }
                if (!await UserManager.IsEmailConfirmedAsync(usr.Id))
                {

                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(usr.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = usr.Id, code = code, appId = appid }, protocol: Request.Url.Scheme);
                    var body = "";
                    //Read template file from the App_Data folder
                    using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    var msgBody = string.Format(body, comp.Name, callbackUrl, "Confirm Your OGISP Account");
                    //var body = MailHelper.getMailBody(callbackUrl, model.Email);
                    MailHelper.SendEmail(usr.Email, "Re Confirm your account", msgBody);

                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

                    var msg = new Message();
                    msg.Company_Id = comp.Id;
                    msg.Content = msgBody;
                    msg.Date = DateTime.Now;
                    msg.Read = 0;
                    msg.Subject = "Re Confirm your account";
                    msg.Sender_Id = "Application";

                    _msgRep.Add(msg);
                    _msgRep.Save(User.Identity.Name, Request.UserHostAddress);
                    ViewBag.Email = usr.Email;
                    TempData["alertModel"] = new AlertModel() { AlertType = "success", Title = "Account Confirmation Re-Sent", Message = "Account confirmation mail has been re-send to your email. Please login to confirm your account." };
                    return RedirectToAction("Login", "Account", new { appId = appid });
                    //return View("DisplayEmailResend");
                }
                //ViewBag.Email = usr.Email;
                var report = string.Format("Your Account is already active. {0} Please use login link or Forgot Password to reset your Password if you've forgotten.", Environment.NewLine);

                TempData["alertModel"] = new AlertModel() { AlertType = "warn", Title = "Account is Active", Message = report };
                return RedirectToAction("Login", "Account", new { appId = appid });
                //return View("DisplayEmailResend");
            }
            ViewBag.UserNotExist = "User does not Exist!";
            TempData["alertModel"] = new AlertModel() { AlertType = "warn", Title = "Alert", Message = ViewBag.UserNotExist };
            //return View("DisplayEmailResend");
            return RedirectToAction("Login", "Account", new { appId = appid });
        }
        #endregion

        // Called from Child Portals to Reset password
        [AllowAnonymous]
        public ActionResult ResetPasswordToken(string Email, string email, string apiHash)
        {
            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { responseCode = 0, message = "App UserName cannot be empty" }, JsonRequestBehavior.AllowGet);

            }
            //check if app is registered
            var app = _appIDRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {
                return Json(new { responseCode = 0, message = "App has been denied Access, Contact NUPRC Dev" }, JsonRequestBehavior.AllowGet);
            }

            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                return Json(new { responseCode = 0, message = "App has been denied Access, Contact NUPRC Dev" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Email))
            {
                // Err: 400
                return Json(new { responseCode = 0, message = "Please check the entered values and try again." }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            var uid = "";
            var code = GetResetPasswordToken(Email, out uid);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = uid, code = code }, protocol: Request.Url.Scheme);
            if (callbackUrl.ToLower() != "fail".ToLower())
            {
                return Json(new { status = 1, code = callbackUrl }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = 0, code = callbackUrl }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult TestLogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            MvcApplication.CurrentUser = null;
            return RedirectToAction("Login");
        }

        // POST: /Account/LogOff
        [HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var appid = "";
            if (TempData["appid"] != null)
            {
                appid = TempData["appid"].ToString();
            }
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            MvcApplication.CurrentUser = null;// new CurrentLog();

            if (!string.IsNullOrEmpty(appid))
            {
                var appIdy = _appIDRep.FindBy(a => a.PublicKey.ToString().ToLower() == appid.ToLower()).FirstOrDefault();
                if (appIdy != null)
                {
                    return Redirect(appIdy.BaseUrl);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult RemoteLogOff(string returnurl, string appid)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            MvcApplication.CurrentUser = null;
            if (!string.IsNullOrEmpty(returnurl))
            {
                return Redirect(returnurl);
            }
            else
            {
                return RedirectToAction("Login", new { appId = appid });
            }
        }

        [AllowAnonymous]
        public ActionResult LogOffFromRemote(string returnurl, string appid)
        {
            if (!string.IsNullOrEmpty(appid))
            {
                TempData["appid"] = appid;
                TempData["returnurl"] = returnurl;

                var frm = $"<form action=\"/Account/Logoff\" id=\"frmTest\" method=\"post\"></form><script>document.getElementById(\"frmTest\").submit();</script>";
                return Content(frm);
            }
            else
            {
                return RedirectToAction("Login", new { appId = appid });
            }
            //try
            //{
            //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //    MvcApplication.CurrentUser = null;
            //    return Json("01");
            //}
            //catch (Exception)
            //{
            //    return Json("00");
            //}
        }


        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
      
        private string GetDefaultPage()
        {
            return string.Format("{0}{1}{2}", Request.Url.Scheme, Uri.SchemeDelimiter, Request.Url.Authority);
        }

        [AllowAnonymous]
        public void MicrosoftSignIn(string returnUrl, string appId)
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl, appId = appId }) },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        /// <summary>
        /// Send an OpenID Connect sign-out request.
        /// </summary>
        public void MicrosoftSignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                    OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    DefaultAuthenticationTypes.ApplicationCookie);
        }

        
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult MicrosoftLogin(string returnUrl, string appId)
        {
            Session["Workaround"] = 0;
            ControllerContext.HttpContext.Session.RemoveAll();
            //returnUrl = GetDefaultPage();
            return new ChallengeResult("Microsoft", Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl, appId = appId }));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }




        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }


        private string GetResetPasswordToken(string email, out string uid)
        {
            var user = UserManager.FindByName(email);
            if (user == null || !(UserManager.IsEmailConfirmed(user.Id)))
            {
                uid = "";
                return "fail"; // View("ForgotPasswordConfirmation");
            }

            var code = UserManager.GeneratePasswordResetToken(user.Id);
            uid = user.Id;
            //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

            return code;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl) || extAppHelper.IsLocalAppUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// This method is responsible for sending a confirmation mail to the Affilate company
        /// </summary>
        /// <param name="callbackUrl"></param>
        /// <param name="email"></param>
        /// <param name="appId"></param>
        private void SendMailToAffiliate(string callbackUrl, string email, string appId)
        {
            var body = "";
            //Read template file from the App_Data folder
            using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
            {
                body = sr.ReadToEnd();
            }

            var msgBody = string.Format(body, email, callbackUrl, "Confirm Your NUPRC ELPS Account");
            UtilityHelper.LogMessage("Sending Affiliate confirmation mail");
            MailHelper.SendEmail(email, "Confirm your NUPRC account", msgBody);
            UtilityHelper.LogMessage("Affiliate Confirmation mail sent to " + email);
        }

        /// <summary>
        /// This method is responsible for sending a confirmation mail to the parent company about the child company
        /// </summary>
        /// <param name="link"></param>
        /// <param name="parentCompany"></param>
        /// <param name="childCompany"></param>
        /// <param name="code"></param>
        private void SendMailToParentCompany(string link, Company parentCompany, Company childCompany, string code)
        {
            var body = "";
            using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ParentComfirmMail.txt"))
            {
                body = sr.ReadToEnd();
            }


            var msgBody = string.Format(body, "Your new Affiliate/Daughter company on NUPRC", parentCompany.Contact_FirstName, link, "",
                DateTime.Now.Year, childCompany.Name, childCompany.Contact_Phone, childCompany.User_Id, childCompany.Contact_FirstName, code.ToUpper());

            UtilityHelper.LogMessage("Sending mail to Parent Company to confirm Affiliate");
            MailHelper.SendEmail(parentCompany.User_Id, "Your new Affiliate/Daughter company on NUPRC", msgBody);
            UtilityHelper.LogMessage("Parent Confirmation of Affiliate mail sent");
        }

        private bool ProcessRegistration(AccountRegisterDTO model, string appId, out List<string> errors, out Company existingCompany)
        {
            try
            {
                existingCompany = _compRep.GetCompany(model.CompanyName, model.RegistrationNumber);
                errors = new List<string>();

                var emailCheck = UserManager.FindByName(model.Email);

                if (emailCheck != null)
                {
                    errors.Add("Email has already been taken");
                    return false;
                }

                if (existingCompany != null)
                    return false;

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, PhoneNumberConfirmed = true };

                var result = UserManager.Create(user, model.Password);

                if (result.Succeeded)
                {
                    //Add a company Role to the created user
                    //var x = UserManager.AddToRole(user.Id, "Company");

                    //Add the company details to a model
                    var comp = new Company();
                    comp.User_Id = model.Email;
                    comp.Name = model.CompanyName;
                    comp.Business_Type = model.BusinessType;
                    comp.Contact_Phone = model.PhoneNumber;
                    comp.RC_Number = model.RegistrationNumber;
                    comp.Date = UtilityHelper.CurrentTime;
                    _compRep.Add(comp);
                    _compRep.Save(User.Identity.Name, Request.UserHostAddress);

                    //create a token for email Confirmation and embed it in the callback url
                    var code = UserManager.GenerateEmailConfirmationToken(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, appId = appId, code = code }, protocol: Request.Url.Scheme);

                    //send this as a mail to the user
                    var body = "";

                    //Read template file from the App_Data folder
                    using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    var msgBody = string.Format(body, model.CompanyName, callbackUrl, "Confirm Your NUPRC ELPS Account");
                    MailHelper.SendEmail(user.Email, "Confirm Your NUPRC ELPS Account", msgBody);

                    //add the message just sent to the messageLog in the database
                    var msg = _msgRep.CreateMessage(comp.Id, msgBody, "Application", "Confirm Your NUPRC ELPS Account");
                    _msgRep.Add(msg);
                    _msgRep.Save(User.Identity.Name, Request.UserHostAddress);

                    return true;
                }
                //If you get here it means there must have been an error, compile the list of errors
                errors = (List<string>)result.Errors;
                return false;
            }
            catch (Exception ex)
            {
                //Revert the creation of user
                var us = UserManager.FindByEmail(model.Email);
                if (us != null)
                {
                    UserManager.Delete(us);
                }
                throw ex;
            }
        }

        //private bool ProcessRegistration(AccountRegisterDTO model, string appId, out string clink, out Company existingCompany)
        //{
        //    try
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, PhoneNumberConfirmed = true };

        //        var comp = new Company();

        //        existingCompany = _compRep.FindBy(C => C.RC_Number.ToLower().Trim() == model.RegistrationNumber.Trim().ToLower()
        //            || C.Name.ToLower().Trim() == model.CompanyName.Trim().ToLower()).FirstOrDefault();
        //        if (existingCompany != null)
        //        {
        //            //throw new Exception("Companyexists");
        //            clink = "Company exists";
        //            return false;
        //        }

        //        var result = UserManager.Create(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            var x = UserManager.AddToRole(user.Id, "Company");

        //            //var comp = new Company();
        //            comp.User_Id = model.Email;
        //            comp.Name = model.CompanyName;
        //            comp.Business_Type = model.BusinessType;
        //            comp.Contact_Phone = model.PhoneNumber;
        //            comp.RC_Number = model.RegistrationNumber;
        //            comp.Date = UtilityHelper.CurrentTime;
        //            _compRep.Add(comp);
        //            _compRep.Save(User.Identity.Name, Request.UserHostAddress);

        //            var code = UserManager.GenerateEmailConfirmationToken(user.Id);
        //            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, appId = appId, code = code }, protocol: Request.Url.Scheme);
        //            var body = "";
        //            //Read template file from the App_Data folder
        //            using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
        //            {
        //                body = sr.ReadToEnd();
        //            }
        //            var msgBody = string.Format(body, model.CompanyName, callbackUrl, "Confirm Your DPR ELPS Account");

        //            //var body = MailHelper.getMailBody(callbackUrl, model.Email);
        //            // UserManager.SendEmail(user.Id, "Confirm your account", msgBody);

        //            MailHelper.SendEmail(user.Email, "Confirm Your DPR ELPS Account", msgBody);
        //            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

        //            var msg = new Message();
        //            msg.Company_Id = comp.Id;
        //            msg.Content = msgBody;
        //            msg.Date = UtilityHelper.CurrentTime;
        //            msg.Read = 0;
        //            msg.Subject = "Confirm Your DPR ELPS Account";
        //            msg.Sender_Id = "Application";

        //            _msgRep.Add(msg);
        //            _msgRep.Save(User.Identity.Name, Request.UserHostAddress);

        //            clink = callbackUrl;

        //            return true;
        //        }
        //        var errs = "";
        //        foreach (var er in result.Errors)
        //        {
        //            errs += er + "; ";
        //        }
        //        throw new ArgumentException(errs);
        //        //clink = "Account not created";
        //        //return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        var us = UserManager.FindByEmail(model.Email);
        //        if (us != null)
        //        {
        //            UserManager.Delete(us);
        //        }
        //        clink = ex.Message;
        //        existingCompany = null;
        //        return false;
        //        //throw;
        //    }
        //}

        /// <summary>
        /// This handles the grouping of portals based on the divisions
        /// </summary>
        /// <param name="allActivePortals"></param>
        /// <param name="selectedPortal"></param>
        /// <param name="divisions"></param>
        /// <param name="appId"></param>
        private void GroupPortals(out List<AppIdentity> allActivePortals, out AppIdentity selectedPortal, out List<Division> divisions, string appId)
        {
            allActivePortals = new List<AppIdentity>();
            selectedPortal = null;
            divisions = _PortalCat.GetAll()
                .OrderBy(m=>m.SortOrder)
                .ThenBy(m=>m.Id).ToList();

            foreach (var division in divisions)
            {
                division.Portals = new List<AppIdentity>();
                var PortalsInDivision = _portalToCategory.GetPortalsForDivision(division.Id);

                var result = allActivePortals.Union(PortalsInDivision, new DistinctComparer<AppIdentity>());

                division.Portals = PortalsInDivision.ToList();

                allActivePortals = result.ToList();
            }

            if (!string.IsNullOrEmpty(appId))
            {
                selectedPortal = allActivePortals.FirstOrDefault(m => m.PublicKey.ToString() == appId);
            }
        }


        /// <summary>
        /// This returns the appropriate url redirect based on user role 
        /// </summary>
        /// <returns></returns>
        private string ReturnBasedOnRole(string userId)
        {

            var userRoles = UserManager.GetRoles(userId);

            if (userRoles.Contains("Admin") || userRoles.Contains("ITAdmin") || userRoles.Contains("Account") || 
                userRoles.Contains("ManagerObserver") || userRoles.Contains("Support") ||userRoles.Contains("LicenseAdmin"))
            {
                return Url.Action("Index", "AdminDashboard");//
            }
            else if (userRoles.Contains("Company")||userRoles.Contains("Affiliate")) //(User.IsInRole("Company"))
            {
                var user = User.Identity.Name.ToLower();
                var myCoy = _compRep.FindBy(c => c.User_Id.ToLower() == user).FirstOrDefault();

                return Url.Action("Index", "Dashboard");
            }
            else
            {
                return Url.Action("StaffDashboard", "AdminDashBoard");
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        
        #endregion Helper

        #region Comment

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, string appId)
        //{
        //    ViewBag.appId = appId;
        //    ViewBag.ReturnUrl = returnUrl;

        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    AppIdentity app = null;

        //    // Check to make sure call is from our clients;
        //    if (!string.IsNullOrEmpty(appId))
        //    {
        //        Guid pk = Guid.Parse(appId);
        //        app = _appIDRep.FindBy(a => a.PublicKey == pk).FirstOrDefault();
        //        if (app == null)
        //        {
        //            return View("Error");
        //        }
        //    }

        //    var usr = await UserManager.FindByEmailAsync(model.Email);

        //    if (usr != null)
        //    {
        //        if (!await UserManager.IsEmailConfirmedAsync(usr.Id))
        //        {
        //            ModelState.AddModelError("", "Please check your mail to confirm your Account before you can Login");
        //            var apps = _appIDRep.FindBy(a => a.IsActive && (a.OfficeUse == null || a.OfficeUse.Value == false)).ToList();
        //            ViewBag.Portals = apps;
        //            //ViewBag.resendActivation = "Please check your mail to confirm your Account before you can Login";
        //            return View(model);

        //            //return RedirectToAction("ResendEmailActivation");
        //        }

        //        //check if the User have been locked out
        //        var lo = _lockOutRep.FindBy(a => a.UserId == usr.Id && a.Resolved == false).FirstOrDefault();

        //        if (lo != null)
        //        {
        //            ModelState.AddModelError("", "Sorry but you have been locked out of DPR Services, Please contact the Support Center for for details");
        //            var apps = _appIDRep.FindBy(a => a.IsActive && (a.OfficeUse == null || a.OfficeUse.Value == false)).ToList();
        //            ViewBag.Portals = apps;
        //            //ViewBag.resendActivation = "Please check your mail to confirm your Account before you can Login";
        //            return View(model);
        //        }
        //    }


        //    // This doesn't count login failures towards account lockout
        //    // To enable password failures to trigger account lockout, change to shouldLockout: true

        //    var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            MvcApplication.CurrentUser = new CurrentLog() { IsAuthenticated = true, Name = model.Email };
        //            if (app != null)
        //            {
        //                //TempData["AppData"] = app;
        //                return RedirectToAction("ProcessAppData", app);// RedirectToLocal(app.Url);
        //            }


        //            return RedirectToLocal(returnUrl + "?email=" + model.Email);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid login attempt.");
        //            var apps = _appIDRep.FindBy(a => a.IsActive && (a.OfficeUse == null || a.OfficeUse.Value == false)).ToList();
        //            ViewBag.Portals = apps;
        //            return View(model);
        //    }
        //}



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult RegisterNonCompanyUser(AccountRegisterDTO model, string appId)
        //{
        //    try
        //    {
        //        ViewBag.Affilate = "Affilate";
        //        var user = UserManager.FindByEmail(model.Email);
        //        if (user != null)
        //        {
        //            return PartialView("RegisterForm", model);
        //            // return Json(new { status = 1, message = "User Already Exist!" }, JsonRequestBehavior.AllowGet);

        //        }
        //        //create an application user
        //        user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, PhoneNumberConfirmed = true };

        //        var result = UserManager.Create(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            var x = UserManager.AddToRole(user.Id, "Affiliate");

        //            var parentCoy = _compRep.FindBy(a => a.Id == model.ParentCoyId).FirstOrDefault();

        //            var comp = new Company();
        //            comp.User_Id = model.Email;
        //            comp.Name = model.ChildCoyName;
        //            comp.Business_Type = model.BizType;
        //            comp.Contact_Phone = model.PhoneNumber;
        //            comp.RC_Number = parentCoy.RC_Number;
        //            comp.Tin_Number = parentCoy.Tin_Number;
        //            //comp.Registered_Address_Id = parentCoy.Registered_Address_Id;
        //            comp.Year_Incorporated = parentCoy.Year_Incorporated;
        //            comp.Nationality = parentCoy.Nationality;
        //            comp.Date = UtilityHelper.CurrentTime;
        //            comp.ParentCompanyId = parentCoy.Id;


        //            _compRep.Add(comp);
        //            _compRep.Save(model.Email, Request.UserHostAddress);

        //            #region Create Affiliate
        //            var _code = (Guid.NewGuid()).ToString().Split('-')[0];
        //            var cc = PaymentRef.getHash(comp.Id.ToString() + "_" + comp.User_Id + "_" + parentCoy.Id.ToString() + "_" + _code, true);

        //            var aff = new Affiliate()
        //            {
        //                ChildId = comp.Id,
        //                DateAdded = UtilityHelper.CurrentTime,
        //                ParentId = parentCoy.Id,
        //                UniqueId = Guid.NewGuid(),
        //                Code = cc
        //            };
        //            _affRep.Add(aff);
        //            _affRep.Save(model.Email, Request.UserHostAddress);
        //            #endregion

        //            var code = UserManager.GenerateEmailConfirmationToken(user.Id);
        //            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, appId = model.AppId, code = code }, protocol: Request.Url.Scheme);
        //            var body = "";
        //            //Read template file from the App_Data folder
        //            using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
        //            {
        //                body = sr.ReadToEnd();
        //            }

        //            var msgBody = string.Format(body, model.Email, callbackUrl, "Confirm Your DPR ELPS Account");
        //            UtilityHelper.LogMessage("Sending Affiliate confirmation mail");
        //            MailHelper.SendEmail(model.Email, "Confirm your DPR account", msgBody);
        //            UtilityHelper.LogMessage("Affiliate Confirmation mail sent to " + model.Email);

        //            #region Send Confirmation mail to Parent
        //            body = "";
        //            using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ParentComfirmMail.txt"))
        //            {
        //                body = sr.ReadToEnd();
        //            }
        //            var link = Url.Action("ConfirmAffiliate", "Account", new { code = aff.UniqueId, em = parentCoy.User_Id }, protocol: Request.Url.Scheme);
        //            msgBody = string.Format(body, "Your new Affiliate/Daughter company on DPR", parentCoy.Contact_FirstName, link, "",
        //                DateTime.Now.Year, comp.Name, comp.Contact_Phone, comp.User_Id, comp.Contact_FirstName, _code.ToUpper());
        //            UtilityHelper.LogMessage("Sending mail to Parent Company to confirm Affiliate");
        //            MailHelper.SendEmail(parentCoy.User_Id, "Your new Affiliate/Daughter company on DPR", msgBody);
        //            UtilityHelper.LogMessage("Parent Confirmation of Affiliate mail sent");
        //            #endregion

        //            return Json(new { status = 1, message = "User should please check email to confirm account" }, JsonRequestBehavior.AllowGet);
        //        }

        //        string err = "";
        //        foreach (var item in result.Errors)
        //        {
        //            err += "; " + item;
        //        }
        //        return Json(new { status = 0, message = err.Trim(';') }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { status = 0, message = "Error Occured, Please try again" }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        //public ActionResult ProcessAppData(AppIdentity app)
        //{
        //AppIdentity app = null;

        //if (!string.IsNullOrEmpty(q))
        //{
        //    var pk = Guid.Parse(q);
        //    app = _appIDRep.FindBy(a => a.PublicKey == pk).FirstOrDefault();
        //}
        //else if (TempData["AppData"] != null)
        //{
        //    app = (AppIdentity)TempData["AppData"];
        //}

        //if (app != null)
        //{
        ////var postForm = "";
        //var usr = User.Identity.Name;
        //var hashStr = $"{app.PublicKey.ToString()}.{usr}.{app.AppId}";
        //var code = HashManager.GetHash(hashStr.ToUpper());
        //var frm = "<form action='" + app.LoginRedirect + "' id='frmTest' method='post'>" +
        //    "<input type='hidden' name='email' value='" + usr + "' />" +
        //    "<input type='hidden' name='code' value='" + code + "' />" +
        //    "</form>" +
        //    "<script>document.getElementById('frmTest').submit();</script>";
        //return Content(frm);
        //}
        //return View("Error");




        //[Route("Login/{returnUrl?}/{appId?}")]
        //public ActionResult Login(string returnUrl, string appId)
        //{
        //return throw;// Exception;

        //ViewBag.ReturnUrl = returnUrl;
        //ViewBag.appId = appId;

        //if (TempData["alertModel"] != null)
        //{
        //    ViewBag.Alert = (AlertModel)TempData["alertModel"];
        //}

        //if (Request.IsAuthenticated)
        //{
        //    if (!string.IsNullOrEmpty(appId))
        //    {
        //        Guid pk = Guid.Parse(appId);
        //        var app = _appIDRep.FindBy(a => a.PublicKey == pk).FirstOrDefault();
        //        if (app != null)
        //        {
        //            TempData["AppData"] = app;
        //            return RedirectToAction("ProcessAppData");// RedirectToLocal(app.Url);
        //        }
        //    }
        //    var userRoles = UserManager.GetRoles(User.Identity.GetUserId());
        //    if (userRoles.Contains("Company")) //(User.IsInRole("Company"))
        //    {
        //        var user = User.Identity.Name.ToLower();
        //        var myCoy = _compRep.FindBy(c => c.User_Id.ToLower() == user).FirstOrDefault();

        //        return RedirectToAction("Index", "Dashboard");
        //    }
        //    else if (userRoles.Contains("Admin") || User.IsInRole("ITAdmin") ||
        //        User.IsInRole("Account") || User.IsInRole("ManagerObserver") || User.IsInRole("Support") ||
        //        User.IsInRole("LicenseAdmin"))
        //    {
        //        return RedirectToAction("Index", "AdminDashboard");//
        //    }
        //    else
        //    {
        //        return RedirectToAction("StaffDashboard", "AdminDashBoard");
        //    }
        //}
        //else
        //{

        //var apps = _appIDRep.FindBy(a => a.IsActive && (a.OfficeUse == null || a.OfficeUse.Value == false)).ToList();
        //ViewBag.Portals = apps;
        //if (string.IsNullOrEmpty(appId))
        //{
        //    return View();
        //}

        //Guid pk = Guid.Parse(appId);
        //var app = apps.Where(a => a.PublicKey == pk).FirstOrDefault();
        //if (app != null)
        //{
        //    return View();
        //}
        //else
        //{
        //    return View("Error");
        //}
        ////}
        //}
















        ////
        //// GET: /Account/VerifyCode
        //[AllowAnonymous]
        //public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        //{
        //    // Require that the user has already logged in via username/password or external login
        //    if (!await SignInManager.HasBeenVerifiedAsync())
        //    {
        //        return View("Error");
        //    }
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/VerifyCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // The following code protects for brute force attacks against the two factor codes. 
        //    // If a user enters incorrect codes for a specified amount of time then the user account 
        //    // will be locked out for a specified amount of time. 
        //    // You can configure the account lockout settings in IdentityConfig
        //    var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(model.ReturnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid code.");
        //            return View(model);
        //    }
        //}

        //
        // GET: /Account/Register



        //
        // POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

        //            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //            // Send an email with this link
        //            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

        //            return RedirectToAction("Index", "Home");
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}




        //public ActionResult LazyLoadAllUserQuery(JQueryDataTableParamModel param)
        //{
        //    var allPayments = _usrRep.GetAll();
        //    IEnumerable<AspNetUser> filteredPayments;
        //    var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

        //    Func<AspNetUser, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Email : c.PhoneNumber
        //         );

        //    var sortDirection = Request["sSortDir_0"]; // asc or desc
        //    List<AspNetUser> displayedTransactions = new List<AspNetUser>();

        //    if (!string.IsNullOrEmpty(param.sSearch))
        //    {
        //        var s = param.sSearch.ToLower();
        //        filteredPayments = allPayments.Where(C => C.Email.Trim().ToLower().Contains(s) ||
        //                C.PhoneNumber.Trim().ToLower().Contains(s));

        //        if (sortDirection == "asc")
        //        {
        //            filteredPayments = filteredPayments.OrderBy(orderingFunction);
        //        }
        //        else
        //        {
        //            filteredPayments = filteredPayments.OrderByDescending(orderingFunction);
        //        }
        //    }
        //    else
        //    {
        //        if (sortDirection == "asc")
        //        {
        //            filteredPayments = allPayments.OrderBy(orderingFunction);
        //        }
        //        else
        //        {
        //            filteredPayments = allPayments.OrderByDescending(orderingFunction);
        //        }
        //}

        //    displayedTransactions = filteredPayments.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

        //    var result = from c in displayedTransactions
        //                 select new[] { c.Email, c.PhoneNumber, c.Id };

        //    return Json(new
        //    {
        //        sEcho = param.sEcho,
        //        iTotalRecords = allPayments.Count(),
        //        iTotalDisplayRecords = filteredPayments.Count(),
        //        aaData = result
        //    }, JsonRequestBehavior.AllowGet);

        //}

        //
        // POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

        //            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //            // Send an email with this link
        //            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

        //            return RedirectToAction("Index", "Home");
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}



        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult Register(AccountRegisterDTO model, string returnUrl, string appId)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.businessType = new SelectList(BussinessType.GetBizType().Select(x => new KeyValuePair<string, string>(x, x)), "Key", "Value");
        //        return View("RegisterView", model);
        //    }

        //    //I dont know what this is for:
        //    if (!string.IsNullOrEmpty(returnUrl))
        //    {
        //        Session["ELPS-rtnUrl"] = returnUrl;
        //    }

        //    AppIdentity app = null;

        //    // Check to make sure call is from our clients;
        //    if (!string.IsNullOrEmpty(appId))
        //    {
        //        Guid pk = Guid.Parse(appId);
        //        app = _appIDRep.FindBy(a => a.PublicKey == pk).FirstOrDefault();
        //        if (app == null)
        //        {
        //            //this is needed since processing is dependent on the correct appId
        //            return View("Error");
        //        }
        //    }

        //    using (var trans = new TransactionScope()) // TransactionScopeAsyncFlowOption.Enabled))
        //    {
        //        //var user = new ApplicationUser();
        //        try
        //        {
        //            #region Process Registration
        //            string outresponse = string.Empty;
        //            Company existingCompany = null;


        //            if (ProcessRegistration(model, appId, out outresponse, out existingCompany))
        //            {

        //                trans.Complete();

        //                //I need a displayEmail Page
        //                TempData["status"] = "Pass";
        //                ViewBag.Email = model.Email;
        //                return View("DisplayEmail");
        //            }
        //            if (outresponse.ToLower() == "Company exists".ToLower())
        //            {
        //                // Try to Log the user in
        //                var lResult = SignInManager.PasswordSignIn(model.Email, model.Password, false, shouldLockout: false);
        //                switch (lResult)
        //                {
        //                    case SignInStatus.Success:
        //                        MvcApplication.CurrentUser = new CurrentLog() { IsAuthenticated = true, Name = model.Email };
        //                        TempData["AppData"] = app;
        //                        return RedirectToAction("ProcessAppData");
        //                    default:
        //                        break;
        //                }

        //                ViewBag.Model = new NonCompanyUserModel()
        //                {
        //                    AppId = appId,
        //                    Email = model.Email,
        //                    Password = model.Password,
        //                    PhoneNumber = model.PhoneNumber,
        //                    ParentCoyId = existingCompany.Id,
        //                    ParentCoyName = existingCompany.Name,
        //                    ChildCoyName = model.CompanyName,
        //                    BizType = model.BusinessType
        //                };

        //                //I need to work on the affiliate view
        //                return View("Affiliate");
        //            }
        //            //AddErrors();
        //            throw new ArgumentException(outresponse); // result.Errors.ToString());
        //            #endregion
        //        }
        //        catch (Exception ex)
        //        {
        //            #region  Failed Registration
        //            trans.Dispose();
        //            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

        //            TempData["status"] = "fail";
        //            TempData["message"] = ex.Message.Contains("already taken") ? "Email '" + model.Email + "' already taken" : ex.Message; // "There was an error while handling your request";
        //            ModelState.AddModelError("", ex);
        //            ViewBag.businessType = new SelectList(BussinessType.GetBizType().Select(x => new KeyValuePair<string, string>(x, x)), "Key", "Value");
        //            // If we got this far, something failed, redisplay form
        //            return View("RegisterView",model);
        //            #endregion
        //        }
        //    }
        //}

        //
        // GET: /Account/ConfirmEmail


        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        //[AllowAnonymous]
        //public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        //{
        //    var userId = await SignInManager.GetVerifiedUserIdAsync();
        //    if (userId == null)
        //    {
        //        return View("Error");
        //    }
        //    var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        //
        // POST: /Account/SendCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SendCode(SendCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    // Generate the token and send it
        //    if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
        //    {
        //        return View("Error");
        //    }
        //    return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        //}

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl, string appId)
        {
            try
            {
                var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (!Request.IsAuthenticated)
                {
                    return RedirectToAction("Login", new { returnUrl = returnUrl, appId = appId });
                }

                //var result2 = await SignInManager.ExternalSignInAsync(loginInfo2, isPersistent: false);
                var userClaims = User.Identity as System.Security.Claims.ClaimsIdentity;


                //You get the user’s first and last name below:
                var name = userClaims?.FindFirst("name")?.Value;

                // The 'preferred_username' claim can be used for showing the username
                var username = userClaims?.FindFirst("preferred_username")?.Value;
                var first_name = userClaims?.FindFirst(ClaimTypes.GivenName)?.Value;
                var last_name = userClaims?.FindFirst(ClaimTypes.Surname)?.Value;
                var email = username; // userClaims?.FindFirst(ClaimTypes.Email)?.Value;
                //var user = UserManager.FindByEmail(email);
                if (email == null)
                {
                    return RedirectToAction("Login", new { returnUrl = returnUrl, appId = appId });
                }
                var idClaim = userClaims?.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null)
                {
                    loginInfo = new ExternalLoginInfo()
                    {
                        DefaultUserName = username,
                        Email = email,
                        ExternalIdentity = userClaims,
                        Login = new UserLoginInfo(idClaim.Issuer, idClaim.Value)
                    };
                }
                var result = "failure";

                var emailSplits = email.Split('@');

                var oldEmail = emailSplits[0] + "@dpr.gov.ng";

                var user = UserManager.FindByEmail(oldEmail);

                if (user != null)
                {
                    var staff = _staffRep.FindBy(a => a.UserId.ToLower() == user.Id.ToLower()).FirstOrDefault();
                    
                    if (staff != null)
                    {
                        staff.Email = email;
                        _staffRep.Edit(staff);
                    }
                    _staffRep.Save();

                    await UserManager.SetEmailAsync(user.Id, email);

                    var user2 = UserManager.FindByEmail(email);

                    if (user2 != null)
                    {
                        await UserManager.SetEmailAsync(user2.Id, email);

                        await SignInManager.SignInAsync(user2, false, false);
                        result = "success";

                        if (await UserManager.IsLockedOutAsync(user2.Id))
                        {
                            result = "lockout";
                        }
                    }
                }
                else
                {
                    var user2 = UserManager.FindByEmail(email);

                    if (user2 != null)
                    {
                        await UserManager.SetEmailAsync(user2.Id, email);

                        await SignInManager.SignInAsync(user2, false, false);
                        result = "success";

                        if (await UserManager.IsLockedOutAsync(user2.Id))
                        {
                            result = "lockout";
                        }
                    }
                }

                switch (result)
                {
                    case "success":
                        MvcApplication.CurrentUser = new CurrentLog() { IsAuthenticated = true, Name = loginInfo.Email };
                        AppIdentity app = null;

                        // Check to make sure call is from our clients;
                        if (!string.IsNullOrEmpty(appId))
                        {
                            app = _appIDRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                            if (app == null)
                            {
                                //this is needed since a redirect needs to be triggered with the correct Id
                                return HttpNotFound("Portal cannot be found");
                            }
                        }
                        if (app != null)
                        {
                            TempData["AppData"] = app;

                            TempData["StaffUsers"] = email;


                            /*return Json(new Response()
                            {
                                Result = Result.Success.ToString(),
                                ReturnUrl = Url.Action("ProcessAppData", "Account", new { appId = appId })
                            });*/
                            returnUrl = Url.Action("ProcessAppData", "Account", new { appId = appId });
                        }
                        if (returnUrl == null)
                        {
                            //var user = UserManager.FindByEmail(loginInfo.Email);
                            returnUrl = ReturnBasedOnRole(user.Id);

                        }
                        //return Json(new Response() { Result = Result.Success.ToString(), ReturnUrl = returnUrl });
                        return RedirectToLocal(returnUrl);


                    case "lockout":
                        //TempData["ErrorList"] = new List<string> { "Sorry but you have been locked out of DPR Services, Please contact the Support Center for for details" };
                        //return PartialView("LoginForm");
                        return RedirectToAction("Login", new { returnUrl = returnUrl, appId = appId });
                    case "failure":
                    default:
                        // If the user does not have an account, then prompt the user to create an account
                        return await RegisterAdStaff(loginInfo.Email, returnUrl, appId);
                }
            }catch(Exception ex)
            {
                var error= "Error during externallogincallback processing"+ex.Message;
                return RedirectToAction("Login", new { returnUrl = returnUrl, appId = appId });
            }
            
            /* var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            
            if (loginInfo == null)
            {
                return RedirectToAction("Login", new { returnUrl = returnUrl, appId = appId });
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {

                case SignInStatus.Success:
                    MvcApplication.CurrentUser = new CurrentLog() { IsAuthenticated = true, Name = loginInfo.Email };
                    AppIdentity app = null;

                    // Check to make sure call is from our clients;
                    if (!string.IsNullOrEmpty(appId))
                    {
                        app = _appIDRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                        if (app == null)
                        {
                            //this is needed since a redirect needs to be triggered with the correct Id
                            return HttpNotFound("Portal cannot be found");
                        }
                    }
                    if (app != null)
                    {
                        TempData["AppData"] = app;

                        return Json(new Response()
                        {
                            Result = Result.Success.ToString(),
                            ReturnUrl = Url.Action("ProcessAppData", "Account", new { appId = appId })
                        });
                    }
                    if (returnUrl == null)
                    {
                        var user = UserManager.FindByEmail(loginInfo.Email);
                        returnUrl = ReturnBasedOnRole(user.Id);

                    }
                    //return Json(new Response() { Result = Result.Success.ToString(), ReturnUrl = returnUrl });
                    return RedirectToLocal(returnUrl);


                case SignInStatus.LockedOut:
                    TempData["ErrorList"] = new List<string> { "Sorry but you have been locked out of DPR Services, Please contact the Support Center for for details" };
                    return PartialView("LoginForm");
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    return await RegisterAdStaff(loginInfo.Email, returnUrl, appId);
            }*/
            //return RedirectToAction("Login");
        }
        private async Task<ActionResult> RegisterAdStaff(string email, string returnUrl, string appId)
        {
            var userClaims = User.Identity as System.Security.Claims.ClaimsIdentity;

            //You get the user’s first and last name below:
            var name = userClaims?.FindFirst("name")?.Value;

            // The 'preferred_username' claim can be used for showing the username
            var username = userClaims?.FindFirst("preferred_username")?.Value; 
            var ipadd = userClaims?.FindFirst("ipaddr")?.Value; 
            var first_name = userClaims?.FindFirst(ClaimTypes.GivenName)?.Value;
            var last_name = userClaims?.FindFirst(ClaimTypes.Surname)?.Value;
            var idClaim = userClaims?.FindFirst(ClaimTypes.NameIdentifier);
            var loginInfo = new ExternalLoginInfo();
            loginInfo.Email = email;
            loginInfo.DefaultUserName = username;
            loginInfo.ExternalIdentity = userClaims;
            loginInfo.Login = new UserLoginInfo(idClaim.Issuer, idClaim.Value);
            if ((first_name == null || last_name==null) && name == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl, appId = appId });
            }
            if (first_name == null || last_name == null)
            {
                string[] names = name.Split(' ');
                first_name = names.FirstOrDefault();
                last_name = names.LastOrDefault();
            }
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true};
            var result = await UserManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var result2 = await UserManager.AddToRoleAsync(user.Id, "Staff");
                if (result2.Succeeded)
                {
                    /*var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
                    var first = claimsIdentity.FindFirst("preferred_username");
                    var last = claimsIdentity.FindFirst("preferred_username");*/
                    Staff staff = new Staff();
                    staff.Email = email;
                    staff.FirstName = first_name;
                    staff.LastName = last_name;
                    staff.UserId = user.Id;
                    _staffRep.Add(staff);
                    //_staffRep.Save(User.Identity.Name, ipadd);

                    result = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        MvcApplication.CurrentUser = new CurrentLog() { IsAuthenticated = true, Name = email };
                        AppIdentity app = null;

                        // Check to make sure call is from our clients;
                        if (!string.IsNullOrEmpty(appId))
                        {
                            app = _appIDRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                            if (app == null)
                            {
                                //this is needed since a redirect needs to be triggered with the correct Id
                                return HttpNotFound("Portal cannot be found");
                            }
                        }
                        if (app != null)
                        {
                            TempData["AppData"] = app;

                            /*return Json(new Response()
                            {
                                Result = Result.Success.ToString(),
                                ReturnUrl = Url.Action("ProcessAppData", "Account", new { appId = appId })
                            });*/
                            returnUrl = Url.Action("ProcessAppData", "Account", new { appId = appId });
                        }
                        if (returnUrl == null)
                        {
                            var nuser = UserManager.FindByEmail(email);
                            returnUrl = ReturnBasedOnRole(nuser.Id);

                        }
                        //return Json(new Response() { Result = Result.Success.ToString(), ReturnUrl = returnUrl });
                        return RedirectToLocal(returnUrl);
                    }
                }
            }
            return RedirectToAction("Login");
        }
        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //

        #endregion Comment

    }
}