namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class Facility:EntityBase
    {
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public DateTime DateAdded { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public int StateId { get; set; }
        public int LGAId { get; set; }
        public string FacilityType { get; set; }

        [NotMapped]
        public List<FacilityDocument> FacilityDocuments { get; set; }
    }

}
