using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class LockedOutUser : EntityBase
    {
       public string UserId { get; set; }
       public bool Resolved { get; set; }
       public string Reason { get; set; }

    }
}
