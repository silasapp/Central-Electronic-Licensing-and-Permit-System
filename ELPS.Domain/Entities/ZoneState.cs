using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class ZoneState : EntityBase
    {
        public int ZoneId { get; set; }
		public int StateId { get; set; }
        public int FDId { get; set; }
    }
}
