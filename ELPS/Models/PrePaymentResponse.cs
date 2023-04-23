using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class PrePaymentResponse
    {
        public string statusmessage { get; set; }
        public string message { get; set; }
        public string AppId { get; set; }
        public string Status { get; set; }
        public string RRR { get; set; }
        public string Transactiontime { get; set; }
        public string TransactionId { get; set; }
        public List<int> RequiredDocs { get; set; }
    }
}