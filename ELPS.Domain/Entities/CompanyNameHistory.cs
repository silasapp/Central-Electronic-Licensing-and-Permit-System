using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class CompanyNameHistory:EntityBase
    {
        public int CompanyId { get; set; }
        public string NewName { get; set; }
        public string OldName { get; set; }
        public DateTime Date { get; set; }
        public string EditedBy { get; set; }
    }
}
