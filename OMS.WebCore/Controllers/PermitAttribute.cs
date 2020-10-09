using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace OMS.Web.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class PermitAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (true)//实现判断逻辑
            {
                if (context.HttpContext.Request.IsAjaxRequest())//ajax
                {
                    context.Result = new JsonResult(new { code = 401 });
                    return;
                }
                context.Result = new RedirectResult("/home/notpermit");
                return;
            }
        }
    }
}
