using ELPS.Domain.Entities;
using ELPS.Helpers;
using ELPS.Models;
using ELPS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNetCore.Cors;

namespace ELPS.Controllers
{
    [Authorize]
    [EnableCors("AllowFromAll")]
    public class CompanyController : Controller
    {
        #region Repository
        IAddressRepository _addRep;
        ICompanyRepository _compRep;
        ICountryRepository _countryRep;
        ICompany_Expatriate_QuotaRepository _expQuotaRep;
        ICompany_NsitfRepository _nsitfRep;
        ICompany_MedicalRepository _medRep;
        ICompany_ProffessionalRepository _profRep;
        ICompany_Technical_AgreementRepository _techAgreeRep;
        ICompany_DirectorRepository _compDirRep;
        ICompany_Key_StaffRepository _compKSRep;
        IKey_Staff_CertificateRepository _ksCertRep;
        IStateRepository _stateRep;
        IvAddressRepository _vAddRep;
        IvCompanyNsitfRepository _vCompNsitifRep;
        IvCompanyMedicalRepository _vCompMedRep;
        IvCompanyProffessionalRepository _vCompProfRep;
        IvCompanyExpatriateQuotaRepository _vCompExpQRep;
        IvCompanyTechnicalAgreementRepository _vCompTechARep;
        IvCompanyDirectorRepository _vCompDirRep;
        IFileRepository _fileRep;
        IDocument_TypeRepository _docRep;
        IApplicationRepository _appRep;
        IAppIdentityRepository _appIdRep;

        CompanyHelper coyHelper;
        #endregion

        public CompanyController(ICompanyRepository compRep, ICountryRepository countryRep, IvCompanyNsitfRepository vCompNsitifRep,
            IvCompanyMedicalRepository vCompMedRep, IvCompanyProffessionalRepository vCompProfRep, IvCompanyExpatriateQuotaRepository vCompExpQRep,
            IvCompanyTechnicalAgreementRepository vCompTechARep, IvCompanyDirectorRepository vCompDir, IFileRepository fileRep,
            IStateRepository state, IvAddressRepository vAdd, ICompany_Key_StaffRepository compKSRep, IAddressRepository addRep,
            ICompany_DirectorRepository compDir, IKey_Staff_CertificateRepository ksCertRep, ICompany_Expatriate_QuotaRepository expQuotaRep,
            ICompany_NsitfRepository nsitfRep, ICompany_MedicalRepository medRep, ICompany_ProffessionalRepository
            profRep, ICompany_Technical_AgreementRepository techAgreeRep, IDocument_TypeRepository docs,
            IApplicationRepository apprep, IAppIdentityRepository appIdRep)
        {
            _appIdRep = appIdRep;
            _appRep = apprep;
            _docRep = docs;
            _expQuotaRep = expQuotaRep;
            _nsitfRep = nsitfRep;
            _medRep = medRep;
            _profRep = profRep;
            _techAgreeRep = techAgreeRep;
            _ksCertRep = ksCertRep;
            _compDirRep = compDir;
            _addRep = addRep;
            _compKSRep = compKSRep;
            _vAddRep = vAdd;
            _stateRep = state;
            _compRep = compRep;
            _countryRep = countryRep;
            _vCompNsitifRep = vCompNsitifRep;
            _vCompMedRep = vCompMedRep;
            _vCompProfRep = vCompProfRep;
            _vCompExpQRep = vCompExpQRep;
            _vCompTechARep = vCompTechARep;
            _vCompDirRep = vCompDir;
            _fileRep = fileRep;

            coyHelper = new CompanyHelper(compRep, apprep);
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            var coy = coyHelper.MyCompany(User.Identity.Name.ToLower());
            if (coy == null)
                return View("Error");
            else
                return View(coy);
        }

        public ActionResult Update(string continueurl, string hide)
        {
            if (!string.IsNullOrEmpty(hide) && hide.ToLower() == "true")
            {
                TempData["Hide-Bar"] = "true";
            }
            var coy = coyHelper.MyCompany(User.Identity.Name.ToLower());
            if (coy == null)
                return View("Error");

            TempData["ELPS-ContinueUrl"] = continueurl;
            ViewBag.compId = coy.Id;
            ViewBag.Stage = 0;

            return View(coy);
        }

        public ActionResult GetState(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var Id = Convert.ToInt32(id);
                var st = _stateRep.FindBy(a => a.CountryId == Id).OrderBy(c => c.Name).ToList();
                return Json(st, JsonRequestBehavior.AllowGet);
            }
            return Content("country Id is required");
        }


        #region Company Detail
        /// <summary>
        /// Called be external portal
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [EnableCors("AllowFromAll")]
        public ActionResult CompanyDetails(int id, string email, string code)
        {
            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                return View("Error");
                //return Ok(new CanAccessResponse() { Code = 2, Status = false, Message = "Access denied: Invalid App Username. Contact DPR Development team" });
            }
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            if (app == null) // || !app.IsActive)
            {
                return View("Error");
                //return Ok(new CanAccessResponse() { Code = 3, Status = false, Message = "App has been denied Access, Contact DPR Development team" });
            }

            if (!HashManager.compair(email, app.AppId, code))
            {
                return View("Error");
                //return Ok(new CanAccessResponse() { Code = 4, Status = false, Message = "App has been denied Access, Contact DPR Development team" });
            }
            #endregion

            var comp = _compRep.FindBy(a => a.Id == id).FirstOrDefault();

            if (comp == null)
            {
                return View("Error");
            }
            var companyVM = new CompanyViewModel();

            List<Country> allCountry = _countryRep.GetAll().OrderBy(c => c.Name).ToList();
            Country ctry = new Country();
            if (comp != null && !string.IsNullOrEmpty(comp.Nationality))
                ctry = allCountry.Find(C => C.Name.ToLower() == comp.Nationality.ToLower());
            ViewBag.country = allCountry;
            ViewBag.Nationality = ctry;// new SelectList(allCountry, "Id", "Name", ctry);

            companyVM.CompanyView = comp;

