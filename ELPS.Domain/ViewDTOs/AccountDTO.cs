using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.ViewDTOs
{
    public abstract class AccountDTO
    {
        public AppIdentity SelectedPortal { get; set; }
        public IEnumerable<Division> Divisions { get; set; }
        public IEnumerable<AppIdentity> ActivePortals { get; set; }

    }
    public class AccountRegisterDTO : AccountDTO
    {
        [Required(ErrorMessage ="Enter the Email Address")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter the Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Enter the Business Name")]
        [Display(Name = "Business Name")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Enter the Business Registration No.")]
        [Display(Name = "Business Registration No")]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Select a Business Type")]
        [Display(Name = "Business Type")]
        public string BusinessType { get; set; }

        [Required(ErrorMessage = "Enter the Phone No.")]
        [Phone]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(13, ErrorMessage = "Phone input a valid phone number")]
        [MinLength(11, ErrorMessage = "Phone input a valid phone number")]
        public string PhoneNumber { get; set; }
    }
    public class AccountLoginDTO: AccountDTO
    {
        [Required(ErrorMessage = "Enter Email Address")]
        [EmailAddress(ErrorMessage = "Enter a valid Email Address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class AccountProcessDataDTO
    {
        public string Code { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
    }
}
