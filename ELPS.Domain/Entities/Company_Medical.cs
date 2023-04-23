namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("Company_Medicals")]
    public partial class Company_Medical:EntityBase
    {
        [Required]
        [StringLength(250)]
        public string Medical_Organisation { get; set; }

        //[Required]
        [StringLength(250)]
        public string Address { get; set; }

        //[Required]
        [StringLength(20)]
        public string Phone { get; set; }

        //[Required]
        [StringLength(200)]
        public string Email { get; set; }

        public int Company_Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date_Issued { get; set; }
        public int? FileId { get; set; }

        [NotMapped]
        public int Elps_Id { get; set; }
    }
}
