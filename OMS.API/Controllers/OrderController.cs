using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMS.Model.JsonModel;
using OMS.Services.Order1;
using OMS.Core.Json;
using OMS.Services.Log;
using OMS.Services;
using OMS.Services.Deliveries;
using OMS.Data.Domain;
using OMS.Core.Tools;
using System.Text;
using NPOI.OpenXml4Net.OPC;
using Microsoft.Extensions.Configuration;
using OMS.Model.Order;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace OMS.API.Controllers
{
    //涉及到不同系统的登录状态，又没有统一的登录验证，所以此api未设置登录访问
    [Route("oms/[controller]/[action]")]
    //[Authorize]
    [AllowAnonymous]
    public class OrderController : Controller
    {
        #region Ctor
        private readonly IOrderService _orderService;
        private readonly ILogService _logService;
        private readonly IPurchasingService _purchasingService;
        private readonly IDeliveriesService _deliveriesService;
        private IConfiguration _configuration;
        public OrderController(
            IOrderService orderService,
            ILogService logService,
            IPurchasingService purchasingService,
            IDeliveriesService deliveriesService,
            IConfiguration configuration)
        {
            _orderService = orderService;
            _logService = logService;
            _purchasingService = purchasingService;
            _deliveriesService = deliveriesService;
            _configuration = configuration;
        }
        #endregion


        #region 查询
        /// <summary>
        /// 获取B2B订单列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetB2BOrders(string token, string orderSerialNumber = "", int orderState = 0, DateTime? startTime = null, DateTime? endTime = null)
        {
            if (VerifySignature(token,_configuration["key"]))
            {
                var data = _orderService.GetAllB2BOrder(orderSerialNumber: orderSerialNumber, orderState: orderState, startTime: startTime, endTime: endTime);
                return Json(new { status = "success", resultCode = "200", data = data });
            }
            return Json(new { status = "fail", resultCode = "201", data = "" });
        }
        /// <summary>
        /// 根据平台单号获取订单信息
        /// </summary>
        /// <param name="pSerialNumber">平台单号</param>
        /// <param name="sign">验证部分</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetOrderInfo(string pSerialNumber, string sign)
        {
            if (VerifySignature(sign, _configuration["key"]))
            {
                if (string.IsNullOrEmpty(pSerialNumber))
                {
                    return Json(new { status = "fail", resultCode = "201", data = "请填写正确平台单号" });
                }
                var data = _orderService.ApiGetOrderInfo(pSerialNumber);
                return Json(new { status = "success", resultCode = "200", data = data });
            }
            return Json(new { status = "fail", resultCode = "201", data = "数据验证错误" });
        }
        #endregion


        #region 操作
        /// <summary>
        /// 通过订单号获取订单当前状态
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetOMSOrderState(string serialNumber)
        {
            var result = _orderService.GetOrderState(serialNumber);
            return Json(new { isSucc = true, meg = result });
        }
        /// <summary>
        /// WMS同步B2C/B2B退单状态到OMS，状态改为已入库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncRefundOrderStateToOMS()
        {
            var content = new StreamReader(Request.Body).ReadToEnd();
            var model = content.ToObj<WMSSyncOrderModel>();
            if (string.IsNullOrEmpty(model.OMSSerialNumber))
                return Json(new { isSucc = false, msg = "同步订单状态时，OMSSerialNumber为空" });
            var order = _orderService.GetOrderBySerialNumber(model.OMSSerialNumber);
            if (order == null)
                return Json(new { isSucc = false, msg = string.Format("同步订单状态时，在OMS中未查到SerialNumber为{0}的订单！", model.OMSSerialNumber) });

            order.State = Data.Domain.OrderState.CheckAccept;//修改退单状态为已入库
            order.DeliveryDate = DateTime.Now;
            _orderService.UpdateOrder(order);

            #region 日志（同步B2C退单状态）
            _logService.InsertOrderLog(order.Id, "同步退单状态", order.State, order.PayState, "WMS同步退单状态到OMS，WMS上传人：" + model.UploadBy);
            #endregion

            return Json(new { isSucc = true, msg = "同步成功" });
        }
        /// <summary>
        /// WMS同步采购订单状态到OMS，OMS采购订单状态由已上传改为已入库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncStockInOrderStateToOMS()
        {
            var content = new StreamReader(Request.Body).ReadToEnd();

            var model = content.ToObj<WMSSyncOrderModel>();
            if (string.IsNullOrEmpty(model.OMSSerialNumber))
                return Json(new { isSucc = false, msg = "同步采购订单状态时，OMSSerialNumber为空" });
            var purchasingOrder = _purchasingService.GetPurchasingBySerialNumber(model.OMSSerialNumber);
            if (purchasingOrder == null)
                return Json(new { isSucc = false, msg = string.Format("同步采购订单状态时，在OMS中未查到SerialNumber为{0}的采购订单！", model.OMSSerialNumber) });
            if (model.ProductInfoModel.Count() > 0)
            {
                //同步采购订单商品WMS实际接收的数量（数量按照WMS接收的数量，包括在暂存库位的数量）
                var setOrderProResult = _purchasingService.SyncWMSFactReceviedProNumsToOMS(purchasingOrder.Id, model.ProductInfoModel);
                if (!setOrderProResult)
                {
                    return Json(new { isSucc = false, msg = "同步采购订单状态时，同步订单商品实际接收数量有错，请联系管理员修正错误！" });
                }
            }

            purchasingOrder.State = Data.Domain.Purchasings.PurchasingState.StoredWareHouse;//修改采购订单状态为已入库
            _purchasingService.UpdatePurchasingOrder(purchasingOrder);

            #region 日志（同步采购订单状态）
            _logService.InsertOrderTableLog("Purchasing", purchasingOrder.Id, "同步采购订单状态", Convert.ToInt32(purchasingOrder.State), "WMS同步采购订单状态到OMS，WMS上传人：" + model.UploadBy);
            #endregion

            return Json(new { isSucc = true, msg = "同步成功" });
        }
        [HttpPost]
        public IActionResult WMSUpdateOrderDeliveryCode()
        {
            var content = new StreamReader(Request.Body).ReadToEnd();
            var model = content.ToObj<WMSOrderModelDelivery>();
            if (string.IsNullOrEmpty(model.OMSSerialNumber))
                return Json(new { isSucc = false, msg = "更新订单快递方式时，OMSSerialNumber为空" });
            var order = _orderService.GetOrderBySerialNumber(model.OMSSerialNumber);
            if (order == null)
            {
                return Json(new { isSucc = false, msg = string.Format("更新订单快递方式时，在OMS中未查到SerialNumber为{0}的订单！", model.OMSSerialNumber) });
            }
            order.DeliveryTypeId = _deliveriesService.GetDeliveryByCode(model.DeliveryCode);
            if (order.DeliveryTypeId == 0)
            {
                return Json(new { isSucc = false, msg = "更新订单快递方式时，OMS不存在" + model.DeliveryCode + "快递方式，请添加！" });
            }

            _orderService.UpdateOrder(order);
            #region 日志（同步采购订单状态）
            _logService.InsertOrderTableLog("order", order.Id, "修改订单快递方式", order.DeliveryTypeId, "WMS更新订单快递方式到OMS，WMS上传人：" + model.UploadBy);
            #endregion
            return Json(new { isSucc = true, msg = "更新成功" });
        }
        /// <summary>
        /// 同步发票信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncOrderInvoiceInfoToOMS()
        {
            var content = new StreamReader(Request.Body).ReadToEnd();
            var model = content.ToObj<WMSSyncInvoiceModel>();
            if (string.IsNullOrEmpty(model.OMSSerialNumber))
                return Json(new { isSucc = false, msg = "更新订单发票信息时，OMSSerialNumber为空" });
            var order = _orderService.GetOrderBySerialNumber(model.OMSSerialNumber);
            if (order == null)
            {
                return Json(new { isSucc = false, msg = string.Format("更新订单发票信息时，在OMS中未查到SerialNumber为{0}的订单！", model.OMSSerialNumber) });
            }
            if (order.InvoiceType == Data.Domain.InvoiceType.NoNeedInvoice || order.InvoiceInfo == null)
            {
                return Json(new { isSucc = false, msg = string.Format("更新订单发票信息时，在OMS中{0}的订单不需要发票信息！", model.OMSSerialNumber) });
            }
            order.InvoiceInfo.InvoiceNo = model.InvoiceNo;
            _orderService.UpdateOrderInvoiceInfo(order.InvoiceInfo);

            #region 日志（同步订单发票信息）
            _logService.InsertOrderLog(order.Id, "同步发票信息", order.State, order.PayState, "WMS同步发票信息发票号：" + model.InvoiceNo + "到OMS，WMS上传人：" + model.UploadBy);
            #endregion
            return Json(new { isSucc = true, msg = "更新成功" });
        }
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="sign"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateOrderToOMS()
        {
            /* *****************************************************************
            * 
            * 订单来源类型（orderType）：实体店线上订单（1）
            * 
            * *****************************************************************/

            var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = reader.ReadToEnd();
            var obj = JObject.Parse(data);
            var orders = JsonConvert.DeserializeObject<List<InterfaceOrderModel>>(obj["orders"].ToString());
            var sign = obj["sign"].ToString();
            var orderType = int.Parse(obj["orderType"].ToString());
            var resultStr = "";
            if (VerifySignature(sign, _configuration["key"]))
            {
                if (orders.Count > 0)
                {
                    var preWord = "";
                    switch (orderType)
                    {
                        case 1:
                            preWord = "XX";
                            break;
                    }
                    resultStr = _orderService.ApiCreateOrder(orders, preWord);
                }
                return Json(new { status = "success", resultCode = "200", data = resultStr });
            }
            return Json(new { status = "fail", resultCode = "201", data = "数据错误" });
        }
        /// <summary>
        /// 线下店退单接口
        /// </summary>
        /// <param name="pSerialNumber"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OffLineCancelOrder()
        {
            var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = reader.ReadToEnd();
            var obj = JObject.Parse(data);
            var sign = obj["sign"].ToString();
            var pSerialNumber = obj["pSerialNumber"].ToString();
            if (VerifySignature(sign, _configuration["key"]))
            {
                var result = _orderService.ApiOffLineCancelOrder(pSerialNumber);
                return Json(new { status = "success", resultCode = "200", data = result });
            }
            return Json(new { status = "fail", resultCode = "201", data = "数据错误" });
        }
        #endregion


        #region 测试用方法
        //[HttpGet]
        //public IActionResult GenKey()
        //{
        //    var str = CommonTools.CreateRandomStr(16);
        //    str = "97AVsyHmvBJFGMXj";
        //    var data = EncryptTools.AESEncrypt(str + "|" + DateTime.Now.ToString("yyyyMMddHHmmss"), str);
        //    var result = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        //    return Json(new { key = str, token = result });
        //}
        //[HttpGet]
        //public IActionResult Test()
        //{
        //    var order = new InterfaceOrderModel();
        //    order.PSerialNumber = "XX45513245556";//必填
        //    order.ShopId = 1;//后续给出（必填）
        //    order.PayType = 120;//后续给出（必填）
        //    order.PayMentType = 0;//填0即可(B2B订单需要)
        //    order.PayDate = "20200918112455";//付款日期，传递过来格式"yyyyMMddHHmmss"
        //    order.PayPrice = 200;//已付款金额（必填）

        //    //order.IsNeedPaperBag = true;//是否需要纸袋
        //    order.DeliveryTypeId = 11;//后续给出（必填）

        //    order.UserName = "";//商城购买者账号，需要关联的时候填写
        //    order.CustomerName = "任盈盈";
        //    order.CustomerPhone = "18776159649";
        //    order.CustomerAddressDetail = "广东省 深圳市 南山区 金融科技大厦12楼";
        //    order.CustomerMark = "尽快发货";
        //    order.AdminMark = "客服备注";

        //    order.ZMCoupon = 0;
        //    order.ZMWineCoupon = 0;
        //    order.WineWorldCoupon = 0;
        //    order.ProductCoupon = "";
        //    order.ZMIntegralValuePrice = 0;


        //    //发票信息
        //    InvoiceInfo invInfo = new InvoiceInfo();
        //    invInfo.CustomerEmail = "8888@qq.com";//客户用于发票接收邮箱
        //    invInfo.Title = "发票抬头";
        //    //发票中包含的增票部分
        //    invInfo.TaxpayerID = "45234234";
        //    invInfo.RegisterAddress = "广东省 深圳市 南山区 金融科技大厦12楼";
        //    invInfo.RegisterTel = "8588885";
        //    invInfo.BankOfDeposit = "广发 桃园支行";
        //    invInfo.BankAccount = "88888888888888";


        //    //商品
        //    List<InterFaceOrderProduct> proList = new List<InterFaceOrderProduct>();
        //    InterFaceOrderProduct pro = new InterFaceOrderProduct();
        //    pro.ProductCode = "Ferrari911";
        //    pro.ProductCode = "Ferrari911";
        //    pro.Quantity = 5;
        //    pro.SumPrice = 200;
        //    pro.Price = 80;



        //    proList.Add(pro);
        //    order.Products = proList;
        //    order.InvoiceInfo = invInfo;
        //    var data = new List<InterfaceOrderModel>();
        //    data.Add(order);
        //    //order.InvoiceMode = item.InvoiceMode;//开票方式
        //    dynamic dd = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(GenKey()));

        //    using (var http = new HttpClient())
        //    {
        //        var data3 = new { orders = data, sign = dd, orderType = 1 };

        //        StringContent content = new StringContent(JsonConvert.SerializeObject(data3));
        //        var response = http.PostAsync("http://localhost:50444", content);
                

        //    }

        //    var cc = CreateOrderToOMS(data, (string)dd.Value["token"], 1);
        //    return Json(cc);
        //}
        #endregion


        #region 验证部分
        /// <summary>
        /// AES验证部分
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="key"></param>
        bool VerifySignature(string sign, string key)
        {
            /* *****************************************************************
             * 
             * sign字符串  Base64[AES(key|dateTime)]
             * dateTime  接口获取时间 误差3分钟  格式"yyyyMMddHHmmss"
             * key       双方约定密钥字符串    97AVsyHmvBJFGMXj
             * 
             * *****************************************************************/
            try
            {
                //反base64
                var req = Convert.FromBase64String(sign);
                var str = Encoding.GetEncoding("utf-8").GetString(req);
                //解密
                var res = EncryptTools.AESDecrypt(str, key);
                //判断
                var result = res.Split("|");
                var time = DateTime.ParseExact(result[1], "yyyyMMddHHmmss", null);
                if (result[0] == key && time.AddMinutes(3) >= DateTime.Now)
                {
                    return true;
                }
                return false;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        #endregion
    }
}