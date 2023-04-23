using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.Concrete;

namespace ELPS.Domain.Concrete
{
    public class EFCompany_Key_StaffRepository : GenericRepository<ELPSContext, Company_Key_Staff>, ICompany_Key_StaffRepository
    {
    }
}
