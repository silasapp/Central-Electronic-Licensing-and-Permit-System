using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Entities;

namespace ELPS.Domain.Abstract
{
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        /// <summary>
        /// returns a boolean indicating whether the company exist by checking for matching registration Number and company name
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="RegistrationNo"></param>
        /// <returns></returns>
        bool CompanyExists(string companyName,string RegistrationNo);

        /// <summary>
        /// returns the comapany datails using the registration Number and company name
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="RegistrationNo"></param>
        /// <returns></returns>
        Company GetCompany(string companyName, string RegistrationNo);
    }
}
