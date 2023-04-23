using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class vZoneState : EntityBase
    {
        public int StateId { get; set; }
        public int ZoneId { get; set; }
        [Display(Name="State Name")]
        public string StateName { get; set; }
        [Display(Name = "Zone Name")]
        public string ZoneName { get; set; }
        public int FDId { get; set; }
    }

    public class vFD_FD : EntityBase
    {
        public int StateId { get; set; }
        public int ZoneId { get; set; }
        [Display(Name = "State Name")]
        public string StateName { get; set; }
        public int FDId { get; set; }
        public bool? IsFieldOffice { get; set; }
        [Display(Name = "FD Name")]
        public string FDName { get; set; }
        public string Address { get; set; }
    }
}
