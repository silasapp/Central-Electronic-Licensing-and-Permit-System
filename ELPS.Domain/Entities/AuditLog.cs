using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELPS.Domain.Entities 
{
    public partial class AuditLog
    {
        [Key]
        public Guid  AuditLogId { get; set; }
        public string UserId { get; set; }
        public System.DateTime EventDateUTC { get; set; }
        public string EventType { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public string ColumnName { get; set; }
        public string OriginalValue { get; set; }
        public string NewValue { get; set; }
        public string IP { get; set; }
    }

}
