namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("permit")]
    public partial class Permit:EntityBase
    {        

        [Required]
        [StringLength(50)]
        public string Permit_No { get; set; }

        public string OrderId { get; set; }

        public int Company_Id { get; set; }

        public DateTime Date_Issued { get; set; }

        public DateTime Date_Expire { get; set; }
        public string CategoryName { get; set; }
        public string Is_Renewed { get; set; }
        public int LicenseId { get; set; }

        [NotMapped]
        public bool Expired
        {
            get
            {
                return this.Date_Expire < DateTime.Now ? true : false;
            }
        }
    }
}
