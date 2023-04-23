using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class RemitaSplit
    {
        public string merchantId { get; set; }
        public string serviceTypeId { get; set; }
        public string amount { get; set; }
        public string hash { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string payerPhone { get; set; }
        public string orderId { get; set; }
        public string responseurl { get; set; }

        public List<RPartner> lineItems { get; set; }
        public List<RCustomFields> customFields { get; set; }
    }

    public class RPartner
    {
        public string lineItemsId { get; set; }
        public string beneficiaryName { get; set; }
        public string beneficiaryAccount { get; set; }
        public string bankCode { get; set; }
        public string beneficiaryAmount { get; set; }
        public string deductFeeFrom { get; set; }
    }

    public class RCustomFields
    {
        public string name { get; set; }
        public string value { get; set; }
        public string type { get; set; }
    }

}