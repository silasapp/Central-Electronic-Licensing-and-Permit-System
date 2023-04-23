using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class MailReceipt : EntityBase
    {
        [Required]
        public string Email { get; set; }
        public bool? Delivered { get; set; }
        public bool? Read { get; set; }
        public DateTime? DateDelivered { get; set; }
        public DateTime? DateRead { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public DateTime DateSent { get; set; }
        [Required]
        public int EntityId { get; set; }
    }
}
