
using Microsoft.AspNetCore.Mvc;
using OMS.Services.StockRemid;
using OMS.Services.StockRemid.StockRemindDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.Web.Controllers
{

    /// <summary>
    /// 模板设置 
    /// </summary>
    [Route("remind")]
    public class StockRemindController : Controller
    {
        private readonly StockRemindService stockRemind;
        private readonly StockTitleService stockTitle;
        private readonly RemindTemplate template;
        public StockRemindController(StockRemindService stockRemind, StockTitleService stockTitle, RemindTemplate template)
        {
            this.stockRemind = stockRemind;
            this.stockTitle = stockTitle;
            this.template = template;
        }


        #region 视图
        [HttpGet("index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("title")]
        public IActionResult Title()
        {
            return View();
        }
        #endregion

        #region  template接口
        /// <summary>
        /// 获取商品
        /// </summary>
        /// <returns></returns>
        [HttpPost("product")]
        public IActionResult GetProduct(SearchProductDto product, int page, int limit)
        {
            return Ok(new { data = template.templateSearch.Search(product, out int count, page, limit), count = count });
        }

        /// <summary>
        /// 获取商品类型
        /// </summary>
        /// <returns></returns>
        [HttpGet("product/type")]
        public IActionResult GetProductType()
        {
            return Ok(stockRemind.GetProductType());
        }

        /// <summary>
        /// 模板开关
        /// </summary>
        /// <returns></returns>
        [HttpPost("template/swtich")]
        public async Task<IActionResult> TemplateSwtich(templateSwtichDto swtich, TemplateSaleDto key)
        {
            var templateCode = string.Empty;
            var flag = template.TemplateSwtich(swtich, out templateCode);
            key.TemplateCode = templateCode;
            var res = await ((TemplateSet)template.TemplateSet).Set(key);
            return Ok(flag && res);
        }


        /// <summary>
        /// 模板设置
        /// </summary>
        [HttpPost("template/set")]
        [DisableRequestSizeLimit]
        public IActionResult TempalteUpdate(List<TemplateSaleDto> key, List<UserDto> user, string templateTitle, int remindStock)
        {
            if (key.Count == 0)
                return BadRequest();
            return Ok(template.TemplateSet.Set(new SetDto() { Key = key, User = user, TemplateTitle = templateTitle, RemindStock = remindStock }));
        }

        /// <summary>
        /// 获取user
        /// </summary>
        /// <returns></returns>
        [HttpGet("user")]
        public IActionResult GetUser()
        {
            return Ok(stockRemind.GetUser());
        }

        /// <summary>
        /// 获取user and title by templatecode
        /// </summary>
        /// <param name="templateCode"></param>
        /// <returns></returns>
        [HttpGet("title/user/{templateCode}")]
        public IActionResult GetTItleAndUser(string templateCode)
        {
            var data = stockRemind.GetTitleAndUser(templateCode);
            return Ok(data);
        }

        /// <summary>
        /// 消息预警
        /// </summary>
        /// <returns></returns>
        [HttpGet("template/warn")]
        public async Task<IActionResult> templateWarn()
        {
            var task = stockRemind.UserMesaage();
            var task1 = stockRemind.TemplateValid(stockTitle);
            await task1; await task;
            return Ok();
        }
        #endregion

        #region title接口
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        [HttpPost("template/title/search")]
        public IActionResult Search(DateTime? min, DateTime? max, string productCode)
        {
            return Ok(stockTitle.Search(min, max, productCode));
        }

        [HttpGet("template/{productCode?}")]
        public IActionResult GetTitle(string productCode, int page = 1, int limit = 10)
        {
            return Ok(new { template = stockTitle.GetTemplate(out int count, productCode, page: page, limit: limit), templateCount = count });
        }

        [HttpGet("title/{min?}/{max?}")]
        public IActionResult GetTemplate(DateTime? min, DateTime? max, int page = 1, int limit = 5)
        {
            return Ok(new { title = stockTitle.ISearch.FirstOrDefault(c => c is TitleSearch).Search(new SearchDto() { Min = min, Max = max }, out int count, page, limit), titleCount = count });
        }

        /// <summary>
        /// 修改标题的状态
        /// </summary>
        /// <returns></returns>
        [HttpPut("statu/alter")]
        public async Task<IActionResult> Statu(string titleId)
        {
            return Ok(await stockTitle.Statu(titleId));
        }

        /// <summary>
        /// 取消预警
        /// </summary>
        [HttpGet("template/cancel/{templateCode}")]
        public async Task<IActionResult> Cancel(string templateCode)
        {
            return Ok(await stockTitle.Cancel(templateCode));
        }

        /// <summary>
        /// 导出excel
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost("export")]
        public IActionResult Export([FromForm] List<TitleSearchDto> data)
        {
            stockTitle.Excel(data);
            return Ok();
        }
        #endregion
    }
}
