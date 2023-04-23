using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class Company_Expatriate_QuotaView
    {
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        public int? FileId { get; set; }
        public string  FileName { get; set; }
        public string  FileSource { get; set; }
        public HttpPostedFileBase File { get; set; }


    }
}