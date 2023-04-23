using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELPS.Helpers
{
    public class ExtApplicationHelper
    {
        ICompanyRepository _coyRep;
        ILicenseRepository _licenseRep;
        IAppIdentityRepository _appIdRep;
        public ExtApplicationHelper(ICompanyRepository coy, ILicenseRepository license, IAppIdentityRepository appIdRep)
        {
            _coyRep = coy;
            _licenseRep = license;
            _appIdRep = appIdRep;
        }

        public bool IsLocalAppUrl(string url)
        {
            var licenses = _appIdRep.GetAll().ToList();// _licenseRep.GetAll().ToList();
            bool ans = false;

            foreach (var item in licenses)
            {
                if (url.ToLower().Trim().StartsWith(item.BaseUrl.ToLower()))
                {
                    ans = true;
                    break;
                }
            }

            return ans;
        }
    }
}