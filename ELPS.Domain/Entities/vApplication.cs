using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class vApplication : EntityBase
    {

        public string OrderId { get; set; }
        public string Status { get; set; }
        public string CategoryName { get; set; }
        public int CompanyId { get; set; }
        public int LicenseId { get; set; }
        public DateTime Date { get; set; }
        public string LicenseName { get; set; }
        public string LicenseShortName { get; set; }
        public string CompanyName { get; set; }
        public bool Submited { get; set; }

        public string ServiceCharge { get; set; }
        public string transaction_amount { get; set; }
        public string approved_amount { get; set; }
        public string RRR { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public string ApplicationItem { get; set; }
        [NotMapped]
        public List<ApplicationItem> ApplicationItems { get; set; }
    }
}
