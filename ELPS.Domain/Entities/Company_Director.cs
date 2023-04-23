namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    [Table("company_directors")]
    public partial class Company_Director : EntityBase
    {
        public int Company_Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public int Address_Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Telephone { get; set; }

        [Required]
        public int Nationality { get; set; }

        [NotMapped]
        public Address  Address { get; set; }
    }
}
