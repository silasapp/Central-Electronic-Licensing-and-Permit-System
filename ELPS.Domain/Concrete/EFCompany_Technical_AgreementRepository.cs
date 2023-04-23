using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.Concrete;

namespace ELPS.Domain.Concrete
{
    public class EFCompany_Technical_AgreementRepository : GenericRepository<ELPSContext, Company_Technical_Agreement>, ICompany_Technical_AgreementRepository
    {
    }
}
