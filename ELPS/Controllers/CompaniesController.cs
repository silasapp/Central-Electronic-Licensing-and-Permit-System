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
using Newtonsoft.Json;
using Microsoft.Owin;

namespace ELPS.Controllers
{
    [RoutePrefix("api/Company")]
    public class CompaniesController : ApiController
    {
        #region properties
        ICompanyRepository _compRep;
        IAppIdentityRepository _appIdRep;
        IvCompanyFileRepository _vCoyFileRep;
        ICompany_NsitfRepository _nsitfRep;
        ICompany_MedicalRepository _medRep;
        ICompany_ProffessionalRepository _profRep;
        ICompany_Technical_AgreementRepository _techAgreeRep;
        ICompany_Expatriate_QuotaRepository _expQuoatRep;
        IvAffiliateRepository _vAffiliateRep;

        IvCompanyDirectorRepository _vCoyDirRep;
        IvCompanyExpatriateQuotaRepository _vCoyExpRep;
        IvCompanyMedicalRepository _vCoyMedRep;
        IvCompanyNsitfRepository _vCoyNsitfRep;
        IvCompanyProffessionalRepository _vCoyProRep;
        IvCompanyTechnicalAgreementRepository _vCoyTechRep;
        IFacilityDocumentRepository _facDocRep;
        IFacilityRepository _facRep;
        IvFacilityFileRepository _facFileRep;
        IAffiliateRepository _affRep;
        IDocument_TypeRepository _docTypeRep;
        IFileRepository _fileRep;
        ICompany_DocumentRepository _compDocRep;
        FileHelper _fileHelper;

        private ApplicationUserManager _userManager;

        string baseUrl = ConfigurationManager.AppSettings["baseUrl"];

        string fileBaseUrl = ConfigurationManager.AppSettings["myFileBaseUrl"];
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compRep"></param>
        /// <param name="appIdRep"></param>
        /// <param name="nsitfRep"></param>
        /// <param name="medRep"></param>
        /// <param name="profRep"></param>
        /// <param name="techAgreeRep"></param>
        /// <param name="expQuoatRep"></param>
        /// <param name="fileRep"></param>
        /// <param name="compDocRep"></param>
        /// <param name="vCoyDirRep"></param>
        /// <param name="vCoyExpRep"></param>
        /// <param name="vCoyMedRep"></param>
        /// <param name="vCoyNsitfRep"></param>
        /// <param name="vCoyProRep"></param>
        /// <param name="vCoyTechRep"></param>
        /// <param name="vCoyFileRep"></param>
        /// <param name="docTypeRep"></param>
        /// <param name="facDocRep"></param>
        /// <param name="facRep"></param>
        /// <param name="facFileRep"></param>
        /// <param name="vAffiliateRep"></param>
        /// <param name="affRep"></param>
        public CompaniesController(ICompanyRepository compRep, IAppIdentityRepository appIdRep, ICompany_NsitfRepository nsitfRep, IAffiliateRepository affRep,
        ICompany_MedicalRepository medRep, ICompany_ProffessionalRepository profRep, ICompany_Technical_AgreementRepository techAgreeRep,
            ICompany_Expatriate_QuotaRepository expQuoatRep, IFileRepository fileRep, ICompany_DocumentRepository compDocRep, IvFacilityFileRepository facFileRep,
            IvCompanyDirectorRepository vCoyDirRep, IvCompanyExpatriateQuotaRepository vCoyExpRep, IvCompanyMedicalRepository vCoyMedRep,
            IvCompanyNsitfRepository vCoyNsitfRep, IvCompanyProffessionalRepository vCoyProRep, IvCompanyTechnicalAgreementRepository
            vCoyTechRep, IvCompanyFileRepository vCoyFileRep, IDocument_TypeRepository docTypeRep,IFacilityDocumentRepository facDocRep,
            IFacilityRepository facRep, IvAffiliateRepository vAffiliateRep)
        {
            _vAffiliateRep = vAffiliateRep;
            _affRep = affRep;
            _facFileRep = facFileRep;
            _facRep = facRep;
            _facDocRep = facDocRep;
            _vCoyFileRep = vCoyFileRep;
            _vCoyDirRep = vCoyDirRep;
            _vCoyExpRep = vCoyExpRep;
            _vCoyMedRep = vCoyMedRep;
            _vCoyNsitfRep = vCoyNsitfRep;
            _vCoyProRep = vCoyProRep;
            _vCoyTechRep = vCoyTechRep;
            _compRep = compRep;
            _appIdRep = appIdRep;
            _nsitfRep = nsitfRep;
            _medRep = medRep;
            _profRep = profRep;
            _techAgreeRep = techAgreeRep;
            _expQuoatRep = expQuoatRep;
            _fileRep = fileRep;
            _compDocRep = compDocRep;
            _docTypeRep = docTypeRep;
            _fileHelper = new FileHelper(_fileRep, _compDocRep, _facDocRep);
        }

        // GET: api/Participants/5
        /// <summary>
        /// Get a company detail by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>Returns a Company Model. </returns>

        [ResponseType(typeof(Company))]
        [Route("~/api/company/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetCompany(int id, string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
                });
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
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion

            var comp = _compRep.FindBy(a => a.Id == id).FirstOrDefault();// db.vParticipants.Find(id);
            if (comp == null)
            {
                return NotFound();
            }

            var isAff = _affRep.FindBy(a => a.ChildId == comp.Id).FirstOrDefault();
            if (isAff != null)
                comp.IsAffiliate = true;
            else
                comp.IsAffiliate = false;

