using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELPS.Domain.Entities
{
    public class ExternalAppIdentity: EntityBase
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string AppId { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; }
        public Guid PublicKey { get; set; }

        [NotMapped]
        public int LicensesInProcessing { get; set; }
    }
}
