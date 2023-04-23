using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;

[assembly: OwinStartupAttribute(typeof(ELPS.Startup))]
namespace ELPS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
        
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    // Add framework services.
        //    services.AddCors(options =>
        //    {
        //        options.AddPolicy("AllowFromAll",
        //            builder => builder
        //            .WithMethods("GET")
        //            .AllowAnyOrigin()
        //            .AllowAnyHeader());
        //    });
        //}
    }
}
