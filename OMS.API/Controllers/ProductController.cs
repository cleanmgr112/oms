using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMS.Core.Json;
using OMS.Data.Domain;
using OMS.Model;
using OMS.Model.Order;
using OMS.Services.Deliveries;
using OMS.Services.Order1;
using OMS.Services.Products;
using OMS.Core.Tools;
using OMS.Model.JsonModel;
using System.Net.Http;
using OMS.Core;
using LitJson;
using System.IO;
using Microsoft.Extensions.Configuration;
using OMS.Services;
using OMS.Services.Log;
using OMS.Data.Interface;
using OMS.Model.Products;
using OMS.Services.ScheduleTasks;
using OMS.Services.CMB;
using OMS.Services.StockRemid;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;

namespace OMS.API.Controllers
{
    //涉及到不同系统的登录状态，又没有统一的登录验证，所以此api未设置登录访问
    [Route("oms/[controller]/[action]")]
    //[Authorize]
    [AllowAnonymous]
    public class ProductController : Controller
    {

        #region Ctor
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IDeliveriesService _deliveriesService;
        private readonly IConfiguration _configuration;
        private readonly IPurchasingService _purchasingService;
        private readonly ILogService _logService;
        private readonly IDbAccessor _omsAccessor;
        private readonly IScheduleTaskFuncService _scheduleTaskFuncService;
        private readonly ICMBService _cmbService;
        private readonly IHubContext<HubContext, IHubContext> hubContext;
        public ProductController(IProductService productService,
            IOrderService orderService,
            IDeliveriesService deliveriesService,
            IConfiguration configuration,
            IPurchasingService purchasingService,
            ILogService logService,
            IDbAccessor omsAccessor,
            IScheduleTaskFuncService scheduleTaskFuncService,
            ICMBService cmbService,
            IHubContext<HubContext, IHubContext> hubContext
            )
        {
            _productService = productService;
            _orderService = orderService;
            _deliveriesService = deliveriesService;
            _configuration = configuration;
            _purchasingService = purchasingService;
            _logService = logService;
            _omsAccessor = omsAccessor;
            _scheduleTaskFuncService = scheduleTaskFuncService;
            _cmbService = cmbService;
           this.hubContext = hubContext;
        }
        #endregion


        [HttpPost]
        public IActionResult Synchronize([FromBody]List<WineDto> wineDtos)
        {
            if (wineDtos.Count > 0)
            {
                dynamic res = _productService.PMSynchronize(wineDtos);
                return Json(res);
            }
            else
            {
                return Json(new { state = "no data" });
            }
        }

        [HttpPost]
        public IActionResult SynchronizeSingle([FromBody]WineDto wineDtos)
        {
            if (wineDtos != null)
            {
                dynamic res = _productService.PMSynchronize(new List<WineDto> { wineDtos });
                return Json(res);
            }
            else
            {
                return Json(new { state = "no data" });
            }
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok("this is a test api...");
        }

        /// <summary>
        /// OMS所有商品库存同步
        /// </summary>
        /// <param name="saleProductWareHouseStocks"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncAllSaleProductStock([FromBody] List<SaleProductWareHouseStockSyncModel> saleProductWareHouseStocks)
        {
            if (saleProductWareHouseStocks != null)
            {
                dynamic res = _productService.SyncSaleProductStock(saleProductWareHouseStocks);
                //向在线的用户发送库存更新消息，重新获取模板
                hubContext.Clients.All.SendMessage("stock", string.Empty);
                return Json(res);
            }
            else
            {
                return Json(new { state = "error" });
            }
        }
        /// <summary>
        /// OMS单个商品库存同步
        /// </summary>
        /// <param name="saleProductWareHouseStocks"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncSingleSaleProductStock([FromBody] List<SaleProductWareHouseStockSyncModel> saleProductWareHouseStocks)
        {
            if (saleProductWareHouseStocks != null)
            {
                dynamic res = _productService.SyncSingleSaleProductStock(saleProductWareHouseStocks);
                //向在线的用户发送库存更新消息，重新获取模板
                hubContext.Clients.All.SendMessage("stock", string.Empty);
                return Json(res);
            }
            else
            {
                return Json(new { state = "error" });
            }
        }


