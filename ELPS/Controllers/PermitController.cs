using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.ViewDTOs;
using ELPS.Helpers;
using ELPS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    [Authorize]
    public class PermitController : Controller
    {
        #region Repository
        IStateRepository _statRep;
        IPermitCategoryRepository _permCatRep;
        ILicenseRepository _licenseRep;
        IPermitRepository _permitRep;
        IvPermitRepository _vPermitRep;
        IAppIdentityRepository _appIdRep;
        ChartHelper _chartHelper;
        IvZoneRepository _vZoneRep;
        IvBranchRepository _vBranchRep;
        IvZoneStateRepository _vZoneStateRep;

        #endregion

        public PermitController(ILicenseRepository license, IPermitRepository permit, IvPermitRepository vPermit, IAppIdentityRepository appIdRep,
            IPermitCategoryRepository permCatRep, IStateRepository statRep, IvZoneRepository vZoneRep, IvBranchRepository vBranchRep,
            IvZoneStateRepository vZoneStateRep)
        {
            _vZoneStateRep = vZoneStateRep;
            _vBranchRep = vBranchRep;
            _vZoneRep = vZoneRep;
            _statRep = statRep;
            _permCatRep = permCatRep;
            _vPermitRep = vPermit;
            _permitRep = permit;
            _licenseRep = license;
            _appIdRep = appIdRep;

            _chartHelper = new ChartHelper();
        }

        // GET: License
        public ActionResult Index(string startDate, string endDate, int? license, string category, string filterby, int? filterparam) //string location)
        {

            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
            ViewBag.SD = sd;
            ViewBag.ED = ed;

            int p = filterparam.GetValueOrDefault(0);
            ViewBag.param = p;
            ViewBag.by = filterby;

            BranchFilterModel filter = new BranchFilterModel();
            filter.States = _statRep.FindBy(a => a.CountryId == 156).ToList();
            filter.Zones = _vZoneRep.GetAll().ToList();
            filter.Branches = _vBranchRep.GetAll().ToList();
            ViewBag.Filter = filter;

            var _license = new List<AppIdentity> { new AppIdentity { Id = 0, ShortName = "All Licenses" } };
            _license.AddRange(_appIdRep.FindBy(a => a.IsActive && (!a.OfficeUse.HasValue || !a.OfficeUse.Value)).ToList());
            ViewBag.licenses = new SelectList(_license, "Id", "ShortName", license);
            ViewBag.license = license == null ? 0 : license;
            var categories = new List<PermitCategory>{ new PermitCategory { AppIdentityId = 0, Id = 0, Name = "All Categories" } };
            if (license != null && license > 0)
            {
                categories.AddRange(_permCatRep.FindBy(a => a.AppIdentityId > 0 && a.AppIdentityId == license.Value).ToList());
            }
            else
            {
                categories.AddRange(_permCatRep.FindBy(a => a.AppIdentityId > 0).ToList());
            }
            ViewBag.categories = new SelectList(categories, "Name", "Name", category);
            ViewBag.category = category;
            //ViewBag.states = new SelectList(_statRep.GetAll().ToList(), "Name", "Name", location);// _permCatRep.GetAll().ToList();
            //ViewBag.location = location;

            List<vPermit> permits = new List<vPermit>();

            return View(permits);
        }

        [HttpPost]
        public ActionResult Index(string coyName)
        {
            ViewBag.licenses = new SelectList(_appIdRep.GetAll().ToList(), "Id", "ShortName");            
            ViewBag.categories = new SelectList(_permCatRep.GetAll().ToList(), "Name", "Name");            
            ViewBag.states = new SelectList(_statRep.GetAll().ToList(), "Name", "Name");
            ViewBag.CompanyName = coyName;
            ViewBag.ByCompany = true;
            List<vPermit> permits = new List<vPermit>();
            return View(permits);
        }

        public ActionResult CompanyPermits(int id)
        {
            var vpermits = _vPermitRep.FindBy(a => a.Company_Id == id).ToList();

            return View(vpermits);
        }

        public ActionResult ViewPermit(int id)
        {
            return RedirectToAction("ViewExtPermit", new { id = id });


            //try
            //{
            //    var pm = _vPermitRep.FindBy(a => a.Id == id).FirstOrDefault();
            //    var hash = PaymentRef.getHash(pm.Id.ToString() + pm.OrderId);
            //    //get permit link
            //    var app = _appIdRep.FindBy(a => a.Id == pm.LicenseId).FirstOrDefault();
            //    //compose external url
            //    var lnk = string.Format("{0}?id={1}&hash={2}", app.PermitLink, pm.Id, hash);

            //    return Redirect(lnk);
            //}
            //catch (Exception)
            //{
            //    return View("Error");
            //}
        }

        public ActionResult AjaxifyPermit(JQueryDataTableParamModel param, string startDate, string endDate, int license, string category, string filterby, int? filterparam)
        {
            DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
            DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

            IEnumerable<vPermit> allPermits;// = new List<vPermit>(); // _invoiceRep.FindBy(C => C.Status == "Paid").ToList();

            if (license == 0)
            {
                if (string.IsNullOrEmpty(category))
                {
                    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed);
                    //if (string.IsNullOrEmpty(location))
                    //{
                    

                    //}
                    //else
                    //{

                    //    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.StateName==location);
                    //}
                }
                else
                {
                    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.CategoryName.ToLower() == category.ToLower());
                    //if (string.IsNullOrEmpty(location))
                    //{
                        
                    //}
                    //else
                    //{

                    //    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);
                  
                    //}
                }
            }
            else
            {
                if (string.IsNullOrEmpty(category))
                {
                    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.LicenseId == license);
                    //if (string.IsNullOrEmpty(location))
                    //{
                        
                    //}
                    //else
                    //{
                    //    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.LicenseId == license && a.StateName == location);
                   
                    //}
                }
                else
                {
                    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower());
                    //if (string.IsNullOrEmpty(location))
                    //{

                    //    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower());
                    //}
                    //else
                    //{

                    //    allPermits = _vPermitRep.FindBy(a => a.Date_Issued >= sd && a.Date_Issued <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);
                  
                    //}
                }
            }

            // Filter By area
            allPermits = FilterPermits(filterby, filterparam.GetValueOrDefault(0), allPermits);
            
            IEnumerable<vPermit> filteredPermit;
            var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<vPermit, string> orderFunction = (c => sortColIndex == 1 ? c.Permit_No :
               sortColIndex == 2 ? c.RRR : sortColIndex == 3 ? c.CompanyName
                 : sortColIndex == 4 ? c.LicenseShortName : sortColIndex == 5 ? c.CategoryName.Trim()
                : sortColIndex == 6 ? c.Date_Issued.ToString() : c.Date_Expire.ToString()); // sortColIndex == 6 ? : c.Invoice_open_date.ToString());

            var sortDirection = Request["sSortDir_0"];
            List<vPermit> returnedPermit = new List<vPermit>();

            #region Select and Sort
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var ps = param.sSearch.ToLower();//_vPermitRep.FindBy(
                filteredPermit = allPermits.Where(a => a.Permit_No.ToLower().Contains(ps) || a.CompanyName.ToLower().Contains(ps) ||
                    a.LicenseShortName.ToLower().Contains(ps) || a.CategoryName.ToLower().Contains(ps) || a.Date_Issued.ToString().ToLower().Contains(ps) ||
                    a.Date_Expire.ToString().ToLower().Contains(ps)).ToList();

                if (sortDirection.ToLower() == "asc")
                {
                    filteredPermit = filteredPermit.OrderBy(orderFunction);
                }
                else
                {
                    filteredPermit = filteredPermit.OrderByDescending(orderFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filteredPermit = allPermits.OrderBy(orderFunction);
                }
                else
                {
                    filteredPermit = allPermits.OrderByDescending(orderFunction);
                }
            }
            #endregion

            returnedPermit = filteredPermit.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            var result = from c in returnedPermit
                         select new[] { 
                             c.Permit_No, 
                             c.RRR, 
                             c.CompanyName, 
                             c.LicenseShortName, 
                             c.CategoryName, 
                             c.Date_Issued.ToShortDateString(),
                             c.Date_Expire.ToShortDateString(),  
                             c.Id.ToString(), 
                             c.Expired.ToString() 
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allPermits.Count(),
                iTotalDisplayRecords = filteredPermit.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);

        }

        private IEnumerable<vPermit> FilterPermits(string filterby, int filterparam,  IEnumerable<vPermit> allPermits)
        {
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
                                switch (fds.Count())
                                {
                                    case 1:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName);
                                        break;
                                    case 2:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower());
                                        break;
                                    case 3:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower());
                                        break;
                                    case 4:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower());
                                        break;
                                    case 5:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower());
                                        break;
                                    case 6:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower());
                                        break;
                                    case 7:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower());
                                        break;
                                    case 8:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower());
                                        break;
                                    case 9:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower());
                                        break;
                                    case 10:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower());
                                        break;
                                    case 11:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower());
                                        break;
                                    case 12:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower());
                                        break;
                                    case 13:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower() || a.StateName.ToLower() == fds[12].StateName.ToLower());
                                        break;
                                    case 14:
                                        allPermits = allPermits.Where(a => a.StateName.ToLower() == fds[0].StateName.ToLower() || a.StateName.ToLower() == fds[1].StateName.ToLower() || a.StateName.ToLower() == fds[2].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[3].StateName.ToLower() || a.StateName.ToLower() == fds[4].StateName.ToLower() || a.StateName.ToLower() == fds[5].StateName.ToLower() || a.StateName.ToLower() == fds[6].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[7].StateName.ToLower() || a.StateName.ToLower() == fds[8].StateName.ToLower() || a.StateName.ToLower() == fds[9].StateName.ToLower() || a.StateName.ToLower() == fds[10].StateName.ToLower()
                                        || a.StateName.ToLower() == fds[11].StateName.ToLower() || a.StateName.ToLower() == fds[12].StateName.ToLower() || a.StateName.ToLower() == fds[13].StateName.ToLower());
                                        break;
                                    default:
                                        allPermits = null;
                                        break;
                                }
                            }
                            else
                            {
                                allPermits = null;
                            }
                            break;
                        }
                    case "fd":
                        {
                            var brch = _vBranchRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                            if (brch != null)
                            {
                                allPermits = allPermits.Where(a => a.StateName.ToLower() == brch.StateName.ToLower());
                            }
                            else
                            {
                                allPermits = null;
                            }
                            break;
                        }
                    case "st":
                        {
                            var st = _statRep.FindBy(a => a.Id == filterparam).FirstOrDefault();
                            allPermits = allPermits.Where(a => a.StateName.ToLower() == st.Name.ToLower());
                            break;
                        }
                    default:
                        break;
                }
            }

            return allPermits;
        }

        public ActionResult AjaxifyPermitByCompany(JQueryDataTableParamModel param, string coyName)
        {
            IEnumerable<vPermit> allPermits;
            allPermits = _vPermitRep.FindBy(a => a.CompanyName.ToLower().Contains(coyName.ToLower()));
            
            IEnumerable<vPermit> filteredPermit;
            var sortColIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

            Func<vPermit, string> orderFunction = (c => sortColIndex == 1 ? c.Permit_No :
               sortColIndex == 2 ? c.RRR : sortColIndex == 3 ? c.CompanyName
                 : sortColIndex == 4 ? c.LicenseShortName : sortColIndex == 5 ? c.CategoryName.Trim()
                : sortColIndex == 6 ? c.Date_Issued.ToString() : c.Date_Expire.ToString()); // sortColIndex == 6 ? : c.Invoice_open_date.ToString());

            var sortDirection = Request["sSortDir_0"];
            List<vPermit> returnedPermit = new List<vPermit>();

            #region Select and Sort
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                var ps = param.sSearch.ToLower();//_vPermitRep.FindBy(
                filteredPermit = allPermits.Where(a => a.Permit_No.ToLower().Contains(ps) || a.CompanyName.ToLower().Contains(ps) ||
                    a.LicenseShortName.ToLower().Contains(ps) || a.CategoryName.ToLower().Contains(ps) || a.Date_Issued.ToString().ToLower().Contains(ps) ||
                    a.Date_Expire.ToString().ToLower().Contains(ps)).ToList();

                if (sortDirection.ToLower() == "asc")
                {
                    filteredPermit = filteredPermit.OrderBy(orderFunction);
                }
                else
                {
                    filteredPermit = filteredPermit.OrderByDescending(orderFunction);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    filteredPermit = allPermits.OrderBy(orderFunction);
                }
                else
                {
                    filteredPermit = allPermits.OrderByDescending(orderFunction);
                }
            }
            #endregion

            returnedPermit = filteredPermit.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

            var result = from c in returnedPermit
                         select new[] {
                             c.Permit_No,
                             c.RRR,
                             c.CompanyName,
                             c.LicenseShortName,
                             c.CategoryName,
                             c.Date_Issued.ToShortDateString(),
                             c.Date_Expire.ToShortDateString(),
                             c.Id.ToString(),
                             c.Expired.ToString()
                         };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = allPermits.Count(),
                iTotalDisplayRecords = filteredPermit.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ViewExtPermit(int id)
        {
            var permit = _vPermitRep.FindBy(a => a.Id == id).FirstOrDefault();
            if (permit != null)
            {
                var app = _appIdRep.FindBy(a => a.Id == permit.LicenseId).FirstOrDefault();
                if (app != null)
                {
                    var hash = HashManager.GetHash(permit.Id.ToString() + app.AppId);
                    var lnk = string.Format("{0}?id={1}&code={2}", app.PermitLink, permit.Id, hash);
                    //var url = app.BaseUrl + "/Application/ExternalViewPermit?id=" + permit.Id + "&code=" + hash;

                    ViewBag.Link = lnk;
                    return View(permit);
                    //return Redirect(lnk);
                }
            }
            return View("Error");
        }

        [AllowAnonymous]
        public ActionResult VerifyPermit(string appId)
        {
            ViewBag.appId = appId;
            if (!string.IsNullOrEmpty(appId))
            {
                var app = _appIdRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                if (app != null)
                {
                    ViewBag.ShortName = app.ShortName;
                    ViewBag.FullName = app.LicenseName;
                }

            }
            return View("VerifyPermitView");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult VerifyPermit(VerifyPermitDTO model,string appId)
        {
            ViewBag.appId = appId;

            if (!ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(appId))
                {
                    var app = _appIdRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                    if (app != null)
                    {
                        ViewBag.ShortName = app.ShortName;
                        ViewBag.FullName = app.LicenseName;
                    }

                }
                TempData["ErrorList"] = new List<string> { "Please fill out the required field(s) correctly" };
                return View("VerifyPermitView");
            }

            ViewBag.PermitNo = model.LicenseNo;

            var permit = _vPermitRep.FindBy(a => a.Permit_No == model.LicenseNo).FirstOrDefault();

            if (permit != null)
            {
                var app = _appIdRep.FindBy(a => a.Id == permit.LicenseId).FirstOrDefault();
                if (app != null)
                {
                    var hash = HashManager.GetHash(permit.Id.ToString() + app.AppId);
                    var lnk = string.Format("{0}?id={1}&code={2}", app.PermitLink, permit.Id, hash);
                    //var url = app.BaseUrl + "/Application/ExternalViewPermit?id=" + permit.Id + "&code=" + hash;

                    ViewBag.Link = lnk;
                    return View("ViewExtPermit", permit);
                   // return Redirect(lnk);
                }
            }

            if (!string.IsNullOrEmpty(appId))
            {
                var app = _appIdRep.FindBy(a => a.PublicKey.ToString() == appId).FirstOrDefault();
                if (app != null)
                {
                    ViewBag.ShortName = app.ShortName;
                    ViewBag.FullName = app.LicenseName;
                }

            }
            ViewBag.Alert = new AlertModel()
            {
                AlertType = "failure",
                Title = "Permit Status",
                Message = "The Permit number you provided do not exist in our record, kindly check the permit number and try again."
            };


            return View("VerifyPermitView");
        }

        //[AllowAnonymous, HttpPost]
        //public ActionResult VerifyPermit(string id)
        //{
        //    ViewBag.PermitNo = id;
        //    var permit = _vPermitRep.FindBy(a => a.Permit_No == id).FirstOrDefault();
        //    if (permit != null)
        //    {
        //        var app = _appIdRep.FindBy(a => a.Id == permit.LicenseId).FirstOrDefault();
        //        if (app != null)
        //        {
        //            var hash = HashManager.GetHash(permit.Id.ToString() + app.AppId);
        //            var lnk = string.Format("{0}?id={1}&code={2}", app.PermitLink, permit.Id, hash);
        //            //var url = app.BaseUrl + "/Application/ExternalViewPermit?id=" + permit.Id + "&code=" + hash;

        //            ViewBag.Link = lnk;
        //            return View("ViewExtPermit", permit);
        //            //return Redirect(lnk);
        //        }
        //    }
        //    return View("ViewExtPermit");
        //}


    }
}