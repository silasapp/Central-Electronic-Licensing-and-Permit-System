using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class vUserBranch : EntityBase
    {
        public int BranchId { get; set; }
        //[Required] Email
        public string UserId { get; set; }
        // [Required]
        public int RoleId { get; set; }
        // [Required]
        public int DepartmentId { get; set; }
        public int DeskCount { get; set; }
        public string DepartmentName { get; set; }
        public string BranchName { get; set; }
        public string location { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; }
        //[NotMapped]
        public string UserBranch()
        {
            return string.Format("{0}({1}) : {2}", BranchName, DepartmentName, location);
        }
        [NotMapped]
        public string FirstName { get; set; }
        [NotMapped]
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
