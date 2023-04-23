using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ELPS.Models
{
    public class PermitLookupModel
    {
        public string Number { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string Email { get; set; }
        public string ExpiryDate { get; set; }
    }

}