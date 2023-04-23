namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("FacilityDocuments")]
    public partial class FacilityDocument:EntityBase
    {        

        public int Company_Id { get; set; }

        public int Document_Type_Id { get; set; }

        public int FacilityId { get; set; }
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Source { get; set; }
        public DateTime Date_Modified { get; set; }
        public DateTime Date_Added { get; set; }

        public bool Status { get; set; }
        public bool Archived { get; set; }
        public string UniqueId { get; set; }

        public string Document_Name { get; set; }

        public DateTime? Expiry_Date { get; set; }
    }
}
