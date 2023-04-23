using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELPS.Domain.Entities
{
    public partial class AspNetUser
    {
       
      

        [Key]
        public string Id { get; set; }
        public string Email { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public DateTime Date  { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }


    }
}
