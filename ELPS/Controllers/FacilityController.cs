using ELPS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELPS.Controllers
{
    public class FacilityController : Controller
    {
        #region Repositories
        IFacilityRepository _facRep;
        IvFacilityRepository _vFacRep;
        IFacilityDocumentRepository _facDocRep;
        #endregion

        public FacilityController(IFacilityRepository facRep, IvFacilityRepository vFacRep, IFacilityDocumentRepository facDocRep)
        {
            _vFacRep = vFacRep;
            _facRep = facRep;
            _facDocRep = facDocRep;
        }

        // GET: Facility
        public ActionResult Index(int CompanyId)
        {
            var coyFacs = _vFacRep.FindBy(a => a.CompanyId == CompanyId).ToList();
            var allCoyFacDocs = _facDocRep.FindBy(a => a.Company_Id == CompanyId).ToList();

            foreach (var fac in coyFacs)
            {
                fac.FacilityDocuments = allCoyFacDocs.Where(a => a.FacilityId == fac.Id).ToList();
            }

            return View(coyFacs);
        }

    }
}