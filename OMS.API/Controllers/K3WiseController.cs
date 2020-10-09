using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMS.Data.Domain;
using OMS.Services.K3Wise;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OMS.API.Controllers
{
    [Route("api/[controller]")]
    public class K3WiseController : Controller
    {
        #region 
        private readonly IK3WiseService _k3WiseService;
        public K3WiseController(IK3WiseService k3WiseService)
        {
            _k3WiseService = k3WiseService;
        }
        #endregion

        /*-----2019-11-18-------*/
        #region 中间服务器
        [HttpGet("AddOrdersToK3")]
        public IActionResult AddOrdersToK3()
        {
            _k3WiseService.AddOrdersToK3();
            return Ok("订单传递成功！");
        }
        [HttpPost("AcceptAndSendOrderToK3")]
        public IActionResult AcceptAndSendOrderToK3([FromBody]dynamic data)
        {
            var result = _k3WiseService.AcceptDataFromOMS(data);
            return Ok(result);
        }
        /// <summary>
        /// 从K3服务器获取客户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCustomersInfo")]
        public IActionResult GetCustomersInfo()
        {
            return Json(_k3WiseService.GetCustomersInfoFromK3());
        }
        #endregion


        #region OMS服务器
        /// <summary>
        /// 给OMS服务器获取所有需要上传的订单信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetOMSOrders")]
        public IActionResult GetNeedToSendOrders()
        {
            var result = _k3WiseService.GetAllOutOfStockOrders();
            return Json(result);
        }
        /// <summary>
        /// 接收订单上传状态
        /// </summary>
        /// <returns></returns>
        [HttpPost("AcceptOrderResult")]
        public IActionResult OrderResult([FromBody]dynamic data)
        {
            var data2 = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data));
            if (data.OMSSeriNo != null && data.K3BillNo != null)
            {
                K3BillNoRelated k3Re = new K3BillNoRelated();
                k3Re.K3BillNo = data.K3BillNo;
                k3Re.OMSSeriNo = data.OMSSeriNo;
                k3Re.Message = data.Message;
                _k3WiseService.AddK3BillNoRelated(k3Re);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
        /// <summary>
        /// 更新客户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("InitCustomers")]
        public IActionResult InitCustomersInfo()
        {
            var result = _k3WiseService.UpdateCustomersInfo();
            return Ok(result);
        }
        #endregion
    }
}
