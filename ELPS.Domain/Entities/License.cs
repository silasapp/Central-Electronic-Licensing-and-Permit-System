namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    //[Table("file")]
    public partial class License : EntityBase
    {
        public string LicenseName { get; set; }
        public string LicenseShortName { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTime DateAdded { get; set; }
        public string ReceiptCode { get; set; }

        [NotMapped]
        public int MyPermits { get; set; }
        [NotMapped]
        public int LicensesInProcessing { get; set; }

        [NotMapped]
        public AppIdentity AppIdentity { get; set; }
    }
}
