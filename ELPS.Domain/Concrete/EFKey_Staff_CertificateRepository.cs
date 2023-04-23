using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.Concrete;

namespace ELPS.Domain.Concrete
{
    public class EFKey_Staff_CertificateRepository : GenericRepository<ELPSContext, Key_Staff_Certificate>, IKey_Staff_CertificateRepository
    {
    }
}