        #region WMS到OMS订单状态同步
        /// <summary>
        /// WMS到OMS订单状态同步(暂时只同步已发货状态)
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncOrderState([FromBody] List<SyncOrderStateModel> models)
        {
            if (models != null && models.Count > 0)
            {
                var sucOrders = new List<string>();
                var errOrders = new List<string>();
                foreach (var model in models)
                {

                    if (string.IsNullOrEmpty(model.OMSSerialNumber))
                    {
                        errOrders.Add(model.SerialNumber);
                        continue;
                    }

                    var order = _orderService.GetOrderBySerialNumber(model.OMSSerialNumber);
                    if (order == null)
                    {
                        errOrders.Add(model.SerialNumber);
                        continue;
                    }

                    if ((model.DeliveryState == DeliveryStateEnum.DeliveredLogistics && model.OrderType == OrderTypeEnum.B2B) ||
                        (model.DeliveryState == DeliveryStateEnum.DeliveredExpress && model.OrderType == OrderTypeEnum.B2C))
                    {

                        if (order.State == OrderState.Delivered)
                        {
                            errOrders.Add(model.SerialNumber);
                            continue;
                        }

                        order.State = OrderState.Delivered;
                        order.DeliveryNumber = model.DeliveryNumber;
                        order.ModifiedTime = DateTime.Now;
                        try
                        {
                            _orderService.UpdateOrder(order);
                            sucOrders.Add(model.SerialNumber);
                        }
                        catch (Exception e)
                        {
                            errOrders.Add(model.SerialNumber);
                        }
                    }
                    else
                    {
                        errOrders.Add(model.SerialNumber);
                        continue;
                    }
                }

                return Json(new { State = true, SucOrders = sucOrders, ErrOrders = errOrders });
            }
            else
            {
                return Json(new { State = false });
            }

        }
        #endregion

