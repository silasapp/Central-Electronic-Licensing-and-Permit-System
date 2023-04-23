using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class NonCompanyUserModel
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string AppId { get; set; }
        public int ParentCoyId { get; set; }
        public string ParentCoyName { get; set; }
        public string ChildCoyName { get; set; }
        public string BizType { get; set; }
    }
}