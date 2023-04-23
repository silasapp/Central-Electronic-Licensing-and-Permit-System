namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("nsitf")]
    public partial class Nsitf:EntityBase
    {        

        [Required]
        [StringLength(250)]
        public string Name { get; set; }
    }
}
