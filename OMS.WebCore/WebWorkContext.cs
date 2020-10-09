using Microsoft.AspNetCore.Http;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Services.Account;

namespace OMS.WebCore
{
    public class WebWorkContext : IWorkContext
    {
        #region ctor
        private User _user;
        private readonly IUserService _userService;
        public WebWorkContext(IHttpContextAccessor httpContext, IUserService userService)
        {
            CurrentHttpContext = httpContext.HttpContext;
            _userService = userService;
        }
        #endregion

        public HttpContext CurrentHttpContext { get; }

        public User CurrentUser
        {
            get
            {
                if (_user == null)
                {
                    if (CurrentHttpContext.User.Identity.IsAuthenticated)
                    {
                        _user = _userService.GetByUserName(CurrentHttpContext.User.Identity.Name);
                        return _user;
                    }
                }
                else
                {
                    return _user;
                }
                return null;
            }
            set
            {
                _user = value;
            }
        }
    }
}
