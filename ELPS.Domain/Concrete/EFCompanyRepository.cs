using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.Concrete;

namespace ELPS.Domain.Concrete
{
    public class EFCompanyRepository : GenericRepository<ELPSContext, Company>, ICompanyRepository
    {
        public bool CompanyExists(string companyName, string RegistrationNo)
        {
            throw new NotImplementedException();
        }

        public Company GetCompany(string companyName, string RegistrationNo)
        {
           return Context.companies.Where(C => 
                (C.RC_Number.ToLower().Trim() == RegistrationNo.Trim().ToLower())|| (C.Name.ToLower().Trim() == companyName.Trim().ToLower())
                ).FirstOrDefault();
        }
    }
}
