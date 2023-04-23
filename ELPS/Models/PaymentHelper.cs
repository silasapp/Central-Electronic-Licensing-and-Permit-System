using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNet.Highcharts.Options;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using ELPS.Domain.Entities;

namespace ELPS.Models
{
    public class PaymentHelper
    {
        public Company Company { get; set; }
        public List<Application> Applications { get; set; }
    }
    
}