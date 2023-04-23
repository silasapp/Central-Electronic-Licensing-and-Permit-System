using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNet.Highcharts.Options;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;

namespace ELPS.Models
{
    public class PaymentSummaryModel
    {
        public string ReportTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportForm { get; set; }
        public Highcharts SummaryChart { get; set; }
        public List<PaymentSummaryTable> SummaryTable { get; set; }
        public List<PaymentReportModel> ReportSummary { get; set; }
    }

    public class PaymentReportModel
    {
        public long ID { get; set; }
        public int ApplicationID { get; set; }
        public string ReferenceNo { get; set; }
        public string PaymentRef { get; set; }
        public string Channel { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public double Amount { get; set; }
        public string CompanyName { get; set; }
        public string ReceiptNo { get; set; }
        public string LicenseShortName { get; set; }
        public double TotalAmount { get; set; }
        public int Fee { get; set; }
        public int Charge { get; set; }
    }

    public class ReceiptModel : PaymentReportModel
    {

    }

    public class StaffProcessModel
    {
        public string StaffId { get; set; }
        public string StaffName { get; set; }
        public int Approved { get; set; }
        public int Processing { get; set; }
        public int Rejected { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class ListOfReportModel
    {
        public string LicenseName { get; set; }
        public List<BasicReportModel> ReportModels { get; set; }
    }

    public class BasicReportModel
    {
        public string Category { get; set; }
        public string LicenseShortName { get; set; }
        public int Count { get; set; }
    }

    public class PaymentSummaryTable
    {
        public string Category { get; set; }
        public List<Distribution> Distribution { get; set; }
    }

    public struct Distribution
    {
        public string Field; public double Value;
    }
}