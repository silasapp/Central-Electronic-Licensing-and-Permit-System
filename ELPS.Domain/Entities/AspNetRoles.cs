using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class AspNetRoles
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
