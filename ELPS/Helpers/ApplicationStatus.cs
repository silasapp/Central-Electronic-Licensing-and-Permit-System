using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELPS.Helpers
{

    public static class ApplicationStatus
    {
        public static string PaymentPending { get { return "Payment Pending"; } }
        public static string PaymentCompleted { get { return "Payment Completed"; } }
        public static string Processing { get { return "Processing"; } }
        public static string Rejected { get { return "Rejected"; } }
        public static string Approved { get { return "Approved"; } }
    }
}