using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Concrete
{
    public class EFDivisionsRepo : GenericRepository<ELPSContext, Division>, IDivisionRepo
    {
        public bool Exist(int id, string name)
        {
          return  Context.Divisions.Where(m => m.Id != id).Any(m => m.Name.ToLower() == name.ToLower());
        }

        public bool Exist(string name)
        {
            return Context.Divisions.Any(m => m.Name.ToLower() == name.ToLower());
        }
    }
}
