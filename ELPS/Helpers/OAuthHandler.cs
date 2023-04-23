using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace ELPS.Helpers
{
    public class OAuthHandler
    {
        /// <summary>
        /// Method that initiates the Azure AD OAuth2 login sequence.
        /// The method will issue a 302 Redirect response to the browser to AADs login page.
        /// The AAD login page will complete the login, issue a redirect/POST to the redirectUrl
        /// This callback has the claims from AAD
        /// </summary>
        /// <param name="Request">HttpRequest object</param>
        /// <param name="Response">HttpResponse object</param>
        /// <param name="redirectUri">Url to redirecto to after Azure AD login form is done</param>
        ///

        private string login_Credentials;
        public string Login_Credentials
        {
            get { return login_Credentials; }
            set { login_Credentials = value; }
        }

        public static void Login_azure(HttpRequest Request, HttpResponse Response, string redirectUri)
        {
            // Get user that logged in

            HttpContext.Current.Session["usernameORemail2"] = Request.RequestContext.HttpContext.User.Identity.Name;

            // Request.RequestContext.HttpContext.User.Identity.Name
            // get stuff
            string nonce = System.Guid.NewGuid().ToString();
            string authUrl = "https://login.microsoftonline.com/common/oauth2/authorize";
            string clientID = ConfigurationManager.AppSettings["ClientId"];
            if (string.IsNullOrEmpty(clientID))
                throw new ArgumentNullException("clientID", "ClientID must be specified in Web.Config as aad.clientid under AppSettings");
            string clientSecret = ConfigurationManager.AppSettings["clientsecret"];
            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException("clientSecret", "clientSecret must be specified in Web.Config as aad.clientsecret under AppSettings");
            /*string appIdUri = ConfigurationManager.AppSettings["aad.appiduri"];
            if (string.IsNullOrEmpty(appIdUri))
                throw new ArgumentNullException("appIdUri", "clientSecret must be specified in Web.Config as aad.appiduri under AppSettings");*/
            redirectUri = GetRedirectUrl(Request, redirectUri);
            if (string.IsNullOrEmpty(redirectUri))
                throw new ArgumentNullException("redirectUri", "redirectUri must be specified in Web.Config as aad.appiduri under AppSettings");
            // build url for AAD auth and redirect to ourself
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}?", authUrl);
            sb.AppendFormat("redirect_uri={0}", Uri.EscapeDataString(redirectUri));
            sb.AppendFormat("&nonce={0}", nonce);
            sb.AppendFormat("&authorizationURL={0}", Uri.EscapeDataString(authUrl));
            sb.AppendFormat("&callbackURL={0}", Uri.EscapeDataString(redirectUri));
            sb.AppendFormat("&clientID={0}", clientID);
            sb.AppendFormat("&clientSecret={0}", Uri.EscapeDataString(clientSecret));
            sb.AppendFormat("&identifierField=openid_identifier");
            sb.AppendFormat("&oidcIssuer={0}", Uri.EscapeDataString("https://sts.windows.net/{tenantid}/"));
            sb.AppendFormat("&responseType=id_token");
            sb.AppendFormat("&revocationURL={0}", Uri.EscapeDataString("https://login.microsoftonline.com/common/oauth2/logout"));
            sb.AppendFormat("&scopeSeparator=%20");
            sb.AppendFormat("&tokenInfoURL=");
            sb.AppendFormat("&tokenURL={0}", Uri.EscapeDataString("https://login.microsoftonline.com/common/oauth2/token"));
            sb.AppendFormat("&userInfoURL={0}", Uri.EscapeDataString("https://login.microsoftonline.com/common/openid/userinfo"));
            sb.AppendFormat("&response_mode=form_post");
            sb.AppendFormat("&response_type=id_token");
            sb.AppendFormat("&scope=openid");
            sb.AppendFormat("&client_id={0}", clientID);
            sb.AppendFormat("&state={0}", nonce);
            // redirect to auth via AAD (and then redirect back to ourself)
            Response.Redirect(sb.ToString(), true);
        }
        /// <summary>
        /// Method that initiates the Azure AD OAuth2 logout sequence. The method will issue a 302 Redirect response to the browser
        /// </summary>
        /// <param name="Request">HttpRequest object</param>
        /// <param name="Response">HttpResponse object</param>
        /// <param name="redirectUri">Url to redirecto to after Azure AD logout is done</param>
        public static void Logout(HttpRequest Request, HttpResponse Response, string redirectUri)
        {
            // make auth cookie be expired

            // HttpCookie cookie = Request.Cookies.Get(FormsAuthentication.FormsCookieName);
            //  cookie.Expires = DateTime.Now.AddDays(-2); // back-date 2 days, since just -1 day may not work in all WW cases
            //  Response.Cookies.Add(cookie);

            // redirectUri = "https://workprogram.dpr.gov.ng/login_azure.aspx";

            redirectUri = "https://workprogram.dpr.gov.ng/";

            // call AAD and tell it we logout
            redirectUri = GetRedirectUrl(Request, redirectUri);
            string clientID = ConfigurationManager.AppSettings["aad.clientid"];
            string authUrl = "https://login.microsoftonline.com/{clientid}/oauth2/logout";
            authUrl = authUrl.Replace("{clientid}", clientID);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}?", authUrl);
            sb.AppendFormat("post_logout_redirect_uri={0}", Uri.EscapeDataString(redirectUri));
            // redirect via AAD
            Response.Redirect(sb.ToString(), true);
        }

        /// <summary>
        /// Fixes up the redirectUri in case being blank
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="redirectUri"></param>
        /// <returns></returns>
        private static string GetRedirectUrl(HttpRequest Request, string redirectUri)
        {
            // make string redirectUri NULL so that the one defined in the webconfig can be returned.

            // redirectUri = null;
            // if blank, construct url to default web app page
            if (!string.IsNullOrWhiteSpace(redirectUri))
            {
                redirectUri = string.Format("{0}{1}{2}{3}", Request.Url.Scheme, Uri.SchemeDelimiter, Request.Url.Authority, redirectUri);
            }
            // if still blank, then get from Web.Config
            if (string.IsNullOrWhiteSpace(redirectUri))
            {
                redirectUri = ConfigurationManager.AppSettings["aad.redirecturl"];
            }
            // if blank, construct url to default web app page
            if (string.IsNullOrWhiteSpace(redirectUri))
            {
                redirectUri = string.Format("{0}{1}{2}", Request.Url.Scheme, Uri.SchemeDelimiter, Request.Url.Authority);
            }

            return redirectUri;
        }
        public static void LoginCallback(HttpRequest Request, HttpResponse Response, string redirectUri)
        {
            // AAD does a http POST (via the browser) back to us after auth has happened. We pickup the "id_token" key in the post-body and store it in a cookie

            // we shouln't already be Auth'd and we need the "id_token" part in the body
            //  if (Request.IsAuthenticated) return;
            if (!Request.Form.AllKeys.Contains("id_token")) return;

            // decode shit
            string value = Request.Form.Get("id_token");
            JObject id_token = JwtDecode(value);
            // UserPrincipalNme, ie a fancy word for the original e-mail address you have in ActiveDirectory
            //string upn = id_token.GetValue("upn").ToString();
            string upn = id_token.GetValue("email").ToString();

            HttpContext.Current.Session["usernameORemail"] = upn;
            DateTime expireTime = GetExpireTime(id_token);
            SetUserPrincipal(id_token);

            // create the cookie and store the JWT token in the UserData attrribute so we can pick it up
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, upn, DateTime.UtcNow, expireTime, false, id_token.ToString(), FormsAuthentication.FormsCookiePath);
            string encryptedCookie = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedCookie);
            cookie.Expires = expireTime;
            Response.Cookies.Add(cookie);

            // redirect to ourself
            redirectUri = GetRedirectUrl(Request, redirectUri);
            Response.Redirect(redirectUri, true);
        }
        /// <summary>
        /// Decode the JWT authentication data
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static JObject JwtDecode(string token)
        {
            var parts = token.Split('.');
            var header = parts[0];
            var payload = parts[1];
            byte[] crypto = Base64UrlDecode(parts[2]);

            var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
            var headerData = JObject.Parse(headerJson);
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            var payloadData = JObject.Parse(payloadJson);

            return payloadData;
        }
        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
        /// <summary>
        /// handles the setup of the HttpContext.User.Identity object in Global.asax.cs Application_AuthenticateRequest
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Response"></param>
        public static void GlobalApplication_AuthenticateRequest(HttpRequest Request, HttpResponse Response)
        {
            // if not already auth'd and we have the aspnet auth cookie, hook up the principal if cookie hasn't expired

            //HttpCookie currentUserCookie = HttpContext.Current.Request.Cookies["currentUser"];
            //HttpContext.Current.Response.Cookies.Remove("currentUser");
            //currentUserCookie.Expires = DateTime.Now.AddDays(-10);
            //currentUserCookie.Value = null;
            //HttpContext.Current.Response.SetCookie(currentUserCookie);


            if (!Request.IsAuthenticated && Request.Cookies.AllKeys.Contains(FormsAuthentication.FormsCookieName))
            {
                HttpCookie cookie = Request.Cookies.Get(FormsAuthentication.FormsCookieName);
                if (cookie != null)
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                    if (!string.IsNullOrWhiteSpace(ticket.UserData))
                    {
                        var id_token = JObject.Parse(ticket.UserData);
                        DateTime expireTime = GetExpireTime(id_token);
                        if (DateTime.UtcNow < expireTime)
                        {
                            SetUserPrincipal(id_token);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts the JWP exp(ire) from Unix Epoch time to .net DateTime
        /// </summary>
        /// <param name="id_token"></param>
        /// <returns></returns>
        private static DateTime GetExpireTime(JObject id_token)
        {
            long exp = long.Parse(id_token.GetValue("exp").ToString());
            DateTime expireTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            expireTime = expireTime.AddSeconds(exp);
            return expireTime;
        }
        /// <summary>
        /// Convertsa a JWT token and creates a Claims Principal
        /// </summary>
        /// <param name="id_token"></param>
        private static void SetUserPrincipal(JObject id_token)
        {
            //string upn = id_token.GetValue("upn").ToString();
            string upn = id_token.GetValue("email").ToString();

            // login_Credentials = upn.ToString();
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session["usernameORemail"] = upn;
        

            List<Claim> claims = new List<Claim>
                        {
                              new Claim(ClaimTypes.Email, upn )
                            , new Claim(ClaimTypes.Upn, upn )
                            , new Claim( "http://schemas.microsoft.com/identity/claims/objectidentifier", id_token.GetValue("oid").ToString() )
                            , new Claim(ClaimTypes.Surname, id_token.GetValue("family_name").ToString() )
                            , new Claim(ClaimTypes.GivenName, id_token.GetValue("given_name").ToString() )
                            , new Claim(ClaimTypes.Name, id_token.GetValue("unique_name").ToString() )
                            , new Claim("name", id_token.GetValue("name").ToString() )
                            , new Claim("iss", id_token.GetValue("iss").ToString() )
                            , new Claim("nbf", id_token.GetValue("nbf").ToString() )
                            , new Claim("exp", id_token.GetValue("exp").ToString() )
                            , new Claim("aud", id_token.GetValue("aud").ToString() )
                            , new Claim(ClaimTypes.NameIdentifier, id_token.GetValue("sub").ToString() )
                            , new Claim("ipaddr", id_token.GetValue("ipaddr").ToString() )
                            , new Claim("http://schemas.microsoft.com/identity/claims/tenantid", id_token.GetValue("tid").ToString() )
                            , new Claim("ver", id_token.GetValue("ver").ToString() )
                        };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);
            HttpContext.Current.User = principal;
            Thread.CurrentPrincipal = principal; // updates
        }
    }
}