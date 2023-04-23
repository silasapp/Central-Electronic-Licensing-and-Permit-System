using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.ViewDTOs
{
    public class VerifyPermitDTO
    {
        [Required(ErrorMessage ="Enter your permit number")]
        [Display(Name ="Permit/License Number")]
        public string LicenseNo { get; set; }
    }
}
