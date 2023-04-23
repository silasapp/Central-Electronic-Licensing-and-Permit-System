namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("Company_Expatriate_Quotas")]
    public partial class Company_Expatriate_Quota:EntityBase
    {        

        public int Company_Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        public int? FileId { get; set; }
        [NotMapped]
        public int Elps_Id { get; set; }
    }
}
