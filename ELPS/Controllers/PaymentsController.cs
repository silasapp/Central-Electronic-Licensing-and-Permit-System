
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ELPS.Domain.Abstract;
using System.Web.Http.Description;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using System.Web;
using ELPS.Models;
using System.Configuration;
using System.Web.Script.Serialization;

using System.Security.Cryptography;
using Rotativa;
using System.Net.Mail;
using System.IO;
using System.Transactions;
using Newtonsoft.Json;
using System.Text;

namespace ELPS.Controllers
{
    [RoutePrefix("api/Payments")]
    public class PaymentsController : ApiController
    {
        ILicenseRepository _licensRep;
        IReceiptRepository _recptRep;
        IInvoiceRepository _invoiceRep;
        IApplicationRepository _appRep;
        IvApplicationRepository _vAppRep;
        ICompanyRepository _compRep;
        IAppIdentityRepository _appIdRep;
        IPermitRepository _permitRep;
        IAddressRepository _addRep;
        ICompany_DocumentRepository _compDocRep;
        IApplicationRequirementRepository _appReqRep;
        IPayment_TransactionRepository _payTransRep;
        IvReceiptRepository _vRctRep;
        IRawRemitaResponseRepository _rawRemitaRep;
        WebApiAccessHelper accessHelper; // = new (appId);

        string _marchantId = ConfigurationManager.AppSettings["merchantID"];
        string _responseUrl = ConfigurationManager.AppSettings["RemitaPaymentCallback"];
        string _apiKey = ConfigurationManager.AppSettings["rKey"];

        public PaymentsController(ICompanyRepository compRep, IAppIdentityRepository appIdRep, IPermitRepository permitRep,
            IAddressRepository addRep, IPayment_TransactionRepository payTransRep, IApplicationRequirementRepository appReqRep,
             IApplicationRepository appRep, ICompany_DocumentRepository compDocRep, IInvoiceRepository invoiceRep, IReceiptRepository recptRep,
            ILicenseRepository licensRep, IvReceiptRepository vRctRep, IvApplicationRepository vAppRep, IRawRemitaResponseRepository rawRemitaRep)
        {
            _rawRemitaRep = rawRemitaRep;
            _vAppRep = vAppRep;
            _vRctRep = vRctRep;
            _recptRep = recptRep;
            _invoiceRep = invoiceRep;
            _appRep = appRep;
            _compRep = compRep;
            _appIdRep = appIdRep;
            _permitRep = permitRep;
            _addRep = addRep;
            _payTransRep = payTransRep;
            _appReqRep = appReqRep;
            _compDocRep = compDocRep;
            _licensRep = licensRep;

            accessHelper = new WebApiAccessHelper(appIdRep);
        }


