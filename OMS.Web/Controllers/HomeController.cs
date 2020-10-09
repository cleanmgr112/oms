using OMS.Services.Authentication;
using OMS.Services.Account;
using OMS.WebCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class HomeController : BaseController
    {
        #region ctor
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        public HomeController(IUserService userService, IAuthenticationService authenticationService)
        {
            _userService = userService;
            _authenticationService = authenticationService;
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }       

        public IActionResult Info()
        {
            ViewBag.UserName = WorkContext.CurrentUser.ToModel().UserName;
            return View();
        }

        [Permit]
        [UserAnonymous]
        public IActionResult Data()
        {
            return Error();
        }

        [HttpPost]
        public IActionResult GetMenuList()
        {
            var options = HttpContext.RequestServices.GetService(typeof(IOptionsMonitor<SiteMap>)) as IOptionsMonitor<SiteMap>;
            if (options != null)
            {
                var sitemap = options.CurrentValue;
                if (sitemap.Nodes.Count > 0)
                {
                    var data = sitemap.Nodes.Where(i => string.IsNullOrEmpty(i.Permit) || true);
                    return Success(data);
                }
            }
            return Error();
        }
    }
}