        /// <summary>
        /// WMS到OMS订单快递信息同步
        /// </summary>
        /// <param name="orderDeliveryInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncOrderDeliveryState([FromBody] OrderDeliveryInfo orderDeliveryInfo)
        {
            if (orderDeliveryInfo != null)
            {
                var zhShopId = _configuration.GetSection("Zhaohang")["shopId"]; //招行店铺号
                var order = _orderService.GetOrderBySerialNumber(orderDeliveryInfo.OMSSerialNumber);
                if (order == null)
                {
                    _logService.Error(string.Format("订单快递信息同步时未找到订单号为{0}的订单", orderDeliveryInfo.OMSSerialNumber));
                    return Json(new { State = false, Message =string.Format("OMS中未找到SerialNumber为{0}的订单",orderDeliveryInfo.OMSSerialNumber) });
                }

                var delivery = _deliveriesService.GetAllDeliveries().Where(r => r.Isvalid && r.Code.Equals(orderDeliveryInfo.DeliveryCode)).FirstOrDefault();
                if (delivery==null)
                {
                    _logService.Error(string.Format("订单号为：{0}的订单在同步快递信息时未找到快递Code为{1}的快递方式", orderDeliveryInfo.OMSSerialNumber, orderDeliveryInfo.DeliveryCode));
                    return Json(new { State = false, Message =string.Format("OMS中未匹配到Code为{0}的快递",orderDeliveryInfo.DeliveryCode) });
                }

                if (order != null)
                {
                    order.DeliveryDate = orderDeliveryInfo.DeliveryDate;
                    order.DeliveryNumber = orderDeliveryInfo.DeliveryNumber;
                    order.DeliveryTypeId = delivery.Id;
                    if (order.Type == OrderType.B2B)
                    {
                        order.State = OrderState.Finished;
                    }
                    else {
                        order.State = OrderState.Delivered;

                    }
                    _orderService.UpdateOrder(order);

                    #region 订单日志(订单发货)
                    OrderLog orderLog = new OrderLog();
                    orderLog.OrderId = order.Id;
                    orderLog.OrderState = order.State;
                    orderLog.PayState = order.PayState;
                    orderLog.OptionType = "订单发货";
                    orderLog.Mark = "订单发货";
                    _logService.InsertOrderLog(orderLog);
                    #endregion
                    _logService.Info(string.Format("OMS订单号为：{0}，订单Id为：{1} ，从WMS到OMS同步订单物流发货状态为{2}",order.SerialNumber,order.Id,order.State.Description()));

                    //推送快递信息到订单辅助系统（官方的商城的订单才可以）
                    var orderList = new List<Order>();//需要通知的发货订单集合
                    if (order.Type == OrderType.B2C_XH && order.ShopId == 97)
                    {
                        //正常的订单流程
                        if (order.AppendType == 0) {
                            orderList.Add(order);
                        }
                        //合并的订单
                        if (order.AppendType == AppendType.Combine) {
                            var orderArr = order.OrgionSerialNumber.Split("_",StringSplitOptions.RemoveEmptyEntries);
                            //找出所有合并订单里面的子订单进行通知
                            for (int i = 0; i < orderArr.Length; i++)
                            {
                                var childOrder = _orderService.GetOrderBySerialNumber(orderArr[i].Trim());
                                childOrder.DeliveryNumber = order.DeliveryNumber;
                                if (childOrder != null) orderList.Add(childOrder);
                            }
                        }

                        //拆分的订单
                        if (order.AppendType == AppendType.Split) {
                            //父订单单号
                            var orgionSerialNumber = order.OrgionSerialNumber.Split("(")[0].Trim();
                            if (!string.IsNullOrEmpty(orgionSerialNumber)) {
                                //判断拆分成的子订单是否发货，存在未发货的不去通知，子订单全部发货完成之后再进行通知商城订单发货
                                var childOrders = _omsAccessor.Get<Order>().Where(x => x.Isvalid && x.State != OrderState.Delivered && x.OrgionSerialNumber.Contains(orgionSerialNumber));
                                if (childOrders==null || childOrders.Count()==0)
                                {
                                    var updateOrder = _orderService.GetOrderBySerialNumber(orgionSerialNumber);
                                    updateOrder.DeliveryNumber = childOrders.FirstOrDefault().DeliveryNumber;
                                    if(updateOrder!=null)orderList.Add(updateOrder);
                                }
                            }
                        }
                        //通知发货状态
                        if (orderList!=null && orderList.Count>0) {         
                            SyncOrderDeliveryToAssist(orderList);
                        }
                    }
                    //推送快递信息到招行系统
                    else if (order.Type == OrderType.B2C_XH && order.ShopId == int.Parse(zhShopId))
                    {
                        //正常的订单流程
                        if (order.AppendType == 0)
                        {
                            orderList.Add(order);
                        }
                        //合并的订单
                        if (order.AppendType == AppendType.Combine)
                        {
                            var orderArr = order.OrgionSerialNumber.Split("_", StringSplitOptions.RemoveEmptyEntries);
                            //找出所有合并订单里面的子订单进行通知
                            for (int i = 0; i < orderArr.Length; i++)
                            {
                                var childOrder = _orderService.GetOrderBySerialNumber(orderArr[i].Trim());
                                childOrder.DeliveryNumber = order.DeliveryNumber;
                                childOrder.DeliveryTypeId = order.DeliveryTypeId;
                                if (childOrder != null) orderList.Add(childOrder);
                            }
                        }

                        //拆分的订单
                        if (order.AppendType == AppendType.Split)
                        {
                            //父订单单号
                            var orgionSerialNumber = order.OrgionSerialNumber.Split("(")[0].Trim();
                            if (!string.IsNullOrEmpty(orgionSerialNumber))
                            {
                                //判断拆分成的子订单是否发货，存在未发货的不去通知，子订单全部发货完成之后再进行通知商城订单发货
                                var childOrders = _omsAccessor.Get<Order>().Where(x => x.Isvalid && x.State != OrderState.Delivered && x.OrgionSerialNumber.Contains(orgionSerialNumber));
                                if (childOrders == null || childOrders.Count() == 0)
                                {
                                    var updateOrder = _orderService.GetOrderBySerialNumber(orgionSerialNumber);
                                    updateOrder.DeliveryNumber = childOrders.FirstOrDefault().DeliveryNumber;
                                    updateOrder.DeliveryTypeId = childOrders.FirstOrDefault().DeliveryTypeId;
                                    if (updateOrder != null) orderList.Add(updateOrder);
                                }
                            }
                        }
                        //通知发货状态
                        if (orderList != null && orderList.Count > 0)
                        {
                            foreach(var item in orderList)
                            {
                                var result = _cmbService.ReturnDeliverResultToCMB(item);
                            }
                        }
                    }
                    //推送发货状态到线下店
                    else if (order.Type == OrderType.B2C_XH && order.PSerialNumber.Contains("XS"))
                    {
                        //正常的订单流程
                        if (order.AppendType == 0)
                        {
                            orderList.Add(order);
                        }
                        //合并的订单
                        if (order.AppendType == AppendType.Combine)
                        {
                            var orderArr = order.OrgionSerialNumber.Split("_", StringSplitOptions.RemoveEmptyEntries);
                            //找出所有合并订单里面的子订单进行通知
                            for (int i = 0; i < orderArr.Length; i++)
                            {
                                var childOrder = _orderService.GetOrderBySerialNumber(orderArr[i].Trim());
                                childOrder.DeliveryNumber = order.DeliveryNumber;
                                if (childOrder != null) orderList.Add(childOrder);
                            }
                        }

                        //拆分的订单
                        if (order.AppendType == AppendType.Split)
                        {
                            //父订单单号
                            var orgionSerialNumber = order.OrgionSerialNumber.Split("(")[0].Trim();
                            if (!string.IsNullOrEmpty(orgionSerialNumber))
                            {
                                //判断拆分成的子订单是否发货，存在未发货的不去通知，子订单全部发货完成之后再进行通知商城订单发货
                                var childOrders = _omsAccessor.Get<Order>().Where(x => x.Isvalid && x.State != OrderState.Delivered && x.OrgionSerialNumber.Contains(orgionSerialNumber));
                                if (childOrders == null || childOrders.Count() == 0)
                                {
                                    var updateOrder = _orderService.GetOrderBySerialNumber(orgionSerialNumber);
                                    updateOrder.DeliveryNumber = childOrders.FirstOrDefault().DeliveryNumber;
                                    if (updateOrder != null) orderList.Add(updateOrder);
                                }
                            }
                        }
                        //通知发货状态
                        if (orderList != null && orderList.Count > 0)
                        {
                            _orderService.ApiReturnOffLineOrderState(orderList);
                        }
                    }
                }

                return Json(new { State = true, Message = "成功" });
            }
            else
            {
                return Json(new { State = false, Message = "no data" });
            }
        }
        /// <summary>
        /// 推送订单物流信息到订单辅助系统
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async void SyncOrderDeliveryToAssist( List<Order> orderList)
        {

            var orderDeliveryDatas = new List<OrderDeliveryData>();
            foreach (var order in orderList)
            {
                var orderDeliveryData = new OrderDeliveryData
                {
                    sd_id = order.ShopId == 97 ? "501" : order.ShopId.ToString(),
                    order_sn = order.SerialNumber,
                    delivery_number = order.DeliveryNumber,
                    delivery_type = _deliveriesService.GetDeliveryById(order.DeliveryTypeId) == null ? "" : _deliveriesService.GetDeliveryById(order.DeliveryTypeId).ShopCode
                };
                orderDeliveryDatas.Add(orderDeliveryData);
            }

            using (var http=new HttpClient())
            {
                var content = new StringContent(orderDeliveryDatas.ToJson(), System.Text.Encoding.UTF8, "application/json");
                var postUrl = _configuration.GetSection("OrderAssistOmsApi")["domain"].ToString() + "/OrderDelivery";
                var response = http.PostAsync(postUrl, content).Result;
                var result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var data = JsonMapper.ToObject<OrderDeliveryResponse>(result);
                    if (data.state)
                    {
                        var orderNotificationResults = data.resp_data.delivery_result_list;
                        var noSuccNotific = new List<OrderNotificationResult>();
                        if (orderNotificationResults!=null && orderNotificationResults.Count>0) {
                            noSuccNotific = orderNotificationResults.Where(r => r.result != "1").ToList();
                        }                       
                        if (noSuccNotific!=null && noSuccNotific.Count() > 0)
                        {
                            var order_snStr = string.Empty;
                            foreach (var item in noSuccNotific)
                            {
                                order_snStr += string.Format("商铺Id:{0},系统方订单号{1};", item.sd_id, item.order_sn);
                            }

                            _logService.Error(string.Format("OMS发货成功通知信息异常：{0}", order_snStr));
                        }
                    }
                    else
                    {
                        _logService.Error(string.Format("OMS发货成功通知信息异常,订单号：{0}，错误信息：{1}", orderList.FirstOrDefault().SerialNumber, data.message));
                    }
                }
            }
        }

