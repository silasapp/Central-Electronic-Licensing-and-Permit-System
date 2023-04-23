using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class FacilityVM
    {
        public string UniqueNo { get; set; }
        public string FacilityName { get; set; }
        public string Type { get; set; }  //Independent/Major marketer
        public FacilityCompany Company { get; set; }
        public AddressVM Address { get; set; }
    }

    public class FacilityCompany
    {
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public AddressVM Address { get; set; }
    }

    public class AddressVM
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}