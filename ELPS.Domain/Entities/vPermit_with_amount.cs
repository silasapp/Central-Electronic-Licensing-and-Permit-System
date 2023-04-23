namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    [Table("vPermit_with_amounts")]
    public partial class vPermit_with_amount : EntityBase
    {
        [Required, Display(Name = "Permit Number")]
        [StringLength(30)]
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
        //[Display(Name = "Business Type")]
        //public string Business_Type { get; set; }
        public string RRR { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public string Transaction_Amount { get; set; }
        public string Approved_Amount { get; set; }


        [NotMapped]
        public decimal OtherFees
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Transaction_Amount))
                    return (Convert.ToDecimal(this.Transaction_Amount) - this.Fee);
                else
                    return 0;
            }
        }

        [NotMapped]
        public decimal Fee
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Approved_Amount))
                    return Convert.ToDecimal(this.Approved_Amount);
                else
                    return 0;
            }
        }
        
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
