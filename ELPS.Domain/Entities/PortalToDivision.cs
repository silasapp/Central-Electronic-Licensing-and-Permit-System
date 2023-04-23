using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class PortalToDivision
    {
        public int PortalId { get; set; }
        public int DivisionId { get; set; }
        [ForeignKey("PortalCategoryId")]
        public virtual Division Division { get; set; }
        [ForeignKey("PortalId")]
        public virtual AppIdentity Portal { get; set; }
    }
}