            #region Company_Nsitf View
            var nts = new List<vCompanyNsitf>();
            var Ntifs = _vCompNsitifRep.FindBy(a => a.Company_Id == comp.Id).ToList();
            if (Ntifs.Count > 0)
            {
                foreach (var item in Ntifs)
                {
                    var nf = new vCompanyNsitf();
                    nf.Id = item.Id;
                    nf.No_People_Covered = item.No_People_Covered;
                    nf.Policy_No = item.Policy_No;
                    nf.Date_Issued = item.Date_Issued;
                    nf.FileId = item.FileId;
                    nf.FileName = item.FileName;
                    nf.FileSource = item.FileSource;
                    nts.Add(nf);
                }
            }
            else
            {
                //if (update)
                //{
                //    var nf = new vCompanyNsitf();
                //    nf.Date_Issued = DateTime.Now;
                //    nts.Add(nf);
                //}
            }
            companyVM.CoyNsitfView = nts;
            #endregion

            #region company medicals
            var cms = new List<vCompanyMedical>();
            var cm = _vCompMedRep.FindBy(a => a.Company_Id == comp.Id).ToList();
            if (cm.Count > 0)
            {
                foreach (var item in cm)
                {
                    var cmv = new vCompanyMedical();
                    cmv.Id = item.Id;
                    cmv.Address = item.Address;
                    cmv.Date_Issued = item.Date_Issued;
                    cmv.Email = item.Email;
                    cmv.Medical_Organisation = item.Medical_Organisation;
                    cmv.Phone = item.Phone;
                    cmv.FileId = item.FileId;
                    cmv.FileName = item.FileName;
                    cmv.FileSource = item.FileSource;

                    cms.Add(cmv);
                }
            }
            else
            {
                //if (update)
                //{
                //    var cmv = new vCompanyMedical() { Date_Issued = DateTime.Now };
                //    cms.Add(cmv);
                //}
            }
            companyVM.CoyMedView = cms;
            #endregion

            #region Proffessional Organisation
            var cps = new List<vCompanyProffessional>();
            var cpo = _vCompProfRep.FindBy(a => a.Company_Id == comp.Id).ToList();
            if (cpo.Count > 0)
            {
                foreach (var item in cpo)
                {
                    var cp = new vCompanyProffessional();
                    cp.Id = item.Id;
                    cp.Cert_No = item.Cert_No;
                    cp.Date_Issued = item.Date_Issued;
                    cp.Proffessional_Organisation = item.Proffessional_Organisation;
                    cp.FileId = item.FileId;
                    cp.FileName = item.FileName;
                    cp.FileSource = item.FileSource;
                    cps.Add(cp);
                }
            }
            else
            {
                //if (update)
                //{
                //    var cp = new vCompanyProffessional() { Date_Issued = DateTime.Now };
                //    cps.Add(cp);
                //}
            }
            companyVM.CoyProfView = cps;
            #endregion

            #region expatrait quota
            var cxs = new List<vCompanyExpatriateQuota>();
            var cxo = _vCompExpQRep.FindBy(a => a.Company_Id == comp.Id).ToList();
            if (cxo.Count > 0)
            {
                foreach (var item in cxo)
                {
                    var cx = new vCompanyExpatriateQuota();
                    cx.Id = item.Id;
                    cx.Name = item.Name;
                    cx.FileId = item.FileId;
                    cx.FileName = item.FileName;
                    var src = _fileRep.FindBy(a => a.Id == item.FileId).FirstOrDefault();
                    cx.FileSource = src != null ? src.Source : "";
                    cxs.Add(cx);
                }
            }
            else
            {
                //if (update)
                //{
                //    var cx = new vCompanyExpatriateQuota() { Id = 0 };
                //    cxs.Add(cx);
                //}
            }
            companyVM.CoyExpQView = cxs;

            #endregion

            #region Technical Agreement
            var ctas = new List<vCompanyTechnicalAgreement>();
            var cta = _vCompTechARep.FindBy(a => a.Company_Id == comp.Id).ToList();
            if (cta.Count > 0)
            {
                foreach (var item in cta)
                {
                    var ct = new vCompanyTechnicalAgreement();
                    ct.Id = item.Id;
                    ct.Name = item.Name;
                    ct.FileId = item.FileId;
                    ct.FileName = item.FileName;
                    var src = _fileRep.FindBy(a => a.Id == item.FileId).FirstOrDefault();
                    ct.FileSource = src != null ? src.Source : "";
                    ctas.Add(ct);
                }
            }
            else
            {
                //if (update)
                //{
                //    var ct = new vCompanyTechnicalAgreement() { Id = 0 };
                //    ctas.Add(ct);
                //}
            }
            companyVM.CoyTechAgreeView = ctas;
            #endregion

            if (string.IsNullOrEmpty(comp.Contact_FirstName) || string.IsNullOrEmpty(comp.Contact_LastName) || string.IsNullOrEmpty(comp.Mission_Vision))
            {
                // This means Company profile has not been filled
                ViewBag.DetailComplete = false;
            }
            else
            {
                //Completed
                ViewBag.DetailComplete = true;
            }
            //if (update)
            //{
            //    ViewBag.View = view;
            //    return View("UpdateDetail", companyVM);
            //}
            //else
            //    return View(companyVM);


