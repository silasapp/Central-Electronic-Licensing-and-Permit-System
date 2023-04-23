using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class ApplicationRequirement:EntityBase
    {
        public int TransactionId { get; set; }
        public string Name { get; set; }
        public string   Description { get; set; }
    }
}
