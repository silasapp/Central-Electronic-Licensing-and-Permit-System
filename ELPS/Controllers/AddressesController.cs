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
    [RoutePrefix("api/Address")]
    public class AddressesController : ApiController
    {


        ICompanyRepository _compRep;
        IAppIdentityRepository _appIdRep;
        IAddressRepository _addRep;
        IvAddressRepository _vAddRep;
        IStateRepository _stateRep;
        public AddressesController(ICompanyRepository compRep, IAppIdentityRepository appIdRep, IAddressRepository addRep, IvAddressRepository vAddRep,
            IStateRepository stateRep)
        {
            _compRep = compRep;
            _appIdRep = appIdRep;
            _addRep = addRep;
            _vAddRep = vAddRep;
            _stateRep = stateRep;

        }

        /// <summary>
        /// Get a company's addresss
        /// </summary>
        /// <param name="CompId">Company Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(Licen) secrete Key</param>
        /// <returns>Returns a list of Adresses that Belongs to this Company. both the regitared and Operational Address</returns>
        [ResponseType(typeof(List<vAddress>))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult GetAddress(int CompId, string email, string apiHash)
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
            
            var adlst = new List<vAddress>();
            //Get the Company first
            var comp = _compRep.FindBy(a => a.Id == CompId).FirstOrDefault();
            if (comp == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Company not found"
                });
            }

            var add1 = _vAddRep.FindBy(a => a.Id == comp.Operational_Address_Id).FirstOrDefault();
            if (add1 != null)
            {
                add1.Type = "operational";
                adlst.Add(add1);
            }
            var add2 = _vAddRep.FindBy(a => a.Id == comp.Registered_Address_Id).FirstOrDefault();
            if (add2 != null)
            {
                add2.Type = "registered";
                adlst.Add(add2);
            }


            //if (adlst.Count <= 0)
            //{
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
            //    {
            //        ReasonPhrase = "there is no Address on this Company yet"
            //    });
            //}

            return Ok(adlst);
        }


        /// <summary>
        /// Get a particular addresss by the Address Id
        /// </summary>
        /// <param name="Id">Address Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(Licen) secrete Key</param>
        /// <returns>Returns the Address model</returns>
        [ResponseType(typeof(vAddress))]
        [Route("ById/{Id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetAddressById(int Id, string email, string apiHash)
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


            var add1 = _vAddRep.FindBy(a => a.Id == Id).FirstOrDefault();

            if (add1 == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Item does not Exist"
                });
            }

            return Ok(add1);
        }
        /// <summary>
        /// get states by country Id
        /// </summary>
        /// <param name="Id"> this is the Country Id</param>
        /// <param name="email">app(Solution Email Id)</param>
        /// <param name="apiHash">SHA512 hash of emailAppId</param>
        /// <returns>Returns a list of the states that belongs to a Country </returns>
        [ResponseType(typeof(List<State>))]
        [Route("states/{Id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetstatesById(int Id, string email, string apiHash)
        {
            // id
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

            if (!HashManager.compair(email, app.AppId, apiHash))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "App has been denied Access, Contact NUPRC Dev"
                });
            }

            #endregion


            var states = _stateRep.FindBy(a => a.CountryId == Id).ToList();

            if (states == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    ReasonPhrase = "Item does not Exist"
                });
            }

            return Ok(states);
        }
        /// <summary>
        /// Posting a company list of Address
        /// </summary>
        /// <param name="CompId">Company Id</param>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(Licen) secrete Key</param>
        /// <param name="Adds">this is List of Address Model that is being saved</param>
        /// <returns>Return List of Address Model that has been saved</returns>
        [ResponseType(typeof(List<Address>))]
        [Route("{CompId:int}/{email}/{apiHash}")]
        public IHttpActionResult PostAddress(int CompId, string email, string apiHash, List<Address> Adds)
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
            try
            {
                ////Get the Company first
                var comp = _compRep.FindBy(a => a.Id == CompId).FirstOrDefault();
                if (comp == null)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        ReasonPhrase = "Item does not Fond"
                    });
                }

                var adlst = new List<Address>();
                if (Adds != null && Adds.Count > 0)
                {
                    comp.Operational_Address_Id = Adds[0].Id;
                    comp.Registered_Address_Id = Adds[0].Id;
                    foreach (var item in Adds)
                    {
                        var ad = new Address();
                        ad.Address_1 = item.Address_1;
                        ad.Address_2 = item.Address_2;
                        ad.City = item.City;
                        ad.Country_Id = item.Country_Id;
                        ad.Postal_Code = item.Postal_Code;
                        ad.StateId = item.StateId;
                        ad.Type = item.Type;

                        _addRep.Add(ad);
                        _addRep.Save(email, HttpContext.Current.Request.UserHostAddress);

                        if (item.Type.ToLower() == "operational")
                        {
                            comp.Operational_Address_Id = ad.Id;
                        }
                        if (item.Type.ToLower() == "registered")
                        {
                            comp.Registered_Address_Id = ad.Id;
                        }
                        adlst.Add(ad);
                    }

                    //comp.Registered_Address_Id = Adds.Where(a => a.Type.ToLower() == "registered").FirstOrDefault().Id;
                    _compRep.Edit(comp);
                    _compRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                }
                else
                {

                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "Address Model cannot be empty"
                    });
                }
                //get 

                if (adlst.Count <= 0)
                {
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        ReasonPhrase = "there is no Address on this Company yet"
                    });
                }

                return Ok(adlst);
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

        /// <summary>
        /// Update a company list of Address
        /// </summary>
        /// <param name="email">the Application(License) Email</param>
        /// <param name="apiHash">this is SHA512 hash of email and Application(License) secrete Key</param>
        /// <param name="Adds">This is List of Address Model that is being Updated</param>
        /// <returns>Return List of Address Model that has been Updated. Please Note:
        /// that if the Address is not found, we will use the Model and Create a new address</returns>
        [ResponseType(typeof(List<Address>))]
        [Route("{email}/{apiHash}")]
        public IHttpActionResult PutAddress(string email, string apiHash, List<Address> Adds)
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
            try
            {
                if (Adds != null && Adds.Count > 0)
                {
                    var nAdds = new List<Address>();
                    foreach (var item in Adds)
                    {
                        var ad = _addRep.FindBy(a => a.Id == item.Id).FirstOrDefault();
                        if (ad != null)
                        {
                            ad.Address_1 = item.Address_1;
                            ad.Address_2 = item.Address_2;
                            ad.City = item.City;
                            ad.Postal_Code = item.Postal_Code;
                            ad.StateId = item.StateId;

                            _addRep.Edit(ad);
                            _addRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                            nAdds.Add(ad);
                        }
                        else
                        {
                            ad = new Address();
                            ad.Address_1 = item.Address_1;
                            ad.Address_2 = item.Address_2;
                            ad.City = item.City;
                            ad.Country_Id = item.Country_Id;
                            ad.Postal_Code = item.Postal_Code;
                            ad.StateId = item.StateId;

                            _addRep.Add(ad);
                            _addRep.Save(email, HttpContext.Current.Request.UserHostAddress);
                            nAdds.Add(ad);
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
                //var frst = Adds.FirstOrDefault();
                //var adds = _addRep.FindBy(a => a.Country_Id == frst.Country_Id).ToList();

                return Ok(Adds);

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