        /// <summary>
        /// WMS到OMS库存变更同步
        /// </summary>
        /// <param name="productStockFeedData"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncProductStock([FromBody] ProductStockFeedData productStockFeedData)
        {
            if(productStockFeedData!=null)
            {
                var saleProduct = _productService.GetSaleProduct(productStockFeedData.ProductId);
                var product = _productService.GetProductById(productStockFeedData.ProductId);
                if (saleProduct == null)
                    return Json(new { isSucc = false, msg = string.Format("OMS更新库存时未找到Id为{0}的商品库存信息", productStockFeedData.ProductId) });
                saleProduct.Stock = productStockFeedData.Quantity;
                saleProduct.LockStock = saleProduct.LockStock- productStockFeedData.ShopQuantity;
                saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                _productService.UpdateSaleProduct(saleProduct);

                _logService.Info(string.Format("商品Id为{0},商品名为：{4}，WMS到OMS同步库存为{1}，可用库存为{2},锁定库存为{3}", productStockFeedData.ProductId, productStockFeedData.Quantity, saleProduct.AvailableStock, saleProduct.LockStock, product.Name));
                var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                _productService.SyncProductStockToAssist(productStockFeedData.ShopId, product.Code, saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock)));

                //发送重新获取库存的消息
                hubContext.Clients.All.SendMessage("stock", string.Empty);
                return Json(new { isSucc = true, msg = "成功" });

            }
            else
            {
                return Json(new { isSucc = false, msg = "no data" });
            }
        }
        /// <summary>
        /// 推送商品库存变更信息到订单辅助系统
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="code"></param>
        /// <param name="quantity"></param>
        [HttpPost]
        public async void SyncProductStockToAssist(int shopid,string code,int quantity)
        {
            var productStockData = new ProductStockData
            {
                sd_id = shopid.ToString(),
                goods_sn=code,
                stock_num=quantity.ToString()
            };
            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var content = new StringContent(productStockData.ToJson(), System.Text.Encoding.UTF8, "application/json");
                var postUrl = _configuration.GetSection("OrderAssistOmsApi")["domain"] + "/ProductStockFeed";
                var response = http.PostAsync(postUrl, content).Result;
                var result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var data = JsonMapper.ToObject<StockFeedbackResponse>(result);
                    if (data.state)
                    {
                        var orderNotificationResults = data.resp_data.feedback_result_list;
                        var noSuccNotific = new List<StockFeedBackResult>();
                        if (orderNotificationResults!=null && orderNotificationResults.Count>0) {
                            noSuccNotific = orderNotificationResults.Where(r => r.result != "1").ToList();
                        }
                        if (noSuccNotific!=null && noSuccNotific.Count() > 0)
                        {
                            var order_snStr = string.Empty;
                            foreach (var item in noSuccNotific)
                            {
                                order_snStr += string.Format("商铺Id:{0},商品Id{1};", item.sd_id, item.goods_sn);
                            }

                            _logService.Error(string.Format("OMS库存变更通知信息异常：{0}", order_snStr));
                        }
                    }
                    else
                    {
                        _logService.Error(string.Format("OMS库存更新通知信息异常,商品编码：{0},错误信息：{1}",code, data.message));
                    }
                }
            }
        }

        /// <summary>
        /// WMS到OMS同步更新采购退单状态为完成
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncRefundPurchasingOrderState()
        {
            var content = new StreamReader(Request.Body).ReadToEnd();
            var model = content.ToObj<WMSSyncOrderModel>();
            if (string.IsNullOrEmpty(model.OMSSerialNumber))
                return Json(new { State = false, Message = "OMSSerialNumber为空" });
            var refundPurchasing = _purchasingService.GetPurchasingBySerialNumber(model.OMSSerialNumber);
            if (refundPurchasing == null)
                return Json(new { State = false, Message = "在OMS未找到单号为" + model.OMSSerialNumber + "的采购退单" });
            refundPurchasing.State = Data.Domain.Purchasings.PurchasingState.OutWareHouse;
            _purchasingService.UpdatePurchasingOrder(refundPurchasing);

            _logService.InsertOrderTableLog("Purchasing", refundPurchasing.Id, "同步采购退单", Convert.ToInt32(refundPurchasing.State), "把采购退单状态改为已出库,上传者为：" + model.UploadBy);
            return Json(new { State = true, Message = "修改OMS采购退单状态成功" });
        }

        /// <summary>
        /// 多个商品同步库存
        /// </summary>
        /// <param name="productStockFeedDatas"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncListProductsStock([FromBody] List<ProductStockFeedData> productStockFeedDatas)
        {
            if (productStockFeedDatas != null)
            {
                var errorCount = 0;
                var errorMsg = "";
                List<SyncProductStockModel> syncProductStockModels = new List<SyncProductStockModel>();
                try
                {

                    //OMS库存同步成功之后，再把库存同步到商城
                    var productStockDataList = new List<ProductStockData>();
                    foreach (var productStockFeedData in productStockFeedDatas)
                    {
                        var saleProduct = _productService.GetSaleProduct(productStockFeedData.ProductId);
                        var product = _productService.GetProductById(productStockFeedData.ProductId);
                        var saleProductWareHouseStock = _productService.GetSPHStockByWareHouseCodeAndProduct(productStockFeedData.WareHouseCode,productStockFeedData.ProductId);
                        if (saleProduct == null)
                        {
                            errorCount++;
                            errorMsg += string.Format("未找到可售商品， 商品Id：{0}，商品名为：{1};  ", productStockFeedData.ProductId, product.Name);
                            continue;
                        }

                        if (saleProductWareHouseStock==null) {
                            errorCount++;
                            errorMsg += string.Format("未找到可售商品仓库库存信息， 商品Id：{0}，商品名为：{1}，仓库编码{2};  ", productStockFeedData.ProductId, product.Name, productStockFeedData.WareHouseCode);
                            continue;
                        }
                        //销售商品总库存信息
                        saleProduct.Stock = productStockFeedData.Quantity;
                        saleProduct.LockStock = saleProduct.LockStock - productStockFeedData.ShopQuantity;  
                        saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                        //当前对应的仓库库存信息
                        saleProductWareHouseStock.Stock = productStockFeedData.WareHouseStock;
                        saleProductWareHouseStock.LockStock = saleProductWareHouseStock.LockStock - productStockFeedData.ShopQuantity;

                        _omsAccessor.Update(saleProduct);
                        _omsAccessor.Update(saleProductWareHouseStock);
                        _logService.Error(string.Format("商品Id为{0},商品名为：{4}，WMS到OMS同步库存为{1}，可用库存为{2},锁定库存为{3},仓库{5}的库存为{6},锁定库存为{7}", productStockFeedData.ProductId, productStockFeedData.Quantity, saleProduct.AvailableStock, saleProduct.LockStock, product.Name, productStockFeedData.WareHouseCode, saleProductWareHouseStock.Stock, saleProductWareHouseStock.LockStock));
                        //更新商品库存到商城
                        var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                        var productStockData = new ProductStockData
                        {
                            sd_id = productStockFeedData.ShopId.ToString(),
                            goods_sn = product.Code,
                            stock_num = (saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock))).ToString(),
                            stock_detail_list = _productService.GetSaleProductWareHouseStocksByProductCode(product.Code)
                        };
                        productStockDataList.Add(productStockData);
                    }
                    if (errorCount > 0)
                    {
                        return Json(new { isSucc = false, msg = "同步库存失败，原因：" + errorMsg });
                    }
                    _omsAccessor.SaveChanges();

                    //更新商品到商城
                    _productService.SyncMoreProductStockToAssist(productStockDataList);

                    //发送重新获取库存的消息
                    hubContext.Clients.All.SendMessage("stock", string.Empty);
                    return Json(new { isSucc = true, msg = "成功" });
                }
                catch(Exception ex)
                {

                    return Json(new { isSucc = false, msg = "同步库存失败，原因：" + ex.Message });
                }
            }
            else
            {
                return Json(new { isSucc = false, msg = "no data" });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wmsToOmsWareHouseStockModels"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SyncProductWareHouseStockToOMS([FromBody]List<WmsToOmsWareHouseStockModel> wmsToOmsWareHouseStockModels) {
            try
            {
                foreach (var wmsToOmsWareHouseStockModel in wmsToOmsWareHouseStockModels)
                {
                    if (wmsToOmsWareHouseStockModel.WareHouseStocks == null || wmsToOmsWareHouseStockModel.WareHouseStocks.Count == 0)
                    {
                        _logService.Error("WMS传递的Id为"+ wmsToOmsWareHouseStockModel.ProductOmsId + "商品库存信息错误!");
                        continue;
                    }
                   _productService.UpdateWareHouseStock(wmsToOmsWareHouseStockModel.WareHouseStocks, wmsToOmsWareHouseStockModel.ProductOmsId);
                }
                return Json(new { isSucc = true, msg = "成功" });
            }
            catch (Exception ex)
            {
                _logService.Error("商品仓库库存更新异常："+ex.Message);

                return Json(new { isSucc = false, msg = "商品仓库库存更新错误！" });
            }
        }
        /// <summary>
        /// 测试接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult TestUpdateStock(int productId) {

           var res =   _productService.SyncStocksWmsToOms(productId);

            return Json(new { isSucc = true,msg ="成功" });
        }

        /// <summary>
        /// 更新所有商品库存到商城
        /// </summary>
        [HttpPost]
        public IActionResult UpdateOMSToShopProductsStocks() {
            try
            {
                _scheduleTaskFuncService.OMSToShopAllProductsStock();
                return Json(new { isSucc = true, msg = "成功" });
            }
            catch (Exception ex)
            {
                var mess = ex.Message;  
                throw;
            }
   

        }
        [HttpPost]
        public IActionResult UpdateStockFromWMSToOMSAndShop()
        {
            var saleProducts = _productService.GetAllSaleProductsList();

            foreach (var item in saleProducts)
            {
                try
                {
                    _productService.SyncStocksWmsToOms(item.ProductId);
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            return Ok();
        }
    }
}