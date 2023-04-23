namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    //[Table("file")]
    public partial class File:EntityBase
    {        


        public int Sort_Order { get; set; }

        [Required]
        [StringLength(10)]
        public string Size { get; set; }

        [Required]
        [StringLength(100)]
        public string Mime { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Source { get; set; }


        //public bool Archived { get; set; }
    }
}
