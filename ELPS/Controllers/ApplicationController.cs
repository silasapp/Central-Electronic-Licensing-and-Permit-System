using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using ELPS.Domain;

namespace ELPS.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {

        IPermitCategoryRepository _permCatRep;
        IApplicationRepository _appRep;
        IvApplicationRepository _vAppRep;
        ICompanyRepository _compRep;
        IAppIdentityRepository _license;
        IStateRepository _stateRep;

        public ApplicationController(IvApplicationRepository vAppRep, IApplicationRepository appRep, IStateRepository stateRep,
            ICompanyRepository compRep, IAppIdentityRepository license, IPermitCategoryRepository permCatRep)
        {
            _stateRep = stateRep;
            _permCatRep = permCatRep;
            _appRep = appRep;
            _vAppRep = vAppRep;
            _compRep = compRep;
            _license = license;
        }

        // GET: Application
        //[Route(Name = "Application/{id:int?}")]
        public ActionResult Index(int? id, string startDate, string endDate, int? license, string category, string location)
        {
            if (id == null)
            {
                //if (string.IsNullOrEmpty(endDate))
                //{
                //    DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-30).Date : DateTime.Parse(startDate).Date;
                //    DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);
                //}
                ViewBag.licenses = new SelectList(_license.GetAll().ToList(), "Id", "ShortName", license);
                ViewBag.license = license == null ? 0 : license;
                if (license != null && license > 0)
                {
                    ViewBag.categories = new SelectList(_permCatRep.FindBy(a => a.AppIdentityId == license.Value).ToList(), "Name", "Name", category);
                }
                else
                {
                    ViewBag.categories = new SelectList(_permCatRep.GetAll().ToList(), "Name", "Name");
                }
                ViewBag.category = category;
                ViewBag.states = new SelectList(_stateRep.GetAll().ToList(), "Name", "Name", location);// _permCatRep.GetAll().ToList();
                ViewBag.location = location;
                ViewBag.SD = startDate; // sd;
                ViewBag.ED = endDate; // ed;
                return View("_aIndexall");
            }
            else
            {
                var app = _vAppRep.FindBy(a => a.Id == id).FirstOrDefault();
                if (app == null)
                {
                    TempData["status"] = "fail";
                    TempData["message"] = "Invalid application credentials";
                }
                else
                {
                    if (!string.IsNullOrEmpty(app.ApplicationItem))
                    {
                        app.ApplicationItems = JsonConvert.DeserializeObject<List<ApplicationItem>>(app.ApplicationItem);
                    }

                }
                return View("AppDetail", app);
            }
        }

        public ActionResult CompanyApplication(int id)
        {
            if (id <= 0)
            {
                return View("Error");
            }

            var apps = _appRep.FindBy(c => c.CompanyId == id).ToList();
            var licenses = _license.GetAll().ToList();
            foreach (var app in apps)
            {
                app.LicenseName = licenses.Where(c => c.Id == app.LicenseId).FirstOrDefault().ShortName;
            }

            return View(apps);
        }

        //[Authorize(Roles = "Admin,Support,Manager,Account, Director")]
        //public ActionResult ManageApplications(int? id, string s)
        //{
        //   // var usr = User.Identity.Name;


        //    if (id == null)
        //    {
        //        return View("_aIndexall");
        //    }
        //    else
        //    {
        //        var app = _vAppRep.FindBy(a => a.Id == id).FirstOrDefault();
        //        if (app == null)
        //        {
        //            TempData["status"] = "fail";
        //            TempData["message"] = "Invalid application credentials";
        //        }
        //        else
        //        {
        //            if (!string.IsNullOrEmpty(app.ApplicationItem))
        //            {
        //                app.ApplicationItems= JsonConvert.DeserializeObject<List<ApplicationItem>>(app.ApplicationItem); 
        //            }

        //        }
        //        return View("_adminAppDetail", app);
        //    }




        //}


