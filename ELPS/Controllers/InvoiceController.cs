using ELPS.Domain.Entities;
using ELPS.Domain.Abstract;
using ELPS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    public class InvoiceController : Controller
    {
        IStateRepository _stateRep;
        IPermitCategoryRepository _permCatRep;
        ILicenseRepository _licensRep;
        IReceiptRepository _recptRep;
        IInvoiceRepository _invoiceRep;
        IvInvoiceRepository _vInvoRep;
        IvReceiptRepository _vReceiptRep;
        IAppIdentityRepository _appIdRep;

        public InvoiceController(ILicenseRepository licensRep, IReceiptRepository recptRep, IInvoiceRepository invoiceRep,IStateRepository stateRep,
            IvInvoiceRepository vInvoRep, IvReceiptRepository vReceiptRep, IAppIdentityRepository appIdRep, IPermitCategoryRepository permCatRep)
        {
            _stateRep = stateRep;
            _permCatRep = permCatRep;
            _licensRep = licensRep;
            _recptRep = recptRep;
            _invoiceRep = invoiceRep;
            _vInvoRep = vInvoRep;
            _vReceiptRep = vReceiptRep;
            _appIdRep = appIdRep;
        }
        // GET: Invoice

        public ActionResult Index(string startDate, string endDate, int? license, string category, string location, string status = "paid")
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            ViewBag.licenses = _appIdRep.GetAll().ToList();
            ViewBag.license = license == null ? 0 : license;
            ViewBag.categories = _permCatRep.GetAll().ToList();
            ViewBag.category = category;
            ViewBag.states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            ViewBag.location = location;
            ViewBag.SD = sd;
            ViewBag.ED = ed;
            ViewBag.Status = status;
            List<vInvoice> allInvoices = new List<vInvoice>(); // _invoiceRep.FindBy(C => C.Status == "Paid").ToList();

            //if (string.IsNullOrEmpty(status) || status.ToLower() == "all")
            //{
            //    allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed).ToList();
            //}
            //else
            //    allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed).ToList();

            return View(allInvoices);
        }

        public ActionResult AjaxifyInvoiceIndex(JQueryDataTableParamModel param, string startDate, string endDate, string status, int license,string category, string location)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

           IEnumerable<vInvoice> allInvoices;// = new List<vInvoice>(); // _invoiceRep.FindBy(C => C.Status == "Paid").ToList();

            if (string.IsNullOrEmpty(status) || status.ToLower() == "all")
            {
                if (license == 0)
                {
                    if (string.IsNullOrEmpty(category))
                    {
                        if (string.IsNullOrEmpty(location))
                        {
                            
                        allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed);//.ToList()

                        }
                        else
                        {

                            allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.StateName==location);//.ToList()
                   
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(location))
                        {
                            allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.CategoryName.ToLower() == category.ToLower());//.ToList()
                        }
                        else
                        {

                            allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);//.ToList()
                        
                        }
                    }

                }
                else
                {
                    if (string.IsNullOrEmpty(category))
                    {
                        if (string.IsNullOrEmpty(location))
                        {
                            allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license);//.ToList()
                        }
                        else
                        {

                            allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license && a.StateName == location);//.ToList()
                        
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(location))
                        {
                            allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower());//.ToList()
                        }
                        else
                        {

                            allInvoices = _vInvoRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);//.ToList()
                       
                        }
                    }
                }
            }
            else
            {
                if (license == 0)
                {

                    if (string.IsNullOrEmpty(category))
                    {
                        if (string.IsNullOrEmpty(location))
                        {
                            allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed);//.ToList()
                        }
                        else
                        {
                            allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.StateName == location);//.ToList()
                        
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(location))
                        {
                            allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.CategoryName.ToLower() == category.ToLower());//.ToList()
                        }
                        else
                        {

                            allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);//.ToList()
                        
                        }
                    }
                }
                else
                {

                    if (string.IsNullOrEmpty(category))
                    {
                        if (string.IsNullOrEmpty(location))
                        {
                            allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license);//.ToList()
                        }
                        else
                        {

                            allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license && a.StateName == location);//.ToList()
                     
                        }
                    }
                    else
                    {
                        //
                        if (string.IsNullOrEmpty(location))
                        {
                            allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower());//.ToList()
                        }
                        else
                        {
                            allInvoices = _vInvoRep.FindBy(iv => iv.Status.ToLower() == status).Where(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);//.ToList()
                        
                        }
                    }

                }
            }
            IEnumerable<vInvoice> filteredInvoice;
            var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<vInvoice, string> orderFunction = (c => sortColIndex == 1 ? c.Id != 0 ? c.Id.ToString() : "1"
                : sortColIndex == 2 ? c.Amount.ToString() : sortColIndex == 3 ? c.Status.Trim()
                : sortColIndex == 4 ? c.Date_Paid.ToString() : sortColIndex == 6 ? c.Payment_Code.Trim()
                : sortColIndex == 5 ? c.Payment_Type : c.Date_Added.ToString());

            var sortDirection = Request["sSortDir_0"];
            List<vInvoice> returnedInvoice = new List<vInvoice>();

            #region Select and Sort
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var p = param.sSearch.ToLower();//_vInvoRep.FindBy
                filteredInvoice = allInvoices.Where(a => a.Amount.ToString().Contains(p) || a.CategoryName.Trim().ToLower().Contains(p) ||
                    a.Application_Id.ToString().Contains(p) || a.Payment_Code.Trim().ToLower().Contains(p) ||
                    a.Id.ToString().Trim().ToLower().Contains(p) || a.Status.Trim().ToLower().Contains(p) ||
                    a.Date_Added.ToString().ToLower().Contains(p) || a.Date_Paid.ToString().ToLower().Contains(p) ||
                    a.Payment_Type.ToLower().Contains(p));

                if (sortDirection.ToLower() == "asc")
                {
                    filteredInvoice = filteredInvoice.OrderBy(orderFunction);
                }
                else
                {
                    filteredInvoice = filteredInvoice.OrderByDescending(orderFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filteredInvoice = allInvoices.OrderBy(orderFunction);
                }
                else
                {
                    filteredInvoice = allInvoices.OrderByDescending(orderFunction);
                }
            }
            #endregion

            if (param.iDisplayLength < 0)
            {
                returnedInvoice = filteredInvoice.ToList();
            }
            else
            {
                returnedInvoice = filteredInvoice.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            }
            var result = from c in returnedInvoice
                         select new[] { c.CompanyName, c.LicenseShortName, c.CategoryName, c.Amount.ToString("N2"), c.Status, c.Date_Paid.ToString(), c.Payment_Type, c.Payment_Code, c.Date_Added.ToString(), c.Id.ToString() };




            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allInvoices.Count(),
                iTotalDisplayRecords = filteredInvoice.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Receipt(string startDate, string endDate, int? license, string location)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            ViewBag.licenses = _appIdRep.GetAll().ToList();
            ViewBag.license = license == null ? 0 : license;
            ViewBag.states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            ViewBag.location = location;
            ViewBag.SD = sd;
            ViewBag.ED = ed;
           // <vReceipt> allReceipts;// = new List<vReceipt>();

           //allReceipts = _vReceiptRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed);

            return View(new List<vReceipt>());
        }

        public ActionResult AjaxifyReceipt(JQueryDataTableParamModel param, string startDate, string endDate, int license, string location)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

           IEnumerable <vReceipt> allReceipts;// = new List<vReceipt>(); // _invoiceRep.FindBy(C => C.Status == "Paid").ToList();

           if (license==0)
           {
               if (string.IsNullOrEmpty(location))
               {

                   allReceipts = _vReceiptRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed);//.ToList();
               }
               else
               {

                   allReceipts = _vReceiptRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.StateName==location);//.ToList();
               }
           }
           else
           {

               if (string.IsNullOrEmpty(location))
               {

                   allReceipts = _vReceiptRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license);//.ToList();
               }
               else
               {

                   allReceipts = _vReceiptRep.FindBy(a => a.Date_Paid >= sd && a.Date_Paid <= ed && a.LicenseId == license && a.StateName == location);//.ToList();
               
               }
           }


            IEnumerable<vReceipt> filteredReceipt;
            var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<vReceipt, string> orderFunction = (c => sortColIndex == 1 ? (!string.IsNullOrEmpty(c.ReceiptNo) ? c.ReceiptNo : "")
                : sortColIndex == 2 ? c.ApplicationReference : sortColIndex == 3 ? c.Amount.ToString().Trim() : sortColIndex == 4 ? c.CompanyName.Trim()
                : sortColIndex == 5 ? c.Date_Paid.ToString() : sortColIndex == 6 ? c.RRR.Trim() : c.Payment_Type); // sortColIndex == 6 ? : c.Invoice_open_date.ToString());

            var sortDirection = Request["sSortDir_0"];
            List<vReceipt> returnedInvoice = new List<vReceipt>();

            #region Select and Sort
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var p = param.sSearch.ToLower();
                filteredReceipt = allReceipts.Where(a => a.ReceiptNo.Trim().ToLower().Contains(p) || a.LicenseShortName.Trim().ToLower().Contains(p) || a.CategoryName.Trim().ToLower().Contains(p) || a.ApplicationReference.Trim().ToLower().Contains(p) ||
                    a.Amount.ToString().Contains(param.sSearch) || a.CompanyName.Trim().ToLower().Contains(p) || a.Date_Paid.ToString().ToLower().Contains(p) ||
                    a.RRR.Trim().ToLower().Contains(p) || a.Payment_Type.ToLower().Contains(p));

                if (sortDirection.ToLower() == "asc")
                {
                    filteredReceipt = filteredReceipt.OrderBy(orderFunction);
                }
                else
                {
                    filteredReceipt = filteredReceipt.OrderByDescending(orderFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filteredReceipt = allReceipts.OrderBy(orderFunction);
                }
                else
                {
                    filteredReceipt = allReceipts.OrderByDescending(orderFunction);
                }
            }
            #endregion

            try
            {
                if (param.iDisplayLength < 0)
                {
                    returnedInvoice = filteredReceipt.ToList();
                }
                else
                {
                    returnedInvoice = filteredReceipt.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                }
                var result = from c in returnedInvoice
                             select new[] { c.CompanyName, c.LicenseShortName, c.CategoryName, c.ReceiptNo, c.ApplicationReference, c.Amount.ToString("N2"), c.Date_Paid.ToString(), c.RRR, c.Payment_Type, c.Id.ToString() };



                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = allReceipts.Count(),
                    iTotalDisplayRecords = filteredReceipt.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public ActionResult Details(string id)
        {
            int Id = 0;
            if (!string.IsNullOrEmpty(id))
            {
                int.TryParse(id, out Id);
            }
            vInvoice invoice = _vInvoRep.FindBy(c => c.Id == Id).FirstOrDefault();
            if (invoice != null)
            {
                return View(invoice);
            }
            return View("Error");
        }

        public ActionResult ReceiptDetails(string id)
        {
            int Id = 0;
            vReceipt recpt = new vReceipt();
            if (!string.IsNullOrEmpty(id))
            {
                if(int.TryParse(id, out Id))
                {
                    recpt = _vReceiptRep.FindBy(c => c.Id == Id).FirstOrDefault();
                    if (recpt != null)
                    {
                        return View(recpt);
                    }
                }
            }
            
            // Id not okay, try receipt no
            recpt = _vReceiptRep.FindBy(c => c.ReceiptNo.ToLower() == id.ToLower()).FirstOrDefault();
            if (recpt != null)
            {
                return View(recpt);
            }

            return View("Error");
        }

    }
}