using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELPS.Helpers
{
    public class WebApiAccessHelper
    {
        IAppIdentityRepository _appIdRep;

        public WebApiAccessHelper(IAppIdentityRepository appIdRep)
        {
            _appIdRep = appIdRep;
        }

        public CanAccessResponse CanAccess(string appEmail, string apiHash)
        {
            #region Initial Check
            if (string.IsNullOrEmpty(appEmail))
            {
                return new CanAccessResponse() { Code = 2, Status = false, Message = "Access denied: Invalid App Username. Contact NUPRC Development team" };
            }
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == appEmail.ToLower().Trim()).FirstOrDefault();
            if (app == null || !app.IsActive)
            {
                return new CanAccessResponse() { Code = 3, Status = false, Message = "App has been denied Access, Contact NUPRC Development team" };
            }
            
            if (!HashManager.compair(appEmail, app.AppId, apiHash))
            {
                return new CanAccessResponse() { Code = 4, Status = false, Message = "App has been denied Access, Contact NUPRC Development team" };
            }

            return new CanAccessResponse() { Code = 1, Status = true, Message = "" };
            
            #endregion
        }
    }

    public class CanAccessResponse
    {
        public bool Status { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }

    public enum ResponseCodes
    {        
        Success = 1,
        Pass = 0,
        Failure = -1,
        NotFound = -2,
        Invalid = -3,
        ServerError = -4,
        NoContent = -5,
        Forbidden = -6,
        BadRequest = -7
    }
}