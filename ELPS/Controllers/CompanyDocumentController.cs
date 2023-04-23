using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    public class CompanyDocumentController : Controller
    {
        #region Repository
        IAddressRepository _addRep;
        IApplicationRepository _appRep;
        ICompanyRepository _coyRep;
        ICompany_DocumentRepository _compDocRep;
        IDocument_TypeRepository _docTypeRep;
        IPayment_TransactionRepository _payTransRep;
        IvCompanyFileRepository _vCoyFileRep;
        

        CompanyHelper coyHelper;
        #endregion

        public CompanyDocumentController(IAddressRepository addrep, ICompanyRepository coy, IvCompanyFileRepository vCoyFile,
            ICompany_DocumentRepository compDocRep, IPayment_TransactionRepository payTransRep, IApplicationRepository appRep,
            IDocument_TypeRepository docTypeRep)
        {
            _docTypeRep = docTypeRep;
            _appRep = appRep;
            _payTransRep = payTransRep;
            _compDocRep = compDocRep;
            _vCoyFileRep = vCoyFile;
            _coyRep = coy;
            _addRep = addrep;

            coyHelper = new CompanyHelper(coy, appRep);
        }

        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                var myCoy = coyHelper.MyCompany(User.Identity.Name);

                var compDocs = _vCoyFileRep.FindBy(a => a.CompanyId == myCoy.Id && !a.Archived).ToList(); 
                // _compDocRep.FindBy(a => a.Company_Id == myCoy.Id).ToList();
                return View(compDocs);
            }
            else
            {
                var myCoy = _coyRep.FindBy(a => a.Id == id).FirstOrDefault();
                if (myCoy != null)
                {
                    var compDocs = _vCoyFileRep.FindBy(a => a.CompanyId == myCoy.Id && !a.Archived).ToList();
                    if (compDocs.Any())
                    {
                        foreach (var doc in compDocs)
                        {
                            doc.source = ConfigurationManager.AppSettings["myBaseUrl"] + doc.source.Replace("~", "");
                        }
                    }
                    return View("DocumentList", compDocs);
                }
            }
            return View("Error");
        }

        public FileResult DisplayPDFDocument(int docId, string docUrl)
        {
            if (!string.IsNullOrEmpty(docUrl))
            {
                return File(docUrl, "application/pdf");
            }
            else
            {
                var compDoc = _vCoyFileRep.FindBy(a => a.Id == docId).FirstOrDefault();
                if (compDoc != null && compDoc.source.ToLower().EndsWith(".pdf"))
                {
                    return File(compDoc.source, "application/pdf");
                }
                return null;
            }
        }

        #region Upload Area

        /// <summary>
        /// Upload Company document
        /// </summary>
        /// <param name="id">Application Reference</param>
        /// <returns></returns>
        public ActionResult UploadDocument(string id)
        {
            ViewBag.appId = id;

            var app = _appRep.FindBy(a => a.OrderId.Trim() == id.Trim()).FirstOrDefault();

            var compDoc = _compDocRep.FindBy(a => a.Company_Id == app.CompanyId && a.Status).ToList();

            #region Load required docs from Pay_Trans => DocumentType string and convert as needed
            var reqd = _payTransRep.FindBy(a => a.Order_Id.Trim() == app.OrderId.Trim()).FirstOrDefault().DocumentType;
            var reqDocs = reqd.Split(';');

            var appRequiredDocs = new List<Document_Type>();
            var allDoctTypes = _docTypeRep.GetAll().ToList();
            foreach (var d in reqDocs)
            {
                var did = Convert.ToInt16(d);
                appRequiredDocs.Add(allDoctTypes.Where(a => a.Id == did).FirstOrDefault());
            }
            #endregion
            
            ViewBag.RequiredDocs = appRequiredDocs;

            return View();
        }

        #endregion
    }
}
