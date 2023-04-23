using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace ELPS.Helpers
{
    public class CompanyHelper
    {
        ICompanyRepository _coyRep;
        IApplicationRepository _appRep;

        public CompanyHelper(ICompanyRepository coy, IApplicationRepository apprep)
        {
            _appRep = apprep;
            _coyRep = coy;
        }

        public Company MyCompany(string user)
        {
            var myCoy = _coyRep.FindBy(c => c.User_Id.ToLower() == user).FirstOrDefault();
            return myCoy;
        }

        private ApplicationStageResponse GetStage(Company coy, vAddress[] CompAddress, List<Company_Director> CompDirs, List<Company_Key_Staff> CompStaff)
        {
            var stage = new ApplicationStageResponse();

            if (string.IsNullOrEmpty(coy.Contact_FirstName) || string.IsNullOrEmpty(coy.Contact_LastName) || string.IsNullOrEmpty(coy.Mission_Vision))
            {
                // This means Company profile has not been filled
                stage.Message = "Company Profile not completed. Please complete your company profile information to continue your application";
                stage.stage = "1";
            }
            else if ((CompAddress[0] == null || (string.IsNullOrEmpty(CompAddress[0].address_1) || CompAddress[0].StateId <= 0)))
            {
                // No Address had been registered to this company OR The address is not properly completed
                stage.Message = "No address found on your application or the Address is not completed properly. Please add your company address information to continue your application";
                stage.stage = "2";
            }
            else if (CompDirs.Count() <= 0)
            {
                // No Directors had been registered to this company
                stage.Message = "No Directors added to company profile. Please add at least one Director to your company to continue your application";
                stage.stage = "3";
            }
            else if (CompStaff.Count() <= 0)
            {
                // No KeyStaff had been registered to this company
                stage.Message = "Key Staff not added to company profile. Please add at least one Key Staff to your company to continue your application";
                stage.stage = "4";
            }
            else
            {
                stage.Message = "";
                stage.stage = "";
            }

            return stage;
        }

        public struct ApplicationStageResponse
        {
            public string stage { get; set; }
            public string Message { get; set; }
        }

        public int AppsInProcessing(int coyId, int lid = 0)
        {
            //WebClient client = new WebClient();
            //var url = "http://localhost:22000/Application/ApplicationInProcessing/";
            //url += (coyId != null && coyId > 0 ? coyId.ToString() : "").ToString();
            //var output = client.DownloadString(url);
            //var addresses = JsonConvert.DeserializeObject<List<AddressModelAPI>>(output);
            var dApps = new List<Application>();
            if(lid > 0)
            {
                dApps = _appRep.FindBy(a => a.CompanyId == coyId && a.Status == "processing" && a.LicenseId == lid).ToList();
            }
            else
            {
                // Get All
                dApps = _appRep.FindBy(a => a.CompanyId == coyId && a.Status == "processing").ToList();
            }

            return Convert.ToInt32(dApps.Count());
        }

        public int CoyApps(int coyId)
        {
            var dApps = _appRep.FindBy(a => a.CompanyId == coyId).ToList();

            return Convert.ToInt32(dApps.Count());
        }
    }
}