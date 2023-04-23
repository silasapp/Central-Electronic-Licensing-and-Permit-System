using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;

namespace ELPS.Domain.Concrete
{
    public class EFLicenseRepository : GenericRepository<ELPSContext, License>, ILicenseRepository
    {
    }

}