        /// <summary>
        /// Prepares a License Payment by getting RRR from remita
        /// </summary>
        /// <param name="CompId">Company Id</param>
        /// <param name="email">API Email</param>
        /// <param name="apiHash">API Hash</param>
        /// <param name="Split">Payment Splitting</param>
        /// <returns></returns>
        [ResponseType(typeof(PrePaymentResponse))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult Payments(int CompId, string email, string apiHash, PaymentSplit Split)
        {
            #region
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }
            #endregion
            var app = _appIdRep.FindBy(a => a.Email.ToLower() == email.ToLower()).FirstOrDefault();
            UtilityHelper.LogMessage("Processing Payment: " + app.LicenseName);
            #region logic
            var response = new CanAccessResponse();
            try
            {
                var pTrans = new Payment_Transaction();
                var ppr = new PrePaymentResponse();

                if (Split != null)
                {
                    UtilityHelper.LogMessage("Split not NULL... Continue");
                    //UtilityHelper.LogMessage(JsonConvert.SerializeObject(Split));

                    #region Prepare Payment Transaction
                    var pick = _payTransRep.FindBy(a => a.Order_Id == Split.orderId || a.Reference_Number == Split.orderId).FirstOrDefault();

                    UtilityHelper.LogMessage("Looking for Transaction:......");
                    if (pick == null)
                    {
                        UtilityHelper.LogMessage("PTransaction is NULL, creating new");
                        pTrans.ReturnSuccessUrl = Split.ReturnSuccessUrl;
                        pTrans.ReturnFailureUrl = Split.ReturnFailureUrl;
                        pTrans.ReturnBankPaymentUrl = Split.ReturnBankPaymentUrl;
                        pTrans.Approved_Amount = Split.AmountDue;
                        pTrans.BankPaymentEndPoint = app.BankPaymentEndPoint;
                        pTrans.Transaction_Amount = Split.totalAmount;
                        pTrans.CompanyId = CompId;
                        pTrans.CompanyName = Split.payerName;
                        pTrans.ServiceTypeId = Split.serviceTypeId;
                        pTrans.Order_Id = Split.orderId;
                        pTrans.Reference_Number = Split.orderId;
                        pTrans.ServiceCharge = Split.ServiceCharge;
                        //pTrans.DocumentType = Split.DocumentTypes;
                        UtilityHelper.LogMessage("Calling RegisterRemitaTransaction METHOD");
                        pTrans = RegisterRemitaTransaction(pTrans, Split.CategoryName, app.Id);
                    }
                    else
                    {
                        UtilityHelper.LogMessage("PTransaction is existing");
                        pTrans = pick;
                    }
                    #endregion

                    UtilityHelper.LogMessage("Checking to see if to go Generate RRR or NOT");
                    var resp = new _NewRemitaResponse(); // RemitaResponse();
                    if (Convert.ToInt32(Split.totalAmount) <= 0 || (!string.IsNullOrEmpty(pTrans.RRR) && pTrans.RRR.ToLower() == "NUPRC-Bank-M".ToLower()))
                    {
                        UtilityHelper.LogMessage("POINT A1");
                        resp = new _NewRemitaResponse() //RemitaResponse()
                        {
                            RRR = !string.IsNullOrEmpty(pTrans.RRR) ? pTrans.RRR : "NUPRC-ELPS",
                            //merchantId = _marchantId,
                            orderId = Split.orderId,
                            //statuscode = "088",
                            status = "01",
                            message = "No Payment Required",
                            paymentDate = pTrans.TransactionDate == null ? pTrans.Transaction_Date : pTrans.TransactionDate.ToString(),
                            amount = pTrans.Transaction_Amount,
                            //statusmessage = "No Payment Required",
                            transactiontime = DateTime.Now.ToString()
                        };
                    }
                    else if (!string.IsNullOrEmpty(pTrans.RRR))
                    {
                        UtilityHelper.LogMessage("POINT A2");
                        resp = new _NewRemitaResponse() // RemitaResponse()
                        {
                            RRR = pTrans.RRR,
                            //merchantId = _marchantId,
                            orderId = pTrans.Order_Id,
                            //statuscode = pTrans.Response_Code,
                            amount = pTrans.Transaction_Amount,
                            paymentDate = pTrans.TransactionDate == null ? pTrans.Transaction_Date : pTrans.TransactionDate.ToString(),
                            status = pTrans.Response_Code,
                            //statusmessage = pTrans.Response_Description,
                            message = pTrans.Response_Description,
                            transactiontime = DateTime.Now.ToString()
                        };
                    }
                    else if (Split.lineItems.Count > 0)
                    {
                        #region Prepare for Remita and place Call
                        UtilityHelper.LogMessage("POINT A3: >>> Answering call to generate RRR for (" + pTrans.Reference_Number + ")");
                        var rs = new RemitaSplit();
                        rs.lineItems = Split.lineItems;
                        if (Split.customFields != null && Split.customFields.Count > 0)
                        {
                            rs.customFields = Split.customFields;
                        }
                        rs.merchantId = _marchantId;
                        rs.orderId = Split.orderId;
                        rs.payerEmail = Split.payerEmail;
                        rs.payerName = Split.payerName;
                        rs.payerPhone = Split.payerPhone;
                        //rs.responseurl = _responseUrl.ToLower(); //replace with our return url
                        rs.serviceTypeId = Split.serviceTypeId;
                        rs.amount = Split.totalAmount;

                        string hashItem = _marchantId + rs.serviceTypeId + rs.orderId + rs.amount + _apiKey; // removed responseurl
                        string hash = PaymentRef.getHash(hashItem, true).ToLower();
                        //rs.hash = hash.ToLower();

                        // rm.Partners.AddRange(rp);

                        var jn = JsonConvert.SerializeObject(rs);


                        UtilityHelper.LogMessage("Raw data to REMITA >>> " + jn);
                        #region Remita Call Back api to get RRR

                        var rUrl = ConfigurationManager.AppSettings["RemitaSplitUrl"];
                        string output = "";
                        string eMsg = "";
                        using (WebClient client = new WebClient())
                        {
                            //try
                            //{
                                UtilityHelper.LogMessage("On the way to Remita");
                                var raw = new RawRemitaResponse() { DateAdded = UtilityHelper.CurrentTime, Direction = $"To REMITA (For: {app.ShortName})", JsonData = jn };
                                _rawRemitaRep.Add(raw);
                                _rawRemitaRep.Save(app.Email, app.Email);

                                UtilityHelper.LogMessage(rUrl);
                                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + _marchantId + ",remitaConsumerToken=" + hash);

                                output = client.UploadString(rUrl, "POST", jn);
                                UtilityHelper.LogMessage(output);
                            //}
                            //catch (Exception ex)
                            //{
                            //    UtilityHelper.LogMessage("ERROR ERROR ERROR: Cannot get from REMITA");
                            //    UtilityHelper.LogMessage("Error after Call to get RRR:>>> " + ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message);
                            //    eMsg = ex.Message;

                            //    response.Code = (int)ResponseCodes.ServerError;
                            //    response.Message = "Internal Server Error:: " + eMsg + "\n" + ex.StackTrace + "\n" + ex.InnerException;

                            //    return Ok(response);
                            //}
                        }

                        _rawRemitaRep.Add(new RawRemitaResponse() { DateAdded = UtilityHelper.CurrentTime, Direction = $"FROM REMITA", JsonData = output });
                        _rawRemitaRep.Save(app.Email, app.Email);
                        output = output.Replace("jsonp (", "");
                        output = output.Replace(")", "");

                        #endregion

                        //jsonp({"statuscode":"025","RRR":"270358747919","orderID":"763295697043","status":"Payment Reference generated"})
                        resp = JsonConvert.DeserializeObject<_NewRemitaResponse>(output);
                        #endregion
                    }

                    if((resp != null && resp.statusMessage != null) && (resp.statuscode == "028" || resp.statusMessage.ToLower().ToLower().Trim() == "duplicate order ref"))
                    {
                        UtilityHelper.LogMessage("Duplicate Order: " + resp.statusMessage + " >>> " + resp.statuscode);
                        // For DUPLICATE order
                        string hash_string = Split.orderId + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
                        string hash = PaymentRef.getHash(hash_string, true).ToLower();

                        //Ref no submitted (Order Id)
                        var pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, RemitaSplitParams.MERCHANTID, Split.orderId, hash);

                        using (WebClient client = new WebClient())
                        {
                            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                            client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);


                            string responseJson = client.DownloadString(pUrl);
                            UtilityHelper.LogMessage("Response fro Duplicate Order::: " + responseJson);
                            //{"amount":31500.0,"RRR":"160358730447","orderId":"529319384014","message":"Transaction Pending","transactiontime":"2019-12-11 12:00:00 AM","status":"021"}

                            var _resp = JsonConvert.DeserializeObject<_NewRemitaResponse>(responseJson);
                            if (!string.IsNullOrEmpty(_resp.RRR))
                            {
                                resp.amount = _resp.amount;
                                resp.orderId = _resp.orderId;
                                resp.RRR = _resp.RRR;
                                resp.orderId = _resp.orderId;
                                resp.status = _resp.status;
                                resp.message = _resp.statusMessage == null ? _resp.message : _resp.statusMessage;
                                resp.transactiontime = _resp.transactiontime;
                            }
                            UtilityHelper.LogMessage("RRR = " + resp.RRR);
                        }
                    }
                    
                    // Fix for new RRR generation
                    if(resp != null && resp.statuscode == "025")
                    {
                        //jsonp({"statuscode":"025","RRR":"270358747919","orderID":"763295697043","status":"Payment Reference generated"})
                        resp.amount = pTrans.Transaction_Amount;
                    }

                    if (resp != null && !string.IsNullOrEmpty(resp.RRR))
                    {
                        //Save RRR on a Temp table to prevent failure
                        ppr.RRR = resp.RRR;
                        ppr.AppId = pTrans.Reference_Number;
                        ppr.Status = resp.statuscode == "025" ? resp.statuscode : resp.statusMessage;
                        ppr.statusmessage = resp.statuscode == "025" ? resp.message : resp.statusMessage;
                        ppr.message = resp.message;
                        ppr.Transactiontime = resp.transactiontime;
                    }
                    else
                    {
                        UtilityHelper.LogMessage($"Returning Error for RRR generation :: RRR = {resp.RRR}");
                        #region Error
                        string RError2 = "";
                        if (resp != null && string.IsNullOrEmpty(resp.RRR))
                            RError2 = "Status Code: " + resp.status + "; Status Mesage: " + resp.statusMessage;

                        response.Code = (int)ResponseCodes.BadRequest;
                        response.Message = RError2 == "" ? "App UserName cannot be empty" : RError2;

                        return Ok(response);
                        #endregion
                    }

