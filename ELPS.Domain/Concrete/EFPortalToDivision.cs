using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Concrete
{
    public class EFPortalToDivision : GenericRepository<ELPSContext, PortalToDivision>, IPortalToDivision
    {
        public IEnumerable<AppIdentity> GetPortalsForDivision(int id)
        {
            return   Context.PortalToDivisions.Where(m=>m.DivisionId==id)
                    .Select(m => m.Portal)
                    .Where(a => a.IsActive && (a.OfficeUse == null || a.OfficeUse.Value == false))
                    .AsEnumerable(); 
        }

        public void DeletePortal(int id)
        {
            var portalRelation=Context.PortalToDivisions.Where(m => m.PortalId == id).ToList();
            Context.PortalToDivisions.RemoveRange(portalRelation);
            Context.SaveChanges();
        }

        public void AddPortalToDivisions(int portalId, List<int> divisionId)
        {
            if (divisionId != null)
            {
                foreach (var division in divisionId)
                {
                    Context.PortalToDivisions.Add(new PortalToDivision { PortalId = portalId, DivisionId = division });
                    Context.SaveChanges();
                }
            }
        
            
        }
    }
}
