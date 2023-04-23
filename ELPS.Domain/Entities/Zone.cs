using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class Zone : EntityBase
    {
        public int Country_Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Code { get; set; }

        public short Status { get; set; }
        public int BranchId { get; set; }
    }
}
