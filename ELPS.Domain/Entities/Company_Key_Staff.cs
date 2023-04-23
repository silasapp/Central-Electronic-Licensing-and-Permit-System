namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    


    [Table("Company_Key_Staffs")]
    public partial class Company_Key_Staff:EntityBase
    {
        public int Company_Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string Nationality { get; set; }

        [Required]
        [StringLength(50)]
        public string Designation { get; set; }

        //[Required]
        public string Qualification { get; set; }

        [Display(Name="Year of Experience")]
        public int Years_Of_Exp { get; set; }

        //[Required]
        public string Skills { get; set; }

        [Display(Name="Training/Certificates")]
        public string Training_Certificates { get; set; }

        [NotMapped]
        public List<Key_Staff_Certificate> Certificates { get; set; }
    }
}
