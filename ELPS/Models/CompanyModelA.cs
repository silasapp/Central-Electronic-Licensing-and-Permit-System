using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class CompanyModel
    {
        public Company Company { get; set; }
        //public List<Company_Medical> CompanyMedicals { get; set; }
        //public List<Company_Expatriate_Quota> CompanyExpatriateQuotas { get; set; }
        //public List<Company_Nsitf> CompanyNsitfs { get; set; }
        //public List<Company_Proffessional> CompanyProffessionals { get; set; }
        //public List<Company_Technical_Agreement> CompanyTechnicalAgreements { get; set; }

        public List<vCompanyMedical> CompanyMedicals { get; set; }
        public List<vCompanyExpatriateQuota> CompanyExpatriateQuotas { get; set; }
        public List<vCompanyNsitf> CompanyNsitfs { get; set; }
        public List<vCompanyProffessional> CompanyProffessionals { get; set; }
        public List<vCompanyTechnicalAgreement> CompanyTechnicalAgreements { get; set; }
    }
}