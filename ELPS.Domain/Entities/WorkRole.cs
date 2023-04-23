namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("address")]
    public partial class WorkRole:EntityBase
    {        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

       
    }
}
