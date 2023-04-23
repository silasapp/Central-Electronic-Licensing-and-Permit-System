namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("company_documents")]
    public partial class Company_Document:EntityBase
    {        

        public int Company_Id { get; set; }

        public int Document_Type_Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Source { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Date_Modified { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Date_Added { get; set; }

        public bool Status { get; set; }
        public bool Archived { get; set; }
        public string UniqueId { get; set; }

        public string Document_Name { get; set; }

        public DateTime? Expiry_Date { get; set; }
    }
}
