namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("addresses")]
    public partial class Address:EntityBase
    {   

        [Required]
        [StringLength(128)]
        public string Address_1 { get; set; }

        [StringLength(128)]
        public string Address_2 { get; set; }

        [Required]
        [StringLength(128)]
        public string City { get; set; }

        //[Required]
        [StringLength(10)]
        public string Postal_Code { get; set; }
        public int StateId { get; set; }
        public int Country_Id { get; set; }

        [NotMapped]
        public string Type { get; set; }
    }
}
