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
    public class BranchFilterModel
    {
        public List<State> States  { get; set; }
        public List<vZone> Zones { get; set; }
        public List<vBranch> Branches { get; set; }
    }
}