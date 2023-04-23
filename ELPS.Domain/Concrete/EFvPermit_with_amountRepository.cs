using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.Concrete;

namespace ELPS.Domain.Concrete
{
    public class EFvPermit_with_amountRepository : GenericRepository<ELPSContext, vPermit_with_amount>, IvPermit_with_amountRepository
    {
    }
}
