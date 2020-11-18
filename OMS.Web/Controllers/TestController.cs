using Microsoft.AspNetCore.Mvc;
using OMS.Services.Products;

namespace OMS.Web.Controllers
{
    [Route("test")]
    public class TestController : Controller
    {
        private readonly IProductService productService;
        public TestController(IProductService productService)
        {
            this.productService = productService;
        }

        [HttpGet("syn/stock")]
        public IActionResult Index()
        {
            return Ok(productService.SynStock());
        }
    }
}
