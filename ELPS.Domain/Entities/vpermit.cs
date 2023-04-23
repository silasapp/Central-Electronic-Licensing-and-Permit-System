namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    //[Table("permit")]
    public partial class vPermit : EntityBase
    {
        [Required, Display(Name = "Permit Number")]
        [StringLength(50)]
        public string Permit_No { get; set; }

        public string OrderId { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

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
        [Display(Name = "License Short Name")]
        public string LicenseShortName { get; set; }
        [Display(Name = "Business Type")]
        public string Business_Type { get; set; }
        public string RRR { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }

        //public int Fees { get; set; }

        //[NotMapped]
        //[Display(Name="Job Specification(s)")]
        //public List<vApplicationJobSpecification> Job_Specifications { get; set; }
        //[NotMapped]
        //[Display(Name="Service(s)")]
        //public List<vApplicationService> Services { get; set; }

        [NotMapped]
        public string JS_Combined { get; set; }
        [NotMapped]
        public bool Expired
        {
            get
            {
                return this.Date_Expire < DateTime.UtcNow.AddHours(1) ? true : false;
            }
        }
    }
}
