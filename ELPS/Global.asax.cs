using AutoMapper;
using ELPS.App_Start;
using ELPS.Crawler;
using ELPS.Helpers;
using ELPS.Infrastructure;
using ELPS.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace ELPS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private CurrentLog _currentUser;

        public static CurrentLog CurrentUser{ get;set;}
        

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();


            // Manually installed WebAPI 2.2 after making an MVC project.
            GlobalConfiguration.Configure(WebApiConfig.Register); // NEW way

            Mapper.Initialize(c => c.AddProfile<MappingProfile>());

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory());
            //NinjectWebCommon.RegisterNinject(GlobalConfiguration.Configuration);


            PaymentCrawler.StartPaymentCrawler("admin@example.com", ":1");
            PaymentCrawler.StartLicenseExpiryCrawler("admin@example.com", ":1");
            PaymentCrawler.StartLicenseExpiryReportCrawler("admin@example.com", ":1");
        }
    }
}
