using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OMS.Services.Common;
using OMS.Data.Domain;
using OMS.Model;
using OMS.Core.Tools;
using OMS.Services.Customer;
using OMS.Services.Order1;
using OMS.Services.Account;
using OMS.Model.B2B;
using OMS.WebCore;
using OMS.Core;
using OMS.Services;
using OMS.Services.Log;
using Newtonsoft.Json;
using OMS.Services.Permissions;
using Microsoft.AspNetCore.Mvc.Rendering;
using OMS.Core.Json;
using OMS.Model.JsonModel;
using System.Data;
using System.Text;
using System.IO;
using OMS.Data.Interface;
using OMS.Services.Products;
using OMS.Services.Deliveries;
using OMS.Model.Grid;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Newtonsoft.Json.Linq;
using OMS.Model.Order;

namespace OMS.Web.Controllers
{
    public class B2BOrderController : BaseController
    {
        #region
        public readonly ICommonService _commonService;
        public readonly ICustomerService _customerService;
        public readonly IOrderService _orderService;
        public readonly IUserService _userService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ILogService _logService;
        private readonly IPermissionService _permissionService;
        protected readonly IDbAccessor _omsAccessor;
        private readonly ISalesManService _salesManService;
        private readonly IProductService _productService;
        private readonly IDeliveriesService _deliveriesService;
        protected readonly IWorkContext _workContext;
        private readonly IHostingEnvironment _hostingEnvironment;
        public B2BOrderController(ICommonService commonService
            , ICustomerService customerService
            , IOrderService orderService
            , IUserService userService
            , IWareHouseService wareHouseService
            , ILogService logService
            , IPermissionService permissionService
            , IDbAccessor omsAccessor
            , IWorkContext workContext
            , ISalesManService salesManService
            , IProductService productService
            , IDeliveriesService deliveriesService
            , IHostingEnvironment hostingEnvironment) {
            _commonService = commonService;
            _customerService = customerService;
            _orderService = orderService;
            _userService = userService;
            _wareHouseService = wareHouseService;
            _logService = logService;
            _permissionService = permissionService;
            _omsAccessor = omsAccessor;
            _workContext = workContext;
            _salesManService = salesManService;
            _productService = productService;
            _deliveriesService = deliveriesService;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region 页面
        public IActionResult B2BSalesBill()
        {

            if (!_permissionService.Authorize("ViewB2BOrder"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Customer = _customerService.GetAllCustomerList();
            ViewBag.CustomerType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.CustomerType), "Id", "Value");
            ViewBag.WareHouses = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");

            ViewBag.SalesMans = _salesManService.GetAllSalesMans();
            return View();
        }
        public IActionResult B2BRefundOrderList()
        {
            if (!_permissionService.Authorize("ViewB2BRefundOrder"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            return View();
        }
        public IActionResult B2BRefundOrderDetail(int id)
        {
            if (!_permissionService.Authorize("ViewB2BRefundOrderDetail"))
            {
                return View("_AccessDeniedView");
            }
            var data = _orderService.GetOrderById(id);
            ViewBag.OrderProducts = _orderService.GetOrderProductsByOrderId(id);
            ViewBag.Customers = new SelectList(_customerService.GetAllCustomerList(), "Id", "Name");
            ViewBag.PriceTypeStr = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PriceType), "Id", "Value", data.PriceTypeId);
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name", data.WarehouseId);
            ViewBag.OrderStateStr = _orderService.GetOrderStateStr();
            ViewBag.SalesMans = new SelectList(_salesManService.GetAllSalesMans(), "Id", "UserName", data.SalesManId);
            ViewBag.PayTypes = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PayStyle), "Id", "Value", data.PayType);
            ViewBag.PayMentTypes = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PayType), "Id", "Value", data.PayMentType);
            //可退货商品信息
            ViewBag.CanRefundOrderProducts = _orderService.GetCanRefundOrderProductsByOrderId(data.OriginalOrderId);
            return View(data);
        }
        public IActionResult AddSalesBill(int orderId = 0)
        {
            try
            {
                if (!_permissionService.Authorize("ViewCreateB2BOrder"))
                {
                    return View("_AccessDeniedView");
                }
                var result = _orderService.GetAllApprovalProcessList();
                List<ApprovalProcessModel> list = new List<ApprovalProcessModel>();
                foreach (var item in result)
                {
                    list.Add(item.ToModel());
                }
                OrderModel orderModel = new OrderModel();
                if (orderId == 0)
                {
                    orderModel.SerialNumber = _commonService.GetOrderSerialNumber("PF");
                }
                else
                {
                    orderModel = _orderService.GetOrderById(orderId).ToModel();
                }
                orderModel.Customers = _customerService.GetAllCustomerList();
                orderModel.CompanyType = _customerService.GetById(orderModel.CustomerId) == null ? "" : _customerService.GetById(orderModel.CustomerId).Dictionary?.Value;
                orderModel.WareHouses = _wareHouseService.GetAllWareHouseList();
                orderModel.PriceType = _commonService.GetBaseDictionaryList(DictionaryType.PriceType);
                orderModel.ApprovalProcessModel = list;
                orderModel.Deliverys = _orderService.GetAllDeliveryList();
                orderModel.PayTypes = _commonService.GetBaseDictionaryList(DictionaryType.PayStyle);
                orderModel.PayMentTypes = _commonService.GetBaseDictionaryList(DictionaryType.PayType);
                orderModel.SalesMans = _salesManService.GetAllSalesMans();
                var orderProducts = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == orderId);
                ViewBag.InoviceModes = new SelectList(EnumExtensions.GetEnumList((Enum)InvoiceModeEnum.Electronic), "Key", "Value", (int)orderModel.InvoiceMode);
                ViewBag.WareHouses = new SelectList(_wareHouseService.GetWareHouses().Where(r => new List<WareHouseTypeEnum>() { WareHouseTypeEnum.Normal, WareHouseTypeEnum.VirtualStore }.Contains(r.WareHouseType)), "Id", "Name", (int)orderModel.WarehouseId);
                ViewBag.TotalQuantity = (orderProducts != null && orderProducts.Count() > 0) ? orderProducts.Sum(x => x.Quantity) : 0;
                ViewBag.InvoiceTypes = new SelectList(EnumExtensions.GetEnumList((Enum)InvoiceType.NoNeedInvoice), "Key", "Value", (int)orderModel.InvoiceType);
                ViewBag.TotalOrginPrice = (orderProducts != null && orderProducts.Count() > 0) ? orderProducts.Sum(x => x.Quantity * x.OrginPrice) : 0;
                //可退货商品信息
                ViewBag.CanRefundOrderProducts = _orderService.GetCanRefundOrderProductsByOrderId(orderId);
                //订单的退货状态
                ViewBag.RefundState = _orderService.JudgeOrderRefundState(orderId);
                return View(orderModel);
            }
            catch (Exception ex)
            {

                _logService.Error("加载B2B详情页页面异常："+ex.Message);
                throw;
            }

        }
        /// <summary>
        /// B2B销货数据分析
        /// </summary>
        /// <returns></returns>
        public IActionResult B2BSaleDataAnalysis()
        {
            ViewBag.Customer = _customerService.GetAllCustomerList();
            ViewBag.CustomerType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.CustomerType), "Id", "Value");
            ViewBag.WareHouses = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");

            return View();
        }
        /// <summary>
        /// B2B退货单
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        public IActionResult AddB2BRefundOrder()
        {
            if (!_permissionService.Authorize("AddB2BRefundOrder"))
            {
                return View("_AccessDeniedView");
            }

            OrderModel orderModel = new OrderModel();

            orderModel.SerialNumber = _commonService.GetOrderSerialNumber("PT");

            orderModel.Customers = _customerService.GetAllCustomerList();
            orderModel.WareHouses = _wareHouseService.GetAllWareHouseList();
            orderModel.PriceType = _commonService.GetBaseDictionaryList(DictionaryType.PriceType);
            orderModel.Deliverys = _orderService.GetAllDeliveryList();
            orderModel.PayTypes = _commonService.GetBaseDictionaryList(DictionaryType.PayStyle);
            orderModel.PayMentTypes = _commonService.GetBaseDictionaryList(DictionaryType.PayType);
            orderModel.SalesMans = _salesManService.GetAllSalesMans();
            return View(orderModel);
        }
        #endregion

        #region 操作

        
        #region B2B订单
        [HttpPost]
        public IActionResult GetOrderList(SearchModel searchModel, OrderModel orderModel)
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
            orderModel.Type = OrderType.B2B;
            try
            {
                var data = _orderService.GetOrderListByType(orderModel, searchModel.PageIndex, searchModel.Length);
                foreach (var itemData in data)
                {
                    var refunddata = _omsAccessor.Get<Order>().Where(r => r.Isvalid
                    && (r.OrgionSerialNumber.Contains(itemData.SerialNumber) 
                    || r.CustomerName.Contains(itemData.SerialNumber)
                    || r.CustomerPhone.Contains(itemData.SerialNumber) 
                    || r.AdminMark.Contains(itemData.SerialNumber) 
                    || r.AddressDetail.Contains(itemData.SerialNumber)
                    || r.CustomerMark.Contains(itemData.SerialNumber)
                    || r.FinanceMark.Contains(itemData.SerialNumber))
                    && r.Type == OrderType.B2B_TH).FirstOrDefault();
                    itemData.IsRefundOrder = refunddata != null
                    && (itemData.State == OrderState.Invalid || itemData.State == OrderState.Delivered || itemData.State == OrderState.CheckAccept || itemData.State == OrderState.Finished);
                    itemData.RefundNum = refunddata != null ? refunddata.SerialNumber : "";
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
        [HttpPost]
        public IActionResult SelectCustomer(int customerId)
        {
            var data = _customerService.GetById(customerId);
            return Success(data);
        }
        [HttpPost]
        public IActionResult AddB2BOrder(OrderModel orderModel)
        {
            if (!_permissionService.Authorize("CreateB2BOrder"))
            {
                return Error("无操作权限！");
            }
            //B2B的订单不用匹配仓库自己选择
            // orderModel.WarehouseId = _orderService.MatchFirstWareHouseId(orderModel.AddressDetail);

            var delivery = _deliveriesService.GetDeliveryById(orderModel.DeliveryTypeId);
            //非客户自提需要填写收货人信息
            if (!delivery.Code.Contains("KHZT"))
            {
                //判断订单信息是否完全
                if (string.IsNullOrEmpty(orderModel.AddressDetail))
                {
                    return Error("非客户自提订单必须填写地址信息！");
                }

                if (string.IsNullOrEmpty(orderModel.CustomerName))
                {
                    return Error("非客户自提订单必须填写收货人信息！");
                }

                if (string.IsNullOrEmpty(orderModel.CustomerPhone))
                {
                    return Error("非客户自提订单必须填写收货人电话信息！");
                }
            }

            var order = _orderService.AddB2BOrder(orderModel);
            #region 日志（创建B2B订单）
            _logService.InsertOrderLog(order.Id, "新增B2B订单", order.State, PayState.Fail, "新增B2B订单[" + order.SerialNumber + "]");//新增订单默认未支付、未发货
            #endregion
            return Success(new { order.Id, order.SerialNumber});//传回新增订单Id继续操作
        }
        [HttpPost]
        public IActionResult UpdateB2BOrder(OrderModel orderModel)
        {
            if (!_permissionService.Authorize("UpdateB2BOrder"))
            {
                return Error("无操作权限！");
            }
            var order = _orderService.GetOrderById(orderModel.Id);
            if (order?.State >= OrderState.Uploaded || order?.State==OrderState.Delivered || order?.State == OrderState.Invalid) {
                return Error("订单已上传或已完成或已发货或订单为无效状态无法修改！");
            }
            if (order != null) {
                var mark = "";
                if (order.CustomerMark != orderModel.CustomerMark && (IsNullOrEmpty(order.CustomerMark) != IsNullOrEmpty(orderModel.CustomerMark)))
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
                if (order.CustomerId != orderModel.CustomerId) {
                    mark += "【客户】：" + _customerService.GetById(order.CustomerId).Name + " 修改为：" + _customerService.GetById(orderModel.CustomerId).Name + " ";
                }
                if (order.DeliveryTypeId != orderModel.DeliveryTypeId) {
                    mark += "【快递方式】：" + _deliveriesService.GetDeliveryById(order.DeliveryTypeId).Name + " 修改为" + _deliveriesService.GetDeliveryById(orderModel.DeliveryTypeId).Name + " ";
                }
                if (order.SalesManId != orderModel.SalesManId)
                {
                    mark += "【业务员】：" + _salesManService.GetSalesManById(order.SalesManId)?.UserName + " 修改为" + _salesManService.GetSalesManById(order.SalesManId)?.UserName + " ";
                }

                _orderService.UpdateB2BOrder(orderModel);
                #region 日志（修改订单）
                _logService.InsertOrderLog(order.Id, "修改订单", order.State, order.PayState, mark);
                #endregion
                return Success();
            }
            else
            {
                return Error("修改订单信息失败！");
            }
        }
        [HttpPost]
        public IActionResult AddB2BOrderProduct(OrderProductModel orderProductModel)
        {
            if (!_permissionService.Authorize("AddB2BOrderProducts"))
            {
                return Error("无操作权限！");
            }
            var order = _orderService.GetOrderById(orderProductModel.OrderId);
            try
            {
                if (_orderService.GetOrderById(orderProductModel.OrderId).State >= OrderState.Uploaded)
                {
                    return Error("订单已上传或已完成无法再修改商品信息！");
                }

                SaleProduct saleProduct = _productService.GetSaleProductBySaleProductId(orderProductModel.SaleProductId);

                if (orderProductModel.Quantity > saleProduct.AvailableStock)
                {
                    return Error("商品可用库存不足！");
                }
                var result = _orderService.ConfirmOrderProductIsExist(orderProductModel.OrderId, orderProductModel.SaleProductId);
                if (result == null)
                {
                    _orderService.AddOrderProduct(orderProductModel.ToEntity());
                    #region 日志（增加B2B订单商品）
                    _logService.InsertOrderLog(order.Id, "新增B2B订单商品", order.State, order.PayState, "新增B2B订单商品[" + orderProductModel.ProductName + "] 数量[" + orderProductModel.Quantity + "]");
                    #endregion
                    //重新计算订单总价
                    var products = _orderService.GetOrderProductsByOrderId(order.Id);
                    //更新销售商品的可售库存
                    saleProduct.LockStock += orderProductModel.Quantity;
                    saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                    _productService.UpdateSaleProduct(saleProduct);
                    //更新销售商品的库存到商城
                    var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                    _productService.SyncProductStockToAssist(501, saleProduct.Product.Code, saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock)));
                    //把商品锁定添加到锁定跟踪表（SaleProductLockedTrack）以及oms库存表（SaleProductWareHouseStock）
                    var resultLock = _productService.AddSaleProductLockedTrackAndWareHouseStock(order.Id, saleProduct.Id, order.WarehouseId, orderProductModel.Quantity, null);
                    if (!resultLock.Contains("成功"))
                    {
                        #region 订单日志(更新锁定信息失败)
                        _logService.InsertOrderLog(order.Id, "更新锁定信息失败", order.State, order.PayState, resultLock);
                        #endregion
                    }
                    return Success();
                }
                else
                {
                    return Error("已有该商品！");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("添加商品出现异常:", ex);
                return Error("B2B订单" + order.SerialNumber + "添加商品错误！");
            }


        }
        [HttpPost]
        public IActionResult UpdateB2BOrderProduct(OrderProductModel orderProductModel)
        {

            if (!_permissionService.Authorize("UpdateB2BOrderProducts"))
            {
                return Error("无操作权限！");
            }
            if (_orderService.GetOrderById(orderProductModel.OrderId).State >= OrderState.Uploaded)
            {
                return Error("订单已上传或已完成无法再修改商品信息！");
            }
            var orderProduct = _orderService.GetOrderProductByIdOne(orderProductModel.Id);
            if (orderProduct == null)
            {
                return Error("未找到订单商品，数据错误");
            }
            try
            {
                SaleProduct saleProduct = _productService.GetSaleProductBySaleProductId(orderProduct.SaleProductId);
                var changeQuantity = orderProductModel.Quantity - orderProduct.Quantity;
                if (changeQuantity > saleProduct.AvailableStock)
                {
                    return Error("商品可用库存不足！");
                }
                saleProduct.LockStock += changeQuantity;
                saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                _productService.UpdateSaleProduct(saleProduct);
                //更新销售商品的库存到商城
                var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                _productService.SyncProductStockToAssist(501, saleProduct.Product.Code, saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock)));

                _orderService.UpdateOrderProduct(orderProductModel.ToEntity());//已修改订单总价
                var order = _orderService.GetOrderById(orderProductModel.OrderId);
                //把商品锁定添加到锁定跟踪表（SaleProductLockedTrack）以及oms库存表（SaleProductWareHouseStock）
                var result = _productService.UpdateOrderProChangeLockedLog(order.Id, saleProduct.Id);
                if (!result.Contains("成功"))
                {
                    #region 订单日志(更新锁定信息失败)
                    _logService.InsertOrderLog(order.Id, "更新锁定信息失败", order.State, order.PayState, result);
                    #endregion
                }
                #region 日志（修改B2B订单商品）
                _logService.InsertOrderLog(orderProductModel.OrderId, "修改B2B订单商品", order.State, order.PayState, "修改订单商品[" + orderProductModel.ProductName + "],价格[" + orderProductModel.Price + "],数量[" + orderProductModel.Quantity + "]");
                #endregion
                return Success();
            }
            catch (Exception ex)
            {

                _logService.Error("B2B订单更新商品信息错误：", ex);
                return Error("B2B订单更新商品信息错误！");
            }

        }
        public IActionResult CheckOrderProductCount(int orderId, int productId)
        {
            var count = _orderService.CheckOrderProductCount(orderId, productId);
            return Success("已有同款商品！", count);
        }
        [HttpPost]
        public IActionResult GetOrderProducts(string search, int orderId, int pageSize, int pageIndex)
        {
            var data = _orderService.GetOrderProductByOrderId(orderId, pageIndex, pageSize, search);
            //entity To Model
            //var orderProductModel = new PageList<OrderProductModel>(data.Select(x => { return x.ToModel(); }), data.PageIndex, data.PageSize, data.TotalCount);
            var orderProductModel = new PageList<Object>(data, data.PageIndex, data.PageSize, data.TotalCount);
            return Success(orderProductModel);
        }
        [HttpPost]
        public IActionResult GetOrderProductInfo(int id)
        {
            var data = _orderService.GetOrderProductById(id);
            return Success(data);
        }
        [HttpPost]
        public IActionResult DeleteOrderProductById(int id, int orderId, string productName, int quantity)
        {
            if (!_permissionService.Authorize("DeleteB2BOrderProduct"))
            {
                return Error("无操作权限！");
            }
            if (_orderService.GetOrderById(orderId).State >= OrderState.Uploaded)
            {
                return Error("订单已上传或已完成无法再修改商品信息！");
            }
            var orderProduct = _orderService.GetOrderProductByIdOne(id);
            if (orderProduct == null)
            {
                return Error("未找到订单商品，数据错误");
            }
            var order = _orderService.GetOrderById(orderId);
            try
            {
                SaleProduct saleProduct = _productService.GetSaleProductBySaleProductId(orderProduct.SaleProductId);

                //把商品锁定添加到锁定跟踪表（SaleProductLockedTrack）以及oms库存表（SaleProductWareHouseStock）
                var result = _productService.DeleteProLockedNumByHasLockedNum(order.Id, orderProduct.Id);
                if (!result)
                {
                    #region 订单日志(更新锁定信息失败)
                    _logService.InsertOrderLog(order.Id, "更新锁定信息失败", order.State, order.PayState, "更新锁定信息失败");
                    #endregion
                }
                //删除订单商品
                _orderService.DeleteOrderProductById(id);
                //更新销售商品库存
                saleProduct.LockStock -= quantity;
                saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                _productService.UpdateSaleProduct(saleProduct);
                //更新销售商品的库存到商城
                var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                _productService.SyncProductStockToAssist(501, saleProduct.Product.Code, saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock)));

                #region 日志（删除订单商品）
                _logService.InsertOrderLog(order.Id, "删除商品", order.State, order.PayState, "删除订单商品[" + productName + "]，数量[" + quantity + "]");
                #endregion

                //重新计算订单总价
                var products = _orderService.GetOrderProductsByOrderId(orderId);
                decimal sumPrice = 0;
                foreach (var i in products)
                {
                    sumPrice += i.Quantity * i.Price;
                }
                order.SumPrice = sumPrice;
                _orderService.UpdateOrder(order);


                return Success();
            }
            catch (Exception ex)
            {
                _logService.Error("B2B订单" + order.SerialNumber + "删除商品错误：", ex);
                return Error("B2B订单删除商品错误！");
            }
        }
        /// <summary>
        /// 提交订单审核
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SubmitApproval(int orderId)
        {
            if (!_permissionService.Authorize("SubmitApprovalB2BOrder"))
            {
                return Error("无操作权限！");
            }
            var result = _orderService.SubmitApproval(orderId, out string msg);
            if (result)
            {
                var order = _orderService.GetOrderById(orderId);
                #region 日志（提交审核订单）
                _logService.InsertOrderLog(order.Id, "提交订单审核", order.State, order.PayState, "提交订单审核");
                #endregion
                return Success();
            }
            else
            {
                return Error(msg);
            }
        }
        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ApprovalOrder(int orderId)
        {
            if (!_permissionService.Authorize("ApprovalB2BOrder"))
            {
                return Error("无操作权限！");
            }
            //审核流程，暂时不用
            //var result= _orderService.ApprovalOrder(orderId, state, out string msg);

            var order = _orderService.GetOrderById(orderId);
            if (order.State != OrderState.ToBeTurned)
            {
                return Error("订单不是待转单状态，无需审核，请刷新页面！");
            }
            order.State = OrderState.Confirmed;
            _orderService.UpdateOrder(order);

            #region 日志（审核订单）
            _logService.InsertOrderLog(order.Id, "审核订单", order.State, order.PayState, "审核订单");
            #endregion
            return Success();
        }
        /// <summary>
        /// 财务确认
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ConfirmOrder(int orderId)
        {
            if (!_permissionService.Authorize("ConfirmB2BOrder"))
            {
                return Error("无操作权限！");
            }
            var result = _orderService.ConfirmOrder(orderId, out string msg);
            var order = _orderService.GetOrderById(orderId);
            if (result)
            {
                #region 日志（财务确认）
                _logService.InsertOrderLog(order.Id, "确认订单", order.State, order.PayState, "确认订单");
                #endregion
                return Success();
            }
            else
            {
                return Error(msg);
            }
        }
        /// <summary>
        /// 记账
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BookKeeping(OrderModel orderModel)
        {
            if (!_permissionService.Authorize("BookKeeping"))
            {
                return Error("无操作权限！");
            }
            var result = _orderService.BookKeeping(orderModel, out string msg);
            if (result)
            {
                var order = _orderService.GetOrderById(orderModel.Id);
                if (order.Type == OrderType.B2B_TH)
                {
                    order.State = OrderState.Bookkeeping;
                }

                var payType = _commonService.GetBaseDictionaryList(DictionaryType.PayStyle).Where(r => r.Id == orderModel.PayType).FirstOrDefault();
                var payMentType = _commonService.GetBaseDictionaryList(DictionaryType.PayType).Where(r => r.Id == orderModel.PayMentType).FirstOrDefault();
                #region 日志（财务记账）
                _logService.InsertOrderLog(order.Id, "财务记账", order.State, order.PayState, string.Format("财务记账，类型：{0}，付款方式：{1}，汇款方式：{2}，总金额：{3}，已支付金额：{4}，此次支付金额：{5}", orderModel.IsPayOrRefund ? "汇款" : "退款", payType != null ? payType.Value : "", payMentType != null ? payMentType.Value : "", order.SumPrice, order.PayPrice, orderModel.PayPrice));
                #endregion
                return Success();
            }
            else
            {
                return Error(msg);
            }
        }
        /// <summary>
        /// 确认完成B2B订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ConfirmCompleteB2BOrder(int orderId)
        {
            if (!_permissionService.Authorize("CompleteB2BOrder"))
            {
                return Error("无操作权限！");
            }
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { isSucc = false, msg = "数据错误，未找到该订单" });
            if (order.State != OrderState.CheckAccept)
            {
                return Error("订单不是验收状态，不能进行确认完成，请刷新页面！");
            }
            order.State = OrderState.Finished;
            _orderService.UpdateOrder(order);

            #region 日志（财务记账）
            _logService.InsertOrderLog(order.Id, "确认完成", order.State, order.PayState, "订单确认完成");
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
             */
            if (!_permissionService.Authorize("SetInvalid"))
            {
                return Error("无操作权限！");
            }
            try
            {
                Order order = _orderService.GetOrderByIdB2C(orderId);
                var resJudge = _commonService.JudgeOptOrderState(order.State, OrderState.Invalid,OptTypeEnum.B2BInvalid);
                if (!string.IsNullOrEmpty(resJudge))
                {
                    return Error(resJudge);
                }
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
                _logService.Error("设置B2B订单无效错误：", ex);
                return Error("设置B2B订单无效错误！");
            }
        }
        /// <summary>
        /// 更新财务备注
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateFinanceMark(int orderId, string mark = "")
        {
            if (!_permissionService.Authorize("UpdateFinanceMarkForB2BOrder"))
            {
                return Error("无操作权限！");
            }
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { isSucc = false, msg = "数据错误，未找到该订单" });

            order.FinanceMark = mark;
            _orderService.UpdateOrder(order);

            #region 日志（财务记账）
            _logService.InsertOrderLog(order.Id, "修改财务备注", order.State, order.PayState, "修改财务备注");
            #endregion

            return Success();
        }
        /// <summary>
        /// 验收
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CheckAcceptB2BOrder(int orderId)
        {
            if (!_permissionService.Authorize("CheckAcceptB2BOrder"))
            {
                return Error("无操作权限！");
            }
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { isSucc = false, msg = "数据错误，未找到该订单" });
            if (order.State != OrderState.Delivered)
            {
                return Error("订单不是发货状态不能进行验收！");
            }
            order.State = OrderState.CheckAccept;
            _orderService.UpdateOrder(order);

            #region 日志（财务记账）
            _logService.InsertOrderLog(order.Id, "验收订单", order.State, order.PayState, "订单验收完成");
            #endregion

            return Success();
        }
        [HttpPost]
        public IActionResult DeleteOrder(int orderId)
        {
            if (!_permissionService.Authorize("DeleteB2BOrder"))
            {
                return Error("无操作权限！");
            }
            var result = _orderService.DeleteOrder(orderId, out string msg);
            if (result)
            {
                var order = _orderService.GetOrderByIdWithAllState(orderId);
                #region 日志（删除订单）
                _logService.InsertOrderLog(order.Id, "删除订单", order.State, order.PayState, "删除订单");
                #endregion
                return Success();
            }
            else
            {
                return Error(msg);
            }
        }
        [HttpPost]
        public IActionResult GetDefaultInvoiceInfo(int orderId)
        {
            var data = _orderService.GetDefaultInvoiceInfo(orderId).ToModel();
            return Success(data);
        }
        /// <summary>
        /// 发票信息
        /// </summary>
        /// <param name="invoiceInfoModel"></param>
        /// <param name="orderId"></param>
        /// <param name="invoiceMode"></param>
        /// <param name="invoiceType"></param>
        /// <returns></returns>
        public IActionResult SubmitOrderInvoiceInfo(InvoiceInfoModel invoiceInfoModel, int orderId, int invoiceMode, int invoiceType)
        {
            _orderService.SubmitOrderInvoiceInfo(invoiceInfoModel, orderId);

            var order = _orderService.GetOrderById(orderId);
            order.InvoiceType = (InvoiceType)invoiceType;
            order.InvoiceMode = (InvoiceModeEnum)invoiceMode;
            _orderService.UpdateOrder(order);
            #region 日志（修改发票）
            _logService.InsertOrderLog(order.Id, "修改发票", order.State, order.PayState, "修改订单发票信息！");
            #endregion
            return Success();

        }
        public IActionResult GetOrderLog(int orderId)
        {
            Dictionary<int, string> dUser = new Dictionary<int, string>();
            foreach (var item in _userService.GetAllUsers())
            {
                dUser.Add(item.Id, item.Name);
            }
            ViewBag.User = dUser;
            var orderStateStr = EnumExtensions.GetEnumList((Enum)OrderState.B2CConfirmed);

            var log = _orderService.GetOrderRecord(orderId);
            var data = new
            {
                dUser,
                log,
                orderStateStr
            };
            return Success(data);
        }
        /// <summary>
        /// 订单退货（一键退货）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IActionResult SetRefundOrder(int orderId)
        {
            //权限
            if (!_permissionService.Authorize("SetB2BRefundOrder"))
            {
                return Error("没有权限！");
            }
            //可以生成退单的状态
            var canRefundOrderStates = new OrderState[] {
                OrderState.Delivered,
                OrderState.CheckAccept,
                OrderState.Finished
            };

            //事务控制
            using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
            {

                try
                {
                    var data = _orderService.GetOrderById(orderId);
                    if (data == null) return Error("未找到订单信息！");
                    //判断订单的退货程度
                    if (_orderService.JudgeOrderRefundState(orderId) != RefundState.No) return Error("订单已经产生了退货不能在进行一键退货！");
                    //订单已完成且订单中商品数不能超过原订单商品数
                    if (!canRefundOrderStates.Contains(data.State))
                    {
                        return Error("订单未发货，不能进行退货！");
                    }
                    else
                    {
                        #region order[退单信息]
                        Order order = new Order();
                        order.SerialNumber = _commonService.GetOrderSerialNumber("PT");
                        order.Type = OrderType.B2B_TH;
                        order.PSerialNumber = (data.PSerialNumber ?? data.SerialNumber) + "(T" + (_omsAccessor.Get<Order>().Where(x => x.Isvalid && x.Type == OrderType.B2B_TH && x.OriginalOrderId == data.Id).Count() + 1) + ")";
                        order.OrgionSerialNumber = data.SerialNumber + "(T" + (_omsAccessor.Get<Order>().Where(x => x.Isvalid && x.Type == OrderType.B2B_TH && x.OriginalOrderId == data.Id).Count() + 1) + ")" + (string.IsNullOrEmpty(data.OrgionSerialNumber) ? "" : "|" + data.OrgionSerialNumber);
                        order.OriginalOrderId = data.Id;//原订单Id
                        order.State = OrderState.ToBeConfirmed;
                        order.PayType = data.PayType;
                        order.PayMentType = data.PayMentType;
                        order.PayState = data.PayState;
                        order.TransDate = data.TransDate;
                        order.SumPrice = data.SumPrice;
                        order.PayPrice = data.PayPrice;
                        order.PayDate = data.PayDate;
                        order.DeliveryTypeId = data.DeliveryTypeId;
                        order.CustomerName = data.CustomerName;
                        order.CustomerPhone = data.CustomerPhone;
                        order.AddressDetail = data.AddressDetail;
                        order.CustomerMark = data.CustomerMark;
                        order.AdminMark = data.AdminMark;
                        order.ToWarehouseMessage = data.ToWarehouseMessage;
                        order.WarehouseId = data.WarehouseId;
                        order.PriceTypeId = data.PriceTypeId;
                        order.CustomerId = data.CustomerId;
                        order.ApprovalProcessId = data.ApprovalProcessId;
                        order.SalesManId = data.SalesManId;
                        #endregion
                        var rfOrder = _orderService.CreatedB2COrder(order);

                        #region 订单商品
                        //插入商品
                        var resultOrderProducts = _orderService.GetOrderProductsByOrderId(data.Id);
                        _orderService.AddProductsToNewOrder(data.Id, rfOrder.Id, out int saleProductsCount, out decimal newOrderSumPrice);
                        _logService.InsertOrderLog(rfOrder.Id, "生成销售退单", rfOrder.State, rfOrder.PayState, "生成销售退单[" + rfOrder.SerialNumber + "]");//创建退单日志
                        #endregion

                        //退单生成完成后更新订单状态
                        data.State = OrderState.Invalid;
                        _orderService.UpdateOrder(data);
                        //日志
                        #region 日志（生成B2B退单）
                        _logService.InsertOrderLog(data.Id, "销售退单", data.State, data.PayState, "订单已作废,生成销售退单[" + rfOrder.SerialNumber + "]");//订单退单日志
                        #endregion
                        //提交事务
                        tran.Commit();
                        return Success(new { refundOrderId = rfOrder.Id, refundSerialNumber = rfOrder.SerialNumber });
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
        /// 订单退货（部分退货）
        /// </summary>
        /// <returns></returns>
        public JsonResult PartRefund(int orderId, string refundProductInfo)
        {
            if (!_permissionService.Authorize("SplitOrder"))
            {
                return Error("无操作权限！");
            }
            //可以生成退单的状态
            var canRefundOrderStates = new OrderState[] {
                OrderState.Delivered,
                OrderState.CheckAccept,
                OrderState.Finished
            };
            var refundProductInfoData = JArray.Parse(refundProductInfo);
            using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    //权限
                    var data = _orderService.GetOrderByIdB2C(orderId);
                    if (data == null || !canRefundOrderStates.Contains(data.State)) return Error("未找到订单信息或者订单没有发货不能进行退单操作！");
                    //判断订单的退货程度
                    if (_orderService.JudgeOrderRefundState(orderId) == RefundState.All) return Error("订单已经全部不能在进行部分退货！");
                    var num = _commonService.GetOrderSerialNumber("PT");
                    #region 销售退单模型
                    Order refundOrderModel = new Order
                    {
                        SerialNumber = num,
                        Type = OrderType.B2B_TH,
                        ShopId = data.ShopId,
                        PSerialNumber = (data.PSerialNumber ?? data.SerialNumber) + "(T" + (_omsAccessor.Get<Order>().Where(x => x.Isvalid && x.Type == OrderType.B2B_TH && x.OriginalOrderId == data.Id).Count() + 1) + ")",
                        OrgionSerialNumber = data.SerialNumber + "(T" + (_omsAccessor.Get<Order>().Where(x => x.Isvalid && x.Type == OrderType.B2B_TH && x.OriginalOrderId == data.Id).Count() + 1) + ")" + (string.IsNullOrEmpty(data.OrgionSerialNumber) ? "" : "|" + data.OrgionSerialNumber),
                        OriginalOrderId = data.Id,
                        State = OrderState.ToBeConfirmed,
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
                        PriceTypeId = data.PriceTypeId,
                        CustomerId = data.CustomerId,
                        SalesManId = data.SalesManId,
                        CustomerMark = data.CustomerMark
                    };
                    #endregion
                    var rfOrder = _orderService.CreatedB2COrder(refundOrderModel);

                    //插入商品
                    var partRefundProductRes = _orderService.AddPartRefundProducts(data.Id, rfOrder.Id, refundProductInfoData);
                    if (!partRefundProductRes) { tran.Rollback(); return Error("部分退货发生错误！"); }
                    _logService.InsertOrderLog(rfOrder.Id, "生成销售退单", rfOrder.State, rfOrder.PayState, "生成销售退单[" + num + "]");//创建退单日志

                    //插入商品后重新计算退单的总金额
                    rfOrder.SumPrice = _orderService.GetOrderProductsByOrderId(rfOrder.Id).Sum(x => x.SumPrice);
                    _orderService.UpdateOrder(rfOrder);
                    //退单生成完成后更新订单状态
                    if (_orderService.JudgeOrderRefundState(data.Id) == RefundState.All)
                    {
                        data.State = OrderState.Invalid;
                        data = _orderService.UpdateOrder(data);
                        _logService.InsertOrderLog(data.Id, "销售退单", data.State, data.PayState, "订单全部退货完成已作废,生成销售退单[" + num + "]");//订单退单日志
                    }
                    else
                    {
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
        /// 匹配仓库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult MatchWareHouseForOrder(int id)
        {
            //请求wms匹配仓库（暂时不要删除）
            //int firstWareHouseId = 0;
            //string res = _orderService.MatchWareHouseForOrder(id,out firstWareHouseId);
            Order order = _orderService.GetOrderByIdB2C(id);
            //直接按地址匹配仓库
            string res = _orderService.MatchFirstWareHouse(order.AddressDetail);
            var result = res.ToObj<MatchWareHouseModel>();
            var mark = "";
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
        /// 上传订单到WMS
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [UserAnonymous]
        public async Task<IActionResult> UploadB2BOrderToWMS(int OrderId)
        {
            try
            {

                var stockRes = _orderService.CheckWareHouseStock(OrderId);
                var stockResult = JsonHelper.ToObj<ResultModel>(stockRes);
                if (stockResult.isSucc)
                {
                    var order = _orderService.GetOrderById(OrderId);

                    if (order.State != OrderState.FinancialConfirmation)
                    {
                        return Error("订单不是确认状态无法上传，请刷新页面！");
                    }
                    //判断订单是否缺货（缺货不让确认订单）
                    var isLackStock = _orderService.IsOrderOutofStock(order);
                    if (isLackStock)
                    {
                        return Error("订单中存在商品缺货，请进行缺货解锁或者重新下单！");
                    }

                    //判断订单商品锁定库存与仓库库存直接的关系（存在商品库存小于锁定库存的情况这个时候就得还原，重新进行缺货处理！)
                    var isCheckWHS = _orderService.IsCheckWarehouseStockCanUse(order);
                    if (!isCheckWHS)
                    {
                        return Error("订单中存在商品的仓库库存发生了调整，请进行调整订单商品信息后进行提交订单！");
                    }

                    //非客户自提需要填写收货人信息
                    if (!order.Delivery.Code.Contains("KHZT")) {
                        //判断订单信息是否完全
                        if (string.IsNullOrEmpty(order.AddressDetail))
                        {
                            return Error("非客户自提订单必须填写地址信息！");
                        }

                        if (string.IsNullOrEmpty(order.CustomerName))
                        {
                            return Error("非客户自提订单必须填写收货人信息！");
                        }

                        if (string.IsNullOrEmpty(order.CustomerPhone))
                        {
                            return Error("非客户自提订单必须填写收货人电话信息！");
                        }
                    } 



                    var delivery = _deliveriesService.GetDeliveryById(order.DeliveryTypeId);
                    //如果订单选择德邦快递，则需要检查德邦快递是否满足配送到收货地址
                    if (delivery.Code == "DB" || delivery.Name == "德邦")
                    {
                        var dbCheckRes = _orderService.CheckDeBangIsDelivery(OrderId);//检查德邦快递是否满足
                        var dbCheckResult = JsonHelper.ToObj<DBDeliveryCheckModel>(dbCheckRes);
                        if (dbCheckResult.isSucc)//调用接口成功
                        {
                            if (dbCheckResult.isDelivery)//满足配送到收货地址
                            {
                                string res = await _orderService.UploadOrder(OrderId);
                                var result = JsonHelper.ToObj<ResultModel>(res);
                                if (result.isSucc)
                                {
                                    order.State = OrderState.Uploaded;
                                    order.ModifiedBy = WorkContext.CurrentUser.Id;
                                    order.ModifiedTime = DateTime.Now;
                                    order.WriteBackState = WriteBackState.WriteSuccess;

                                    _orderService.UpdateOrder(order);

                                    #region 订单日志(确认订单)
                                    OrderLog orderLog = new OrderLog
                                    {
                                        OrderId = order.Id,
                                        OrderState = order.State,
                                        PayState = order.PayState,
                                        OptionType = "上传订单",
                                        Mark = "上传订单"
                                    };
                                    _logService.InsertOrderLog(orderLog);
                                    #endregion

                                    return Success("订单已完成上传WMS操作！");
                                }
                                else
                                    return Error(result.msg);
                            }
                            else
                            {
                                return Error("该收货地址德邦快递不能正常配送，请更换快递方式！");
                            }
                        }
                        else
                        {
                            _logService.Error(dbCheckResult.msg);
                            return Error("调用检查德邦快递配送接口失败");
                        }
                    }
                    else
                    {
                        string res = await _orderService.UploadOrder(OrderId);
                        var result = JsonHelper.ToObj<ResultModel>(res);
                        if (result.isSucc)
                        {
                            order.State = OrderState.Uploaded;
                            order.ModifiedBy = WorkContext.CurrentUser.Id;
                            order.ModifiedTime = DateTime.Now;
                            order.WriteBackState = WriteBackState.WriteSuccess;

                            _orderService.UpdateOrder(order);

                            #region 订单日志(确认订单)
                            OrderLog orderLog = new OrderLog
                            {
                                OrderId = order.Id,
                                OrderState = order.State,
                                PayState = order.PayState,
                                OptionType = "上传订单",
                                Mark = "上传订单"
                            };
                            _logService.InsertOrderLog(orderLog);
                            #endregion

                            return Success("订单已完成上传WMS操作！");
                        }
                        else
                            return Error(result.msg);
                    }
                }
                else
                {
                    return Error(stockResult.msg);
                }

            }
            catch (Exception ex)
            {
                _logService.Error("B2B订单上传失败:", ex);
                return Error("订单上传失败！");
            }
        }
        /// <summary>
        /// 取消上传B2B订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [UserAnonymous]
        public IActionResult CancelUploadB2BOrder(int OrderId)
        {
            try
            {
                string res = _orderService.CancelUploadOrder(OrderId);
                var result = JsonHelper.ToObj<CancelOrderResultModel>(res);
                if (result.isSucc)
                {
                    var order = _orderService.GetOrderById(OrderId);

                    if (order.State != OrderState.Uploaded)
                    {
                        return Error("订单不是上传状态无法取消上传，请刷新页面！");
                    }
                    order.State = OrderState.FinancialConfirmation;
                    order.WriteBackState = WriteBackState.NoWrite;
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
                    return Success("订单取消上传成功！");
                }
                else
                {
                    return Error(result.msg);
                }

            }
            catch (Exception ex)
            {

                _logService.Error("B2B订单取消上传失败:", ex);
                return Error("订单取消上传失败！");
            }


        }
        /// <summary>
        /// 导出订单信息
        /// </summary>
        /// <returns></returns>
        public IActionResult ExportOrder(string searchB2BOrderModelStr, bool isOrderDetail, string exportType = "")
        {

            try
            {
                var searchB2BOrderModel = new SearchB2BOrderModel();
                searchB2BOrderModel = JsonConvert.DeserializeObject<SearchB2BOrderModel>(searchB2BOrderModelStr);

                //获取导出数据
                var data = _orderService.GetAllB2BOrdersByCommand(searchB2BOrderModel, isOrderDetail);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");


                //设置Excel表头
                Dictionary<string, string> columnNames = CommonTools.OutPutTemplate(OutPutType.B2B);

                //是否是导出订单详情
                if (isOrderDetail)
                {
                    columnNames = CommonTools.OutPutTemplate(OutPutType.B2BDetail);
                }
                DataTable table = data.ToDataTable(columnNames);


                //导出.xls和.cvs格式的文件
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
                    return Json(new { Result = false, Message = "错误信息：导出失败" });

            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }
        /// <summary>
        /// 修改物流单号
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="deliveryNum"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ChangeDeliveryNum(int orderId, string deliveryNum)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order != null)
            {
                var oldDeliveryNum = order.DeliveryNumber;
                order.DeliveryNumber = deliveryNum;
                _orderService.UpdateOrder(order);
                #region 日志
                _logService.InsertOrderLog(order.Id, "修改物流单号", order.State, order.PayState, "物流单号由：" + oldDeliveryNum + " 更改为：" + deliveryNum);
                #endregion
            }
            return Success();
        }

        /// <summary>
        /// 批量导入商品
        /// </summary>
        /// <param name="file"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IActionResult B2BProductImport(IFormFile file, int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null) return Error("没找到订单信息！");
            if (!(order.State < OrderState.Uploaded && order.State != OrderState.Delivered && order.State != OrderState.Invalid))
            {
                return Error("订单已上传或已完成或者为无效订单，不能进行添加商品！");
            }
            IWorkbook workbook = null;
            if (file == null)
            {
                return Error("请添加要导入文件");
            }
            else
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName; using (FileStream fs = System.IO.File.Create(fileName))
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

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
                    {
                        row = sheet.GetRow(i);
                        try
                        {
                            //插入订单商品    
                            var product = _productService.GetProductByCode(row.GetCell(0).ToString().Trim());
                            if (product == null)
                            {
                                tran.Rollback();
                                errorStr.Append(string.Format("未找到编码为{0}的商品;", row.GetCell(0).ToString().Trim()));
                                errorCount++;
                                continue;
                            }
                            else
                            {
                                var saleProduct = _productService.GetSaleProduct(product.Id);
                                if (saleProduct == null)
                                {
                                    tran.Rollback();
                                    errorStr.Append(string.Format("未在销售商品列表中找到编码为{0}的商品;", row.GetCell(0).ToString().Trim()));
                                    errorCount++;
                                    continue;
                                }
                                else
                                {
                                    if (saleProduct.AvailableStock < Convert.ToInt32(row.GetCell(1).ToString().Trim()))
                                    {
                                        tran.Rollback();
                                        errorStr.Append(string.Format("销售商品编码{0}的商品，可售库存{1}小于商品数量{2}，库存不足;", row.GetCell(0).ToString().Trim(), saleProduct.AvailableStock, Convert.ToInt32(row.GetCell(1).ToString().Trim())));
                                        errorCount++;
                                        continue;
                                    }
                                    var orderProducts = _orderService.GetOrderProductsByOrderId(order.Id).Where(x => x.SaleProductId == saleProduct.Id).FirstOrDefault();
                                    if (orderProducts != null)
                                    {
                                        tran.Rollback();
                                        errorStr.Append(string.Format("销售商品编码{0}的商品，订单已经存在该商品不能重复插入！", row.GetCell(0).ToString().Trim()));
                                        errorCount++;
                                        continue;
                                    }
                                    var orderProduct = new OrderProduct();
                                    {
                                        orderProduct.OrderId = order.Id;
                                        orderProduct.SaleProductId = saleProduct.Id;
                                        orderProduct.Quantity = Convert.ToInt32(row.GetCell(1).ToString().Trim());
                                        orderProduct.OrginPrice = Convert.ToDecimal(row.GetCell(2).ToString().Trim());
                                        orderProduct.Price = Convert.ToDecimal(row.GetCell(4).ToString().Trim());
                                        orderProduct.SumPrice = Convert.ToDecimal(row.GetCell(5).ToString().Trim());
                                    }
                                    _orderService.AddOrderProductB2C(orderProduct);
                                    //库存锁定跟踪
                                    _productService.CreateSaleProductLockedTrackAndWareHouseStock(order.Id, saleProduct.Id, order.WarehouseId, orderProduct.Quantity, orderProduct.Id);
                                }
                                //更新商品的锁定库存和可用库存
                                saleProduct.LockStock += Convert.ToInt32(row.GetCell(1).ToString().Trim());
                                saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                                _productService.UpdateSaleProduct(saleProduct);
                                //需要同步库存的商品Id集合
                                productIds.Add(saleProduct.ProductId);
                            }
                            //更新订单总价
                            order.SumPrice = _orderService.GetOrderProductsByOrderId(order.Id).Sum(x => x.SumPrice);
                            _orderService.UpdateOrder(order);

                            tran.Commit();
                            succCount++;

                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            errorStr.Append(string.Format("商品编号为{0}插入失败;", row.GetCell(0).ToString().Trim()));
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
                    string err = string.Format("总共{0}个商品，其中导入成功{1}个商品，失败{2}个商品，导入错误的商品信息：{3}", sheet.LastRowNum, succCount, errorCount, errorStr.ToString());
                    _logService.Error(err);
                    return Error(err);
                }
                else
                {
                    return Success(string.Format("总共{0}个商品，导入成功{1}个商品", sheet.LastRowNum, succCount));
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
            var oldOrder = _orderService.GetOrderById(orderId);
            //判断订单状态
            if (oldOrder.State != OrderState.Invalid)
            {
                return Error("当前订单状态无法复制订单，请尝试刷新页面！");
            }
            if (oldOrder.IsCopied == true)
            {
                return Error("订单无法复制或已经复制过订单，请在已经复制生成订单处复制订单！");
            }
            var result = _orderService.CopyB2BOrder(orderId);
            if (!result.Contains("成功"))
            {
                return Error(result);
            }
            return Success("");
        }
        /// <summary>
        /// 解锁缺货订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IActionResult UnLockLackOrder(int orderId)
        {
            //判断订单是否为缺货订单，防止重复多点
            var order = _orderService.GetOrderById(orderId);
            if (order.IsLackStock == false)
            {
                return Error("当前订单未缺货，请尝试刷新页面后再试！");
            }
            var result = _productService.UnLockLackStockOrder(new List<int>() { orderId });
            if (result != "")
            {
                return Error(result + "订单状态有误！无法解除缺货");
            }
            else
            {
                var failOrder = "";
                var orderResult = _orderService.GetOrderById(orderId);
                var orderIsLackList = _orderService.GetOrderProductsModelByOrderId(orderId, orderResult.WarehouseId);
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
        /// 补录商城购买帐号
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IActionResult UpdateOrderUserName(int orderId,string userName)
        {
            var data = _orderService.GetOrderById(orderId);
            if (data == null)
            {
                return Error("无法找到该订单！");
            }
            if (data.UserName != null && data.UserName.Trim() != "")
            {
                return Error("已填入购买者帐号，需要修改请联系管理员！");
            }
            data.UserName = userName;
            _orderService.UpdateOrder(data);
            #region 日志
            _logService.InsertOrderTableLog("Order", orderId, "修改商城购买帐号", (int)data.State, "修改商城购买帐号为：" + userName);
            #endregion
            return Success();
        }
        #endregion

        
        #region B2B退单
        public IActionResult GetB2BRefundOrderList(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? warehouseId, string searchStr)
        {
            var data = _orderService.GetB2BRefundOrderList(pageIndex, pageSize, startTime, endTime, warehouseId, searchStr);
            return Success(data);
        }
        [HttpPost]
        public IActionResult AddB2BRefundOrderProduct(OrderProductModel orderProductModel)
        {
            if (!_permissionService.Authorize("AddB2BOrderProducts"))
            {
                return Error("无操作权限！");
            }
            var order = _orderService.GetOrderById(orderProductModel.OrderId);
            if (order == null)
                return Error("数据错误，订单不存在！");
            if (order.State >= OrderState.Uploaded)
            {
                return Error("订单已上传或已完成无法再修改商品信息！");
            }
            //原订单
            var originalOrder = _orderService.GetOrderBySerialNumber(order.OrgionSerialNumber);
            var oriCount = originalOrder.OrderProduct.Where(r => r.Isvalid && r.SaleProductId == orderProductModel.SaleProductId).Sum(r => r.Quantity);//原订单中商品数量
            if (oriCount <= 0)
                return Error(string.Format("订单号为{0}的订单商品中不存在商品名为{1}的商品，不能添加到退单中", order.OrgionSerialNumber, orderProductModel.ProductName));

            //原订单生成的所有的退单
            var refundOrders = _orderService.GetRefundOrdersByOriginalSerialNumber(order.OrgionSerialNumber);
            var refundProducts = refundOrders.Select(r => r.OrderProduct);
            List<OrderProduct> allProducts = new List<OrderProduct>();
            foreach (var item in refundProducts)
            {
                allProducts.AddRange(item);
            }
            //原订单中的该款商品已经生成退单的商品数量
            var refundCount = allProducts.Where(r => r.Isvalid && r.SaleProductId == orderProductModel.SaleProductId).Sum(r => r.Quantity);
            if (refundCount + orderProductModel.Quantity > oriCount)
                return Error(string.Format("订单号为{0}生成的所有退单中，商品名为{1}的酒款的退货总数大于原订单中的数量，不能添加", order.OrgionSerialNumber, orderProductModel.ProductName));

            var result = _orderService.ConfirmOrderProductIsExist(orderProductModel.OrderId, orderProductModel.SaleProductId);
            if (result == null)
            {
                _orderService.AddOrderProduct(orderProductModel.ToEntity());
                //var order = _orderService.GetOrderById(orderProductModel.OrderId);
                #region 日志（增加B2B订单商品）
                _logService.InsertOrderLog(order.Id, "新增B2B退单商品", order.State, order.PayState, "新增B2B退单商品[" + orderProductModel.ProductName + "] 数量[" + orderProductModel.Quantity + "]");
                #endregion
                //重新计算订单总价
                var products = _orderService.GetOrderProductsByOrderId(order.Id);
                decimal sumPrice = 0;
                foreach (var i in products)
                {
                    sumPrice += i.Quantity * i.Price;
                }
                order.SumPrice = sumPrice;
                _orderService.UpdateOrder(order);

                return Success();
            }
            else
            {
                result.Quantity += orderProductModel.Quantity;
                //result.SumPrice += orderProductModel.SumPrice;
                _orderService.UpdateOrderProduct(result);
                //return Error("已有该商品！");

                #region 日志（增加B2B订单商品）
                _logService.InsertOrderLog(order.Id, "新增B2B退单商品", order.State, order.PayState, "新增B2B退单商品[" + orderProductModel.ProductName + "] 数量[" + orderProductModel.Quantity + "]");
                #endregion
                return Success();
            }

        }
        /// <summary>
        /// 删除退货单商品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="orderId"></param>
        /// <param name="productName"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteRefundOrderProductById(int id, int orderId, string productName, int quantity)
        {
            if (!_permissionService.Authorize("DeleteB2BOrderProduct"))
            {
                return Error("无操作权限！");
            }
            if (_orderService.GetOrderById(orderId).State >= OrderState.Uploaded)
            {
                return Error("订单已上传或已完成无法再修改商品信息！");
            }
            var orderProduct = _orderService.GetOrderProductByIdOne(id);
            if (orderProduct == null)
            {
                return Error("未找到订单商品，数据错误");
            }
            _orderService.DeleteOrderProductById(id);
            var order = _orderService.GetOrderById(orderId);
            try
            {
                #region 日志（删除订单商品）
                _logService.InsertOrderLog(order.Id, "删除商品", order.State, order.PayState, "删除订单商品[" + productName + "]，数量[" + quantity + "]");
                #endregion

                //重新计算订单总价
                var products = _orderService.GetOrderProductsByOrderId(orderId);
                decimal sumPrice = 0;
                foreach (var i in products)
                {
                    sumPrice += i.Quantity * i.Price;
                }
                order.SumPrice = sumPrice;
                _orderService.UpdateOrder(order);


                return Success();
            }
            catch (Exception ex)
            {
                _logService.Error("B2B订单" + order.SerialNumber + "删除商品错误：", ex);
                return Error("B2B订单删除商品错误！");
            }
        }
        [HttpPost]
        public IActionResult UpdataB2BRefundOrder(OrderModel orderModel)
        {
            //权限
            if (!_permissionService.Authorize("UpdataB2BRefundOrder"))
            {
                return Error("无操作权限！");
            }
            var order = _orderService.GetOrderById(orderModel.Id);
            if (order == null)
                return Json(new { isSucc = false, msg = "数据错误，未找到该订单" + orderModel.Id });
            order.CustomerId = orderModel.CustomerId;
            order.CustomerName = orderModel.CustomerName;
            order.CustomerPhone = orderModel.CustomerPhone;
            order.AddressDetail = orderModel.AddressDetail;
            order.WarehouseId = orderModel.WarehouseId;
            order.CustomerMark = orderModel.CustomerMark;
            order.AdminMark = orderModel.AdminMark;
            order.ToWarehouseMessage = orderModel.ToWarehouseMessage;
            order.SalesManId = orderModel.SalesManId;
            _orderService.UpdateOrder(order);
            //日志
            #region 日志（修改订单）
            _logService.InsertOrderLog(order.Id, "修改订单", order.State, order.PayState, "修改订单");
            #endregion
            return Success();
        }
        public IActionResult SetB2BRefundOrderConfirm(int orderId, string updateType)
        {
            //权限
            if (!_permissionService.Authorize("ConfirmB2BRefundOrder"))
            {
                return Error("无操作权限！");
            }
            var order = _orderService.GetOrderById(orderId);
            if (updateType.Equals("Confirm"))
            {
                order.State = OrderState.Confirmed;
                _orderService.UpdateOrder(order);
                //日志
                #region 日志（确认订单（B2B退单））
                _logService.InsertOrderLog(orderId, "确认订单", order.State, order.PayState, "确认订单");
                #endregion
            }
            else if (updateType.Equals("UnConfirm"))
            {
                order.State = OrderState.ToBeConfirmed;
                _orderService.UpdateOrder(order);
                //日志
                #region 日志（反确认订单（B2B退单））
                _logService.InsertOrderLog(orderId, "反确认订单", order.State, order.PayState, "反确认订单");
                #endregion
            }
            return Success();
        }
        /// <summary>
        /// 上传B2B退单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UploadB2BRefundOrder(int id)
        {
            //权限
            if (!_permissionService.Authorize("UploadB2BRefundOrder"))
            {
                return Error("无操作权限！");
            }

            string message = _orderService.UploadB2BRefundOrder(id);

            var returnMessage = message.ToObj<MessageModel>();

            if (returnMessage.isSucc)
            {
                var order = _orderService.GetOrderById(id);
                if (order != null)
                {
                    if (order.State != OrderState.Confirmed)
                    {
                        return Error("订单不是确认状态无法上传，请刷新页面！");
                    }
                    order.State = OrderState.Uploaded;
                    _orderService.UpdateOrder(order);
                }

                //日志
                #region 日志（上传B2B退单）
                _logService.InsertOrderLog(id, "上传B2B退单", order.State, order.PayState, "上传B2B退单");
                #endregion
            }
            return Content(message);
        }
        /// <summary>
        /// 取消B2B退单上传
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UnUploadB2BOrder(int id)
        {
            string message = _orderService.UnUploadRefundOrder(id);

            var returnMessage = message.ToObj<MessageModel>();

            if (returnMessage.isSucc)
            {
                var order = _orderService.GetOrderById(id);
                if (order != null)
                {
                    if (order.State != OrderState.Uploaded)
                    {
                        return Error("订单不是上传状态无法取消上传，请刷新页面！");
                    }
                    order.State = OrderState.Confirmed;
                    _orderService.UpdateOrder(order);
                }

                //日志
                #region 日志（上传B2B退单）
                _logService.InsertOrderLog(id, "取消上传B2B退单", order.State, order.PayState, "取消上传B2B退单");
                #endregion
            }
            return Content(message);
        }
        /// <summary>
        /// 手动添加B2B退货单
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreatedB2BRefundOrder(OrderModel orderModel, List<OrderProductModel> orderProductModels)
        {
            if (!_permissionService.Authorize("AddB2BRefundOrder"))
            {
                return View("_AccessDeniedView");
            }

            var order = _orderService.GetOrderBySerialNumber(orderModel.OrgionSerialNumber.Trim());
            if (order == null)
                return Json(new { isSucc = false, msg = string.Format("未找到订单号为{0}的订单", orderModel.OrgionSerialNumber.Trim()) });


            if (orderProductModels.Count == 0)
                return Json(new { isSucc = false, msg = "请通过原单明细添加退单商品！" });

            #region 检查退单数量与原单是否相等
            //检测是否存在退单，如果存在退单，检测退单和原单相比是否是完整的，如果是，则不允许生成新的退单，如果不是，则需要和原单的商品比较，所有退单商品数量加起来不能超过原单的商品数量

            //获取到所有的退单
            var refundOrders = _orderService.GetRefundOrdersByOriginalSerialNumber(orderModel.OrgionSerialNumber.Trim());
            if (refundOrders.Count!=0)
            {
                if (order.OrderProduct.Count == 0)
                {
                    return Json(new { isSucc = false, msg = string.Format("订单号为{0}的订单中不存在商品，不能生成退单", orderModel.OrgionSerialNumber.Trim()) });
                }
                else
                {
                    //退单商品
                    List<OrderProduct> orderProducts = refundOrders.SelectMany(c=>c.OrderProduct).ToList();
           
                    //所有的已退单的商品按照销售商品ID分组，数量相加
                    var products = orderProducts.GroupBy(r => r.SaleProductId).Select(t => new { SaleProductId = t.Key, Quantity = t.Sum(s => s.Quantity) }).ToList();
                    //原订单的商品按照销售商品id分组，数量相加
                    var oldOrderProducts = order.OrderProduct.GroupBy(r => r.SaleProductId).Select(t => new { SaleProductId = t.Key, Quantity = t.Sum(r => r.Quantity) }).ToList();
                    foreach (var item in oldOrderProducts)
                    {
                        var product = products.Where(r => r.SaleProductId == item.SaleProductId).FirstOrDefault();
                        //此次生成退单的该款商品的数量
                        var addProduct = orderProductModels.Where(r => r.SaleProductId == item.SaleProductId).FirstOrDefault();
                        var addCount = addProduct == null ? 0 : addProduct.Quantity;
                        if (product != null)
                        {
                            if (product.Quantity + addCount > item.Quantity)
                            {
                                return Json(new { isSucc = false, msg = string.Format("订单号为{0}生成的所有退单中，商品名为{1}的总数大于原订单商品数量，不能生成退单", orderModel.OrgionSerialNumber.Trim(), addProduct.ProductName) });
                            }
                            else
                                continue;
                        }
                        else
                        {
                            continue;
                        }

                    }
                }
            }
            #endregion
            try
            {
                var newOrder = new Order()
                {
                    SerialNumber = _commonService.GetOrderSerialNumber("PT"),
                    OrgionSerialNumber = orderModel.OrgionSerialNumber,
                    OriginalOrderId = order.Id,
                    PriceTypeId = orderModel.PriceTypeId,
                    Type = OrderType.B2B_TH,
                    ShopId = order.ShopId,
                    State = OrderState.ToBeConfirmed,
                    WriteBackState = WriteBackState.NoWrite,
                    LockStock = false,
                    IsNeedPaperBag = false,
                    PayPrice = 0,
                    SumPrice = orderProductModels.Sum(r => r.SumPrice),
                    CustomerId = orderModel.CustomerId,
                };
                newOrder.WarehouseId = orderModel.WarehouseId;
                newOrder.DeliveryTypeId = orderModel.DeliveryTypeId;
                newOrder.CustomerName = orderModel.CustomerName;
                newOrder.CustomerPhone = orderModel.CustomerPhone;
                newOrder.AdminMark = orderModel.AdminMark;
                newOrder.SalesManId = orderModel.SalesManId;

                _omsAccessor.Insert<Order>(newOrder);
                _omsAccessor.SaveChanges();

                //var orderProduct = _orderService.GetOrderProductsModelByOrderId(order.Id);
                List<OrderProduct> orderProducts = new List<OrderProduct>();
                foreach (var item in orderProductModels)
                {
                    var orderProductModel = new OrderProduct()
                    {
                        OrderId = newOrder.Id,
                        SaleProductId = item.SaleProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        OrginPrice = item.OrginPrice,
                        SumPrice = item.SumPrice,
                        Type = item.Type
                    };
                    orderProducts.Add(orderProductModel);
                }
                _omsAccessor.InsertRange<OrderProduct>(orderProducts);
                _omsAccessor.SaveChanges();

                return Json(new { isSucc = true, id = newOrder.Id });

            }
            catch (Exception ex)
            {
                return Json(new { isSucc = false, msg = "出现异常，原因：" + ex.Message });
            }
        }
        /// <summary>
        /// 验收B2B退单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CheckAcceptB2BRefundOrder(int orderId)
        {
            if (_permissionService.Authorize("CheckAcceptB2BRefundOrder"))
            {
                return Error("没有权限");
            }
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { isSucc = false, msg = "数据错误，未找到该退单" });
            try
            {
                order.State = OrderState.CheckAccept;
                _orderService.UpdateOrder(order);

                #region 日志（财务验收）
                _logService.InsertOrderLog(order.Id, "退单验收", order.State, order.PayState, "退单验收完成");
                #endregion
                return Success();
            }
            catch (Exception ex)
            {
                return Json(new { isSucc = false, msg = "出现异常，原因：" + ex.Message });
            }
        }
        /// <summary>
        /// 记账B2B退单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BookkeepingB2BRefundOrder(int orderId)
        {
            if (_permissionService.Authorize("BookkeepingB2BRefundOrder"))
            {
                return Error("没有权限");
            }
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { isSucc = false, msg = "数据错误，未找到该退单" });
            try
            {
                order.State = OrderState.Bookkeeping;
                _orderService.UpdateOrder(order);

                #region 日志（财务记账）
                _logService.InsertOrderLog(order.Id, "退单记账", order.State, order.PayState, "退单记账完成");
                #endregion
                return Success();
            }
            catch (Exception ex)
            {
                return Json(new { isSucc = false, msg = "出现异常，原因：" + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult GetOriginalB2BOrder(string serialNumber = "")
        {
            if (string.IsNullOrEmpty(serialNumber))
                return Error("错误，原单号为空！");

            var order = _orderService.GetOrderBySerialNumber(serialNumber);
            if (order == null)
                return Error("未找到对应订单，请检查原单号是否正确！");
            var orderInfo = new
            {
                order.CustomerName,
                order.CustomerPhone,
                order.CustomerId,
                order.WarehouseId,
                order.DeliveryTypeId,
                order.PriceTypeId
            };
            var products = _orderService.GetOrderProductsModelByOrderId(order.Id, order.WarehouseId);

            return Success(new { order = orderInfo, products = products });
        }
        #endregion

        
        #region 其他
        private class MessageModel
        {
            public bool isSucc { get; set; }
            public string msg { get; set; }
        }
        /// <summary>
        /// 分页获取B2B销货数据分析
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetB2BSaleDataModel(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? customerId, int? wareHouseId, int? customerTypeId, int? orderType, int? checkType, int? bookKeepType, string search = "")
        {
            endTime = endTime?.AddDays(1);
            deliverEndTime = deliverEndTime?.AddDays(1);
            int count;
            decimal sumPrice;
            var data = _orderService.GetB2BSaleDataAnalysisModelByPage(pageIndex, pageSize, startTime, endTime, deliverStartTime, deliverEndTime, customerId, wareHouseId, customerTypeId, orderType, checkType, bookKeepType, out count, out sumPrice, search);
            return Json(new { isSucc = true, data = new { result = data, count = count, sumPrice = sumPrice }, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }
        public IActionResult ExportB2BSaleDataAnalysis(DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? customerId, int? wareHouseId, int? customerTypeId, int? orderType, int? checkType, int? bookKeepType, string search = "")
        {
            endTime = endTime?.AddDays(1);
            deliverEndTime = deliverEndTime?.AddDays(1);
            try
            {
                var data = _orderService.GetAllExportB2BSaleDataAnalysisModel(startTime, endTime, deliverStartTime, deliverEndTime, customerId, wareHouseId, customerTypeId, orderType, checkType, bookKeepType, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "单据类型","OrderTypeStr" },
                    { "单据编号","SerialNumber" },
                    { "下单时间","CreatedTime" },
                    { "收发时间","DeliveryDate" },
                    { "客户","CustomerName" },
                    { "商品编码","ProductCode" },
                    { "商品名称","ProductName" },
                    { "单价","UnitPrice"},
                    { "数量","Quantity" },
                    { "金额","Price" },
                    { "客户类型","CustomerTypeName"},
                    { "备注","Mark" },
                    { "仓库","WareHouseName" },
                    { "验收","IsCheckStr" },
                    { "记账","IsBookKeepingStr" },
                    { "业务员","SalesManName"},
                };

                DataTable table = data.ToDataTable(columnNames);


                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("合计--数量：{0} | 金额：{1} ", data.Sum(r => r.Quantity), data.Sum(r => r.Price)));
                sb.Append("\r\n");
                sb.Append("单据类型,单据编号,下单时间,收发时间,客户,商品编码,商品名称,单价,数量,金额,客户类型,备注,仓库,验收,记账,业务员");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["单据类型"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["单据编号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["下单时间"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["收发时间"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["客户"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["商品编码"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["商品名称"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["单价"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["金额"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["客户类型"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["备注"] + "\""+",");
                    sb.Append("\"" + "\t" + table.Rows[i]["仓库"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["验收"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["记账"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["业务员"] + "\"" + ",");

                    sb.Append("\r\n");
                }
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "B2B销货数据分析.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }

        #endregion
        

        #endregion

        #region 审核
        public IActionResult ApprovalProcess()
        {
            if (!_permissionService.Authorize("ViewApprovalProcess"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Users = _userService.GetAllUsers().ToList();
            var result = _orderService.GetAllApprovalProcessList();
            List<ApprovalProcessModel> list = new List<ApprovalProcessModel>();
            foreach (var item in result)
            {
                list.Add(item.ToModel());
            }
            ViewBag.List = list;
            return View();
        }
        [HttpPost]
        public IActionResult AddApprovalProcess(string name, string ids)
        {
            if (!_permissionService.Authorize("AddApprovalProcess"))
            {
                return Error("无操作权限！");
            }
            if (IsNullOrEmpty(name) || IsNullOrEmpty(ids))
                return Error();
            var isExist = _orderService.ConfirmApprovalProcessIsExistByName(name);
            if (isExist != null)
            {
                return Error("检查是否有同名流程！");
            }
            var userIds = ids.Split(",");
            var l = 1;
            List<ApprovalProcessDetail> list = new List<ApprovalProcessDetail>();
            foreach (var it in userIds)
            {
                var uId = Convert.ToInt32(it);
                ApprovalProcessDetail approvalProcessDetail = new ApprovalProcessDetail
                {
                    UserId = uId,
                    Sort = l,
                    CreatedBy = WorkContext.CurrentUser.Id,
                    ModifiedBy = WorkContext.CurrentUser.Id,
                    ModifiedTime = DateTime.Now,
                    User = _userService.GetById(uId)
                };
                list.Add(approvalProcessDetail);
                l++;
            }
            ApprovalProcess approvalProcess = new ApprovalProcess
            {
                Name = name,
                CreatedBy = WorkContext.CurrentUser.Id,
                ModifiedBy = WorkContext.CurrentUser.Id,
                ModifiedTime = DateTime.Now,
                ApprovalProcessDetail = list
            };
            _orderService.InsertApprovalProcess(approvalProcess);
            var data = approvalProcess.ToModel();
            return Success(data);
        }

        [HttpPost]
        public IActionResult UpdateAPDetailSort(int id, string sort)
        {
            _orderService.UpdateAPDetailSort(id, sort);
            return Success();
        }
        [HttpPost]
        public IActionResult DeleteApprovalProcess(int id)
        {
            if (!_permissionService.Authorize("DeleteApprovalProcess"))
            {
                return Error("无操作权限！");
            }
            _orderService.DeleteApprovalProcess(id);
            return Success();
        }
        #endregion

        #region 注释
        ///// <summary>
        ///// 订单退货（一键退货）
        ///// </summary>
        ///// <param name="orderId"></param>
        ///// <returns></returns>
        //public IActionResult SetRefundOrder(int orderId)
        //{
        //    //权限
        //    if (!_permissionService.Authorize("SetB2BRefundOrder"))
        //    {
        //        return Error("没有权限！");
        //    }
        //    var data = _orderService.GetOrderById(orderId);
        //    //订单已完成且订单中商品数不能超过原订单商品数
        //    if (data.State != OrderState.Finished)
        //    {
        //        return Error("订单状态非完成状态！");
        //    }
        //    else
        //    {
        //        var originalOrder = _omsAccessor.Get<Order>().Where(r => r.Isvalid && r.Type == OrderType.B2B_TH && r.OriginalOrderId == data.Id && r.OrgionSerialNumber == data.SerialNumber).FirstOrDefault();
        //        if (originalOrder != null)
        //            return Error("该订单已生成退单，不能重复生成！");

        //        #region order
        //        Order order = new Order();
        //        order.SerialNumber = _commonService.GetOrderSerialNumber("PT");
        //        order.Type = OrderType.B2B_TH;
        //        order.OrgionSerialNumber = data.SerialNumber;
        //        order.OriginalOrderId = data.Id;//原订单Id
        //        order.State = OrderState.ToBeConfirmed;
        //        order.PayType = data.PayType;
        //        order.PayMentType = data.PayMentType;
        //        order.PayState = data.PayState;
        //        order.TransDate = data.TransDate;
        //        order.SumPrice = data.SumPrice;
        //        order.PayPrice = data.PayPrice;
        //        order.PayDate = data.PayDate;
        //        order.DeliveryTypeId = data.DeliveryTypeId;
        //        order.CustomerName = data.CustomerName;
        //        order.CustomerPhone = data.CustomerPhone;
        //        order.AddressDetail = data.AddressDetail;
        //        order.CustomerMark = data.CustomerMark;
        //        order.AdminMark = data.AdminMark;
        //        order.ToWarehouseMessage = data.ToWarehouseMessage;
        //        order.WarehouseId = data.WarehouseId;
        //        order.PriceTypeId = data.PriceTypeId;
        //        order.CustomerId = data.CustomerId;
        //        order.ApprovalProcessId = data.ApprovalProcessId;
        //        #endregion
        //        _orderService.CreatedB2COrder(order);

        //        #region 订单商品
        //        var orderProducts = _orderService.GetOrderProductsByOrderId(data.Id);
        //        List<OrderProduct> orderProductModels = new List<OrderProduct>();

        //        foreach (var item in orderProducts)
        //        {
        //            var product = new OrderProduct
        //            {
        //                OrderId = order.Id,
        //                SaleProductId = item.SaleProductId,
        //                Quantity = item.Quantity,
        //                OrginPrice = item.OrginPrice,
        //                Price = item.Price,
        //                SumPrice = item.SumPrice,
        //                Type = item.Type,
        //            };
        //            orderProductModels.Add(product);
        //        }
        //        _omsAccessor.InsertRange<OrderProduct>(orderProductModels);
        //        _omsAccessor.SaveChanges();
        //        #endregion

        //        data.State = OrderState.Invalid;
        //        _orderService.UpdateOrder(data);
        //        //日志
        //        #region 日志（生成B2B退单）
        //        _logService.InsertOrderLog(orderId, "生成B2B退单", data.State, data.PayState, "生成B2B退单[" + order.SerialNumber + "],本订单设为无效");
        //        #endregion
        //        return Success();
        //    }
        //}
        #endregion

    }
}