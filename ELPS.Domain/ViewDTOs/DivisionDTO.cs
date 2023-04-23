using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.ViewDTOs
{
    class DivisionDTO
    {
    }
    public class DivisionViewWithPortalDTO
    {
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public IEnumerable<AppIdentity> Portals { get; set; }
    }
}
