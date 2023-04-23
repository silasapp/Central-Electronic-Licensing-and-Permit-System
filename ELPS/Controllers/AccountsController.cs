using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using System.Net;
using System.Net.Http;
using System.Web.Http;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ELPS.Models;
using ELPS.Domain.Abstract;
using ELPS.Helpers;
using System.Web.Security;
using ELPS.Domain.Entities;
using System.IO;
using System.Configuration;
using System.Web.Http.Description;
using System.Collections.Generic;

namespace ELPS.Controllers
{
    [RoutePrefix("api/Accounts")]
    public class AccountsController : ApiController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationSignInManager _signInManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        IAppIdentityRepository _appIdRep;
        ICompanyRepository _compRep;
        IMessageRepository _msgRep;
        WebApiAccessHelper accessHelper;
        IStaffRepository _staffRep;
        ICompanyNameHistoryRepository _compHistRep;

        public AccountsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public AccountsController(IAppIdentityRepository appId, ICompanyRepository compRep, IMessageRepository msgRep,
            IStaffRepository staffRep, ICompanyNameHistoryRepository compHistRep)
        {
            _compHistRep = compHistRep;
            _msgRep = msgRep;
            _compRep = compRep;
            _appIdRep = appId;
            accessHelper = new WebApiAccessHelper(appId);
            _staffRep = staffRep;
        }

