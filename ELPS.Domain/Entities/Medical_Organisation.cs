namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    public partial class Medical_Organisation:EntityBase
    {        

        [Required]
        [StringLength(250)]
        public string Name { get; set; }
    }
}
