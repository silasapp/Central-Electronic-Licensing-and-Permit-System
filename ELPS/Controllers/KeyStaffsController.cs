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
using System.Transactions;

namespace ELPS.Controllers
{
    [RoutePrefix("api/KeyStaff")]
    public class KeyStaffsController : ApiController
    {
        ICompanyRepository _compRep;
        IAppIdentityRepository _appIdRep;
        ICompany_Key_StaffRepository _keyStaffRep;
        IAddressRepository _addRep;
        IKey_Staff_CertificateRepository _keyCertRep;

        public KeyStaffsController(ICompanyRepository compRep, IAppIdentityRepository appIdRep, ICompany_Key_StaffRepository keyStaffRep,
            IAddressRepository addRep, IKey_Staff_CertificateRepository kCert)
        {
            _keyCertRep = kCert;
            _compRep = compRep;
            _appIdRep = appIdRep;
            _keyStaffRep = keyStaffRep;
            _addRep = addRep;

        }

        [ResponseType(typeof(List<Company_Key_Staff>))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult GetKeyStaff(int CompId, string email, string apiHash)
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



            //var x = url.Split('/');
            //var usrl = string.Format(x[2]);

            var drs = _keyStaffRep.FindBy(a => a.Company_Id == CompId).ToList();
            if (drs == null || drs.Count < 0)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Item does not Fond"
                });
            }


            return Ok(drs);
        }

        [ResponseType(typeof(Company_Key_Staff))]
        [Route("ById/{Id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetKeyStaffById(int Id, string email, string apiHash)
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


            var ks = _keyStaffRep.FindBy(a => a.Id == Id).FirstOrDefault();

            if (ks == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Item does not Exist"
                });
            }
            else
            {
                ks.Certificates = _keyCertRep.FindBy(a => a.Company_Key_Staff == ks.Id).ToList();
            }
            return Ok(ks);
        }

        [ResponseType(typeof(List<Company_Key_Staff>))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult PostKeyStaff(int CompId, string email, string apiHash, List<Company_Key_Staff> keyStaffs)
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

            var comp = _compRep.FindBy(a => a.Id == CompId).FirstOrDefault();
            if (comp == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Item does not Fond"
                });
            }

            #endregion
            #region logic
            using (var trans = new TransactionScope())
            {
                try
                {
                    var cdLst = new List<Company_Key_Staff>();
                    if (keyStaffs != null && keyStaffs.Count > 0)
                    {
                        foreach (var item in keyStaffs)
                        {
                            var ks = new Company_Key_Staff();
                            if (item.Id <= 0)
                            {
                                #region Add New
                                ks.Company_Id = comp.Id;
                                ks.FirstName = item.FirstName;
                                ks.LastName = item.LastName;
                                ks.Nationality = item.Nationality;
                                ks.Designation = item.Designation;
                                ks.Qualification = item.Qualification;
                                ks.Skills = item.Skills;
                                //ks.Training_Certificates = item.Training_Certificates;
                                ks.Years_Of_Exp = item.Years_Of_Exp;

                                _keyStaffRep.Add(ks);
                                #endregion
                                _keyStaffRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                #region Work Cert
                                if (item.Certificates != null && item.Certificates.Any())
                                {
                                    ks.Certificates = new List<Key_Staff_Certificate>();
                                    foreach (var cert in item.Certificates)
                                    {
                                        var cCheck = _keyCertRep.FindBy(a => a.Id == cert.Id && a.Company_Key_Staff == ks.Id).FirstOrDefault();
                                        if (cCheck != null)
                                        {
                                            //EDIT
                                            cCheck.Issuer = cert.Issuer;
                                            cCheck.Name = cert.Name;
                                            cCheck.Year = cert.Year;
                                            cCheck.Cert_No = cert.Cert_No;
                                            _keyCertRep.Edit(cCheck);
                                        }
                                        else
                                        {
                                            //Add
                                            cCheck = new Key_Staff_Certificate();
                                            cCheck.Issuer = cert.Issuer;
                                            cCheck.Company_Key_Staff = ks.Id;
                                            cCheck.Name = cert.Name;
                                            cCheck.Year = cert.Year;
                                            cCheck.Cert_No = cert.Cert_No;
                                            _keyCertRep.Add(cCheck);
                                        }
                                        _keyCertRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                        ks.Certificates.Add(cCheck);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                ks = _keyStaffRep.FindBy(a => a.Id == ks.Id).FirstOrDefault();

                                if (ks != null)
                                {
                                    #region Edit
                                    ks.Company_Id = comp.Id;
                                    ks.FirstName = item.FirstName;
                                    ks.LastName = item.LastName;
                                    ks.Nationality = item.Nationality;
                                    ks.Designation = item.Designation;
                                    ks.Qualification = item.Qualification;
                                    ks.Skills = item.Skills;
                                    //ks.Training_Certificates = item.Training_Certificates;
                                    ks.Years_Of_Exp = item.Years_Of_Exp;
                                    _keyStaffRep.Edit(ks);
                                    #endregion
                                }
                                else
                                {
                                    #region Add New
                                    ks.Company_Id = comp.Id;
                                    ks.FirstName = item.FirstName;
                                    ks.LastName = item.LastName;
                                    ks.Nationality = item.Nationality;
                                    ks.Designation = item.Designation;
                                    ks.Qualification = item.Qualification;
                                    ks.Skills = item.Skills;
                                    //ks.Training_Certificates = item.Training_Certificates;
                                    ks.Years_Of_Exp = item.Years_Of_Exp;

                                    _keyStaffRep.Add(ks);
                                    #endregion
                                }
                                _keyStaffRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                #region Work Cert
                                if (item.Certificates != null && item.Certificates.Any())
                                {
                                    ks.Certificates = new List<Key_Staff_Certificate>();
                                    foreach (var cert in item.Certificates)
                                    {
                                        var cCheck = _keyCertRep.FindBy(a => a.Id == cert.Id && a.Company_Key_Staff == ks.Id).FirstOrDefault();
                                        if (cCheck != null)
                                        {
                                            //EDIT
                                            cCheck.Issuer = cert.Issuer;
                                            cCheck.Name = cert.Name;
                                            cCheck.Year = cert.Year;
                                            cCheck.Cert_No = cert.Cert_No;
                                            _keyCertRep.Edit(cCheck);
                                        }
                                        else
                                        {
                                            //Add
                                            cCheck = new Key_Staff_Certificate();
                                            cCheck.Company_Key_Staff = ks.Id;
                                            cCheck.Issuer = cert.Issuer;
                                            cCheck.Name = cert.Name;
                                            cCheck.Year = cert.Year;
                                            cCheck.Cert_No = cert.Cert_No;
                                            _keyCertRep.Add(cCheck);
                                        }
                                        _keyCertRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                        ks.Certificates.Add(cCheck);
                                    }
                                }
                                #endregion
                            }

                            cdLst.Add(ks);
                        }
                    }
                    else
                    {
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            ReasonPhrase = "Address Model cannot be empty"
                        });
                    }

                    trans.Complete();
                    return Ok(cdLst);

                }
                catch (Exception ex)
                {
                    trans.Dispose();

                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        ReasonPhrase = "Some Error while handling your Request"
                    });
                }
            }
            #endregion
        }

        [ResponseType(typeof(List<Company_Key_Staff>))]
        [Route("{email}/{apiHash}")]
        public IHttpActionResult PutKeyStaff(string email, string apiHash, List<Company_Key_Staff> keyStaffs)
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
            #region logic
            var cdLst = new List<Company_Key_Staff>();
            try
            {
                if (keyStaffs != null && keyStaffs.Count > 0)
                {
                    foreach (var item in keyStaffs)
                    {
                        var ks = _keyStaffRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                        if (ks != null)
                        {
                            ks.Company_Id = item.Company_Id;
                            ks.FirstName = item.FirstName;
                            ks.LastName = item.LastName;
                            ks.Nationality = item.Nationality;
                            ks.Designation = item.Designation;
                            ks.Qualification = item.Qualification;
                            ks.Skills = item.Skills;
                            ks.Training_Certificates = item.Training_Certificates;
                            ks.Years_Of_Exp = item.Years_Of_Exp;

                            _keyStaffRep.Edit(ks);
                            _keyStaffRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                            cdLst.Add(ks);

                            if(item.Certificates != null && item.Certificates.Any())
                            {
                                //Save the Certificates too
                                #region Work Cert
                                ks.Certificates = new List<Key_Staff_Certificate>();
                                    foreach (var cert in item.Certificates)
                                    {
                                        var cCheck = _keyCertRep.FindBy(a => a.Id == cert.Id && a.Company_Key_Staff == ks.Id).FirstOrDefault();
                                        if (cCheck != null)
                                        {
                                            //EDIT
                                            cCheck.Issuer = cert.Issuer;
                                            cCheck.Name = cert.Name;
                                            cCheck.Year = cert.Year;
                                            cCheck.Cert_No = cert.Cert_No;
                                            _keyCertRep.Edit(cCheck);
                                        }
                                        else
                                        {
                                            //Add
                                            cCheck = new Key_Staff_Certificate();
                                            cCheck.Issuer = cert.Issuer;
                                            cCheck.Company_Key_Staff = ks.Id;
                                            cCheck.Name = cert.Name;
                                            cCheck.Year = cert.Year;
                                            cCheck.Cert_No = cert.Cert_No;
                                            _keyCertRep.Add(cCheck);
                                        }
                                        _keyCertRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                        ks.Certificates.Add(cCheck);
                                    }
                                
                                #endregion
                            }
                        }
                    }
                }
                else
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "Staff Model cannot be empty"
                    });
                }

                return Ok(cdLst);
            }
            catch (Exception)
            {

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "Some Error while handling your Request"
                });
            }
            #endregion
        }
    }
}
