using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Models;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Helpers;
using ELPS.Helpers;

namespace ELPS.Controllers
{
    public class ReportsController : Controller
    {
        IStateRepository _stateRep;
        IvApplicationRepository _vAppRepo;
        ChartHelper _chartHelper;
        IInvoiceRepository _invoRep;
        IvPermitRepository _vPermitRep;
        IAppIdentityRepository _appIdRep;
        IvInvoiceRepository _vInvoRep;
        IvAccountReportRepository _vAccReportRep;
        IvZoneRepository _vZoneRep;
        IvBranchRepository _vBranchRep;
        IvZoneStateRepository _vZoneStateRep;

        public ReportsController(IvApplicationRepository vAppRepo, IInvoiceRepository invoRep, IvInvoiceRepository vInvoRep, IvPermitRepository vPermitRep, IvZoneRepository vZoneRep,
            IAppIdentityRepository appIdRep, IStateRepository stateRep, IvAccountReportRepository vAccReportRep, IvBranchRepository vBranchRep, IvZoneStateRepository vZoneStateRep)
        {
            _vZoneStateRep = vZoneStateRep;
            _vBranchRep = vBranchRep;
            _vZoneRep = vZoneRep;
            _vAccReportRep = vAccReportRep;
            _stateRep = stateRep;
            _vAppRepo = vAppRepo;
            _invoRep = invoRep;
            _vInvoRep = vInvoRep;
            _chartHelper = new ChartHelper();
            _vPermitRep = vPermitRep;
            _appIdRep = appIdRep;
        }

        public ActionResult GenerateReport(string startDate, string endDate, int? license, string filterby, int? filterparam)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date; //.AddHours(23).AddMinutes(59);
            ViewBag.States = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            var licenses = _appIdRep.GetAll().ToList();
            IEnumerable<vAccountReport> _applications;

            _applications = _vAccReportRep.FindBy(a => a.TransactionDate >= sd && a.TransactionDate <= ed && a.Status != ApplicationStatus.PaymentPending);
            if (license.GetValueOrDefault(0) > 0)
            {
                var lic = licenses.Where(a => a.Id == license).FirstOrDefault();

                _applications = lic != null ? _applications.Where(a => a.LicenseName.ToUpper() == lic.ShortName.ToUpper()) : _applications;
            }


            ViewBag.Licenses = licenses;
            ViewBag.SD = sd;
            ViewBag.ED = ed;

            int p = filterparam.GetValueOrDefault(0);
            ViewBag.param = p;
            ViewBag.by = filterby;
            var mm = "for ";

