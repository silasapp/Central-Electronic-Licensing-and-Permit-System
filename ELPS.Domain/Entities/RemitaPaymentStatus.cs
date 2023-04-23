using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    [Table("RemitaPaymentStatuses")]
    public class RemitaPaymentStatus : EntityBase
    {
        //[Key]
        public string strId { get; set; }
        public string rrr { get; set; }
        public string channnel { get; set; }
        public string amount { get; set; }
        public string responseCode { get; set; }
        public string transactiondate { get; set; }
        public string debitdate { get; set; }
        public string bank { get; set; }
        public string branch { get; set; }
        public string serviceTypeId { get; set; }
        public string dateSent { get; set; }
        public string dateRequested { get; set; }
        public string orderRef { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string payerPhoneNumber { get; set; }
        public string uniqueIdentifier { get; set; }
        public bool? Orphan { get; set; }
        //public DateTime? Date_Sent { get; set; }
        public DateTime? Date_Requested { get; set; }
        [NotMapped]
        public bool IsCompleted { get; set; }
        [NotMapped]
        public string BankPaymentEndPoint { get; set; }
        [NotMapped]
        public DateTime? Date
        {
            get
            {
                //if (Date_Requested.HasValue)
                //{
                //    return Date_Requested;
                //}
                //else 
                if (!string.IsNullOrEmpty(this.debitdate))
                {
                    DateTime dd;
                    if (DateTime.TryParse(this.debitdate, out dd)){
                        return dd;
                    }
                    else if (!string.IsNullOrEmpty(this.transactiondate))
                    {
                        DateTime dt;
                        if (DateTime.TryParse(this.transactiondate, out dt))
                        {
                            return dt;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else if (!string.IsNullOrEmpty(this.transactiondate))
                {
                    DateTime dd;
                    if (DateTime.TryParse(this.transactiondate, out dd))
                    {
                        return dd;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
        }
    }
}
