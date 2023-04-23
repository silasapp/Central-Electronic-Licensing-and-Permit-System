using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class vZone : EntityBase
    {
		public string Name { get; set; }
		public string BranchName { get; set; }
		public string Code { get; set; }
		public int BranchId { get; set; }
        public int StateId { get; set; }
        public string Address { get; set; }

        public string StateName { get; set; }

        public string CountryName { get; set; }

        public int CountryId { get; set; }
        
        [NotMapped]
        [Display(Name = "Covered States")]
        public List<vZoneState> CoveredStates { get; set; }

        [NotMapped]
        [Display(Name = "Covered Field Offices")]
        public List<vBranch> CoveredFieldOffices { get; set; }

    }
}
