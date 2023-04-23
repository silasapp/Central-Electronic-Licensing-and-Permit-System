using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class vLockedOutUser:EntityBase
    {
        public string Email { get; set; }
        public string UserId { get; set; }
        public bool Resolved { get; set; }
        public string Reason { get; set; }
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        // dbo.LockedOutUsers.Id, dbo.AspNetUsers.Email, dbo.LockedOutUsers.Resolved, dbo.LockedOutUsers.Reason, dbo.LockedOutUsers.UserId


    }
}
