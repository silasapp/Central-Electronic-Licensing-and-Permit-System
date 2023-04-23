using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.Concrete;

namespace ELPS.Domain.Concrete
{
    public class EFLockedOutUserRepository : GenericRepository<ELPSContext, LockedOutUser>, ILockedOutUserRepository
    {
        public bool IsUserLockedOut(string userId)
        {
           return  Context.LockedOutUsers.Any(a => a.UserId == userId && a.Resolved == false);
        }
    }
}
