namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("Document_Types")]
    public partial class Document_Type:EntityBase
    {        

        [Required]
        [StringLength(250)]
        public string Name { get; set; }
        public string Type { get; set; }

        [NotMapped]
        public bool Selected { get; set; }

        [NotMapped]
        public bool ParentSelected { get; set; }
        [NotMapped]
        public string Document_Name { get; set; }
        [NotMapped]
        public string UniqueId { get; set; }
        [NotMapped]
        public string Source { get; set; }
        [NotMapped]
        public int CoyFileId { get; set; }
    }
}
