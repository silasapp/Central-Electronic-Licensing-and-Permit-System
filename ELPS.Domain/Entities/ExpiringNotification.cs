namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
   

    public partial class ExpiringNotification : EntityBase
    {
        [Required]
        public string Permit_No { get; set; }
        [Required]
        public int NotificationCounter { get; set; }
        [Required]
        public DateTime DateLastNotified { get; set; }
    }
}
