namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    //[Table("permit")]
    public partial class vExpiringLicense : EntityBase
    {
        //,  CompanyEmail, 
        //                 contact_firstname, contact_phone
        public bool IsLicense { get; set; }
        [Required, Display(Name = "License Number")]
        [StringLength(30)]
        public string Permit_No { get; set; }
        [Required, Display(Name = "Reference")]
        public string OrderId { get; set; }
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Display(Name = "Company Email")]
        public string CompanyEmail { get; set; }
        [Display(Name ="Contact Name")]
        public string contact_firstname { get; set; }
        [Display(Name = "Contact Phone")]
        public string contact_phone { get; set; }
        public int Company_Id { get; set; }
        [Display(Name = "Issue Date")]
        public DateTime Date_Issued { get; set; }
        [Display(Name = "Expiry Date")]
        public DateTime Date_Expire { get; set; }
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        public int LicenseId { get; set; }
        [Display(Name = "License Name")]
        public string LicenseName { get; set; }
        [Display(Name = "License")]
        public string LicenseShortName { get; set; }
        [Display(Name = "Business Type")]
        public string Business_Type { get; set; }
        public string RRR { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public int? NotificationCounter { get; set; }
        public DateTime? DateLastNotified { get; set; }

        //public int Fees { get; set; }
        //[NotMapped]
        //[Display(Name="Job Specification(s)")]
        //public List<vApplicationJobSpecification> Job_Specifications { get; set; }
        //[NotMapped]
        //[Display(Name="Service(s)")]
        //public List<vApplicationService> Services { get; set; }

        [NotMapped]
        public string JS_Combined { get; set; }
    }
}
