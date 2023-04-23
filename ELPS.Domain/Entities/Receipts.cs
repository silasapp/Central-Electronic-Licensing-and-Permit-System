namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("invoice")]
    public partial class Receipt
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        //[StringLength(50)]
        [Display(Name = "Receipt Number")]
        public string ReceiptNo { get; set; }
        public int ApplicationId { get; set; }
        public long InvoiceId { get; set; }
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Display(Name = "Amount Paid")]
        public double Amount { get; set; }
        //[Required]
        //[StringLength(6)]
        public string Status { get; set; }
        //[Required]
        //[StringLength(100)]
        [Display(Name= "Application Reference")]
        public string ApplicationReference { get; set; }
        public string RRR { get; set; }
        [Display(Name = "Date Paid")]
        public DateTime Date_Paid { get; set; }
    }
}
