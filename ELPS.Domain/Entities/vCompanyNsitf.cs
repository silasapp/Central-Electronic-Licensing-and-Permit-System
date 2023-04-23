using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class vCompanyNsitf
    {
     
        public int Id { get; set; }

        public int No_People_Covered { get; set; }

        public string Policy_No { get; set; }

        public int Company_Id { get; set; }
        public DateTime Date_Issued { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }

        [NotMapped]
        public int Elps_Id { get; set; }
    }
}
