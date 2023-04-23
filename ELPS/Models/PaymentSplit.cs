using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ELPS.Domain.Entities;
using ELPS.Domain;

namespace ELPS.Models
{
    public class PaymentSplit
    {
        public string serviceTypeId { get; set; }
        public string CategoryName { get; set; }
        public string totalAmount { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string ServiceCharge { get; set; }
        public string AmountDue { get; set; }
        public string payerPhone { get; set; }
        public string orderId { get; set; }
        public string ReturnSuccessUrl { get; set; }
        public string ReturnFailureUrl { get; set; }
        public string ReturnBankPaymentUrl { get; set; }

        public List<RPartner> lineItems { get; set; }
        public List<RCustomFields> customFields { get; set; }

        public List<int> DocumentTypes { get; set; }
        public List<ApplicationItem> ApplicationItems { get; set; }
    }

    public class RemitaPaymentModel
    {
        public string merchantId { get; set; }
        public string serviceTypeId { get; set; }
        public string orderId { get; set; }
        public string hash { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string payerPhone { get; set; }
        public string Amount { get; set; }
        public string responseurl { get; set; }
        public string ReturnSuccessUrl { get; set; }
        public string ReturnFailureUrl { get; set; }
        public string ReturnBankPaymentUrl { get; set; }

        public string amt { get { return this.Amount; } }
        public List<RPartner> LineItems { get; set; }
        public List<RCustomFields> customFields { get; set; }
    }

}