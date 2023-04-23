namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("Key_Staff_Certificates")]
    public partial class Key_Staff_Certificate : EntityBase
    {
        public int Company_Key_Staff { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Cert_No { get; set; }

        public int Year { get; set; }
        public string Issuer { get; set; }
    }
}
