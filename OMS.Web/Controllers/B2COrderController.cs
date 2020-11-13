using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Model;
using OMS.Services;
using OMS.Services.Account;
using OMS.Services.Common;
using OMS.Services.Log;
using OMS.Services.Order1;
using OMS.Services.Permissions;
using OMS.Services.Products;
using OMS.Services.ScheduleTasks;
using OMS.Core.Json;
using OMS.Model.JsonModel;
using System.IO;
using System.Data;
using OMS.Core.Tools;
using System.Dynamic;
using System.Text;
using OMS.Services.Deliveries;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using System.Net.Http.Headers;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using OMS.Data.Interface;
using OMS.Model.Grid;
using OMS.Model.Order;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OMS.WebCore;
using NPOI.SS.Formula.Functions;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class B2COrderController : BaseController
    {
        #region
        private readonly IOrderService _orderService;
        private readonly ILogService _logService;
        private readonly ICommonService _commonService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IScheduleTaskFuncService _scheduleTaskFuncService;
        private readonly IDeliveriesService _deliveriesService;
        private readonly IDbAccessor _omsAccessor;
        protected readonly IWorkContext _workContext;
        public B2COrderController(IOrderService orderService,
            ILogService logService,
            ICommonService commonService,
            IHostingEnvironment hostingEnvironment,
            IProductService productService,
            IUserService userService,
            IPermissionService permissionService,
            IWareHouseService wareHouseService,
            IScheduleTaskFuncService scheduleTaskFuncService,
            IDeliveriesService deliveriesService,
            IDbAccessor omsAccessor,
            IWorkContext workContext
            )
        {
            _orderService = orderService;
            _commonService = commonService;
            _hostingEnvironment = hostingEnvironment;
            _logService = logService;
            _productService = productService;
            _userService = userService;
            _permissionService = permissionService;
            _wareHouseService = wareHouseService;
            _scheduleTaskFuncService = scheduleTaskFuncService;
            _deliveriesService = deliveriesService;
            _omsAccessor = omsAccessor;
            _workContext = workContext;
        }
        #endregion


        #region 页面
        public IActionResult Index()
        {
            if (!_permissionService.Authorize("ViewB2COrder"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            return View();
        }
        public IActionResult CombineOrderList()
        {
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            return View();
        }
        public IActionResult CreatedB2COrder()
        {
            if (!_permissionService.Authorize("ViewCreateB2COrder"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Delivery = new SelectList(_orderService.GetAllDeliveryList(), "Id", "Name");
            ViewBag.Platform = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.PayType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PayType), "Id", "Value");
            return View();
        }
        public IActionResult B2COrderDetail(int id)
        {
            if (!_permissionService.Authorize("ViewB2COrderDetail"))
            {
                return View("_AccessDeniedView");
            }
            
            var orderresult = _orderService.GetOrderByIdB2C(id);
            orderresult.InvoiceInfo = _orderService.GetOrderInvoiceRecord(id);
            ViewBag.User = GetUserName();
            ViewBag.Delivery = new SelectList(_orderService.GetAllDeliveryList(), "Id", "Name", orderresult.DeliveryTypeId);
            ViewBag.Platform = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value", orderresult.ShopId);
            ViewBag.PayType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PayType), "Id", "Value", orderresult.PayType);
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetWareHouses().Where(r => new List<WareHouseTypeEnum>() { WareHouseTypeEnum.Normal, WareHouseTypeEnum.VirtualStore }.Contains(r.WareHouseType)), "Id", "Name", orderresult.WarehouseId);
            ViewBag.OrderPayRecord = _orderService.GetOrderPay(id);
            ViewBag.OrderLog = _orderService.GetOrderRecord(id);
            ViewBag.OrderProducts = _orderService.GetOrderProductsModelByOrderId(id,orderresult.WarehouseId);
            //可退货商品信息
            ViewBag.CanRefundOrderProducts = _orderService.GetCanRefundOrderProductsByOrderId(id);
            ViewBag.SaleProducts = _productService.GetSaleProductsByOrderType(94);
            ViewBag.InvoiceTypes = new SelectList(EnumExtensions.GetEnumList((Enum)InvoiceType.NoNeedInvoice), "Key", "Value", (int)orderresult.InvoiceType);
            ViewBag.InoviceModes = new SelectList(EnumExtensions.GetEnumList((Enum)InvoiceModeEnum.Electronic), "Key", "Value", (int)orderresult.InvoiceMode);
            //ViewBag.OrderStateStr = GetOrderStateName();
            ViewBag.RefundState = _orderService.JudgeOrderRefundState(id);
            ViewBag.OrderStateStr = EnumExtensions.GetEnumList((Enum)OrderState.B2CConfirmed);
            ViewBag.IsLackStock = _orderService.GetOrderProductsModelByOrderId(id, orderresult.WarehouseId).Where(r => r.Quantity > r.TotalQuantity || r.IsLackStock).Count() > 0;//是否存在缺货
            ViewBag.OriginalPrice = orderresult.OrderProduct.Sum(x => x.OrginPrice * x.Quantity);
            return View(orderresult);
        }
        /// <summary>
        /// 合并订单列表页
        /// </summary>
        /// <returns></returns>
        public IActionResult MergeOrderList()
        {
            return View();
        }
        /// <summary>
        /// B2C退单列表（页面）
        /// </summary>
        /// <returns></returns>
        public IActionResult B2CRefundOrdersList()
        {
            if (!_permissionService.Authorize("ViewB2CRefundOrder"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            return View();
        }
        /// <summary>
        /// B2C退单详情（页面）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult B2CRefundOrderDetail(int id)
        {
            if (!_permissionService.Authorize("ViewB2CRefundOrderDetail"))
            {
                return View("_AccessDeniedView");
            }
            var order = _orderService.GetOrderByIdB2C(id);
            ViewBag.User = GetUserName();
            ViewBag.Platform = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value", order.ShopId);
            ViewBag.Delivery = new SelectList(_orderService.GetAllDeliveryList(), "Id", "Name", order.DeliveryTypeId);
            ViewBag.Paytype = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PayType), "Id", "Value", order.PayType);
            ViewBag.Warehouse = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name", order.WarehouseId);
            ViewBag.SaleProducts = _productService.GetSaleProductsByOrderType(94);
            ViewBag.OrderProducts = _orderService.GetOrderProductsByOrderId(id);
            ViewBag.OrderLog = _orderService.GetOrderRecord(id);
            //可退货商品信息
            ViewBag.CanRefundOrderProducts = _orderService.GetCanRefundOrderProductsByOrderId(order.OriginalOrderId);
            //ViewBag.OrderStateStr = GetOrderStateName();
            ViewBag.OrderStateStr = EnumExtensions.GetEnumList((Enum)OrderState.B2CConfirmed);
            return View(order);
        }
        /// <summary>
        /// 客户的历史订单
        /// </summary>
        /// <returns></returns>
        public IActionResult CustomerHistoryOrderList(string userName = "")
        {
            ViewBag.UserName = userName;
            return View();
        }
        public IActionResult HeBingOrderList(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            List<Order> orders = new List<Order>();

            if (order != null && order.OrgionSerialNumber.Split(',').Count() > 0)
            {
                var serialNumbers = order.OrgionSerialNumber.Split(',');
                foreach (var item in serialNumbers)
                {
                    var oriOrder = _orderService.GetOrderBySerialNumber(item);
                    if (oriOrder != null)
                        orders.Add(oriOrder);
                }
            }
            return View(orders);
        }
        /// <summary>
        /// 待处理订单列表
        /// </summary>
        /// <returns></returns>
        public IActionResult PendingOrdersList()
        {
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            ViewBag.Deliver = new SelectList(_orderService.GetAllDeliveryList(),"Id", "Name");
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            return View();
        }
        public IActionResult CreatedKJRefundOrder()
        {
            ViewBag.Delivery = new SelectList(_orderService.GetAllDeliveryList(), "Id", "Name");
            ViewBag.Platform = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.PayType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PayType), "Id", "Value");
            return View();
        }
        public IActionResult KJRefundOrderList()
        {
            if (!_permissionService.Authorize("ViewB2CRefundOrder"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            return View();
        }
        /// <summary>
        /// 跨境退单详情（页面）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult KJRefundOrderDetail(int id)
        {
            if (!_permissionService.Authorize("ViewB2CRefundOrderDetail"))
            {
                return View("_AccessDeniedView");
            }
            var order = _orderService.GetOrderByIdB2C(id);
            ViewBag.User = GetUserName();
            ViewBag.Platform = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value", order.ShopId);
            ViewBag.Delivery = new SelectList(_orderService.GetAllDeliveryList(), "Id", "Name", order.DeliveryTypeId);
            ViewBag.Paytype = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PayType), "Id", "Value", order.PayType);
            ViewBag.Warehouse = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name", order.WarehouseId);
            ViewBag.SaleProducts = _productService.GetSaleProductsByOrderType(94);
            ViewBag.OrderProducts = _orderService.GetOrderProductsByOrderId(id);
            ViewBag.OrderLog = _orderService.GetOrderRecord(id);
            //可退货商品信息
            ViewBag.CanRefundOrderProducts = _orderService.GetCanRefundOrderProductsByOrderId(order.OriginalOrderId);
            //ViewBag.OrderStateStr = GetOrderStateName();
            ViewBag.OrderStateStr = EnumExtensions.GetEnumList((Enum)OrderState.B2CConfirmed);
            return View(order);
        }
        #endregion


        #region 操作


        #region B2C正常订单
        /// <summary>
        /// 获取B2C订单
        /// </summary>
        public IActionResult GetB2COrders(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, OrderState? orderState, int? wareHouseId, string search = "", string supplierName = "")
        {
            var data = _orderService.GetAllB2COrdersByPage(pageIndex, pageSize, startTime, endTime, shopId, orderState, wareHouseId, search);

            return Success(data);
        }
        /// <summary>
        /// 获取B2C订单列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetB2COrdersTable(SearchModel searchModel, SearchOrderContext searchOrderContext)
        {

            if (searchModel.Length == 0)
            {
                searchModel.Length = 10;
            }
            if (searchModel.Start == 0)
            {
                searchModel.PageIndex = 1;
            }
            if (searchModel.Start > 0)
            {
                searchModel.PageIndex = searchModel.Start / searchModel.Length + 1;
            }
            searchOrderContext.PageIndex = searchModel.PageIndex;
            searchOrderContext.PageSize = searchModel.Length;
            searchOrderContext.EndTime = searchOrderContext.EndTime?.AddMinutes(1);
            searchOrderContext.PayEndTime = searchOrderContext.PayEndTime?.AddMinutes(1);
            searchOrderContext.DeliverEndTime = searchOrderContext.DeliverEndTime?.AddMinutes(1);
            Dictionary<InvoiceType, string> InvoiceTypeName = new Dictionary<InvoiceType, string>
            {
                { InvoiceType.NoNeedInvoice,"无发票"},
                { InvoiceType.PersonalInvoice,"个人发票"},
                { InvoiceType.CompanyInvoice,"普通单位发票"},
                { InvoiceType.SpecialInvoice,"专用发票"}
            };

            try
            {
                var data = _orderService.GetAllB2COrdersTableByPage(searchOrderContext);

                foreach (var itemData in data)
                {

                    var refunddata = _omsAccessor.Get<Order>().Where(r => r.Isvalid && r.OriginalOrderId == itemData.Id && r.Type== OrderType.B2C_TH && r.State!= OrderState.Invalid).FirstOrDefault();
                    itemData.StateName = _orderService.GetOrderStateStr()[itemData.State];
                    itemData.PayStateName = itemData.PayState.ToString() == "Success" ? "已付款" : "未付款";
                    itemData.InvoiceTypeName = InvoiceTypeName[itemData.InvoiceType];
                    itemData.historyOrders = _omsAccessor.Get<Order>().Where(r => r.Isvalid && r.Id != itemData.Id && r.UserName.Equals(itemData.UserName) && !string.IsNullOrEmpty(r.UserName)).Count() > 0;
                    itemData.refundOrder = refunddata != null && (itemData.State == OrderState.Invalid || itemData.State== OrderState.Delivered);
                    itemData.refundNum = refunddata != null ? refunddata.SerialNumber : "";
                }
                var result = new SearchResultModel
                {
                    Data = data,
                    Draw = searchModel.Draw,
                    RecordsTotal = data.TotalCount,
                    RecordsFiltered = data.TotalCount,
                    isSucc = true,
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new SearchResultModel
                {
                    isSucc = false
                };

                return Json(result);
            }
        }
        /// <summary>
        /// 查询合并订单列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetCombineOrderListTable(SearchModel searchModel, SearchOrderContext searchOrderContext) {
            if (searchModel.Length == 0)
            {
                searchModel.Length = 10;
            }
            if (searchModel.Start == 0)
            {
                searchModel.PageIndex = 1;
            }
            if (searchModel.Start > 0)
            {
                searchModel.PageIndex = searchModel.Start / searchModel.Length + 1;
            }
            searchOrderContext.PageIndex = searchModel.PageIndex;
            searchOrderContext.PageSize = searchModel.Length;
            try
            {
                var data = _orderService.GetCanPageListCombineOrders(searchOrderContext);

                var result = new SearchResultModel
                {
                    Data = data,
                    Draw = searchModel.Draw,
                    RecordsTotal = data.TotalCount,
                    RecordsFiltered = data.TotalCount,
                    isSucc = true,
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new SearchResultModel
                {
                    isSucc = false
                };

                return Json(result);
            }
        }
        /// <summary>
        /// 合并订单
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CombineOrder(int[] list)
        {
            var newSerialNumber = _commonService.GetOrderSerialNumber("");
            var oldSerialNumber = new List<string>();
            var oldSerialNumberStr = "";
            var checkmsg = "";
            var checkCombineOrder = _orderService.CheckCombineOrderInfo(list,out checkmsg);
            if (!checkCombineOrder) {
                return Error(checkmsg);
            }
            var data = _orderService.CombineOrder(list, newSerialNumber, out oldSerialNumber);
            foreach (var item in oldSerialNumber)
            {
                oldSerialNumberStr += item + ",";
            }
            //日志
            _logService.InsertOrderLog(data.Id, "合并订单", data.State, data.PayState, "订单合并（原订单：" + oldSerialNumberStr + "）");
            foreach (var item in list)
            {
                _logService.InsertOrderLog(item, "合并订单", OrderState.Invalid, PayState.Success, "订单合并（订单：" + data.SerialNumber + "）");
            }
            var resultData = new Dictionary<string, string>();
            resultData.Add("Id", data.Id.ToString());
            resultData.Add("SerialNumber", data.SerialNumber);
            return Success(resultData);
        }   
        /// <summary>
        /// 生成B2C订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreatedB2COrder(Order order)
        {
            if (!_permissionService.Authorize("CreateB2COrder"))
            {
                return View("_AccessDeniedView");
            }
            try
            {
                order.SerialNumber = _commonService.GetOrderSerialNumber();
                order.WriteBackState = 0;
                order.IsLocked = false;
                order.PriceTypeId = 103;//标准价
                order.State = OrderState.Paid;//手工新增订单默认为已付款状态
                order.PayState = PayState.Success;
                order.PayDate = DateTime.Now;
                order.AddressDetail = order.AddressDetail;
                order.WarehouseId = _orderService.MatchFirstWareHouseId(order.AddressDetail);
                order.DistrictId = 0;
                var orderid = 0;
                //判断是否为作废订单
                if (_orderService.ConfirmPSerialNumberIsExist(order.PSerialNumber, false,false)){
                    return Json(new { isSucc = false, msg = "添加失败,检查平台订单号是否与其他订单重复或者该订单不是作废订单！" });
                }
                //如果存在平台单号的话加一个标识(N1)
                if (_orderService.ConfirmPSerialNumberIsExist(order.PSerialNumber,true,false))
                {
                    order.PSerialNumber = order.PSerialNumber + "(N)";
                    if (_orderService.ConfirmPSerialNumberIsExist(order.PSerialNumber,false,true))
                    {
                        return Json(new { isSucc = false, msg = "添加失败,检查平台订单号" + order.PSerialNumber.Replace("(N)", "") + "是否已经重新添加了平台单号：" + order.PSerialNumber + "的订单！" });
                    }
                }
                if (_orderService.GetOrderBySerialNumber(order.SerialNumber) == null)
                {   
                    Order newOrder = _orderService.CreatedB2COrder(order);
                    orderid = newOrder.Id;
                    var orderPayPrice = new OrderPayPrice
                    {
                        OrderId = orderid,
                        IsPay = true,
                        PayType = order.PayType,
                        PayMentType = order.PayMentType,
                        Price = order.PayPrice,
                    };
                    _orderService.AddOrderPayPrice(orderPayPrice);
                    #region 订单日志(新增订单)
                    OrderLog orderLog = new OrderLog();
                    orderLog.OrderState = newOrder.State;
                    orderLog.PayState = PayState.Success;//默认新增已支付
                    orderLog.OrderId = newOrder.Id;
                    orderLog.OptionType = "新增订单";
                    orderLog.Mark = "新增订单【" + newOrder.SerialNumber + "】";
                    orderLog.CreatedBy = newOrder.CreatedBy;
                    _logService.InsertOrderLog(orderLog);
                    #endregion

                    return Json(new { isSucc = true, orderId = orderid, orderSerialNumber = newOrder.SerialNumber });
                }
                else
                {
                    return Json(new { isSucc = false, msg = "添加失败,检查平台订单号是否与其他订单重复或者该订单不是作废订单！" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { isSucc = false, msg = "添加失败,原因：" + ex.Message });
            }
        }
        /// <summary>
        /// 获取所有销售商品
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult GetAllSaleProducts(string search, int pageSize, int pageIndex)
        {
            var data = _productService.GetSaleProductsPageByOrderType(94, pageSize, pageIndex, search);
            return Success(data);
        }
        /// <summary>
        /// 修改订单
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateOrder(OrderModel orderModel)
        {
            if (!_permissionService.Authorize("UpdateB2COrder"))
            {
                return Error("无操作权限！", 500);
            }
            Order order = _orderService.GetOrderByIdB2C(orderModel.Id);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid,OptTypeEnum.B2CSave);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            if (order != null)
            {
                var mark = "";
                if (order.CustomerMark != orderModel.CustomerMark && (IsNullOrEmpty(order.CustomerMark)!= IsNullOrEmpty(orderModel.CustomerMark)))
                {
                    mark += "【客户留言】：" + order.CustomerMark + " 修改为：" + orderModel.CustomerMark + " ";
                }
                if (order.CustomerName != orderModel.CustomerName)
                {
                    mark += "【收货人姓名】：" + order.CustomerName + " 修改为：" + orderModel.CustomerName + " ";
                }
                if (order.CustomerPhone != orderModel.CustomerPhone)
                {
                    mark += "【收货人电话】：" + order.CustomerPhone + " 修改为：" + orderModel.CustomerPhone + " ";
                }
                if (order.AddressDetail != orderModel.AddressDetail)
                {
                    mark += "【收货地址】：" + order.AddressDetail + " 修改为：" + orderModel.AddressDetail + " ";
                }
                if (order.AdminMark != orderModel.AdminMark && (IsNullOrEmpty(order.AdminMark) != IsNullOrEmpty(orderModel.AdminMark)))
                {
                    mark += "【客服备注】：" + order.AdminMark + " 修改为：" + orderModel.AdminMark + " ";
                }
                if (order.WarehouseId != orderModel.WarehouseId)
                {
                    mark += "【发货仓库】：" + _wareHouseService.GetById(order.WarehouseId).Name + " 修改为：" + _wareHouseService.GetById(orderModel.WarehouseId).Name + " ";
                    //更新库存锁定
                    if (order.WarehouseId != orderModel.WarehouseId)
                    {
                        var changeWHSPRes = _productService.ChangeWareHouseSetProLockedLog(order.Id, order.WarehouseId, orderModel.WarehouseId);
                        if (!changeWHSPRes)
                        {
                            return Error("修改仓库锁定仓库库存时失败！");
                        }
                    }
                }
                if (order.ToWarehouseMessage != orderModel.ToWarehouseMessage && (IsNullOrEmpty(order.ToWarehouseMessage) != IsNullOrEmpty(orderModel.ToWarehouseMessage)))
                {
                    mark += "【给仓库留言】：" + order.ToWarehouseMessage + " 修改为：" + orderModel.ToWarehouseMessage + " ";
                }
                order.CustomerMark = orderModel.CustomerMark;
                order.CustomerName = orderModel.CustomerName;
                order.CustomerPhone = orderModel.CustomerPhone;
                order.AddressDetail = orderModel.AddressDetail;
                order.AdminMark = orderModel.AdminMark;
                order.WarehouseId = orderModel.WarehouseId;
                order.ToWarehouseMessage = orderModel.ToWarehouseMessage;
                _orderService.UpdateOrder(order);

                OrderLog orderLog = new OrderLog();
                if (!string.IsNullOrEmpty(mark)) {
                    #region 订单日志(修改订单信息)                 
                    orderLog.OrderId = order.Id;
                    orderLog.OrderState = order.State;
                    orderLog.PayState = order.PayState;
                    orderLog.OptionType = "修改订单信息";
                    orderLog.Mark = mark;
                    _logService.InsertOrderLog(orderLog);
                    #endregion
                }

                //新增沟通备注信息
                if (!string.IsNullOrEmpty(orderModel.CommunicateMark)) {
                    #region 订单日志(备注沟通)
                    orderLog = new OrderLog();
                    orderLog.OrderId = order.Id;
                    orderLog.OrderState = order.State;
                    orderLog.PayState = order.PayState;
                    orderLog.OptionType = "沟通";
                    orderLog.Mark = orderModel.CommunicateMark;
                    _logService.InsertOrderLog(orderLog);
                    #endregion
                }

                return Success("修改信息成功");
            }
            else
            {
                return Error("修改订单信息失败！");
            }
        }
        [HttpGet]
        public Object GetOrderProductDetial(int id)
        {
            var json = JsonConvert.SerializeObject(_productService.GetSaleProductPriceBySaleProductId(id));
            var jsonresult = JsonConvert.DeserializeObject(json);
            return jsonresult;
        }
        /// <summary>
        /// 为订单发货匹配最佳仓库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult MatchWareHouseForOrder(int id)
        {


            //请求wms匹配仓库（暂时不要删除）
            //int firstWareHouseId = 0;
            //string res = _orderService.MatchWareHouseForOrder(id,out firstWareHouseId);
            //更新订单仓库
            Order order = _orderService.GetOrderByIdB2C(id);
            //直接按地址匹配仓库


            string res = _orderService.MatchFirstWareHouse(order.AddressDetail);
            var result = res.ToObj<MatchWareHouseModel>();
            var mark = "";

            ////如果匹配wms失败按地址分配仓库就近原则    //请求wms匹配仓库（暂时不要删除）
            //if (firstWareHouseId!=0 && !result.isSucc) {
            //    result.wareHouseId = firstWareHouseId;
            //    result.isSucc = true;
            //}
            if (result.isSucc)
            {
                var wh = _wareHouseService.GetById(result.wareHouseId);
                if (wh == null)
                {
                    return Error("仓库信息匹配错误，请重新选择！");
                }
                var oldWarehouse = _wareHouseService.GetById(order.WarehouseId);
                if (oldWarehouse == null)
                {
                    mark = "订单的仓库修改为" + wh.Name;
                }
                else
                {
                    mark = "订单的仓库由" + oldWarehouse.Name + "，修改为" + wh.Name;
                }
                order.WarehouseId = result.wareHouseId;
                _orderService.UpdateOrder(order);
                #region 订单日志(修改订单信息)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = order.Id;
                orderLog.OrderState = order.State;
                orderLog.PayState = order.PayState;
                orderLog.OptionType = "匹配仓库信息";
                orderLog.Mark = mark;
                _logService.InsertOrderLog(orderLog);
                #endregion
                return Json(new { isSucc = true, wareHouseId = result.wareHouseId });
            }
            else
            {

                return Error(result.msg);
            }
        }
        /// <summary>
        /// 手动保存仓库信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveWareHouseForOrder(int id, int wareHouseId)
        {
            /* 更新库存锁定信息
             * SaleProductWareHouseStock SaleProductLockedTrack
             */
            var wh = _wareHouseService.GetById(wareHouseId);
            if (wh == null)
            {
                return Error("仓库选择错误请重新选择！");
            }
            var mark = "";
            //更新订单仓库
            Order order = _orderService.GetOrderByIdB2C(id);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid,OptTypeEnum.B2CSave);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            var oldWarehouse = _wareHouseService.GetById(order.WarehouseId);
            if (oldWarehouse == null)
            {
                mark = "订单的仓库修改为" + wh.Name;
            }
            else
            {
                mark = "订单的仓库由" + oldWarehouse.Name + "，修改为" + wh.Name;
                //更新库存锁定
                var changeWHSPRes = _productService.ChangeWareHouseSetProLockedLog(order.Id, oldWarehouse.Id, wareHouseId);
                if (!changeWHSPRes)
                {
                    return Error("修改仓库锁定仓库库存时失败！");
                }
            }
            order.WarehouseId = wareHouseId;
            _orderService.UpdateOrder(order);

            #region 订单日志(修改订单信息)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "修改仓库信息";
            orderLog.Mark = mark;
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Json(new { isSucc = true, wareHouseId = wareHouseId });
        }
        /// <summary>
        /// 获取订单商品库存
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetProStock(int productId)
        {
            var data = _productService.GetSaleProductStocksByProductId(productId);
            return Success(data);
        }
        /// <summary>
        /// 获取订单锁定商品库存信息
        /// </summary>
        /// <param name="saleProductId"></param>
        /// <returns></returns>
        public IActionResult GetProLockedTackLog(int saleProductId, int pageIndex, int pageSize,int? wareHouseId)
        {
            var data = _productService.GetAllSaleProductLockedTrackBySaleProId(saleProductId).Where(r => (!wareHouseId.HasValue || r.WareHouseId == wareHouseId || wareHouseId==0));
            return Success(new PageList<SaleProductLockedTrackModel>(data.AsQueryable(), pageIndex, pageSize));
        }
        /// <summary>
        /// 添加订单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <returns></returns>
        public JsonResult AddOrderProducts(OrderProduct orderProduct, string name)
        {
            /*添加商品需要更新
             * （1）锁定跟踪表（SaleProductLockedTrack）
             * （2）oms库存表（SaleProductWareHouseStock）
             * （3）销售商品的可售库存（SaleProduct）
             * （4）更新销售商品的库存到商城
             */
            if (!_permissionService.Authorize("AddB2COrderProducts"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderProduct.OrderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid,OptTypeEnum.B2CSave);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            try
            {
                SaleProduct saleProduct = _productService.GetSaleProductBySaleProductId(orderProduct.SaleProductId);
                if (orderProduct.Quantity > saleProduct.AvailableStock)
                {
                    return Error("商品可用库存不足！");
                }
                var orderPros = _orderService.GetOrderProductsByOrderId(orderProduct.OrderId);
                var samePros = orderPros.Where(r => r.SaleProductId == orderProduct.SaleProductId).ToList();
                var oldQty = samePros.Sum(r => r.Quantity);

                var qty = 0;
                if (samePros.Count() >= 2)
                {
                    return Error("已有商品，且商品记录大于等于2，请删除原来商品从新添加！");
                }
                if (samePros.Count() > 0)
                {
                    //原来已有商品且为1
                    var oldOrderProductId = samePros.FirstOrDefault().Id;
                    samePros.Add(orderProduct);
                    qty = samePros.Sum(r => r.Quantity);
                    var newSumPrice = samePros.Sum(r => r.SumPrice);
                    var newPrice = _commonService.GetNewDecimalNotRounding(newSumPrice / qty);
                    if (newSumPrice % qty == 0)
                    {
                        //除尽
                        var oldOrderPro = _orderService.GetOrderProductByIdOne(oldOrderProductId);
                        oldOrderPro.Price = newPrice;
                        oldOrderPro.Quantity = qty;
                        oldOrderPro.SumPrice = newSumPrice;
                        _orderService.UpdateOrderProduct(oldOrderPro);
                    }
                    else
                    {
                        //除不尽
                        var oldOrderPro = _orderService.GetOrderProductByIdOne(oldOrderProductId);
                        oldOrderPro.Price = newPrice;
                        oldOrderPro.Quantity = qty - 1;
                        oldOrderPro.SumPrice = oldOrderPro.Price * oldOrderPro.Quantity;
                        _orderService.UpdateOrderProduct(oldOrderPro);

                        OrderProduct newOrderPro = new OrderProduct();
                        newOrderPro.OrderId = oldOrderPro.OrderId;
                        newOrderPro.SaleProductId = oldOrderPro.SaleProductId;
                        newOrderPro.OrginPrice = oldOrderPro.OrginPrice;
                        newOrderPro.Price = newSumPrice - oldOrderPro.SumPrice;
                        newOrderPro.Quantity = 1;
                        newOrderPro.SumPrice = newOrderPro.Price;
                        _orderService.AddOrderProduct(newOrderPro);
                    }
                }
                else
                {
                    qty = orderProduct.Quantity;
                    decimal sumPrice = orderProduct.SumPrice;
                    //原来没有商品
                    if (orderProduct.SumPrice % orderProduct.Quantity == 0)
                    {
                        //除尽
                        _orderService.AddOrderProductB2C(orderProduct);
                    }
                    else
                    {
                        //没除尽
                        orderProduct.Price = _commonService.GetNewDecimalNotRounding(orderProduct.SumPrice / orderProduct.Quantity);
                        orderProduct.Quantity = orderProduct.Quantity - 1;
                        orderProduct.SumPrice = orderProduct.Price * orderProduct.Quantity;

                        OrderProduct newOrderPro = new OrderProduct();
                        newOrderPro.OrderId = orderProduct.OrderId;
                        newOrderPro.SaleProductId = orderProduct.SaleProductId;
                        newOrderPro.OrginPrice = orderProduct.OrginPrice;
                        newOrderPro.Price = sumPrice - orderProduct.SumPrice;
                        newOrderPro.Quantity = 1;
                        newOrderPro.SumPrice = newOrderPro.Price;

                        _orderService.AddOrderProductB2C(orderProduct);
                        _orderService.AddOrderProductB2C(newOrderPro);
                    }
                }
                //修改订单总价
                decimal finalSumPrice = _orderService.GetOrderProductsByOrderId(orderProduct.OrderId).ToList().Sum(r => r.SumPrice);
                order.SumPrice = finalSumPrice;
                _orderService.UpdateOrder(order);


                //更新销售商品的可售库存
                saleProduct.LockStock += qty - oldQty;
                saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                _productService.UpdateSaleProduct(saleProduct);
                //更新销售商品的库存到商城
                var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                _productService.SyncProductStockToAssist(501, saleProduct.Product.Code, saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock)));

                //把商品锁定添加到锁定跟踪表（SaleProductLockedTrack）以及oms库存表（SaleProductWareHouseStock）
                foreach(var i in _orderService.GetOrderProductsByOrderId(orderProduct.OrderId).Where(r => r.SaleProductId == orderProduct.SaleProductId).ToList())
                {
                    //判断是否应该更新锁定
                    var checkIsNeed = _productService.GetSaleProductLockedTrackById(order.Id, saleProduct.Id, i.Id);
                    if (checkIsNeed != null && checkIsNeed.LockNumber == i.Quantity)
                    {
                        continue;
                    }
                    else
                    {
                        var num = 0;
                        if (checkIsNeed != null)
                        {
                            num = checkIsNeed.LockNumber;
                        }
                        var result = _productService.AddSaleProductLockedTrackAndWareHouseStock(order.Id, saleProduct.Id, order.WarehouseId, i.Quantity - num, i.Id);
                        if (!result.Contains("成功"))
                        {
                            #region 订单日志(更新锁定信息失败)
                            _logService.InsertOrderLog(order.Id, "更新锁定信息失败", order.State, order.PayState, result);
                            #endregion
                        }
                    }
                }
                #region 订单日志(添加订单商品)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = order.Id;
                orderLog.OrderState = order.State;
                orderLog.PayState = order.PayState;
                orderLog.OptionType = "添加订单商品";
                orderLog.Mark = "添加订单商品【商品名：" + name + " 价格：" + orderProduct.Price + "】数量：" + qty;
                _logService.InsertOrderLog(orderLog);
                #endregion


                return Success("商品添加成功！");
            }
            catch (Exception ex)
            {

                _logService.Error("B2C订单" + order.SerialNumber + "添加商品失败：", ex);
                return Error("B2C订单" + order.SerialNumber + "添加商品出现错误！");
            }
        }
        /// <summary>
        /// 删除订单商品
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderProductId"></param>
        /// <returns></returns>
        public IActionResult DelOrderProduct(int id, int orderId, string name)
        {
            /* AddSaleProductLockedTrackAndWareHouseStock 写在删除商品前，不然无法获取到商品信息
            * 添加商品需要更新
            * （1）锁定跟踪表（SaleProductLockedTrack）
            * （2）oms库存表（SaleProductWareHouseStock）
            * （3）销售商品的可售库存（SaleProduct）
            * （4）更新销售商品的库存到商城
            */
            if (!_permissionService.Authorize("DeleteB2COrderProducts"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderId);
            //判断订单状态
            if (new List<int> {
                OrderState.Uploaded.GetHashCode(),
                OrderState.Invalid.GetHashCode(),
                OrderState.Delivered.GetHashCode(),
                OrderState.B2CConfirmed.GetHashCode(),
                OrderState.Confirmed.GetHashCode()
            }.Contains(order.State.GetHashCode()) || order.IsLocked)
            {
                return Error("当前订单状态不支持修改商品！");
            }
            try
            {
                OrderProduct orderProduct = _orderService.GetOrderProductByIdOne(id);
                SaleProduct saleProduct = _productService.GetSaleProductBySaleProductId(orderProduct.SaleProductId);
                //把商品锁定添加到锁定跟踪表（SaleProductLockedTrack）以及oms库存表（SaleProductWareHouseStock）
                var result = _productService.DeleteProLockedNumByHasLockedNum(order.Id, orderProduct.Id);
                if (!result)
                {
                    #region 订单日志(更新锁定信息失败)
                    _logService.InsertOrderLog(order.Id, "更新锁定信息失败", order.State, order.PayState, "更新锁定信息失败");
                    #endregion
                }
                _orderService.DeleteOrderProductById(id);

                //修改订单总价
                decimal sumPrice = 0;
                foreach (var i in _orderService.GetOrderProductsByOrderId(orderId))
                {
                    sumPrice += i.SumPrice;
                }
                order.SumPrice = sumPrice;
                _orderService.UpdateOrder(order);

                //更新销售商品库存
                saleProduct.LockStock -= orderProduct.Quantity;
                saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                _productService.UpdateSaleProduct(saleProduct);
                //更新销售商品的库存到商城
                var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                _productService.SyncProductStockToAssist(501, saleProduct.Product.Code, saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock)));

                #region 订单日志(删除订单商品)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = order.Id;
                orderLog.OrderState = order.State;
                orderLog.PayState = order.PayState;
                orderLog.OptionType = "删除订单商品";
                orderLog.Mark = "删除订单商品【商品名：" + saleProduct.Product.Name + "】";
                _logService.InsertOrderLog(orderLog);
                #endregion


                return Success("删除成功！");
            }
            catch (Exception ex)
            {
                _logService.Error("B2C订单" + order.SerialNumber + "删除商品失败：", ex);
                return Error("B2C订单" + order.SerialNumber + "删除商品出现错误！");
            }

        }
        /// <summary>
        /// 添加支付信息
        /// </summary>
        /// <param name="orderPayPrice"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddOrderPayPrice(OrderPayPrice orderPayPrice)
        {
            if (!_permissionService.Authorize("AddB2COrderPayPrice"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderPayPrice.OrderId);
            //检测订单状态是否可以操作
            var checkResult = "";
            if (order.Type == OrderType.B2C_TH)
            {
                checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.ToBeConfirmed,OptTypeEnum.B2CRModify);
            }
            else
            {
                checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid, OptTypeEnum.B2CSave);
            }
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            orderPayPrice.IsPay = true;
            _orderService.AddOrderPayPrice(orderPayPrice);


            order.PayState = PayState.Success;
            order.PayType = orderPayPrice.PayType;
            order.PayPrice = orderPayPrice.Price;
            order.State = OrderState.Paid;
            order.PayDate = DateTime.Now;
            _orderService.UpdateOrder(order);

            Dictionary<int, string> PayTypeName = new Dictionary<int, string>();
            foreach (var i in _commonService.GetBaseDictionaryList(DictionaryType.PayType))
            {
                PayTypeName.Add(i.Id, i.Value);
            }

            #region 订单日志(修改支付信息)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = orderPayPrice.OrderId;
            orderLog.OrderState = order.State;
            orderLog.PayState = PayState.Success;
            orderLog.OptionType = "修改支付方式";
            orderLog.Mark = "修改支付方式【支付类型：" + PayTypeName[orderPayPrice.PayType] + " 金额：" + orderPayPrice.Price + "】";
            _logService.InsertOrderLog(orderLog);
            #endregion


            return Success("修改成功！");
        }
        /// <summary>
        /// 发票信息添加
        /// </summary>
        /// <param name="invoiceInfoModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddInvoiceInfo(InvoiceInfoModel invoiceInfoModel, int InvoiceTypes, int InoviceModes)
        {
            if (!_permissionService.Authorize("AddInvoiceInfo"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(invoiceInfoModel.OrderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid, OptTypeEnum.B2CSave);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            InvoiceInfo invoiceInfo = _orderService.GetOrderInvoiceRecord(invoiceInfoModel.OrderId);
            if (invoiceInfo == null)
            {
                if (invoiceInfoModel.CustomerEmail == null)//若邮箱为空 默认插入空格
                    invoiceInfoModel.CustomerEmail = "";
                _orderService.SubmitOrderInvoiceInfo(invoiceInfoModel, invoiceInfoModel.OrderId);
            }
            else
            {
                invoiceInfo.OrderId = invoiceInfoModel.OrderId;
                if (invoiceInfoModel.CustomerEmail == null)//若邮箱为空 默认插入空格
                    invoiceInfo.CustomerEmail = "";
                else
                    invoiceInfo.CustomerEmail = invoiceInfoModel.CustomerEmail;
                invoiceInfo.Title = invoiceInfoModel.Title;
                invoiceInfo.TaxpayerID = invoiceInfoModel.TaxpayerID;
                invoiceInfo.RegisterAddress = invoiceInfoModel.RegisterAddress;
                invoiceInfo.RegisterTel = invoiceInfoModel.RegisterTel;
                invoiceInfo.BankOfDeposit = invoiceInfoModel.BankOfDeposit;
                invoiceInfo.BankAccount = invoiceInfoModel.BankAccount;
                invoiceInfo.BankCode = invoiceInfoModel.BankCode;
                invoiceInfo.InvoiceNo = invoiceInfoModel.InvoiceNo;
                _orderService.UpdateOrderInvoiceInfo(invoiceInfo);
            }

            order.InvoiceType = (InvoiceType)InvoiceTypes;
            order.InvoiceMode = (InvoiceModeEnum)InoviceModes;
            _orderService.UpdateOrder(order);

            #region 订单日志(修改发票信息)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "修改发票信息";
            orderLog.Mark = "修改发票信息【发票类型：" + order.InvoiceType.Description() + " 发票抬头：" + invoiceInfoModel.Title + "】";
            _logService.InsertOrderLog(orderLog);
            #endregion

            var dateTime = DateTime.Now.ToString("yyMMddHHmmssff");
            return Success("修改成功！");
        }
        /// <summary>
        /// 修改快递方式
        /// </summary>
        /// <returns></returns>
        public IActionResult AddDeliveryType(int orderId, int deliveryTypeId, string deliveryNumber = "", DateTime? deliveryDate = null)
        {
            if (!_permissionService.Authorize("AddDeliveryType"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            var checkResult = "";
            if (order.Type == OrderType.B2C_TH)
            {
                checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.ToBeConfirmed, OptTypeEnum.B2CRModify);
            }
            else
            {
                checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid, OptTypeEnum.B2CSave);
            }
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            order.DeliveryTypeId = deliveryTypeId;
            order.DeliveryNumber = deliveryNumber;
            order.DeliveryDate = deliveryDate;
            _orderService.UpdateOrder(order);



            Dictionary<int, string> delivery = new Dictionary<int, string> { };
            foreach (var item in _orderService.GetAllDeliveryList())
            {
                delivery.Add(item.Id, item.Name);
            }


            #region 订单日志(修改快递方式)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "修改快递方式";
            orderLog.Mark = "修改快递方式【快递类型:" + delivery[order.DeliveryTypeId] + " 快递单号：" + order.DeliveryNumber + " 快递日期：" + order.DeliveryDate + "】";
            _logService.InsertOrderLog(orderLog);
            #endregion

            return Success("修改成功！");
        }
        /// <summary>
        /// 订单转单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult TurnOrder(int orderId)
        {
            if (!_permissionService.Authorize("TurnOrder"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderId);
            OrderPayPrice orderPayPrice = _orderService.GetOrderPay(orderId);


            #region 订单日志(转单)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            if (orderPayPrice == null)
            {
                orderLog.PayState = PayState.Fail;
                order.State = OrderState.Unpaid;
                order.TransDate = DateTime.Now;
                orderLog.OrderState = OrderState.Unpaid;
            }
            else
            {
                if (orderPayPrice.IsPay == true)
                {
                    orderLog.PayState = PayState.Success;
                    order.State = OrderState.Paid;
                    order.TransDate = DateTime.Now;
                    orderLog.OrderState = OrderState.Paid;
                }
                else if (orderPayPrice.IsPay == false)
                {
                    orderLog.PayState = PayState.Fail;
                    order.State = OrderState.Unpaid;
                    order.TransDate = DateTime.Now;
                    orderLog.OrderState = OrderState.Unpaid;
                }
            }
            orderLog.OptionType = "订单转单";
            orderLog.Mark = "转单";

            _orderService.UpdateOrder(order);
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Success();
        }
        /// <summary>
        /// 锁定订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult LockOrder(int orderId)
        {
            if (!_permissionService.Authorize("LockOrder"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.B2CConfirmed, OptTypeEnum.B2CConfirmed,order.IsLocked, true);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            order.IsLocked = true;
            _orderService.UpdateOrder(order);

            #region 订单日志(锁定订单)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "锁定订单";
            orderLog.Mark = "锁定订单";
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Success();
        }
        /// <summary>
        /// 解锁订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult UnLockOrder(int orderId)
        {
            if (!_permissionService.Authorize("UnLockOrder"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            if (order.State != OrderState.Paid)
            {
                return Error("当前订单状态不满足操作要求，请刷新页面查看订单是否已经被其他同事操作过了！");
            }else if (order.State == OrderState.Paid && !order.IsLocked)
            {
                return Error("当前订单状态不满足操作要求，请刷新页面查看订单是否已经被其他同事操作过了！（已解锁）");
            }
            order.IsLocked = false;
            _orderService.UpdateOrder(order);

            #region 订单日志(解锁)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "解锁订单";
            orderLog.Mark = "解锁订单";
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Success();
        }
        /// <summary>
        /// 确认订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmOrder(int orderId, OrderType orderType, OrderModel orderModel)
        {
            if (orderType == OrderType.B2C_TH)
            {
                if (!_permissionService.Authorize("ConfirmB2CRefundOrder"))
                {
                    return Error("无操作权限！");
                }
            }
            else if (orderType == OrderType.B2C_XH)
            {
                if (!_permissionService.Authorize("ConfirmOrder"))
                {
                    return Error("无操作权限！");
                }
            }
            else
            {
                return Error("未知权限错误！");
            }


            Order order = _orderService.GetOrderByIdB2C(orderId);
            if (order==null) {
                return Error("未找到订单信息！");
            }
            //检测订单状态是否可以操作
            OptTypeEnum opte = new OptTypeEnum();
            if (order.Type == OrderType.B2C_TH)
            {
                opte = OptTypeEnum.B2CRConfirmed;
            }
            else {
                opte = OptTypeEnum.B2CConfirmed;
            }
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.B2CConfirmed,opte);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            //判断订单是否缺货（缺货不让确认订单）
            var isLackStock = _orderService.IsOrderOutofStock(order);
            if (isLackStock)
            {
                return Error("订单中存在商品缺货，请进行缺货解锁或者进行拆单处理！");
            }
            //判断订单是否被修改
            if ((order.CustomerMark != orderModel.CustomerMark && (IsNullOrEmpty(order.CustomerMark) != IsNullOrEmpty(orderModel.CustomerMark)))
                || order.CustomerName != orderModel.CustomerName
                || order.CustomerPhone != orderModel.CustomerPhone
                || order.AddressDetail != orderModel.AddressDetail 
                || (order.AdminMark != orderModel.AdminMark && (IsNullOrEmpty(order.AdminMark) != IsNullOrEmpty(orderModel.AdminMark))) 
                || (order.ToWarehouseMessage != orderModel.ToWarehouseMessage && (IsNullOrEmpty(order.ToWarehouseMessage) != IsNullOrEmpty(orderModel.ToWarehouseMessage))) 
                || order.WarehouseId != orderModel.WarehouseId 
                || !string.IsNullOrEmpty(orderModel.CommunicateMark))
            {
                return Error("订单信息被修改，请点击保存后确认订单！");   
            }
            if (order.WarehouseId == 0)
            {
                return Error("请选择并保存仓库信息！");
            }
            if (order.Type == OrderType.B2C_TH)
            {
                order.State = OrderState.B2CConfirmed;
                _orderService.UpdateOrder(order);

                #region 订单日志(确认订单)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = order.Id;
                orderLog.OrderState = order.State;
                orderLog.PayState = order.PayState;
                orderLog.OptionType = "确认订单";
                orderLog.Mark = "确认订单";
                _logService.InsertOrderLog(orderLog);
                #endregion
                return Success();
            }
            else
            {
                OrderPayPrice orderPayPrice = _orderService.GetOrderPay(orderId);
                if (orderPayPrice == null || orderPayPrice.IsPay == false)
                {
                    return Error("订单未支付");
                }
                else if (order.SumPrice > (order.PayPrice + (decimal)order.ZMWineCoupon + (decimal)order.WineWorldCoupon))
                {
                    return Error("订单未支付完全，请修改支付价格后提交！");
                }
                else if (order.SumPrice < (order.PayPrice + (decimal)order.ZMWineCoupon + (decimal)order.WineWorldCoupon))
                {
                    return Error("订单支付金额大于订单总金额，请修改支付价格后提交！");
                }
                else
                {
                    order.State = OrderState.B2CConfirmed;
                    _orderService.UpdateOrder(order);

                    #region 订单日志(确认订单)
                    OrderLog orderLog = new OrderLog();
                    orderLog.OrderId = order.Id;
                    orderLog.OrderState = order.State;
                    orderLog.PayState = order.PayState;
                    orderLog.OptionType = "确认订单";
                    orderLog.Mark = "确认订单";
                    _logService.InsertOrderLog(orderLog);
                    #endregion
                    return Success();
                }
            }
        }

        /// <summary>
        /// 反确认订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult UnConfirmOrder(int orderId, OrderType orderType)
        {
            if (orderType == OrderType.B2C_TH)
            {
                if (!_permissionService.Authorize("UnConfirmB2CRefundOrder"))
                {
                    return Error("无操作权限！");
                }
            }
            else if (orderType == OrderType.B2C_XH)
            {
                if (!_permissionService.Authorize("UnConfirmOrder"))
                {
                    return Error("无操作权限！");
                }
            }
            else
            {
                return Error("未知权限错误！");
            }

            Order order = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid, OptTypeEnum.B2CReConfirmed);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            if (orderType == OrderType.B2C_TH)
            {
                order.State = OrderState.ToBeConfirmed;
            }
            else
            {
                order.State = OrderState.Paid;//订单上一级状态为已付款
            }
            _orderService.UpdateOrder(order);

            #region 订单日志(反确认订单)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "反确认订单";
            orderLog.Mark = "反确认订单";
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Success();
        }

        /// <summary>
        /// 设置订单无效
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult SetInvalid(int orderId)
        {
            /* 释放库存
             * 更新表SaleProduct,SaleProductLockedTrack，SaleProductWareHouseStock
             * 订单接口部分也有类似功能[OrderService][ApiOffLineUpdateOrder]需一并修改
             */
            if (!_permissionService.Authorize("SetInvalid"))
            {
                return Error("无操作权限！");
            }
            try
            {
                Order order = _orderService.GetOrderByIdB2C(orderId);
                order.State = OrderState.Invalid;
                //order.Isvalid = false;
                order.ModifiedBy = _workContext.CurrentUser.Id;
                _omsAccessor.Update(order);

                //设置无效单之后修改订单里面销售商品的库存信息
                var productStockDataList = new List<ProductStockData>();
                foreach (var itemOrderProduct in order.OrderProduct)
                {
                    var saleProduct = _productService.GetSaleProductBySaleProductId(itemOrderProduct.SaleProductId);
                    saleProduct.LockStock -= itemOrderProduct.Quantity;
                    saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                    saleProduct.ModifiedBy = _workContext.CurrentUser.Id;
                    _omsAccessor.Update(saleProduct);
                    //更新销售商品的库存到商城
                    var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                    var productStockData = new ProductStockData
                    {
                        sd_id = "501",
                        goods_sn = saleProduct.Product.Code,
                        stock_num = (saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock))).ToString(),
                        stock_detail_list = _productService.GetSaleProductWareHouseStocksByProductCode(saleProduct.Product.Code)
                    };
                    productStockDataList.Add(productStockData);
                    //释放库存
                    var result = _productService.DeleteProLockedNumByHasLockedNum(order.Id, itemOrderProduct.Id);
                    if (!result)
                    {
                        _logService.InsertOrderLog(order.Id, "更新锁定信息失败", order.State, order.PayState, "【" + saleProduct.Product.Code + "】 数量：" + itemOrderProduct.Quantity);
                    }
                }

                //批量更新库存到商城
                _productService.SyncMoreProductStockToAssist(productStockDataList);
                #region 订单日志(设置订单无效)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = order.Id;
                orderLog.OrderState = order.State;
                orderLog.PayState = order.PayState;
                orderLog.OptionType = "设置订单无效";
                orderLog.Mark = "订单无效";
                orderLog.CreatedBy = _workContext.CurrentUser.Id;
                _omsAccessor.Insert(orderLog);
                _omsAccessor.SaveChanges();
                #endregion
                return Success();
            }
            catch (Exception ex)
            {
                _logService.Error("设置B2C订单无效错误：", ex);
                return Error("设置B2C订单无效错误！");
            }
        }
        /// <summary>
        /// 拆分订单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SplitOrder(int orderId, string productInfo)
        {
            if (!_permissionService.Authorize("SplitOrder"))
            {
                return Error("无操作权限！");
            }
            var oldOrderInfo = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(oldOrderInfo.State, OrderState.Invalid, OptTypeEnum.B2CSplit,false, false, (bool)oldOrderInfo.IsCopied);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            var data = JArray.Parse(productInfo);
            Dictionary<int, int> saleproductsInfo = new Dictionary<int, int>();
            foreach (var item in data)
            {
                saleproductsInfo.Add(Convert.ToInt32(item["splitOrderProductId"].ToString()), Convert.ToInt32(item["splitQuantity"].ToString()));
            }
            List<string> newOrderSerialNum = new List<string>();
            List<int> splitNewOrders = new List<int>();
            #region 拆分成两个订单
            for (int newOrderNum = 1; newOrderNum <= 2; newOrderNum++)
            {
                //订单新增
                Order oldOrder = _orderService.GetOrderByIdB2C(orderId);
                #region newOrder
                Order newOrder = new Order
                {
                    SerialNumber =_commonService.GetOrderSerialNumber(),
                    Type = oldOrder.Type,
                    ShopId = oldOrder.ShopId,
                    PSerialNumber = oldOrder.PSerialNumber + "(" + newOrderNum + ")",
                    OrgionSerialNumber = oldOrder.SerialNumber + "(" + newOrderNum + ")",
                    State = oldOrder.State,
                    UserName = oldOrder.UserName,
                    WriteBackState = oldOrder.WriteBackState,
                    PayType = oldOrder.PayType,
                    PayMentType = oldOrder.PayMentType,
                    PayState = oldOrder.PayState,
                    TransDate = oldOrder.TransDate,
                    IsLocked = oldOrder.IsLocked,
                    LockMan = oldOrder.LockMan,
                    LockStock = oldOrder.LockStock,
                    SumPrice = oldOrder.SumPrice,
                    PayPrice = oldOrder.PayPrice,
                    PayDate = DateTime.Now,
                    DeliveryTypeId = oldOrder.DeliveryTypeId,
                    DeliveryNumber = oldOrder.DeliveryNumber,
                    DeliveryDate = oldOrder.DeliveryDate,
                    CustomerName = oldOrder.CustomerName,
                    CustomerPhone = oldOrder.CustomerPhone,
                    AddressDetail = oldOrder.AddressDetail,
                    DistrictId = oldOrder.DistrictId,
                    CustomerMark = oldOrder.CustomerMark,
                    AdminMark = oldOrder.AdminMark,
                    ToWarehouseMessage = oldOrder.ToWarehouseMessage,
                    WarehouseId = oldOrder.WarehouseId,
                    PriceTypeId = oldOrder.PriceTypeId,
                    CustomerId = oldOrder.CustomerId,
                    ApprovalProcessId = oldOrder.ApprovalProcessId,
                    AppendType = AppendType.Split
                };
                #endregion
                if (newOrderNum == 1)
                {
                    //只给第一个拆分订单分红酒券
                    newOrder.WineWorldCoupon = oldOrder.WineWorldCoupon;
                    newOrder.ZMWineCoupon = oldOrder.ZMWineCoupon;
                }
                newOrder = _orderService.CreatedB2COrder(newOrder);

                //订单商品
                decimal newOrderSumPayPrice;
                decimal newOrderSumPrice;
                int newOrderId = newOrder.Id;
                var newOrderSaleProductsCount = _orderService.AddProductsToNewOrder(orderId, newOrderId, saleproductsInfo, newOrderNum, out newOrderSumPayPrice, out newOrderSumPrice);


                //订单发票
                InvoiceInfo invoiceInfo = _orderService.GetOrderInvoiceRecord(orderId);
                if (invoiceInfo != null)
                {
                    InvoiceInfoModel invoiceInfoModel = new InvoiceInfoModel();
                    invoiceInfoModel.OrderId = newOrderId;
                    invoiceInfoModel.CustomerEmail = invoiceInfo.CustomerEmail;
                    invoiceInfoModel.Title = invoiceInfo.Title;
                    invoiceInfoModel.TaxpayerID = invoiceInfo.TaxpayerID;
                    invoiceInfoModel.RegisterAddress = invoiceInfo.RegisterAddress;
                    invoiceInfoModel.RegisterTel = invoiceInfo.RegisterTel;
                    invoiceInfoModel.BankOfDeposit = invoiceInfo.BankOfDeposit;
                    invoiceInfoModel.BankAccount = invoiceInfo.BankAccount;
                    invoiceInfoModel.BankCode = invoiceInfo.BankCode;
                    invoiceInfoModel.InvoiceNo = invoiceInfo.InvoiceNo;
                    _orderService.SubmitOrderInvoiceInfo(invoiceInfoModel, newOrderId);
                }


                //订单支付
                OrderPayPrice oldOrderPayPrice = _orderService.GetOrderPay(orderId);
                OrderPayPrice newOrderPayPrice = new OrderPayPrice();
                if (oldOrderPayPrice != null && oldOrderPayPrice.IsPay == true)
                {
                    newOrderPayPrice.OrderId = newOrderId;
                    newOrderPayPrice.IsPay = true;
                    newOrderPayPrice.PayType = oldOrderPayPrice.PayType;
                    newOrderPayPrice.Price = newOrderSumPayPrice;
                    newOrderPayPrice.Mark = "此价格为原单号价格平均价*新订单酒款数量";
                    //修改订单总价及支付价格
                    newOrder.SumPrice = newOrderSumPrice;
                    if (newOrderNum == 1)
                    {
                        //只给第一个拆分订单分红酒券
                        newOrderPayPrice.Price = newOrderSumPayPrice - ((decimal)newOrder.WineWorldCoupon + (decimal)newOrder.ZMWineCoupon);
                        newOrder.PayPrice = newOrderPayPrice.Price;
                    }
                    else
                    {
                        newOrder.PayPrice = newOrderPayPrice.Price;
                    }
                    newOrder.PayDate = DateTime.Now;
                    _orderService.UpdateOrder(newOrder);

                    _orderService.AddOrderPayPrice(newOrderPayPrice);
                }




                Order order = _orderService.GetOrderByIdB2C(orderId);
                #region 订单日志(拆单)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = newOrder.Id;
                orderLog.OrderState = newOrder.State;
                orderLog.PayState = newOrder.PayState;
                orderLog.OptionType = "拆单";
                orderLog.Mark = "由订单：" + order.SerialNumber + "拆分生成订单：" + newOrder.SerialNumber;
                _logService.InsertOrderLog(orderLog);
                #endregion

                newOrderSerialNum.Add(newOrder.SerialNumber);
                splitNewOrders.Add(newOrder.Id);
            }


            //设置旧订单状态
            Order order1 = _orderService.GetOrderByIdB2C(orderId);
            order1.State = OrderState.Invalid;
            order1.IsCopied = true;
            _orderService.UpdateOrder(order1);

            #region 订单日志(拆单)
            OrderLog orderLog1 = new OrderLog();
            orderLog1.OrderId = order1.Id;
            orderLog1.OrderState = order1.State;
            orderLog1.PayState = order1.PayState;
            orderLog1.OptionType = "拆单";
            orderLog1.Mark = "订单：" + order1.SerialNumber + "拆分生成订单：" + newOrderSerialNum[0] + " 和 " + newOrderSerialNum[1];
            _logService.InsertOrderLog(orderLog1);
            #endregion

            //解除旧订单商品锁定
            _productService.ChangeProLockedNumByHasLockedNum(order1.Id);
            //设置新订单商品锁定
            foreach (var item in splitNewOrders)
            {
                var orderInfo = _orderService.GetOrderById(item);
                var orderProsInfo = _orderService.GetOrderProductsByOrderId(item).ToList();
                foreach (var proItem in orderProsInfo)
                {
                    var result = _productService.AddSaleProductLockedTrackAndWareHouseStock(orderInfo.Id, proItem.SaleProductId, orderInfo.WarehouseId, proItem.Quantity, proItem.Id);//事务
                    var getProName = _productService.GetSaleProductDetailBySaleProductId(proItem.SaleProductId);
                    if (!result.Contains("成功"))
                    {
                        _logService.InsertOrderLog(orderInfo.Id, "更新库存锁定失败", orderInfo.State, orderInfo.PayState, "【" + getProName.ProductName + "】 数量：" + proItem.Quantity);
                    }
                }
            }
            #endregion

            return Success();
        }
        /// <summary>
        /// 订单退货（部分退货）
        /// </summary>
        /// <returns></returns>
        public JsonResult PartRefund(int orderId,string refundProductInfo) {
            if (!_permissionService.Authorize("SplitOrder"))
            {
                return Error("无操作权限！");
            }

            var refundProductInfoData = JArray.Parse(refundProductInfo);
            using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    //权限
                    var data = _orderService.GetOrderByIdB2C(orderId);
                    if (data == null || !data.State.Equals(OrderState.Delivered)) return Error("未找到订单信息或者订单没有发货不能进行退单操作！");
                    //判断订单的退货程度
                    if (_orderService.JudgeOrderRefundState(orderId) == RefundState.All) return Error("订单已经全部不能在进行部分退货！");
                    var num = _commonService.GetOrderSerialNumber("RF");
                    #region 销售退单模型
                    Order refundOrderModel = new Order
                    {
                        SerialNumber = num,
                        Type = OrderType.B2C_TH,
                        ShopId = data.ShopId,
                        PSerialNumber = data.PSerialNumber + "(T" + (_omsAccessor.Get<Order>().Where(x => x.Isvalid && x.Type == OrderType.B2C_TH && x.OriginalOrderId == data.Id).Count() + 1) + ")",
                        OrgionSerialNumber = data.SerialNumber + "(T" + (_omsAccessor.Get<Order>().Where(x => x.Isvalid && x.Type == OrderType.B2C_TH && x.OriginalOrderId == data.Id).Count() + 1) + ")" + (string.IsNullOrEmpty(data.OrgionSerialNumber) ? "" : "|" + data.OrgionSerialNumber),
                        OriginalOrderId = data.Id,
                        State = OrderState.Paid,
                        SumPrice = data.SumPrice,
                        CustomerName = data.CustomerName,
                        CustomerPhone = data.CustomerPhone,
                        AddressDetail = data.AddressDetail,
                        DistrictId = data.DistrictId,
                        DeliveryTypeId = data.DeliveryTypeId,
                        InvoiceType = data.InvoiceType,
                        AdminMark = data.AdminMark,
                        ToWarehouseMessage = data.ToWarehouseMessage,
                        WarehouseId = data.WarehouseId,
                        PayDate = DateTime.Now,
                        PayState = PayState.Success,
                        PayType = data.PayType,
                        PayPrice = data.SumPrice,
                        PayMentType = data.PayMentType
                    };
                    #endregion
                    var rfOrder = _orderService.CreatedB2COrder(refundOrderModel);

                    //插入商品
                    var partRefundProductRes = _orderService.AddPartRefundProducts(data.Id, rfOrder.Id, refundProductInfoData);
                    if (!partRefundProductRes) { tran.Rollback(); return Error("部分退货发生错误！");  }
                    _logService.InsertOrderLog(rfOrder.Id, "生成销售退单", rfOrder.State, rfOrder.PayState, "生成销售退单[" + num + "]");//创建退单日志

                    //插入商品后重新计算退单的总金额
                    rfOrder.SumPrice = _orderService.GetOrderProductsByOrderId(rfOrder.Id).Sum(x => x.SumPrice);
                    rfOrder.PayPrice = rfOrder.SumPrice;
                    _orderService.UpdateOrder(rfOrder);
                    //添加支付信息
                    var orderPayPrice = new OrderPayPrice
                    {
                        OrderId = rfOrder.Id,
                        IsPay = true,
                        PayType = rfOrder.PayType,
                        PayMentType = rfOrder.PayMentType,
                        Price = rfOrder.PayPrice,
                    };
                    _orderService.AddOrderPayPrice(orderPayPrice);
                    //退单生成完成后更新订单状态
                    if (_orderService.JudgeOrderRefundState(data.Id) == RefundState.All)
                    {
                        data.State = OrderState.Invalid;
                        data = _orderService.UpdateOrder(data);
                        _logService.InsertOrderLog(data.Id, "销售退单", data.State, data.PayState, "订单全部退货完成已作废,生成销售退单[" + num + "]");//订单退单日志
                    }
                    else {
                        _logService.InsertOrderLog(data.Id, "销售退单", data.State, data.PayState, "订单部分退货,生成销售退单[" + num + "]");//订单退单日志
                    }

                    tran.Commit();
                    return Success(new { refundOrderId = rfOrder.Id, refundSerialNumber = rfOrder.SerialNumber });
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    _logService.Error("部分退货失败：" + ex.Message);
                    return Error("部分退货异常！");
                }
            }              
        }
        /// <summary>
        /// 订单退货（一键退货）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult RefundOrder(int id)
        {
            //权限
            if (!_permissionService.Authorize("SplitOrder"))
            {
                return Error("无操作权限！");
            }
            using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {                  
                    var data = _orderService.GetOrderByIdB2C(id);

                    if (data == null) return Error("未找到订单信息！");
                    //判断订单的退货程度
                    if (_orderService.JudgeOrderRefundState(id) != RefundState.No) return Error("订单已经产生了退货不能在进行一键退货！");
                    var num = _commonService.GetOrderSerialNumber("RF");

                    if (data.State.Equals(OrderState.Delivered))
                    {
                        #region 销售退单模型
                        Order refundOrderModel = new Order
                        {
                            SerialNumber = num,
                            Type = OrderType.B2C_TH,
                            ShopId = data.ShopId,
                            PSerialNumber = data.PSerialNumber + "(T" + (_omsAccessor.Get<Order>().Where(x => x.Isvalid && x.Type == OrderType.B2C_TH && x.OriginalOrderId == data.Id).Count() + 1) + ")",
                            OrgionSerialNumber = data.SerialNumber + "(T" + (_omsAccessor.Get<Order>().Where(x => x.Isvalid && x.Type == OrderType.B2C_TH && x.OriginalOrderId == data.Id).Count() + 1) + ")" + (string.IsNullOrEmpty(data.OrgionSerialNumber) ? "" : "|" + data.OrgionSerialNumber),
                            OriginalOrderId = data.Id,
                            State = OrderState.Paid,
                            SumPrice = data.SumPrice,
                            CustomerName = data.CustomerName,
                            CustomerPhone = data.CustomerPhone,
                            AddressDetail = data.AddressDetail,
                            DistrictId = data.DistrictId,
                            InvoiceType = data.InvoiceType,
                            AdminMark = data.AdminMark,
                            ToWarehouseMessage = data.ToWarehouseMessage,
                            WarehouseId = data.WarehouseId,
                            DeliveryTypeId = data.DeliveryTypeId,
                            PayDate = DateTime.Now,
                            PayState = PayState.Success,
                            PayType = data.PayType,
                            PayPrice = data.SumPrice,
                            PayMentType = data.PayMentType
                        };
                        #endregion
                        var rfOrder = _orderService.CreatedB2COrder(refundOrderModel);

                        //添加支付信息
                        var orderPayPrice = new OrderPayPrice
                        {
                            OrderId = rfOrder.Id,
                            IsPay = true,
                            PayType = rfOrder.PayType,
                            Price = rfOrder.PayPrice,
                            PayMentType = rfOrder.PayMentType
                        };
                        _orderService.AddOrderPayPrice(orderPayPrice);
                        //插入商品
                        _orderService.AddProductsToNewOrder(data.Id, rfOrder.Id, out int saleProductsCount, out decimal newOrderSumPrice);
                        _logService.InsertOrderLog(rfOrder.Id, "生成销售退单", rfOrder.State, rfOrder.PayState, "生成销售退单[" + num + "]");//创建退单日志

                        //退单生成完成后更新订单状态
                        data.State = OrderState.Invalid;
                        data = _orderService.UpdateOrder(data);
                        _logService.InsertOrderLog(data.Id, "销售退单", data.State, data.PayState, "订单已作废,生成销售退单[" + num + "]");//订单退单日志
                        tran.Commit();                                                                                                    //创建退单
                        return Success(new { refundOrderId = rfOrder.Id, refundSerialNumber = rfOrder.SerialNumber });
                    }
                    else
                    {
                        return Error("订单状态处于非已发货状态！");
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    _logService.Error("生成退单失败：" + ex.Message);
                    return Error("生成退单异常！");
                }
            }
          

        }
        /// <summary>
        /// 复制订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IActionResult CopyOrder(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Invalid, OptTypeEnum.B2CCopy,false, false, (bool)order.IsCopied); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            var result = _orderService.CopyB2COrder(orderId);
            if (result.Contains("成功"))
            {
                return Success();
            }
            else
            {
                return Error(result);
            }
        }
        /// <summary>
        /// 等价换货
        /// </summary>
        /// <param name="orderProId"></param>
        /// <param name="saleProId"></param>
        /// <param name="proNum"></param>
        /// <returns></returns>
        public IActionResult ChangeProEqValue(int orderProId, int saleProId, int proNum)
        {
            /* saleProduct锁定库存变更
             * 库存跟踪
             * 仓库库存
             */
            var orginOrderPro = _orderService.GetOrderProductByIdOne(orderProId);//原订单商品
            if (orginOrderPro == null)
            {
                return Error("无法订单商品");
            }
            var order = _orderService.GetOrderByIdB2C(orginOrderPro.OrderId);
            //判断订单状态
            if (new List<int> {
                OrderState.Uploaded.GetHashCode(),
                OrderState.Invalid.GetHashCode(),
                OrderState.Delivered.GetHashCode(),
                OrderState.B2CConfirmed.GetHashCode(),
                OrderState.Confirmed.GetHashCode()
            }.Contains(order.State.GetHashCode()) || order.IsLocked)
            {
                return Error("当前订单状态不支持修改商品！");
            }
            var saleProduct = _productService.GetSaleProductBySaleProductId(saleProId);//替换的销售商品
            if (saleProduct == null)
            {
                return Error("无法获取到替换销售商品数据");
            }
            else
            {
                var result = _orderService.ChangeProEqPrice(orderProId, saleProId, proNum);
                if (result != "")
                {
                    return Error(result);
                }
                return Success();
            }
        }
        /// <summary>
        /// 修改订单商品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <param name="originPrice"></param>
        /// <param name="salePrice"></param>
        /// <param name="sumPrice"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeOrderProduct(int id, int quantity, decimal originPrice, decimal salePrice, decimal sumPrice)
        {
            /* 更新表
             * SaleProduct  
             * SaleProductWareHouseStock
             * SaleProductLockedTrack
             * 更新库存到商城
             */
            if (!_permissionService.Authorize("updateB2COrderProducts"))
            {
                return Error("无操作权限！");
            }
            var orderProduct = _orderService.GetOrderProductByIdOne(id);
            var order = _orderService.GetOrderById(orderProduct.OrderId);
            //判断订单状态
            if (new List<int> {
                OrderState.Uploaded.GetHashCode(),
                OrderState.Invalid.GetHashCode(),
                OrderState.Delivered.GetHashCode(),
                OrderState.B2CConfirmed.GetHashCode(),
                OrderState.Confirmed.GetHashCode()
            }.Contains(order.State.GetHashCode()) || order.IsLocked)
            {
                return Error("当前订单状态不支持修改商品！");
            }
            try
            {
                if (orderProduct == null)
                {
                    return Json(new { isSucc = false, msg = "未找到订单商品，数据错误" });
                }
                SaleProduct saleProduct = _productService.GetSaleProductBySaleProductId(orderProduct.SaleProductId);
                var changeQuantity = quantity - orderProduct.Quantity;
                var oldProSumNumber = orderProduct.Quantity;
                if (changeQuantity > saleProduct.AvailableStock)
                {
                    return Error("商品可用库存不足！");
                }
                //原/新商品价格数量单价总价
                var oldProQty = orderProduct.Quantity;
                var oldProPrice = orderProduct.Price;
                var oldTotalPrice = orderProduct.SumPrice;
                var oldOriPrice = orderProduct.OrginPrice;
                saleProduct.LockStock += changeQuantity;
                saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                _productService.UpdateSaleProduct(saleProduct);
                //更新销售商品的库存到商城
                var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                _productService.SyncProductStockToAssist(501, saleProduct.Product.Code, saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock)));
                var orderPros = _orderService.GetOrderProductsByOrderId(orderProduct.OrderId);
                var count = orderPros.Count(r => r.SaleProductId == orderProduct.SaleProductId);
                //如果总价除以数量除不尽  则添加两条记录
                if ((sumPrice * 100) % quantity == 0)
                {
                    orderProduct.Quantity = quantity;
                    orderProduct.OrginPrice = originPrice;
                    orderProduct.Price = salePrice;
                    orderProduct.SumPrice = sumPrice;
                    orderProduct.ModifiedBy = WorkContext.CurrentUser.Id;
                    orderProduct.ModifiedTime = DateTime.Now;
                    _orderService.UpdateOrderProduct(orderProduct);
                }
                else
                {
                    decimal splitPrice = sumPrice / quantity;
                    decimal sumSplitPrice = _commonService.GetNewDecimalNotRounding(splitPrice) * (quantity - 1);
                    orderProduct.Quantity = quantity - 1;
                    orderProduct.OrginPrice = originPrice;
                    orderProduct.Price = salePrice;
                    orderProduct.SumPrice = sumSplitPrice;
                    orderProduct.ModifiedBy = WorkContext.CurrentUser.Id;
                    orderProduct.ModifiedTime = DateTime.Now;
                    //除余后的商品添加进去
                    if (count > 1)
                    {
                        return Json(new { isSucc = false, msg = "已有两个同样商品！请确认是否需要修改或者先删除其中一个商品再修改！" });
                    }
                    else
                    {
                        OrderProduct newOrderPro = new OrderProduct();
                        newOrderPro.OrderId = order.Id;
                        newOrderPro.SaleProductId = orderProduct.SaleProductId;
                        newOrderPro.Quantity = 1;
                        newOrderPro.OrginPrice = originPrice;
                        newOrderPro.Price = sumPrice - sumSplitPrice;
                        newOrderPro.SumPrice = (sumPrice - sumSplitPrice);
                        _orderService.UpdateOrderProduct(orderProduct);
                        _orderService.AddOrderProduct(newOrderPro);
                    }
                }
                //把商品锁定添加到锁定跟踪表（SaleProductLockedTrack）以及oms库存表（SaleProductWareHouseStock）
                var result = _productService.UpdateOrderProChangeLockedLog(order.Id, saleProduct.Id);
                if (!result.Contains("成功"))
                {
                    #region 订单日志(更新锁定信息失败)
                    _logService.InsertOrderLog(order.Id, "更新锁定信息失败", order.State, order.PayState, result);
                    #endregion
                }
                //修改订单总价
                order.SumPrice = orderPros.Sum(r => r.SumPrice);
                order.ModifiedBy = WorkContext.CurrentUser.Id;
                order.ModifiedTime = DateTime.Now;
                _orderService.UpdateOrder(order);

                #region 订单日志(修改订单商品)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = order.Id;
                orderLog.OrderState = order.State;
                orderLog.PayState = order.PayState;
                orderLog.OptionType = "修改订单商品";
                orderLog.Mark = "修改订单商品【" + orderProduct.SaleProduct.Product.Name + "】【数量：" + oldProQty + " 原价：" + oldOriPrice + " 价格：" + oldProPrice + " 总价：" + oldTotalPrice + "】修改为：【数量：" + quantity + " 原价：" + originPrice + " 价格：" + salePrice + " 总价：" + sumPrice + "】";
                _logService.InsertOrderLog(orderLog);
                #endregion

                return Json(new { isSucc = true, msg = "修改成功！" });
            }
            catch (Exception ex)
            {
                _logService.Error(string.Format("修改订单号为：{0}的商品价格失败，原因：{1}", order.SerialNumber, ex.Message));
                return Json(new { isSucc = false, msg = "修改失败！原因：" + ex.Message });
            }
        }
        [HttpPost]
        /// <summary>
        /// 订单详情备注沟通
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public IActionResult OrderCommunicateMark(int id, string mark)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return Error("数据错误，订单不存在");

            #region 订单日志(备注沟通)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "沟通";
            orderLog.Mark = mark;
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Success();
        }
        /// <summary>
        /// 一键支付订单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult QuickPayOrder(int id) {
            if (!_permissionService.Authorize("AddB2COrderPayPrice"))
            {
                return Error("无操作权限！");
            }

            Order order = _orderService.GetOrderByIdB2C(id);
            if (order == null)
            {
                return Error("没有找到该订单信息！");
            }
            try
            {
                if (order.PayState == PayState.Fail && order.State == OrderState.Unpaid)
                {
                    return Error("订单处于未支付状态，请自己选择支付进行支付！");
                }
                if (order.PayPrice == (order.SumPrice - ((decimal)order.ZMWineCoupon + (decimal)order.WineWorldCoupon)))
                {
                    return Error("订单的总金额与支付金额相等，不用再进行一键支付！");
                }
                order.PayPrice = (order.SumPrice - ((decimal)order.ZMWineCoupon + (decimal)order.WineWorldCoupon));
                order.PayDate = DateTime.Now;
                _orderService.UpdateOrder(order);

                var orderPayPrice = new OrderPayPrice
                {
                    OrderId = id,
                    IsPay = true,
                    PayType = order.PayType,
                    PayMentType = order.PayMentType,
                    Price = order.PayPrice,
                };
                _orderService.AddOrderPayPrice(orderPayPrice);
                Dictionary<int, string> PayTypeName = new Dictionary<int, string>();
                foreach (var i in _commonService.GetBaseDictionaryList(DictionaryType.PayType))
                {
                    PayTypeName.Add(i.Id, i.Value);
                }

                #region 订单日志(一键支付信息)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = orderPayPrice.OrderId;
                orderLog.OrderState = order.State;
                orderLog.PayState = PayState.Success;
                orderLog.OptionType = "一键支付";
                orderLog.Mark = "一键支付【支付类型：" + PayTypeName[orderPayPrice.PayType] + " 金额：" + orderPayPrice.Price + "】";
                _logService.InsertOrderLog(orderLog);
                #endregion
                return Success("一键支付成功！");
            }
            catch (Exception ex)
            {

                _logService.Error("订单" + order.SerialNumber + "一键支付失败：" + ex);
                return Error("订单一键支付失败！");
            }

        }
        /// <summary>
        /// 上传订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [UserAnonymous]
        public async Task<IActionResult> UploadOrder(int id)
        {
            if (!_permissionService.Authorize("UploadB2cOrderToWMS"))
            {
                return Error("无操作权限！");
            }

            var stockRes = _orderService.CheckWareHouseStock(id);//检查库存是否满足
            var stockResult = JsonHelper.ToObj<ResultModel>(stockRes);
            if (stockResult.isSucc)
            {
                var order = _orderService.GetOrderById(id);
                //检测订单状态是否可以操作
                var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Uploaded, OptTypeEnum.B2CUploaded);
                if (checkResult != "")
                {
                    return Error(checkResult);
                }
                //判断订单是否缺货（缺货不让确认订单）
                var isLackStock = _orderService.IsOrderOutofStock(order);
                if (isLackStock)
                {
                    return Error("订单中存在商品缺货，请进行缺货解锁或者进行拆单处理！");
                }

                //判断订单商品锁定库存与仓库库存直接的关系（存在商品库存小于锁定库存的情况这个时候就得还原，重新进行缺货处理！)
                var isCheckWHS = _orderService.IsCheckWarehouseStockCanUse(order);
                if (!isCheckWHS)
                {
                    return Error("订单中存在商品的仓库库存发生了调整，请进行调整订单商品信息后进行提交订单！");
                }

                var delivery = _deliveriesService.GetDeliveryById(order.DeliveryTypeId);
                //如果订单选择德邦快递，则需要检查德邦快递是否满足配送到收货地址
                if (delivery.Code == "DB" || delivery.Name == "德邦")
                {

                    var dbCheckRes = _orderService.CheckDeBangIsDelivery(id);//检查德邦快递是否满足
                    var dbCheckResult = JsonHelper.ToObj<DBDeliveryCheckModel>(dbCheckRes);
                    if (dbCheckResult.isSucc)//调用接口成功
                    {
                        if (dbCheckResult.isDelivery)//满足配送到收货地址
                        {
                            string res = await _orderService.UploadOrder(id);
                            var result = JsonHelper.ToObj<ResultModel>(res);
                            if (result.isSucc)
                            {
                                //var order = _orderService.GetOrderById(id);
                                order.State = OrderState.Uploaded;
                                order.ModifiedBy = WorkContext.CurrentUser.Id;
                                order.ModifiedTime = DateTime.Now;

                                _orderService.UpdateOrder(order);

                                #region 订单日志(确认订单)
                                OrderLog orderLog = new OrderLog();
                                orderLog.OrderId = order.Id;
                                orderLog.OrderState = order.State;
                                orderLog.PayState = order.PayState;
                                orderLog.OptionType = "上传订单";
                                orderLog.Mark = "上传订单";
                                _logService.InsertOrderLog(orderLog);
                                #endregion

                                return Success("订单已完成上传WMS操作！");
                            }
                            else
                            {
                                _logService.Error(result.msg);
                                return Error(result.msg);
                            }
                        }
                        else
                        {
                            return Error("该收货地址德邦快递不能正常配送，请修改地址，修改地址后仍然不能配送请更换配送方式！");
                        }
                    }
                    else
                    {
                        _logService.Error(dbCheckResult.msg);
                        return Error("调用检查德邦快递配送接口失败");
                    }
                }
                else //不是德邦快递，直接上传
                {
                    string res = await _orderService.UploadOrder(id);
                    var result = JsonHelper.ToObj<ResultModel>(res);
                    if (result.isSucc)
                    {
                        //var order = _orderService.GetOrderById(id);
                        order.State = OrderState.Uploaded;
                        order.ModifiedBy = WorkContext.CurrentUser.Id;
                        order.ModifiedTime = DateTime.Now;

                        _orderService.UpdateOrder(order);

                        #region 订单日志(确认订单)
                        OrderLog orderLog = new OrderLog();
                        orderLog.OrderId = order.Id;
                        orderLog.OrderState = order.State;
                        orderLog.PayState = order.PayState;
                        orderLog.OptionType = "上传订单";
                        orderLog.Mark = "上传订单";
                        _logService.InsertOrderLog(orderLog);
                        #endregion

                        return Success("订单已完成上传WMS操作！");
                    }
                    else
                    {
                        _logService.Error(result.msg);
                        return Error(result.msg);
                    }
                }

            }
            else
            {
                _logService.Error(stockResult.msg);
                return Error(stockResult.msg);
            }
        }
        [HttpPost]
        [UserAnonymous]
        public IActionResult CancelUploadB2COrder(int id)
        {
            var order = _orderService.GetOrderById(id);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.B2CConfirmed,OptTypeEnum.B2CCancel);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            string res = _orderService.CancelUploadOrder(id);
            var result = JsonHelper.ToObj<CancelOrderResultModel>(res);
            if (result.isSucc)
            {
                order.State = OrderState.B2CConfirmed;
                order.ModifiedBy = WorkContext.CurrentUser.Id;
                order.ModifiedTime = DateTime.Now;
                _orderService.UpdateOrder(order);

                #region 订单日志(确认订单)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = order.Id;
                orderLog.OrderState = order.State;
                orderLog.PayState = order.PayState;
                orderLog.OptionType = "取消上传";
                orderLog.Mark = "取消订单上传";
                _logService.InsertOrderLog(orderLog);
                #endregion
            }
            return Json(new { Data = result });
        }
        /// <summary>
        /// 导出订单
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="shopId"></param>
        /// <param name="payState"></param>
        /// <param name="orderState"></param>
        /// <param name="search"></param>
        /// <param name="exportType"></param>
        /// <returns></returns>
        public IActionResult ExportOrder(DateTime? startTime, DateTime? endTime, DateTime? payStartTime, DateTime? payEndTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, OrderState? orderState, bool isRefundOrder, string search = "", string exportType = "", bool isOrderDetail = false)
        {
            try
            {
                //获取导出数据
                endTime = endTime?.AddMinutes(1);
                payEndTime = payEndTime?.AddMinutes(1);
                deliverEndTime = deliverEndTime?.AddMinutes(1);
                var data = _orderService.GetAllOrdersByCommand(startTime, endTime, payStartTime, payEndTime, deliverStartTime, deliverEndTime, shopId, orderState, search, isOrderDetail, isRefundOrder);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = CommonTools.OutPutTemplate(OutPutType.B2C);
            
                //是否是导出订单详情
                if (isOrderDetail)
                {
                    columnNames = CommonTools.OutPutTemplate(OutPutType.B2CDetail);
                }
                DataTable table = data.ToDataTable(columnNames);

                //导出.xls格式的文件
                if (exportType == ".xls" || exportType == ".csv")
                {
                    table.TableName = "Sheet1";
                    Stream stream = CommonTools.WriteExcel(table);

                    if (exportType == ".xls")
                    {
                        return File(stream, "application/vnd.ms-excel;charset=UTF-8", fileName + exportType);

                    }
                    else
                    {
                        return File(stream, "text/csv;charset=UTF-8", fileName + exportType);
                    }
                }
                else
                    return null;

            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }
        /// <summary>
        /// B2C订单批量导入
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult B2COrdersImport(IFormFile file)
        {
            IWorkbook workbook = null;
            if (file == null)
            {
                return Error("请添加要导入文件");
            }
            else
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0)
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else if (fileName.IndexOf(".xls") > 0)
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                ISheet sheet = workbook.GetSheetAt(0);
                IRow row;

                StringBuilder errorStr = new StringBuilder();//记录错误信息
                List<int> productIds = new List<int>();//需要同步到商城的商品可用库存Id集合
                int succCount = 0;
                int errorCount = 0;
                List<string> failName = new List<string>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
                    {
                        row = sheet.GetRow(i);

                        if (row.GetCell(0) == null || row.GetCell(3) == null)
                        {
                            continue;
                        }
                        else if(string.IsNullOrEmpty(row.GetCell(3).ToString().Trim()))
                        {
                            continue;
                        }
                        try
                        {
                            //插入订单
                            var order = _orderService.GetOrderByPSerialNumber(row.GetCell(3).ToString().Trim());
                            //判断店铺是否存在
                            var shopIdDic = _omsAccessor.Get<Dictionary>().Where(x => new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.Consignment, DictionaryType.ThirdpartyOnlineSales }.Contains(x.Type) && x.Isvalid && x.Id == Convert.ToInt32(row.GetCell(2).ToString().Trim())).FirstOrDefault();
                            if (shopIdDic == null)
                            {
                                errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,未找到编码为{2}的店铺;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(2).ToString().Trim()));
                                errorCount++;
                                continue;
                            }
                            if (order == null)
                            {
                                order = new Order()
                                {
                                    CreatedTime = Convert.ToDateTime(row.GetCell(1).ToString().Trim()),
                                    ShopId = Convert.ToInt32(row.GetCell(2).ToString().Trim()),
                                    SerialNumber =  _commonService.GetOrderSerialNumber(""),
                                    PSerialNumber = row.GetCell(3).ToString().Trim(),
                                    Type = OrderType.B2C_XH,
                                    State = OrderState.Paid,
                                    WriteBackState = WriteBackState.NoWrite,                              
                                    PayState = PayState.Success,
                                    PayDate = Convert.ToDateTime(row.GetCell(12).ToString().Trim()),
                                    IsLocked = false,
                                    LockStock = true,                                                                
                                    CustomerName = row.GetCell(5).ToString().Trim(),
                                    CustomerPhone = row.GetCell(6).ToString().Trim(),
                                    AddressDetail = row.GetCell(8).ToString().Trim(),                                                                         
                                    ZMCoupon = 0,
                                    ZMWineCoupon = 0,
                                    WineWorldCoupon = 0,
                                    UserName = row.GetCell(4).ToString().Trim(),
                                    IsNeedPaperBag = false,
                                };

                                order.PayType = _commonService.GetDictionaryOfPayByTypeAndValue(DictionaryType.PayType, row.GetCell(10).ToString().Trim()) == 0 ? 119 : _commonService.GetDictionaryOfPayByTypeAndValue(DictionaryType.PayType, row.GetCell(10).ToString().Trim());                                
                                order.SumPrice = Convert.ToDecimal(row.GetCell(18).ToString().Trim()) * Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                order.PayPrice = Convert.ToDecimal(row.GetCell(18).ToString().Trim()) * Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                order.DeliveryTypeId = _deliveriesService.GetDeliveryByValue(row.GetCell(13).ToString().Trim());
                                order.WarehouseId = _orderService.MatchFirstWareHouseId(row.GetCell(8).ToString().Trim());
                                order.AdminMark = row.GetCell(16)?.ToString().Trim() + (string.IsNullOrEmpty(row.GetCell(14)?.ToString()) ? "" : " 运费：" + row.GetCell(14)?.ToString().Trim()) + (string.IsNullOrEmpty(row.GetCell(15)?.ToString()) ? "" : " 折扣：" + row.GetCell(15)?.ToString().Trim());
                                order.PriceTypeId =103;
                                //判断发票类型
                                if (row.GetCell(21) != null)
                                {
                                    switch (row.GetCell(21).ToString().Trim())
                                    {
                                        case "个人发票":
                                            order.InvoiceType = InvoiceType.PersonalInvoice;
                                            break;
                                        case "普通单位发票":
                                            order.InvoiceType = InvoiceType.CompanyInvoice;
                                            break;
                                        case "专用发票":
                                            order.InvoiceType = InvoiceType.SpecialInvoice;
                                            break;
                                        default:
                                            order.InvoiceType = InvoiceType.NoNeedInvoice;
                                            break;
                                    }
                                    //发票方式
                                    if (row.GetCell(23) != null)
                                    {
                                        order.InvoiceMode = InvoiceModeEnum.Electronic;
                                    }
                                    else {
                                        order.InvoiceMode = InvoiceModeEnum.Paper;
                                    }
                                }
                                else {
                                    order.InvoiceType = InvoiceType.NoNeedInvoice;
                                }
                                _orderService.CreatedB2COrder(order);
                            }
                            else
                            {
                                order.SumPrice += Convert.ToDecimal(row.GetCell(18).ToString().Trim()) * Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                order.PayPrice += Convert.ToDecimal(row.GetCell(18).ToString().Trim()) * Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                order.PayDate = Convert.ToDateTime(row.GetCell(12).ToString().Trim());
                                _orderService.UpdateOrder(order);
                            }
                            //插入订单商品    
                            var product = _productService.GetProductByCode(row.GetCell(17).ToString().Trim());
                            if (product == null)
                            {
                                tran.Rollback();
                                errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,未找到编码为{2}的商品;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(17).ToString().Trim()));
                                errorCount++;
                                continue;
                            }   
                            else
                            {
                                var saleProduct = _productService.GetSaleProduct(product.Id);
                                if (saleProduct == null)
                                {
                                    tran.Rollback();
                                    errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,未在销售商品列表中找到编码为{2}的商品;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(17).ToString().Trim()));
                                    errorCount++;
                                    continue;
                                }
                                else
                                {
                                    if (saleProduct.AvailableStock < Convert.ToInt32(row.GetCell(20).ToString().Trim())) {
                                        tran.Rollback();
                                        errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,销售商品编码{2}的商品，可售库存{3}小于商品数量{4}，库存不足;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(17).ToString().Trim(), saleProduct.AvailableStock, Convert.ToInt32(row.GetCell(20).ToString().Trim())));
                                        errorCount++;
                                        continue;
                                    }
                                    var orderProducts = _orderService.GetOrderProductsByOrderId(order.Id).Where(x => x.SaleProductId == saleProduct.Id).FirstOrDefault();
                                    if (orderProducts!=null) {
                                        tran.Rollback();
                                        errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,销售商品编码{2}的商品，订单已经存在该商品不能重复插入！", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(17).ToString().Trim()));
                                        errorCount++;
                                        continue;
                                    }
                                    var orderProduct = new OrderProduct();
                                    {
                                        orderProduct.OrderId = order.Id;
                                        orderProduct.SaleProductId = saleProduct.Id;
                                        orderProduct.Quantity = Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                        orderProduct.OrginPrice = _productService.GetOriginalPriceBySaleProductId(orderProduct.SaleProductId);
                                        orderProduct.Price = Convert.ToDecimal(row.GetCell(18).ToString().Trim());
                                        orderProduct.SumPrice = orderProduct.Price * orderProduct.Quantity;
                                    }
                                    _orderService.AddOrderProductB2C(orderProduct);
                                    //库存锁定跟踪
                                    _productService.CreateSaleProductLockedTrackAndWareHouseStock(order.Id, saleProduct.Id, order.WarehouseId, orderProduct.Quantity,orderProduct.Id);
                                }
                                //更新商品的锁定库存和可用库存
                                saleProduct.LockStock += Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                                _productService.UpdateSaleProduct(saleProduct);

                                //需要同步库存的商品Id集合
                                productIds.Add(saleProduct.ProductId);

                                //插入发票
                                if (order.InvoiceType != InvoiceType.NoNeedInvoice)
                                {
                                    var invoice = _omsAccessor.Get<InvoiceInfo>().Where(r => r.Isvalid && r.OrderId == order.Id).FirstOrDefault();
                                    if (invoice == null)
                                    {
                                        invoice = new InvoiceInfo
                                        {
                                            OrderId = order.Id,
                                            Title =row.GetCell(22)==null?"":row.GetCell(22).ToString().Trim(),
                                            CustomerEmail = row.GetCell(23)==null?"":row.GetCell(23).ToString().Trim(),
                                            TaxpayerID = row.GetCell(24) == null ? "" : row.GetCell(24).ToString().Trim(),
                                        };
                                        _omsAccessor.Insert<InvoiceInfo>(invoice);
                                        _omsAccessor.SaveChanges();
                                    }
                                }

                                //插入订单商品支付记录
                                var payPrice = _omsAccessor.Get<OrderPayPrice>().Where(r => r.Isvalid && r.OrderId == order.Id).FirstOrDefault();
                                if (payPrice != null)
                                {
                                    payPrice.Price += order.PayPrice;
                                    _omsAccessor.Update<OrderPayPrice>(payPrice);
                                    _omsAccessor.SaveChanges();
                                }
                                else
                                {
                                    payPrice = new OrderPayPrice
                                    {
                                        OrderId = order.Id,
                                        IsPay = true,
                                        PayType = order.PayType,
                                        PayMentType = order.PayMentType,
                                        Price = order.PayPrice,
                                    };
                                    _omsAccessor.Insert<OrderPayPrice>(payPrice);
                                    _omsAccessor.SaveChanges();
                                }
                            }

                            tran.Commit();
                            succCount++;
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim()));
                            errorCount++;
                            continue;
                        }
                    }
                }
                System.IO.File.Delete(fileName);
                fileStream.Close();

                //同步所有订单里面的商品可用库存到商城
                productIds = productIds.Distinct().ToList();
                var productStockDataList = new List<ProductStockData>();
                foreach (var id in productIds)
                {
                    try
                    {
                        var product = _productService.GetProductById(id);
                        var saleProduct = _productService.GetSaleProduct(id);
                        if (product == null || saleProduct == null)
                        {
                            errorStr.Append(string.Format("OMS中未找到Id为{0}的商品，或者该商品的销售商品为空;", id));
                            continue;
                        }
                        var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                        var productStockData = new ProductStockData
                        {
                            sd_id = "501",
                            goods_sn = product.Code,
                            stock_num = (saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock))).ToString(),
                            stock_detail_list = _productService.GetSaleProductWareHouseStocksByProductCode(product.Code)
                        };
                        productStockDataList.Add(productStockData);
                    }
                    catch (Exception ex)
                    {
                        errorStr.Append(string.Format("OMS中Id为{0}的商品同步库存到商城出错;", id));
                        continue;
                    }
                }
                //更新库存到商城
                _productService.SyncMoreProductStockToAssist(productStockDataList);


                if (errorCount > 0 || errorStr.Length > 0)
                {
                    string err = string.Format("总共{0}个订单，其中导入成功{1}个订单，失败{2}个订单，导入错误的订单信息：{3}", sheet.LastRowNum, succCount, errorCount, errorStr.ToString());
                    _logService.Error(err);
                    return Error(err);
                }
                else
                {
                    return Success(string.Format("总共{0}个订单，导入成功{1}个订单", sheet.LastRowNum, succCount));
                }
            }
        }
        /// <summary>
        /// 缺货订单解锁
        /// </summary>
        /// <param name="orderIdList"></param>
        /// <returns></returns>
        public IActionResult UnLockLackOrder(List<int> orderIdList)
        {
            //订单状态（无效状态，已发货状态，已上传）无法使用缺货解除
            var result = _productService.UnLockLackStockOrder(orderIdList);
            if (result != "")
            {
                return Error(result + "订单状态有误！无法解除缺货");
            }
            else
            {
                var failOrder = "";
                foreach (var item in orderIdList)
                {
                    var orderResult = _orderService.GetOrderById(item);
                    var orderIsLackList = _orderService.GetOrderProductsModelByOrderId(item, orderResult.WarehouseId);
                    #region 日志
                    OrderLog orderLog = new OrderLog();
                    orderLog.OrderId = orderResult.Id;
                    orderLog.OptionType = "缺货解除";
                    orderLog.OrderState = orderResult.State;
                    orderLog.PayState = orderResult.PayState;
                    #endregion
                    foreach (var proResult in orderIsLackList)
                    {
                        if (proResult.IsLackStock)
                        {
                            failOrder += orderResult.SerialNumber + "<br />";
                            orderLog.Mark = "缺货解除失败！部分商品库存不足！";
                            break;
                        }
                        else
                        {
                            orderLog.Mark = "缺货解除成功！";
                        }
                    }
                    _logService.InsertOrderLog(orderLog);
                }
                if (failOrder != "")
                {
                    return Error(failOrder + "缺货解除失败！部分商品库存不足！");
                }
                else
                {
                    return Success();
                }
            }
        }
        /// <summary>
        /// 批量确认订单
        /// </summary>
        /// <param name="orderList"></param>
        /// <returns></returns>
        public IActionResult BatchConfirmOrders(List<int> orderList)
        {
            var errStr = "";
            foreach(var item in orderList)
            {
                OrderModel orderModel = _orderService.GetOrderByIdB2C(item).ToModel();
                //判断订单状态
                if (orderModel.IsLocked == true)
                {
                    errStr += orderModel.SerialNumber + "订单已锁定，无法确认！" + "<br />";
                    continue;
                }
                if (orderModel.State == OrderState.Invalid)
                {
                    errStr += orderModel.SerialNumber + "订单已无效，无法确认！" + "<br />";
                    continue;
                }
                else if (orderModel.State == OrderState.Uploaded)
                {
                    errStr += orderModel.SerialNumber + "订单已上传，无法确认！" + "<br />";
                    continue;
                }
                var result = ConfirmOrder(item, orderModel.Type, orderModel);
                dynamic re = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result.Value));
                if (re["isSucc"] == false)
                {
                    errStr += orderModel.SerialNumber + re["msg"] + "<br />";
                }
            }
            if (errStr != "")
            {
                return Error(errStr);
            }
            else
            {
                return Success();
            }
        }
        /// <summary>
        /// 批量上传订单
        /// </summary>
        /// <param name="orderList"></param>
        /// <returns></returns>
        public IActionResult BatchUploadOrders(List<int> orderList)
        {
            var errStr = "";
            foreach (var item in orderList)
            {
                Order order = _orderService.GetOrderByIdB2C(item);
                if (order.State != OrderState.B2CConfirmed)
                {
                    errStr += order.SerialNumber + "订单状态不是已确认，无法上传！" + "<br />";
                    continue;
                }
                var result = UploadOrder(item);
                dynamic re = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result.Result));
                if (re["isSucc"] == false)
                {
                    errStr += order.SerialNumber + re["msg"] + "<br />";
                }
            }
            if (errStr != "")
            {
                return Error(errStr);
            }
            else
            {
                return Success();
            }
        }
        /// <summary>
        /// 批量修改快递方式
        /// </summary>
        /// <param name="orderList"></param>
        /// <param name="deliverType"></param>
        /// <returns></returns>
        public IActionResult BatchChangeDeliver(List<int> orderList,int deliverType)
        {
            //判断订单状态
            var errStr = "";
            Dictionary<int, string> delivery = new Dictionary<int, string> { };
            foreach (var i in _orderService.GetAllDeliveryList())
            {
                delivery.Add(i.Id, i.Name);
            }
            foreach (var item in orderList)
            {
                //无法修改快递状态：已锁定，无效，已上传，已完成，已确认
                var orderModel = _orderService.GetOrderByIdB2C(item);
                if (orderModel.IsLocked == true)
                {
                    errStr += orderModel.SerialNumber + "订单已锁定，无法修改快递方式！" + "<br />";
                    continue;
                }
                if (orderModel.State == OrderState.Invalid)
                {
                    errStr += orderModel.SerialNumber + "订单已无效，无法修改快递方式！" + "<br />";
                    continue;
                }
                else if (orderModel.State == OrderState.Uploaded)
                {
                    errStr += orderModel.SerialNumber + "订单已上传，无法修改快递方式！" + "<br />";
                    continue;
                }
                else if (orderModel.State == OrderState.Finished)
                {
                    errStr += orderModel.SerialNumber + "订单已完成，无法修改快递方式！" + "<br />";
                    continue;
                }
                else if (orderModel.State == OrderState.B2CConfirmed)
                {
                    errStr += orderModel.SerialNumber + "订单已确认，无法修改快递方式！" + "<br />";
                    continue;
                }
                #region 订单日志(修改快递方式)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = orderModel.Id;
                orderLog.OrderState = orderModel.State;
                orderLog.PayState = orderModel.PayState;
                orderLog.OptionType = "（批量）修改快递方式";
                orderLog.Mark = "快递方式由【" + delivery[orderModel.DeliveryTypeId] + "】 修改为：【" + delivery[deliverType] + "】";
                #endregion
                orderModel.DeliveryTypeId = deliverType;
                _orderService.UpdateOrder(orderModel);
                _logService.InsertOrderLog(orderLog);
            }
            if (errStr != "")
            {
                return Error(errStr);
            }
            return Success();
        }
        /// <summary>
        /// 批量修改仓库
        /// </summary>
        /// <param name="orderList"></param>
        /// <param name="wareHouseId"></param>
        /// <returns></returns>
        public IActionResult BatchChangeWareHouse(List<int> orderList,int wareHouseId)
        {
            var errStr = "";
            foreach (var item in orderList)
            {
                //无法修改仓库订单状态：已锁定，无效，已上传，已完成，已确认
                var orderModel = _orderService.GetOrderByIdB2C(item);
                if (orderModel.IsLocked == true)
                {
                    errStr += orderModel.SerialNumber + "订单已锁定，无法修改仓库！" + "<br />";
                    continue;
                }
                if (orderModel.State == OrderState.Invalid)
                {
                    errStr += orderModel.SerialNumber + "订单已无效，无法修改仓库！" + "<br />";
                    continue;
                }
                else if (orderModel.State == OrderState.Uploaded)
                {
                    errStr += orderModel.SerialNumber + "订单已上传，无法修改仓库！" + "<br />";
                    continue;
                }
                else if (orderModel.State == OrderState.Finished)
                {
                    errStr += orderModel.SerialNumber + "订单已完成，无法修改仓库！" + "<br />";
                    continue;
                }
                else if (orderModel.State == OrderState.B2CConfirmed)
                {
                    errStr += orderModel.SerialNumber + "订单已确认，无法修改仓库！" + "<br />";
                    continue;
                }
                var result = SaveWareHouseForOrder(item, wareHouseId);
                dynamic re = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(result));
                if (re["Value"]["isSucc"] == false)
                {
                    errStr += orderModel.SerialNumber + re["msg"] + "<br />";
                }
            }
            if (errStr != "")
            {
                return Error(errStr);
            }
            return Success();
        }
        #endregion


        #region B2C退货
        /// <summary>
        /// 获取B2C退单列表（方法）
        /// </summary>
        /// <returns></returns>
        public IActionResult GetB2CRefundOrdersList(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, int? orderState, string search = "", string supplierName = "")
        {
            endTime = endTime?.AddMinutes(1);
            deliverStartTime = deliverStartTime?.AddMinutes(1);
            var data = _orderService.GetAllRefundOrdersByPage(pageIndex, pageSize, startTime, endTime, deliverStartTime, deliverEndTime, shopId, orderState, search, OrderType.B2C_TH);
            return Success(data);
        }
        /// <summary>
        /// 保存B2C退单信息
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveRefundOrderInfo(OrderModel orderModel)
        {
            //if (!_permissionService.Authorize("SaveB2CRefundOrderInfo"))
            //{
            //    return Error("没有此权限！");
            //}
            Order order = _orderService.GetOrderById(orderModel.Id);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.ToBeConfirmed, OptTypeEnum.B2CRModify); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            order.WarehouseId = orderModel.WarehouseId;
            order.CustomerName = orderModel.CustomerName;
            order.CustomerPhone = orderModel.CustomerPhone;
            order.AddressDetail = orderModel.AddressDetail;
            order.AdminMark = orderModel.AdminMark;
            order.ToWarehouseMessage = orderModel.ToWarehouseMessage;
            _orderService.UpdateOrder(order);
            //日志
            _logService.InsertOrderLog(order.Id, "修改退单信息", order.State, order.PayState, "修改退单信息");
            return Success();
        }
        /// <summary>
        /// 添加退货订单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult AddRefundOrderProducts(OrderProduct orderProduct, string name, int orderType = 1)
        {
            if (!_permissionService.Authorize("AddB2COrderProducts"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderProduct.OrderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.ToBeConfirmed, OptTypeEnum.B2CRModify); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            try
            {
                if (_orderService.ConfirmOrderProductIsExist(orderProduct.OrderId, orderProduct.SaleProductId) == null)
                {
                    _orderService.AddOrderProductB2C(orderProduct);
                    //修改订单总价(跨境购退单)
                    if (orderType == 2)
                    {
                        var pros = _orderService.GetOrderProductsByOrderId(orderProduct.OrderId);
                        order.SumPrice = pros.Sum(r => r.SumPrice);
                        _orderService.UpdateOrder(order);
                    }
                    #region 订单日志(添加订单商品)
                    OrderLog orderLog = new OrderLog();
                    orderLog.OrderId = order.Id;
                    orderLog.OrderState = order.State;
                    orderLog.PayState = order.PayState;
                    orderLog.OptionType = "添加订单商品";
                    orderLog.Mark = "添加订单商品【商品名：" + name + " 价格：" + orderProduct.Price + "】数量：" + orderProduct.Quantity;
                    _logService.InsertOrderLog(orderLog);
                    #endregion


                    return Success("商品添加成功！");
                }
                else if (_orderService.ConfirmOrderProductIsExist(orderProduct.OrderId, orderProduct.SaleProductId) != null)
                {
                    return Error("商品添加失败！已有商品");
                }
                else
                {
                    return Error("其他错误！");
                }
            }
            catch (Exception ex)
            {

                _logService.Error("B2C退货订单" + order.SerialNumber + "添加商品失败：", ex);
                return Error("B2C退货订单" + order.SerialNumber + "添加商品出现错误！");
            }
        }
        /// <summary>
        /// 添加退货订单商品
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="refundProductInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddRefundProducts(int orderId, string refundProductInfo)
        {
            if (!_permissionService.Authorize("SplitOrder"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.ToBeConfirmed, OptTypeEnum.B2CRModify); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            var refundProductInfoData = JArray.Parse(refundProductInfo);
            using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    //权限
                    var data = _orderService.GetOrderByIdB2C(orderId);
                    if (data == null) return Error("未找到订单信息！");
                    var originalOrder = _orderService.GetOrderByIdB2C(data.OriginalOrderId);
                    if (originalOrder == null) return Error("未找到订单的原始订单信息！");
                    //判断订单的退货程度
                    if (_orderService.JudgeOrderRefundState(orderId) == RefundState.All) return Error("原订单已经退货完成不能在进行添加商品！");
                    //插入商品
                    var partRefundProductRes = _orderService.AddPartRefundProducts(data.OriginalOrderId, data.Id, refundProductInfoData);
                    if (!partRefundProductRes) { tran.Rollback(); return Error("添加商品发生错误！"); }
                    #region 订单日志(添加订单商品)
                    foreach (var item in refundProductInfoData)
                    {
                        OrderLog orderLog = new OrderLog();
                        orderLog.OrderId = data.Id;
                        orderLog.OrderState = data.State;
                        orderLog.PayState = data.PayState;
                        orderLog.OptionType = "添加订单商品";
                        orderLog.Mark = "添加订单商品【商品名：" + item["productname"].ToString() + " 价格：" + item["price"] + "】数量：" + item["refundQuantity"].ToString();
                        _logService.InsertOrderLog(orderLog);
                    }

                    #endregion

                    //插入商品后重新计算退单的总金额和支付金额
                    data.SumPrice = _orderService.GetOrderProductsByOrderId(data.Id).Sum(x => x.SumPrice);
                    data.PayPrice = data.SumPrice;
                    _orderService.UpdateOrder(data);

                    //添加支付信息
                    var orderPayPrice = new OrderPayPrice
                    {
                        OrderId = data.Id,
                        IsPay = true,
                        PayType = data.PayType,
                        Price = data.PayPrice,
                        PayMentType = data.PayMentType
                    };
                    _orderService.AddOrderPayPrice(orderPayPrice);

                    //添加订单商品后
                    if (_orderService.JudgeOrderRefundState(data.OriginalOrderId) == RefundState.All)
                    {
                        originalOrder.State = OrderState.Invalid;
                        originalOrder = _orderService.UpdateOrder(originalOrder);
                        _logService.InsertOrderLog(originalOrder.Id, "销售退单新增退单商品", originalOrder.State, originalOrder.PayState, "销售退单" + data.SerialNumber + "添加商品，订单退货完成订单作废设置为无效！");//订单退单日志
                    }
                    tran.Commit();
                    return Success("添加商品成功！");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    _logService.Error("添加商品失败：" + ex.Message);
                    return Error("添加商品异常！");
                }
            }
        }
        /// <summary>
        /// 更新退货单订单商品信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateRefundOrderProduct(int id, int quantity, decimal originPrice, decimal salePrice, decimal sumPrice, string productName)
        {
            if (!_permissionService.Authorize("SplitOrder"))
            {
                return Error("无操作权限！");
            }

            var refundOrderProduct = _orderService.GetOrderProductByIdOne(id);
            Order order = _orderService.GetOrderByIdB2C(refundOrderProduct.OrderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.ToBeConfirmed, OptTypeEnum.B2CRModify); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            if (refundOrderProduct == null) return Error("未找到商品信息！");
            var refundOrder = _orderService.GetOrderByIdB2C(refundOrderProduct.OrderId);
            if (refundOrder == null) return Error("未找到订单信息！");
            var originalOrder = _orderService.GetOrderByIdB2C(refundOrder.OriginalOrderId);
            if (originalOrder == null) return Error("未找到订单的原始订单信息！");
            var canRefundQuantity = _orderService.GetCanRefundOrderProductsByOrderId(refundOrder.OriginalOrderId).Where(x => x.OrderProductId == refundOrderProduct.OrginId).Sum(x => x.CanRedundQuantity);
            if ((refundOrderProduct.Quantity + canRefundQuantity) < quantity) return Error("填写的商品的退货数量错误，最大可填数量为" + (refundOrderProduct.Quantity + canRefundQuantity));
            //原/新商品价格数量单价总价
            var oldProQty = refundOrderProduct.Quantity;
            var oldProPrice = refundOrderProduct.Price;
            var oldTotalPrice = refundOrderProduct.SumPrice;
            var oldOriPrice = refundOrderProduct.OrginPrice;
            using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    refundOrderProduct.Quantity = quantity;
                    refundOrderProduct.Price = salePrice;
                    refundOrderProduct.OrginPrice = originPrice;
                    refundOrderProduct.SumPrice = sumPrice;
                    _orderService.UpdateOrderProduct(refundOrderProduct);
                    #region 订单日志(修改订单商品)
                    OrderLog orderLog = new OrderLog
                    {
                        OrderId = refundOrder.Id,
                        OrderState = refundOrder.State,
                        PayState = refundOrder.PayState,
                        OptionType = "修改订单商品",
                        Mark = "修改订单商品【" + productName + "】【数量：" + oldProQty + " 原价：" + oldOriPrice + " 价格：" + oldProPrice + " 总价：" + oldTotalPrice + "】修改为：【数量：" + quantity + " 原价：" + originPrice + " 价格：" + salePrice + " 总价：" + sumPrice + "】"
                    };
                    _logService.InsertOrderLog(orderLog);
                    #endregion
                    //修改商品后重新计算退单的总金额
                    refundOrder.SumPrice = _orderService.GetOrderProductsByOrderId(refundOrder.Id).Sum(x => x.SumPrice);
                    refundOrder.PayPrice = refundOrder.SumPrice;
                    _orderService.UpdateOrder(refundOrder);
                    //添加支付信息
                    var orderPayPrice = new OrderPayPrice
                    {
                        OrderId = refundOrder.Id,
                        IsPay = true,
                        PayType = refundOrder.PayType,
                        Price = refundOrder.PayPrice,
                        PayMentType = refundOrder.PayMentType
                    };
                    _orderService.AddOrderPayPrice(orderPayPrice);

                    var orginOrderRefundState = _orderService.JudgeOrderRefundState(refundOrder.OriginalOrderId);
                    //修改之后原订单商品全部退完
                    if (orginOrderRefundState == RefundState.All && originalOrder.State == OrderState.Delivered)
                    {
                        originalOrder.State = OrderState.Invalid;
                        originalOrder = _orderService.UpdateOrder(originalOrder);
                        _logService.InsertOrderLog(originalOrder.Id, "销售退单修改退单商品", originalOrder.State, originalOrder.PayState, "销售退单" + refundOrder.SerialNumber + "修改商品，订单退货完成订单作废设置为无效！");//订单退单日志
                    }
                    //修改之后原订单商品原订单没有退完
                    if (orginOrderRefundState != RefundState.All && originalOrder.State == OrderState.Invalid)
                    {
                        originalOrder.State = OrderState.Delivered;
                        originalOrder = _orderService.UpdateOrder(originalOrder);
                        _logService.InsertOrderLog(originalOrder.Id, "销售退单修改退单商品", originalOrder.State, originalOrder.PayState, "销售退单" + refundOrder.SerialNumber + "修改商品，订单退货为完成订单修改为已发货！");//订单退单日志
                    }

                    tran.Commit();
                    return Success("修改商品成功！");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    _logService.Error("修改商品失败：" + ex.Message);
                    return Error("修改商品异常！");

                }
            }
        }
        /// <summary>
        /// 删除退货订单商品
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderProductId"></param>
        /// <returns></returns>
        public IActionResult DelRefundOrderProduct(int id, int orderId, string name)
        {
            if (!_permissionService.Authorize("DeleteB2COrderProducts"))
            {
                return Error("无操作权限！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.ToBeConfirmed, OptTypeEnum.B2CRModify); 
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            if (order == null) return Error("没有找到订单信息！");
            var orginOrder = _orderService.GetOrderByIdB2C(order.OriginalOrderId);
            if (orginOrder == null) return Error("没有找到原订单信息！");
            using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    OrderProduct orderProduct = _orderService.GetOrderProductByIdOne(id);
                    SaleProduct saleProduct = _productService.GetSaleProductBySaleProductId(orderProduct.SaleProductId);
                    _orderService.DeleteOrderProductById(id);
                    #region 订单日志(删除订单商品)
                    OrderLog orderLog = new OrderLog();
                    orderLog.OrderId = order.Id;
                    orderLog.OrderState = order.State;
                    orderLog.PayState = order.PayState;
                    orderLog.OptionType = "删除订单商品";
                    orderLog.Mark = "删除订单商品【商品名：" + saleProduct.Product.Name + "】";
                    _logService.InsertOrderLog(orderLog);
                    #endregion


                    //删除商品后重新计算退单的总金额
                    order.SumPrice = _orderService.GetOrderProductsByOrderId(order.Id).Sum(x => x.SumPrice);
                    order.PayPrice = order.SumPrice;
                    _orderService.UpdateOrder(order);
                    //添加支付信息
                    var orderPayPrice = new OrderPayPrice
                    {
                        OrderId = order.Id,
                        IsPay = true,
                        PayType = order.PayType,
                        Price = order.PayPrice,
                        PayMentType = order.PayMentType
                    };
                    _orderService.AddOrderPayPrice(orderPayPrice);

                    //更新原始订单状态
                    if (_orderService.JudgeOrderRefundState(orginOrder.Id) != RefundState.All && orginOrder.State == OrderState.Invalid)
                    {
                        orginOrder.State = OrderState.Delivered;
                        orginOrder = _orderService.UpdateOrder(orginOrder);
                        _logService.InsertOrderLog(orginOrder.Id, "销售退单删除商品", orginOrder.State, orginOrder.PayState, "销售退单" + order.SerialNumber + "删除商品，将原订单改成发货状态！");//订单退单日志
                    }
                    tran.Commit();
                    return Success("删除成功！");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    _logService.Error("退货订单" + order.SerialNumber + "删除商品失败：", ex);
                    return Error("退货订单" + order.SerialNumber + "删除商品出现错误！");
                }
            }


        }
        /// <summary>
        /// 确认退货订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult ConfirmRefundOrder(int orderId, OrderType orderType)
        {
            if (orderType == OrderType.B2C_TH || orderType == OrderType.B2C_KJRF)
            {
                if (!_permissionService.Authorize("ConfirmB2CRefundOrder"))
                {
                    return Error("无操作权限！");
                }
            }
            else if (orderType == OrderType.B2C_XH)
            {
                if (!_permissionService.Authorize("ConfirmOrder"))
                {
                    return Error("无操作权限！");
                }
            }
            else
            {
                return Error("未知权限错误！");
            }
            Order order = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.B2CConfirmed, OptTypeEnum.B2CRConfirmed); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            if (order.WarehouseId == 0)
            {
                return Error("请选择并保存仓库信息！");
            }
            if (order.Type == OrderType.B2C_TH)
            {
                order.State = OrderState.B2CConfirmed;
                _orderService.UpdateOrder(order);

                #region 订单日志(确认订单)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderId = order.Id;
                orderLog.OrderState = order.State;
                orderLog.PayState = order.PayState;
                orderLog.OptionType = "确认订单";
                orderLog.Mark = "确认订单";
                _logService.InsertOrderLog(orderLog);
                #endregion
                return Success();
            }
            else
            {
                OrderPayPrice orderPayPrice = _orderService.GetOrderPay(orderId);
                if (orderPayPrice == null || orderPayPrice.IsPay == false)
                {
                    return Error("订单未支付");
                }
                else if (order.SumPrice > order.PayPrice)
                {
                    return Error("订单未支付完全，请修改支付价格后提交！");
                }
                else if (order.SumPrice < order.PayPrice)
                {
                    return Error("订单支付金额大于订单总金额，请修改支付价格后提交！");
                }
                else
                {
                    order.State = OrderState.B2CConfirmed;
                    _orderService.UpdateOrder(order);

                    #region 订单日志(确认订单)
                    OrderLog orderLog = new OrderLog();
                    orderLog.OrderId = order.Id;
                    orderLog.OrderState = order.State;
                    orderLog.PayState = order.PayState;
                    orderLog.OptionType = "确认订单";
                    orderLog.Mark = "确认订单";
                    _logService.InsertOrderLog(orderLog);
                    #endregion
                    return Success();
                }
            }
        }
        /// <summary>
        /// 反确认退货订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult UnRefundConfirmOrder(int orderId, OrderType orderType)
        {
            if (orderType == OrderType.B2C_TH)
            {
                if (!_permissionService.Authorize("UnConfirmB2CRefundOrder"))
                {
                    return Error("无操作权限！");
                }
            }
            else if (orderType == OrderType.B2C_XH || orderType == OrderType.B2C_KJRF)
            {
                if (!_permissionService.Authorize("UnConfirmOrder"))
                {
                    return Error("无操作权限！");
                }
            }
            else
            {
                return Error("未知权限错误！");
            }

            Order order = _orderService.GetOrderByIdB2C(orderId);
            //检测订单状态是否可以操作
            if (order.State != OrderState.B2CConfirmed)
            {
                return Error("当前订单状态不满足操作要求，请刷新页面查看订单是否已经被其他同事操作过了！");
            }
            if (orderType == OrderType.B2C_TH)
            {
                order.State = OrderState.Paid;
            }
            else
            {
                order.State = OrderState.Paid;//订单上一级状态为已付款
            }
            _orderService.UpdateOrder(order);

            #region 订单日志(反确认订单)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "反确认订单";
            orderLog.Mark = "反确认订单";
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Success();
        }
        [HttpPost]
        public IActionResult B2CRefundOrderDetail(OrderModel orderModel)
        {
            return Success();
        }
        /// <summary>
        /// B2C退货单上传
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [UserAnonymous]
        public IActionResult UploadRefundOrder(int id)
        {
            var orderInfo = _orderService.GetOrderById(id);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(orderInfo.State, OrderState.Uploaded, OptTypeEnum.B2CRUploaded);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            string message = _orderService.UploadRefundOrder(id);

            var returnMessage = message.ToObj<ResultModel>();

            if (returnMessage.isSucc)
            {
                var order = _orderService.GetOrderById(id);
                if (order != null)
                {
                    order.State = OrderState.Uploaded;
                    _orderService.UpdateOrder(order);
                }

                //日志
                #region 日志（上传B2C退单）
                _logService.InsertOrderLog(id, "上传B2C退单", order.State, order.PayState, "上传B2C退单");
                #endregion
            }
            return Success(message);
        }
        /// <summary>
        /// B2C退货单取消上传
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UnUploadRefundOrder(int id)
        {
            var orderInfo = _orderService.GetOrderById(id);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(orderInfo.State, OrderState.B2CConfirmed, OptTypeEnum.B2CRCancel); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            string message = _orderService.UnUploadRefundOrder(id);
            var returnMessage = message.ToObj<ResultModel>();

            if (returnMessage.isSucc)
            {
                var order = _orderService.GetOrderById(id);
                if (order != null)
                {
                    order.State = OrderState.B2CConfirmed;
                    _orderService.UpdateOrder(order);
                }

                //日志
                #region 日志（上传B2C退单）
                _logService.InsertOrderLog(id, "取消上传退单", order.State, order.PayState, "取消上传退单");
                #endregion
            }
            return Content(message);
        }
        /// <summary>
        /// 退单设为无效
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SetRefundOrderInvalid(int id)
        {
            if (!_permissionService.Authorize("SetInvalidB2CRefundOrder"))
            {
                return Error("没有此权限！");
            }
            var refundOrder = _orderService.GetOrderById(id);
            if (refundOrder == null)
                return Error("数据错误，退单不存在！");
            if (refundOrder.State == OrderState.Uploaded || refundOrder.State == OrderState.CheckAccept || refundOrder.State == OrderState.Finished)
                return Error(string.Format("退单在{0}状态下，不能设为无效！", refundOrder.State.Description()));

            refundOrder.State = OrderState.Invalid;
            _orderService.UpdateOrder(refundOrder);
            _logService.InsertOrderLog(refundOrder.Id, "设为无效", refundOrder.State, refundOrder.PayState, "把退单设为无效订单");

            if (refundOrder.OriginalOrderId.HasValue)
            {         
                var originalOrder = _orderService.GetOrderById(refundOrder.OriginalOrderId.Value);
                _logService.InsertOrderLog(originalOrder.Id, "退货单设为无效", originalOrder.State, originalOrder.PayState, "退单" + refundOrder.SerialNumber + "设为无效订单后，恢复原订单的退货数量！");
                //更新原始订单状态
                if (_orderService.JudgeOrderRefundState(originalOrder.Id) != RefundState.All && originalOrder.State == OrderState.Invalid) {
                    originalOrder.State = OrderState.Delivered;
                    _orderService.UpdateOrder(originalOrder);
                    _logService.InsertOrderLog(originalOrder.Id, "恢复原订单", originalOrder.State, originalOrder.PayState, "退单" + refundOrder.SerialNumber + "设为无效订单后，恢复原订单为已发货状态");
                }
            }
            return Success();
        }
        /// <summary>
        /// 验收B2C退单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CheckAcceptRefundOrder(int id)
        {
            if (!_permissionService.Authorize("CheckAcceptB2CRefundOrder"))
            {
                return Error("没有此权限！");
            }

            var order = _orderService.GetOrderById(id);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.CheckAccept, OptTypeEnum.B2CRCheckAccept); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            if (order == null)
                return Error("数据错误，该订单不存在！");

            order.State = OrderState.CheckAccept;
            _orderService.UpdateOrder(order);

            _logService.InsertOrderLog(order.Id, "验收退单", order.State, order.PayState, "验收B2C退单");
            return Success();
        }
        /// <summary>
        /// 完成B2C退单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CompleteRefundOrder(int id)
        {
            if (!_permissionService.Authorize("CompleteB2CRefundOrder"))
            {
                return Error("没有此权限！");
            }

            var order = _orderService.GetOrderById(id);
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Finished,OptTypeEnum.B2CRFinished);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            if (order == null)
                return Error("数据错误，该订单不存在！");

            order.State = OrderState.Finished;
            _orderService.UpdateOrder(order);

            _logService.InsertOrderLog(order.Id, "完成退单", order.State, order.PayState, "完成B2C退单");
            return Success();
        }
        #endregion


        #region 客户历史订单页
        public IActionResult GetHistoryOrderListByUserName(int pageIndex, int pageSize, string userName)
        {
            var data = _orderService.GetHistoryOrderListByPage(pageIndex, pageSize, userName);
            return Success(data);
        }
        #endregion

        #region 跨境购退单
        /// <summary>
        /// 获取跨境退单列表（方法）
        /// </summary>
        /// <returns></returns>
        public IActionResult GetKJRefundOrdersList(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, int? orderState, string search = "", string supplierName = "")
        {
            endTime = endTime?.AddMinutes(1);
            deliverStartTime = deliverStartTime?.AddMinutes(1);
            var data = _orderService.GetAllRefundOrdersByPage(pageIndex, pageSize, startTime, endTime, deliverStartTime, deliverEndTime, shopId, orderState, search, OrderType.B2C_KJRF);
            return Success(data);
        }
        public IActionResult GetExportKJRefundOrders(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, int? orderState, string search = "", string supplierName = "")
        {
            endTime = endTime?.AddMinutes(1);
            deliverStartTime = deliverStartTime?.AddMinutes(1);
            var data = _orderService.GetAllKJRefundOrderInfo(pageIndex, pageSize, startTime, endTime, deliverStartTime, deliverEndTime, shopId, orderState, search, OrderType.B2C_KJRF);
            return Success(data);
        }
        /// <summary>
        /// 添加跨境购退单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreatedKJRefundOrder(Order order)
        {
            try
            {
                //判断是否为作废订单
                if (_orderService.ConfirmPSerialNumberIsExist(order.PSerialNumber, false, false))
                {
                    return Json(new { isSucc = false, msg = "添加失败,检查平台订单号是否与其他订单重复或者该订单不是作废订单！" });
                }
                //如果存在平台单号的话加一个标识(N1)
                if (_orderService.ConfirmPSerialNumberIsExist(order.PSerialNumber, true, false))
                {
                    order.PSerialNumber = order.PSerialNumber + "(N)";
                    if (_orderService.ConfirmPSerialNumberIsExist(order.PSerialNumber, false, true))
                    {
                        return Json(new { isSucc = false, msg = "添加失败,检查平台订单号" + order.PSerialNumber.Replace("(N)", "") + "是否已经重新添加了平台单号：" + order.PSerialNumber + "的订单！" });
                    }
                }
                order.SerialNumber = _commonService.GetOrderSerialNumber("KJRF");
                order.WriteBackState = 0;
                order.IsLocked = false;
                order.LockStock = false;
                order.InvoiceType = InvoiceType.NoNeedInvoice;
                order.PriceTypeId = 103;//标准价
                order.IsLackStock = false;
                order.ZMCoupon = 0;
                order.ZMWineCoupon = 0;
                order.WineWorldCoupon = 0;
                order.State = OrderState.Paid;//手工新增订单默认为已付款状态
                order.PayState = PayState.Success;
                order.PayDate = DateTime.Now;
                order.WarehouseId = _orderService.MatchFirstWareHouseId(order.AddressDetail);
                order.DistrictId = 0;
                Order newOrder = _orderService.CreatedB2COrder(order);
                var orderid = newOrder.Id;
                var orderPayPrice = new OrderPayPrice
                {
                    OrderId = orderid,
                    IsPay = true,
                    PayType = order.PayType,
                    PayMentType = order.PayMentType,
                    Price = order.PayPrice,
                };
                _orderService.AddOrderPayPrice(orderPayPrice);
                #region 订单日志(新增订单)
                OrderLog orderLog = new OrderLog();
                orderLog.OrderState = newOrder.State;
                orderLog.PayState = PayState.Success;//默认新增已支付
                orderLog.OrderId = newOrder.Id;
                orderLog.OptionType = "新增订单";
                orderLog.Mark = "新增订单【" + newOrder.SerialNumber + "】";
                orderLog.CreatedBy = newOrder.CreatedBy;
                _logService.InsertOrderLog(orderLog);
                #endregion
                return Json(new { isSucc = true, orderId = orderid, orderSerialNumber = newOrder.SerialNumber });
            }
            catch (Exception ex)
            {
                return Json(new { isSucc = false, msg = "添加失败,原因：" + ex.Message });
            }
        }
        /// <summary>
        /// 删除跨境购商品
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderProId"></param>
        /// <returns></returns>
        public IActionResult DelteKJRefundOrderPro(int orderId,int orderProId)
        {
            var order = _orderService.GetOrderById(orderId);
            //状态判断
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid, OptTypeEnum.KJRFModify);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            var orderPro = _orderService.GetOrderProductByIdOne(orderProId);
            if (orderPro == null || orderPro.OrderId != order.Id)
            {
                return Error("无法在该订单下找到该商品！请尝试刷新页面");
            }
            if (!_orderService.DeleteOrderProductById(orderProId))
            {
                return Error("无法删除该商品！");
            }
            return Success();
        }
        /// <summary>
        /// 跨境购订单确认
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IActionResult KJRefundOrderConfirm(int orderId)
        {
            //状态判断
            var order = _orderService.GetOrderById(orderId);
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Confirmed, OptTypeEnum.KJRFConfirm);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            if (order.PayPrice == 0 && order.SumPrice != 0)
            {
                #region 订单日志(自动计算已付价格)
                OrderLog orderLog1 = new OrderLog();
                orderLog1.OrderId = order.Id;
                orderLog1.OrderState = order.State;
                orderLog1.PayState = order.PayState;
                orderLog1.OptionType = "自动设置已付金额";
                orderLog1.Mark = "原来已付金额：" + order.PayPrice + " 修改为已付金额：" + order.SumPrice;
                _logService.InsertOrderLog(orderLog1);
                #endregion
                order.PayPrice = order.SumPrice;
                OrderPayPrice orderPayPrice = new OrderPayPrice();
                orderPayPrice.OrderId = order.Id;
                orderPayPrice.IsPay = true;
                orderPayPrice.PayType = order.PayType;
                orderPayPrice.PayMentType = order.PayMentType;
                orderPayPrice.Price = order.SumPrice;
                orderPayPrice.Mark = "自动设置已付价格";
                _orderService.AddOrderPayPrice(orderPayPrice);
            }
            order.State = OrderState.B2CConfirmed;
            _orderService.UpdateOrder(order);
            #region 订单日志(确认订单)
            OrderLog orderLog = new OrderLog();
            orderLog.OrderId = order.Id;
            orderLog.OrderState = order.State;
            orderLog.PayState = order.PayState;
            orderLog.OptionType = "确认订单";
            orderLog.Mark = "确认订单";
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Success();
        }
        /// <summary>
        /// 修改跨境购退单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateKJRefundOrderPro(OrderProduct orderProduct,string productName)
        {
            var order = _orderService.GetOrderById(orderProduct.OrderId);
            if (order == null)
            {
                return Error("未找到该订单！");
            }
            //查看权限
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.Paid, OptTypeEnum.KJRFModify);
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            var orderPro = _orderService.GetOrderProductByIdOne(orderProduct.Id);
            if (orderPro == null|| orderPro.OrderId != order.Id)
            {
                return Error("未找到该商品！");
            }
            var oldProQty = orderPro.Quantity;
            var oldOriPrice = orderPro.OrginPrice;
            var oldProPrice = orderPro.Price;
            var oldTotalPrice = orderPro.SumPrice;
            orderPro.Quantity = orderProduct.Quantity;
            orderPro.SumPrice = orderProduct.SumPrice;
            orderPro.OrginPrice = orderProduct.OrginPrice;
            orderPro.Price = orderProduct.Price;
            if (!_orderService.UpdateOrderProduct(orderPro))
            {
                return Error("修改失败！");
            }
            //订单总金额
            var pros = _orderService.GetOrderProductsByOrderId(orderProduct.OrderId);
            order.SumPrice = pros.Sum(r => r.SumPrice);
            _orderService.UpdateOrder(order);
            //日志
            #region 订单日志(修改订单商品)
            OrderLog orderLog = new OrderLog
            {
                OrderId = order.Id,
                OrderState = order.State,
                PayState = order.PayState,
                OptionType = "修改订单商品",
                Mark = "修改订单商品【" + productName + "】【数量：" + oldProQty + " 原价：" + oldOriPrice + " 价格：" + oldProPrice + " 总价：" + oldTotalPrice + "】修改为：【数量：" + orderProduct.Quantity + " 原价：" + orderProduct.OrginPrice + " 价格：" + orderProduct.Price + " 总价：" + orderProduct.SumPrice + "】"
            };
            _logService.InsertOrderLog(orderLog);
            #endregion
            return Success();
        }
        /// <summary>
        /// 跨境购退单验收
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult KJRefundOrderAccept(int orderId)
        {
            if (!_permissionService.Authorize("CheckAcceptB2CRefundOrder"))
            {
                return Error("没有此权限！");
            }

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Error("数据错误，该订单不存在！");
            //检测订单状态是否可以操作
            var checkResult = _commonService.JudgeOptOrderState(order.State, OrderState.CheckAccept, OptTypeEnum.KJRFCheckAccept); ;
            if (checkResult != "")
            {
                return Error(checkResult);
            }
            order.State = OrderState.CheckAccept;
            _orderService.UpdateOrder(order);

            _logService.InsertOrderLog(order.Id, "验收退单", order.State, order.PayState, "验收B2C退单");
            return Success();
        }
        /// <summary>
        /// 批量上传跨境够退单
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult KJRefundOrdersImport(IFormFile file)
        {
            IWorkbook workbook = null;
            if (file == null)
            {
                return Error("请添加要导入文件");
            }
            else
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0)
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else if (fileName.IndexOf(".xls") > 0)
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                ISheet sheet = workbook.GetSheetAt(0);
                IRow row;

                StringBuilder errorStr = new StringBuilder();//记录错误信息
                int succCount = 0;
                int errorCount = 0;
                List<string> failName = new List<string>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
                    {
                        row = sheet.GetRow(i);

                        if (row.GetCell(0) == null || row.GetCell(3) == null)
                        {
                            continue;
                        }
                        else if (string.IsNullOrEmpty(row.GetCell(3).ToString().Trim()))
                        {
                            continue;
                        }
                        try
                        {
                            //插入订单
                            var order = _orderService.GetOrderByPSerialNumber(row.GetCell(3).ToString().Trim());
                            //判断店铺是否存在
                            var shopIdDic = _omsAccessor.Get<Dictionary>().Where(x => new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.Consignment, DictionaryType.ThirdpartyOnlineSales }.Contains(x.Type) && x.Isvalid && x.Id == Convert.ToInt32(row.GetCell(2).ToString().Trim())).FirstOrDefault();
                            if (shopIdDic == null)
                            {
                                errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,未找到编码为{2}的店铺;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(2).ToString().Trim()));
                                errorCount++;
                                continue;
                            }
                            if (order == null)
                            {
                                order = new Order()
                                {
                                    CreatedTime = Convert.ToDateTime(row.GetCell(1).ToString().Trim()),
                                    ShopId = Convert.ToInt32(row.GetCell(2).ToString().Trim()),
                                    SerialNumber = _commonService.GetOrderSerialNumber("KJRF"),
                                    PSerialNumber = row.GetCell(3).ToString().Trim(),
                                    Type = OrderType.B2C_KJRF,
                                    State = OrderState.Paid,
                                    WriteBackState = WriteBackState.NoWrite,
                                    PayState = PayState.Success,
                                    PayDate = Convert.ToDateTime(row.GetCell(12).ToString().Trim()),
                                    IsLocked = false,
                                    LockStock = true,
                                    CustomerName = row.GetCell(5).ToString().Trim(),
                                    CustomerPhone = row.GetCell(6).ToString().Trim(),
                                    AddressDetail = row.GetCell(8).ToString().Trim(),
                                    ZMCoupon = 0,
                                    ZMWineCoupon = 0,
                                    WineWorldCoupon = 0,
                                    UserName = row.GetCell(4).ToString().Trim(),
                                    IsNeedPaperBag = false,
                                };

                                order.PayType = _commonService.GetDictionaryOfPayByTypeAndValue(DictionaryType.PayType, row.GetCell(10).ToString().Trim()) == 0 ? 119 : _commonService.GetDictionaryOfPayByTypeAndValue(DictionaryType.PayType, row.GetCell(10).ToString().Trim());
                                order.SumPrice = Convert.ToDecimal(row.GetCell(18).ToString().Trim()) * Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                order.PayPrice = Convert.ToDecimal(row.GetCell(18).ToString().Trim()) * Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                order.DeliveryTypeId = _deliveriesService.GetDeliveryByValue(row.GetCell(13).ToString().Trim());
                                order.WarehouseId = _orderService.MatchFirstWareHouseId(row.GetCell(8).ToString().Trim());
                                order.AdminMark = row.GetCell(16)?.ToString().Trim() + (string.IsNullOrEmpty(row.GetCell(14)?.ToString()) ? "" : " 运费：" + row.GetCell(14)?.ToString().Trim()) + (string.IsNullOrEmpty(row.GetCell(15)?.ToString()) ? "" : " 折扣：" + row.GetCell(15)?.ToString().Trim());
                                order.PriceTypeId = 103;
                                //判断发票类型
                                if (row.GetCell(21) != null)
                                {
                                    switch (row.GetCell(21).ToString().Trim())
                                    {
                                        case "个人发票":
                                            order.InvoiceType = InvoiceType.PersonalInvoice;
                                            break;
                                        case "普通单位发票":
                                            order.InvoiceType = InvoiceType.CompanyInvoice;
                                            break;
                                        case "专用发票":
                                            order.InvoiceType = InvoiceType.SpecialInvoice;
                                            break;
                                        default:
                                            order.InvoiceType = InvoiceType.NoNeedInvoice;
                                            break;
                                    }
                                    //发票方式
                                    if (row.GetCell(23) != null)
                                    {
                                        order.InvoiceMode = InvoiceModeEnum.Electronic;
                                    }
                                    else
                                    {
                                        order.InvoiceMode = InvoiceModeEnum.Paper;
                                    }
                                }
                                else
                                {
                                    order.InvoiceType = InvoiceType.NoNeedInvoice;
                                }
                                _orderService.CreatedB2COrder(order);
                            }
                            else
                            {
                                order.SumPrice += Convert.ToDecimal(row.GetCell(18).ToString().Trim()) * Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                order.PayPrice += Convert.ToDecimal(row.GetCell(18).ToString().Trim()) * Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                order.PayDate = Convert.ToDateTime(row.GetCell(12).ToString().Trim());
                                _orderService.UpdateOrder(order);
                            }
                            //插入订单商品    
                            var product = _productService.GetProductByCode(row.GetCell(17).ToString().Trim());
                            if (product == null)
                            {
                                tran.Rollback();
                                errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,未找到编码为{2}的商品;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(17).ToString().Trim()));
                                errorCount++;
                                continue;
                            }
                            else
                            {
                                var saleProduct = _productService.GetSaleProduct(product.Id);
                                if (saleProduct == null)
                                {
                                    tran.Rollback();
                                    errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,未在销售商品列表中找到编码为{2}的商品;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(17).ToString().Trim()));
                                    errorCount++;
                                    continue;
                                }
                                else
                                {
                                    var orderProducts = _orderService.GetOrderProductsByOrderId(order.Id).Where(x => x.SaleProductId == saleProduct.Id).FirstOrDefault();
                                    if (orderProducts != null)
                                    {
                                        tran.Rollback();
                                        errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败,销售商品编码{2}的商品，订单已经存在该商品不能重复插入！", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim(), row.GetCell(17).ToString().Trim()));
                                        errorCount++;
                                        continue;
                                    }
                                    var orderProduct = new OrderProduct();
                                    {
                                        orderProduct.OrderId = order.Id;
                                        orderProduct.SaleProductId = saleProduct.Id;
                                        orderProduct.Quantity = Convert.ToInt32(row.GetCell(20).ToString().Trim());
                                        orderProduct.OrginPrice = _productService.GetOriginalPriceBySaleProductId(orderProduct.SaleProductId);
                                        orderProduct.Price = Convert.ToDecimal(row.GetCell(18).ToString().Trim());
                                        orderProduct.SumPrice = orderProduct.Price * orderProduct.Quantity;
                                    }
                                    _orderService.AddOrderProductB2C(orderProduct);
                                }

                                //插入发票
                                if (order.InvoiceType != InvoiceType.NoNeedInvoice)
                                {
                                    var invoice = _omsAccessor.Get<InvoiceInfo>().Where(r => r.Isvalid && r.OrderId == order.Id).FirstOrDefault();
                                    if (invoice == null)
                                    {
                                        invoice = new InvoiceInfo
                                        {
                                            OrderId = order.Id,
                                            Title = row.GetCell(22) == null ? "" : row.GetCell(22).ToString().Trim(),
                                            CustomerEmail = row.GetCell(23) == null ? "" : row.GetCell(23).ToString().Trim(),
                                            TaxpayerID = row.GetCell(24) == null ? "" : row.GetCell(24).ToString().Trim(),
                                        };
                                        _omsAccessor.Insert<InvoiceInfo>(invoice);
                                        _omsAccessor.SaveChanges();
                                    }
                                }

                                //插入订单商品支付记录
                                var payPrice = _omsAccessor.Get<OrderPayPrice>().Where(r => r.Isvalid && r.OrderId == order.Id).FirstOrDefault();
                                if (payPrice != null)
                                {
                                    payPrice.Price += order.PayPrice;
                                    _omsAccessor.Update<OrderPayPrice>(payPrice);
                                    _omsAccessor.SaveChanges();
                                }
                                else
                                {
                                    payPrice = new OrderPayPrice
                                    {
                                        OrderId = order.Id,
                                        IsPay = true,
                                        PayType = order.PayType,
                                        PayMentType = order.PayMentType,
                                        Price = order.PayPrice,
                                    };
                                    _omsAccessor.Insert<OrderPayPrice>(payPrice);
                                    _omsAccessor.SaveChanges();
                                }
                            }

                            tran.Commit();
                            succCount++;
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            errorStr.Append(string.Format("订单序号为{0}，订单交易号为{1}的订单插入失败;", row.GetCell(0).ToString().Trim(), row.GetCell(3).ToString().Trim()));
                            errorCount++;
                            continue;
                        }
                    }
                }
                System.IO.File.Delete(fileName);
                fileStream.Close();

                if (errorCount > 0 || errorStr.Length > 0)
                {
                    string err = string.Format("总共{0}个订单，其中导入成功{1}个订单，失败{2}个订单，导入错误的订单信息：{3}", sheet.LastRowNum, succCount, errorCount, errorStr.ToString());
                    _logService.Error(err);
                    return Error(err);
                }
                else
                {
                    return Success(string.Format("总共{0}个订单，导入成功{1}个订单", sheet.LastRowNum, succCount));
                }
            }
        }
        #endregion

        #region 其他
        [HttpGet]
        public IActionResult GetEveryMonthOrder(int? month)
        {
            var data = _orderService.GetEveryMonthB2COrders(month);
            return Success(data);
        }
        /// <summary>
        /// 订单状态中文名
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetOrderStateName()
        {
            Dictionary<string, string> orderStateStr = new Dictionary<string, string> { };
            orderStateStr.Add(OrderState.ToBeTurned.ToString(), OrderState.ToBeTurned.Description());
            orderStateStr.Add(OrderState.ToBeConfirmed.ToString(), OrderState.ToBeConfirmed.Description());
            orderStateStr.Add(OrderState.Confirmed.ToString(), OrderState.Confirmed.Description());
            orderStateStr.Add(OrderState.FinancialConfirmation.ToString(), OrderState.FinancialConfirmation.Description());
            orderStateStr.Add(OrderState.Unpaid.ToString(), OrderState.Unpaid.Description());
            orderStateStr.Add(OrderState.Paid.ToString(), OrderState.Paid.Description());
            orderStateStr.Add(OrderState.B2CConfirmed.ToString(), OrderState.B2CConfirmed.Description());
            orderStateStr.Add(OrderState.Unshipped.ToString(), OrderState.Unshipped.Description());
            orderStateStr.Add(OrderState.Delivered.ToString(), OrderState.Delivered.Description());
            orderStateStr.Add(OrderState.Invalid.ToString(), OrderState.Invalid.Description());
            orderStateStr.Add(OrderState.Uploaded.ToString(), OrderState.Uploaded.Description());
            orderStateStr.Add(OrderState.Finished.ToString(), OrderState.Finished.Description());
            orderStateStr.Add(OrderState.Cancel.ToString(), OrderState.Cancel.Description());
            return orderStateStr;
        }
        /// <summary>
        /// 用户中文名
        /// </summary>
        /// <returns></returns>
        Dictionary<int, string> GetUserName()
        {
            Dictionary<int, string> dUser = new Dictionary<int, string>();
            foreach (var item in _userService.GetAllUsers())
            {
                dUser.Add(item.Id, item.Name);
            }
            return dUser;
        }
        /// <summary>
        /// 获取商城店铺名简称
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
        public string GetMallShortName(int shopid)
        {
            var shop = _commonService.GetDictionaryById(shopid);
            if (shop == null)
                return null;
            else
            {
                if (shop.Value.Contains("官网"))
                    return "SC";
                else if (shop.Value.Contains("京东"))
                    return "JD";
                else if (shop.Value.Contains("天猫"))
                    return "TM";
                else if (shop.Value.Contains("苏宁"))
                    return "SN";
                else if (shop.Value.Contains("国美"))
                    return "GM";
                else if (shop.Value.Contains("百度"))
                    return "BD";
                else
                    return null;
            }
        }
        public IActionResult OrderDetail(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order.Type == OrderType.B2B)
            {
                return Success("/B2BOrder/AddSalesBill?orderId=");
            }else if(order.Type == OrderType.B2C_XH)
            {
               return Success("/B2COrder/B2COrderDetail?id=");
            }
            else
            {
                return Success("/B2COrder/B2COrderDetail?id=");
            }
        }
        #endregion
        #endregion

        //public IActionResult ExportEx()
        //{
        //    _orderService.ExportEx();
        //    return Ok();
        //}
    }
}