                    #region All done, prepare and return
                    if (Split.ApplicationItems != null && Split.ApplicationItems.Count > 0)
                    {
                        pTrans.ApplicationItem = new JavaScriptSerializer().Serialize(Split.ApplicationItems);
                        UtilityHelper.LogMessage("App Items: " + pTrans.ApplicationItem);
                    }
                    var rqDocs = new List<int>();
                    string dt = "";
                    if (Split.DocumentTypes != null && Split.DocumentTypes.Count > 0)
                    {
                        foreach (var item in Split.DocumentTypes)
                        {
                            var compDoc = _compDocRep.FindBy(a => a.Archived == false && a.Company_Id == CompId && a.Document_Type_Id == item).FirstOrDefault();
                            if (compDoc == null)
                            {
                                rqDocs.Add(item);
                            }
                            dt += item.ToString() + ";";
                        }
                        ppr.RequiredDocs = rqDocs;
                    }
                    pTrans.DocumentType = dt;
                    UtilityHelper.LogMessage("Doc types req: >>> " + pTrans.DocumentType);

                    //var trns = RecordRemitaTransaction(pTrans,
                    //    new _NewRemitaResponse
                    //    {
                    //        amount = resp.amount,
                    //        message = resp.message,
                    //        transactiontime = resp.transactiontime,
                    //        orderId = resp.orderId,
                    //        RRR = resp.RRR,
                    //        status = resp.status
                    //    }, Split.CategoryName, app.Id);
                    var trns = RecordRemitaTransaction(pTrans, resp, Split.CategoryName, app.Id);

                    ppr.TransactionId = trns.Id.ToString();
                    #endregion

