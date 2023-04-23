using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class Application:EntityBase
    {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string CategoryName { get; set; }
        public int CompanyId { get; set; }
        public int LicenseId { get; set; }
        public DateTime Date { get; set; }

        [NotMapped]
        public string LicenseName { get; set; }
    }
}