            return Ok(comp);
        }

        /// <summary>
        /// Get a company detail by Company's email
        /// </summary>
        /// <param name="compemail">Company email</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>Returns a Company Model. </returns>
        [ResponseType(typeof(Company))]
        [Route("~/api/company/{compemail}/{email}/{apiHash}")]
        public IHttpActionResult GetCompany(string compemail, string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
                });
            }

            //check if call is from the app Owner
            //Uri url = new Uri(HttpContext.Current.Request.Url.AbsoluteUri);
            //if (url.HostNameType == UriHostNameType.Dns)
            //{
            //    //Nothing for now
            //}

            //string host = url.Host;
            //if (!host.ToLower().StartsWith(app.Url))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}

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
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion


            var comp = _compRep.FindBy(a => a.User_Id == compemail).FirstOrDefault();// db.vParticipants.Find(id);
            if (comp == null)
            {
                return NotFound();
            }
            var isAff = _affRep.FindBy(a => a.ChildId == comp.Id).FirstOrDefault();
            if (isAff != null)
                comp.IsAffiliate = true;
            else
                comp.IsAffiliate = false;

            return Ok(comp);
        }
        
        /// <summary>
        /// Gets a Company's Full model, this will include all Company Expatriate Quotas, Company Medicals, Company Nsitfs, Company Proffessionals, Company Technical Agreements
        /// </summary>
        /// <param name="compemail">Company email</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>company Details and all Company Expatriate Quotas, Company Medicals, Company Nsitfs, Company Proffessionals, Company Technical Agreements</returns>
        [ResponseType(typeof(CompanyModel))]
        [Route("~/api/companyfull/{compemail}/{email}/{apiHash}")]
        public IHttpActionResult GetCompanyFull(string compemail, string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
                });
            }

            //check if call is from the app Owner
            //Uri url = new Uri(HttpContext.Current.Request.Url.AbsoluteUri);
            //if (url.HostNameType == UriHostNameType.Dns)
            //{
            //    //Nothing for now
            //}

            //string host = url.Host;
            //if (!host.ToLower().StartsWith(app.Url))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}

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
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion

            var returnModel = new CompanyModel();

            var comp = _compRep.FindBy(a => a.User_Id.ToLower().Trim() == compemail.ToLower().Trim()).FirstOrDefault();// db.vParticipants.Find(id);
            
            if (comp == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    ReasonPhrase = "Company with the supplied Email could not be found"
                });
            }
            var isAff = _affRep.FindBy(a => a.ChildId == comp.Id).FirstOrDefault();
            if (isAff != null)
                comp.IsAffiliate = true;
            else
                comp.IsAffiliate = false;

            returnModel.Company = comp;
            returnModel.CompanyExpatriateQuotas = _vCoyExpRep.FindBy(a => a.Company_Id == comp.Id).ToList();
            returnModel.CompanyMedicals = _vCoyMedRep.FindBy(a => a.Company_Id == comp.Id).ToList();
            returnModel.CompanyNsitfs = _vCoyNsitfRep.FindBy(a => a.Company_Id == comp.Id).ToList();
            returnModel.CompanyProffessionals = _vCoyProRep.FindBy(a => a.Company_Id == comp.Id).ToList();
            returnModel.CompanyTechnicalAgreements = _vCoyTechRep.FindBy(a => a.Company_Id == comp.Id).ToList();

            return Ok(returnModel);
        }

        /// <summary>
        /// Post  a Copany Model to create a new Company
        /// </summary>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <param name="comp">Company Model to be added</param>
        /// <returns>Returns the Added Company model</returns>
        [ResponseType(typeof(Company))]
        [Route("~/api/company/Create/{email}/{apiHash}")]
        public IHttpActionResult CreateCompany(string email, string apiHash, Company comp)
        {
            #region Initial Check
            if (string.IsNullOrEmpty(email))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "App UserName cannot be empty"
                });
            }
            var app = _appIdRep.FindBy(a => a.Email.ToLower().Trim() == email.ToLower().Trim()).FirstOrDefault();
            /*if (app == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact DPR Dev"
                });
            }*/
            if (comp == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Company Object cannot be empty"
                });
            }
            
            /*if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact DPR Dev"
                });
            }*/
            var cp = _compRep.FindBy(a => a.RC_Number == comp.RC_Number || a.User_Id.ToLower() == comp.User_Id.ToLower() || a.Name.ToLower() == comp.Name.ToLower()).FirstOrDefault();
            if (cp != null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Company Object is Existing"
                });
            }
            #endregion

            try
            {
                #region Create company

                cp = new Company
                {
                    RC_Number = comp.RC_Number,
                    Name = comp.Name,
                    User_Id = comp.User_Id, //this was changed, it was formally Name = comp.User_Id
                    Business_Type = comp.Business_Type,
                    Contact_Phone = comp.Contact_Phone,
                    Date = UtilityHelper.CurrentTime,
                    IsCompleted = false
                };

                _compRep.Add(cp);
                _compRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                
                #endregion

                return Ok(cp);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "there were some Error while handling your Request: " + ex.Message
                });
            }
            //var nsitf = compV.CoyNsitfView;

        }

        /// <summary>
        /// Update a Company and its Components 
        /// </summary>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <param name="companyModel">Company Detail with Company Expatriate Quotas, Company Medicals, Company Nsitfs, Company Proffessionals and Company Technical Agreements </param>
        /// <returns>company Details and all Company Expatriate Quotas, Company Medicals, Company Nsitfs, Company Proffessionals, Company Technical Agreements</returns>

        [ResponseType(typeof(CompanyModel))]
        [Route("~/api/company/Edit/{email}/{apiHash}")]
        public IHttpActionResult PutCompany(string email, string apiHash, CompanyModel companyModel)
        {
            #region Initial Check
            var RComp = new CompanyModel();
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

            if (companyModel == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Company Obect cannot be empty"
                });
            }
            if (companyModel.Company == null)
            {

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Company Obect cannot be empty"
                });
            }
            var comp = companyModel.Company;

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
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            // select the Company to be Edited
            var cp = _compRep.FindBy(a => a.Id == comp.Id).FirstOrDefault();
            if (cp == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Item not found"
                });
            }
            #endregion
            try
            {
                #region update company
                cp.Accident = comp.Accident;
                cp.Accident_Report = comp.Accident_Report;
                cp.Affiliate = comp.Affiliate;
                cp.Contact_FirstName = comp.Contact_FirstName;
                cp.Contact_LastName = comp.Contact_LastName;
                cp.Contact_Phone = comp.Contact_Phone;
                cp.Hse = comp.Hse;
                cp.HSEDoc = comp.HSEDoc;
                cp.IsCompleted = comp.IsCompleted;
                cp.Mission_Vision = comp.Mission_Vision;
                cp.Nationality = comp.Nationality;
                cp.No_Expatriate = comp.No_Expatriate;
                cp.No_Staff = comp.No_Staff;
                cp.Operational_Address_Id = comp.Operational_Address_Id;
                cp.Registered_Address_Id = comp.Registered_Address_Id;
                cp.Tin_Number = comp.Tin_Number;
                cp.Total_Asset = comp.Total_Asset;
                cp.Training_Program = comp.Training_Program;
                cp.Year_Incorporated = comp.Year_Incorporated;
                cp.Yearly_Revenue = comp.Yearly_Revenue;

                _compRep.Edit(cp);
                _compRep.Save(cp.User_Id, HttpContext.Current.Request.UserHostAddress);
                RComp.Company = cp;
                #endregion

                #region NSITF
                if (companyModel.CompanyNsitfs != null && companyModel.CompanyNsitfs.Count > 0)
                {
                    //var nsts = new List<Company_Nsitf>();
                    foreach (var item in companyModel.CompanyNsitfs)
                    {
                        if (item.Id > 0)
                        {
                            var nt = _nsitfRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                            if (nt != null)
                            {
                                nt.Date_Issued = item.Date_Issued;
                                nt.FileId = item.FileId;
                                nt.No_People_Covered = item.No_People_Covered;
                                nt.Policy_No = item.Policy_No;
                                _nsitfRep.Edit(nt);
                                _nsitfRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                            else
                            {
                                nt = new Company_Nsitf
                                {
                                    Date_Issued = item.Date_Issued,
                                    Company_Id = comp.Id,
                                    FileId = item.FileId,
                                    No_People_Covered = item.No_People_Covered,
                                    Policy_No = item.Policy_No
                                };

                                _nsitfRep.Add(nt);
                                _nsitfRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                        }
                        else
                        {
                            var nt = new Company_Nsitf
                            {
                                Date_Issued = item.Date_Issued,
                                Company_Id = comp.Id,
                                FileId = item.FileId,
                                No_People_Covered = item.No_People_Covered,
                                Policy_No = item.Policy_No
                            };

                            _nsitfRep.Add(nt);
                            _nsitfRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                        }
                    }
                    RComp.CompanyNsitfs = _vCoyNsitfRep.FindBy(a => a.Company_Id == comp.Id).ToList();
                }
                #endregion

                #region Medicals
                if (companyModel.CompanyMedicals != null && companyModel.CompanyMedicals.Count > 0)
                {
                    foreach (var item in companyModel.CompanyMedicals)
                    {
                        if (item.Id > 0)
                        {
                            var cm = _medRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                            if (cm != null)
                            {
                                cm.Address = item.Address;
                                cm.Date_Issued = item.Date_Issued;
                                cm.Email = item.Email;
                                cm.FileId = item.FileId;
                                cm.Medical_Organisation = item.Medical_Organisation;
                                cm.Phone = item.Phone;

                                _medRep.Edit(cm);
                                _medRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                            else
                            {
                                cm = new Company_Medical
                                {
                                    Address = item.Address,
                                    Date_Issued = item.Date_Issued,
                                    Email = item.Email,
                                    FileId = item.FileId,
                                    Medical_Organisation = item.Medical_Organisation,
                                    Phone = item.Phone,
                                    Company_Id = comp.Id
                                };

                                _medRep.Add(cm);
                                _medRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                        }
                        else
                        {
                            var cm = new Company_Medical
                            {
                                Address = item.Address,
                                Date_Issued = item.Date_Issued,
                                Email = item.Email,
                                FileId = item.FileId,
                                Medical_Organisation = item.Medical_Organisation,
                                Phone = item.Phone,
                                Company_Id = comp.Id
                            };

                            _medRep.Add(cm);
                            _medRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);

                        }
                    }
                    RComp.CompanyMedicals = _vCoyMedRep.FindBy(a => a.Company_Id == comp.Id).ToList();

                }
                #endregion

                #region Professionals
                if (companyModel.CompanyProffessionals != null && companyModel.CompanyProffessionals.Count > 0)
                {
                    foreach (var item in companyModel.CompanyProffessionals)
                    {
                        if (item.Id > 0)
                        {

                            var pf = _profRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                            if (pf != null)
                            {
                                pf.Cert_No = item.Cert_No;
                                pf.Date_Issued = item.Date_Issued;
                                pf.FileId = item.FileId;
                                pf.Proffessional_Organisation = item.Proffessional_Organisation;
                                _profRep.Edit(pf);
                                _profRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                            else
                            {
                                pf = new Company_Proffessional
                                {
                                    Company_Id = comp.Id,
                                    Cert_No = item.Cert_No,
                                    Date_Issued = item.Date_Issued,
                                    FileId = item.FileId,
                                    Proffessional_Organisation = item.Proffessional_Organisation
                                };
                                _profRep.Add(pf);
                                _profRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }

                        }
                        else
                        {
                            var pf = new Company_Proffessional
                            {
                                Company_Id = comp.Id,
                                Cert_No = item.Cert_No,
                                Date_Issued = item.Date_Issued,
                                FileId = item.FileId,
                                Proffessional_Organisation = item.Proffessional_Organisation
                            };
                            _profRep.Add(pf);
                            _profRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                        }

                    }
                    RComp.CompanyProffessionals = _vCoyProRep.FindBy(a => a.Company_Id == comp.Id).ToList();
                }
                #endregion

                #region Technical
                if (companyModel.CompanyTechnicalAgreements != null && companyModel.CompanyTechnicalAgreements.Count > 0)
                {
                    foreach (var item in companyModel.CompanyTechnicalAgreements)
                    {
                        if (item.Id > 0)
                        {
                            var ta = _techAgreeRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                            if (ta != null)
                            {
                                ta.FileId = item.FileId;
                                ta.Name = item.Name;

                                _techAgreeRep.Edit(ta);
                                _techAgreeRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                            else
                            {
                                ta = new Company_Technical_Agreement
                                {
                                    FileId = item.FileId,
                                    Name = item.Name,
                                    Company_Id = comp.Id
                                };
                                _techAgreeRep.Add(ta);
                                _techAgreeRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                        }
                        else
                        {
                            var ta = new Company_Technical_Agreement
                            {
                                FileId = item.FileId,
                                Name = item.Name,
                                Company_Id = comp.Id
                            };
                            _techAgreeRep.Add(ta);
                            _techAgreeRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                        }
                    }

                    RComp.CompanyTechnicalAgreements = _vCoyTechRep.FindBy(a => a.Company_Id == comp.Id).ToList();
                }
                #endregion

                #region Expertrate
                if (companyModel.CompanyExpatriateQuotas != null && companyModel.CompanyExpatriateQuotas.Count > 0)
                {
                    foreach (var item in companyModel.CompanyExpatriateQuotas)
                    {
                        if (item.Id > 0)
                        {
                            var eq = _expQuoatRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                            if (eq != null)
                            {
                                eq.FileId = item.FileId;
                                eq.Name = item.Name;
                                _expQuoatRep.Edit(eq);
                                _expQuoatRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                            else
                            {
                                eq = new Company_Expatriate_Quota
                                {
                                    Company_Id = comp.Id,
                                    FileId = item.FileId,
                                    Name = item.Name
                                };
                                _expQuoatRep.Add(eq);
                                _expQuoatRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                            }
                        }
                        else
                        {
                            var eq = new Company_Expatriate_Quota
                            {
                                Company_Id = comp.Id,
                                FileId = item.FileId,
                                Name = item.Name
                            };
                            _expQuoatRep.Add(eq);
                            _expQuoatRep.Save(comp.User_Id, HttpContext.Current.Request.UserHostAddress);
                        }
                    }

                    RComp.CompanyExpatriateQuotas = _vCoyExpRep.FindBy(a => a.Company_Id == comp.Id).ToList();

                }
                #endregion
                //return CreatedAtRoute("DefaultApi", new { id = comp.Id }, RComp);

                return Ok(RComp);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "there were some Error while handling your Request"
                });
            }
        }

        [ResponseType(typeof(CompanyModel))]
        [Route("~/api/Company/QuickEdit/{email}/{apiHash}")]
        public IHttpActionResult PutCompanyOnly(string email, string apiHash, CompanyModel companyModel)
        {
            #region Initial Check
            var RComp = new CompanyModel();
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

            if (companyModel == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Company Obect cannot be empty"
                });
            }
            if (companyModel.Company == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Company Obect cannot be empty"
                });
            }
            var comp = companyModel.Company;
            
            //compare hash provided
            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            // select the Company to be Edited
            var cp = _compRep.FindBy(a => a.Id == comp.Id).FirstOrDefault();
            if (cp == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Item not found"
                });
            }
            #endregion

            try
            {
                #region update company
                cp.Accident = comp.Accident;
                cp.Accident_Report = comp.Accident_Report;
                cp.Affiliate = comp.Affiliate;
                cp.Contact_FirstName = comp.Contact_FirstName;
                cp.Contact_LastName = comp.Contact_LastName;
                cp.Contact_Phone = comp.Contact_Phone;
                cp.Hse = comp.Hse;
                cp.HSEDoc = comp.HSEDoc;
                cp.IsCompleted = comp.IsCompleted;
                cp.Mission_Vision = comp.Mission_Vision;
                cp.Nationality = comp.Nationality;
                cp.No_Expatriate = comp.No_Expatriate;
                cp.No_Staff = comp.No_Staff;
                cp.Operational_Address_Id = comp.Operational_Address_Id;
                cp.Registered_Address_Id = comp.Registered_Address_Id;
                cp.Tin_Number = comp.Tin_Number;
                cp.Total_Asset = comp.Total_Asset;
                cp.Training_Program = comp.Training_Program;
                cp.Year_Incorporated = comp.Year_Incorporated;
                cp.Yearly_Revenue = comp.Yearly_Revenue;

                _compRep.Edit(cp);
                _compRep.Save(cp.User_Id, HttpContext.Current.Request.UserHostAddress);
                RComp.Company = cp;
                #endregion

                return Ok(RComp);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "there were some Error while handling your Request"
                });
            }
        }

        #region CompanyDocuments
        /// <summary>
        /// Get company Documents
        /// </summary>
        /// <param name="id">Company Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>Returns a list of company's Document</returns>
        [ResponseType(typeof(List<vCompanyFile>))]
        [Route("~/api/CompanyDocuments/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetCompanyDocuments(int id, string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
                });
            }

            //check if call is from the app Owner
            //Uri url = new Uri(HttpContext.Current.Request.Url.AbsoluteUri);
            //if (url.HostNameType == UriHostNameType.Dns)
            //{
            //    //Nothing for now
            //}

            //string host = url.Host;
            //if (!host.ToLower().StartsWith(app.Url))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}

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
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion
             
            var comp = _compRep.FindBy(a => a.Id == id).FirstOrDefault();// db.vParticipants.Find(id);
            if (comp == null)
            {
                return NotFound();
            }

            var compDocs = _vCoyFileRep.FindBy(a => a.CompanyId == comp.Id && !a.Archived && a.Status).ToList();
            if (compDocs.Any())
            {
                foreach (var doc in compDocs)
                {
                    doc.source = fileBaseUrl + doc.source.Replace("~", "");
                }
            }

            return Ok(compDocs);
        }
        
        /// <summary>
        /// Get Parent company Documents
        /// </summary>
        /// <param name="id">Company Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>Returns a list of company's Document</returns>
        [ResponseType(typeof(List<vCompanyFile>))]
        [Route("~/api/ParentCompanyDocuments/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetParentCompanyDocuments(int id, string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
                });
            }

            //check if call is from the app Owner
            //Uri url = new Uri(HttpContext.Current.Request.Url.AbsoluteUri);
            //if (url.HostNameType == UriHostNameType.Dns)
            //{
            //    //Nothing for now
            //}

            //string host = url.Host;
            //if (!host.ToLower().StartsWith(app.Url))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Sorry but you are not autorized to call from this app"
            //    });
            //}

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
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion

            var comp = _compRep.FindBy(a => a.Id == id).FirstOrDefault();// db.vParticipants.Find(id);
            if (comp == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Company Not Found"
                });
            }
            var aff = _affRep.FindBy(a => a.ChildId == comp.Id).FirstOrDefault();
            if(aff == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "No Affiliation Found"
                });
            }

            var pComp = _compRep.FindBy(a => a.Id == aff.ParentId).FirstOrDefault();// db.vParticipants.Find(id);
            if (pComp == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Parent Company Not Found"
                });
            }


            var compDocs = _vCoyFileRep.FindBy(a => a.CompanyId == pComp.Id && !a.Archived && a.Status).ToList();
            if (compDocs.Any())
            {
                foreach (var doc in compDocs)
                {
                    doc.source = fileBaseUrl + doc.source.Replace("~", "");
                }
            }

            return Ok(compDocs);
        }


        /// <summary>
        /// get Company Document by Id
        /// </summary>
        /// <param name="id">Document Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>Returns a Company's Document</returns>
        [ResponseType(typeof(vCompanyFile))]
        [Route("~/api/CompanyDocument/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetCompanyDocument(int id, string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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

            //var comp = _compRep.FindBy(a => a.Id == compid).FirstOrDefault();// db.vParticipants.Find(id);
            //if (comp == null)
            //{
            //    return NotFound();
            //}

            var compDoc = _vCoyFileRep.FindBy(a => a.Id == id).FirstOrDefault();
            if (compDoc != null)
            {
                compDoc.source = fileBaseUrl + compDoc.source.Replace("~", "");
            }

            return Ok(compDoc);
        }


        /// <summary>
        /// get Document types
        /// </summary>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>Returns all Document types</returns>
        [ResponseType(typeof(List<DocumentType>))]
        [Route("~/api/Documents/Types/{email}/{apiHash}")]
        public IHttpActionResult GetDocumentTypes( string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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

            var docs = _docTypeRep.FindBy(a => string.IsNullOrEmpty(a.Type) || a.Type.ToLower() == "company").ToList(); //.GetAll();
            var dtyps = new List<DocumentType>();
            DocumentType dtp;
            foreach (var item in docs)
            {
                dtp = new DocumentType() { Id = item.Id, Name = item.Name, Type = !string.IsNullOrEmpty(item.Type) ? item.Type : "company" };
                dtyps.Add(dtp);
            }

            return Ok(dtyps);
        }

        /// <summary>
        /// Posting Files (Save HttpPostedFileBase and return the File Id)
        /// </summary>
        /// <returns>returns the FileId and the Name</returns>
        [ResponseType(typeof(FileResponse))]
        [Route("UploadFile")]
        public IHttpActionResult PostUploadFile()//(IEnumerable<HttpPostedFileBase> files)
        {
            int FileId = 0;
            FileResponse fr = null;
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (file != null && file.ContentLength > 0)
            {
                fr = _fileHelper.UploadImage(file, User.Identity.Name, HttpContext.Current.Request.UserHostAddress);
            }

            //foreach (var item in files)
            //{
            //    FileId = _fileHelper.UploadImage(item,   User.Identity.Name, HttpContext.Current.Request.UserHostAddress);
            //    //FileId = _fileHelper.UploadCompDoc(item, comp, 0, id, User.Identity.Name, Request.UserHostAddress, note);
            //}

            fr.source = fileBaseUrl + fr.source.Replace("~", "");

            return Json(fr);
            //return Json(files.Select(x => new { name = x.FileName, fileid = FileId }));
        }

        /// <summary>
        /// Posting Files (Save HttpPostedFileBase) to Update annd replace Company document
        /// </summary>
        /// <param name="docid">Document Id</param>
        /// <param name="compid">Company Id</param>
        /// <param name="docfor">Document is either Company or Facility</param>
        /// <returns>Returns the FileId , the Name and the Source(file URL) </returns>
        [ResponseType(typeof(FileResponse))]
        [Route("~/api/CompanyDocument/UpdateFile/{id:int}/{compid}/{docfor}")]
        public IHttpActionResult PostUpdateFile(int docid, int compid, string docfor)//(IEnumerable<HttpPostedFileBase> files)
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (file != null && file.ContentLength > 0)
            {
                UtilityHelper.LogMessage("File to Upload (Size: " + file.ContentLength + "; Doc Id: " + docid + "; Comp Id: " + compid +")");
                var fr = _fileHelper.UpdateCompDoc(file, docid, compid, compid.ToString(), HttpContext.Current.Request.UserHostAddress, "", docfor);
                if (fr.FileId > 0)
                {
                    fr.source= fileBaseUrl + fr.source.Replace("~", "");
                    fr.name = file.FileName;
                    UtilityHelper.LogMessage("File update completed >>> Source: " + fr.source + "; FileID: " + fr.FileId);
                    return Json(fr);
                }
            }
            return Json(0);
        }

        /// <summary>
        /// Update Company Document
        /// </summary>
        /// <param name="id">Document Id</param>
        /// <param name="compid">Company Id</param>
        /// <param name="status">Document Status: Rejected or Not</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>Returns the Updated company Document model</returns>
        [ResponseType(typeof(Company_Document))]
        [Route("~/api/UpdateDocument/{id:int}/{compid}/{status}/{email}/{apiHash}")]
        public IHttpActionResult PutUpdateDocument(int id, int compid, bool status, string email, string apiHash)//(IEnumerable<HttpPostedFileBase> files)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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
            try
            {
                var coyDoc = _compDocRep.FindBy(a => a.Id == id && a.Company_Id == compid).FirstOrDefault();
                if (coyDoc != null)
                {
                    //Found in companyDocument
                    coyDoc.Status = status;
                    _compDocRep.Edit(coyDoc);
                    _compDocRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                    return Ok(coyDoc);
                }
                else
                {
                    //Check Facility Document
                    var facDoc = _facDocRep.FindBy(a => a.Id == id && a.Company_Id == compid).FirstOrDefault();
                    if (facDoc != null)
                    {
                        //Found in companyDocument
                        facDoc.Status = status;
                        _facDocRep.Edit(facDoc);
                        _facDocRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                        coyDoc = new Company_Document()
                        {
                            Company_Id = compid,
                            Status = status,
                            Source = facDoc.Source,
                            Name = facDoc.Name,
                            Document_Name = facDoc.Document_Name,
                            Document_Type_Id = facDoc.Document_Type_Id
                        };
                        return Ok(coyDoc);
                    }
                    else
                    {
                        throw new ArgumentException("Document not found");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "there were some Error while handling your Request"
                });
            }
            
        }

        /// <summary>
        /// Posting Files (Save HttpPostedFileBase )for a Company document type
        /// </summary>
        /// <param name="docTypeId">Document Type Id</param>
        /// <param name="compId">Company Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <param name="docName">(Optional)Document Name in the Case of Other Document that is not defined in the Official Document Types</param>
        /// <param name="uniqueid">(Optional)Unique Id for DocName in the Case of Other Document that is not defined in the Official Document Types</param>
        /// <returns></returns>
        [ResponseType(typeof(FileResponse))]
        [Route("~/api/UploadCompanyDoc/{docTypeId:int}/{compId:int}/{email}/{apiHash}")]
        public IHttpActionResult PostUploadCompanyDoc(int docTypeId, int compId, string email, string apiHash, string docName="", string uniqueid="")//, IEnumerable<HttpPostedFileBase> files)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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
            var company = _compRep.FindBy(a => a.Id == compId).FirstOrDefault();
            if (company == null)
            {
                return Json(new FileResponse { name = "Company Not found" });
            }

            var docu = _docTypeRep.FindBy(a => a.Id == docTypeId).FirstOrDefault();
            if (docu == null)
            {
                return Json(new FileResponse { name = "Specified document Not found" });
            }
            #endregion
            
            if (!string.IsNullOrEmpty(docName) && docName.ToLower() == "undefined")
            {
                docName = string.Empty;
            }
            if (!string.IsNullOrEmpty(uniqueid) && uniqueid.ToLower() == "undefined")
            {
                uniqueid = string.Empty;
            }

            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    var fr = _fileHelper.UploadCompDoc(file, docTypeId, compId, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.UserHostAddress, docName, uniqueid);//, User.Identity.Name, HttpContext.Current.Request.UserHostAddress);
                                                                                                                                                                                      // source = _vCoyFileRep.FindBy(a => a.Id == FileId).FirstOrDefault().source;
                    fr.source = fileBaseUrl + fr.source.Replace("~", "");

                    return Json(fr);
                }
                catch (Exception)
                {
                    return Json(new FileResponse { name = "An Error occured while processing your request." });
                }
            }
            else
            {
                return Json(new FileResponse { name = "No File Sent with the Request string." });
            }



            //foreach (var item in files)
            //{
            //    FileId = _fileHelper.UploadCompDoc(item, docTypeId, compId, HttpContext.Current.Request.Url.OriginalString, HttpContext.Current.Request.UserHostAddress, docName);
            //    //FileId = _fileHelper.UploadCompDoc(item, comp, 0, id, User.Identity.Name, Request.UserHostAddress, note);
            //}

            //return Json(files.Select(x => new { name = x.FileName, fileid = FileId }));
        }
        
        /// <summary>
        /// 
        /// </summary>
         /// <param name="id">Company Document Id</param>
         /// <param name="email">the Application(License) Email</param>
         /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>returns 1 if successful and 0 if not</returns>
        [Route("~/api/CompanyDocument/Delete/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult DeleteUploadCompanyDoc(int id, string email, string apiHash)//, IEnumerable<HttpPostedFileBase> files)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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
            var compDoc = _compDocRep.FindBy(c => c.Id == id).FirstOrDefault();
            if (compDoc != null)
            {
                _compDocRep.Delete(compDoc);
                _compDocRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                return Json(1);
            }
            else
            {
                return Json(0);
            }

        }

        #endregion


        #region Facilities

        /// <summary>
        /// Posting Files (Save HttpPostedFileBase )for a Company's Facility document type
        /// </summary>
        /// <param name="docTypeId">Document Type Id</param>
        /// <param name="compId">Company Id</param>
        /// <param name="facId"></param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <param name="docName">(Optional)Document Name in the Case of Other Document that is not defined in the Official Document Types</param>
        /// <param name="uniqueid">(Optional)Unique Id for DocName in the Case of Other Document that is not defined in the Official Document Types</param>
        /// <returns></returns>
        [ResponseType(typeof(FileResponse))]
        [Route("~/api/Facility/UploadFile/{docTypeId:int}/{compId:int}/{facId:int}/{email}/{apiHash}")]
        public IHttpActionResult PostUploadFacilityDoc(int docTypeId, int compId, int facId, string email, string apiHash, string docName = "", string uniqueid = "")//, IEnumerable<HttpPostedFileBase> files)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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
            var company = _compRep.FindBy(a => a.Id == compId).FirstOrDefault();
            if (company == null)
            {
                return Json(new FileResponse { name = "Company Not found" });
            }

            var docu = _docTypeRep.FindBy(a => a.Id == docTypeId).FirstOrDefault();
            if (docu == null)
            {
                return Json(new FileResponse { name = "Specified document Not found" });
            }
            #endregion

            //int FileId = 0;
            string source = string.Empty;
            if (!string.IsNullOrEmpty(docName) && docName.ToLower() == "undefined")
            {
                docName = string.Empty;
            }
            if (!string.IsNullOrEmpty(uniqueid) && uniqueid.ToLower() == "undefined")
            {
                uniqueid = string.Empty;
            }
            var fr = new FileResponse();
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    fr = _fileHelper.UploadFacilityDoc(file, docTypeId, compId, facId, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.UserHostAddress, docName, uniqueid);//, User.Identity.Name, HttpContext.Current.Request.UserHostAddress);
                    fr.source = fileBaseUrl + fr.source;// source = baseUrl + source.Replace("~", "");

                    return Json(fr);
                }
                catch (Exception)
                {
                    return Json(new FileResponse { FileId = 0, name = "An Error occured while processing your request." });
                }
            }
            else
            {
                return Json(new FileResponse { FileId = 0, name = "No File Sent with the Request string." });
            }
        }

        /// <summary>
        /// Posting Files (Save HttpPostedFileBase) to Update annd replace Company document
        /// </summary>
        /// <param name="docid">Document Id</param>
        /// <param name="facid">Company Id</param>
        /// <returns>Returns the FileId , the Name and the Source(file URL) </returns>
        [ResponseType(typeof(FileResponse))]
        [Route("~/api/FacilityDocument/UpdateFile/{id:int}/{facid}")]
        public IHttpActionResult PostUpdateFacilityFile(int docid, int facid)//(IEnumerable<HttpPostedFileBase> files)
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (file != null && file.ContentLength > 0)
            {
                var fac = _facRep.FindBy(a => a.Id == facid).FirstOrDefault();
                if (fac != null) {
                    var coy = _compRep.FindBy(a => a.Id == fac.CompanyId).FirstOrDefault();
                    if (coy != null)
                    {
                        UtilityHelper.LogMessage("File to Upload (Size: " + file.ContentLength + "; Doc Id: " + docid + "; Comp Id: " + coy.Id + ")");
                        var fr = _fileHelper.UpdateFacilityDoc(file, docid, coy.Id, facid, coy.User_Id, HttpContext.Current.Request.UserHostAddress);
                        if (fr.FileId > 0)
                        {
                            fr.source = fileBaseUrl + fr.source.Replace("~", "");
                            fr.name = file.FileName;
                            UtilityHelper.LogMessage("File update completed >>> Source: " + fr.source + "; FileID: " + fr.FileId);
                            return Json(fr);
                        }
                    }
                }
            }
            return Json(0);
        }


        /// <summary>
        /// Get All Facility Document types
        /// </summary>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <param name="Type">Document Type</param>
        /// <returns>Returns all Document types</returns>
        [ResponseType(typeof(List<DocumentType>))]
        [Route("~/api/Documents/Facility/{email}/{apiHash}/{Type}")]
        public IHttpActionResult GetDocumentTypes(string email, string apiHash, string Type)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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

            var docs = _docTypeRep.FindBy(a => a.Type.Trim().ToLower() == Type.Trim().ToLower()).ToList();
            var dtyps = new List<DocumentType>();
            DocumentType dtp;
            foreach (var item in docs)
            {
                dtp = new DocumentType() { Id = item.Id, Name = item.Name, Type = item.Type };
                dtyps.Add(dtp);
            }

            return Ok(dtyps);
        }


        //[ResponseType(typeof(List<string>))]
        //[Route("~/api/containers")]
        //public IHttpActionResult GetContainers()
        //{
        //    return Ok(_fileHelper.getContainer());
        //}

        /// <summary>
        /// Get Facility Documents by FacilityId
        /// </summary>
        /// <param name="id">Facility Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <returns>Returns a Company's Document</returns>
        [ResponseType(typeof(List<vFacilityFile>))]
        [Route("~/api/FacilityDocuments/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetFacilityDocument(int id, string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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

            var comp = _facRep.FindBy(a => a.Id == id).FirstOrDefault();// db.vParticipants.Find(id);
            if (comp == null)
            {
                return NotFound();
            }

            var facDoc = _facFileRep.FindBy(a => a.FacilityId == id).ToList();
            foreach (var item in facDoc)
            {
                item.source = fileBaseUrl + item.source.Replace("~", "");
            }

            return Ok(facDoc);
        }

        /// <summary>
        /// Get Facility Files by FacilityId
        /// </summary>
        /// <param name="id">Facility ID</param>
        /// <param name="email"></param>
        /// <param name="apiHash"></param>
        /// <returns></returns>
        [ResponseType(typeof(vFacilityFile))]
        [Route("~/api/FacilityFiles/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetFacilityFiles(int id, string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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

            var comp = _facRep.FindBy(a => a.Id == id).FirstOrDefault();// db.vParticipants.Find(id);
            if (comp == null)
            {
                return NotFound();
            }
            var compDoc = _facFileRep.FindBy(a => a.FacilityId == id).ToList();
            foreach (var item in compDoc)
            {
                item.source = fileBaseUrl + item.source.Replace("~", "");
            }

            return Ok(compDoc);
        }

        /// <summary>
        /// Add new Company Facility
        /// </summary>
        /// <param name="email"></param>
        /// <param name="apiHash"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [ResponseType(typeof(Facility))]
        [Route("~/api/Facility/Add/{email}/{apiHash}")]
        public IHttpActionResult PostAddFacility(string email, string apiHash, Facility model)
        {
            UtilityHelper.LogMessage("Adding Facility ... " + model == null ? "Empty Model":"Full Model (" + model.FacilityType + ")");
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

            #region logic
            try
            {
                if (model != null)
                {
                    var param = JsonConvert.SerializeObject(model);
                    UtilityHelper.LogMessage("Creating new Facility >>> " + param);
                    var fac = new Facility
                    {
                        City = model.City,
                        CompanyId = model.CompanyId,
                        FacilityType = model.FacilityType,
                        LGAId = model.LGAId,
                        Name = model.Name,
                        StateId = model.StateId,
                        StreetAddress = model.StreetAddress,
                        DateAdded = DateTime.Now
                    };
                    _facRep.Add(fac);
                    _facRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                    return Ok(fac);
                }
                else
                {
                    UtilityHelper.LogMessage("Empty model sent for Adding");
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "Facility Model cannot be empty"
                    });
                }
            }
            catch (Exception ex)
            {
                UtilityHelper.LogMessage(">>>>> " + ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "Some Error while handling your Request >>> " + ex.InnerException == null ? ex.Message : ex.InnerException.InnerException == null ? ex.InnerException.Message : ex.InnerException.InnerException.Message
                });
            }
            #endregion
        }

        /// <summary>
        /// Delete a Facility Document
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="email"></param>
        /// <param name="apiHash"></param>
        /// <returns></returns>
        [Route("~/api/FacilityDocument/Delete/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult DeleteFacilityDocument(int id, string email, string apiHash)//, IEnumerable<HttpPostedFileBase> files)
        {
            var doc = _facDocRep.FindBy(c => c.Id == id).FirstOrDefault();
            if (doc != null)
            {
                _facDocRep.Delete(doc);
                _facDocRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                return Json(1);
            }
            else
            {
                return Json(0);
            }

        }


        #endregion

        #region AFFILIATES
        [ResponseType(typeof(List<vAffiliate>))]
        [Route("~/api/CompanyAffiliates/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetCompanyAffiliates(string email, string apiHash)
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
                    ReasonPhrase = "App has been denied Access, Contact ELPS Dev. Team"
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

            var affiliates = _vAffiliateRep.GetAll().ToList();
            
            return Ok(affiliates);
        }

        #endregion
    }
    
}