                    return Ok(ppr);
                }
                else
                {
                    UtilityHelper.LogMessage("Payment Split data is null");
                    #region Error

                    throw new ArgumentException("Line Items, Payment Partners cannot be Null");
                    #endregion
                }
            }
            catch (Exception ex)
            {
                UtilityHelper.LogMessage("Final Catch >>> " + (ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message));
                #region Error
                response.Code = (int)ResponseCodes.ServerError;
                response.Message = "Some Error while handling your Request => " + ex.Message + "\n" + ex.StackTrace + "\n" +ex.InnerException;

                return Ok(response);
                #endregion
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CompId"></param>
        /// <param name="email"></param>
        /// <param name="apiHash"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [ResponseType(typeof(PrePaymentResponse))]
        [Route("ExtraPayment/{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult ExtraPayment(int CompId, string email, string apiHash, RemitaPaymentModel model)
        {
            #region Authentication
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }
            var app = _appIdRep.FindBy(a => a.Email.ToLower() == email.ToLower()).FirstOrDefault();
            UtilityHelper.LogMessage("Processing Extra Payment on: " + app.LicenseName);
            #endregion

            #region Logic
            using(var trans = new TransactionScope())
            {

                try
                {
                    if (model != null)
                    {
                        var pTrans = new Payment_Transaction();
                        var ppr = new PrePaymentResponse();
                        UtilityHelper.LogMessage("Extra Pay Model not NULL... Continue");

                        #region Prepare Payment Transaction
                        var pick = _payTransRep.FindBy(a => a.Order_Id == model.orderId || a.Reference_Number == model.orderId).FirstOrDefault();
                        UtilityHelper.LogMessage("Looking for Transaction:......");
                        if (pick == null)
                        {
                            pTrans.ReturnSuccessUrl = model.ReturnSuccessUrl;
                            pTrans.ReturnFailureUrl = model.ReturnFailureUrl;
                            pTrans.ReturnBankPaymentUrl = model.ReturnBankPaymentUrl;
                            pTrans.Approved_Amount = model.Amount;
                            pTrans.BankPaymentEndPoint = app?.BankPaymentEndPoint;
                            pTrans.Transaction_Amount = model.Amount;
                            pTrans.CompanyId = CompId;
                            pTrans.CompanyName = model.payerName;
                            pTrans.ServiceTypeId = model.serviceTypeId;
                            pTrans.Order_Id = model.orderId;
                            pTrans.Reference_Number = model.orderId;
                            pTrans.ServiceCharge = 0.ToString();
                            //pTrans.DocumentType = Split.DocumentTypes;

                            pTrans = RegisterRemitaTransaction(pTrans, "", app.Id);
                        }
                        else
                        {
                            pTrans = pick;
                        }

                        var resp = new RemitaResponse();
                        if (!string.IsNullOrEmpty(pTrans.RRR))
                        {
                            resp = new RemitaResponse()
                            {
                                RRR = pTrans.RRR,
                                merchantId = _marchantId,
                                orderId = pTrans.Order_Id,
                                statuscode = pTrans?.Response_Code,
                                status = pTrans?.Response_Code,
                                statusMessage = pTrans?.Response_Description,
                                transactiontime = DateTime.Now.ToString()
                            };
                        }
                        else
                        {
                            #region Prepare for Remita and place Call
                            UtilityHelper.LogMessage("Answering call to generate RRR for Extra Payment");
                            string hashItem = _marchantId + model.serviceTypeId + model.orderId + model.Amount + _apiKey; //removed model.responseurl
                            string hash = PaymentRef.getHash(hashItem, true).ToLower();
                            model.hash = hash.ToLower();
                            model.merchantId = _marchantId;

                            var remitaModel = new RemitaSplit()
                            {
                                //hash = hash.ToLower(),
                                merchantId = _marchantId,
                                orderId = pTrans.Order_Id.ToLower(),
                                payerEmail = model.payerEmail,
                                payerName = model.payerName,
                                payerPhone = model.payerPhone,
                                serviceTypeId = model.serviceTypeId,
                                amount = model.Amount,
                                //responseurl = model.responseurl.ToLower(),
                                lineItems = model.LineItems
                            };
                            if (model.customFields != null && model.customFields.Count > 0)
                            {
                                remitaModel.customFields = model.customFields;
                            }
                            // rm.Partners.AddRange(rp);
                            var jn = JsonConvert.SerializeObject(remitaModel);
                            UtilityHelper.LogMessage(jn);

                            #region Remita Call Back api to get RRR

                            var rUrl = ConfigurationManager.AppSettings["RemitaSplitUrl"];
                            string output = "";
                            string eMsg = "";
                            using (WebClient client = new WebClient())
                            {
                                try
                                {
                                    UtilityHelper.LogMessage(rUrl);
                                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                    client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + _marchantId + ",remitaConsumerToken=" + hash);

                                    output = client.UploadString(rUrl, "POST", jn);
                                    UtilityHelper.LogMessage(output);
                                }
                                catch (Exception ex)
                                { 
                                    UtilityHelper.LogMessage("Error after Call to get RRR:>>> " + ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message);
                                    eMsg = ex.Message;

                                    check.Code = (int)ResponseCodes.ServerError;
                                    check.Message = "Internal Server Error: " + eMsg;

                                    return Ok(check);
                                    //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                                    //{
                                    //    ReasonPhrase = eMsg //"There was an Error getting your RRR"
                                    //});
                                }
                            }
                            //ErrMessage: jsonp({ "statuscode":"028","status":"Duplicate Order Ref"})

                            output = output.Replace("jsonp (", "");
                            output = output.Replace(")", "");

                            #endregion

                            resp = JsonConvert.DeserializeObject<RemitaResponse>(output);
                            #endregion

                            if (resp != null && (resp?.statuscode == "028" || resp?.statuscode == "025" || resp?.statusMessage?.ToLower().Trim() == "duplicate order ref"))
                            {
                                UtilityHelper.LogMessage("Duplicate Order: " + resp?.statusMessage + " >>> " + resp?.statuscode);
                                // For DUPLICATE order
                                string hash_string = model.orderId + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
                                hash = PaymentRef.getHash(hash_string, true).ToLower();

                                //Ref no submitted (Order Id)
                                var pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, RemitaSplitParams.MERCHANTID, model.orderId, hash);

                                using (WebClient client = new WebClient())
                                {
                                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                                    client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                                    string responseJson = client.DownloadString(pUrl);

                                    var _resp = JsonConvert.DeserializeObject<_NewRemitaResponse>(responseJson);

                                    if (!string.IsNullOrEmpty(_resp.RRR))
                                    {
                                        resp.RRR = _resp.RRR;
                                        resp.orderId = _resp.orderId;
                                    }
                                    UtilityHelper.LogMessage("RRR = " + resp.RRR);
                                }
                            }

                            if (resp != null && !string.IsNullOrEmpty(resp.RRR))
                            {
                                //Save RRR on a Temp table to prevent failure
                                ppr.RRR = resp.RRR;
                                ppr.AppId = app.AppId;
                                ppr.Status = resp?.status;
                                ppr.statusmessage = resp.statusMessage == null ? resp?.status : resp?.statusMessage;
                                ppr.message = resp?.message;
                                ppr.Transactiontime = resp?.transactiontime;
                            }
                            else
                            {
                                #region Error
                                string RError2 = "";
                                if (resp != null && string.IsNullOrEmpty(resp.RRR))
                                    RError2 = "Status Code: " + resp?.statuscode + "; Status Mesage: " + resp?.status;

                                check.Code = (int)ResponseCodes.BadRequest;
                                check.Message = RError2 == "" ? "App UserName cannot be empty" : RError2;

                                return Ok(check);
                                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                                //{
                                //    ReasonPhrase = RError2 == "" ? "App UserName cannot be empty" : RError2
                                //});
                                #endregion
                            }

                            trans.Complete();
                            return Ok(ppr);
                        }

                        throw new ArgumentException("Something went wrong");
                        #endregion
                    }
                    else
                    {
                        UtilityHelper.LogMessage("Extra Payment Model data is null");
                        #region Error
                        check.Code = (int)ResponseCodes.BadRequest;
                        check.Message = "Extra Payment Model data cannot be Null";

                        return Ok(check);
                        //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                        //{
                        //    ReasonPhrase = "Extra Payment Model data cannot be Null"
                        //});
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    UtilityHelper.LogMessage("Final Catch >>> " + ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message);
                    trans.Dispose();
                    check.Code = (int)ResponseCodes.ServerError;
                    check.Message = "Some Errors were encountered while handling your Request: " + ex.Message;

                    return Ok(check);
                    //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    //{
                    //    ReasonPhrase = "Some Errors were encountered while handling your Request: " + ex.Message
                    //});
                }
            }
            #endregion
        }

        private Payment_Transaction RecordRemitaTransaction(Payment_Transaction ptrans, _NewRemitaResponse resp, string categoryName, int licenceId, string pType = "Online")
        {
            var now = UtilityHelper.CurrentTime;
            UtilityHelper.LogMessage("Recording Remita Transactions");
            try
            {
                if (ptrans.Id <= 0)
                {
                    UtilityHelper.LogMessage("PTrans not existing, saving it");
                    var pt = new Payment_Transaction();
                    pt.Approved_Amount = ptrans.Approved_Amount;
                    pt.CompanyId = ptrans.CompanyId;
                    pt.CompanyName = ptrans.CompanyName;
                    pt.Order_Id = ptrans.Order_Id;
                    pt.Payment_Reference = resp.RRR;
                    pt.PaymentSource = "Remita";
                    pt.Reference_Number = ptrans.Order_Id;
                    pt.Response_Code = resp.statuscode == "025" ? resp.statuscode : resp.statusMessage;
                    pt.Response_Description = resp.statuscode == "025" ? resp.status : resp.statusMessage; //.statusmessage;
                    pt.RRR = resp.RRR;
                    pt.Transaction_Amount = ptrans.Transaction_Amount;
                    pt.Transaction_Currency = "566";
                    pt.Transaction_Date = now.ToString();
                    pt.Type = pType;
                    pt.Query_Date = ptrans.Query_Date;
                    pt.ReturnSuccessUrl = ptrans.ReturnSuccessUrl;
                    pt.ReturnFailureUrl = ptrans.ReturnFailureUrl;
                    pt.ReturnBankPaymentUrl = ptrans.ReturnBankPaymentUrl;
                    pt.DocumentType = ptrans.DocumentType;
                    pt.ApplicationItem = ptrans.ApplicationItem;
                    pt.ServiceTypeId = ptrans.ServiceTypeId;
                    pt.ServiceCharge = ptrans.ServiceCharge;
                    //pt.TransactionDate = now;
                    DateTime tt;
                    if (!string.IsNullOrEmpty(resp.paymentDate) && DateTime.TryParse(resp.paymentDate, out tt))
                    {
                        pt.TransactionDate = tt;
                    }
                    else
                        pt.TransactionDate = now; // DateTime.Parse(resp.transactiontime);

                    //_payTransRep.Add(ptrans);
                    _payTransRep.Add(pt);
                    _payTransRep.Save(User.Identity.Name, HttpContext.Current.Request.UserHostAddress);
                    UtilityHelper.LogMessage("PTrans saved: " + pt.Reference_Number + ">>> " + pt.RRR);

                    var app = _appRep.FindBy(a => a.OrderId == ptrans.Order_Id).FirstOrDefault();
                    if (app == null)
                    {
                        app = new Application();
                        app.CategoryName = categoryName;
                        app.CompanyId = ptrans.CompanyId;
                        app.Date = now;
                        app.OrderId = ptrans.Order_Id;
                        app.LicenseId = licenceId;
                        app.Status = ApplicationStatus.PaymentPending.ToString();

                        _appRep.Add(app);
                        _appRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);
                    }

                    //Create Invoice 
                    var invo = _invoiceRep.FindBy(a => a.Payment_Code == pt.Order_Id).FirstOrDefault();
                    if (invo == null)
                    {
                        invo = new Invoice();
                        invo.Amount = Convert.ToDouble(pt.Transaction_Amount);
                        invo.Application_Id = app.Id;
                        invo.Payment_Code = pt.Order_Id;
                        invo.Payment_Type = string.Empty;
                        invo.Status = "Unpaid";
                        invo.Date_Added = UtilityHelper.CurrentTime;
                        invo.Date_Paid = UtilityHelper.CurrentTime.AddDays(-7);

                        _invoiceRep.Add(invo);
                        _invoiceRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);
                    }

                    ptrans = pt;
                }
                else
                {
                    var jn = new JavaScriptSerializer().Serialize(ptrans);
                    UtilityHelper.LogMessage("PTrans existing, Editing it:>> " + jn);
                    UtilityHelper.LogMessage($"Response to Update PTrans with >>> {JsonConvert.SerializeObject(resp)}");
                    ptrans.Payment_Reference = resp.RRR;
                    ptrans.PaymentSource = "Remita";
                    ptrans.Reference_Number = ptrans.Order_Id;
                    ptrans.Response_Code = resp.statuscode == "025" ? resp.statuscode : resp.statusMessage;
                    ptrans.Response_Description = resp.statuscode == "025" ? resp.status : resp.statusMessage; //.statusmessage;
                    ptrans.RRR = resp.RRR;
                    ptrans.Transaction_Currency = "566";
                    ptrans.Transaction_Date = !string.IsNullOrEmpty(resp.transactiontime) ? resp.transactiontime : now.ToString();
                    //ptrans.TransactionDate = now;
                    ptrans.Type = pType;
                    DateTime tt;
                    if (!string.IsNullOrEmpty(resp.paymentDate) && DateTime.TryParse(resp.paymentDate, out tt))
                    {
                        ptrans.TransactionDate = tt;
                    }
                    else
                        ptrans.TransactionDate = now; // DateTime.Parse(resp.transactiontime);

                    UtilityHelper.LogMessage("PTrans after Editing (before saving)>> " + jn);
                    _payTransRep.Edit(ptrans);
                    _payTransRep.Save(User.Identity.Name, HttpContext.Current.Request.UserHostAddress);
                    UtilityHelper.LogMessage("PTrans Updated");

                    var app = _appRep.FindBy(a => a.OrderId == ptrans.Order_Id).FirstOrDefault();
                    if (app == null)
                    {
                        app = new Application();
                        app.CategoryName = categoryName;
                        app.CompanyId = ptrans.CompanyId;
                        app.Date = DateTime.Now;
                        app.OrderId = ptrans.Order_Id;
                        app.LicenseId = licenceId;
                        app.Status = ApplicationStatus.PaymentPending.ToString();

                        _appRep.Add(app);
                        _appRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);
                    }

                    //Create Invoice 
                    var invo = _invoiceRep.FindBy(a => a.Payment_Code == ptrans.Order_Id).FirstOrDefault();
                    if (invo == null)
                    {
                        invo = new Invoice();
                        invo.Amount = Convert.ToDouble(ptrans.Transaction_Amount);
                        invo.Application_Id = app.Id;
                        invo.Payment_Code = ptrans.Order_Id;
                        invo.Payment_Type = string.Empty;
                        invo.Status = "Unpaid";
                        invo.Date_Added = UtilityHelper.CurrentTime;
                        invo.Date_Paid = UtilityHelper.CurrentTime.AddDays(-7);

                        _invoiceRep.Add(invo);
                        _invoiceRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);
                    }
                }

                UtilityHelper.LogMessage("Returning PTrans");
                return ptrans;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
            
        private Payment_Transaction RegisterRemitaTransaction(Payment_Transaction ptrans, string categoryName, int licenceId)
        {
            UtilityHelper.LogMessage("Adding PTrans Afresh");
            ptrans.Transaction_Currency = "566";
            ptrans.Transaction_Date = UtilityHelper.CurrentTime.ToString();

            _payTransRep.Add(ptrans);
            _payTransRep.Save(User.Identity.Name, HttpContext.Current.Request.UserHostAddress);

            var app = _appRep.FindBy(a => a.OrderId == ptrans.Order_Id).FirstOrDefault();
            if (app == null)
            {
                #region Create Application (if not existing)
                app = new Application();
                app.CategoryName = categoryName;
                app.CompanyId = ptrans.CompanyId;
                app.Date = DateTime.Now;
                app.OrderId = ptrans.Order_Id;
                app.LicenseId = licenceId;
                app.Status = ApplicationStatus.PaymentPending.ToString();

                _appRep.Add(app);
                _appRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);
                #endregion
            }

            //Create Invoice 
            var invo = _invoiceRep.FindBy(a => a.Payment_Code == ptrans.Order_Id).FirstOrDefault();
            if (invo == null)
            {
                #region Create Invoice (if not existing)
                invo = new Invoice();
                invo.Amount = Convert.ToDouble(ptrans.Transaction_Amount);
                invo.Application_Id = app.Id;
                invo.Payment_Code = ptrans.Order_Id;
                invo.Payment_Type = string.Empty;
                invo.Status = "Unpaid";
                invo.Date_Added = UtilityHelper.CurrentTime;
                invo.Date_Paid = UtilityHelper.CurrentTime.AddDays(-7);

                _invoiceRep.Add(invo);
                _invoiceRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);
                #endregion
            }


            return ptrans;
        }

        [ResponseType(typeof(RemitaResponse))]
        [Route("BankPaymentInfo/{CompId:int}/{email}/{apiHash}/{RRR}")]
        public IHttpActionResult GetBankPaymentInfo(int CompId, string email, string apiHash, string RRR)
        {
            #region
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }
            #endregion

            if (string.IsNullOrEmpty(RRR))
            {
                check.Code = (int)ResponseCodes.Invalid;
                check.Message = "Invalid login request";

                return Ok(check);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NoContent)
                //{
                //    ReasonPhrase = "Please supply RRR"
                //});
            }
            var pTrans = _payTransRep.FindBy(a => a.RRR == RRR).FirstOrDefault();
            RemitaResponse rrp;

            if (pTrans == null)
            {
                check.Code = (int)ResponseCodes.NotFound;
                check.Message = "Item does not Exist";

                return Ok(check);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                //{
                //    ReasonPhrase = "Item does not Exist"
                //});
            }
            else if (pTrans.Response_Code == "01" || pTrans.Response_Code == "00")
            {
                var ap = _appRep.FindBy(a => a.OrderId == pTrans.Order_Id).FirstOrDefault();
                //Update application Status
                if (ap != null)
                {
                    ap.Status = ApplicationStatus.PaymentCompleted;

                    _appRep.Edit(ap);
                    _appRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);
                }
                var app = _appIdRep.FindBy(a => a.Email.ToLower() == email.ToLower()).FirstOrDefault();
                UpdateInvoice_Receipt(ap.Id, Convert.ToDouble(pTrans.Transaction_Amount), pTrans.Order_Id, pTrans.RRR, pTrans.CompanyName, Convert.ToDateTime(pTrans.Transaction_Date), app.ReceiptCode);
                rrp = new RemitaResponse();
                rrp.orderId = pTrans.Order_Id;
                rrp.RRR = pTrans.RRR;
                rrp.status = pTrans.Response_Code;
                rrp.statusMessage = pTrans.Response_Description;
                rrp.transactiontime = pTrans.Transaction_Date;

                return Ok(rrp);
            }
            else
            {
                try
                {
                    var app = _appIdRep.FindBy(a => a.Email.ToLower() == email.ToLower()).FirstOrDefault();
                    #region call Back api [Verify payment from REMITA]

                    string hash_string = pTrans.Order_Id + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
                    string hash = PaymentRef.getHash(hash_string, true).ToLower();
                    var pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, RemitaSplitParams.MERCHANTID, pTrans.Order_Id, hash);
                    _NewRemitaResponse response = new _NewRemitaResponse();

                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                         client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                        string responseJson = client.DownloadString(pUrl);

                        response = JsonConvert.DeserializeObject<_NewRemitaResponse>(responseJson);
                        if (response != null)
                        {
                            rrp = new RemitaResponse();
                            rrp.orderId = response.orderId;
                            rrp.RRR = response.RRR;
                            rrp.status = response.status;
                            rrp.statusMessage = response.message;
                            rrp.message = response.message;
                            rrp.transactiontime = response.transactiontime;
                            rrp.Amount = response.amount;
                            if (response.status == "01" || response.status == "00")
                            {
                                #region Payment Valid and Approved, Give value

                                //Update Remita_Transaction table

                                pTrans.Query_Date = UtilityHelper.CurrentTime;
                                pTrans.Response_Code = response.status;
                                pTrans.Response_Description = response.message;
                                pTrans.Type = "BANK";
                                pTrans.Completed = true;
                                DateTime tt;
                                if (DateTime.TryParse(response.paymentDate, out tt))
                                {
                                    pTrans.TransactionDate = tt;
                                }
                                else
                                    pTrans.TransactionDate = DateTime.Parse(response.transactiontime);


                                _payTransRep.Edit(pTrans);
                                _payTransRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);

                                var ap = _appRep.FindBy(a => a.OrderId == pTrans.Order_Id).FirstOrDefault();
                                //Update application Status
                                if (ap != null)
                                {
                                    ap.Status = ApplicationStatus.PaymentCompleted;

                                    _appRep.Edit(ap);
                                    _appRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);
                                }
                                //Update Invoice and Create Receipt

                                UpdateInvoice_Receipt(ap.Id, Convert.ToDouble(pTrans.Transaction_Amount), pTrans.Order_Id, pTrans.RRR, pTrans.CompanyName, Convert.ToDateTime(pTrans.Transaction_Date), app.ReceiptCode);
                                #endregion
                                return Ok(rrp);

                            }
                            return Ok(rrp);
                        }
                        else
                        {
                            check.Code = (int)ResponseCodes.NotFound;
                            check.Message = "Item does not Exist";

                            return Ok(check);
                            //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                            //{
                            //    ReasonPhrase = "Item does not Exist"
                            //});
                        }
                    }

                    #endregion
                }
                catch (Exception)
                {
                    check.Code = (int)ResponseCodes.ServerError;
                    check.Message = "Some Error Occurred while handling your request";

                    return Ok(check);
                    //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    //{
                    //    ReasonPhrase = "Some Error Occurred while handling your request"
                    //});
                }
            }
        }





        private void UpdateInvoice_Receipt(int appId, double Amount, string OrderId, string RRR, string CompanyName, DateTime TransactionTime, string ReceiptCode)
        {
            var invo = _invoiceRep.FindBy(a => a.Payment_Code == OrderId).FirstOrDefault();
            if (invo != null)
            {
                invo.Status = "Paid";
                invo.Date_Paid = TransactionTime; // Convert.ToDateTime(response.transactiontime);

                _invoiceRep.Edit(invo);
                _invoiceRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);

            }

            var rpt = _recptRep.FindBy(a => a.ApplicationReference == OrderId).FirstOrDefault();
            if (rpt == null)
            {
                rpt = new Receipt();
                rpt.Amount = Amount; // Convert.ToDouble(pTrans.Transaction_Amount);
                rpt.ApplicationId = appId;
                rpt.ApplicationReference = OrderId;
                rpt.CompanyName = CompanyName;
                rpt.Date_Paid = TransactionTime; // Convert.ToDateTime(response.transactiontime);
                rpt.InvoiceId = Convert.ToInt32(invo.Id);
                rpt.ReceiptNo = "---"; // UtilityHelper.GenerateReceiptNo(rpt.Amount, ap.Id, app.Id.ToString("0#")); // lc.ReceiptCode);
                rpt.RRR = RRR;
                rpt.Status = "Paid";
                _recptRep.Add(rpt);
                _recptRep.Save(HttpContext.Current.Request.UserHostName, HttpContext.Current.Request.UserHostAddress);

                rpt.ReceiptNo = UtilityHelper.GenerateReceiptNo(rpt.Amount, rpt.Id, ReceiptCode, rpt.Date_Paid);
                _recptRep.Edit(rpt);
                _recptRep.Save(User.Identity.Name, HttpContext.Current.Request.UserHostAddress);
            }
        }







        /// <summary>
        /// Get the Remita Status of an Application 
        /// </summary>
        /// <param name="email">API Email</param>
        /// <param name="apiHash">API Hash</param>
        /// <param name="orderId">12 Digit Application Reference Number</param>
        /// <returns></returns>
        [ResponseType(typeof(RemitaResponse))]
        [Route("Status/{email}/{apiHash}/{orderId}")]
        public IHttpActionResult GetStatus(string email, string apiHash, string orderId)
        {
            #region
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }
            #endregion

            if (string.IsNullOrEmpty(orderId))
            {
                check.Code = (int)ResponseCodes.NoContent;
                check.Message = "Please supply Application Reference Number";

                return Ok(check);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NoContent)
                //{
                //    ReasonPhrase = "Please supply Application Reference Number"
                //});
            }
            var pTrans = _payTransRep.FindBy(a => a.Order_Id == orderId).FirstOrDefault();

            if (pTrans == null)
            {
                check.Code = (int)ResponseCodes.NotFound;
                check.Message = "Item does not Exist";

                return Ok(check);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                //{
                //    ReasonPhrase = "Item does not Exist"
                //});
            }


            string hash_string = orderId + RemitaSplitParams.APIKEY + RemitaSplitParams.MERCHANTID;
            string hash = PaymentRef.getHash(hash_string, true).ToLower();
            var pUrl = string.Format(RemitaSplitParams.CHECKSTATUS_ORDERID, RemitaSplitParams.MERCHANTID, orderId, hash);
            NewRemitaResponse response = new NewRemitaResponse();

            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                 client.Headers.Add(HttpRequestHeader.Authorization, "remitaConsumerKey=" + RemitaSplitParams.MERCHANTID + ",remitaConsumerToken=" + hash);

                string responseJson = client.DownloadString(pUrl);

                response = JsonConvert.DeserializeObject<NewRemitaResponse>(responseJson);
                if (response != null)
                {
                    var rrp = new RemitaResponse();
                    rrp.Amount = response.amount;
                    rrp.orderId = response.orderId;
                    rrp.RRR = response.RRR;
                    rrp.status = response.status;
                    rrp.statusMessage = response.statusMessage == null ? response.message : response.statusMessage;
                    rrp.message = response.message;
                    rrp.transactiontime = response.transactiontime;

                    return Ok(rrp);
                }
                else
                {
                    check.Code = (int)ResponseCodes.NotFound;
                    check.Message = "Item does not Exist";

                    return Ok(check);
                    //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    //{
                    //    ReasonPhrase = "Item does not Exist"
                    //});
                }
            }
        }






        /// <summary>
        /// Get List of Receipts within a specified time range
        /// </summary>
        /// <param name="startDate">Start Date to begin search from (mm-dd-yyy format)</param>
        /// <param name="endDate">End Date to end search on (mm-dd-yyy format)</param>
        /// <param name="email"></param>
        /// <param name="apiHash"></param>
        /// <returns></returns>
        [ResponseType(typeof(List<ReceiptModel>))]
        [Route("Receipts/{startDate}/{endDate}/{email}/{apiHash}")]
        public IHttpActionResult GetReceipts(string startDate, string endDate, string email, string apiHash)
        {
            #region
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }
            #endregion
            DateTime sDate;
            if (string.IsNullOrEmpty(startDate))
            {
                sDate = DateTime.Today.AddDays(-30).Date;
            }
            else
            {
                DateTime.TryParse(startDate, out sDate);
                sDate = sDate < DateTime.Parse("01-01-2000") ? DateTime.Today.AddDays(-30).Date : sDate;
            }
            DateTime eDate;
            if (string.IsNullOrEmpty(endDate))
            {
                eDate = DateTime.Now;
            }
            else
            {
                DateTime.TryParse(endDate, out eDate);
                eDate = eDate < DateTime.Parse("01-01-2000 12:00:00 AM") ? DateTime.Now.AddHours(23).AddMinutes(59) : eDate;
            }
            //var sd = string.IsNullOrEmpty(startDate) ?  : 
            //var ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            var license = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();

            var receipts = _vRctRep.FindBy(a => a.Date_Paid >= sDate && a.Date_Paid <= eDate && a.LicenseId == license.Id).ToList();  //This allows only the paid for Applications only to be used
            List<ReceiptModel> reportModel = new List<ReceiptModel>();

            foreach (var receipt in receipts)
            {
                var app = _vAppRep.FindBy(a => a.Id == receipt.ApplicationId).FirstOrDefault();
                //if (app != null)
                //{
                //listOfApplications.Add(app);
                var rm = new ReceiptModel();
                rm.ID = receipt.Id;
                rm.ApplicationID = receipt.ApplicationId;
                rm.ReferenceNo = receipt.ApplicationReference;
                rm.Category = receipt.CategoryName;
                rm.Date = receipt.Date_Paid;
                rm.CompanyName = receipt.CompanyName;
                rm.ReceiptNo = receipt.ReceiptNo; //.Receipt_No;
                try
                {
                    rm.Fee = app != null ? (int)Convert.ToDouble(app.approved_amount) : 0;
                    rm.Charge = app != null ? (int)Convert.ToDouble(app.ServiceCharge) : 0;
                }
                catch (Exception ex)
                {
                    rm.Fee = 0;
                    rm.Charge = 0;
                }

                rm.Amount = receipt.Amount;
                rm.PaymentRef = !string.IsNullOrEmpty(receipt.RRR) ? receipt.RRR : receipt.Payment_Code;

                reportModel.Add(rm);
            }
            return Ok(reportModel);
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="refno"></param>
        /// <param name="em"></param>
        /// <param name="email"></param>
        /// <param name="apiHash"></param>
        /// <param name="Split"></param>
        /// <returns></returns>
        [ResponseType(typeof(StringResponse))]
        [Route("Bank/{refno}/{em}/{email}/{apiHash}")]
        public IHttpActionResult PostBank(string refno, string em, string email, string apiHash, PaymentSplit Split)
        {
            var resp = new CanAccessResponse();
            #region
            if (string.IsNullOrEmpty(email))
            {
                resp.Code = (int)ResponseCodes.BadRequest;
                resp.Message = "App UserName cannot be empty";

                return Ok(resp);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                //{
                //    ReasonPhrase = "App UserName cannot be empty"
                //});
            }
            //check if app is registered
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null)
            {
                resp.Code = (int)ResponseCodes.Forbidden;
                resp.Message = "App has been denied Access, Contact NUPRC Dev";

                return Ok(resp);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                //{
                //    ReasonPhrase = "App has been denied Access, Contact DPR Dev"
                //});
            }
            //check if call is from the app Owner

            //var url = HttpContext.Current.Request.UserHostName;//HttpContext.Current.Request.Url.OriginalString;

            //if (url != app.Url)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}
            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                resp.Code = (int)ResponseCodes.Forbidden;
                resp.Message = "App has been denied Access, Contact NUPRC Dev";

                return Ok(resp);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                //{
                //    ReasonPhrase = "App has been denied Access, Contact DPR Dev"
                //});
            }

            #endregion

            #region logic
            try
            {
                var application = _vAppRep.FindBy(a => a.OrderId == refno).FirstOrDefault();
                if(application != null)
                {
                    UtilityHelper.LogMessage("Application found form Manual Value: " + application.OrderId);
                    var _app = _appRep.FindBy(a => a.Id == application.Id).FirstOrDefault();
                    _app.Status = "Payment Completed";
                    _appRep.Edit(_app);
                    _appRep.Save(em, "");

                    var now = UtilityHelper.CurrentTime;
                    var net = Convert.ToDouble(application.ServiceCharge) + Convert.ToDouble(application.approved_amount);
                    var trans = _payTransRep.FindBy(a => a.Order_Id == application.OrderId).FirstOrDefault();
                    if (trans == null)
                    {
                        UtilityHelper.LogMessage("Transactions not Created b4, creating new one");
                        trans = new Payment_Transaction()
                        {
                            CompanyId = application.CompanyId,
                            CompanyName = application.CompanyName,
                            Completed = true,
                            Order_Id = application.OrderId,
                            PaymentSource = "Bank-Remita",
                            Payment_Reference = "NUPRC-Bank-M",
                            Query_Date = now.AddYears(-10),
                            Reference_Number = application.OrderId,
                            Response_Code = "01",
                            Response_Description = "Payment Completed",
                            RRR = "NUPRC-Bank-M",
                            Transaction_Currency = "566",
                            Transaction_Date = now.ToString(),
                            Type = "Offline",
                            ServiceCharge = Split.ServiceCharge,
                            ServiceTypeId = Split.serviceTypeId,
                            ReturnSuccessUrl = Split.ReturnSuccessUrl,
                            ReturnFailureUrl = Split.ReturnFailureUrl,
                            ReturnBankPaymentUrl = Split.ReturnBankPaymentUrl,
                            Approved_Amount = Split.AmountDue,
                            BankPaymentEndPoint = app.BankPaymentEndPoint,
                            Transaction_Amount = Split.totalAmount,
                            TransactionDate = now
                        };
                        if (Split.ApplicationItems != null && Split.ApplicationItems.Count > 0)
                        {
                            trans.ApplicationItem = new JavaScriptSerializer().Serialize(Split.ApplicationItems);
                        }
                        var rqDocs = new List<int>();
                        string dt = "";
                        if (Split.DocumentTypes != null && Split.DocumentTypes.Count > 0)
                        {
                            foreach (var item in Split.DocumentTypes)
                            {
                                var compDoc = _compDocRep.FindBy(a => a.Archived == false && a.Company_Id == application.CompanyId && a.Document_Type_Id == item).FirstOrDefault();
                                if (compDoc == null)
                                {
                                    rqDocs.Add(item);
                                }
                                dt += item.ToString() + ";";
                            }
                            //ppr.RequiredDocs = rqDocs;
                        }
                        trans.DocumentType = dt;
                        _payTransRep.Add(trans);
                    }
                    else
                    {
                        UtilityHelper.LogMessage("Transaction Found: Updating...");
                        trans.Completed = true;
                        trans.Response_Code = "01";
                        trans.Response_Description = "Payment Completed";
                        //DateTime tt;
                        //if (DateTime.TryParse(response.paymentDate, out tt))
                        //{
                        //    pTrans.TransactionDate = tt;
                        //}
                        //else
                        //    pTrans.TransactionDate = DateTime.Parse(response.transactiontime);

                        _payTransRep.Edit(trans);
                    }
                    _payTransRep.Save(em, "");

                    //_NewRemitaResponse resp;
                    //var comp = _compRep.FindBy(a => a.Id == application.CompanyId).FirstOrDefault();
                    //if (!ProcessPayment(trans, comp.User_Id, out resp))
                    //{
                    //    UtilityHelper.LogMessage("/Payment/Remita" + Environment.NewLine + "Application payment NOT successful");
                    //    throw new ArgumentException("Unable to save Transaction");
                    //}

                    resp.Status = true;
                    resp.Code = (int)ResponseCodes.Success;
                    resp.Message = "ok";

                    return Ok(resp);
                }
                else
                {
                    throw new ArgumentException("Application not Found");
                }
            }
            catch (Exception ex)
            {
                var exx = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message;
                UtilityHelper.LogMessage(exx);
                resp.Code = (int)ResponseCodes.ServerError;
                resp.Message = "Internal Server Error:: " + exx;

                return Ok(resp);
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //{
                //    ReasonPhrase = exx
                //});
            }
            #endregion
        }



        //public static string InnerRRR()
        //{
        //    //generate 12 digit numbers
        //    var bytes = new byte[8];
        //    var rng = RandomNumberGenerator.Create();
        //    rng.GetBytes(bytes);
        //    ulong random = BitConverter.ToUInt64(bytes, 0) % 1000000000000;
        //    var whole = String.Format("{0:D8}", random);
        //    var frt = whole.Substring(0, 1);
        //    var rest = whole.Substring(1);
        //    return ChangeToAlpha(Convert.ToInt16(frt)) + rest;
        //}

        //public static string ChangeToAlpha(int x)
        //{
        //    switch (x)
        //    {
        //        case 1:
        //            return "A";
        //        case 2:
        //            return "B";
        //        case 3:
        //            return "C";
        //        case 4:
        //            return "D";
        //        case 5:
        //            return "E";
        //        case 6:
        //            return "F";
        //        case 7:
        //            return "G";
        //        case 8:
        //            return "H";
        //        case 9:
        //            return "I";
        //        default:
        //            return "J";
        //    }
        //}
    }
}
