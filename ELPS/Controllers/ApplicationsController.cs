using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using ELPS.Models;
using Newtonsoft.Json;
using System.Transactions;

namespace ELPS.Controllers
{
    [RoutePrefix("api/Application")]
    public class ApplicationsController : ApiController
    {

        IApplicationRepository _appRep;
        ICompanyRepository _compRep;
        IAppIdentityRepository _appIdRep;
        IPayment_TransactionRepository _payTransRep;
        IInvoiceRepository _invoiceRep;
        IReceiptRepository _recptRep;
        WebApiAccessHelper accessHelper;


        public ApplicationsController(IApplicationRepository appRep, ICompanyRepository compRep, IAppIdentityRepository appIdRep,
            IPayment_TransactionRepository payTransRep, IInvoiceRepository invoiceRep, IReceiptRepository recptRep)
        {
            _appIdRep = appIdRep;
            _appRep = appRep;
            _compRep = compRep;
            _payTransRep = payTransRep;
            _invoiceRep = invoiceRep;
            _recptRep = recptRep;

            accessHelper = new WebApiAccessHelper(appIdRep);
        }

        [ResponseType(typeof(List<Application>))]
        [Route("ApplicationList/{page:int}/{appId:int}")]
        public IHttpActionResult GetApplicationList(int page, int appId) //, string email, string apiHash)
        {
            #region
            //if (string.IsNullOrEmpty(email))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
            //    {
            //        ReasonPhrase = "App UserName cannot be empty"
            //    });
            //}
            ////check if app is registered
            //var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            //if (app == null)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "App has been denied Access, Contact DPR Dev"
            //    });
            //}

            ////compare hash provided
            //if (!HashManager.compair(email, app.AppId, apiHash))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "App has been denied Access, Contact DPR Dev"
            //    });
            //}
            #endregion

            int skip = 1000 * (page -1);
            var ap = _appRep.FindBy(a => a.CategoryName.Trim().ToLower() == "general" && a.LicenseId == appId).OrderBy(a => a.Id).Skip(skip).Take(1000).ToList();

            return Ok(ap);
        }

        /// <summary>
        /// Get a company's list of applications
        /// </summary>
        /// <param name="CompId">Company Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(Licen) secrete Key</param>
        /// <returns>Returns a list of applications that Belongs to this Company. </returns>

        [ResponseType(typeof(List<Application>))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult GetApplication(int CompId, string email, string apiHash)
        {
            #region
            if (string.IsNullOrEmpty(email))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "App UserName cannot be empty"
                });
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }
            #endregion

            var ap = _appRep.FindBy(a => a.CompanyId == CompId).ToList();

