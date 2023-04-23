using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ELPS.Domain.Entities
{
    public class vAccountReport : EntityBase
    {
        public DateTime TransactionDate { get; set; }
        public string Transaction_Date { get; set; }
        public string Reference_Number { get; set; }
        public string Approved_Amount { get; set; }
        public string Transaction_Amount { get; set; }
        public string Transaction_Currency { get; set; }
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public string ServiceCharge { get; set; }
        public string RRR { get; set; }
        public string Status { get; set; }
        public string CategoryName { get; set; }
        public string LicenseName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string RC_Number { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public int StateId { get; set; }
        public string ApplicationItem { get; set; }

        /// <summary>
        /// Total paid by customer
        /// </summary>
        [NotMapped]
        public double GrossAmount { get; set; } // { return double.Parse(Transaction_Amount); } }
        /// <summary>
        /// Amount collected by contractor
        /// </summary>
        


        [NotMapped]
        public double NetAmount
        {
            get; set;
        }
        [NotMapped]
        public string Currency
        {
            get
            {
                return Transaction_Currency == "566" ? "NGN" : "";
            }
        }

    }

}