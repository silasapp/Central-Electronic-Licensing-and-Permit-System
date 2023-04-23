namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("country")]
    public partial class Country:EntityBase
    {        

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        [StringLength(2)]
        public string Iso_Code_2 { get; set; }

        [Required]
        [StringLength(3)]
        public string Iso_Code_3 { get; set; }

        [Required]
        public string Address_Format { get; set; }

        public short PostCode_Required { get; set; }

        public short Status { get; set; }
    }
}
