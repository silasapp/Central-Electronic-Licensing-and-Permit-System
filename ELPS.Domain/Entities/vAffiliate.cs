using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELPS.Domain.Entities
{
    public class vAffiliate : EntityBase
    {
        public Guid UniqueId { get; set; }
        public int ParentId { get; set; }
        public int ChildId { get; set; }
        public DateTime DateAdded { get; set; }
        public string Code { get; set; }
        public bool? Approved { get; set; }
        public DateTime? DateConfirmed { get; set; }
        public string ParentCompany { get; set; }
        public string ChildCompany { get; set; }
    }
}