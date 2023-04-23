using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public partial class vFacilityFile
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int FacilityId { get; set; }
        public string FileName { get; set; }
        public string source { get; set; }
        public int document_type_id { get; set; }
        public string DocumentTypeName { get; set; }
        public DateTime  date_modified { get; set; }
        public DateTime date_added { get; set; }
        public bool Status { get; set; }
        public bool Archived { get; set; }
        public string Document_Name { get; set; }
        public string UniqueId { get; set; }
    }
}
