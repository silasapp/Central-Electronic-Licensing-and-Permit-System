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
    public class AlertModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string AlertType { get; set; }
    }

    public class LogModel
    {
        public string LogBody { get; set; }
    }
}