using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Abstract
{
    public interface IPortalToDivision : IGenericRepository<PortalToDivision>
    {
        /// <summary>
        /// this returns an enumerable list of all portal model(AppIdentity) for a particular division
        /// </summary>
        /// <param name="id">Division Id</param>
        /// <returns></returns>
        IEnumerable<AppIdentity> GetPortalsForDivision(int id);

        /// <summary>
        /// This deletes every portal to division relationship 
        /// </summary>
        /// <param name="id">Portal Id</param>
        void DeletePortal(int id);

        /// <summary>
        /// This add a a single portal to multiple division
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="divisionId">Division Id</param>
        void AddPortalToDivisions(int portalId,List<int> divisionId);
    }
}
