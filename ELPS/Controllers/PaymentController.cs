using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ELPS.Domain.Abstract;
using System.Web.Http.Description;
using ELPS.Domain.Entities;
using ELPS.Models;
using System.Configuration;
using ELPS.Helpers;
using System.Net;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using ELPS.Domain;
using System.IO;
using Rotativa;
using System.Net.Mail;
using System.Transactions;

namespace ELPS.Controllers
{
    public class PaymentController : Controller
    {
        IStateRepository _stateRep;
        IPermitCategoryRepository _permCatRep;
        IAppIdentityRepository _licensRep;
        IReceiptRepository _recptRep;
        IInvoiceRepository _invoiceRep;

        IvApplicationRepository _vAppRep;
        IApplicationRepository _appRep;
        IPayment_TransactionRepository _payTransRep;
        IvPaymentTransactionRepository _vPayTransRep;
        IApplicationRequirementRepository _appReqRep;
        ICompanyRepository _compRep;
        IDocument_TypeRepository _docTRep;
        IRemitaPaymentStatusRepository _remPStatRep;
        IvZoneRepository _vZoneRep;
        IvBranchRepository _vBranchRep;
        IAppIdentityRepository _appIdentityRep;
        IvZoneStateRepository _vZoneStateRep;

        string _marchantId = ConfigurationManager.AppSettings["merchantID"];
        string _responseUrl = ConfigurationManager.AppSettings["RemitaPaymentCallback"];
        string _apiKey = ConfigurationManager.AppSettings["rKey"];


        public PaymentController(IPayment_TransactionRepository payTransRep, IApplicationRequirementRepository appReqRep, ICompanyRepository compRep,
            IDocument_TypeRepository docTRep, IRemitaPaymentStatusRepository remPStatRep, IApplicationRepository appRep,
            IvPaymentTransactionRepository vPayTransRep, IAppIdentityRepository licensRep, IReceiptRepository recptRep, IInvoiceRepository invoiceRep,
            IPermitCategoryRepository permCatRep, IStateRepository stateRep, IvApplicationRepository vAppRep, IvZoneRepository vZoneRep,
            IvBranchRepository vBranchRep, IAppIdentityRepository appIdentityRep, IvZoneStateRepository vZoneStateRep)
        {
            _vZoneStateRep = vZoneStateRep;
            _appIdentityRep = appIdentityRep;
            _vZoneRep = vZoneRep;
            _vBranchRep = vBranchRep;
            _vAppRep = vAppRep;
            _stateRep = stateRep;
            _permCatRep = permCatRep;
            _licensRep = licensRep;
            _recptRep = recptRep;
            _invoiceRep = invoiceRep;
            _payTransRep = payTransRep;
            _appReqRep = appReqRep;
            _compRep = compRep;
            _docTRep = docTRep;
            _remPStatRep = remPStatRep;
            _appRep = appRep;
            _vPayTransRep = vPayTransRep;
        }


        #region MANUAL VALUE FOR APPLICATIONS
        public ActionResult Pay_Test(string rrr)
        {
            //return RedirectToAction("Remita", new { orderId = trans.Order_Id, RRR = rrr });

            if (string.IsNullOrEmpty(rrr))
            {
                return View("PaymentFairlure");
            }
            var trans = _payTransRep.FindBy(a => a.RRR.ToLower() == rrr.ToLower()).FirstOrDefault();
            if (trans != null)
            {
                var docs = new List<Document_Type>();
                if (!string.IsNullOrEmpty(trans.ApplicationItem))
                {
                    var apr = JsonConvert.DeserializeObject<List<ApplicationItem>>(trans.ApplicationItem); // _appReqRep.FindBy(a => a.TransactionId == trans.Id).ToList();

                    trans.ApplicationItems = apr;
                }

                if (!string.IsNullOrEmpty(trans.DocumentType))
                {
                    var dts = trans.DocumentType.Trim(';').Split(';');
                    if (dts.Any())
                    {
                        foreach (var item in dts)
                        {
                            var id = Convert.ToInt32(item);
                            var d = _docTRep.FindBy(a => a.Id == id).FirstOrDefault();
                            if (d != null)
                            {
                                docs.Add(d);
                            }
                        }
                        trans.Document_Types = docs;
                    }
                }


                var rm = new RemitaSplit();
                var comp = _compRep.FindBy(a => a.Id == trans.CompanyId).FirstOrDefault();
                rm.payerPhone = comp.Contact_Phone;

                rm.merchantId = _marchantId;// wp.merchantId;
                rm.orderId = trans.Order_Id; // ptrans == null ? app.Reference : refe;// wp.txn_ref;
                rm.payerEmail = comp.User_Id;
                rm.payerName = trans.CompanyName;// wp.cust_name;
                //rm.responseurl = ConfigurationManager.AppSettings["RemitaPaymentCallback"];// wp.site_redirect_url;
                //rm.serviceTypeId = ServiceTypeId;// wp.serviceTypeId;
                rm.amount = trans.Transaction_Amount;// wp.amount;

                string hashItem2 = _marchantId + trans.RRR.Trim() + _apiKey;
                string hash2 = PaymentRef.getHash(hashItem2);

                //rm.hash = hash2.ToLower();

                ViewBag.rrr = rrr;
                ViewBag.webPayData = rm;

                var app = _appRep.FindBy(a => a.OrderId == trans.Order_Id).FirstOrDefault();
                if (app == null)
                {
                    return View("Error");
                }
                else
                {
                    var portal = _licensRep.FindBy(a => a.Id == app.LicenseId).FirstOrDefault();
                    if (portal == null)
                    {
                        return View("Error");
                    }
                    ViewBag.License = portal.LicenseName;
                    ViewBag.LicenseShortName = portal.ShortName;
                }

                return View("nouse_Pay", trans);
            }
            return View("PaymentFairlure");
        }





        public ActionResult Remita_Test(string status, string statuscode, string RRR, string orderId)
        {
            //record all the transaction
            #region record transaction
            var trans = _payTransRep.FindBy(a => a.Order_Id == orderId).FirstOrDefault();
            var vapp = _vAppRep.FindBy(a => a.OrderId == orderId).FirstOrDefault();
            var comp = _compRep.FindBy(a => a.Id == vapp.CompanyId).FirstOrDefault();
            if (trans != null)
            {
                var ReturnUrl = "";
                var frm = string.Empty;
                frm = "<form action='{0}' id='frmTest' method='post'>" +
                    "<input type='hidden' name='status' value='{1}' />" +
                    "<input type='hidden' name='statuscode' value='{2}' />" +
                    "<input type='hidden' name='orderId' value='{3}' />" +
                    "<input type='hidden' name='RRR' value='{4}' /></form><script>document.getElementById('frmTest').submit();</script>";


                ReturnUrl = trans.ReturnSuccessUrl;


                //var resp = JsonConvert.DeserializeObject<RemitaResponse>(output);
                var resp = new RemitaResponse()
                {
                    status = "01",
                    transactiontime = DateTime.Now.ToString(),
                    RRR = RRR,
                    orderId = orderId,
                    statusMessage = "Approved"
                };
                if (resp != null && resp.status == "01")
                {
                    ReturnUrl = trans.ReturnSuccessUrl;
                    trans.Completed = true;
                    trans.Response_Code = resp.status;
                    trans.Response_Description = resp.statusMessage;
                    _payTransRep.Edit(trans);
                    _payTransRep.Save(Request.Url.Host, Request.UserHostAddress);

                    #region invoice

                    var app = _appRep.FindBy(a => a.OrderId == trans.Order_Id).FirstOrDefault();
                    if (app != null)
                    {
                        app.Status = ApplicationStatus.PaymentCompleted;
                        _appRep.Edit(app);
                        _appRep.Save(Request.Url.Host, Request.UserHostAddress);
                    }

                    //Update Invoice and Create Receipt

                    var invo = _invoiceRep.FindBy(a => a.Payment_Code == trans.Order_Id).FirstOrDefault();
                    if (invo != null)
                    {
                        invo.Status = "Paid";
                        invo.Date_Paid = Convert.ToDateTime(resp.transactiontime);

                        _invoiceRep.Edit(invo);
                        _invoiceRep.Save(Request.UserHostName, Request.UserHostAddress);

                    }

                    #endregion
                    //get the License receipt Code

                    var lc = _licensRep.FindBy(a => a.Id == app.LicenseId).FirstOrDefault();


                    #region Receipt
                    var rpt = _recptRep.FindBy(a => a.ApplicationReference == trans.Order_Id).FirstOrDefault();
                    if (rpt == null)
                    {

                        rpt = new Receipt
                        {
                            Amount = Convert.ToDouble(trans.Transaction_Amount),
                            ApplicationId = app.Id,
                            ApplicationReference = trans.Order_Id,
                            CompanyName = trans.CompanyName,
                            Date_Paid = Convert.ToDateTime(resp.transactiontime),
                            InvoiceId = Convert.ToInt32(invo.Id),
                            ReceiptNo = "---",// UtilityHelper.GenerateReceiptNo(rpt.Amount, app.Id, lc.ReceiptCode);
                            RRR = trans.RRR,
                            Status = "Paid"
                        };
                        _recptRep.Add(rpt);
                        _recptRep.Save(Request.UserHostName, Request.UserHostAddress);

                        rpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rpt.Amount, rpt.Id, lc.ReceiptCode, rpt.Date_Paid);
                        _recptRep.Edit(rpt);
                        _recptRep.Save(User.Identity.Name, Request.UserHostAddress);
                    }
                    #endregion

                    #region send Mail
                    var body = "";
                    using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "GeneralFormat.txt"))
                    {
                        body = sr.ReadToEnd();
                    }

                    var apDetails = string.Format("This is to notify you that we have received payment for your " + lc.ShortName + " Permit Application with Reference Number: {0}.<br />Please Find the Attached Receipt file for Details.", app.OrderId);



                    // string subject = msg.Subject;  "Payment Received: " + app.Reference;
                    var msgbody = string.Format(body, "Receipt Confirmation", apDetails);
                    //var body = mailhelper.getmailbody(callbackurl, model.email);

                    //var pdf = new ActionAsPdf("getReceiptPdf", new { orderId = orderId, recptId = rpt.Id }) { FileName = "ElpsPaymentReceipt.pdf" };
                    //var binary = pdf.BuildPdf(this.ControllerContext);


                    MailHelper.SendEmail(comp.User_Id, "Receipt Confirmation", msgbody);//, new Attachment(new MemoryStream(binary), "Elps-Payment-Receipt.pdf"));

                    #endregion

                }
                else
                {
                    ReturnUrl = trans.ReturnFailureUrl;
                }

                frm = string.Format(frm, ReturnUrl, resp.status, resp.statusMessage, resp.orderId, resp.RRR.Trim());

