using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Concrete
{
    public class EFvZoneStateRepository : GenericRepository<ELPSContext, vZoneState>, IvZoneStateRepository
    {
    }
}
