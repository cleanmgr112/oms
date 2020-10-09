using OMS.Data.Domain;
using Microsoft.AspNetCore.Http;

namespace OMS.Core
{
    public interface IWorkContext
    {
        HttpContext CurrentHttpContext { get; }

        User CurrentUser { get; set; }
    }
}
