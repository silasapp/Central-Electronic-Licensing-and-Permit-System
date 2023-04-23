using System;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Owin;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.Google;
using ELPS.Models;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Host.SystemWeb;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using System.Security.Claims;
using System.Security.Principal;
using System.Collections.Concurrent;
using ELPS.Domain.Entities;

namespace ELPS
{
    public partial class Startup
    {
        // The Client ID is used by the application to uniquely identify itself to Microsoft identity platform.
        string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
        string clientSecret = System.Configuration.ConfigurationManager.AppSettings["clientsecret"];

        // RedirectUri is the URL where the user will be redirected to after they sign in.
        string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];

        // Tenant is the tenant ID (e.g. contoso.onmicrosoft.com, or 'common' for multi-tenant)
        static string tenant = System.Configuration.ConfigurationManager.AppSettings["Tenant"];

        // Authority is the URL for authority, composed of the Microsoft identity platform and the tenant name (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0)
        string authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, System.Configuration.ConfigurationManager.AppSettings["Authority"], tenant);


        //ELPSContext db = null;
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ApplicationCookie);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                //AuthenticationType = "Cookies",
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                CookieManager = new SystemWebCookieManager(),
                LoginPath = new PathString("/Account/Login"),
                
                //AuthenticationMode = AuthenticationMode.Passive,
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }


                ////AuthenticationType = "Application",
                //LoginPath = new PathString(Paths.LoginPath),
                //LogoutPath = new PathString(Paths.LogoutPath),



            });
            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    // Sets the ClientId, authority, RedirectUri as obtained from web.config
                    ClientId = clientId,
                    Authority = authority,
                    RedirectUri = redirectUri,
                    //ClientSecret = clientSecret,
                    // PostLogoutRedirectUri is the page that users will be redirected to after sign-out. In this case, it is using the home page
                    PostLogoutRedirectUri = redirectUri,
                    Scope = OpenIdConnectScope.OpenIdProfile,
                    //AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                    // ResponseType is set to request the code id_token - which contains basic information about the signed-in user
                    ResponseType = OpenIdConnectResponseType.CodeIdToken,
                    CookieManager = new SystemWebCookieManager(),
                    // ValidateIssuer set to false to allow personal and work accounts from any organization to sign in to your application
                    // To only allow users from a single organizations, set ValidateIssuer to true and 'tenant' setting in web.config to the tenant name
                    // To allow users from only a list of specific organizations, set ValidateIssuer to true and use ValidIssuers parameter
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true, // This is a simplification
                        
                    },
                    // OpenIdConnectAuthenticationNotifications configures OWIN to send notification of failed authentications to OnAuthenticationFailed method
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = OnAuthenticationFailed,
                    }
                }
            );

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            //            {
            //                AuthorizeEndpointPath = new PathString("/OAuth/Authorize"),
            //                TokenEndpointPath = new PathString("/OAuth/Token"),
            //                ApplicationCanDisplayErrors = true,
            //#if DEBUG
            //                AllowInsecureHttp = true,
            //#endif
            //                // Authorization server provider which controls the lifecycle of Authorization Server
            //                Provider = new OAuthAuthorizationServerProvider
            //                {
            //                    OnValidateClientRedirectUri = ValidateClientRedirectUri,
            //                    OnValidateClientAuthentication = ValidateClientAuthentication,
            //                    OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials,
            //                    OnGrantClientCredentials = GrantClientCredetails
            //                },

            //                // Authorization code provider which creates and receives authorization code
            //                AuthorizationCodeProvider = new AuthenticationTokenProvider
            //                {
            //                    OnCreate = CreateAuthenticationCode,
            //                    OnReceive = ReceiveAuthenticationCode,
            //                },

            //                // Refresh token provider which creates and receives referesh token
            //                RefreshTokenProvider = new AuthenticationTokenProvider
            //                {
            //                    OnCreate = CreateRefreshToken,
            //                    OnReceive = ReceiveRefreshToken,
            //                }
            //            });




            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            //app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            //Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "24ad25d8-9231-4735-bc89-b9785604ecb6",
            //    clientSecret: "GP4xBj-4.oTvkU~2GnZCeTCb5zd5_wn~Kf");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});




        }

        /// <summary>
        /// Handle failed authentication requests by redirecting the user to the home page with an error in the query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }
        #region user Token

        //private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        //{
        //    db = new ELPSContext();
        //    var ids = db.AppIdentities.FirstOrDefault(a => a.Id == context.ClientId);
        //    if (ids!=null)
        //    {
        //        context.Validated(ids.Url);
        //    }
        //    //if (context.ClientId == Clients.Client1.Id)
        //    //{
        //    //    context.Validated(Clients.Client1.RedirectUrl);
        //    //}
        //    //else if (context.ClientId == Clients.Client2.Id)
        //    //{
        //    //    context.Validated(Clients.Client2.RedirectUrl);
        //    //}
        //    return Task.FromResult(0);
        //}

        //private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        //{
        //    db = new ELPSContext();
        //    var ids = db.AppIdentities.FirstOrDefault(a => a.Id == context.ClientId);

        //    string clientId;
        //    string clientSecret;
        //    if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
        //        context.TryGetFormCredentials(out clientId, out clientSecret))
        //    {
        //        if (clientId == ids.Id && clientSecret == ids.AppId)
        //        {
        //            context.Validated();
        //        }
        //        //else if (clientId == Clients.Client2.Id && clientSecret == Clients.Client2.Secret)
        //        //{
        //        //    context.Validated();
        //        //}
        //    }
        //    return Task.FromResult(0);
        //}

        //private Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        //{
        //    var identity = new ClaimsIdentity(new GenericIdentity(context.UserName, OAuthDefaults.AuthenticationType), context.Scope.Select(x => new Claim("urn:oauth:scope", x)));

        //    context.Validated(identity);

        //    return Task.FromResult(0);
        //}

        //private Task GrantClientCredetails(OAuthGrantClientCredentialsContext context)
        //{
        //    var identity = new ClaimsIdentity(new GenericIdentity(context.ClientId, OAuthDefaults.AuthenticationType), context.Scope.Select(x => new Claim("urn:oauth:scope", x)));

        //    context.Validated(identity);

        //    return Task.FromResult(0);
        //}


        //private readonly ConcurrentDictionary<string, string> _authenticationCodes =
        //    new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        //private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        //{
        //    context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
        //    _authenticationCodes[context.Token] = context.SerializeTicket();
        //}

        //private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        //{
        //    string value;
        //    if (_authenticationCodes.TryRemove(context.Token, out value))
        //    {
        //        context.DeserializeTicket(value);
        //    }
        //}

        //private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        //{
        //    context.SetToken(context.SerializeTicket());
        //}

        //private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        //{
        //    context.DeserializeTicket(context.Token);
        //}

        #endregion
    }

}