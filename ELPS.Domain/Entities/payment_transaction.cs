namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    [Table("payment_transactions")]
    public partial class Payment_Transaction:EntityBase
    {

        //[StringLength(50)]
        public string Type { get; set; }


        //[StringLength(50)]
        public string Transaction_Date { get; set; }


        //[StringLength(50)]
        public string Reference_Number { get; set; }

        //[StringLength(100)]
        public string Payment_Reference { get; set; }

       
        //[StringLength(10)]
        public string Approved_Amount { get; set; }

        //[StringLength(100)]
        public string Response_Description { get; set; }

       
        //[StringLength(50)]
        public string Response_Code { get; set; }

  
        //[StringLength(10)]
        public string Transaction_Amount { get; set; }

     
        //[StringLength(5)]
        public string Transaction_Currency { get; set; }

        
        //[StringLength(200)]
        public string CompanyName { get; set; }

        public int CompanyId { get; set; }

        //[Required]
        //[StringLength(20)]
        public string Order_Id { get; set; }
        
        public DateTime Query_Date { get; set; }

        public string ServiceCharge { get; set; }
        public string RRR { get; set; }

        public string PaymentSource { get; set; }
        public string ReturnSuccessUrl { get; set; }
        public string ReturnFailureUrl { get; set; }
        public string ReturnBankPaymentUrl { get; set; }
        public string ServiceTypeId { get; set; }
        public bool Completed { get; set; }
        public string ApplicationItem { get; set; }
        public string DocumentType { get; set; }
        public string BankPaymentEndPoint { get; set; }
        public string ManualValueMessage { get; set; }
        public DateTime? TransactionDate { get; set; }
        [NotMapped]
        public List<ApplicationItem> ApplicationItems { get; set; }
        [NotMapped]
        public List<Document_Type> Document_Types { get; set; }
    }
}