        //
        // POST: ~api/Accounts/ChangePassword
        [Route("ChangePassword/{useremail}/{email}/{apiHash}")]
        [HttpPost]
        public async Task<IHttpActionResult> PostChangePassword(string useremail, string email, string apiHash, ChangePasswordViewModel model)//(IEnumerable<HttpPostedFileBase> files)
        {
            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "App UserName cannot be empty"
                });
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }


            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }
            if (string.IsNullOrEmpty(model.OldPassword) || string.IsNullOrEmpty(model.NewPassword) || string.IsNullOrEmpty(model.ConfirmPassword) || (model.NewPassword != model.ConfirmPassword))
            {
                // Err: 400
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Please check the entered values and try again."
                });
            }
            #endregion

            try
            {
                var user = UserManager.FindByEmail(useremail);
                if (user != null)
                {
                    var result = await ChangePwd(user.Id, model.OldPassword, model.NewPassword);
                    if (Convert.ToBoolean(result))
                    {
                        return Ok(new { code = 1, msg = "ok" });
                    }
                }

                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                return Ok(new { code = 0, msg = "not changed" });
            }
        }

        async Task<bool> ChangePwd(string userid, string oldpwd, string newpwd)
        {
            var result = await UserManager.ChangePasswordAsync(userid, oldpwd, newpwd);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return true; //true;
            }
            return false;
        }

        [Route("ChangeEmail/{email}/{apiHash}")]
        [HttpPost]
        public IHttpActionResult PostChangeEmail(string email, string apiHash, CompanyChangeModel model)//(IEnumerable<HttpPostedFileBase> files)
        {
            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "App UserName cannot be empty"
                });
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }
            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }
            if(model == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "No object was sent for operation."
                });
            }
            #endregion

            try
            {
                var comp = _compRep.FindBy(a => a.Id == model.CompanyId).FirstOrDefault();
                if(comp == null)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "Company not found."
                    });
                }
                string oldEmail = comp.User_Id;
                var oldName = comp.Name;
                bool change = false;
                bool emailChanged = false;
                if (!string.IsNullOrEmpty(model.NewEmail) && model.NewEmail.ToLower() != comp.User_Id.ToLower())
                {
                    comp.User_Id = model.NewEmail;
                    emailChanged = true;
                }
                if (!string.IsNullOrEmpty(model.Name) && model.Name.ToLower() != comp.Name.ToLower())
                {
                    comp.Name = model.Name;
                    change = true;
                }
                if (!string.IsNullOrEmpty(model.RC_Number) && model.RC_Number.ToLower() != comp.RC_Number.ToLower())
                {
                    comp.RC_Number = model.RC_Number;
                    change = true;
                }
                if (!string.IsNullOrEmpty(model.Business_Type) && model.Business_Type.ToLower() != comp.Business_Type.ToLower())
                {
                    comp.Business_Type = model.Business_Type;
                    change = true;
                }


                if (emailChanged)
                {
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
                        user = new ApplicationUser { UserName = model.NewEmail, Email = model.NewEmail, PhoneNumber = comp.Contact_Phone, PhoneNumberConfirmed = true };
                        var result = UserManager.Create(user, $"{model.NewEmail}1");
                        if (result.Succeeded)
                        {
                            var x = UserManager.AddToRole(user.Id, "Company");
                        }
                        #endregion
                    }

                    var code = UserManager.GenerateEmailConfirmationToken(user.Id);

                    #region send Re-Activation Link

                    //send activation link
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = resp.userId, code = code, appId = ELPSAPIHelper.PublicKey }, protocol: Request.Url.Scheme);
                    var callbackUrl = string.Format("{0}/Account/ConfirmEmail?userId={1}&code={2}&appId={3}", ConfigurationManager.AppSettings["myBaseUrl"], user.Id, code.Replace("+", "%2B"), app.PublicKey);
                    var body = "";
                    using (var sr = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath(@"\\App_Data\\Templates\") + "ComfirmEmailChange.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    var msgBody = string.Format(body, model.Name, callbackUrl, "Confirm Your NUPRC ELPS Account");
                    MailHelper.SendEmail(model.NewEmail, "Confirm Your NUPRC ELPS Account", msgBody);

                    var msg = new Message();
                    msg.Company_Id = comp.Id;
                    msg.Content = msgBody;
                    msg.Date = UtilityHelper.CurrentTime;
                    msg.Read = 0;
                    msg.Subject = "Confirm Your NUPRC ELPS Account";
                    msg.Sender_Id = "Application";

                    _msgRep.Add(msg);
                    _msgRep.Save(model.NewEmail, HttpContext.Current.Request.UserHostAddress);
                    #endregion
                    
                    _compRep.Edit(comp);
                    _compRep.Save(model.NewEmail, HttpContext.Current.Request.UserHostAddress);
                    
                    var cn = new CompanyNameHistory();
                    cn.CompanyId = comp.Id;
                    cn.Date = DateTime.Now;
                    cn.NewName = $"Company New Email: {model.NewEmail}";
                    cn.OldName = "Company Old Email: " + oldEmail;
                    cn.EditedBy = User.Identity.Name;


                    _compHistRep.Add(cn);
                    _compHistRep.Save(User.Identity.Name, HttpContext.Current.Request.UserHostAddress);

                    return Ok(new { responseCode = 1 });
                }
                else if (change)
                {
                    _compRep.Edit(comp);
                    _compRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                    if (oldName.ToLower() != comp.Name.ToLower())
                    {
                        var cn = new CompanyNameHistory();
                        cn.CompanyId = comp.Id;
                        cn.Date = DateTime.Now;
                        cn.NewName = $"Company New Name: {comp.Name}";
                        cn.OldName = "Company Old Name: " + oldName;
                        cn.EditedBy = User.Identity.Name;
                        _compHistRep.Add(cn);
                        _compHistRep.Save(User.Identity.Name, HttpContext.Current.Request.UserHostAddress);
                    }
                    return Ok(new { responseCode = 1 });
                }
                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                return Ok(new { responseCode = 0, message = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message });
            }
        }

        [Route("Login/{accEmail}/{email}/{apiHash}")]
        public IHttpActionResult GetLogin(string accEmail, string email, string apiHash)
        {
            #region
            if (string.IsNullOrEmpty(email))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "App UserName cannot be empty"
                });
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion
            

            #region Method Logic
            //Get Currently Logged in user
            if (MvcApplication.CurrentUser != null && MvcApplication.CurrentUser.IsAuthenticated)
            {
                var currentUser = MvcApplication.CurrentUser.Name;
                if (!string.IsNullOrEmpty(currentUser))
                {
                    //There is a user logged in
                    if (currentUser.ToLower() == accEmail.ToLower())
                    {
                        return Ok(new { code = "01" });
                    }
                }
            }

            #endregion

            return Ok(new { code = "00", message = "Invalid login request" });
        }

        [ResponseType(typeof(List<Staff>))]
        [Route("Staff/{email}/{apiHash}")]
        public IHttpActionResult GetStaff(string email, string apiHash)
        {
            #region
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();

            #endregion

            List<Staff> staff = _staffRep.GetAll().ToList();
            //var Usrs = UserManager.Users.Where(C => C.Roles.Select(y => y.RoleId).Contains("45ea949b-c11f-4de0-9042-fb2e8d12d89d")).ToList();
            //foreach (var usr in Usrs)
            //{
            //    staff.Add(new Staff() { UserId = usr.Id, Email = usr.Email });
            //}

            return Ok(staff);
        }

        [ResponseType(typeof(Staff))]
        [Route("Staff/{staffEmail}/{email}/{apiHash}")]
        public IHttpActionResult GetStaff(string staffEmail, string email, string apiHash)
        {
            #region
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();

            #endregion

            Staff staff = _staffRep.FindBy(a => a.Email.ToLower().Trim() == staffEmail.ToLower().Trim()).FirstOrDefault();

            return Ok(staff);
        }


    }
}