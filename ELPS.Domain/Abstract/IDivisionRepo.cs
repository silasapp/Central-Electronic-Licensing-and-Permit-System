using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Abstract
{
    public interface IDivisionRepo : IGenericRepository<Division>
    {
        bool Exist(int id, string name);
        bool Exist(string name);
    }
}