            return Ok(ap);
        }

        /// <summary>
        /// Get a Particular Application by application orderId
        /// </summary>
        /// <param name="orderId">Application orderId</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(Licen) secrete Key</param>
        /// <returns>returns Application Details</returns>
        [ResponseType(typeof(Application))]
        [Route("ByOrderId/{orderId}/{email}/{apiHash}")]
        public IHttpActionResult GetApplication(string orderId, string email, string apiHash)
        {
            #region
            if (string.IsNullOrEmpty(email))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "App UserName cannot be empty"
                });
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }
            //check if call is from the app Owner

            //var url = HttpContext.Current.Request.UserHostName;//HttpContext.Current.Request.Url.OriginalString;

            //if (url != app.BaseUrl)
            //{

            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}
            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion



            var ap = _appRep.FindBy(a => a.OrderId == orderId).FirstOrDefault();
            //if (ap == null)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
            //    {
            //        ReasonPhrase = "Item does not Exist"
            //    });
            //}


            return Ok(ap);
        }

        /// <summary>
        /// To Update a Particular Application
        /// </summary>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(Licen) secrete Key</param>
        /// <param name="model">Application Object to be Updated</param>
        /// <returns>Returns the Updated Application Details</returns>
        [ResponseType(typeof(Application))]
        [Route("{email}/{apiHash}")]
        public IHttpActionResult PutApplication(string email, string apiHash, Application model)
        {
            #region
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            var lc = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            #endregion

            using (var trans = new TransactionScope())
            {

                if (model != null && !string.IsNullOrEmpty(model.Status) && !string.IsNullOrEmpty(model.OrderId))
                {
                    var ap = _appRep.FindBy(a => a.OrderId == model.OrderId).FirstOrDefault();
                    if (ap == null)
                    {
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            ReasonPhrase = "Application Not Found"
                        });
                    }

                    if (model.Status == ApplicationStatus.Processing)
                    {

                        //check transaction table to see if the application payment is completed
                        var ptrns = _payTransRep.FindBy(a => a.Order_Id == model.OrderId || a.Reference_Number == model.OrderId).FirstOrDefault();

                        if (ptrns != null && ptrns.Completed)
                        {
                            //check if the app paymennt  have been completed
                            ap.Status = ApplicationStatus.Processing;
                        }
                        else
                        {

                            try
                            {
                                var ordid = !string.IsNullOrEmpty(ptrns.Order_Id) ? ptrns.Order_Id : ptrns.Reference_Number;
                                string hash_string = ordid + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
                                string hash = PaymentRef.getHash(hash_string);
                                var pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, RemitaSplitParams.MERCHANTID, ordid, hash.ToLower());
                                NewRemitaResponse response = new NewRemitaResponse();

                                using (WebClient client = new WebClient())
                                {
                                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                    string responseJson = client.DownloadString(pUrl);
                                    DateTime date;
                                    DateTime bmark;
                                    var dt = DateTime.TryParse(ptrns.Transaction_Date, out date);
                                    dt = DateTime.TryParse("2015-07-31", out bmark);

                                    response = JsonConvert.DeserializeObject<NewRemitaResponse>(responseJson);
                                    if ((response != null && (response.status == "01" || response.status == "00")) || (date.Date <= bmark.Date))
                                    {
                                        #region Payment Valid and Approved, Give value

                                        //Update Remita_Transaction table

                                        ptrns.Query_Date = UtilityHelper.CurrentTime;
                                        ptrns.Response_Code = response.status;
                                        ptrns.Response_Description = response.message;
                                        ptrns.Type = "BANK";
                                        ptrns.Completed = true;
                                        _payTransRep.Edit(ptrns);
                                        _payTransRep.Save(email, HttpContext.Current.Request.UserHostAddress);


                                        ap.Status = ApplicationStatus.PaymentCompleted.ToString();

                                        _appRep.Edit(ap);
                                        _appRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                                        //Update Invoice and Create Receipt
                                        if (!(date.Date <= bmark.Date))
                                        {
                                            var invo = _invoiceRep.FindBy(a => a.Payment_Code == ordid).FirstOrDefault();
                                            if (invo != null)
                                            {
                                                invo.Status = "Paid";
                                                invo.Date_Paid = Convert.ToDateTime(response.transactiontime);

                                                _invoiceRep.Edit(invo);
                                                _invoiceRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                            }
                                            else
                                            {
                                                invo = new Invoice();
                                                invo.Amount = Convert.ToDouble(ptrns.Transaction_Amount);
                                                invo.Application_Id = ap.Id;
                                                invo.Payment_Code = ordid;
                                                invo.Payment_Type = "Remita Bank";
                                                invo.Status = "Paid";
                                                invo.Date_Added = UtilityHelper.CurrentTime;
                                                invo.Date_Paid = Convert.ToDateTime(response.transactiontime);
                                                invo.PaymentTransaction_Id = ptrns.Id;

                                                _invoiceRep.Add(invo);
                                                _invoiceRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                                            }

                                            //get the License receipt Code

                                            var rpt = _recptRep.FindBy(a => a.ApplicationReference == ordid && a.ApplicationId == ap.Id).FirstOrDefault();
                                            if (rpt == null)
                                            {
                                                rpt = new Receipt();
                                                rpt.Amount = Convert.ToDouble(ptrns.Transaction_Amount);
                                                rpt.ApplicationId = ap.Id;
                                                rpt.ApplicationReference = ordid;
                                                rpt.CompanyName = ptrns.CompanyName;
                                                rpt.Date_Paid = Convert.ToDateTime(response.transactiontime);
                                                rpt.InvoiceId = Convert.ToInt32(invo.Id);
                                                rpt.ReceiptNo = "---"; // UtilityHelper.GenerateReceiptNo(rpt.Amount, ap.Id, app.Id.ToString("0#")); // lc.ReceiptCode);
                                                rpt.RRR = ptrns.RRR;
                                                rpt.Status = "Paid";
                                                _recptRep.Add(rpt);
                                                _recptRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                                                rpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rpt.Amount, rpt.Id, lc.ReceiptCode, rpt.Date_Paid);
                                                _recptRep.Edit(rpt);
                                                _recptRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                                        {
                                            ReasonPhrase = "Payment is not completed yet for this Application"
                                        });
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                trans.Dispose();
                                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                                {
                                    ReasonPhrase = "Some Error Occured while handling your Request: " + ex.Message
                                });
                            }

                        }
                    }

                    ap.Status = model.Status;
                    _appRep.Edit(ap);
                    _appRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                    trans.Complete();
                    return Ok(ap);
                }
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                ReasonPhrase = "Status/OrderId cannot be null"
            });
        }

        [ResponseType(typeof(Application))]
        [Route("TestApplication/{email}/{apiHash}"),ApiExplorerSettings(IgnoreApi=true)]
        public IHttpActionResult PutTestApplication(string email, string apiHash, Application model)
        {
            #region
            if (string.IsNullOrEmpty(email))
            {
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                //{
                //    ReasonPhrase = "App UserName cannot be empty"
                //});
                return Ok(new { code = 1, message = "App UserName cannot be empty" });
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {
                return Ok(new { code = 2, message = "App has been denied Access, Contact NUPRC Dev" });
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                //{
                //    ReasonPhrase = "App has been denied Access, Contact DPR Dev"
                //});
            }

            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                //{
                //    ReasonPhrase = "App has been denied Access, Contact DPR Dev"
                //});
                return Ok(new { code = 3, message = "App has been denied Access, Contact NUPRC Dev" });
            }

            #endregion

            using (var trans = new TransactionScope())
            {
                if (model != null && !string.IsNullOrEmpty(model.Status) && !string.IsNullOrEmpty(model.OrderId))
                {
                    var ap = _appRep.FindBy(a => a.OrderId == model.OrderId).FirstOrDefault();
                    if (ap == null)
                    {
                        //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                        //{
                        //    ReasonPhrase = "Application Not Found"
                        //});
                        return Ok(new { code = 4, message = "Application Not Found" });
                    }

                    var stat = "Started";
                    if (model.Status == ApplicationStatus.Processing)
                    {

                        //check transaction table to see if the application payment is completed
                        var ptrns = _payTransRep.FindBy(a => a.Order_Id == model.OrderId || a.Reference_Number == model.OrderId).FirstOrDefault();

                        if (ptrns != null && ptrns.Completed)
                        {
                            //check if the app paymennt  have been completed
                            ap.Status = ApplicationStatus.Processing;
                        }
                        else
                        {
                            try
                            {
                                var ordid = !string.IsNullOrEmpty(ptrns.Order_Id) ? ptrns.Order_Id : ptrns.Reference_Number;
                                string hash_string = ordid + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
                                string hash = PaymentRef.getHash(hash_string);
                                var pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, RemitaSplitParams.MERCHANTID, ordid, hash.ToLower());
                                NewRemitaResponse response = new NewRemitaResponse();

                                using (WebClient client = new WebClient())
                                {
                                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                    string responseJson = client.DownloadString(pUrl);

                                    response = JsonConvert.DeserializeObject<NewRemitaResponse>(responseJson);
                                    if (response != null)
                                    {
                                        if (response.status == "01" || response.status == "00")
                                        {
                                            #region Payment Valid and Approved, Give value

                                            //Update Remita_Transaction table

                                            ptrns.Query_Date = UtilityHelper.CurrentTime;
                                            ptrns.Response_Code = response.status;
                                            ptrns.Response_Description = response.message;
                                            ptrns.Type = "BANK";
                                            ptrns.Completed = false;
                                            //_payTransRep.Edit(ptrns);
                                            //_payTransRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                            stat += "PTrans Saved; ";

                                            ap.Status = ApplicationStatus.PaymentCompleted.ToString();

                                            _appRep.Edit(ap);
                                            _appRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                            stat += "Application Saved; ";
                                            //Update Invoice and Create Receipt

                                            var invo = _invoiceRep.FindBy(a => a.Payment_Code == ptrns.Order_Id).FirstOrDefault();
                                            if (invo != null)
                                            {
                                                invo.Status = "Paid";
                                                invo.Date_Paid = Convert.ToDateTime(response.transactiontime);

                                                _invoiceRep.Edit(invo);
                                                _invoiceRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                                stat += "Invoice Updated; ";
                                            }
                                            else
                                            {
                                                invo = new Invoice();
                                                invo.Amount = Convert.ToDouble(ptrns.Transaction_Amount);
                                                invo.Application_Id = ap.Id;
                                                invo.Payment_Code = ptrns.Order_Id;
                                                invo.Payment_Type = "Remita Bank";
                                                invo.Status = "Paid";
                                                invo.Date_Added = UtilityHelper.CurrentTime;
                                                invo.Date_Paid = Convert.ToDateTime(response.transactiontime);
                                                invo.PaymentTransaction_Id = ptrns.Id;

                                                _invoiceRep.Add(invo);
                                                _invoiceRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                                                stat += "Invoice Saved; ";

                                            }

                                            //get the License receipt Code
                                            var rpt = _recptRep.FindBy(a => a.ApplicationReference == ptrns.Order_Id && a.ApplicationId == ap.Id).FirstOrDefault();
                                            if (rpt == null)
                                            {
                                                rpt = new Receipt();
                                                rpt.Amount = Convert.ToDouble(ptrns.Transaction_Amount);
                                                rpt.ApplicationId = ap.Id;
                                                rpt.ApplicationReference = ptrns.Order_Id;
                                                rpt.CompanyName = ptrns.CompanyName;
                                                rpt.Date_Paid = Convert.ToDateTime(response.transactiontime);
                                                rpt.InvoiceId = invo != null ? Convert.ToInt32(invo.Id) : 0;
                                                rpt.ReceiptNo = "---"; // UtilityHelper.GenerateReceiptNo(rpt.Amount, ap.Id, app.Id.ToString("0#")); // lc.ReceiptCode);
                                                rpt.RRR = ptrns.RRR;
                                                rpt.Status = "Paid";
                                                _recptRep.Add(rpt);
                                                _recptRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                                                rpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rpt.Amount, rpt.Id, app.ReceiptCode, rpt.Date_Paid);
                                                _recptRep.Edit(rpt);
                                                _recptRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                                                stat += "Receipt Saved; ";
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            return Ok(new { code = 6, message = "Payment is not completed yet for this Application" });
                                            //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                                            //{
                                            //    ReasonPhrase = "Payment is not completed yet for this Application"
                                            //});
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                trans.Dispose();
                                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                                //{
                                //    ReasonPhrase = "Some Error Occured while handling your Request: " + ex.Message
                                //});
                                var msg = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message;
                                return Ok(new { code = 5, message = msg, status = stat });
                            }

                        }
                    }

                    //ap.Status = model.Status;

                    //_appRep.Edit(ap);
                    //_appRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                    trans.Complete();
                    //return Ok(ap);
                    return Ok(new { code = 0, message = "Application Processing completed: " + ap.Status });
                }
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                ReasonPhrase = "Status/OrderId cannot be null"
            });
        }
    }
}
