namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
   

    public partial class Category:EntityBase
    {

        public int MaxClassification { get; set; }
        public int MaxService { get; set; }

        [Required]
        [StringLength(70), Display(Name="Category Name")]
        public string Name { get; set; }

        [Display(Name="Fee Payable")]
        public int Price { get; set; }
        [Display(Name="Service Charge")]
        public int  ServiceCharge { get; set; }

        [NotMapped]
        public int PermitCount { get; set; }
        [NotMapped]
        public int ApplicationCount { get; set; }
    }
}
