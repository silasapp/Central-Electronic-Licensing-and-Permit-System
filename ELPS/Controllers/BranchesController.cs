using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace ELPS.Controllers
{
    [RoutePrefix("api/Branch")]
    public class BranchesController : ApiController
    {
        IBranchRepository _branchRep;
        IvBranchRepository _vBranchRep;
        IZoneRepository _zoneRep;
        IvZoneRepository _vZoneRep;
        IZoneStateRepository _zoneStateRep;
        IStateRepository _stateRep;
        IvZoneStateRepository _vZoneStateRep;
        WebApiAccessHelper accessHelper;
        IAppIdentityRepository _appIdRep;

        public BranchesController(IBranchRepository branchRep, IvBranchRepository vBranchRep, IZoneRepository zoneRep,
            IvZoneRepository vZoneRep, IZoneStateRepository zoneStateRep, IStateRepository state,
            IvZoneStateRepository vZoneStateRep, IAppIdentityRepository appId)
        {
            _appIdRep = appId;
            accessHelper = new WebApiAccessHelper(appId);
            _vZoneStateRep = vZoneStateRep;
            _stateRep = state;
            _zoneStateRep = zoneStateRep;
            _vZoneRep = vZoneRep;
            _zoneRep = zoneRep;
            _vBranchRep = vBranchRep;
            _branchRep = branchRep;
        }

        /// <summary>
        /// Gets All Branches on the Portal
        /// </summary>
        /// <param name="email"></param>
        /// <param name="apiHash"></param>
        /// <returns></returns>
        [ResponseType(typeof(List<vBranch>))]
        [Route("All/{email}/{apiHash}")]
        public IHttpActionResult GetAllBranches(string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if(check != null && check.Status == false)
            {
                return Ok(check);
            }

            List<vBranch> branches = _vBranchRep.GetAll().ToList();
            return Ok(branches);
        }

        [ResponseType(typeof(vBranch))]
        [Route("{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetBranch(int id, string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            var branch = _vBranchRep.FindBy(a => a.Id == id).FirstOrDefault();
            
            return Ok(branch);
        }


        [ResponseType(typeof(vBranch))]
        [Route("GetBranchByState/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetBranchByState(int id, string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            //  var branch = _vBranchRep.FindBy(a => a.Id == id).FirstOrDefault();

            var fdOffice = _vZoneStateRep.FindBy(C => C.StateId == id).FirstOrDefault();

            var branch = _vBranchRep.FindBy(a => a.Id == fdOffice.FDId).FirstOrDefault();

            return Ok(branch);
        }

        [ResponseType(typeof(List<vZone>))]
        [Route("AllZones/{email}/{apiHash}")]
        public IHttpActionResult GetAllZones(string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            List<vZone> zones = _vZoneRep.GetAll().ToList();
            return Ok(zones);
        }

        [ResponseType(typeof(vZone))]
        [Route("Zone/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetZone(int id, string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            var zone = _vZoneRep.FindBy(a => a.Id == id).FirstOrDefault();
            return Ok(zone);
        }

        [ResponseType(typeof(vZone))]
        [Route("ZoneByName/{zoneName}/{email}/{apiHash}")]
        public IHttpActionResult GetZone(string zoneName, string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            var zone = _vZoneRep.FindBy(z => z.Name.ToLower().Trim() == zoneName.ToLower().Trim()).FirstOrDefault();
            return Ok(zone);
        }

        [ResponseType(typeof(List<vZoneState>))]
        [Route("StatesInZone/{id:int}/{email}/{apiHash}")]
        public IHttpActionResult GetStatesInZone(int id, string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            var zoneStates = _vZoneStateRep.FindBy(a => a.ZoneId == id).ToList();
            return Ok(zoneStates);
        }

        [ResponseType(typeof(vZone))]
        [Route("StatesInZone/ByZoneName/{zoneName}/{email}/{apiHash}")]
        public IHttpActionResult GetStatesInZone(string zoneName, string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            var zoneStates = _vZoneStateRep.FindBy(a => a.ZoneName.ToLower().Trim() == zoneName.Trim().ToLower()).ToList();
            return Ok(zoneStates);
        }


        [ResponseType(typeof(vZone))]
        [Route("ZoneMapping/{email}/{apiHash}")]
        public IHttpActionResult GetZoneMapping(string email, string apiHash)
        {
            var check = accessHelper.CanAccess(email, apiHash);
            if (check != null && check.Status == false)
            {
                return Ok(check);
            }

            var zones = _vZoneRep.GetAll().ToList();
            foreach (var z in zones)
            {
                z.CoveredStates = _vZoneStateRep.FindBy(a => a.ZoneId == z.Id).ToList();
                z.CoveredFieldOffices = new List<vBranch>();
                foreach (var st in z.CoveredStates)
                {
                    if (st.FDId <= 0)
                    {
                        var fds = _vBranchRep.FindBy(a => a.StateId == st.StateId && a.IsFieldOffice && a.Id != z.BranchId).ToList();
                        z.CoveredFieldOffices.AddRange(fds);
                    }
                    else
                    {
                        var pp = _vBranchRep.FindBy(a => a.Id == st.FDId && a.IsFieldOffice && a.Id != z.BranchId).ToList();
                        z.CoveredFieldOffices.AddRange(pp);
                    }
                    
                }
            }

            return Ok(zones);
        }

    }
}
