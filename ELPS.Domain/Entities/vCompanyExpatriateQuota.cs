using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class vCompanyExpatriateQuota
    {
        public int Id { get; set; }
        public int Company_Id { get; set; }

        public string Name { get; set; }
        public int? FileId { get; set; }
        public string FileName { get; set; }
        public string FileSource { get; set; }
        [NotMapped]
        public int Elps_Id { get; set; }
    }
}