        [Authorize(Roles = "Admin,Support,Manager,Account,Director,ManagerObserver, ITAdmin")]
        public ActionResult LazyIndex(JQueryDataTableParamModel param, string startDate, string endDate, int license, string category, string location)
        {
            try
            {
                IEnumerable<vApplication> allApplications;
                if (string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
                {
                    // No dates specified
                    if (license == 0)
                    {
                        if (string.IsNullOrEmpty(category))
                        {
                            if (string.IsNullOrEmpty(location))
                            {
                                allApplications = _vAppRep.GetAll();
                            }
                            else
                            {
                                allApplications = _vAppRep.FindBy(a => a.StateName == location);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(location))
                            {
                                allApplications = _vAppRep.FindBy(a => a.CategoryName.ToLower() == category.ToLower());
                            }
                            else
                            {
                                allApplications = _vAppRep.FindBy(a => a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(category))
                        {
                            if (string.IsNullOrEmpty(location))
                            {
                                allApplications = _vAppRep.FindBy(a => a.LicenseId == license);
                            }
                            else
                            {
                                allApplications = _vAppRep.FindBy(a => a.LicenseId == license && a.StateName == location);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(location))
                            {
                                allApplications = _vAppRep.FindBy(a => a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower());
                            }
                            else
                            {

                                allApplications = _vAppRep.FindBy(a => a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);

                            }
                        }
                    }
                }
                else
                {
                    // Dates specified (MM/DD/YYY)
                    DateTime sd = string.IsNullOrEmpty(startDate) ? DateTime.Parse("01/01/2017").Date : DateTime.Parse(startDate).Date;
                    DateTime ed = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate).Date.AddHours(23).AddMinutes(59);

                    if (license == 0)
                    {
                        if (string.IsNullOrEmpty(category))
                        {
                            if (string.IsNullOrEmpty(location))
                            {
                                allApplications = _vAppRep.FindBy(a => a.Date >= sd && a.Date <= ed);
                            }
                            else
                            {
                                allApplications = _vAppRep.FindBy(a => a.Date >= sd && a.Date <= ed && a.StateName == location);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(location))
                            {
                                allApplications = _vAppRep.FindBy(a => a.Date >= sd && a.Date <= ed && a.CategoryName.ToLower() == category.ToLower());
                            }
                            else
                            {
                                allApplications = _vAppRep.FindBy(a => a.Date >= sd && a.Date <= ed && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(category))
                        {
                            if (string.IsNullOrEmpty(location))
                            {
                                allApplications = _vAppRep.FindBy(a => a.Date >= sd && a.Date <= ed && a.LicenseId == license);
                            }
                            else
                            {
                                allApplications = _vAppRep.FindBy(a => a.Date >= sd && a.Date <= ed && a.LicenseId == license && a.StateName == location);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(location))
                            {
                                allApplications = _vAppRep.FindBy(a => a.Date >= sd && a.Date <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower());
                            }
                            else
                            {

                                allApplications = _vAppRep.FindBy(a => a.Date >= sd && a.Date <= ed && a.LicenseId == license && a.CategoryName.ToLower() == category.ToLower() && a.StateName == location);

                            }
                        }
                    }
                }

                IEnumerable<vApplication> filteredApplications;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]) + 1;

                var sortDirection = Request["sSortDir_0"];
                List<vApplication> displayedApplications = new List<vApplication>();

                if (!string.IsNullOrEmpty(param.sSearch))
                {
                    var p = param.sSearch.ToLower();
                    filteredApplications = allApplications.Where(C => 
                            (!string.IsNullOrEmpty(C.CompanyName) && C.CompanyName.ToLower().Contains(p)) || 
                            (!string.IsNullOrEmpty(C.LicenseShortName) && C.LicenseShortName.ToLower().Contains(p)) ||
                            (!string.IsNullOrEmpty(C.OrderId) && C.OrderId.ToLower().Contains(p)) || 
                            (!string.IsNullOrEmpty(C.RRR) && C.RRR.ToLower().Contains(p)) ||
                            (!string.IsNullOrEmpty(C.CategoryName) && C.CategoryName.ToLower().Contains(p)) ||
                            (!string.IsNullOrEmpty(C.Status) && C.Status.ToLower().Contains(p)));

                }
                else
                {
                    filteredApplications = allApplications;
                }
                if (sortDirection == "asc")
                {
                    //filteredApplications = allApplications.OrderBy(orderingFunction);
                    switch (sortColumnIndex)
                    {
                        case 1:
                            filteredApplications = filteredApplications.OrderBy(a => a.OrderId);
                            break;
                        case 2:
                            filteredApplications = filteredApplications.OrderBy(a => a.CompanyName);
                            break;
                        case 3:
                            filteredApplications = filteredApplications.OrderBy(a => a.LicenseShortName);
                            break;
                        case 4:
                            filteredApplications = filteredApplications.OrderBy(a => a.RRR);
                            break;
                        case 5:
                            filteredApplications = filteredApplications.OrderBy(a => a.CategoryName);
                            break;
                        case 6:
                            filteredApplications = filteredApplications.OrderBy(a => a.Status);
                            break;
                        case 7:
                            filteredApplications = filteredApplications.OrderBy(a => a.Date);
                            break;
                        default:
                            filteredApplications = filteredApplications.OrderBy(a => a.OrderId);
                            break;
                    }
                }
                else
                {
                    //filteredApplications = allApplications.OrderByDescending(orderingFunction);
                    switch (sortColumnIndex)
                    {
                        case 1:
                            filteredApplications = filteredApplications.OrderByDescending(a => a.OrderId);
                            break;
                        case 2:
                            filteredApplications = filteredApplications.OrderByDescending(a => a.CompanyName);
                            break;
                        case 3:
                            filteredApplications = filteredApplications.OrderByDescending(a => a.LicenseShortName);
                            break;
                        case 4:
                            filteredApplications = filteredApplications.OrderByDescending(a => a.RRR);
                            break;
                        case 5:
                            filteredApplications = filteredApplications.OrderByDescending(a => a.CategoryName);
                            break;
                        case 6:
                            filteredApplications = filteredApplications.OrderByDescending(a => a.Status);
                            break;
                        case 7:
                            filteredApplications = filteredApplications.OrderByDescending(a => a.Date);
                            break;
                        default:
                            filteredApplications = filteredApplications.OrderByDescending(a => a.OrderId);
                            break;
                    }
                }
                

                displayedApplications = filteredApplications.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();

                var result = from c in displayedApplications
                             select new[] {c.OrderId, c.CompanyName, c.LicenseShortName,
                                c.RRR, c.CategoryName, c.Status,c.Date.ToString(), Convert.ToString(c.Id) };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = allApplications.Count(),
                    iTotalDisplayRecords = filteredApplications.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ActionResult getCategory(int id)
        {
            var cats = _permCatRep.FindBy(a => a.AppIdentityId == id && a.AppIdentityId > 0).ToList();

            return Json(cats, JsonRequestBehavior.AllowGet);
        }
    }
}