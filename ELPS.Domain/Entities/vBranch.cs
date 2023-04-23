namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("branch")]
    public partial class vBranch : EntityBase
    {
        public string Name { get; set; }

        public int StateId { get; set; }
        public string Address { get; set; }

        public string StateName { get; set; }

        public string CountryName { get; set; }

        public int CountryId { get; set; }
        public bool IsFieldOffice { get; set; }
        [NotMapped]
        public bool Selected { get; set; }
    }
}
