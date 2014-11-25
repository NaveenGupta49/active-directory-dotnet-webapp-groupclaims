﻿using System;
using System.Web;

//The following libraries were added to this sample.
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

//The following libraries were defined and added to this sample.
using WebAppGroupClaimsDotNet.Utils;

namespace WebAppGroupClaimsDotNet
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            //Configure OpenIDConnect, register callbacks for OpenIDConnect Notifications
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = ConfigHelper.ClientId,
                    Authority = ConfigHelper.Authority,
                    PostLogoutRedirectUri = ConfigHelper.PostLogoutRedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthorizationCodeReceived = context =>
                        {
                            try
                            {
                                ClientCredential credential = new ClientCredential(ConfigHelper.ClientId, ConfigHelper.AppKey);
                                string userObjectId = context.AuthenticationTicket.Identity.FindFirst(Globals.ObjectIdClaimType).Value;
                                AuthenticationContext authContext = new AuthenticationContext(ConfigHelper.Authority, new TokenDbCache(userObjectId));
                                AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(
                                    context.Code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, ConfigHelper.GraphResourceId);
                            }
                            catch (AdalException e)
                            {
                                context.HandleResponse();
                                context.Response.Redirect("/Error/ShowError?errorMessage=" + e.Message + "&signIn=true");
                            }
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}