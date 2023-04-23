namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("Company_Nsitfs")]
    public partial class Company_Nsitf:EntityBase
    {        

        public int No_People_Covered { get; set; }

        [Required]
        [StringLength(100)]
        public string Policy_No { get; set; }
        public int? FileId { get; set; }
        public int Company_Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date_Issued { get; set; }
        [NotMapped]
        public int Elps_Id { get; set; }
    }
}
