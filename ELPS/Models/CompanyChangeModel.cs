using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNet.Highcharts.Options;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;

namespace ELPS.Models
{
    public class CompanyChangeModel
    {
        public string Name { get; set; }
        public string RC_Number { get; set; }
        public string Business_Type { get; set; }
        public bool emailChange { get; set; }
        public int CompanyId { get; set; }
        public string NewEmail { get; set; }
    }
}