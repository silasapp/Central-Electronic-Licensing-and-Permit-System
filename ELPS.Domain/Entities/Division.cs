using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class Division:EntityBase
    {
        public string Name { get; set; }
        [Required]
        public int SortOrder { get; set; }
        public List<PortalToDivision> DivisionToPortals { get; set; }
        [NotMapped]
        public virtual List<AppIdentity> Portals { get; set; }
    }
}