            BranchFilterModel filter = new BranchFilterModel();
            if (HttpContext.Cache["_filterModel_"] == null)
            {
                filter.States = _stateRep.FindBy(a => a.CountryId == 156).ToList();
                filter.Zones = _vZoneRep.GetAll().ToList();
                filter.Branches = _vBranchRep.GetAll().ToList();
                ViewBag.Filter = filter;
                HttpContext.Cache["_filterModel_"] = filter;
            }
            else
            {
                filter = (BranchFilterModel)HttpContext.Cache["_filterModel_"];
                ViewBag.Filter = filter;
            }
            if (!string.IsNullOrEmpty(filterby))
            {
                switch (filterby.ToLower())
                {
                    case "zn":
                        {
                            var zn = _vZoneRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                            if (zn != null)
                            {
                                var fds = _vZoneStateRep.FindBy(a => a.ZoneId == zn.Id).ToList();

                                UtilityHelper.LogMessage($"Zone Found: {zn.Name} ==> {fds?.Count()}");
                                mm += "Filtered by " + zn.Name;
                                switch (fds.Count())
                                {
                                    case 1:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && a.StateName.ToLower() == fds[0].StateName.ToLower());
                                        break;
                                    case 2:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()));
                                        break;
                                    case 3:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()));
                                        break;
                                    case 4:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()));
                                        break;
                                    case 5:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()));
                                        break;
                                    case 6:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()));
                                        break;
                                    case 7:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[6].StateName.ToLower()));
                                        break;
                                    case 8:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower()));
                                        break;
                                    case 9:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[8].StateName.ToLower()));
                                        break;
                                    case 10:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[8].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[9].StateName.ToLower()));
                                        break;
                                    case 11:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[8].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[9].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[10].StateName.ToLower()));
                                        break;
                                    case 12:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[8].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[9].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower()));
                                        break;
                                    case 13:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[8].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[9].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[12].StateName.ToLower()));
                                        break;
                                    case 14:
                                        _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName)
                                        && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[8].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[9].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[12].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[13].StateName.ToLower()));
                                        break;
                                    default:
                                        _applications = null;
                                        break;
                                }
                            }
                            else
                            {
                                UtilityHelper.LogMessage($"Zone NOT Found");
                                _applications = null;
                            }
                            break;
                        }
                    case "fd":
                        {
                            var brch = _vBranchRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                            UtilityHelper.LogMessage($"Field Office => {brch?.Name}");
                            if (brch != null)
                            {
                                mm += "Filtered by " + brch.Name;
                                _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName) && a.StateName.ToLower() == brch.StateName.ToLower());
                            }
                            else
                            {
                                UtilityHelper.LogMessage($"Field Office NOT Found");
                                _applications = null;
                            }
                            break;
                        }
                    case "st":
                        {
                            mm += "Filtered by All Offices";
                            var _state = _stateRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                            UtilityHelper.LogMessage($"State Filter => {_state?.Name}");
                            if (_state != null)
                            {
                                mm += "Filtered by " + _state.Name;
                                _applications = _applications.Where(a => !string.IsNullOrEmpty(a.StateName) && a.StateName.ToLower() == _state.Name.ToLower());
                            }
                            else
                                _applications = null;
                            break;
                        }
                    default:
                        break;
                }
            }
            else
                mm += "Filtered by All Offices";
            ViewBag.ResultTitle = mm;

            return View("Index", _applications.ToList());
        }

        public ActionResult PaymentSummary(string startDate, string endDate, int? license, string t = "combined")
        {
            // Use date range, but if not supplied, use last 30 days as the default
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            ViewBag.licenses = _appIdRep.GetAll().ToList();
            var lid = license == null ? 0 : license;
            ViewBag.states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            //ViewBag.location = location;

            ViewBag.SD = sd;
            ViewBag.ED = ed;

            var paidInvoices = new List<vInvoice>();
            if (lid == 0)
            {
                paidInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == "paid").Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed).OrderBy(o => o.Date_Paid).ToList();
            }
            else
            {
                paidInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == "paid").Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == lid).OrderBy(o => o.Date_Paid).ToList();
            }

            List<PaymentReportModel> reportModel = new List<PaymentReportModel>();
            var apps = _vAppRepo.FindBy(a => a.Status.ToLower() != "payment panding".ToLower()).ToList();

            int xo = 0;
            foreach (var invoice in paidInvoices)
            {
                var app = apps.Where(a => a.Id == invoice.Application_Id).FirstOrDefault();
                if (app != null)
                {
                    //listOfApplications.Add(app);
                    var rm = new PaymentReportModel();
                    rm.ApplicationID = app.Id;
                    rm.Category = app.CategoryName;
                    rm.Date = invoice.Date_Paid;
                    rm.CompanyName = app.CompanyName;
                    rm.ReceiptNo = invoice.ReceiptNo;
                    rm.LicenseShortName = app.LicenseShortName;
                    rm.Amount = (t.ToLower() == "servicecharge" ? Convert.ToDouble(app.ServiceCharge.Trim()) : (t.ToLower() == "application" ? Convert.ToDouble(app.approved_amount.Trim()) : Convert.ToDouble(invoice.Amount)));
                    rm.TotalAmount = invoice.Amount;
                    var x = int.TryParse(app.ServiceCharge, out xo);
                    rm.Charge = xo;

                    x = int.TryParse(app.approved_amount, out xo);
                    rm.Fee = xo;
                    reportModel.Add(rm);
                }
            }

            List<string> cates = new List<string>();
            if (lid == 0)
            {
                cates = GetCategoryList(apps, "license");
            }
            else
            {
                cates = GetCategoryList(apps, "Category");
            }

            Series[] series = new Series[cates.Count()];
            int counter = 0;

            List<string> groups = new List<string>();
            foreach (var item in reportModel)
            {
                if (!groups.Contains(item.Date.ToShortDateString()))
                    groups.Add(item.Date.ToShortDateString());
            }

            foreach (var cate in cates)
            {
                List<object> objct = new List<object>();

                //List of apps in this category
                //var appTouse = listOfApplications.Where(ap => ap.CategoryName.ToLower() == cate.ToLower()).ToList();
                var touse = new List<PaymentReportModel>();
                if (lid == 0)
                {
                    touse = reportModel.Where(ap => ap.LicenseShortName.Trim().ToLower() == cate.ToLower()).ToList();
                }
                else
                {
                    touse = reportModel.Where(ap => ap.Category.Trim().ToLower() == cate.ToLower()).ToList();
                }

                decimal sum = 0;
                string grp = string.Empty;
                for (var i = 0; i < groups.Count; i++)
                {
                    var choosen = touse.Where(a => a.Date.ToShortDateString() == groups[i]).ToList();
                    if (choosen != null)
                    {
                        sum = (decimal)choosen.Sum(a => a.Amount);
                    }
                    objct.Add(sum);
                }

                series[counter] = new Series { Name = cate, Data = new Data(objct.ToArray()) };
                counter++;
            }
            PaymentSummaryModel returnedData = new PaymentSummaryModel();
            returnedData.EndDate = ed;
            returnedData.StartDate = sd;
            returnedData.ReportForm = t;
            returnedData.ReportSummary = reportModel;

            //var rs = returnedData.ReportSummary.GroupBy(a => a.LicenseShortName).ToList();
            //foreach (var item in rs)
            //{
            //    var x = item.Key;
            //    var y = item.Sum(a => a.TotalAmount);
            //}
            string yAxis = "Amount"; //yName.BranchName + "(" + yName.DealerName + ") Gross Profit";
            string title = (t.ToLower() == "servicecharge" ? "Service Charge" : (t.ToLower() == "application" ? "Application Fee" : "Payments"));
            title += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            string tooltip = @"function() { return ''+ this.x +': ₦'+ this.y; }";
            string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>₦ {point.y:.2f}</b></td></tr>";
            returnedData.SummaryChart = _chartHelper.MultiBarChart(series, groups, yAxis, title, "Report", tooltip, pointer);

            //ViewBag.Categories = _catRepo.GetAll().ToList();
            returnedData.ReportTitle = (t.ToLower() == "servicecharge" ? "Service Charge" : (t.ToLower() == "application" ? "Application Fee" : "Payments"));
            title += (string.IsNullOrEmpty(startDate) ? " Summary for the last 30 days" : " Summary from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            var sumTable = GetSummaryTable(reportModel);

            returnedData.SummaryTable = sumTable;

            return View(returnedData);
        }

        public ActionResult ApplicationReport(string startDate, string endDate, int? license, string filterby, int? filterparam)
        {
            // Use date range, but if not supplied, use last 30 days as the default
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            var licenses = _appIdRep.GetAll().ToList();
            ViewBag.licenses = licenses;
            ViewBag.license = license == null ? 0 : license;
            ViewBag.states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            ViewBag.SD = sd;
            ViewBag.ED = ed;
            int p = filterparam.GetValueOrDefault(0);
            ViewBag.param = p;
            ViewBag.by = filterby;
            var lid = license == null ? 0 : license;
            BranchFilterModel filter = new BranchFilterModel();
            if (HttpContext.Cache["_filterModel_"] == null)
            {
                filter.States = _stateRep.FindBy(a => a.CountryId == 156).ToList();
                filter.Zones = _vZoneRep.GetAll().ToList();
                filter.Branches = _vBranchRep.GetAll().ToList();
                ViewBag.Filter = filter;
                HttpContext.Cache["_filterModel_"] = filter;
            }
            else
            {
                filter = (BranchFilterModel)HttpContext.Cache["_filterModel_"];
                ViewBag.Filter = filter;
            }
            //var mm = "for ";
            
            //if (!string.IsNullOrEmpty(filterby))
            //{
            //    switch (filterby.ToLower())
            //    {
            //        case "zn":
            //            var z = filter.Zones.Where(a => a.Id == p).FirstOrDefault();
            //            mm += "Filtered by " + z.Name;
            //            break;
            //        case "fd":
            //            var f = filter.Branches.Where(a => a.Id == p).FirstOrDefault();
            //            mm += "Filtered by " + f.Name;
            //            break;
            //        default:
            //            mm += "Filtered by All Offices";
            //            break;
            //    }
            //}
            //else
            //    mm += "Filtered by All Offices";

            //ViewBag.ResultTitle = mm;

            List<vApplication> applications = new List<vApplication>();

            List<ListOfReportModel> report = new List<ListOfReportModel>();
            List<string> categories = new List<string>();
            if (lid == 0)
            {
                applications = _vAppRepo.FindBy(a => a.Date >= sd && a.Date <= ed).OrderBy(o => o.Date).ToList();
                categories = GetCategoryList(applications, "license");
            }
            else
            {
                applications = _vAppRepo.FindBy(a => a.Date >= sd && a.Date <= ed && a.LicenseId == lid).OrderBy(o => o.Date).ToList();
                categories = GetCategoryList(applications, "Category");
            }

            Series[] series = new Series[categories.Count()];
            int counter = 0;

            List<string> groups = new List<string>();
            var diff = (ed - sd).TotalDays;
            for (int i = 0; i < diff; i++)
            {
                groups.Add(sd.AddDays(i).ToShortDateString());
            }

            List<BasicReportModel> arm = new List<BasicReportModel>();

            #region Seperated
            foreach (var category in categories)
            {
                List<object> objct = new List<object>();
                var touse = new List<vApplication>();
                if (lid == 0)
                {
                    touse = applications.Where(ap => ap.LicenseShortName.Trim().ToLower() == category.ToLower()).ToList();
                }
                else
                {
                    touse = applications.Where(ap => ap.CategoryName.Trim().ToLower() == category.ToLower()).ToList();
                }


                var armodel = new BasicReportModel();
                armodel.LicenseShortName = category;
                armodel.Count = touse.Count();
                arm.Add(armodel);

                for (var i = 0; i < groups.Count; i++)
                {
                    int sum = touse.Where(a => a.Date.ToShortDateString() == groups[i]).Count();
                    objct.Add(sum);
                }

                series[counter] = new Series { Name = category, Data = new Data(objct.ToArray()) };
                counter++;
            }
            string yAxis = "Application Rate";
            string title = "Application Rate";
            title += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            string tooltip = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            //string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>₦ {point.y:.2f}</b></td></tr>";
            ViewBag.ApplicationChart = _chartHelper.LineChart(series, groups, title, yAxis, "AppChart", tooltip);
            #endregion

            #region Combined
            List<object> obj = new List<object>();
            Series[] seriesCom = new Series[1];
            //int sum = 0;
            //string grp = string.Empty;
            for (var i = 0; i < groups.Count; i++)
            {
                int sum = applications.Where(a => a.Date.ToShortDateString() == groups[i]).Count();
                obj.Add(sum);
            }
            seriesCom[0] = new Series { Name = "All Applications", Data = new Data(obj.ToArray()) };

            string yAxisCom = "Application Rate";
            string titleCom = "Combined Application Rate";
            titleCom += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            string tooltipCom = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            //string pointer = @"<tr><td style=""color:{series.color};padding:0"">{series.name}: </td><td style=""padding:0""><b>₦ {point.y:.2f}</b></td></tr>";
            ViewBag.CombinedChart = _chartHelper.LineChart(seriesCom, groups, titleCom, yAxisCom, "CombinedChart", tooltipCom);
            #endregion


            foreach (var app in applications)
            {
                var check = report.Where(a => a.LicenseName.Trim().ToLower() == app.LicenseShortName.Trim().ToLower()).ToList();
                if (check.Count() <= 0)
                {
                    var xArm = applications.Where(a => a.LicenseShortName.ToLower() == app.LicenseShortName.ToLower()).ToList();
                    var xa = new List<BasicReportModel>();
                    foreach (var a in xArm)
                    {
                        if (xa.Where(x => x.Category.Trim().ToLower() == a.CategoryName.Trim().ToLower()).ToList().Count() <= 0)
                        {
                            xa.Add(new BasicReportModel()
                            {
                                Category = a.CategoryName.Trim(),
                                LicenseShortName = a.LicenseShortName,
                                Count = xArm.Where(x => x.CategoryName.Trim().ToLower() == a.CategoryName.Trim().ToLower()).Count()
                            });
                        }
                    }
                    report.Add(new ListOfReportModel()
                    {
                        LicenseName = app.LicenseShortName,
                        ReportModels = xa
                    });
                }
            }

            ViewBag.Counter = arm;
            ViewBag.Report = report;
            return View();

        }

        // [Authorize(Roles = "Admin,Support,Manager,Account,Director,ManagerObserver, Approver")]
        public ActionResult PermitReport(string startDate, string endDate, int? license, string location)
        {
            // Use date range, but if not supplied, use last 30 days as the default
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            ViewBag.SD = sd;
            ViewBag.ED = ed;

            ViewBag.licenses = _appIdRep.GetAll().ToList();
            var lid = license == null ? 0 : license;
            ViewBag.states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            ViewBag.location = location;

            List<vPermit> permits = new List<vPermit>();
            List<string> categories = new List<string>();
            if (lid == 0)
            {
                if (string.IsNullOrEmpty(location))
                {

                    permits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed).OrderBy(o => o.Date_Issued).ToList();

                }
                else
                {

                    permits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.StateName == location).OrderBy(o => o.Date_Issued).ToList();

                }

                foreach (var item in permits)
                {
                    if (!categories.Contains(item.LicenseShortName.Trim()))
                        categories.Add(item.LicenseShortName.Trim());
                }
            }
            else
            {
                if (string.IsNullOrEmpty(location))
                {
                    permits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.LicenseId == lid).OrderBy(o => o.Date_Issued).ToList();
                }
                else
                {

                    permits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.LicenseId == lid && a.StateName == location).OrderBy(o => o.Date_Issued).ToList();

                }
                foreach (var item in permits)
                {
                    if (!categories.Contains(item.CategoryName.Trim()))
                        categories.Add(item.CategoryName.Trim());
                }
            }
            //List<vPermit> permits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed).OrderBy(o => o.Date_Issued).ToList();
            //List<string> categories = new List<string>(); // GetCategoryList(applications);


            Series[] series = new Series[categories.Count()];
            int counter = 0;

            List<string> groups = new List<string>();
            var diff = (ed - sd).TotalDays;
            for (int i = 0; i < diff; i++)
            {
                groups.Add(sd.AddDays(i).ToShortDateString());
            }

            List<BasicReportModel> arm = new List<BasicReportModel>();

            #region Seperated
            foreach (var category in categories)
            {
                List<object> objct = new List<object>();
                var touse = new List<vPermit>();
                if (license == 0)
                {
                    touse = permits.Where(ap => ap.LicenseShortName.Trim().ToLower() == category.ToLower()).ToList();
                }
                else
                {
                    touse = permits.Where(ap => ap.CategoryName.Trim().ToLower() == category.ToLower()).ToList();
                }

                var armodel = new BasicReportModel();
                armodel.LicenseShortName = category;
                armodel.Count = touse.Count();
                arm.Add(armodel);

                for (var i = 0; i < groups.Count; i++)
                {
                    int sum = touse.Where(a => a.Date_Issued.ToShortDateString() == groups[i]).Count();
                    objct.Add(sum);
                }

                series[counter] = new Series { Name = category, Data = new Data(objct.ToArray()) };
                counter++;

                string yAxis = "Permit Rate";
                string title = "Permit Rate";
                title += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

                string tooltip = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
                ViewBag.ApplicationChart = _chartHelper.LineChart(series, groups, title, yAxis, "AppChart", tooltip);
            }

            #endregion

            #region Combined
            List<object> obj = new List<object>();
            Series[] seriesCom = new Series[1];
            //int sum = 0;
            //string grp = string.Empty;
            for (var i = 0; i < groups.Count; i++)
            {
                int sum = permits.Where(a => a.Date_Issued.ToShortDateString() == groups[i]).Count();
                obj.Add(sum);
            }
            seriesCom[0] = new Series { Name = "All Applications", Data = new Data(obj.ToArray()) };

            string yAxisCom = "Permit Rate";
            string titleCom = "Combined Permit Rate";
            titleCom += (string.IsNullOrEmpty(startDate) ? " for the last 30 days" : " from " + sd.ToShortDateString() + " to " + ed.ToShortDateString());

            string tooltipCom = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y +''; }";
            ViewBag.CombinedChart = _chartHelper.LineChart(seriesCom, groups, titleCom, yAxisCom, "CombinedChart", tooltipCom);
            #endregion

            ViewBag.Counter = arm;

            return View();
        }

        public List<PaymentSummaryTable> GetSummaryTable(List<PaymentReportModel> model)
        {
            List<PaymentSummaryTable> returnModel = new List<PaymentSummaryTable>();
            foreach (var item in model)
            {
                if (returnModel.Where(m => m.Category.Trim().ToLower() == item.Category.Trim().ToLower()).Count() <= 0)
                    returnModel.Add(new PaymentSummaryTable() { Category = item.Category.Trim() });
            }

            #region Generating the Table
            foreach (var item in returnModel)
            {
                if (item.Category.ToLower() == "general")
                {
                    var touse = model.Where(a => a.Category.Trim().ToLower() == "general").ToList();
                    double amount = touse.Sum(a => a.TotalAmount);
                    double fee = touse.Sum(a => a.Fee);
                    double amountToShare = ((amount * .985) - (fee) - (10 * touse.Count()));
                    item.Distribution = GenerateTableRow(amountToShare, fee);
                }
                else if (item.Category.ToLower() == "major")
                {
                    var touse = model.Where(a => a.Category.Trim().ToLower() == "major").ToList();
                    double sCharge = touse.Sum(a => a.Charge);
                    double fee = touse.Sum(a => a.Fee);
                    double amountToShare = (sCharge - 265 - (10 * touse.Count()));
                    item.Distribution = GenerateTableRow(amountToShare, fee);
                }
                else if (item.Category.ToLower() == "specialized")
                {
                    var touse = model.Where(a => a.Category.Trim().ToLower() == "specialized").ToList();
                    double sCharge = touse.Sum(a => a.Charge);
                    double fee = touse.Sum(a => a.Fee);
                    double amountToShare = (sCharge - 550 - (10 * touse.Count()));
                    item.Distribution = GenerateTableRow(amountToShare, fee);
                }
            }
            #endregion

            return returnModel;
        }

        private List<Distribution> GenerateTableRow(double ats, double fee)
        {
            List<Distribution> result = new List<Distribution>();
            Distribution d;
            d.Field = "FG";
            d.Value = fee;
            result.Add(d);

            d.Field = "NUPRC";
            d.Value = ats * 0.1;
            result.Add(d);

            d.Field = "BrandOneMaxFront";
            d.Value = ats * 0.9;
            result.Add(d);

            d.Field = "Total";
            d.Value = fee + ats;
            result.Add(d);

            return result;
        }


        /// <summary>
        ///     Returns the List of Licenses or Categories in a Group of Applications
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        /// 
        private List<string> GetCategoryList(List<vApplication> list, string type)
        {
            List<string> cates = new List<string>();
            foreach (var item in list)
            {
                if (type == "license")
                {
                    //Returns List of Licenses
                    if (!cates.Contains(item.LicenseShortName.Trim()))
                        cates.Add(item.LicenseShortName.Trim());
                }
                else
                {
                    //Returns List of Categories
                    if (!cates.Contains(item.CategoryName.Trim()))
                        cates.Add(item.CategoryName.Trim());
                }
            }
            return cates;
        }
        //private List<string> GetCategoryList(List<vApplication> list)
        //{
        //    List<string> cates = new List<string>();
        //    foreach (var item in list)
        //    {
        //        if (!cates.Contains(item.CategoryName.Trim()))
        //            cates.Add(item.CategoryName.Trim());
        //    }
        //    return cates;
        //}


    }
}