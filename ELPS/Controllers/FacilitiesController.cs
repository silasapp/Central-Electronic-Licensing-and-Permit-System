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
using System.Web;
using ELPS.Models;
using System.Configuration;

namespace ELPS.Controllers
{
    [RoutePrefix("api/Facility")]
    public class FacilitiesController : ApiController
    {
        IvFacilityRepository _vFacRep;
        IFacilityRepository _facRep;
        ICompanyRepository _compRep;
        IvAddressRepository _vAddRep;
        IAppIdentityRepository _appIdRep;
        WebApiAccessHelper accessHelper;

        public FacilitiesController(IAppIdentityRepository appIdRep, IvFacilityRepository vFacRep, ICompanyRepository compRep, IvAddressRepository vAddRep,
            IFacilityRepository facRep)
        {
            _facRep = facRep;
            _appIdRep = appIdRep;
            _vAddRep = vAddRep;
            _compRep = compRep;
            _vFacRep = vFacRep;
            accessHelper = new WebApiAccessHelper(appIdRep);
        }

        /// <summary>
        /// Get Facility Information
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(FacilityVM))]
        [Route("~/api/GetFacility")] //{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult PostFacility(string email, string code, string id)
        {
            string errStr = "error";

            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                return Ok(new CanAccessResponse() { Code = 2, Status = false, Message = "Access denied: Invalid App Username. Contact NUPRC Development team" });
            }
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null) // || !app.IsActive)
            {
                return Ok(new CanAccessResponse() { Code = 3, Status = false, Message = "App has been denied Access, Contact NUPRC Development team" });
            }

            if (!HashManager.compair(email, app.AppId, code))
            {
                return Ok(new CanAccessResponse() { Code = 4, Status = false, Message = "App has been denied Access, Contact NUPRC Development team" });
            }
            #endregion

            #region
            //Check Authenticity of the caller prefix 
            var prefix = "DPR/ELPS/F/";
            var facId = Convert.ToInt32(id.Trim().Replace(prefix, "").Trim());

            var facility = _vFacRep.FindBy(a => a.Id == facId).FirstOrDefault();
            if (facility != null)
            {
                var fac = new FacilityVM()
                {
                    UniqueNo = prefix + facId.ToString("000000"),
                    FacilityName = facility.Name,
                    Type = facility.FacilityType
                };
                var fAdd = new AddressVM() { City = facility.City, State = facility.StateName, StreetAddress = facility.StreetAddress };
                var comp = _compRep.FindBy(a => a.Id == facility.CompanyId).FirstOrDefault();
                var add = _vAddRep.FindBy(a => a.Id == comp.Registered_Address_Id).FirstOrDefault();
                var cAdd = new AddressVM() { City = add.City, State = add.StateName, StreetAddress = add.address_1 };

                fac.Company = new FacilityCompany()
                {
                    Address = cAdd,
                    ContactName = comp.Contact_FirstName + " " + comp.Contact_LastName,
                    Phone = comp.Contact_Phone,
                    Email = comp.User_Id,
                    Name = comp.Name
                };
                fac.Address = fAdd;

                return Ok(fac);
            }
            else
                errStr = "-1: Facility not found or does not exist";

            #endregion

            return Ok(errStr);
        }

        /// <summary>
        /// Updating Facility information especially for Take Over in ROMS
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [ResponseType(typeof(Facility))]
        [Route("EditFacility/{email}/{code}")]
        public IHttpActionResult PutFacility(string email, string code, Facility model)
        {
            string errStr = "error";

            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                return Ok(new CanAccessResponse() { Code = 2, Status = false, Message = "Access denied: Invalid App Username. Contact NUPRC Development team" });
            }
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null) // || !app.IsActive)
            {
                return Ok(new CanAccessResponse() { Code = 3, Status = false, Message = "App has been denied Access, Contact NUPRC Development team" });
            }

            if (!HashManager.compair(email, app.AppId, code))
            {
                return Ok(new CanAccessResponse() { Code = 4, Status = false, Message = "App has been denied Access, Contact NUPRC Development team" });
            }
            #endregion

            #region
            var facility = _facRep.FindBy(a => a.Id == model.Id).FirstOrDefault();
            if (facility != null)
            {
                facility.City = model.City;
                facility.CompanyId = model.CompanyId;
                facility.FacilityType = model.FacilityType;
                facility.LGAId = model.LGAId;
                facility.Name = model.Name;
                facility.StateId = model.StateId;
                facility.StreetAddress = model.StreetAddress;

                _facRep.Edit(facility);
                _facRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                return Ok(facility);
            }
            else
                errStr = "Facility not found or does not exist";

            #endregion

            return Ok(errStr);
        }
    }
}