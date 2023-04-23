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
    [RoutePrefix("api/Licenses")]
    public class LicensesController : ApiController
    {
        IPermitRepository _licenseRep;
        //IvFacilityRepository _vFacRep;
        //IFacilityRepository _facRep;
        //ICompanyRepository _compRep;
        //IvAddressRepository _vAddRep;
        //IAppIdentityRepository _appIdRep;
        //WebApiAccessHelper accessHelper;

        public LicensesController(IPermitRepository licenseRep)//IAppIdentityRepository appIdRep, IvFacilityRepository vFacRep, ICompanyRepository compRep, IvAddressRepository vAddRep,
            //IFacilityRepository facRep)
        {
            _licenseRep = licenseRep;

            //_facRep = facRep;
            //_appIdRep = appIdRep;
            //_vAddRep = vAddRep;
            //_compRep = compRep;
            //_vFacRep = vFacRep;
            //accessHelper = new WebApiAccessHelper(appIdRep);
        }

        /// <summary>
        /// Get License Validity
        /// </summary>
        [ResponseType(typeof(string))]
        //[Route("Check")] //{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult GetCheckLicense(string licenseno)
        {
            #region
            //Check Authenticity of the caller prefix 
            
            var license = _licenseRep.FindBy(a => a.Permit_No.ToLower() == licenseno.ToLower()).FirstOrDefault();
            bool valid = false;

            if (license != null)
            {
                if(license.Date_Expire > UtilityHelper.CurrentTime)
                {
                    return Ok("valid");
                }
                else
                {
                    return Ok("expired");
                }
            }
            #endregion

            return Ok("Not valid");
        }
        
    }
}