using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class Company_MedicalView
    {
        public int id { get; set; }
        [Required]
        public string Medical_Organisation { get; set; }
        //[Required]
        public string Address { get; set; }
        [MaxLength(13),MinLength(11),Required]
        public string Phone { get; set; }
        
        [StringLength(200),EmailAddress]
        public string Email { get; set; }

        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
        public DateTime Date_Issued { get; set; }
        public HttpPostedFileBase  File { get; set; }
    }
}