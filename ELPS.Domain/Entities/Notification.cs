namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("Notifications")]
    public partial class Notification:EntityBase
    {
        public string ToStaff { get; set; }
        public string Message { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
