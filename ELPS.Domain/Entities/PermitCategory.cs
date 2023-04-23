using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class PermitCategory : EntityBase
    {
        public int AppIdentityId { get; set; }
        public string Name { get; set; }

    }
}
