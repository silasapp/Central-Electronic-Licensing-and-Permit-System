using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class Company_NsitfView
    {
        public int id { get; set; }
        public int No_People_Covered { get; set; }

        [Required]
        public string Policy_No { get; set; }
        public DateTime Date_Issued { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
        public HttpPostedFileBase file { get; set; }

    }
}