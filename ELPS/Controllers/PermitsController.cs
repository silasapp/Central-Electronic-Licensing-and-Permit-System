using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ELPS.Domain.Abstract;
using System.Web.Http.Description;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using ELPS.Models;
using System.Web;

namespace ELPS.Controllers
{
    [RoutePrefix("api/Permits")]
    public class PermitsController : ApiController
    {

        IApplicationRepository _appRep;
        ICompanyRepository _compRep;
        IAppIdentityRepository _appIdRep;
        IPermitRepository _permitRep;
        IAddressRepository _addRep;
        IvAddressRepository _vAddRep;
        IvPermitRepository _vPermitRep;
        IExternalAppIdentityRepository _extAppRep;

        public PermitsController(ICompanyRepository compRep, IAppIdentityRepository appIdRep, IPermitRepository permitRep, IvAddressRepository vAddRep,
            IAddressRepository addRep, IApplicationRepository appRep, IvPermitRepository vPermitRep, IExternalAppIdentityRepository extAppRep)
        {
            _vAddRep = vAddRep;
            _extAppRep = extAppRep;
            _compRep = compRep;
            _appIdRep = appIdRep;
            _permitRep = permitRep;
            _addRep = addRep;
            _appRep = appRep;
            _vPermitRep = vPermitRep;

        }

        [ResponseType(typeof(List<vPermit>))]
        [Route("~/api/Permits/{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult GetPermits(int CompId, string email, string apiHash)
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
            //check if call is from the app Owner

            //var url = HttpContext.Current.Request.UserHostName;//HttpContext.Current.Request.Url.OriginalString;

            //if (url != app.Url)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}
            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion
            var pm = _vPermitRep.FindBy(a => a.Company_Id == CompId).ToList();

            return Ok(pm);
        }

        [ResponseType(typeof(List<vPermit>))]
        [Route("PermitList/{page:int}/{appId:int}")]
        public IHttpActionResult GetPermitList(int page, int appId) //, string email, string apiHash)
        {
            int skip = 1000 * (page - 1);
            var pm = _vPermitRep.FindBy(a => a.LicenseId == appId && a.CategoryName.Trim().ToLower() == "general").OrderBy(a => a.Id).Skip(skip).Take(1000).ToList();

            return Ok(pm);
        }

        [ResponseType(typeof(vPermit))]
        [Route("~/api/Permits/ById/{Id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetById(int Id, string email, string apiHash)
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
            //check if call is from the app Owner

            //var url = HttpContext.Current.Request.UserHostName;//HttpContext.Current.Request.Url.OriginalString;

            //if (url != app.Url)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}
            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion


            var pm = _vPermitRep.FindBy(a => a.Id == Id).FirstOrDefault();

            //if (pm == null)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
            //    {
            //        ReasonPhrase = "Item does not Exist"
            //    });
            //}
            return Ok(pm);
        }

        [ResponseType(typeof(Permit))]
        [Route("~/api/Permits/ByPermitNumber/{permitNo}/{email}/{apiHash}")]
        public IHttpActionResult GetKeyPermitNumber(string permitNo, string email, string apiHash)
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
            //check if call is from the app Owner

            //var url = HttpContext.Current.Request.UserHostName;//HttpContext.Current.Request.Url.OriginalString;

            //if (url != app.Url)
            //{

            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}
            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion


            var pm = _permitRep.FindBy(a => a.Permit_No == permitNo).FirstOrDefault();

            //if (pm == null)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
            //    {
            //        ReasonPhrase = "Item does not Exist"
            //    });
            //}
            return Ok(pm);
        }

        [ResponseType(typeof(Permit))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult PostPermit(int CompId, string email, string apiHash, Permit permit)
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
            //check if call is from the app Owner

            //var url = HttpContext.Current.Request.UserHostName;//HttpContext.Current.Request.Url.OriginalString;

            //if (url != app.Url)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}
            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion

            #region logic
            try
            {
                var pm = new Permit();
                if (permit != null)
                {
                    pm.OrderId = permit.OrderId;
                    pm.CategoryName = permit.CategoryName;
                    pm.Company_Id = permit.Company_Id;
                    pm.Date_Expire = permit.Date_Expire;
                    pm.Date_Issued = permit.Date_Issued;
                    pm.Is_Renewed = permit.Is_Renewed;
                    pm.LicenseId = app.Id;
                    pm.Permit_No = permit.Permit_No;


                    _permitRep.Add(pm);
                    _permitRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                    var appc = _appRep.FindBy(a => a.OrderId == pm.OrderId).FirstOrDefault();
                    if (appc != null)
                    {
                        appc.Status = ApplicationStatus.Approved.ToString();
                        _appRep.Edit(appc);
                        _appRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                    }
                }
                else
                {

                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "Address Model cannot be empty"
                    });
                }

                return Ok(pm);

            }
            catch (Exception)
            {

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "Some Error while handling your Request"
                });
            }
            #endregion
        }

        #region MyRegion

        // Code = SHA512 string of (pk + sk + licenseNo) all Lower case

        /// <summary>
        /// 
        /// </summary>
        /// <param name="licenseNo"></param>
        /// <param name="pk"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [ResponseType(typeof(PermitLookupModel))]
        [Route("~/api/Lookup/{pk}/{code}")]
        public IHttpActionResult GetExternalLookup(string licenseNo, string pk, string code)
        {
            #region Check the access first
            var extApp = _extAppRep.FindBy(a => a.PublicKey.ToString().ToLower() == pk.ToLower()).FirstOrDefault();
            if (extApp == null || !extApp.IsActive)
            {
                return Ok(new { message = "Unauthorized Access" });
            }
            var str = pk.Trim() + extApp.AppId + licenseNo.Trim().ToLower();
            var localCode = PaymentRef.getHash(str, true);
            if (localCode != code)
            {
                return Ok(new { message = "Unauthorized Access" });
            }
            #endregion

            try
            {

                var model = new PermitLookupModel();
                var permit = _vPermitRep.FindBy(a => a.Permit_No.ToLower() == licenseNo.ToLower()).FirstOrDefault();
                if (permit == null)
                {
                    return Ok(new { message = "Requested License/Permit not found" });
                }
                var coy = _compRep.FindBy(a => a.Id == permit.Company_Id).FirstOrDefault();
                if (coy == null)
                {
                    return Ok(new { message = "Requested License/Permit not found" });
                }
                var addid = coy.Operational_Address_Id == null || coy.Operational_Address_Id.Value == 0 ? coy.Registered_Address_Id.GetValueOrDefault(0) : coy.Operational_Address_Id.Value;
                var coyAdd = _vAddRep.FindBy(a => a.Id == addid).FirstOrDefault();

                model.CompanyName = permit.CompanyName;
                model.CompanyAddress = $"{coyAdd?.address_1} {coyAdd?.City}, {coyAdd?.StateName}, {coyAdd?.CountryName}";
                model.Email = coy.User_Id;
                model.ExpiryDate = permit.Date_Expire.ToShortDateString();
                model.Number = permit.Permit_No;

                return Ok(model);
            }
            catch (Exception)
            {
                return Ok(new { message = "An error occured." });
            }
        }
        #endregion
    }
}