                return Content(frm);
            }

            return View("PaymentFailure");
            #endregion
        }

        #endregion






        public ActionResult Pay(string rrr)
        {
            if (string.IsNullOrEmpty(rrr))
            {
                return View("PaymentFairlure");
            }
            var trans = _payTransRep.FindBy(a => a.RRR.ToLower() == rrr.ToLower()).FirstOrDefault();
            if (trans != null)
            {
                //Check if payment has been made
                if (trans.Response_Code == "01")
                {
                    //Payment made already, redirect to Give value
                    return RedirectToAction("Remita", new { RRR = trans.RRR, orderId = trans.Order_Id });
                }
                else // ()
                {
                    #region call Back api [Verify payment from REMITA]

                    string hashItem = trans.RRR.Trim() + _apiKey + _marchantId;
                    string hash = PaymentRef.getHash(hashItem, false).ToLower();

                    //http://www.remitademo.net/remita/ecomm/merchantId/RRR/hash/RESPONSE_TYPE/status.reg
                    //var url = string.Format("http://www.remitademo.net/remita/ecomm/{0}/{1}/{2}/json/status.reg", MerchantId, RRR, hash.ToLower());
                    var url = string.Format(ConfigurationManager.AppSettings["RemitaStatus_RRR"].ToString(), _marchantId, trans.RRR.Trim(), hash);

                    string output = "";
                    using (WebClient client = new WebClient())
                    {
                        // performs an HTTP POST
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                        client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                        output = client.DownloadString(url);
                    }
                    var resp = JsonConvert.DeserializeObject<RemitaResponse>(output);
                    
                    if (resp != null && resp.status == "01")
                    {
                        //Payment made already, redirect to Give value
                        return RedirectToAction("Remita", new { RRR = resp.RRR, orderId = resp.orderId });
                    }
                    #endregion
                }

                var docs = new List<Document_Type>();
                if (!string.IsNullOrEmpty(trans.ApplicationItem))
                {
                    var apr = JsonConvert.DeserializeObject<List<ApplicationItem>>(trans.ApplicationItem); // _appReqRep.FindBy(a => a.TransactionId == trans.Id).ToList();

                    trans.ApplicationItems = apr;
                }

                if (!string.IsNullOrEmpty(trans.DocumentType))
                {
                    var dts = trans.DocumentType.Trim(';').Split(';');
                    if (dts.Any())
                    {
                        foreach (var item in dts)
                        {
                            var id = Convert.ToInt32(item);
                            var d = _docTRep.FindBy(a => a.Id == id).FirstOrDefault();
                            if (d != null)
                            {
                                docs.Add(d);
                            }
                        }
                        trans.Document_Types = docs;
                    }
                }


                var rm = new RemitaSplit();
                var comp = _compRep.FindBy(a => a.Id == trans.CompanyId).FirstOrDefault();
                rm.payerPhone = comp.Contact_Phone;

                rm.merchantId = _marchantId;// wp.merchantId;
                rm.orderId = trans.Order_Id; // ptrans == null ? app.Reference : refe;// wp.txn_ref;
                rm.payerEmail = comp.User_Id;
                rm.payerName = trans.CompanyName;// wp.cust_name;
                rm.responseurl = ConfigurationManager.AppSettings["RemitaPaymentCallback"];// wp.site_redirect_url;
                //rm.serviceTypeId = ServiceTypeId;// wp.serviceTypeId;
                rm.amount = trans.Transaction_Amount;// wp.amount;

                string hashItem2 = _marchantId + trans.RRR.Trim() + _apiKey;
                string hash2 = PaymentRef.getHash(hashItem2, false);
                rm.hash = hash2.ToLower();
                ViewBag.rrr = rrr;
                ViewBag.webPayData = rm;

                var app = _appRep.FindBy(a => a.OrderId == trans.Order_Id).FirstOrDefault();
                if (app == null)
                {
                    return View("Error");
                }
                else
                {
                    var portal = _licensRep.FindBy(a => a.Id == app.LicenseId).FirstOrDefault();
                    if (portal == null)
                    {
                        return View("Error");
                    }
                    ViewBag.License = portal.LicenseName;
                    ViewBag.LicenseShortName = portal.ShortName;
                }

                return View(trans);
            }
            return View("PaymentFairlure");
        }





        /// <summary>
        /// Callback URL for Remita after any transaction
        /// </summary>
        /// <param name="status"></param>
        /// <param name="statuscode"></param>
        /// <param name="RRR"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public ActionResult Remita(string status, string statuscode, string RRR, string orderId)
        {
            //record all the transaction
            #region record transaction
            var trans = _payTransRep.FindBy(a => a.Order_Id == orderId).FirstOrDefault();
            UtilityHelper.LogMessage("/Payment/Remita" + Environment.NewLine + trans == null ? "Payment Transaction not found" : "Found Payment Transaction >> RRR: " + trans.RRR + ": Ref No: " + trans.Reference_Number + ": Status Code: " + trans.Response_Code);
            if (trans != null)
            {
                var vapp = _vAppRep.FindBy(a => a.OrderId == orderId).FirstOrDefault();
                UtilityHelper.LogMessage("/Payment/Remita" + Environment.NewLine + trans == null ? "Application not found" : "Found Application");
                var comp = _compRep.FindBy(a => a.Id == vapp.CompanyId).FirstOrDefault();
                UtilityHelper.LogMessage("/Payment/Remita" + Environment.NewLine + trans == null ? "Company not found" : "Company Found: '" + comp.Name + "'");

                var ReturnUrl = "";
                var frm = string.Empty;
                frm = "<form action='{0}' id='frmTest' method='post'>" +
                    "<input type='hidden' name='status' value='{1}' />" +
                    "<input type='hidden' name='statuscode' value='{2}' />" +
                    "<input type='hidden' name='orderId' value='{3}' />" +
                    "<input type='hidden' name='RRR' value='{4}' /></form><script>document.getElementById('frmTest').submit();</script>";

                _NewRemitaResponse resp;
                if (!ProcessPayment(trans, comp.User_Id, out resp))
                {
                    ReturnUrl = trans.ReturnFailureUrl;
                    UtilityHelper.LogMessage("/Payment/Remita" + Environment.NewLine + "Application payment NOT successful");
                }
                else
                {
                    ReturnUrl = trans.ReturnSuccessUrl;
                    UtilityHelper.LogMessage("/Payment/Remita" + Environment.NewLine + "Application payment successful");
                }

                frm = string.Format(frm, ReturnUrl, resp.status, resp.message, resp.orderId, resp.RRR.Trim());

                return Content(frm);
            }

            return View("PaymentFailure");
            #endregion
        }




        /// <summary>
        /// Called from Child Portal to give value to a specific application
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="email"></param>
        /// <param name="apiHash"></param>
        /// <returns></returns>
        public ActionResult GiveRemitaValue(string reference, string email, string apiHash)
        {
            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { responseCode = 0, message = "App UserName cannot be empty" }, JsonRequestBehavior.AllowGet);

            }
            //check if app is registered
            var app = _licensRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {
                return Json(new { responseCode = 0, message = "App has been denied Access, Contact NUPRC Dev" }, JsonRequestBehavior.AllowGet);
            }

            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                return Json(new { responseCode = 0, message = "App has been denied Access, Contact NUPRC Dev" }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(reference) || string.IsNullOrEmpty(reference))
            {
                // Err: 400
                return Json(new { responseCode = 0, message = "Please check the entered values and try again." }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            var trans = _payTransRep.FindBy(a => a.Reference_Number == reference || a.Order_Id == reference).FirstOrDefault();
            var coy = _compRep.FindBy(a => a.Id == trans.CompanyId).FirstOrDefault();
            var resp = new _NewRemitaResponse();

            if (ProcessPayment(trans, coy.User_Id, out resp))
            {
                return Json("01", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("00", JsonRequestBehavior.AllowGet);
            }
        }




        private bool ProcessPayment(Payment_Transaction trans, string userEmail, out _NewRemitaResponse resp)
        {
            if (trans.Response_Code == "01")
            {
                resp = new _NewRemitaResponse()
                {
                    RRR = trans.RRR,
                    orderId = trans.Order_Id,
                    status = trans.Response_Code,
                    amount = trans.Transaction_Amount,
                    message = trans.Response_Description,
                    paymentDate = trans.Transaction_Date
                };
                UtilityHelper.LogMessage("/Payment/ProcessPayment" + Environment.NewLine + "Payment Transaction Code: " + trans.Response_Code);
            }
            else
            {
                #region call Back api [Verify payment from REMITA]

                string hashItem = trans.RRR.Trim() + _apiKey + _marchantId;
                string hash = PaymentRef.getHash(hashItem, true).ToLower();

                var url = string.Format(ConfigurationManager.AppSettings["RemitaStatus_RRR"].ToString(), _marchantId, trans.RRR.Trim(), hash);

                string output = "";
                using (WebClient client = new WebClient())
                {
                    // performs an HTTP POST
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                     client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                    output = client.DownloadString(url);
                }
                #endregion
                UtilityHelper.LogMessage("/Payment/ProcessPayment" + Environment.NewLine + "Response from Remita: " + output);
                resp = JsonConvert.DeserializeObject<_NewRemitaResponse>(output);
            }

            if (resp != null && (resp.status == "01" || resp.status == "00" || resp.message == "Successful"))
            {
                trans.Response_Code = resp.status;
                trans.Response_Description = "Payment Completed";
                trans.Completed = true;
                _payTransRep.Edit(trans);
                _payTransRep.Save(Request.Url.Host, Request.UserHostAddress);

                #region invoice
                var app = _appRep.FindBy(a => a.OrderId == trans.Order_Id).FirstOrDefault();
                if (app != null)
                {
                    app.Status = ApplicationStatus.PaymentCompleted;
                    _appRep.Edit(app);
                    _appRep.Save(Request.Url.Host, Request.UserHostAddress);
                    UtilityHelper.LogMessage("/Payment/ProcessPayment" + Environment.NewLine + "Application updated to PAYMENT COMPLETED [" + app.OrderId + "]");
                }

                // Update Invoice and Create Receipt
                var invo = _invoiceRep.FindBy(a => a.Payment_Code == trans.Order_Id).FirstOrDefault();
                if (invo != null)
                {
                    invo.Status = "Paid";
                    invo.Date_Paid = Convert.ToDateTime(resp.paymentDate);

                    _invoiceRep.Edit(invo);
                    _invoiceRep.Save(Request.UserHostName, Request.UserHostAddress);
                    UtilityHelper.LogMessage("/Payment/ProcessPayment" + Environment.NewLine + "Invoice updated to PAID [" + invo.Amount + "]");
                }

                #endregion
                //get the License receipt Code

                var lc = _licensRep.FindBy(a => a.Id == app.LicenseId).FirstOrDefault();
                #region Receipt
                var rpt = _recptRep.FindBy(a => a.ApplicationReference == trans.Order_Id).FirstOrDefault();
                if (rpt == null)
                {
                    rpt = new Receipt
                    {
                        Amount = Convert.ToDouble(trans.Transaction_Amount),
                        ApplicationId = app.Id,
                        ApplicationReference = trans.Order_Id,
                        CompanyName = trans.CompanyName,
                        Date_Paid = Convert.ToDateTime(resp.paymentDate),
                        InvoiceId = Convert.ToInt32(invo.Id),
                        ReceiptNo = "---",
                        RRR = trans.RRR,
                        Status = "Paid"
                    };
                    _recptRep.Add(rpt);
                    _recptRep.Save(Request.UserHostName, Request.UserHostAddress);

                    rpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rpt.Amount, rpt.Id, lc.ReceiptCode, rpt.Date_Paid);
                    _recptRep.Edit(rpt);
                    _recptRep.Save(User.Identity.Name, Request.UserHostAddress);
                    UtilityHelper.LogMessage("/Payment/ProcessPayment" + Environment.NewLine + "Receipt Created: " + rpt.ReceiptNo);
                }
                #endregion

                #region send Mail
                //var dt = comp.Date.Day.ToString() + comp.Date.Month.ToString() + comp.Date.Year.ToString();
                //var sn = msg.Id;
                var body = "";
                using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "GeneralFormat.txt"))
                {
                    body = sr.ReadToEnd();
                }
                var apDetails = string.Format("This is to notify you that we have received payment for your " + lc.ShortName + " Permit Application with Reference Number: {0}.<br />Please Find the Attached Receipt file for Details.", app.OrderId);

                //var apDetails = string.Format("This is to notify you that we have received payment for your OGISP Permit Application with Reference Number: {0}.<br />Please Find the Attached Receipt file for Details.", app.OrderId);



                // string subject = msg.Subject;  "Payment Received: " + app.Reference;
                var msgbody = string.Format(body, "Receipt Confirmation", apDetails);
                //var body = mailhelper.getmailbody(callbackurl, model.email);

                var pdf = new ActionAsPdf("getReceiptPdf", new { orderId = trans.Order_Id, recptId = rpt.Id }) { FileName = "ElpsPaymentReceipt_" + app.OrderId + ".pdf" };

                var binary = pdf.BuildPdf(this.ControllerContext);


                //MailHelper.SendEmail(userEmail, "Receipt Confirmation", msgbody, new Attachment(new MemoryStream(binary), "ElpsPaymentReceipt_" + app.OrderId + ".pdf"));


                //MailHelper.SendEmail(comp.User_Id, "Receipt Confirmation", msgbody);
                #endregion

                return true;
            }
            else
            {
                //ReturnUrl = trans.ReturnFailureUrl;
                return false;
            }
        }






        public ActionResult GetReceiptPdf(string orderId, int recptId)
        {
            var app = _vAppRep.FindBy(a => a.OrderId == orderId).FirstOrDefault();

            var rcpt = _recptRep.FindBy(a => a.ApplicationReference == app.OrderId).FirstOrDefault();
            ViewBag.receipt = rcpt;
            if (!string.IsNullOrEmpty(app.ApplicationItem))
            {
                var apr = JsonConvert.DeserializeObject<List<ApplicationItem>>(app.ApplicationItem);
                app.ApplicationItems = new List<ApplicationItem>();
                app.ApplicationItems.AddRange(apr);
            }

            if (app.ApplicationItems != null && app.ApplicationItems.Any() && app.ApplicationItems.Where(a => a.Name.ToLower() == "paybreak").FirstOrDefault() != null)
            {
                ViewBag.PayBreak = app.ApplicationItems.Where(a => a.Name.ToLower() == "paybreak").FirstOrDefault().Description;
                UtilityHelper.LogMessage("Payment Breakdown: " + ViewBag.PayBreak);
            }


            return View(app);
        }

        [HttpPost]
        public ActionResult RemitaPayment(string paymentType, string orderId, string rrr)
        {
            var trans = _payTransRep.FindBy(a => a.RRR == rrr).FirstOrDefault();

            if (trans == null)
            {
                return View("Error");
            }

            if (trans != null)
            {
                var ReturnUrl = trans.ReturnBankPaymentUrl;
                var frm = "<form action='" + ReturnUrl + "' id='frmTest' method='post'>" +
                    "<input type='hidden' name='paymentType' value='" + paymentType + "' />" +
                    "<input type='hidden' name='orderId' value='" + orderId + "' />" +
                    "<input type='hidden' name='RRR' value='" + rrr + "' /></form><script>document.getElementById('frmTest').submit();</script>";

                return Content(frm);
            }

            return View("Error");
        }





        /// <summary>
        /// Bounced to for BANK Payments from Child portal
        /// </summary>
        /// <param name="model"></param>
        /// <returns>List of Payment Responses from Remita</returns>
        [HttpPost, AllowAnonymous]
        public ActionResult RemitaPay(List<RemitaPaymentStatus> model)
        {
            List<string> errors = new List<string>();
            List<string> orders = new List<string>();
            int counter = 0;
            if (model != null && model.Count > 0)
            {
                var remP = new List<RemitaPaymentStatus>();
                #region Loop tru Model
                UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Processing Payment: " + model.Count() + " payments sent in");
                foreach (var rps in model)
                {
                    counter++;
                    // var rps = model.FirstOrDefault();
                    if (!string.IsNullOrEmpty(rps.rrr))
                    {
                        //Atleast something was Sent
                        //lets keep this Value first
                        try
                        {
                            var rpt = _remPStatRep.FindBy(a => a.rrr.ToLower().Trim() == rps.rrr.ToLower().Trim()).FirstOrDefault();
                            if (rpt == null)
                            {
                                //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + counter + ": >> RPT: Not avail, Saving...");
                                rps.strId = rps.Id.ToString();
                                DateTime dd; // df;
                                if(DateTime.TryParse(rps.dateRequested, out dd))
                                {
                                    rps.Date_Requested = dd;
                                }
                                else
                                {
                                    rps.Date_Requested = UtilityHelper.CurrentTime;
                                }
                                //if(DateTime.TryParse(rps.dateSent, out df))
                                //{
                                //    rps.Date_Sent = df;
                                //}
                                //else
                                //{
                                //    rps.Date_Sent = UtilityHelper.CurrentTime;
                                //}
                                _remPStatRep.Add(rps);
                                _remPStatRep.Save("Remita", "Remita");

                                rpt = _remPStatRep.FindBy(a => a.rrr.ToLower().Trim() == rps.rrr.ToLower().Trim()).FirstOrDefault();
                            }


                            //get the Transactions
                            var trans = _payTransRep.FindBy(a => a.RRR == rps.rrr || a.Order_Id.ToLower() == rps.orderRef.ToLower()).FirstOrDefault();

                            if (trans != null)
                            {
                                #region record transaction

                                string hash_string = trans.Order_Id + _apiKey + _marchantId;
                                string hash = PaymentRef.getHash(hash_string).ToLower();

                                var url = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, _marchantId, trans.Order_Id, hash);
                                _NewRemitaResponse response = new _NewRemitaResponse();

                                using (WebClient client = new WebClient())
                                {
                                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                     client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                                    string responseJson = client.DownloadString(url);
                                    //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Response from Remita: >>>   " + responseJson);
                                    response = JsonConvert.DeserializeObject<_NewRemitaResponse>(responseJson);
                                    if (response != null && (response.status == "01" || response.status == "00"))
                                    {
                                        var RRR = response.RRR;
                                        // update tarnsaction
                                        trans.Completed = true;
                                        trans.Response_Code = response.status;
                                        trans.Response_Description = response.message;
                                        trans.Type = rpt != null ? rpt.channnel : "BRANCH";
                                        if (string.IsNullOrEmpty(trans.RRR))
                                            trans.RRR = rpt.rrr;
                                        trans.PaymentSource = rpt.channnel;
                                        trans.Transaction_Date = response.paymentDate;
                                        trans.TransactionDate = DateTime.Parse(trans.Transaction_Date);

                                        _payTransRep.Edit(trans);
                                        _payTransRep.Save("Remita", "Remita");
                                        //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Payment Transaction Updated to COMPLETED [" + trans.Order_Id + "]");

                                        #region Payment Valid and Approved, Give value

                                        rps.BankPaymentEndPoint = trans.ReturnBankPaymentUrl;
                                        rps.orderRef = string.IsNullOrEmpty(rps.orderRef) ? trans.Order_Id : rps.orderRef;
                                        remP.Add(rps);
                                        //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "New Remita Payment Status Added [" + rps.rrr + " >> " + rps.orderRef + "]");
                                        var app = _appRep.FindBy(a => a.OrderId == trans.Order_Id).FirstOrDefault();
                                        if (app != null)
                                        {
                                            app.Status = ApplicationStatus.PaymentCompleted;
                                            _appRep.Edit(app);
                                            _appRep.Save(Request.Url.Host, Request.UserHostAddress);
                                            //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Application updated to PAYMENT COMPLETED [" + app.OrderId + "]");
                                        }

                                        //Update Invoice and Create Receipt
                                        var invo = _invoiceRep.FindBy(a => a.Payment_Code == trans.Order_Id).FirstOrDefault();
                                        if (invo != null)
                                        {
                                            invo.Status = "Paid";
                                            invo.Date_Paid = Convert.ToDateTime(response.paymentDate);

                                            _invoiceRep.Edit(invo);
                                            _invoiceRep.Save(Request.UserHostName, Request.UserHostAddress);
                                            //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Invoice Updated to PAID [" + invo.Amount + "]");
                                        }

                                        //get the License receipt Code

                                        var lc = _licensRep.FindBy(a => a.Id == app.LicenseId).FirstOrDefault();

                                        var rcpt = _recptRep.FindBy(a => a.RRR.Trim().ToLower() == trans.RRR.Trim().ToLower()).FirstOrDefault();
                                        if (rcpt == null)
                                        {
                                            #region Create Receipt
                                            rcpt = new Receipt
                                            {
                                                Amount = Convert.ToDouble(trans.Transaction_Amount),
                                                ApplicationId = app.Id,
                                                ApplicationReference = trans.Order_Id,
                                                CompanyName = trans.CompanyName,
                                                Date_Paid = Convert.ToDateTime(response.paymentDate),
                                                InvoiceId = Convert.ToInt32(invo.Id),
                                                ReceiptNo = "---", // UtilityHelper.GenerateReceiptNo(rcpt.Amount, app.Id, lc.ReceiptCode);
                                                RRR = trans.RRR,
                                                Status = "Paid"
                                            };
                                            _recptRep.Add(rcpt);
                                            _recptRep.Save(Request.UserHostName, Request.UserHostAddress);

                                            rcpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rcpt.Amount, rcpt.Id, lc.ReceiptCode, rcpt.Date_Paid);
                                            _recptRep.Edit(rcpt);
                                            _recptRep.Save(User.Identity.Name, Request.UserHostAddress);
                                            #endregion
                                        }

                                        #region send Mail
                                        var comp = _compRep.FindBy(a => a.Id == app.CompanyId).FirstOrDefault();
                                        var body = "";
                                        using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "GeneralFormat.txt"))
                                        {
                                            body = sr.ReadToEnd();
                                        }

                                        var apDetails = string.Format("This is to notify you that we have received payment for your " + lc.ShortName + " Permit Application with Reference Number: {0}." + Environment.NewLine + "Please Find the Attached Receipt file for Details.", app.OrderId);
                                        var msgbody = string.Format(body, "NUPRC Receipt Confirmation: " + app.OrderId, apDetails);

                                        var pdf = new ActionAsPdf("getReceiptPdf", new { orderId = trans.Order_Id, recptId = rpt.Id }) { FileName = "ElpsPaymentReceipt_" + app.OrderId + ".pdf" };
                                        var binary = pdf.BuildPdf(this.ControllerContext);

                                        MailHelper.SendEmail(comp.User_Id, "NUPRC Receipt Confirmation", msgbody, new Attachment(new MemoryStream(binary), "ElpsPaymentReceipt_" + app.OrderId + ".pdf"));
                                        orders.Add(app.OrderId);
                                        #endregion

                                        #endregion
                                    }
                                    else
                                    {
                                        errors.Add("RRR: " + rps.rrr + " failed" + response.message);
                                        UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Payment not confirmed [" + rps.rrr + "]");
                                    }
                                }

                                #endregion
                            }
                            else
                            {
                                errors.Add("RRR: " + rps.rrr + "; transaction not found");
                                UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Transaction not found [" + rps.rrr + "]");

                                // Log as Orphan Payments
                                rpt.Orphan = true;
                                _remPStatRep.Edit(rpt);
                                _remPStatRep.Save("Remita", "Remita");
                            }
                        }
                        catch (Exception ex)
                        {
                            //return Content("An Error occured: " + ex.Message);
                            errors.Add("RRR: " + rps.rrr + " failed >> More: " + (ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message));
                            continue;
                        }
                    }
                }

                if (errors.Any())
                {
                    var sub = "Error from payment on ELPS";
                    var msg = "Errors encountered from Payment on ELSP. <br /> ";
                    foreach (var err in errors)
                    {
                        msg += err + "<br />";
                    }
                    MailHelper.SendEmail("errors@siga.33mail.com", sub, msg);
                }
                else
                {
                    UtilityHelper.LogMessage("No error while processing send Payments");
                }

                if (remP.Count > 0)
                {
                    #region
                    var remps = remP.GroupBy(a => a.BankPaymentEndPoint).ToList();
                    foreach (var items in remps)
                    {
                        UtilityHelper.LogMessage("Group (Bank EndPoint): " + items.Key);

                        var rpl = new List<RemitaPaymentStatus>();
                        //get the Endpoint Url
                        var bep = items.Key;
                        foreach (var item in items)
                        {
                            item.BankPaymentEndPoint = string.Empty;
                            //item.IsCompleted
                            rpl.Add(item);
                        }

                        //Post to the EndPoint
                        var rplJ = JsonConvert.SerializeObject(rpl);
                        //UtilityHelper.LogMessage(rplJ);

                        var rUrl = bep;
                        string output = "";
                        string eMsg = "";
                        using (WebClient client = new WebClient())
                        {
                            // performs an HTTP POST
                            try
                            {
                                //client.Headers[HttpRequestHeader.Accept] = "application/json";
                                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                output = client.UploadString(rUrl, "POST", rplJ);
                                //UtilityHelper.LogMessage(output);
                                UtilityHelper.LogMessage("Project URL: " + rUrl);
                            }
                            catch (Exception x)
                            {
                                eMsg = x.Message;
                                UtilityHelper.LogMessage("Error as I tried calling owner project to get its payment. >>> " + eMsg);
                            }
                        }
                        output = output.Replace("jsonp (", "");
                        output = output.Replace(")", "");

                        try
                        {
                            var resp = JsonConvert.DeserializeObject<List<int>>(output);
                            if (resp.Count > 0)
                            {
                                UtilityHelper.LogMessage("Response deserialize successfully");
                                return Content("All ok");
                            }
                        }
                        catch (Exception)
                        {
                            UtilityHelper.LogMessage("Cannot Deserialize the response from Project");
                        }
                    }
                    #endregion
                }

                #endregion
            }
            return Content("No Data is received");
        }





        [Authorize]
        public ActionResult Transactions(string startDate, string endDate, int? license, string status, string category, string filterby, int? filterparam, string location)
        {
            // Use date range, but if not supplied, use last 30 days as the default
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            int p = filterparam.GetValueOrDefault(0);
            ViewBag.param = p;
            ViewBag.by = filterby;
            ViewBag.Status = status;
            var mm = "for ";

            var ls = new List<AppIdentity>();
            if (HttpContext.Cache["_licenses_"] == null)
            {
                ls = _licensRep.FindBy(a => a.IsActive).ToList(); //.GetAll().ToList();
                HttpContext.Cache["_licenses_"] = ls;
            }
            else
            {
                ls = (List<AppIdentity>)HttpContext.Cache["_licenses_"];
            }
            mm += $"{(license == null ? "All Licenses/Permits" : ls.Where(a => a.Id == license).FirstOrDefault()?.ShortName)} ";
            ViewBag.licenses = ls;
            ViewBag.license = license == null ? 0 : license;
            var pcg = _permCatRep.GetAll().ToList();
            mm += $"({(string.IsNullOrEmpty(category) ? "All Categories" : pcg.Where(a => a.Name.ToLower() == category.ToLower()).FirstOrDefault()?.Name)}) ";
            ViewBag.categories = pcg;
            ViewBag.category = category;
            //ViewBag.states = _stateRep.FindBy(a => a.CountryId == 156).ToList();// _stateRep.GetAll().ToList();// _permCatRep.GetAll().ToList();
            ViewBag.location = location;
            ViewBag.SD = sd;
            ViewBag.ED = ed;

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
                        var z = filter.Zones.Where(a => a.Id == p).FirstOrDefault();
                        mm += "Filtered by " + z.Name;
                        break;
                    case "fd":
                        var f = filter.Branches.Where(a => a.Id == p).FirstOrDefault();
                        mm += "Filtered by " + f.Name;
                        break;
                    default:
                        mm += "Filtered by All Offices";
                        break;
                }
            }
            else
                mm += "Filtered by All Offices";

            ViewBag.ResultTitle = mm;



            if (TempData["Alert"] != null)
            {
                ViewBag.Alert = TempData["Alert"];
                TempData.Clear();
            }
            var allPayments = new List<vPaymentTransaction>();// _vPayTransRep.GetAll().ToList();
            return View(allPayments);
        }





        [Authorize]
        public ActionResult OrphanTransaction(string startDate, string endDate, bool useDate = false)
        {
            if (useDate)
            {
                DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
                DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

                ViewBag.SD = sd;
                ViewBag.ED = ed;
            }
            else
            {
                ViewBag.SD = "";
                ViewBag.ED = "";
            }
            ViewBag.UseDate = useDate;

            return View();
        }





        [Authorize]
        public ActionResult LazyLoadOrphanTransaction(JQueryDataTableParamModel param, string startDate, string endDate, bool useDate = false)
        {
            IEnumerable<RemitaPaymentStatus> orphans;
            IEnumerable<RemitaPaymentStatus> filterOrphans;
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;
            var sortDirection = Request["sSortDir_0"]; // asc or desc
            List<RemitaPaymentStatus> displayedOrphans = new List<RemitaPaymentStatus>();

            //if (useDate)
            //{
            //    UtilityHelper.LogMessage($"Using Date to filter");
            //    DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            //    DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            //    //orphans = _remPStatRep.FindBy(a => a.Orphan.HasValue && a.Orphan.Value && a.Date_Sent.HasValue && a.Date_Sent.Value >= sd && a.Date_Sent.Value <= ed);

            //}
            //else
            //    orphans = _remPStatRep.FindBy(a => a.Orphan.HasValue && a.Orphan.Value);

            orphans = _remPStatRep.FindBy(a => a.Orphan.HasValue && a.Orphan.Value);

            if (!string.IsNullOrEmpty(param.sSearch)){
                var s = param.sSearch.ToLower();
                filterOrphans = orphans.Where(a =>
                        (!string.IsNullOrEmpty(a.rrr) && a.rrr.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(a.channnel) && a.channnel.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(a.amount) && a.amount.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(a.orderRef) && a.orderRef.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(a.payerName) && a.payerName.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(a.payerEmail) && a.payerEmail.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(a.payerPhoneNumber) && a.payerPhoneNumber.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(a.debitdate) && a.debitdate.Trim().ToLower().Contains(s)));
            }
            else
            {
                filterOrphans = orphans;
            }

            if (sortDirection == "asc")
            {
                #region Sort Ascending
                switch (sortColumnIndex)
                {
                    case 1:
                        filterOrphans = filterOrphans.OrderBy(a => a.rrr);
                        break;
                    case 2:
                        filterOrphans = filterOrphans.OrderBy(a => a.channnel);
                        break;
                    case 3:
                        filterOrphans = filterOrphans.OrderBy(a => a.amount);
                        break;
                    case 4:
                        filterOrphans = filterOrphans.OrderBy(a => a.orderRef);
                        break;
                    case 5:
                        filterOrphans = filterOrphans.OrderBy(a => a.payerName);
                        break;
                    default:
                        filterOrphans = filterOrphans.OrderBy(a => a.debitdate);
                        break;

                }
                #endregion
            }
            else
            {
                #region Sort Descending
                switch (sortColumnIndex)
                {
                    case 1:
                        filterOrphans = filterOrphans.OrderByDescending(a => a.rrr);
                        break;
                    case 2:
                        filterOrphans = filterOrphans.OrderByDescending(a => a.channnel);
                        break;
                    case 3:
                        filterOrphans = filterOrphans.OrderByDescending(a => a.amount);
                        break;
                    case 4:
                        filterOrphans = filterOrphans.OrderByDescending(a => a.orderRef);
                        break;
                    case 5:
                        filterOrphans = filterOrphans.OrderByDescending(a => a.payerName);
                        break;
                    default:
                        filterOrphans = filterOrphans.OrderByDescending(a => a.debitdate);
                        break;
                }
                #endregion
            }


            if (useDate)
            {
                UtilityHelper.LogMessage($"Using Date to filter");
                DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
                DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
                displayedOrphans = filterOrphans.ToList().Where(a => a.Date.HasValue && a.Date.Value >= sd && a.Date.Value <= ed).ToList();
            }
            else
            {
                displayedOrphans = filterOrphans.ToList();
            }

            var toreturn = new List<RemitaPaymentStatus>();

            //UtilityHelper.LogMessage($"Pagination Param:: Len={param.iDisplayLength}; Start={param.iDisplayStart}");
            if (param.iDisplayLength != -1)
            {
                toreturn = displayedOrphans.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            }
            else
                toreturn = displayedOrphans.ToList();

            UtilityHelper.LogMessage("Pagination done... Returning to View");
            int cc = 1;
            var result = from c in toreturn
                         select new[] {
                             cc++.ToString(),
                             c.rrr,                 //  0
                             c.channnel,            //  1
                             c.amount,              //  2
                             c.orderRef,            //  3
                             c.payerName,           //  4
                             c.payerEmail,          //  5
                             c.payerPhoneNumber,    //  6
                             c.debitdate            //  7
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = filterOrphans.Count(),
                iTotalDisplayRecords = displayedOrphans.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }



        [Authorize]
        public ActionResult LazyLoadRemitaQuery(JQueryDataTableParamModel param, string startDate, string endDate, int license, string status, string category, string filterby, int? filterparam, string location)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            IEnumerable<vPaymentTransaction> allPayments;
            IEnumerable<vPaymentTransaction> filteredPayments;
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;
            var sortDirection = Request["sSortDir_0"]; // asc or desc
            List<vPaymentTransaction> displayedTransactions = new List<vPaymentTransaction>();

            if (string.IsNullOrEmpty(param.sSearch))
            {
                #region License & Category filter
                if (license == 0)
                {
                    #region No License specified
                    if (string.IsNullOrEmpty(category))
                    {
                        allPayments = _vPayTransRep.FindBy(a => a.TransactionDate >= sd && a.TransactionDate <= ed);
                    }
                    else
                    {
                        allPayments = _vPayTransRep.FindBy(a => a.CategoryName.ToLower() == category.ToLower() && a.TransactionDate >= sd && a.TransactionDate <= ed);
                    }
                    #endregion
                }
                else
                {
                    #region  License Specified
                    if (string.IsNullOrEmpty(category))
                    {
                        allPayments = _vPayTransRep.FindBy(a => a.LicenseId == license && a.TransactionDate >= sd && a.TransactionDate <= ed);
                    }
                    else
                    {
                        allPayments = _vPayTransRep.FindBy(a => a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower() && a.TransactionDate >= sd && a.TransactionDate <= ed);
                    }
                    #endregion
                }
                UtilityHelper.LogMessage("Initial Filter with date and License type");
                #endregion

                #region Using Status
                if (!string.IsNullOrEmpty(status))
                {
                    if (status.ToLower() == "completed")
                    {
                        allPayments = allPayments.Where(a => (!string.IsNullOrEmpty(a.Response_Code) && a.Response_Code == "01") || (!string.IsNullOrEmpty(a.RRR) &&  a.RRR.ToLower() == "NUPRC-ELPS".ToLower()));
                    }
                    else if (status.ToLower() == "not completed")
                    {
                        allPayments = allPayments.Where(a => (string.IsNullOrEmpty(a.Response_Code) || a.Response_Code != "01") && (string.IsNullOrEmpty(a.RRR) || a.RRR.ToLower() != "NUPRC-ELPS".ToLower()));
                    }
                }
                #endregion

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
                                    switch (fds.Count())
                                    {
                                        case 1:
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
                                            && a.StateName.ToLower() == fds[0].StateName.ToLower());
                                            break;
                                        case 2:
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
                                            && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[1].StateName.ToLower()));
                                            break;
                                        case 3:
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
                                            && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[2].StateName.ToLower()));
                                            break;
                                        case 4:
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
                                            && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[3].StateName.ToLower()));
                                            break;
                                        case 5:
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
                                            && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[4].StateName.ToLower()));
                                            break;
                                        case 6:
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
                                            && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[5].StateName.ToLower()));
                                            break;
                                        case 7:
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
                                            && (a.StateName.ToLower() == fds[0].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[1].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[3].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[4].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[5].StateName.ToLower()
                                            || a.StateName.ToLower() == fds[6].StateName.ToLower()));
                                            break;
                                        case 8:
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
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
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
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
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
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
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
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
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
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
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
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
                                            allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName)
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
                                            allPayments = null;
                                            break;
                                    }
                                }
                                else
                                {
                                    UtilityHelper.LogMessage($"Zone NOT Found");
                                    allPayments = null;
                                }
                                break;
                            }
                        case "fd":
                            {
                                var brch = _vBranchRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                                UtilityHelper.LogMessage($"Field Office => {brch?.Name}");
                                if (brch != null)
                                {
                                    allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName) && a.StateName.ToLower() == brch.StateName.ToLower());
                                }
                                else
                                {
                                    UtilityHelper.LogMessage($"Field Office NOT Found");
                                    allPayments = null;
                                }
                                break;
                            }
                        case "st":
                            {
                                var _state = _stateRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                                UtilityHelper.LogMessage($"State Filter => {_state?.Name}");
                                if (_state != null)
                                    allPayments = allPayments.Where(a => !string.IsNullOrEmpty(a.StateName) && a.StateName.ToLower() == _state.Name.ToLower());
                                else
                                    allPayments = null;
                                break;
                            }
                        default:
                            break;
                    }
                }
                UtilityHelper.LogMessage("Filter by completed");
                filteredPayments = allPayments;
            }
            else
            {
                allPayments = _vPayTransRep.GetAll();
                var s = param.sSearch.ToLower();
                filteredPayments = allPayments.Where(C =>
                        (!string.IsNullOrEmpty(C.CompanyName) && C.CompanyName.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(C.CategoryName) && C.CategoryName.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(C.LicenseShortName) && C.LicenseShortName.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(C.Transaction_Date) && C.Transaction_Date.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(C.Transaction_Amount) && C.Transaction_Amount.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(C.Order_Id) && C.Order_Id.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(C.RRR) && C.RRR.Trim().ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(C.Type) && C.Type.ToLower().Contains(s)) ||
                        (!string.IsNullOrEmpty(C.Response_Code) && C.Response_Code.ToLower().Contains(s)));
            }

            UtilityHelper.LogMessage("Search param usage completed");


            //var sortDate = "";
            if (sortDirection == "asc")
            {
                #region Sort Ascending
                switch (sortColumnIndex)
                {
                    case 1:
                        filteredPayments = filteredPayments.OrderBy(a => a.CompanyName);
                        break;
                    case 2:
                        filteredPayments = filteredPayments.OrderBy(a => a.LicenseShortName);
                        break;
                    case 3:
                        filteredPayments = filteredPayments.OrderBy(a => a.CategoryName);
                        break;
                    case 4:
                        filteredPayments = filteredPayments.OrderBy(a => a.TransactionDate);
                        //sortDate = "ya";
                        break;
                    case 5:
                        filteredPayments = filteredPayments.OrderBy(a => a.Transaction_Amount);
                        break;
                    case 6:
                        filteredPayments = filteredPayments.OrderBy(a => a.Order_Id);
                        break;
                    case 7:
                        filteredPayments = filteredPayments.OrderBy(a => a.RRR);
                        break;
                    case 8:
                        filteredPayments = filteredPayments.OrderBy(a => a.Type);
                        break;
                    default:
                        filteredPayments = filteredPayments.OrderBy(a => a.TransactionDate);
                        break;

                }
                #endregion
            }
            else
            {
                #region Sort Descending
                switch (sortColumnIndex)
                {
                    case 1:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.CompanyName);
                        break;
                    case 2:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.LicenseShortName);
                        break;
                    case 3:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.CategoryName);
                        break;
                    case 4:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.TransactionDate);
                        //sortDate = "yd";
                        break;
                    case 5:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.Transaction_Amount);
                        break;
                    case 6:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.Order_Id);
                        break;
                    case 7:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.RRR);
                        break;
                    case 8:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.Type);
                        break;
                    default:
                        filteredPayments = filteredPayments.OrderByDescending(a => a.TransactionDate);
                        break;
                }
                #endregion
            }
            UtilityHelper.LogMessage("Sorting accordingly...");
            displayedTransactions = filteredPayments.ToList();

            var toreturn = new List<vPaymentTransaction>();

            UtilityHelper.LogMessage($"Pagination Param:: Len={param.iDisplayLength}; Start={param.iDisplayStart}");
            if (param.iDisplayLength != -1)
            {
                toreturn = displayedTransactions.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
            }
            else
                toreturn = displayedTransactions.ToList();

            UtilityHelper.LogMessage("Pagination done... Returning to View");

            var result = from c in toreturn
                         select new[] {
                             c.CompanyName,                     //  0
                             c.LicenseShortName,                //  1
                             c.CategoryName,                    //  2
                             c.TransactionDate.ToString(),      //  3
                             c.Transaction_Amount,              //  4
                             c.Order_Id,                        //  5
                             c.RRR,                             //  6
                             c.Type,                            //  7
                             c.Response_Code,                   //  8
                             Convert.ToString(c.Id),            //  9
                             c.Approved_Amount,                 //  10
                             c.ServiceCharge,                   //  11
                             c.ApplicationItem                  //  12
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = filteredPayments.Count(),
                iTotalDisplayRecords = displayedTransactions.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }





        [Authorize]
        public ActionResult TransactionDetail(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return View("Error");
            }
            var id = Convert.ToInt32(Id);
            var trans = _vPayTransRep.FindBy(C => C.Id == id).FirstOrDefault();

            if (trans != null) //( (app != null && app.Status.ToLower() == "payment pending"))
            {
                string txnref = trans.Reference_Number;
                //

                #region not needed
                //if ((trans.ApplicationStatus.Trim().ToLower() == "paymentpending")
                //    || (trans.ApplicationStatus.Trim().ToLower() == "paymentcompleted" && trans.Response_Code != "01"))
                //{
                //    // Application existing and Value not given yet. Check if payment has been made for possible value
                //    #region call Back api [Verify payment from REMITA]

                //    string hash_string = trans.Order_Id + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
                //    string hash = PaymentRef.getHash(hash_string);
                //    var url = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, RemitaSplitParams.MERCHANTID, trans.Order_Id, hash.ToLower());
                //    NewRemitaResponse response = new NewRemitaResponse();

                //    using (WebClient client = new WebClient())
                //    {
                //        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                //        string responseJson = client.DownloadString(url);

                //        response = JsonConvert.DeserializeObject<NewRemitaResponse>(responseJson);
                //        if (response != null && (response.status == "01" || response.status == "00"))
                //        {
                //            var vapp = _vAppRep.FindBy(a => a.Id == app.Id).FirstOrDefault();

                //            #region Payment Valid and Approved, Give value
                //            GiveRemitaValue(app, trans.Id, trans.RRR, vapp.CompanyName, DateTime.Now);

                //            //Update Remita_Transaction table
                //            trans.Transaction_Date = response.transactiontime;
                //            trans.Query_Date = UtilityHelper.CurrentTime;
                //            trans.Response_Code = response.status;
                //            trans.Response_Description = response.message;
                //            trans.Type = "Bank";
                //            _remitaRepo.Edit(trans);
                //            _remitaRepo.Save(User.Identity.Name, Request.UserHostAddress);

                //            #region Send Payment Received Mail


                //            var msg = new OGIS.Domain.Entities.Message();
                //            msg.Company_Id = app.Company_Id;
                //            msg.Content = "Loading...";
                //            msg.Date = UtilityHelper.CurrentTime;
                //            msg.Read = 0;
                //            msg.Subject = "Payment Received for Application: " + vapp.Reference;
                //            msg.Sender_Id = "Application";

                //            _msgRep.Add(msg);
                //            _msgRep.Save(User.Identity.Name, Request.UserHostAddress);

                //            //var dt = comp.Date.Day.ToString() + comp.Date.Month.ToString() + comp.Date.Year.ToString();
                //            var sn = msg.Id;
                //            var body = "";
                //            using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "GeneralFormat.txt"))
                //            {
                //                body = sr.ReadToEnd();
                //            }
                //            var apDetails = "";
                //            var tk = string.Format("This is to notify you that we have received payment for your OGISP Permit Application with Reference Number: {0}.<br />The table below shows the breakdown of the funds received.", app.Reference);
                //            var tbl = "<table><tr><td><b>Company Name</b></td><td>{0}</td></tr>" +
                //                "<tr><td><b>Reference Number:</b></td><td>{1}</td></tr>" +
                //                "<tr><td><b>Statutory Permit Fee:</b></td><td>{2}</td></tr>" +
                //                "<tr><td><b>Service Charge:</b></td><td>{3}</td></tr>" +
                //                "<tr><td><b>Total Amount Due:</b></td><td>{4}</td></tr></table>";

                //            tbl = string.Format(tbl, vapp.CompanyName, vapp.Reference, vapp.Fee_Payable, vapp.Service_Charge, vapp.Service_Charge + vapp.Fee_Payable);

                //            var srvcs = _vAppServRep.FindBy(a => a.Application_Id == vapp.Id).ToList();
                //            var src = "<ul>";
                //            foreach (var item in srvcs)
                //            {
                //                src += "<li>" + item.ServiceName + "</li>";
                //            }
                //            src += "</ul>";
                //            var services = "<p>Applicable Services:<br>" + src + "<br /></p>";

                //            var jbSpecs = _vAppJSpecRep.FindBy(a => a.Application_Id == vapp.Id).ToList();
                //            var spks = "<ul>";
                //            foreach (var item in jbSpecs)
                //            {
                //                spks += "<li>" + item.Job_SpecificationName + "</li>";
                //            }
                //            spks += "</ul>";
                //            var jobSpecs = "<p>Applicable Job Specialization: <br> " + spks + "<br /></li>";

                //            apDetails = tk + tbl + services + jobSpecs;
                //            string subject = msg.Subject;   //"Payment Received: " + app.Reference;
                //            var msgbody = string.Format(body, subject, apDetails, sn);
                //            //var body = mailhelper.getmailbody(callbackurl, model.email);
                //            MailHelper.SendEmail(vapp.user_id, subject, msgbody);

                //            var mm = _msgRep.FindBy(m => m.Id == msg.Id).FirstOrDefault();
                //            mm.Content = msgbody;
                //            _msgRep.Edit(mm);
                //            _msgRep.Save(User.Identity.Name, Request.UserHostAddress);

                //            #endregion

                //            ViewBag.Alert = new AlertBox()
                //            {
                //                ButtonType = 1,
                //                Message = "Payment verified sucessfully and Value given to Permit Application with Reference number: " + app.Reference,
                //                Title = "Payment Verified!"
                //            };


                //            #endregion
                //        }
                //        else
                //        {
                //            ViewBag.Alert = new AlertBox()
                //            {
                //                ButtonType = 4,
                //                Message = "Payment Cannot be verified from Remita. Please confirm that payment had been made with the RRR on the application",
                //                Title = "Payment Not Verified!"
                //            };
                //        }
                //    }

                //    #endregion

                //}
                #endregion
                return View(trans);
            }
            TempData["Alert"] = new AlertBox()
            {
                ButtonType = 3,
                Message = "Application Transaction not found or does not exist",
                Title = "Transaction not found!"
            };

            return RedirectToAction("Transactions");
        }





        /// <summary>
        /// Check Payment status directly from Remita
        /// </summary>
        /// <param name="id">Reference number (12 digit) or RRR</param>
        /// <returns></returns>
        public ActionResult CheckIfPaid(string id)
        {
            string hash_string = (id.Trim().ToLower().StartsWith("r") ? id.Substring(1) : id) + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
            string pUrl = string.Empty;
            string hash = PaymentRef.getHash(hash_string).ToLower();

            if (!id.Trim().ToLower().StartsWith("r"))
            {
                //Ref no submitted (Order Id)
                pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, RemitaSplitParams.MERCHANTID, id, hash);
            }
            else
            {
                //RRR Submitted
                id = id.Substring(1);
                pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_RRR, RemitaSplitParams.MERCHANTID, id, hash);
            }

            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                 client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                string responseJson = client.DownloadString(pUrl);

                return Content(responseJson);
            }
        }

        #region GiveValueToBankPayment
        [Authorize(Roles = "Admin")]
        public ActionResult ManualBank()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ManualBank(string reference, string newRRR)
        {
            if (!string.IsNullOrEmpty(reference))
            {
                using (var transactionScope = new TransactionScope())
                {
                    try
                    {
                        var app = _appRep.FindBy(a => a.OrderId == reference).FirstOrDefault();
                        var comp = _compRep.FindBy(a => a.Id == app.CompanyId).FirstOrDefault();
                        var trans = _payTransRep.FindBy(a => a.Reference_Number == reference).FirstOrDefault();

                        app.Status = ApplicationStatus.PaymentCompleted;
                        _appRep.Edit(app);
                        _appRep.Save("admin", "system");
                        if (trans == null)
                        {
                            UtilityHelper.LogMessage("Transactions not Created b4, creating new one");
                            trans = new Payment_Transaction()
                            {
                                CompanyId = app.CompanyId,
                                CompanyName = comp.Name,
                                Completed = true,
                                Order_Id = app.OrderId,
                                PaymentSource = "Bank-Remita",
                                Payment_Reference = "NUPRC-Bank-M",
                                Query_Date = UtilityHelper.CurrentTime.AddYears(-10),
                                Reference_Number = app.OrderId,
                                Response_Code = "01",
                                Response_Description = "Payment Completed",
                                RRR = "NUPRC-Bank-M",
                                Transaction_Currency = "566",
                                Transaction_Date = UtilityHelper.CurrentTime.ToString(),
                                Type = "Offline",
                                ServiceCharge = "",
                                ServiceTypeId = "",
                                ReturnSuccessUrl = "",
                                ReturnFailureUrl = "",
                                ReturnBankPaymentUrl = "",
                                Approved_Amount = "",
                                BankPaymentEndPoint = "",
                                Transaction_Amount = ""
                            };

                            _payTransRep.Add(trans);
                            _payTransRep.Save("Admin", "System");
                        }
                        else
                        {
                            trans.Completed = true;
                            trans.RRR = !string.IsNullOrEmpty(trans.RRR) ? trans.RRR : !string.IsNullOrEmpty(newRRR) ? newRRR : "";
                            trans.Response_Code = "01";
                            trans.Response_Description = "Payment Completed";
                            trans.ManualValueMessage = !string.IsNullOrEmpty(newRRR) ? "Bank Payment with New RRR: " + newRRR : trans.Payment_Reference;
                            _payTransRep.Edit(trans);
                            _payTransRep.Save("Admin", "System");

                            _NewRemitaResponse resp;
                            if (!ProcessPayment(trans, comp.User_Id, out resp))
                            {
                                UtilityHelper.LogMessage("/Payment/Remita" + Environment.NewLine + "Application payment NOT successful");
                                throw new ArgumentException("Unable to save Transaction");
                            }
                            transactionScope.Complete();
                            return Json(new { code = "01" });
                        }

                        #region
                        //if (!string.IsNullOrEmpty(stay) && stay.ToLower() == "on")
                        //{
                        //    var license = _licensRep.FindBy(a => a.Id == app.LicenseId).FirstOrDefault();
                        //    var returnUrl = trans.ReturnSuccessUrl; // license.BaseUrl + "Application/RemitaConfirm";

                        //    string frm = "<form action='{0}' id='frmTest' method='post'>" +
                        //        "<input type='hidden' name='orderId' value='{1}' />" +
                        //        "</form><script>document.getElementById('frmTest').submit();</script>";


                        //    frm = string.Format(frm, returnUrl, reference);
                        //    transactionScope.Complete();
                        //    return Content(frm);
                        //}
                        //else
                        //{
                        //    transactionScope.Complete();
                        //    TempData["msgType"] = "pass";
                        //    ViewBag.Message = "Value Given";
                        //    return View();
                        //}
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        transactionScope.Dispose();
                        //TempData["msgType"] = "fail";
                        var msg = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message;
                        //ViewBag.Message = "Something is not Correct. See: " + msg;
                        return Json(new { code = "00", message = "Something went wrong. Please try again. " + msg });
                    }
                }
            }
            //TempData["msgType"] = "warn";
            //ViewBag.Message = "Invalid Parameters";
            return Json(new { code = "00", message = "Invalid Parameters" });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult RemitaConfirm(string appRef)
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult FindApplication(string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Trim().ToLower();

                Application application;
                List<Company> companies = new List<Company>();
                PaymentHelper paymentHelper = new PaymentHelper
                {
                    Applications = new List<Application>()
                };
                var paymentHelperList = new List<PaymentHelper>();

                if (query.Length == 12)
                {
                    //possibly Reference Number
                    application = _appRep.FindBy(a => a.OrderId == query).FirstOrDefault();
                    if (application != null)
                    {
                        var company = _compRep.FindBy(a => a.Id == application.CompanyId).FirstOrDefault();
                        paymentHelper.Applications.Add(application);
                        paymentHelper.Company = company;
                        paymentHelperList.Add(paymentHelper);
                    }
                }
                else
                {
                    companies = _compRep.FindBy(a => a.Name.ToLower().Trim().Contains(query)).ToList();
                    foreach (var coy in companies)
                    {
                        var apps = _appRep.FindBy(a => a.CompanyId == coy.Id).ToList();
                        if (apps.Any())
                        {
                            paymentHelper = new PaymentHelper
                            {
                                Company = coy
                            };
                            paymentHelper.Applications = apps;

                            paymentHelperList.Add(paymentHelper);
                        }
                    }
                }
                return View("RemitaConfirm", paymentHelperList);
            }
            else
            {
                return View("RemitaConfirm");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CompletePayments(int noToPay)
        {
            var apps = _appRep.FindBy(a => a.Status.ToLower() == ApplicationStatus.PaymentPending.ToLower()).Take(noToPay).ToList();
            foreach (var app in apps)
            {
                var trans = _payTransRep.FindBy(a => a.Reference_Number == app.OrderId).FirstOrDefault();

                string hash_string = trans.RRR.Trim() + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
                string hash = PaymentRef.getHash(hash_string).ToLower();

                var pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_RRR, RemitaSplitParams.MERCHANTID, trans.RRR.Trim(), hash);

                WebClient client = new WebClient();
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                 client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                string responseJson = client.DownloadString(pUrl);
                var response = JsonConvert.DeserializeObject<_NewRemitaResponse>(responseJson);

                if (response.status == "01")
                {
                    app.Status = ApplicationStatus.PaymentCompleted;
                    _appRep.Edit(app);
                    _appRep.Save("admin", "system");

                    trans.Completed = true;
                    trans.Response_Code = "01";
                    trans.Response_Description = "Approved";
                    _payTransRep.Edit(trans);
                    _payTransRep.Save("Admin", "System");

                    var returnUrl = trans.ReturnSuccessUrl;

                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    responseJson = client.DownloadString(returnUrl);

                    string frm = "<form action='{0}' id='frmTest' method='post'>" +
                        "<input type='hidden' name='orderId' value='{1}' />" +
                        "</form><script>document.getElementById('frmTest').submit();</script>";


                    frm = string.Format(frm, returnUrl, app.OrderId);
                    return Content(frm);
                }
            }
            return View();
        }
        #endregion

        #region PaymentTracker
        [Authorize]
        public ActionResult PaymentTracker()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PaymentTracker(string em, string ip, int page = 0, int size = 50)
        {
            int skp = page * size;
            var trans = _payTransRep.FindBy(a => !string.IsNullOrEmpty(a.Reference_Number) && a.Response_Code != "00" && a.Response_Code != "01" && a.Payment_Reference.ToLower() != "NUPRC-ELPS".ToLower() &&
                !string.IsNullOrEmpty(a.RRR)).OrderBy(a => a.Id).Skip(skp).Take(size).ToList();
            UtilityHelper.LogPaymentTrack($"Processing [{trans.Count()}] applications");
            UtilityHelper.LogPaymentTrack($"///////////////////////////////////////////////");
            int noPay = 0;
            int paid = 0;
            int notFound = 0;

            if (trans.Any())
            {
                // Take first 50...
                //var take = trans.ToList();
                var licenses = _appIdentityRep.GetAll().ToList();
                int count = 1;
                UtilityHelper.LogPaymentTrack($"FIRST... {trans[0].Reference_Number} / {trans[0].Id}" +
                    Environment.NewLine + $"LAST... {trans[size - 1].Reference_Number} / {trans[size - 1].Id}");

                foreach (var tran in trans)
                {
                    _NewRemitaResponse resp = new _NewRemitaResponse();

                    var app = _appRep.FindBy(a => a.OrderId == tran.Reference_Number).FirstOrDefault();
                    UtilityHelper.LogPaymentTrack($"({count})... {tran.Reference_Number} / {tran.Id}");
                    if (app != null && !string.IsNullOrEmpty(tran.RRR))
                    {
                        // Check payment status on REMITA
                        string hashItem = tran.RRR.Trim() + _apiKey + _marchantId;
                        string hash = PaymentRef.getHash(hashItem, false).ToLower();

                        var url = string.Format(ConfigurationManager.AppSettings["RemitaStatus_RRR"].ToString(), _marchantId, tran.RRR.Trim(), hash);

                        string output = "";
                        using (WebClient client = new WebClient())
                        {
                            // performs an HTTP POST
                            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                            client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                            try
                            {
                                output = client.DownloadString(url);
                            }
                            catch (Exception)
                            {
                                output = "{}";
                            }
                        }
                        resp = JsonConvert.DeserializeObject<_NewRemitaResponse>(output);
                        if (resp != null && (resp.status == "01" || resp.status == "00"))
                        {
                            //Payment made already, Update Transaction and Application appropriately.
                            using (var transaction = new TransactionScope())
                            {
                                Invoice invo = new Invoice();
                                Receipt rpt = new Receipt();
                                try
                                {
                                    tran.Response_Code = resp.status;
                                    tran.Response_Description = ApplicationStatus.PaymentCompleted;
                                    tran.Transaction_Date = resp.paymentDate;

                                    _payTransRep.Edit(tran);
                                    _payTransRep.Save(em, ip);

                                    if (app.Status == ApplicationStatus.PaymentPending)
                                    {
                                        app.Status = ApplicationStatus.PaymentCompleted;
                                        _appRep.Edit(app);
                                        _appRep.Save(em, ip);
                                    }

                                    // Do invoice & Receipt
                                    invo = _invoiceRep.FindBy(a => a.Application_Id == app.Id).FirstOrDefault();
                                    if (invo == null)
                                    {
                                        invo = new Invoice();
                                        invo.Amount = Convert.ToDouble(tran.Transaction_Amount);
                                        invo.Application_Id = app.Id;
                                        invo.Payment_Code = tran.Reference_Number;
                                        invo.Payment_Type = string.Empty;
                                        invo.Status = "Paid";
                                        invo.Date_Added = UtilityHelper.CurrentTime;
                                        invo.Date_Paid = Convert.ToDateTime(resp.paymentDate);

                                        _invoiceRep.Add(invo);
                                        _invoiceRep.Save(em, ip);
                                        UtilityHelper.LogPaymentTrack($"Invoice Created...");
                                    }
                                    else
                                    {
                                        invo.Status = "Paid";
                                        invo.Date_Paid = Convert.ToDateTime(resp.paymentDate);

                                        _invoiceRep.Edit(invo);
                                        _invoiceRep.Save(em, ip);
                                        UtilityHelper.LogPaymentTrack($"Invoice updated...");
                                    }
                                    #region Receipt
                                    rpt = _recptRep.FindBy(a => a.ApplicationReference == tran.Order_Id).FirstOrDefault();
                                    if (rpt == null)
                                    {
                                        rpt = new Receipt
                                        {
                                            Amount = Convert.ToDouble(tran.Transaction_Amount),
                                            ApplicationId = app.Id,
                                            ApplicationReference = tran.Order_Id,
                                            CompanyName = tran.CompanyName,
                                            Date_Paid = Convert.ToDateTime(resp.paymentDate),
                                            InvoiceId = Convert.ToInt32(invo.Id),
                                            ReceiptNo = "---",// UtilityHelper.GenerateReceiptNo(rpt.Amount, app.Id, lc.ReceiptCode);
                                            RRR = tran.RRR,
                                            Status = "Paid"
                                        };
                                        _recptRep.Add(rpt);
                                        _recptRep.Save(Request.UserHostName, Request.UserHostAddress);
                                        UtilityHelper.LogPaymentTrack($"Receipt Created... ({rpt.Id})");

                                        rpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rpt.Amount, rpt.Id, licenses.FirstOrDefault(a => a.Id == app.LicenseId)?.ReceiptCode, rpt.Date_Paid);
                                        _recptRep.Edit(rpt);
                                        _recptRep.Save(User.Identity.Name, Request.UserHostAddress);
                                    }
                                    else
                                    {
                                        UtilityHelper.LogPaymentTrack($"Receipt already created before...");
                                    }
                                    #endregion
                                    transaction.Complete();
                                    // build response message.
                                    string msg = $"Application Reference: {app.OrderId.Trim()} = ({tran.Order_Id})" + Environment.NewLine +
                                        $"RRR: {tran.RRR.Trim()}" + Environment.NewLine +
                                        $"Receipt Number: {rpt.ReceiptNo.Trim()}" + Environment.NewLine +
                                        $"Invoice Pay Code: {invo.Payment_Code.Trim()}" + Environment.NewLine +
                                        $"Payment Date: {resp.paymentDate.Trim()}";
                                    UtilityHelper.LogPaymentTrack(msg);
                                    paid++;
                                }
                                catch (Exception)
                                {
                                    transaction.Dispose();
                                    var msg = $"Error while processing \"{app.OrderId}\" = ({tran.Order_Id})" + Environment.NewLine +
                                        $"RRR: {tran?.RRR}" + Environment.NewLine +
                                        $"Receipt Number: {rpt?.ReceiptNo}" + Environment.NewLine +
                                        $"Invoice Pay Code: {invo?.Payment_Code}" + Environment.NewLine +
                                        $"Payment Date: {resp?.transactiontime}";
                                    UtilityHelper.LogPaymentTrack(msg);
                                }
                            }
                        }
                        else
                        {
                            noPay++;
                            UtilityHelper.LogPaymentTrack($"Not yet paid for...");
                        }
                    }
                    else
                    {
                        notFound++;
                        var msg = $"Application Not Found \"{tran.Reference_Number}\"" + Environment.NewLine +
                                        $"RRR: {tran?.RRR}" + Environment.NewLine +
                                        //$"Receipt Number: {rpt?.ReceiptNo}" + Environment.NewLine +
                                        //$"Invoice Pay Code: {invo?.Payment_Code}" + Environment.NewLine +
                                        $"Payment Date: {resp?.transactiontime}";
                        UtilityHelper.LogPaymentTrack(msg);
                    }
                    count++;
                }
            }
            UtilityHelper.LogPaymentTrack($"///////////////////////////////////////////////");
            return Json(new { Page = page + 1, PageSize = size, Skip = skp, Paid = paid, NotPaid = noPay, NotFound = notFound });
        }

        [HttpPost]
        public ActionResult Payment_Tracker(string sd, string ed)
        {
            DateTime _sd = DateTime.Parse(sd);
            DateTime _ed = DateTime.Parse(ed);
            var trans = _payTransRep.FindBy(a => !string.IsNullOrEmpty(a.Reference_Number) && a.Response_Code != "00" && a.Response_Code != "01" && a.Payment_Reference.ToLower() != "NUPRC-ELPS".ToLower() &&
                !string.IsNullOrEmpty(a.RRR) && a.TransactionDate >= _sd && a.TransactionDate <= _ed).ToList(); //.Take(20).ToList();
            int tCount = trans.Count();
            UtilityHelper.LogPaymentTrack($"Processing [{tCount}] applications");
            UtilityHelper.LogPaymentTrack($"///////////////////////////////////////////////");
            int noPay = 0;
            int paid = 0;
            int notFound = 0;

            if (trans.Any())
            {
                // Take first 50...
                //var take = trans.ToList();
                var licenses = _appIdentityRep.GetAll().ToList();
                int count = 1;
                UtilityHelper.LogPaymentTrack($"FIRST... {trans[0].Reference_Number} / {trans[0].Id}" +
                    Environment.NewLine + $"LAST... {trans[tCount - 1].Reference_Number} / {trans[trans.Count() - 1].Id}");

                foreach (var tran in trans)
                {
                    _NewRemitaResponse resp = new _NewRemitaResponse();

                    var app = _appRep.FindBy(a => a.OrderId == tran.Reference_Number).FirstOrDefault();
                    UtilityHelper.LogPaymentTrack($"({count})... {tran.Reference_Number} / {tran.Id}");
                    if (app != null && !string.IsNullOrEmpty(tran.RRR))
                    {
                        // Check payment status on REMITA
                        string hashItem = tran.RRR.Trim() + _apiKey + _marchantId;
                        string hash = PaymentRef.getHash(hashItem, false).ToLower();

                        var url = string.Format(ConfigurationManager.AppSettings["RemitaStatus_RRR"].ToString(), _marchantId, tran.RRR.Trim(), hash);

                        string output = "";
                        using (WebClient client = new WebClient())
                        {
                            // performs an HTTP POST
                            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                            client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                            output = client.DownloadString(url);
                        }
                        UtilityHelper.LogPaymentTrack($"Remita Response >> {output}");
                        resp = JsonConvert.DeserializeObject<_NewRemitaResponse>(output);
                        if (resp != null && (resp.status == "01" || resp.status == "00"))
                        {
                            //Payment made already, Update Transaction and Application appropriately.
                            using (var transaction = new TransactionScope())
                            {
                                Invoice invo = new Invoice();
                                Receipt rpt = new Receipt();
                                try
                                {
                                    tran.Response_Code = resp.status;
                                    tran.Response_Description = ApplicationStatus.PaymentCompleted;
                                    tran.Transaction_Date = resp.paymentDate;

                                    _payTransRep.Edit(tran);
                                    _payTransRep.Save(User.Identity.Name, Request.UserHostAddress);

                                    if (app.Status == ApplicationStatus.PaymentPending)
                                    {
                                        app.Status = ApplicationStatus.PaymentCompleted;
                                        _appRep.Edit(app);
                                        _appRep.Save(User.Identity.Name, Request.UserHostAddress);
                                    }

                                    // Do invoice & Receipt
                                    invo = _invoiceRep.FindBy(a => a.Application_Id == app.Id).FirstOrDefault();
                                    if (invo == null)
                                    {
                                        invo = new Invoice();
                                        invo.Amount = Convert.ToDouble(tran.Transaction_Amount);
                                        invo.Application_Id = app.Id;
                                        invo.Payment_Code = tran.Reference_Number;
                                        invo.Payment_Type = string.Empty;
                                        invo.Status = "Paid";
                                        invo.Date_Added = UtilityHelper.CurrentTime;
                                        invo.Date_Paid = Convert.ToDateTime(resp.paymentDate);

                                        _invoiceRep.Add(invo);
                                        _invoiceRep.Save(User.Identity.Name, Request.UserHostAddress);
                                        //UtilityHelper.LogPaymentTrack($"Invoice Created...");
                                    }
                                    else
                                    {
                                        invo.Status = "Paid";
                                        invo.Date_Paid = Convert.ToDateTime(resp.paymentDate);

                                        _invoiceRep.Edit(invo);
                                        _invoiceRep.Save(User.Identity.Name, Request.UserHostAddress);
                                        //UtilityHelper.LogPaymentTrack($"Invoice updated...");
                                    }
                                    #region Receipt
                                    rpt = _recptRep.FindBy(a => a.ApplicationReference == tran.Order_Id).FirstOrDefault();
                                    if (rpt == null)
                                    {
                                        rpt = new Receipt
                                        {
                                            Amount = Convert.ToDouble(tran.Transaction_Amount),
                                            ApplicationId = app.Id,
                                            ApplicationReference = tran.Order_Id,
                                            CompanyName = tran.CompanyName,
                                            Date_Paid = Convert.ToDateTime(resp.paymentDate),
                                            InvoiceId = Convert.ToInt32(invo.Id),
                                            ReceiptNo = "---",// UtilityHelper.GenerateReceiptNo(rpt.Amount, app.Id, lc.ReceiptCode);
                                            RRR = tran.RRR,
                                            Status = "Paid"
                                        };
                                        _recptRep.Add(rpt);
                                        _recptRep.Save(Request.UserHostName, Request.UserHostAddress);
                                        //UtilityHelper.LogPaymentTrack($"Receipt Created... ({rpt.Id})");

                                        rpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rpt.Amount, rpt.Id, licenses.FirstOrDefault(a => a.Id == app.LicenseId)?.ReceiptCode, rpt.Date_Paid);
                                        _recptRep.Edit(rpt);
                                        _recptRep.Save(User.Identity.Name, Request.UserHostAddress);
                                    }
                                    else
                                    {
                                        //UtilityHelper.LogPaymentTrack($"Receipt already created before...");
                                    }
                                    #endregion
                                    transaction.Complete();
                                    // build response message.
                                    string msg = $"Application Reference: {app.OrderId.Trim()} = ({tran.Order_Id})" + Environment.NewLine +
                                        $"RRR: {tran.RRR.Trim()}" + Environment.NewLine +
                                        $"Receipt Number: {rpt.ReceiptNo.Trim()}" + Environment.NewLine +
                                        $"Invoice Pay Code: {invo.Payment_Code.Trim()}" + Environment.NewLine +
                                        $"Payment Date: {resp.paymentDate.Trim()}";
                                    UtilityHelper.LogPaymentTrack(msg);
                                    paid++;
                                }
                                catch (Exception)
                                {
                                    transaction.Dispose();
                                    var msg = $"Error while processing \"{app.OrderId}\" = ({tran.Order_Id})" + Environment.NewLine +
                                        $"RRR: {tran?.RRR}" + Environment.NewLine +
                                        $"Receipt Number: {rpt?.ReceiptNo}" + Environment.NewLine +
                                        $"Invoice Pay Code: {invo?.Payment_Code}" + Environment.NewLine +
                                        $"Payment Date: {resp?.transactiontime}";
                                    UtilityHelper.LogPaymentTrack(msg);
                                }
                            }
                        }
                        else
                        {
                            noPay++;
                            //UtilityHelper.LogPaymentTrack($"Not yet paid for...");
                        }
                    }
                    else
                    {
                        notFound++;
                        var msg = $"Application Not Found \"{tran.Reference_Number}\"" + Environment.NewLine +
                                        $"RRR: {tran?.RRR}" + Environment.NewLine +
                                        //$"Receipt Number: {rpt?.ReceiptNo}" + Environment.NewLine +
                                        //$"Invoice Pay Code: {invo?.Payment_Code}" + Environment.NewLine +
                                        $"Payment Date: {resp?.transactiontime}";
                        UtilityHelper.LogPaymentTrack(msg);
                    }

                    System.Threading.Thread.Sleep(00); // Delay for 1/2 a second
                    count++;
                }
            }
            UtilityHelper.LogPaymentTrack($"///////////////////////////////////////////////");
            return Json(new { StartDate = sd, EndDate = ed, Paid = paid, NotPaid = noPay, NotFound = notFound, Total = tCount });
        }

        [HttpPost]
        public ActionResult Payment_Tracker_Single(string reference)
        {
            string msg = "";
            try
            {
                var tran = _payTransRep.FindBy(a => a.Reference_Number == reference).FirstOrDefault();

                if (tran != null)
                {
                    var licenses = _appIdentityRep.GetAll().ToList();

                    _NewRemitaResponse resp = new _NewRemitaResponse();

                    var app = _appRep.FindBy(a => a.OrderId == tran.Reference_Number).FirstOrDefault();
                    if (app != null && !string.IsNullOrEmpty(tran.RRR))
                    {
                        // Check payment status on REMITA
                        string hashItem = tran.RRR.Trim() + _apiKey + _marchantId;
                        string hash = PaymentRef.getHash(hashItem, false).ToLower();

                        var url = string.Format(ConfigurationManager.AppSettings["RemitaStatus_RRR"].ToString(), _marchantId, tran.RRR.Trim(), hash);

                        string output = "";
                        using (WebClient client = new WebClient())
                        {
                            // performs an HTTP POST
                            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                            client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                            output = client.DownloadString(url);
                        }
                        resp = JsonConvert.DeserializeObject<_NewRemitaResponse>(output);
                        if (resp != null && (resp.status == "01" || resp.status == "00"))
                        {
                            //Payment made already, Update Transaction and Application appropriately.
                            using (var transaction = new TransactionScope())
                            {
                                Invoice invo = new Invoice();
                                Receipt rpt = new Receipt();
                                try
                                {
                                    tran.Response_Code = resp.status;
                                    tran.Response_Description = ApplicationStatus.PaymentCompleted;
                                    tran.Transaction_Date = resp.paymentDate;

                                    _payTransRep.Edit(tran);
                                    _payTransRep.Save(User.Identity.Name, Request.UserHostAddress);

                                    if (app.Status == ApplicationStatus.PaymentPending)
                                    {
                                        app.Status = ApplicationStatus.PaymentCompleted;
                                        _appRep.Edit(app);
                                        _appRep.Save(User.Identity.Name, Request.UserHostAddress);
                                    }

                                    // Do invoice & Receipt
                                    invo = _invoiceRep.FindBy(a => a.Application_Id == app.Id).FirstOrDefault();
                                    if (invo == null)
                                    {
                                        invo = new Invoice();
                                        invo.Amount = Convert.ToDouble(tran.Transaction_Amount);
                                        invo.Application_Id = app.Id;
                                        invo.Payment_Code = tran.Reference_Number;
                                        invo.Payment_Type = string.Empty;
                                        invo.Status = "Paid";
                                        invo.Date_Added = UtilityHelper.CurrentTime;
                                        invo.Date_Paid = Convert.ToDateTime(resp.paymentDate);

                                        _invoiceRep.Add(invo);
                                        _invoiceRep.Save(User.Identity.Name, Request.UserHostAddress);
                                        //UtilityHelper.LogPaymentTrack($"Invoice Created...");
                                    }
                                    else
                                    {
                                        invo.Status = "Paid";
                                        invo.Date_Paid = Convert.ToDateTime(resp.paymentDate);

                                        _invoiceRep.Edit(invo);
                                        _invoiceRep.Save(User.Identity.Name, Request.UserHostAddress);
                                        //UtilityHelper.LogPaymentTrack($"Invoice updated...");
                                    }
                                    #region Receipt
                                    rpt = _recptRep.FindBy(a => a.ApplicationReference == tran.Order_Id).FirstOrDefault();
                                    if (rpt == null)
                                    {
                                        rpt = new Receipt
                                        {
                                            Amount = Convert.ToDouble(tran.Transaction_Amount),
                                            ApplicationId = app.Id,
                                            ApplicationReference = tran.Order_Id,
                                            CompanyName = tran.CompanyName,
                                            Date_Paid = Convert.ToDateTime(resp.paymentDate),
                                            InvoiceId = Convert.ToInt32(invo.Id),
                                            ReceiptNo = "---",// UtilityHelper.GenerateReceiptNo(rpt.Amount, app.Id, lc.ReceiptCode);
                                            RRR = tran.RRR,
                                            Status = "Paid"
                                        };
                                        _recptRep.Add(rpt);
                                        _recptRep.Save(Request.UserHostName, Request.UserHostAddress);
                                        //UtilityHelper.LogPaymentTrack($"Receipt Created... ({rpt.Id})");

                                        rpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rpt.Amount, rpt.Id, licenses.FirstOrDefault(a => a.Id == app.LicenseId)?.ReceiptCode, rpt.Date_Paid);
                                        _recptRep.Edit(rpt);
                                        _recptRep.Save(User.Identity.Name, Request.UserHostAddress);
                                    }

                                    #endregion
                                    transaction.Complete();
                                    // build response message.
                                    msg = $"Application Reference: {app.OrderId.Trim()} = ({tran.Order_Id})" + Environment.NewLine +
                                        $"RRR: {tran.RRR.Trim()}" + Environment.NewLine +
                                        $"Receipt Number: {rpt.ReceiptNo.Trim()}" + Environment.NewLine +
                                        $"Invoice Pay Code: {invo.Payment_Code.Trim()}" + Environment.NewLine +
                                        $"Payment Date: {resp.paymentDate.Trim()}";
                                    UtilityHelper.LogPaymentTrack(msg);

                                }
                                catch (Exception)
                                {
                                    transaction.Dispose();
                                    msg = $"Error while processing \"{app.OrderId}\" = ({tran.Order_Id})" + Environment.NewLine +
                                        $"RRR: {tran?.RRR}" + Environment.NewLine +
                                        $"Receipt Number: {rpt?.ReceiptNo}" + Environment.NewLine +
                                        $"Invoice Pay Code: {invo?.Payment_Code}" + Environment.NewLine +
                                        $"Payment Date: {resp?.transactiontime}";
                                    //UtilityHelper.LogPaymentTrack(msg);
                                    throw new ArgumentException(msg);
                                }
                            }
                        }
                        else
                            throw new ArgumentException("Payment not confirmed");
                    }
                    else
                    {
                        throw new ArgumentException($"Application Not Found \"{tran.Reference_Number}\"" + Environment.NewLine +
                                $"RRR: {tran?.RRR}" + Environment.NewLine +
                                $"Payment Date: {resp?.transactiontime}");
                    }
                }
                else
                    throw new ArgumentException("Transaction for this reference not found");
            }
            catch (Exception ex)
            {
                UtilityHelper.LogPaymentTrack(ex.Message);
            }
            return Json(new { msg = msg });
        }

        [HttpPost]
        public ActionResult Payment_Tracker_RemitaPay(List<RemitaPaymentStatus> model)
        {
            List<string> errors = new List<string>();
            List<string> orders = new List<string>();
            int counter = 0;
            if (model != null && model.Count > 0)
            {
                var remP = new List<RemitaPaymentStatus>();
                #region Loop tru Model
                UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Processing Payment: " + model.Count() + " payments sent in");
                foreach (var rps in model)
                {
                    counter++;
                    // var rps = model.FirstOrDefault();
                    if (!string.IsNullOrEmpty(rps.rrr))
                    {
                        //Atleast something was Sent
                        //lets keep this Value first
                        try
                        {
                            var rpt = new RemitaPaymentStatus
                            {
                                 amount = rps.amount, bank = rps.bank, branch = rps.branch, 
                            };
                            if (rpt == null)
                            {
                                //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + counter + ": >> RPT: Not avail, Saving...");
                                rps.strId = rps.Id.ToString();
                                DateTime dd; // df;
                                if (DateTime.TryParse(rps.dateRequested, out dd))
                                {
                                    rps.Date_Requested = dd;
                                }
                                else
                                {
                                    rps.Date_Requested = UtilityHelper.CurrentTime;
                                }
                                //if (DateTime.TryParse(rps.dateSent, out df))
                                //{
                                //    rps.Date_Sent = df;
                                //}
                                //else
                                //{
                                //    rps.Date_Sent = UtilityHelper.CurrentTime;
                                //}
                                _remPStatRep.Add(rps);
                                _remPStatRep.Save("Remita", "Remita");

                                rpt = _remPStatRep.FindBy(a => a.rrr.ToLower().Trim() == rps.rrr.ToLower().Trim()).FirstOrDefault();
                            }


                            //get the Transactions
                            var trans = _payTransRep.FindBy(a => a.RRR == rps.rrr || a.Order_Id.ToLower() == rps.orderRef.ToLower()).FirstOrDefault();

                            if (trans != null)
                            {
                                #region record transaction

                                string hash_string = trans.Order_Id + _apiKey + _marchantId;
                                string hash = PaymentRef.getHash(hash_string);
                                var url = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, _marchantId, trans.Order_Id, hash.ToLower());
                                _NewRemitaResponse response = new _NewRemitaResponse();

                                using (WebClient client = new WebClient())
                                {
                                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                    client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                                    string responseJson = client.DownloadString(url);
                                    //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Response from Remita: >>>   " + responseJson);
                                    response = JsonConvert.DeserializeObject<_NewRemitaResponse>(responseJson);
                                    if (response != null && (response.status == "01" || response.status == "00"))
                                    {
                                        var RRR = response.RRR;
                                        // update tarnsaction
                                        trans.Completed = true;
                                        trans.Response_Code = response.status;
                                        trans.Response_Description = response.message;
                                        trans.Type = rpt != null ? rpt.channnel : "BRANCH";
                                        if (string.IsNullOrEmpty(trans.RRR))
                                            trans.RRR = rpt.rrr;
                                        trans.PaymentSource = rpt.channnel;
                                        trans.Transaction_Date = response.paymentDate;
                                        trans.TransactionDate = DateTime.Parse(trans.Transaction_Date);

                                        _payTransRep.Edit(trans);
                                        _payTransRep.Save("Remita", "Remita");
                                        //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Payment Transaction Updated to COMPLETED [" + trans.Order_Id + "]");

                                        #region Payment Valid and Approved, Give value

                                        rps.BankPaymentEndPoint = trans.ReturnBankPaymentUrl;
                                        rps.orderRef = string.IsNullOrEmpty(rps.orderRef) ? trans.Order_Id : rps.orderRef;
                                        remP.Add(rps);
                                        //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "New Remita Payment Status Added [" + rps.rrr + " >> " + rps.orderRef + "]");
                                        var app = _appRep.FindBy(a => a.OrderId == trans.Order_Id).FirstOrDefault();
                                        if (app != null)
                                        {
                                            app.Status = ApplicationStatus.PaymentCompleted;
                                            _appRep.Edit(app);
                                            _appRep.Save(Request.Url.Host, Request.UserHostAddress);
                                            //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Application updated to PAYMENT COMPLETED [" + app.OrderId + "]");
                                        }

                                        //Update Invoice and Create Receipt
                                        var invo = _invoiceRep.FindBy(a => a.Payment_Code == trans.Order_Id).FirstOrDefault();
                                        if (invo != null)
                                        {
                                            invo.Status = "Paid";
                                            invo.Date_Paid = Convert.ToDateTime(response.paymentDate);

                                            _invoiceRep.Edit(invo);
                                            _invoiceRep.Save(Request.UserHostName, Request.UserHostAddress);
                                            //UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Invoice Updated to PAID [" + invo.Amount + "]");
                                        }

                                        //get the License receipt Code

                                        var lc = _licensRep.FindBy(a => a.Id == app.LicenseId).FirstOrDefault();

                                        var rcpt = _recptRep.FindBy(a => a.RRR.Trim().ToLower() == trans.RRR.Trim().ToLower()).FirstOrDefault();
                                        if (rcpt == null)
                                        {
                                            #region Create Receipt
                                            rcpt = new Receipt
                                            {
                                                Amount = Convert.ToDouble(trans.Transaction_Amount),
                                                ApplicationId = app.Id,
                                                ApplicationReference = trans.Order_Id,
                                                CompanyName = trans.CompanyName,
                                                Date_Paid = Convert.ToDateTime(response.paymentDate),
                                                InvoiceId = Convert.ToInt32(invo.Id),
                                                ReceiptNo = "---", // UtilityHelper.GenerateReceiptNo(rcpt.Amount, app.Id, lc.ReceiptCode);
                                                RRR = trans.RRR,
                                                Status = "Paid"
                                            };
                                            _recptRep.Add(rcpt);
                                            _recptRep.Save(Request.UserHostName, Request.UserHostAddress);

                                            rcpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rcpt.Amount, rcpt.Id, lc.ReceiptCode, rcpt.Date_Paid);
                                            _recptRep.Edit(rcpt);
                                            _recptRep.Save(User.Identity.Name, Request.UserHostAddress);
                                            #endregion
                                        }

                                        #region send Mail
                                        var comp = _compRep.FindBy(a => a.Id == app.CompanyId).FirstOrDefault();
                                        var body = "";
                                        using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "GeneralFormat.txt"))
                                        {
                                            body = sr.ReadToEnd();
                                        }

                                        var apDetails = string.Format("This is to notify you that we have received payment for your " + lc.ShortName + " Permit Application with Reference Number: {0}." + Environment.NewLine + "Please Find the Attached Receipt file for Details.", app.OrderId);
                                        var msgbody = string.Format(body, "NUPRC Receipt Confirmation: " + app.OrderId, apDetails);

                                        var pdf = new ActionAsPdf("getReceiptPdf", new { orderId = trans.Order_Id, recptId = rpt.Id }) { FileName = "ElpsPaymentReceipt_" + app.OrderId + ".pdf" };
                                        var binary = pdf.BuildPdf(this.ControllerContext);

                                        MailHelper.SendEmail(comp.User_Id, "NUPRC Receipt Confirmation", msgbody, new Attachment(new MemoryStream(binary), "ElpsPaymentReceipt_" + app.OrderId + ".pdf"));
                                        orders.Add(app.OrderId);
                                        #endregion

                                        #endregion
                                    }
                                    else
                                    {
                                        errors.Add("RRR: " + rps.rrr + " failed" + response.message);
                                        UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Payment not confirmed [" + rps.rrr + "]");
                                    }
                                }

                                #endregion
                            }
                            else
                            {
                                errors.Add("RRR: " + rps.rrr + "; transaction not found");
                                UtilityHelper.LogMessage("/Payment/RemitaPay" + Environment.NewLine + "Transaction not found [" + rps.rrr + "]");

                                // Log as Orphan Payments
                                rpt.Orphan = true; 
                                _remPStatRep.Edit(rpt);
                                _remPStatRep.Save("Remita", "Remita");
                            }
                        }
                        catch (Exception ex)
                        {
                            //return Content("An Error occured: " + ex.Message);
                            errors.Add("RRR: " + rps.rrr + " failed >> More: " + (ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message));
                            continue;
                        }
                    }
                }

                if (errors.Any())
                {
                    var sub = "Error from payment on ELPS";
                    var msg = "Errors encountered from Payment on ELSP. <br /> ";
                    foreach (var err in errors)
                    {
                        msg += err + "<br />";
                    }
                    MailHelper.SendEmail("errors@siga.33mail.com", sub, msg);
                }
                else
                {
                    UtilityHelper.LogMessage("No error while processing send Payments");
                }

                if (remP.Count > 0)
                {
                    #region
                    var remps = remP.GroupBy(a => a.BankPaymentEndPoint).ToList();
                    foreach (var items in remps)
                    {
                        UtilityHelper.LogMessage("Group (Bank EndPoint): " + items.Key);

                        var rpl = new List<RemitaPaymentStatus>();
                        //get the Endpoint Url
                        var bep = items.Key;
                        foreach (var item in items)
                        {
                            item.BankPaymentEndPoint = string.Empty;
                            //item.IsCompleted
                            rpl.Add(item);
                        }

                        //Post to the EndPoint
                        var rplJ = JsonConvert.SerializeObject(rpl);
                        //UtilityHelper.LogMessage(rplJ);

                        var rUrl = bep;
                        string output = "";
                        string eMsg = "";
                        using (WebClient client = new WebClient())
                        {
                            // performs an HTTP POST
                            try
                            {
                                //client.Headers[HttpRequestHeader.Accept] = "application/json";
                                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                output = client.UploadString(rUrl, "POST", rplJ);
                                //UtilityHelper.LogMessage(output);
                                UtilityHelper.LogMessage("Project URL: " + rUrl);
                            }
                            catch (Exception x)
                            {
                                eMsg = x.Message;
                                UtilityHelper.LogMessage("Error as I tried calling owner project to get its payment. >>> " + eMsg);
                            }
                        }
                        output = output.Replace("jsonp (", "");
                        output = output.Replace(")", "");

                        try
                        {
                            var resp = JsonConvert.DeserializeObject<List<int>>(output);
                            if (resp.Count > 0)
                            {
                                UtilityHelper.LogMessage("Response deserialize successfully");
                                return Content("All ok");
                            }
                        }
                        catch (Exception)
                        {
                            UtilityHelper.LogMessage("Cannot Deserialize the response from Project");
                        }
                    }
                    #endregion
                }

                #endregion
            }
            return Content("No Data is received");
        }

        #endregion
    }
}