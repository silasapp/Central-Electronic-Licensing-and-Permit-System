using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ELPS.Domain.Entities;

namespace ELPS.Models
{
    public class Company_ProffessionalView
    {
        public int Id { get; set; }
        [Required]
        [StringLength(250)]
        public string Proffessional_Organisation { get; set; }

        //[Required]
        [StringLength(250)]
        public string Cert_No { get; set; }

        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
        public DateTime Date_Issued { get; set; }
        public HttpPostedFileBase File { get; set; }

    }

    public class CompanyAddressModel
    {
        public vAddress vRegAddress { get; set; }
        public vAddress vOpeAddress { get; set; }
        public Address RegAddress { get; set; }
        public Address OpeAddress { get; set; }
    }
}