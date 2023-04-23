using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Entities;

namespace ELPS.Domain.Abstract
{
    public interface ILockedOutUserRepository : IGenericRepository<LockedOutUser>
    {
        /// <summary>
        /// return a boolean indicating whether the user is locked out
        /// </summary>
        /// <param name="userId">The IdentityUser Id</param>
        /// <returns></returns>
        bool IsUserLockedOut(string userId);
    }
}