            return View(companyVM);
            //return View("Error");
        }


        public ActionResult CompanyDetail(int id, string view)
        {
            bool update = !string.IsNullOrEmpty(view) && view.ToLower().Trim() == "update" ? true : false;

            var comp = _compRep.FindBy(a => a.Id == id).FirstOrDefault();

            if (comp != null)
            {
                var companyVM = new CompanyViewModel();

                List<Country> allCountry = _countryRep.GetAll().OrderBy(c => c.Name).ToList();
                Country ctry = new Country();
                if (comp != null && !string.IsNullOrEmpty(comp.Nationality))
                    ctry = allCountry.Find(C => C.Name.ToLower() == comp.Nationality.ToLower());
                ViewBag.country = allCountry;
                ViewBag.Nationality = ctry;// new SelectList(allCountry, "Id", "Name", ctry);

                companyVM.CompanyView = comp;

                #region Company_Nsitf View
                var nts = new List<vCompanyNsitf>();
                var Ntifs = _vCompNsitifRep.FindBy(a => a.Company_Id == comp.Id).ToList();
                if (Ntifs.Count > 0)
                {
                    foreach (var item in Ntifs)
                    {
                        var nf = new vCompanyNsitf();
                        nf.Id = item.Id;
                        nf.No_People_Covered = item.No_People_Covered;
                        nf.Policy_No = item.Policy_No;
                        nf.Date_Issued = item.Date_Issued;
                        nf.FileId = item.FileId;
                        nf.FileName = item.FileName;
                        nf.FileSource = item.FileSource;
                        nts.Add(nf);
                    }
                }
                else
                {
                    if (update)
                    {
                        var nf = new vCompanyNsitf();
                        nf.Date_Issued = DateTime.Now;
                        nts.Add(nf);
                    }
                }
                companyVM.CoyNsitfView = nts;
                #endregion

                #region company medicals
                var cms = new List<vCompanyMedical>();
                var cm = _vCompMedRep.FindBy(a => a.Company_Id == comp.Id).ToList();
                if (cm.Count > 0)
                {
                    foreach (var item in cm)
                    {
                        var cmv = new vCompanyMedical();
                        cmv.Id = item.Id;
                        cmv.Address = item.Address;
                        cmv.Date_Issued = item.Date_Issued;
                        cmv.Email = item.Email;
                        cmv.Medical_Organisation = item.Medical_Organisation;
                        cmv.Phone = item.Phone;
                        cmv.FileId = item.FileId;
                        cmv.FileName = item.FileName;
                        cmv.FileSource = item.FileSource;

                        cms.Add(cmv);
                    }
                }
                else
                {
                    if (update)
                    {
                        var cmv = new vCompanyMedical() { Date_Issued = DateTime.Now };
                        cms.Add(cmv);
                    }
                }
                companyVM.CoyMedView = cms;
                #endregion

                #region Proffessional Organisation
                var cps = new List<vCompanyProffessional>();
                var cpo = _vCompProfRep.FindBy(a => a.Company_Id == comp.Id).ToList();
                if (cpo.Count > 0)
                {
                    foreach (var item in cpo)
                    {
                        var cp = new vCompanyProffessional();
                        cp.Id = item.Id;
                        cp.Cert_No = item.Cert_No;
                        cp.Date_Issued = item.Date_Issued;
                        cp.Proffessional_Organisation = item.Proffessional_Organisation;
                        cp.FileId = item.FileId;
                        cp.FileName = item.FileName;
                        cp.FileSource = item.FileSource;
                        cps.Add(cp);
                    }
                }
                else
                {
                    if (update)
                    {
                        var cp = new vCompanyProffessional() { Date_Issued = DateTime.Now };
                        cps.Add(cp);
                    }
                }
                companyVM.CoyProfView = cps;
                #endregion

                #region expatrait quota
                var cxs = new List<vCompanyExpatriateQuota>();
                var cxo = _vCompExpQRep.FindBy(a => a.Company_Id == comp.Id).ToList();
                if (cxo.Count > 0)
                {
                    foreach (var item in cxo)
                    {
                        var cx = new vCompanyExpatriateQuota();
                        cx.Id = item.Id;
                        cx.Name = item.Name;
                        cx.FileId = item.FileId;
                        cx.FileName = item.FileName;
                        var src = _fileRep.FindBy(a => a.Id == item.FileId).FirstOrDefault();
                        cx.FileSource = src != null ? src.Source : "";
                        cxs.Add(cx);
                    }
                }
                else
                {
                    if (update)
                    {
                        var cx = new vCompanyExpatriateQuota() { Id = 0 };
                        cxs.Add(cx);
                    }
                }
                companyVM.CoyExpQView = cxs;

                #endregion

                #region Technical Agreement
                var ctas = new List<vCompanyTechnicalAgreement>();
                var cta = _vCompTechARep.FindBy(a => a.Company_Id == comp.Id).ToList();
                if (cta.Count > 0)
                {
                    foreach (var item in cta)
                    {
                        var ct = new vCompanyTechnicalAgreement();
                        ct.Id = item.Id;
                        ct.Name = item.Name;
                        ct.FileId = item.FileId;
                        ct.FileName = item.FileName;
                        var src = _fileRep.FindBy(a => a.Id == item.FileId).FirstOrDefault();
                        ct.FileSource = src != null ? src.Source : "";
                        ctas.Add(ct);
                    }
                }
                else
                {
                    if (update)
                    {
                        var ct = new vCompanyTechnicalAgreement() { Id = 0 };
                        ctas.Add(ct);
                    }
                }
                companyVM.CoyTechAgreeView = ctas;
                #endregion

                if (string.IsNullOrEmpty(comp.Contact_FirstName) || string.IsNullOrEmpty(comp.Contact_LastName) || string.IsNullOrEmpty(comp.Mission_Vision))
                {
                    // This means Company profile has not been filled
                    ViewBag.DetailComplete = false;
                }
                else
                {
                    //Completed
                    ViewBag.DetailComplete = true;
                }
                if (update)
                {
                    ViewBag.View = view;
                    return View("UpdateDetail", companyVM);
                }
                else
                    return View(companyVM);
            }
            return View("Error");
        }

        [HttpPost]
        public ActionResult CompanyDetailUpdate(Company CompanyView, List<vCompanyNsitf> ntifs, List<vCompanyMedical> compMed,
            List<vCompanyProffessional> compProf, List<vCompanyExpatriateQuota> compExpert, List<vCompanyTechnicalAgreement> compTech)
        {
            using (var trans = new TransactionScope())
            {
                var conturl = TempData["ELPS-ContinueUrl"] != null ? TempData["ELPS-ContinueUrl"].ToString() : "";

                try
                {
                    var cp = _compRep.FindBy(a => a.Id == CompanyView.Id).FirstOrDefault();
                    if (cp == null)
                        throw new ArgumentException("Company not found");

                    #region update company
                    cp.Accident = CompanyView.Accident;
                    cp.Accident_Report = CompanyView.Accident_Report;
                    cp.Affiliate = CompanyView.Affiliate;
                    cp.Contact_FirstName = CompanyView.Contact_FirstName;
                    cp.Contact_LastName = CompanyView.Contact_LastName;
                    cp.Contact_Phone = CompanyView.Contact_Phone;
                    cp.Hse = CompanyView.Hse;
                    cp.HSEDoc = CompanyView.HSEDoc;
                    cp.IsCompleted = CompanyView.IsCompleted;
                    cp.Mission_Vision = CompanyView.Mission_Vision;
                    cp.Nationality = CompanyView.Nationality;
                    cp.No_Expatriate = CompanyView.No_Expatriate;
                    cp.No_Staff = CompanyView.No_Staff;
                    cp.Operational_Address_Id = CompanyView.Operational_Address_Id;
                    cp.Registered_Address_Id = CompanyView.Registered_Address_Id;
                    cp.Tin_Number = CompanyView.Tin_Number;
                    cp.Total_Asset = CompanyView.Total_Asset;
                    cp.Training_Program = CompanyView.Training_Program;
                    cp.Year_Incorporated = CompanyView.Year_Incorporated;
                    cp.Yearly_Revenue = CompanyView.Yearly_Revenue;

                    _compRep.Edit(cp);
                    _compRep.Save(cp.Name, Request.UserHostAddress);
                    #endregion

                    #region NSITF
                    if (ntifs != null && ntifs.Count > 0)
                    {
                        //var nsts = new List<Company_Nsitf>();
                        foreach (var item in ntifs)
                        {
                            if (item.Elps_Id > 0)
                            {
                                var nt = _nsitfRep.FindBy(a => a.Id == item.Elps_Id).FirstOrDefault();
                                if (nt != null)
                                {
                                    nt.Date_Issued = item.Date_Issued;
                                    nt.FileId = item.FileId;
                                    nt.No_People_Covered = item.No_People_Covered;
                                    nt.Policy_No = item.Policy_No;
                                    _nsitfRep.Edit(nt);
                                    _nsitfRep.Save(cp.Name, Request.UserHostAddress);
                                }
                                else
                                {
                                    nt = new Company_Nsitf();
                                    nt.Date_Issued = item.Date_Issued;
                                    nt.Company_Id = CompanyView.Id;
                                    nt.FileId = item.FileId;
                                    nt.No_People_Covered = item.No_People_Covered;
                                    nt.Policy_No = item.Policy_No;

                                    _nsitfRep.Add(nt);
                                    _nsitfRep.Save(cp.Name, Request.UserHostAddress);
                                }
                            }
                            else
                            {
                                var nt = new Company_Nsitf();
                                nt.Date_Issued = item.Date_Issued;
                                nt.Company_Id = CompanyView.Id;
                                nt.FileId = item.FileId;
                                nt.No_People_Covered = item.No_People_Covered;
                                nt.Policy_No = item.Policy_No;

                                _nsitfRep.Add(nt);
                                _nsitfRep.Save(cp.Name, Request.UserHostAddress);
                            }
                        }
                    }
                    #endregion

                    #region Medicals
                    if (compMed != null && compMed.Count > 0)
                    {
                        foreach (var item in compMed)
                        {
                            if (item.Elps_Id > 0)
                            {
                                var cm = _medRep.FindBy(a => a.Id == item.Elps_Id).FirstOrDefault();
                                if (cm != null)
                                {
                                    cm.Address = item.Address;
                                    cm.Date_Issued = item.Date_Issued;
                                    cm.Email = item.Email;
                                    cm.FileId = item.FileId;
                                    cm.Medical_Organisation = item.Medical_Organisation;
                                    cm.Phone = item.Phone;

                                    _medRep.Edit(cm);
                                    _medRep.Save(cp.Name, Request.UserHostAddress);
                                }
                                else
                                {
                                    cm = new Company_Medical();
                                    cm.Address = item.Address;
                                    cm.Date_Issued = item.Date_Issued;
                                    cm.Email = item.Email;
                                    cm.FileId = item.FileId;
                                    cm.Medical_Organisation = item.Medical_Organisation;
                                    cm.Phone = item.Phone;
                                    cm.Company_Id = CompanyView.Id;

                                    _medRep.Add(cm);
                                    _medRep.Save(cp.Name, Request.UserHostAddress);
                                }
                            }
                            else
                            {
                                var cm = new Company_Medical();
                                cm.Address = item.Address;
                                cm.Date_Issued = item.Date_Issued;
                                cm.Email = item.Email;
                                cm.FileId = item.FileId;
                                cm.Medical_Organisation = item.Medical_Organisation;
                                cm.Phone = item.Phone;
                                cm.Company_Id = CompanyView.Id;

                                _medRep.Add(cm);
                                _medRep.Save(cp.Name, Request.UserHostAddress);

                            }
                        }
                    }
                    #endregion

                    #region Professionals
                    if (compProf != null && compProf.Count > 0)
                    {
                        foreach (var item in compProf)
                        {
                            if (item.Elps_Id > 0)
                            {

                                var pf = _profRep.FindBy(a => a.Id == item.Elps_Id).FirstOrDefault();
                                if (pf != null)
                                {
                                    pf.Cert_No = item.Cert_No;
                                    pf.Date_Issued = item.Date_Issued;
                                    pf.FileId = item.FileId;
                                    pf.Proffessional_Organisation = item.Proffessional_Organisation;
                                    _profRep.Edit(pf);
                                    _profRep.Save(cp.Name, Request.UserHostAddress);
                                }
                                else
                                {
                                    pf = new Company_Proffessional();
                                    pf.Company_Id = CompanyView.Id;
                                    pf.Cert_No = item.Cert_No;
                                    pf.Date_Issued = item.Date_Issued;
                                    pf.FileId = item.FileId;
                                    pf.Proffessional_Organisation = item.Proffessional_Organisation;
                                    _profRep.Add(pf);
                                    _profRep.Save(cp.Name, Request.UserHostAddress);
                                }

                            }
                            else
                            {
                                var pf = new Company_Proffessional();
                                pf.Company_Id = CompanyView.Id;
                                pf.Cert_No = item.Cert_No;
                                pf.Date_Issued = item.Date_Issued;
                                pf.FileId = item.FileId;
                                pf.Proffessional_Organisation = item.Proffessional_Organisation;
                                _profRep.Add(pf);
                                _profRep.Save(cp.Name, Request.UserHostAddress);
                            }

                        }
                    }
                    #endregion

                    #region Technical
                    if (compTech != null && compTech.Count > 0)
                    {
                        foreach (var item in compTech)
                        {
                            if (item.Elps_Id > 0)
                            {
                                var ta = _techAgreeRep.FindBy(a => a.Id == item.Elps_Id).FirstOrDefault();
                                if (ta != null)
                                {
                                    ta.FileId = item.FileId;
                                    ta.Name = item.Name;

                                    _techAgreeRep.Edit(ta);
                                    _techAgreeRep.Save(cp.Name, Request.UserHostAddress);

                                }
                                else
                                {
                                    ta = new Company_Technical_Agreement();
                                    ta.FileId = item.FileId;
                                    ta.Name = item.Name;
                                    ta.Company_Id = CompanyView.Id;
                                    _techAgreeRep.Add(ta);
                                    _techAgreeRep.Save(cp.Name, Request.UserHostAddress);
                                }
                            }
                            else
                            {
                                var ta = new Company_Technical_Agreement();
                                ta.FileId = item.FileId;
                                ta.Name = item.Name;
                                ta.Company_Id = CompanyView.Id;
                                _techAgreeRep.Add(ta);
                                _techAgreeRep.Save(cp.Name, Request.UserHostAddress);
                            }
                        }

                    }
                    #endregion

                    #region Expertrate
                    if (compExpert != null && compExpert.Count > 0)
                    {
                        foreach (var item in compExpert)
                        {
                            if (item.Elps_Id > 0)
                            {
                                var eq = _expQuotaRep.FindBy(a => a.Id == item.Elps_Id).FirstOrDefault();
                                if (eq != null)
                                {
                                    eq.FileId = item.FileId;
                                    eq.Name = item.Name;
                                    _expQuotaRep.Edit(eq);
                                    _expQuotaRep.Save(cp.Name, Request.UserHostAddress);

                                }
                                else
                                {
                                    eq = new Company_Expatriate_Quota();
                                    eq.Company_Id = CompanyView.Id;
                                    eq.FileId = item.FileId;
                                    eq.Name = item.Name;
                                    _expQuotaRep.Add(eq);
                                    _expQuotaRep.Save(cp.Name, Request.UserHostAddress);
                                }
                            }
                            else
                            {
                                var eq = new Company_Expatriate_Quota();
                                eq.Company_Id = CompanyView.Id;
                                eq.FileId = item.FileId;
                                eq.Name = item.Name;
                                _expQuotaRep.Add(eq);
                                _expQuotaRep.Save(cp.Name, Request.UserHostAddress);
                            }
                        }
                    }
                    #endregion
                    //return CreatedAtRoute("DefaultApi", new { id = comp.Id }, RComp);


                    trans.Complete();
                    TempData["status"] = "pass";
                    TempData["message"] = "Company Address updated successfully";
                    return RedirectToAction("Update", new { continueurl = conturl });
                }
                catch (Exception ex)
                {
                    TempData["status"] = "fail";
                    TempData["message"] = "Sorry there was an error while handling your request";

                    return RedirectToAction("Update", new { continueurl = conturl });
                }
            }
        }


        //[HttpPost]
        //public ActionResult CompanyDetail(Company model, string EditEmail)
        //{
        //    var comp = _compRep.FindBy(a => a.Id == model.Id).FirstOrDefault();
        //    if (comp != null)
        //    {
        //        comp.Name = model.Name;
        //        comp.RC_Number = model.RC_Number;
        //        comp.Business_Type = model.Business_Type;
        //        using (var trans = new TransactionScope())
        //        {
        //            try
        //            {

        //                if (!string.IsNullOrEmpty(EditEmail))
        //                {
        //                    if (model.User_Id.ToLower() != EditEmail.ToLower())
        //                    {
        //                        var user = UserManager.FindByEmail(model.User_Id);
        //                        if (user != null)
        //                        {
        //                            //update user
        //                            user.Email = EditEmail;
        //                            user.UserName = EditEmail;
        //                            user.EmailConfirmed = false;

        //                            UserManager.Update(user);
        //                            var result = UserManager.AddPassword(user.Id, EditEmail + 1);
        //                        }
        //                        else
        //                        {
        //                            //create new user
        //                            #region create new user
        //                            user = new ApplicationUser { UserName = EditEmail, Email = EditEmail, PhoneNumber = comp.Contact_Phone, PhoneNumberConfirmed = true };
        //                            var result = UserManager.Create(user, EditEmail + 1);
        //                            if (result.Succeeded)
        //                            {
        //                                var x = UserManager.AddToRole(user.Id, "Company");
        //                            }
        //                            #endregion
        //                        }

        //                        #region send Re-Activation Link

        //                        //send activation link

        //                        var code = UserManager.GenerateEmailConfirmationToken(user.Id);
        //                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //                        var body = "";
        //                        //Read template file from the App_Data folder
        //                        using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
        //                        {
        //                            body = sr.ReadToEnd();
        //                        }
        //                        var msgBody = string.Format(body, model.Name, callbackUrl, "Confirm Your OGISP Account");
        //                        //var body = MailHelper.getMailBody(callbackUrl, model.Email);
        //                        UserManager.SendEmail(user.Id, "Confirm your account", msgBody);

        //                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

        //                        var msg = new OGIS.Domain.Entities.Message();
        //                        msg.Company_Id = comp.Id;
        //                        msg.Content = msgBody;
        //                        msg.Date = UtilityHelper.CurrentTime;
        //                        msg.Read = 0;
        //                        msg.Subject = "Confirm your account";
        //                        msg.Sender_Id = "Application";

        //                        _msgRep.Add(msg);
        //                        _msgRep.Save(User.Identity.Name, Request.UserHostAddress);
        //                        #endregion

        //                        comp.User_Id = EditEmail;

        //                    }
        //                }

        //                _compRep.Edit(comp);
        //                _compRep.Save(User.Identity.Name, Request.UserHostAddress);
        //                trans.Complete();
        //            }
        //            catch (DbEntityValidationException x)
        //            {
        //                // Elmah.ErrorSignal.FromCurrentContext().Raise(x);
        //                foreach (var validationErrors in x.EntityValidationErrors)
        //                {
        //                    foreach (var validationError in validationErrors.ValidationErrors)
        //                    {
        //                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
        //                    }
        //                }
        //                trans.Dispose();

        //                //throw;
        //            }
        //        }
        //        return RedirectToAction("Detail", new { id = model.Id });
        //    }
        //    return View("Error");
        //}
        #endregion

        #region Company Address
        public ActionResult CompanyAddresses(int id, string view)
        {
            ViewBag.CompanyId = id;
            var comp = _compRep.FindBy(a => a.Id == id).FirstOrDefault();
            if (comp != null)
            {
                var regAdd = new vAddress();
                var opAdd = new vAddress();
                int NigId;
                var Nig = _countryRep.FindBy(a => a.Name.ToLower() == "nigeria").FirstOrDefault();

                if (Nig != null)
                {
                    NigId = Nig.Id;
                    regAdd.Country_Id = Nig.Id;
                    regAdd.CountryName = Nig.Name;

                    opAdd.Country_Id = Nig.Id;
                    opAdd.CountryName = Nig.Name;
                    ViewBag.state = _stateRep.FindBy(a => a.CountryId == NigId).OrderBy(c => c.Name).ToList();
                }
                if (comp.Registered_Address_Id.GetValueOrDefault() > 0)
                {
                    regAdd = _vAddRep.FindBy(a => a.Id == comp.Registered_Address_Id.Value).FirstOrDefault();
                }
                if (comp.Registered_Address_Id.GetValueOrDefault() != comp.Operational_Address_Id.GetValueOrDefault() && comp.Operational_Address_Id.GetValueOrDefault() > 0)
                {
                    opAdd = _vAddRep.FindBy(a => a.Id == comp.Operational_Address_Id.Value).FirstOrDefault();
                }
                if (comp.Registered_Address_Id == comp.Operational_Address_Id)
                {
                    ViewBag.SameAdd = true;
                }

                ViewBag.opAdd = opAdd;
                if (regAdd == null || (string.IsNullOrEmpty(regAdd.address_1) || regAdd.StateId <= 0))
                {
                    // No Address had been registered to this company OR The address is not properly completed
                    ViewBag.AddressComplete = false;
                }
                else
                {
                    //Completed
                    ViewBag.AddressComplete = true;
                }

                ViewBag.country = _countryRep.GetAll().OrderBy(c => c.Name).ToList();
                if (!string.IsNullOrEmpty(view) && view.ToLower().Trim() == "update")
                {
                    ViewBag.View = view;
                    return View("UpdateAddress", regAdd);
                }
                else
                    return View(regAdd);
            }
            return View("Error");
        }

        [HttpPost]
        public ActionResult CompanyAddressUpdate(List<vAddress> model, string DiffOpAdd)
        {
            var usr = User.Identity.Name;
            var conturl = TempData["ELPS-ContinueUrl"] != null ? TempData["ELPS-ContinueUrl"].ToString() : "";
            //bool result = UpdateCompanyAddress(usr, model, DiffOpAdd);

            var comp = _compRep.FindBy(a => a.User_Id == usr).FirstOrDefault();
            if (model != null && model.Count > 0)
            {
                using (var trans = new TransactionScope())
                {
                    var regAd = model[0];
                    try
                    {
                        #region Save Registered Address to local

                        var ra = new Address();
                        if (regAd.Id > 0)
                        {
                            ra = _addRep.FindBy(a => a.Id == regAd.Id).FirstOrDefault();
                            if (ra != null)
                            {
                                ra.Address_1 = regAd.address_1;
                                ra.Address_2 = regAd.address_2;
                                ra.City = regAd.City;
                                ra.Country_Id = regAd.Country_Id;
                                ra.StateId = regAd.StateId;
                                ra.Postal_Code = regAd.postal_code;
                                _addRep.Edit(ra);
                            }
                            else
                            {
                                ra = new Address();
                                ra.Address_1 = regAd.address_1;
                                ra.Address_2 = regAd.address_2;
                                ra.City = regAd.City;
                                ra.Country_Id = regAd.Country_Id;
                                ra.StateId = regAd.StateId;
                                ra.Postal_Code = regAd.postal_code;
                                _addRep.Add(ra);
                            }
                        }
                        else
                        {
                            ra.Address_1 = regAd.address_1;
                            ra.Address_2 = regAd.address_2;
                            ra.City = regAd.City;
                            ra.Country_Id = regAd.Country_Id;
                            ra.StateId = regAd.StateId;
                            ra.Postal_Code = regAd.postal_code;
                            _addRep.Add(ra);
                        }
                        _addRep.Save(User.Identity.Name, Request.UserHostAddress);
                        #endregion

                        comp.Registered_Address_Id = ra.Id;
                        comp.Operational_Address_Id = ra.Id;

                        #region Save Operational Address if diffrent to Reg Add to Local
                        if (!string.IsNullOrEmpty(DiffOpAdd))
                        {
                            var opAd = model[1];
                            var op = new Address();
                            if (opAd.Id > 0 && ra.Id != opAd.Id)
                            {
                                op = _addRep.FindBy(a => a.Id == opAd.Id).FirstOrDefault();
                                if (op != null)
                                {
                                    op.Address_1 = opAd.address_1;
                                    op.Address_2 = opAd.address_2;
                                    op.City = opAd.City;
                                    op.Country_Id = opAd.Country_Id;
                                    op.StateId = opAd.StateId;
                                    op.Postal_Code = opAd.postal_code;
                                    _addRep.Edit(op);
                                }
                                else
                                {
                                    op = new Address();
                                    op.Address_1 = opAd.address_1;
                                    op.Address_2 = opAd.address_2;
                                    op.City = opAd.City;
                                    op.Country_Id = opAd.Country_Id;
                                    op.StateId = opAd.StateId;
                                    op.Postal_Code = opAd.postal_code;
                                    _addRep.Add(op);
                                }
                            }
                            else
                            {
                                op.Address_1 = opAd.address_1;
                                op.Address_2 = opAd.address_2;
                                op.City = opAd.City;
                                op.Country_Id = opAd.Country_Id;
                                op.StateId = opAd.StateId;
                                op.Postal_Code = opAd.postal_code;
                                _addRep.Add(op);
                            }
                            _addRep.Save(User.Identity.Name, Request.UserHostAddress);
                            comp.Operational_Address_Id = op.Id;
                        }
                        #endregion

                        _compRep.Edit(comp);
                        _compRep.Save(User.Identity.Name, Request.UserHostAddress);

                        trans.Complete();
                        TempData["status"] = "pass";
                        TempData["message"] = "Company Address updated successfully";
                        return RedirectToAction("Update", new { continueurl = conturl });
                    }
                    catch (Exception)
                    {
                        TempData["status"] = "fail";
                        TempData["message"] = "Sorry there was an error while handling your request";

                        return RedirectToAction("Update", new { continueurl = conturl });
                    }
                }
            }
            TempData["status"] = "fail";
            TempData["message"] = "Sorry there was an error while handling your request";

            return RedirectToAction("Update", new { continueurl = conturl });
        }

        #endregion

        #region Company Director

        public ActionResult CompanyDirector(int id, int? did, string view)
        {
            var comp = _compRep.FindBy(a => a.Id == id).FirstOrDefault();
            if (comp != null)
            {
                var cd = new vCompanyDirector();
                if (did != null)
                {
                    var d = _vCompDirRep.FindBy(a => a.Id == did).FirstOrDefault();
                    if (d != null)
                    {
                        cd = d;
                    }
                }
                ViewBag.country = _countryRep.GetAll().OrderBy(c => c.Name).ToList();
                var dts = _vCompDirRep.FindBy(a => a.Company_Id == comp.Id).ToList();

                if (dts.Count <= 0)
                {
                    // No Address had been registered to this company OR The address is not properly completed
                    ViewBag.DirComplete = false;
                }
                else
                {
                    //Completed
                    ViewBag.DirComplete = true;
                }
                ViewBag.directors = dts;
                if (!string.IsNullOrEmpty(view) && view.ToLower().Trim() == "update")
                {
                    ViewBag.View = view;
                    return View("UpdateDirector", cd);
                }
                else
                    return View(cd);
            }
            return View("Error");
        }
        public ActionResult getDirector(string id)
        {
            var Id = Convert.ToInt32(id);
            var d = _vCompDirRep.FindBy(a => a.Id == Id).FirstOrDefault();
            ViewBag.state = _stateRep.FindBy(a => a.CountryId == d.Country_Id).ToList();
            ViewBag.country = _countryRep.GetAll().OrderBy(c => c.Name).ToList();
            return View(d == null ? new vCompanyDirector() : d);
        }

        [HttpPost]
        public ActionResult CompanyDirectorUpdate(vCompanyDirector model, string d_action)
        {

            var usr = User.Identity.Name;
            var conturl = TempData["ELPS-ContinueUrl"] != null ? TempData["ELPS-ContinueUrl"].ToString() : "";

            using (var trns = new TransactionScope())
            {
                try
                {
                    var comp = coyHelper.MyCompany(usr);
                    var director = new Company_Director();

                    if (comp != null)
                    {
                        #region Save to Local
                        var ad = new Address();
                        if (model.Address_Id > 0)
                        {
                            #region Save Local address
                            ad = _addRep.FindBy(a => a.Id == model.Address_Id).FirstOrDefault();
                            if (ad != null)
                            {
                                ad.Address_1 = model.address_1;
                                ad.Address_2 = model.address_2;
                                ad.City = model.City;
                                ad.Country_Id = model.Country_Id;
                                ad.StateId = model.StateId;
                                ad.Postal_Code = model.PostalCode;
                                _addRep.Edit(ad);
                            }
                            else
                            {
                                ad = new Address();
                                ad.Address_1 = model.address_1;
                                ad.Address_2 = model.address_2;
                                ad.City = model.City;
                                ad.Country_Id = model.Country_Id;
                                ad.StateId = model.StateId;
                                ad.Postal_Code = model.PostalCode;
                                _addRep.Add(ad);
                            }
                            #endregion
                        }
                        else
                        {
                            ad = new Address();
                            ad.Address_1 = model.address_1;
                            ad.Address_2 = model.address_2;
                            ad.City = model.City;
                            ad.Country_Id = model.Country_Id;
                            ad.StateId = model.StateId;
                            ad.Postal_Code = model.PostalCode;
                            _addRep.Add(ad);

                        }
                        _addRep.Save(User.Identity.Name, Request.UserHostAddress);

                        #region Save Director to Local
                        if (model.Id > 0)
                        {
                            director = _compDirRep.FindBy(a => a.Id == model.Id).FirstOrDefault();
                            if (director != null)
                            {
                                director.Address_Id = ad.Id;
                                director.Company_Id = comp.Id;
                                director.FirstName = model.FirstName;
                                director.LastName = model.LastName;
                                director.Nationality = model.Nationality;
                                director.Telephone = model.Telephone;
                                _compDirRep.Edit(director);
                            }
                            else
                            {
                                director = new Company_Director();
                                director.Address_Id = ad.Id;
                                director.Company_Id = comp.Id;
                                director.FirstName = model.FirstName;
                                director.LastName = model.LastName;
                                director.Nationality = model.Nationality;
                                director.Telephone = model.Telephone;
                                _compDirRep.Add(director);
                            }
                        }
                        else
                        {
                            director = new Company_Director();
                            director.Address_Id = ad.Id;
                            director.Company_Id = comp.Id;
                            director.FirstName = model.FirstName;
                            director.LastName = model.LastName;
                            director.Nationality = model.Nationality;
                            director.Telephone = model.Telephone;
                            _compDirRep.Add(director);
                        }
                        _compDirRep.Save(User.Identity.Name, Request.UserHostAddress);
                        #endregion

                        trns.Complete();

                        TempData["status"] = "pass";
                        TempData["message"] = "New Director added to Company Directors.";
                        return RedirectToAction("Update", new { continueurl = conturl });
                        #endregion
                    }
                    throw new ArgumentException("Company not found");
                }
                catch (Exception ex)
                {
                    // Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    trns.Dispose();

                    TempData["status"] = "fail";
                    TempData["message"] = "Sorry there was an error while processing your request. Please try again";
                    return RedirectToAction("Update", new { continueurl = conturl });
                }
            }
        }

        public ActionResult SingleDirector(int id)
        {
            var cd = _vCompDirRep.FindBy(a => a.Id == id).FirstOrDefault();
            ViewBag.country = _countryRep.GetAll().OrderBy(c => c.Name).ToList();
            ViewBag.state = _stateRep.FindBy(a => a.CountryId == cd.Country_Id).ToList();

            return View("_ViewDir", cd);
        }

        public ActionResult SingleStaff(int id)
        {
            var cd = _compKSRep.FindBy(a => a.Id == id).FirstOrDefault();
            if (cd != null)
            {
                ViewBag.country = _countryRep.GetAll().OrderBy(c => c.Name).ToList();
                ViewBag.KSCert = _ksCertRep.FindBy(a => a.Company_Key_Staff == cd.Id).ToList();
            }
            return View("_ViewStaff", cd);
        }

        #endregion

        #region Company KeyStaff

        public ActionResult CompanyStaff(int id, string view)
        {
            var comp = _compRep.FindBy(a => a.Id == id).FirstOrDefault();
            if (comp != null)
            {
                ViewBag.country = _countryRep.GetAll().OrderBy(c => c.Name).ToList();
                var cs = new Company_Key_Staff();
                var css = _compKSRep.FindBy(a => a.Company_Id == comp.Id).ToList();

                if (css.Count <= 0)
                {
                    // No Address had been registered to this company OR The address is not properly completed
                    ViewBag.StaffComplete = false;
                }
                else
                {
                    //Completed
                    ViewBag.StaffComplete = true;
                }

                ViewBag.ckStaff = css;
                if (!string.IsNullOrEmpty(view) && view.ToLower().Trim() == "update")
                {
                    ViewBag.staffId = cs.Id;
                    ViewBag.View = view;
                    return View("UpdateStaff", cs);
                }
                else
                    return View(cs);
            }
            return View("Error");
        }

        public ActionResult getStaff(string id)
        {
            var Id = Convert.ToInt32(id);
            ViewBag.country = _countryRep.GetAll().OrderBy(c => c.Name).ToList();

            var cs = _compKSRep.FindBy(a => a.Id == Id).FirstOrDefault();
            cs.Certificates = _ksCertRep.FindBy(a => a.Company_Key_Staff == Id).ToList();

            return View(cs);
        }

        [HttpPost]
        public ActionResult CompanyStaffUpdate(Company_Key_Staff model, List<Key_Staff_Certificate> sCert, string did, string k_action)
        {

            int Did = Convert.ToInt32(did);
            if (model.Company_Id == model.Id)
            {
                model.Id = Did;
            }
            var usr = User.Identity.Name;
            var conturl = TempData["ELPS-ContinueUrl"] != null ? TempData["ELPS-ContinueUrl"].ToString() : "";

            var comp = coyHelper.MyCompany(usr);
            if (comp != null)
            {
                var cs = new Company_Key_Staff();

                using (var trans = new TransactionScope())
                {
                    try
                    {
                        #region Key Staff info
                        if (model.Id > 0)
                        {
                            cs = _compKSRep.FindBy(a => a.Id == model.Id).FirstOrDefault();
                            if (cs != null)
                            {
                                cs.Designation = model.Designation;
                                cs.FirstName = model.FirstName;
                                cs.LastName = model.LastName;
                                cs.Nationality = model.Nationality;
                                cs.Qualification = model.Qualification;
                                cs.Skills = model.Skills;
                                cs.Years_Of_Exp = model.Years_Of_Exp;

                                _compKSRep.Edit(cs);
                            }
                            else
                            {
                                cs = new Company_Key_Staff();
                                cs.Company_Id = comp.Id;
                                cs.Designation = model.Designation;
                                cs.FirstName = model.FirstName;
                                cs.LastName = model.LastName;
                                cs.Nationality = model.Nationality;
                                cs.Qualification = model.Qualification;
                                cs.Skills = model.Skills;
                                cs.Years_Of_Exp = model.Years_Of_Exp;

                                _compKSRep.Add(cs);
                            }
                        }
                        else
                        {
                            cs = new Company_Key_Staff();
                            cs.Company_Id = comp.Id;
                            cs.Designation = model.Designation;
                            cs.FirstName = model.FirstName;
                            cs.LastName = model.LastName;
                            cs.Nationality = model.Nationality;
                            cs.Qualification = model.Qualification;
                            cs.Skills = model.Skills;
                            cs.Years_Of_Exp = model.Years_Of_Exp;

                            _compKSRep.Add(cs);
                        }
                        _compKSRep.Save(User.Identity.Name, Request.UserHostAddress);
                        #endregion

                        #region key staff certificate

                        if (sCert != null && sCert.Count > 0)
                        {
                            foreach (var item in sCert)
                            {
                                var sc = new Key_Staff_Certificate();
                                if (item.Id > 0)
                                {
                                    sc = _ksCertRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                                    if (sc != null)
                                    {
                                        sc.Cert_No = item.Cert_No;
                                        sc.Name = item.Name;
                                        sc.Issuer = item.Issuer;
                                        sc.Year = item.Year;
                                        _ksCertRep.Edit(sc);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(item.Cert_No))
                                    {
                                        sc.Company_Key_Staff = cs.Id;
                                        sc.Cert_No = item.Cert_No;
                                        sc.Issuer = item.Issuer;
                                        sc.Name = item.Name;
                                        sc.Year = item.Year;
                                        _ksCertRep.Add(sc);
                                    }
                                }
                                _ksCertRep.Save(User.Identity.Name, Request.UserHostAddress);

                            }
                        }
                        #endregion

                        trans.Complete();

                        TempData["status"] = "pass";
                        TempData["message"] = "New Key Staff added to Company Staff.";

                        if (!string.IsNullOrEmpty(k_action) && k_action.ToLower().Contains("continue"))
                        {
                            //Move on
                            return Redirect(conturl);
                        }
                        else
                        {
                            //Save and stay
                            return RedirectToAction("Update", new { continueurl = conturl });
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Dispose();

                        TempData["status"] = "fail";
                        TempData["message"] = "Sorry there was an error while processing your request. Please try again";
                        return RedirectToAction("Update", new { continueurl = conturl });
                    }
                }
            }
            TempData["status"] = "fail";
            TempData["message"] = "Sorry there was an error while processing your request. Please try again";
            return RedirectToAction("Update", new { continueurl = conturl });
        }


        #endregion

        #region DOCUMENTS
        public ActionResult Documents()
        {
            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }

            var docs = _docRep.GetAll().ToList();

            return View(docs);
        }

        public ActionResult AddDocument()
        {
            return View(new Document_Type());
        }

        [HttpPost]
        public ActionResult AddDocument(string name, string type)
        {
            var model = _docRep.FindBy(a => a.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if (model == null)
            {
                model = new Document_Type() { Name = name, Type = type };
                _docRep.Add(model);
                _docRep.Save(User.Identity.Name, Request.UserHostAddress);

                TempData["alertModel"] = new AlertModel() { AlertType = "success", Title = "Document Alert", Message = "New Document Created successfully." };
            }
            else
            {
                TempData["alertModel"] = new AlertModel() { AlertType = "fail", Title = "Document Alert", Message = "Something happened! Document not created." };
            }
            return RedirectToAction("Documents");
        }

        public ActionResult EditDocument(int id)
        {
            var model = _docRep.FindBy(a => a.Id == id).FirstOrDefault();
            if (model != null)
            {
                return View(model);
            }
            else
            {
                TempData["alertModel"] = new AlertModel() { AlertType = "fail", Title = "Document Alert", Message = "Selected Document for edit does not exist or has been deleted." };
                return RedirectToAction("Documents");
            }
        }

        [HttpPost]
        public ActionResult EditDocument(Document_Type model)
        {
            var pick = _docRep.FindBy(a => a.Id == model.Id).FirstOrDefault();
            if (pick != null)
            {
                pick.Name = model.Name;
                if (!string.IsNullOrEmpty(model.Type))
                    pick.Type = model.Type.Trim().ToLower();
                _docRep.Edit(pick);
                _docRep.Save(User.Identity.Name, Request.UserHostAddress);

                TempData["alertModel"] = new AlertModel() { AlertType = "success", Title = "Document Alert", Message = "New Document type modified successfully." };
            }
            else
            {
                TempData["alertModel"] = new AlertModel() { AlertType = "fail", Title = "Document Alert", Message = "Something happened! Document not Modified." };
            }
            return RedirectToAction("Documents");
        }


        public int DocCount()
        {
            var docs = _docRep.GetAll().ToList();
            return docs.Count();
        }
        #endregion
    }
}