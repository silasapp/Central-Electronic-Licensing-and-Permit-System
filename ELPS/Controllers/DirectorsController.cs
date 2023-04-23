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

namespace ELPS.Controllers
{
    [RoutePrefix("api/Directors")]
    public class DirectorsController : ApiController
    {
        ICompanyRepository _compRep;
        IAppIdentityRepository _appIdRep;
        ICompany_DirectorRepository _compDRep;
        IAddressRepository _addRep;
        public DirectorsController(ICompanyRepository compRep, IAppIdentityRepository appIdRep, ICompany_DirectorRepository compDRep,
            IAddressRepository addRep)
        {
            _compRep = compRep;
            _appIdRep = appIdRep;
            _compDRep = compDRep;
            _addRep = addRep;

        }

        [ResponseType(typeof(List<Company_Director>))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult GetDirectors(int CompId, string email, string apiHash)
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

            //var url = HttpContext.Current.Request.UserHostName;//email;

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



            //var x = url.Split('/');
            //var usrl = string.Format(x[2]);

            var drs = _compDRep.FindBy(a => a.Company_Id == CompId).ToList();
            //if (drs == null || drs.Count <= 0)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
            //    {
            //        ReasonPhrase = "Item does not Fond"
            //    });
            //}
            foreach (var item in drs)
            {
                var ad = _addRep.FindBy(a => a.Id == item.Address_Id).FirstOrDefault();
                if (ad != null)
                {
                    item.Address = ad;
                }
            }

            return Ok(drs);
        }

        [ResponseType(typeof(Company_Director))]
        [Route("ById/{Id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetDirectorById(int Id, string email, string apiHash)
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

            //var url = HttpContext.Current.Request.UserHostName;//email;

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


            var dr = _compDRep.FindBy(a => a.Id == Id).FirstOrDefault();

            if (dr == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Item does not Exist"
                });
            }
            var cdAdd = _addRep.FindBy(a => a.Id == dr.Address_Id).FirstOrDefault();
            if (cdAdd != null)
            {
                dr.Address = cdAdd;
            }

            return Ok(dr);
        }

        [ResponseType(typeof(List<Company_Director>))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult PostDirectors(int CompId, string email, string apiHash, List<Company_Director> Directors)
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

            //var url = HttpContext.Current.Request.UserHostName;//email;

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
            try
            {
                var cdLst = new List<Company_Director>();
                if (Directors != null && Directors.Count > 0)
                {
                    foreach (var item in Directors)
                    {
                        if (item.Address != null)
                        {
                            var add = new Address();
                            add.Address_1 = item.Address.Address_1;
                            add.Address_2 = item.Address.Address_2;
                            add.City = item.Address.City;
                            add.Country_Id = item.Address.Country_Id;
                            add.Postal_Code = item.Address.Postal_Code;
                            add.StateId = item.Address.StateId;
                            _addRep.Add(add);
                            _addRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                            var dr = new Company_Director();
                            dr.Address_Id = add.Id;
                            dr.Company_Id = comp.Id;
                            dr.FirstName = item.FirstName;
                            dr.LastName = item.LastName;
                            dr.Nationality = item.Nationality;
                            dr.Telephone = item.Telephone;
                            _compDRep.Add(dr);
                            _compDRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                            dr.Address = add;
                            cdLst.Add(dr);
                        }

                    }
                }
                else
                {

                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "Address Model cannot be empty"
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

        [ResponseType(typeof(List<Company_Director>))]
        [Route("{email}/{apiHash}")]
        public IHttpActionResult PutDirectors(string email, string apiHash, List<Company_Director> Directors)
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

            //var url = HttpContext.Current.Request.UserHostName;//email;

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
            #region logic
            var cdLst = new List<Company_Director>();
            try
            {
                if (Directors != null && Directors.Count > 0)
                {
                    foreach (var item in Directors)
                    {
                        if (item.Address != null)
                        {
                            var ad = _addRep.FindBy(a => a.Id == item.Address.Id).FirstOrDefault();
                            if (ad != null)
                            {
                                ad.Address_1 = item.Address.Address_1;
                                ad.Address_2 = item.Address.Address_2;
                                ad.City = item.Address.City;
                                ad.Postal_Code = item.Address.Postal_Code;
                                ad.StateId = item.Address.StateId;
                                ad.Country_Id = item.Address.Country_Id;

                                _addRep.Edit(ad);
                                _addRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                            }
                            else
                            {
                                ad = new Address();
                                ad.Address_1 = item.Address.Address_1;
                                ad.Address_2 = item.Address.Address_2;
                                ad.City = item.Address.City;
                                ad.Postal_Code = item.Address.Postal_Code;
                                ad.StateId = item.Address.StateId;
                                ad.Country_Id = item.Address.Country_Id;

                                _addRep.Add(ad);
                                _addRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                                item.Address_Id = ad.Id;
                                item.Address = ad;
                            }

                        }

                        var dr = _compDRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                        if (dr != null)
                        {
                            dr.Address_Id = item.Address_Id;
                            dr.Company_Id = item.Company_Id;
                            dr.FirstName = item.FirstName;
                            dr.LastName = item.LastName;
                            dr.Nationality = item.Nationality;
                            dr.Telephone = item.Telephone;

                            _compDRep.Edit(dr);
                            _compDRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                            dr.Address = item.Address;
                            cdLst.Add(dr);
                        }

                    }
                }
                else
                {

                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "Address Model cannot be empty"
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
