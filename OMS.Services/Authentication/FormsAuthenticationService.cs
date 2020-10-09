using OMS.Core;
using OMS.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;

namespace OMS.Services.Authentication
{
    public class FormsAuthenticationService : IAuthenticationService
    {
        #region ctor
        private readonly IWorkContext _workContext;
        private readonly AppSettings _appSettings;
        public FormsAuthenticationService(IWorkContext workContext, IOptionsMonitor<AppSettings> options)
        {
            _workContext = workContext;
            _appSettings = options.CurrentValue;
        }
        #endregion

        public void SignIn(string userName)
        {
            var claims = new Claim[] { new Claim(ClaimTypes.Name, userName) };
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "OMSCookies"));
            _workContext.CurrentHttpContext.SignInAsync("OMSCookies", userPrincipal, new AuthenticationProperties
            {
                //ExpiresUtc = DateTime.UtcNow.AddMinutes(_appSettings.CookieTimeout),
                ExpiresUtc = DateTime.UtcNow.AddHours(_appSettings.CookieTimeout),
                IsPersistent = _appSettings.CookieIsPersistent,
                AllowRefresh = false,
            });
        }

        public void SignOut()
        {
            _workContext.CurrentHttpContext.SignOutAsync();
        }
    }
}
