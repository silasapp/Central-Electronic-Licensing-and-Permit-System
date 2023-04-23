namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("branch")]
    public partial class Branch:EntityBase
    {        

        [Required]
        public string Name { get; set; }
        
        public string BranchCode { get; set; }
        public string TestVar { get; set; }
        

        [Display(Name="Date Dreated")]
        public DateTime Create_At { get; set; }

        [Display(Name = "Last Modified Date")]
        public DateTime LastEdit_At { get; set; }
        public bool IsFieldOffice { get; set; }
        public int Status { get; set; }
        [NotMapped]
        public bool Selected { get; set; }
        public string Address { get; set; }
        public int StateId { get; set; }
    }
}
