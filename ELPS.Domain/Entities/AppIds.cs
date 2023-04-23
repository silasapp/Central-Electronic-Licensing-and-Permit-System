using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELPS.Domain.Entities
{
    public class AppIdentity : EntityBase
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [Display(Name = "License Name")]
        public string LicenseName { get; set; }
        [Display(Name = "Short Name")]
        public string ShortName { get; set; }
        public string Description { get; set; }
        [Required]
        [Display(Name = "Base Url")]
        public string BaseUrl { get; set; }
        [Required]
        public string AppId { get; set; }
        public string ReceiptCode { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        [Display(Name = "Bank Payment EndPoint")]
        public string BankPaymentEndPoint { get; set; }
        [Required]
        [Display(Name = "Login Redirect")]
        public string LoginRedirect { get; set; }
        [Required]
        [Display(Name = "Permit Link")]
        public string PermitLink { get; set; }
        [Required]
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; }
        public Guid PublicKey { get; set; }
        public bool? OfficeUse { get; set; }
        public bool LoginByPass { get; set; }

        [Display(Name ="Divisions")]
        [NotMapped]
        public IEnumerable<int> CategoryId { get; set; }
        public List<PortalToDivision> PortalToDivisions { get; set; }

        [NotMapped]
        public int MyPermits { get; set; }

        [NotMapped]
        public int LicensesInProcessing { get; set; }
    }
}
