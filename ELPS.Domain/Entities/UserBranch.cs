namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("userbranches")]
    public partial class UserBranch : EntityBase
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        public string UserId { get; set; }
       [Required]
        public int RoleId { get; set; }
       [Required]
        public int DepartmentId { get; set; }
       public int DeskCount { get; set; }
        public bool Active { get; set; }
    }
}
