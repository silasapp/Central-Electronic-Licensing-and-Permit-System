namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

   
    public partial class vPaymentTransaction : EntityBase
    {
        [StringLength(50)]
        public string Type { get; set; }


        [StringLength(50)]
        public string Transaction_Date { get; set; }


        [StringLength(50)]
        public string Reference_Number { get; set; }

        [StringLength(100)]
        public string Payment_Reference { get; set; }


        [StringLength(10)]
        public string Approved_Amount { get; set; }

        [StringLength(100)]
        public string Response_Description { get; set; }


        [StringLength(50)]
        public string Response_Code { get; set; }


        [StringLength(10)]
        public string Transaction_Amount { get; set; }


        [StringLength(5)]
        public string Transaction_Currency { get; set; }


        [StringLength(200)]
        public string CompanyName { get; set; } 

        public int CompanyId { get; set; }

        [Required]
        [StringLength(20)]
        public string Order_Id { get; set; }
        public string ServiceCharge { get; set; }
        public string RRR { get; set; }

        public bool Completed { get; set; }
        public string ApplicationItem { get; set; }
        public string CategoryName { get; set; }
        public int LicenseId { get; set; }
        public string ApplicationStatus { get; set; }
        public string LicenseShortName { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        [NotMapped]
        public List<ApplicationItem> ApplicationItems { get; set; }
        public DateTime TransactionDate { get; set; }


        //[NotMapped]
        //public DateTime? TransactionDate
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(this.Transaction_Date))
        //        {
        //            return DateTime.Parse(this.Transaction_Date);
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}
    }
}
