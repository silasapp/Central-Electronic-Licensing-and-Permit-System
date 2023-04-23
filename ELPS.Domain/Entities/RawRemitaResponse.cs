namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
   

    public class RawRemitaResponse : EntityBase
    {
        [Required]
        [StringLength(50)]
        public string Direction { get; set; }
        [Required]
        [StringLength(4000)]
        public string JsonData { get; set; }
        [Required]
        public DateTime DateAdded { get; set; }

    }
}
