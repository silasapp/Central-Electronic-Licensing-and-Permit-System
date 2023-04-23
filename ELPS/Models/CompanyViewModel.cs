using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class CompanyViewModel
    {
        //public Company CompanyView { get; set; }
        //public List<Company_MedicalView> CoyMedView { get; set; }
        //public List<Company_Expatriate_QuotaView> CoyExpQView { get; set; }
        //public List<Company_NsitfView> CoyNsitfView { get; set; }
        //public List<Company_ProffessionalView> CoyProfView { get; set; }
        //public List<Company_Technical_AgreementView> CoyTechAgreeView { get; set; }

        public Company CompanyView { get; set; }
        public List<vCompanyMedical> CoyMedView { get; set; }
        public List<vCompanyExpatriateQuota> CoyExpQView { get; set; }
        public List<vCompanyNsitf> CoyNsitfView { get; set; }
        public List<vCompanyProffessional> CoyProfView { get; set; }
        public List<vCompanyTechnicalAgreement> CoyTechAgreeView { get; set; }
    }
}