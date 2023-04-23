namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("invoice")]
    public partial class Invoice
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int Application_Id { get; set; }
        [Display(Name = "Amount Paid")]
        public double Amount { get; set; }

        //[Required]
        //[StringLength(6)]
        public string Status { get; set; }

        //[Required]
        //[StringLength(100)]
        [Display(Name="Payment Code")]
        public string Payment_Code { get; set; }

        //[Required]
        //[StringLength(50)]
        [Display(Name = "Payment Type")]
        public string Payment_Type { get; set; }
        [Display(Name = "Date Opened")]
        public DateTime Date_Added { get; set; }
        [Display(Name = "Date Paid")]
        public DateTime Date_Paid { get; set; }
        public int PaymentTransaction_Id { get; set; }
    }
}
