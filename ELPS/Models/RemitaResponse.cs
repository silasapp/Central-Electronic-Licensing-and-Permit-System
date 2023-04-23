using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class StringResponse
    {
        public int Code { get; set; }
        public string Status { get; set; }
    }

    public class RemitaResponse
    {
        public string statusMessage { get; set; }
        public string message { get; set; }
        public string merchantId { get; set; }
        public string status { get; set; }
        public string RRR { get; set; }
        public string transactiontime { get; set; }
        public string orderId { get; set; }
        public string statuscode { get; set; }
        public string Amount { get; set; }
    }


    public class NewRemitaResponse
    {
        public string amount { get; set; }
        public string orderId { get; set; }
        public string RRR { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string transactiontime { get; set; }
        public string statusMessage { get; set; }
    }

    //{"amount":31500.0,"RRR":"160358730447","orderId":"529319384014","message":"Transaction Pending","transactiontime":"2019-12-11 12:00:00 AM","status":"021"}

    //({"statuscode":"025","RRR":"100358740052","orderID":"877056271948","status":"Payment Reference generated"})
    public class _NewRemitaResponse
    {
        public string amount { get; set; }
        public string RRR { get; set; }
        public string orderId { get; set; }
        public string status { get; set; }
        public string statusMessage { get; set; }
        public string message { get; set; }
        public string paymentDate { get; set; }
        public string transactiontime { get; set; }
        public string statuscode { get; set; }
    }


    public static class RemitaSplitParams
    {
        public static string MERCHANTID = ConfigurationManager.AppSettings["merchantID"];// "2547916";
        public static string SERVICETYPE_GEN = ConfigurationManager.AppSettings["servTyp_Gen"];  //"4430731";
        public static string SERVICETYPE_MAJ = ConfigurationManager.AppSettings["servTyp_Maj"];
        public static string SERVICETYPE_SPE = ConfigurationManager.AppSettings["servTyp_Spe"];
        public static string APIKEY = ConfigurationManager.AppSettings["rKey"]; //"1946";
        public static string GATEWAYURL = ConfigurationManager.AppSettings["RemitaSplitUrl"];
        public static string CHECKSTATUS_ORDERID = ConfigurationManager.AppSettings["RemitaStatus_OrderID"];
        public static string CHECKSTATUS_RRR = ConfigurationManager.AppSettings["RemitaStatus_RRR"];
        public static string RESPONSEURL = ConfigurationManager.AppSettings["RemitaPaymentCallback"];
        public static string RRRGATEWAYPAYMENTURL = ConfigurationManager.AppSettings["RemitaRRRGateway"];
    }
}