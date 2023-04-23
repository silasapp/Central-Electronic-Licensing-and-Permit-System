using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using ELPS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    [Authorize(Roles = "Admin, ITAdmin")]
    public class BranchController : Controller
    {
        //
        // GET: /Branch/
        IBranchRepository _branchRep;
        IvBranchRepository _vBranchRep;
        IZoneRepository _zoneRep;
        IvZoneRepository _vZoneRep;
        IZoneStateRepository _zoneStateRep;
        IStateRepository _stateRep;
        IvZoneStateRepository _vZoneStateRep;
        IvFD_FDRepository _fd2fdRep;

        public BranchController(IBranchRepository branchRep, IvBranchRepository vBranchRep, IZoneRepository zoneRep,
            IvZoneRepository vZoneRep, IZoneStateRepository zoneStateRep, IStateRepository state, IvFD_FDRepository fd2fdRep,
            IvZoneStateRepository vZoneStateRep)
        {
            _fd2fdRep = fd2fdRep;
            _vZoneStateRep = vZoneStateRep;
            _stateRep = state;
            _zoneStateRep = zoneStateRep;
            _vZoneRep = vZoneRep;
            _zoneRep = zoneRep;
            _vBranchRep = vBranchRep;
            _branchRep = branchRep;
        }

        
        public ActionResult Index()
        {
            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }

            List<Branch> branches = _branchRep.GetAll().ToList();
            return View(branches);
        }

        public ActionResult FieldOffices()
        {
            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }

            List<Branch> branches = _branchRep.FindBy(a => a.IsFieldOffice).ToList();
            return View(branches);
        }

        public ActionResult Create(bool? isField)
        {
            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }

            var states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            ViewBag.States = new SelectList(states, "Id", "Name");

            if (isField.GetValueOrDefault())
            {
                ViewBag.IsField = true;
                return View(new Branch() { IsFieldOffice = true });
            }
            return View();
        }

        [HttpPost]
        public ActionResult Create(Branch branches)
        {
            var states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            ViewBag.States = new SelectList(states, "Id", "Name");
            try
            {
                //if (ModelState.IsValid)
                //{
                    Branch branch = _branchRep.FindBy(C => C.Name.ToLower() == branches.Name.ToLower()).FirstOrDefault();
                    if (branch == null)
                    {
                        branches.Create_At = UtilityHelper.CurrentTime;
                        branches.LastEdit_At = UtilityHelper.CurrentTime;
                        _branchRep.Add(branches);
                        _branchRep.Save(User.Identity.Name, Request.UserHostAddress);

                        TempData["alertModel"] = new AlertModel() { AlertType = "success", Title = "Branch Alert", Message = "New Branch created successfully." };
                        
                        return RedirectToAction("Index");
                    }
                //}
                ViewBag.Alert = new AlertModel() { AlertType = "warning", Title = "Branch Alert", Message = "New Branch not created. Please confirm all fields are properly filled." };

                return View(branches);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                TempData["alertModel"] = new AlertModel() { AlertType = "warning", Title = "Branch Alert", Message = "New Branch could not be created. Please try again." };

                return RedirectToAction("Index");
            }
        }

        public ActionResult Details(int id)
        {
            Branch _branch = _branchRep.FindBy(c => c.Id == id).FirstOrDefault();
            if (_branch != null)
            {
                return View(_branch);
            }
            return View();
        }

        public ActionResult Edit(int id)
        {
            Branch _branch = _branchRep.FindBy(c => c.Id == id).FirstOrDefault();
            if (_branch != null)
            {
                return View(_branch);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Edit(Branch branch)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    branch.LastEdit_At = UtilityHelper.CurrentTime;
                    _branchRep.Edit(branch);
                    _branchRep.Save(User.Identity.Name, Request.UserHostAddress);
                    TempData["status"] = "pass";
                    TempData["Message"] = "Branch was edited";
                    return RedirectToAction("Index");
                }
                TempData["status"] = "warn";
                TempData["Message"] = "Fill out the form properly";
                return View(branch);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                TempData["status"] = "fail";
                TempData["Message"] = "Branch was not edited";
                return View(branch);
            }
        }

        public ActionResult Delete(int id)
        {
            if (id > 0)
            {
                Branch brc = _branchRep.FindBy(b => b.Id == id).FirstOrDefault();
                if (brc != null)
                {
                    return View(brc);
                }
            }
            TempData["status"] = "warn";
            TempData["Message"] = "Branch not Found";
            return RedirectToAction("Index");

        }

        [HttpPost]
        public ActionResult Delete(Branch model)
        {
            try
            {
                Branch _branch = _branchRep.FindBy(c => c.Id == model.Id).FirstOrDefault();
                if (_branch != null)
                {
                    _branchRep.Delete(_branch);
                    _branchRep.Save(User.Identity.Name, Request.UserHostAddress);
                    TempData["status"] = "pass";
                    TempData["Message"] = "Branch was deleted";
                    return RedirectToAction("Index");
                }
                return View();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                TempData["status"] = "fail";
                TempData["Message"] = "Branch was not deleted";
                return RedirectToAction("Index");
            }
        }

        #region branch zone mapping
        public ActionResult Zones()
        {
            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }

            var vZones = _vZoneRep.GetAll().ToList();
            foreach (var zone in vZones)
            {
                zone.CoveredStates = _vZoneStateRep.FindBy(a => a.ZoneId == zone.Id).ToList();
            }

            return View(vZones); // (zb);
        }

        public ActionResult CreateZone(int? id)
        {
            var branches = _branchRep.GetAll().ToList();
            ViewBag.Branch = new SelectList(branches, "Id", "Name");
            if(id != null && id > 0)
            {
                var model = _zoneRep.FindBy(a => a.Id == id).FirstOrDefault();
                return View(model);
            }
            return View();
        }

        [HttpPost]
        public ActionResult CreateZone(Zone model)
        {
            var branches = _branchRep.GetAll().ToList();
            ViewBag.Branch = new SelectList(branches, "Id", "Name");
            try
            {
                _zoneRep.Add(model);
                _zoneRep.Save(User.Identity.Name, Request.UserHostAddress);

                TempData["alertModel"] = new AlertModel() { AlertType = "success", Title = "Zone Alert", Message = "New Zone created successfully." };

                return RedirectToAction("Zones");
            }
            catch (Exception)
            {
                ViewBag.Alert = new AlertModel() { AlertType = "warning", Title = "Zone Alert", Message = "New Zone not created. Please try again." };

                return View();
            }
        }

        public ActionResult MapZoneToState(int? id)
        {
            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }

            var states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            var zones = _zoneRep.GetAll().ToList();
            var zoneState = _zoneStateRep.GetAll().ToList();
            List<State> useStates = new List<State>();



            //Filter states already mapped out
            foreach (var state in states)
            {
                var pick = zoneState.Where(a => a.StateId == state.Id).FirstOrDefault();
                if(pick == null)
                {
                    useStates.Add(state);
                }
            }
            ViewBag.State = useStates; // new SelectList(useStates, "Id", "Name");
            ViewBag.Zones = new SelectList(zones, "Id", "Name");
            
            return View();
        }

        [HttpPost]
        public ActionResult MapZoneToState(int id, params string[] stateIds) // model)
        {
            try
            {
                if (stateIds != null && stateIds.Any())
                {
                    var model = _zoneRep.FindBy(a => a.Id == id).FirstOrDefault();
                    foreach (var stateId in stateIds)
                    {
                        _zoneStateRep.Add(new ZoneState() { StateId = Convert.ToInt16(stateId), ZoneId = model.Id });
                    }
                    _zoneStateRep.Save(User.Identity.Name, Request.UserHostAddress);

                    TempData["alertModel"] = new AlertModel() { AlertType = "success", Title = "Zone Mapping", Message = "States Mapped to Zone successfully." };
                }
                else
                {
                    TempData["alertModel"] = new AlertModel() { AlertType = "warning", Title = "Zone Mapping", Message = "No State is selected for mapping. Please try again." };
                }
            }
            catch (Exception)
            {
                TempData["alertModel"] = new AlertModel() { AlertType = "warning", Title = "Zone Alert", Message = "New Zone not created. Please try again." };
            }

            return RedirectToAction("Zones");
        }

        public ActionResult FDtoFD()
        {
            if (TempData["alertModel"] != null)
            {
                ViewBag.Alert = (AlertModel)TempData["alertModel"];
            }

            var fdMaps = _fd2fdRep.GetAll().GroupBy(a => a.FDName).ToList();
            ViewBag.Model = fdMaps;
            return View();

            //var states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            //var zones = _zoneRep.GetAll().ToList();
            List<State> useStates = new List<State>();

            //Filter states already mapped out
            //foreach (var state in states)
            //{
            //    var pick = fdMaps.Where(a => a.FirstOrDefault().StateId == state.Id).FirstOrDefault();
            //    if (pick == null)
            //    {
            //        useStates.Add(state);
            //    }
            //}
            //ViewBag.State = useStates; // new SelectList(useStates, "Id", "Name");
            //ViewBag.Zones = new SelectList(zones, "Id", "Name");
        }

        public ActionResult AddFDtoFD()
        {
            var brs = _branchRep.FindBy(a => a.IsFieldOffice).ToList();
            ViewBag.FDs = new SelectList(brs, "Id", "Name");
            //Filter states already mapped out
            var fdMaps = _vZoneStateRep.GetAll().ToList();
            var states = _stateRep.FindBy(a => a.CountryId == 156).ToList();
            //List<State> useStates = new List<State>();
            //foreach (var state in states)
            //{
            //    var pick = fdMaps.Where(a => a.StateId == state.Id).FirstOrDefault();
            //    if (pick == null)
            //    {
            //        useStates.Add(state);
            //    }
            //}
            ViewBag.State = new SelectList(states.OrderBy(a => a.Name), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult AddFDtoFD(int fdId, int stateId)
        {
            AlertModel alert = new AlertModel();
            var znState = _zoneStateRep.FindBy(a => a.StateId == stateId).FirstOrDefault();

            if (znState != null && znState.FDId > 0)
            {
                alert = new AlertModel() { AlertType = "fail", Message = "State already mapped." };
                return RedirectToAction("FDtoFD");
            }
            try
            {

                //var znSt = _zoneStateRep.FindBy(a => a.FDId == fdId).FirstOrDefault();
                if(znState == null)
                {
                    //It has not been done b4
                    var newMap = new ZoneState();
                    newMap.FDId = fdId;
                    newMap.StateId = stateId;

                    var branch = _branchRep.FindBy(a => a.Id == fdId).FirstOrDefault();
                    // Get my Zone from mappings
                    var zones = _vZoneRep.GetAll().ToList();
                    foreach (var z in zones)
                    {
                        z.CoveredStates = _vZoneStateRep.FindBy(a => a.ZoneId == z.Id).ToList();
                    }
                    var zn = zones.Where(a => a.CoveredStates.Select(s => s.StateId).Contains(branch.StateId)).FirstOrDefault();
                    var _znSt = zn.CoveredStates.Where(a => a.StateId == branch.StateId).FirstOrDefault();
                    newMap.ZoneId = _znSt.ZoneId;
                    _zoneStateRep.Add(newMap);
                    _zoneStateRep.Save(User.Identity.Name, Request.UserHostAddress);
                    alert = new AlertModel() { AlertType = "pass", Message = "FD to FD mapping done successfully!" };
                }
                else
                {
                    znState.FDId = fdId;

                    _zoneStateRep.Edit(znState);
                    _zoneStateRep.Save(User.Identity.Name, Request.UserHostAddress);
                    alert = new AlertModel() { AlertType = "pass", Message = "FD to FD mapping modified successfully!" };
                }

                TempData["alertModel"] = alert;
            }
            catch (Exception)
            {
                alert = new AlertModel() { AlertType = "fail", Message = "Cannot perform FD to FD mapping." };
                TempData["alertModel"] = alert;
            }
            return RedirectToAction("FDtoFD");
        }

        //public ActionResult EditZone(int id)
        //{
        //    var zm = _vZMapRep.FindBy(a => a.Id == id).FirstOrDefault();
        //    if (zm != null)
        //    {
        //        ViewBag.state = _stateRep.GetAll().ToList();
        //        ViewBag.branch = _branchRep.GetAll().ToList();

        //        return View(zm);
        //    }
        //    return View("Error");
        //}

        //[HttpPost]
        //public ActionResult Edit(vZoneMapping model)
        //{
        //    //var zm = _zMapRep.FindBy(a => a.Id == model.Id).FirstOrDefault();
        //    if (true) //zm != null)
        //    {
        //        //zm.State_Id = model.State_Id;
        //        //zm.Branch_Id = model.Branch_id;
        //        //_zMapRep.Add(zm);
        //        //_zMapRep.Save(User.Identity.Name, Request.UserHostAddress);
        //        return RedirectToAction("Index");
        //    }
        //    return View("Error");
        //}

        #endregion
    }
}