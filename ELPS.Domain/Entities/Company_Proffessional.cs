namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("Company_Proffessionals")]
    public partial class Company_Proffessional:EntityBase
    {        

        [Required]
        [StringLength(250)]
        public string Proffessional_Organisation { get; set; }

        //[Required]
        [StringLength(250)]
        public string Cert_No { get; set; }

        public int Company_Id { get; set; }
        public int? FileId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date_Issued { get; set; }
        [NotMapped]
        public int Elps_Id { get; set; }
    }
}
