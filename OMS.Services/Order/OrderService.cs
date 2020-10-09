using System;
using System.Collections.Generic;
using System.Text;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Interface;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OMS.Model;
using OMS.Services.Log;
using OMS.Core.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using OMS.Core.Tools;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using OMS.Model.Order;
using OMS.Model.DataStatistics;
using OMS.Model.B2B;
using OMS.Data.Domain.SalesMans;
using OMS.Model.JsonModel;
using OMS.Services.Common;
using OMS.Services.Products;
using NPOI.SS.Formula.Functions;
using Newtonsoft.Json;

namespace OMS.Services.Order1
{
    public class OrderService : ServiceBase, IOrderService
    {

        #region ctor   
        public ILogService _logService;
        public IProductService _productService;
        public ICommonService _commonService;
        public OrderService(IDbAccessor omsAccessor,
            IWorkContext workContext,
            IConfiguration configuration,
            ILogService logService,
            IProductService productService,
            ICommonService commonService) : base(omsAccessor, workContext, configuration)
        {
            this._logService = logService;
            _productService = productService;
            _commonService = commonService;
        }
        #endregion

        #region B2B订单
        /// <summary>
        /// 添加审核流程
        /// </summary>
        /// <param name="approvalProcess"></param>
        /// <returns></returns>
        public bool InsertApprovalProcess(ApprovalProcess approvalProcess)
        {
            try
            {
                _omsAccessor.Insert(approvalProcess);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 删除审核流程
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteApprovalProcess(int id)
        {
            try
            {
                var d = _omsAccessor.Get<ApprovalProcessDetail>().Where(p => p.ApprovalProcessId == id & p.Isvalid);
                _omsAccessor.DeleteRange(d);
                _omsAccessor.DeleteById<ApprovalProcess>(id);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 更新审核人员排序
        /// </summary>
        /// <param name="apdId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public bool UpdateAPDetailSort(int apdId, string userIds)
        {
            try
            {
                var sort = userIds.Split(',');
                if (sort.Length > 0)
                {
                    var list = _omsAccessor.Get<ApprovalProcessDetail>().Where(p => p.ApprovalProcessId == apdId & p.Isvalid);
                    for (int i = 0; i < sort.Length; i++)
                    {
                        var s = Convert.ToInt32(sort[i]);
                        foreach (var item in list)
                        {
                            if (s == item.UserId)
                            {
                                item.Sort = i + 1;
                                item.ModifiedTime = DateTime.Now;
                                item.ModifiedBy = _workContext.CurrentUser.Id;
                                _omsAccessor.Update(item);
                                break;
                            }
                        }
                    }
                }
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 获取审核流程
        /// </summary>
        /// <returns></returns>
        public List<ApprovalProcess> GetAllApprovalProcessList()
        {
            return _omsAccessor.Get<ApprovalProcess>()
                .Where(p => p.Isvalid).OrderBy(p => p.CreatedTime).Include(p => p.ApprovalProcessDetail).ThenInclude(p => p.User).ToList();
        }
        /// <summary>
        /// 通过流程名判断是否存在相同流程名
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public ApprovalProcess ConfirmApprovalProcessIsExistByName(string Name)
        {
            var result = _omsAccessor.Get<ApprovalProcess>().Where(a => a.Isvalid && a.Name == Name).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// 添加B2B订单
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns>新生成订单Id</returns>
        public Order AddB2BOrder(OrderModel orderModel)
        {
            try
            {
                #region 审核流程暂时不用
                //List<OrderApproval> orderApproval = new List<OrderApproval>();
                //var approvalProcess = _omsAccessor.Get<ApprovalProcess>().Where(p => p.Id == orderModel.ApprovalProcessId && p.Isvalid).Include(p => p.ApprovalProcessDetail).FirstOrDefault();
                //if (approvalProcess != null)
                //{
                //    foreach (var item in approvalProcess.ApprovalProcessDetail)
                //    {
                //        OrderApproval o = new OrderApproval
                //        {
                //            CreatedBy = _workContext.CurrentUser.Id,
                //            UserId = item.UserId,
                //            Sort = item.Sort,
                //            State = OrderApprovalState.Unaudited
                //        };
                //        orderApproval.Add(o);
                //    }
                //}
                #endregion
                Order order = new Order
                {
                    SerialNumber = orderModel.SerialNumber,
                    Type = OrderType.B2B,
                    ShopId = 0,
                    State = OrderState.ToBeTurned,
                    WriteBackState = WriteBackState.NoWrite,
                    IsLocked = false,
                    LockStock = false,
                    CustomerName = orderModel.CustomerName,
                    CustomerPhone = orderModel.CustomerPhone,
                    CustomerMark = orderModel.CustomerMark,
                    AddressDetail = orderModel.AddressDetail,
                    WarehouseId = orderModel.WarehouseId,
                    CustomerId = orderModel.CustomerId,
                    PriceTypeId = orderModel.PriceTypeId,
                    DeliveryTypeId = orderModel.DeliveryTypeId,
                    UserName = orderModel.UserName,
                    //OrderApproval = orderApproval,
                    ApprovalProcessId = orderModel.ApprovalProcessId,
                    CreatedBy = _workContext.CurrentUser.Id,
                    InvoiceType = orderModel.InvoiceType,
                    SalesManId = orderModel.SalesManId
                };
                _omsAccessor.Insert(order);
                _omsAccessor.SaveChanges();
                return order;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        /// <summary>
        /// 更新B2B订单
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        public bool UpdateB2BOrder(OrderModel orderModel)
        {
            try
            {
                //重新审核，如果已审核，解除库存锁定
                var order = Re_Review(orderModel.Id, orderModel.ApprovalProcessId);

                order.CustomerName = orderModel.CustomerName;
                order.CustomerPhone = orderModel.CustomerPhone;
                order.CustomerMark = orderModel.CustomerMark;
                order.AddressDetail = orderModel.AddressDetail;
                order.WarehouseId = orderModel.WarehouseId;
                order.CustomerId = orderModel.CustomerId;
                order.PriceTypeId = orderModel.PriceTypeId;
                order.DeliveryTypeId = orderModel.DeliveryTypeId;
                order.ApprovalProcessId = orderModel.ApprovalProcessId;
                order.UserName = orderModel.UserName;
                //order.InvoiceType = orderModel.InvoiceType;
                order.SalesManId = orderModel.SalesManId;
                order.ModifiedBy = _workContext.CurrentUser.Id;
                order.ModifiedTime = DateTime.Now;
                _omsAccessor.Update(order);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 修改订单，需要重新审核
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        public Order Re_Review(int orderId, int approvalProcessId = 0)
        {

            var order = _omsAccessor.Get<Order>().Where(p => p.Id == orderId && p.Isvalid).Include(p => p.OrderProduct).ThenInclude(p => p.SaleProduct).FirstOrDefault();
            if (order == null) { return new Order(); }

            #region 审核流程暂时不用
            //if (approvalProcessId == 0) { approvalProcessId = order.ApprovalProcessId; }
            //List<OrderApproval> orderApproval = _omsAccessor.Get<OrderApproval>().Where(p => p.OrderId == orderId && p.Isvalid && p.State != OrderApprovalState.Failure).ToList();
            //orderApproval.Each(p => p.State = OrderApprovalState.Failure);
            //var approvalProcess = _omsAccessor.Get<ApprovalProcess>().Where(p => p.Id == approvalProcessId && p.Isvalid).Include(p => p.ApprovalProcessDetail).FirstOrDefault();
            //if (approvalProcess != null)
            //{
            //    foreach (var item in approvalProcess.ApprovalProcessDetail)
            //    {
            //        OrderApproval o = new OrderApproval
            //        {
            //            CreatedBy = _workContext.CurrentUser.Id,
            //            UserId = item.UserId,
            //            Sort = item.Sort,
            //            State = OrderApprovalState.Unaudited
            //        };
            //        orderApproval.Add(o);
            //    }
            //}
            #endregion
            //order.State = OrderState.ToBeTurned;//修改订单，需要重新待审核（暂时保留当前状态）
            if (order.LockStock)
            {
                //如果锁定库存，需先释放库存
                //foreach (var item in order.OrderProduct)
                //{
                //    if (item.SaleProduct != null)
                //    {
                //        item.SaleProduct.LockStock = item.SaleProduct.LockStock - item.Quantity;
                //        item.SaleProduct.AvailableStock = item.SaleProduct.AvailableStock + item.Quantity;
                //    }
                //}
                //已解锁
                order.LockStock = false;
            }
            //order.OrderApproval = orderApproval;
            return order;
        }
        /// <summary>
        /// 添加订单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <returns></returns>
        public bool AddOrderProduct(OrderProduct orderProduct)
        {
            try
            {
                _omsAccessor.Insert(orderProduct);
                _omsAccessor.SaveChanges();
                //var order = Re_Review(orderProduct.OrderId);
                var order = _omsAccessor.GetById<Order>(orderProduct.OrderId);
                if (order != null && order.Type == OrderType.B2B)
                {
                    order.SumPrice = _omsAccessor.Get<OrderProduct>().Where(p => p.OrderId == orderProduct.OrderId && p.Isvalid).Sum(p => p.SumPrice);
                    _omsAccessor.Update(order);
                    _omsAccessor.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 更新订单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <returns></returns>
        public bool UpdateOrderProduct(OrderProduct orderProduct)
        {
            try
            {
                //var order = Re_Review(orderProduct.OrderId);
                var order = _omsAccessor.GetById<Order>(orderProduct.OrderId);
                //修改订单商品
                var data = order.OrderProduct.Where(s => s.Isvalid && s.Id == orderProduct.Id).FirstOrDefault();
                data.SaleProductId = orderProduct.SaleProductId;
                data.Quantity = orderProduct.Quantity;
                data.OrginPrice = orderProduct.OrginPrice;
                data.Price = orderProduct.Price;
                //data.SumPrice = orderProduct.Price * orderProduct.Quantity;
                data.SumPrice = orderProduct.SumPrice;
                _omsAccessor.Update<OrderProduct>(data);
                _omsAccessor.SaveChanges();

                if (order != null && order.Type == OrderType.B2B)
                {
                    order.SumPrice = _omsAccessor.Get<OrderProduct>().Where(p => p.OrderId == orderProduct.OrderId && p.Isvalid).Sum(p => p.SumPrice);
                    _omsAccessor.Update(order);
                    _omsAccessor.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 订单商品分页
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public PageList<Object> GetOrderProductByOrderId(int orderId, int pageIndex, int pageSize, string search = "")
        {

            //可显示缺货订单信息状态
            var canOrderStates = new OrderState[] {
                OrderState.B2CConfirmed,
                //OrderState.CheckAccept,
                OrderState.Confirmed,
                OrderState.FinancialConfirmation,
                OrderState.Paid,
                OrderState.ToBeConfirmed,
                OrderState.Unpaid,
                OrderState.ToBeTurned,
                OrderState.Unshipped
            };
            var result = from s in _omsAccessor.Get<OrderProduct>().Where(p => p.OrderId == orderId && p.Isvalid && (string.IsNullOrEmpty(search) || p.SaleProduct.Product.Name.Contains(search)))
                         join o in _omsAccessor.Get<Order>().Where(x => x.Isvalid) on s.OrderId equals o.Id into or
                         from ord in or.DefaultIfEmpty()
                         join p in _omsAccessor.Get<SaleProduct>() on s.SaleProductId equals p.Id into spl
                         from sp in spl.DefaultIfEmpty()
                         join pr in _omsAccessor.Get<Product>() on sp.ProductId equals pr.Id into sprl
                         from spr in sprl.DefaultIfEmpty()
                         select new
                         {
                             s.Id,
                             s.OrderId,
                             s.SaleProductId,
                             s.Quantity,
                             s.OrginPrice,
                             s.Price,
                             s.SumPrice,
                             sp.ProductId,
                             ProductName = spr.Name,
                             ProductCode = spr.Code,
                             TotalQuantity = sp != null ? sp.Stock : 0,
                             LockNum = _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.WareHouseId == ord.WarehouseId && x.OrderProductId == s.Id).FirstOrDefault() == null ? 0 : _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.WareHouseId == ord.WarehouseId && x.OrderProductId == s.Id).FirstOrDefault().LockNumber,
                             IsLackStock = !canOrderStates.Contains(ord.State)?false:(_omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.LockNumber == s.Quantity && x.WareHouseId == ord.WarehouseId && x.OrderProductId == s.Id).FirstOrDefault() == null ? true : false),
                             RefundQuantity = (from ofp in _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.SaleProductId == s.SaleProductId && x.OrginId == s.Id)
                                               join of in _omsAccessor.Get<Order>().Where(x => x.Isvalid && x.OriginalOrderId == ord.Id && new OrderType[] { OrderType.B2B_TH, OrderType.B2C_TH }.Contains(x.Type) && x.State != OrderState.Invalid)
                                               on ofp.OrderId equals of.Id
                                               select ofp).Sum(x => x.Quantity)
                         };
            return new PageList<Object>(result, pageIndex, pageSize);
        }
        /// <summary>
        /// 获取B2B退单列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="warehouseId"></param>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public PageList<Object> GetB2BRefundOrderList(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? warehouseId, string searchStr = "")
        {
            var orderStateStr = GetOrderStateStr();
            var data = from o in _omsAccessor.Get<Order>().Where(o => o.Isvalid && o.Type == OrderType.B2B_TH && (string.IsNullOrEmpty(searchStr) || o.SerialNumber.Contains(searchStr) || o.OrgionSerialNumber.Contains(searchStr))
             && (!warehouseId.HasValue || o.WarehouseId == warehouseId)
             && (!startTime.HasValue || o.CreatedTime >= startTime) && (!endTime.HasValue || o.CreatedTime <= endTime))
                       join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                       from ow in owl.DefaultIfEmpty()
                       select new
                       {
                           o.Id,
                           o.CreatedTime,
                           o.SerialNumber,
                           o.WarehouseId,
                           WarehouseName = ow.Name,
                           o.CustomerName,
                           o.CustomerPhone,
                           o.SumPrice,
                           o.PayPrice,
                           o.PayDate,
                           o.PSerialNumber,
                           o.OrgionSerialNumber,
                           o.PayType,
                           PayTypeName = (o.PayType == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == o.PayType).FirstOrDefault().Value),
                           o.PayState,
                           PayStateName = o.PayState.ToString() == "Success" ? "已付款" : "未付款",
                           o.State,
                           StateName = orderStateStr[o.State],
                           o.AdminMark,
                           ProductQuantities = (from q in _omsAccessor.Get<OrderProduct>().Where(op => op.OrderId == o.Id) select q.Quantity).Sum()
                       };
            return new PageList<Object>(data.OrderByDescending(r => r.CreatedTime), pageIndex, pageSize);
        }
        /// <summary>
        /// 获取该订单下所有商品
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IEnumerable<OrderProduct> GetOrderProductsByOrderId(int orderId)
        {
            var result = _omsAccessor.Get<OrderProduct>(o => o.Isvalid && o.OrderId == orderId).Include(o => o.SaleProduct).Include(o => o.SaleProduct.Product).Include(o => o.SaleProduct.SaleProductPrice);
            return result;
        }
        /// <summary>
        /// 获取订单详情页商品Model
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<OrderDetailProductsModel> GetOrderProductsModelByOrderId(int orderId,int wareHouseId)
        {

            //可显示缺货订单信息状态
            var canOrderStates = new OrderState[] {
                OrderState.B2CConfirmed,
                //OrderState.CheckAccept,
                OrderState.Confirmed,
                OrderState.FinancialConfirmation,
                OrderState.Paid,
                OrderState.ToBeConfirmed,
                OrderState.Unpaid,
                OrderState.ToBeTurned,
                OrderState.Unshipped
            };
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>().Where(x => x.Isvalid) on op.OrderId equals o.Id into or
                         from ord in or.DefaultIfEmpty()
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         where op.Isvalid && op.OrderId == orderId && osp.Isvalid && ospp.Isvalid
                         select new OrderDetailProductsModel
                         {
                             Id = op.Id,
                             ProductId = op.SaleProduct.ProductId,
                             ProductName = ospp.Name,
                             ProductCode = ospp.Code,
                             Quantity = op.Quantity,
                             TotalQuantity = (osp == null || ospp == null) ? 0 : osp.Stock,
                             Price = op.Price,
                             OrginPrice = op.OrginPrice,
                             SumPrice = op.SumPrice,
                             Type = op.Type,
                             SaleProductId = osp.Id,
                             LockNum = _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == wareHouseId && x.OrderProductId == op.Id && x.SaleProductId==osp.Id).FirstOrDefault() == null ? 0 : _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == wareHouseId && x.OrderProductId == op.Id && x.SaleProductId == osp.Id).FirstOrDefault().LockNumber,
                             IsLackStock = !canOrderStates.Contains(ord.State) ? false : (_omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.LockNumber == op.Quantity && x.WareHouseId == wareHouseId && x.OrderProductId == op.Id && x.SaleProductId == osp.Id).FirstOrDefault() == null ? true : false),
                             RefundQuantity = (from ofp in _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.SaleProductId == op.SaleProductId && x.OrginId == op.Id)
                                               join of in _omsAccessor.Get<Order>().Where(x => x.Isvalid && x.OriginalOrderId == ord.Id && new OrderType[] {OrderType.B2B_TH,OrderType.B2C_TH}.Contains(x.Type) && x.State != OrderState.Invalid)
                                               on ofp.OrderId equals of.Id
                                               select ofp).Sum(x => x.Quantity)
                         };
            return result.ToList();
        }
        /// <summary>
        /// 获取可以退货的订单商品信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<CanRefundOrderProduct> GetCanRefundOrderProductsByOrderId(int? orderId) {
            var canRefundOrderProductList = new List<CanRefundOrderProduct>();
            var orderProducts = _omsAccessor.Get<OrderProduct>().Where(x=>x.Isvalid && x.OrderId == orderId);
            foreach (var item in orderProducts)
            {
                var refundQuantity = (from ofp in _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.SaleProductId == item.SaleProductId && x.OrginId == item.Id)
                                      join of in _omsAccessor.Get<Order>().Where(x => x.Isvalid && x.OriginalOrderId == orderId && new OrderType[] { OrderType.B2B_TH, OrderType.B2C_TH }.Contains(x.Type) && x.State != OrderState.Invalid)
                                      on ofp.OrderId equals of.Id
                                      select ofp).Sum(x => x.Quantity);
                if (item.Quantity <= refundQuantity) continue;
                var product = (from sp in _omsAccessor.Get<SaleProduct>().Where(x => x.Isvalid && x.Id == item.SaleProductId)
                               join p in _omsAccessor.Get<Product>().Where(x => x.Isvalid)
                               on sp.ProductId equals p.Id select p).FirstOrDefault();
                if (product == null) continue;
                var canRefundOrderProduct = new CanRefundOrderProduct()
                {
                    CanRedundQuantity = item.Quantity - refundQuantity,
                    Price = item.Price,
                    OrderProductId = item.Id,
                    SaleProductId = item.SaleProductId,
                    ProductCode = product.Code,
                    ProductName =product.Name
                };
                canRefundOrderProductList.Add(canRefundOrderProduct);
            }

            return canRefundOrderProductList;
        }
        /// <summary>
        /// 订单分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PageList<OrderModel> GetOrderListByType(OrderModel orderMode, int pageIndex, int pageSize)
        {
            //可显示缺货订单信息状态
            var canOrderStates = new OrderState[] {
                OrderState.B2CConfirmed,
                //OrderState.CheckAccept,
                OrderState.Confirmed,
                OrderState.FinancialConfirmation,
                OrderState.Paid,
                OrderState.ToBeConfirmed,
                OrderState.Unpaid,
                OrderState.ToBeTurned,
                OrderState.Unshipped
            };

            orderMode.EndTime = orderMode.EndTime?.AddDays(1);
            orderMode.DeliverEndTime = orderMode.DeliverEndTime?.AddDays(1);
            List<OrderModel> list = new List<OrderModel>();
            var result = (from o in _omsAccessor.Get<Order>().Where(p => p.Isvalid
           && p.Type == orderMode.Type
           && (string.IsNullOrEmpty(orderMode.SerialNumber) 
           || p.SerialNumber.Contains(orderMode.SerialNumber) 
           || p.CustomerName.Contains(orderMode.SerialNumber)
           || p.CustomerPhone.Contains(orderMode.SerialNumber) 
           || p.AdminMark.Contains(orderMode.SerialNumber) 
           || p.AddressDetail.Contains(orderMode.SerialNumber)
           || p.CustomerMark.Contains(orderMode.SerialNumber)
           || p.FinanceMark.Contains(orderMode.SerialNumber))
           && (!orderMode.StartTime.HasValue || p.CreatedTime > orderMode.StartTime.Value)
           && (!orderMode.EndTime.HasValue || p.CreatedTime < orderMode.EndTime.Value)
           && (!orderMode.DeliverStartTime.HasValue || p.DeliveryDate > orderMode.DeliverStartTime.Value)
           && (!orderMode.DeliverEndTime.HasValue || p.DeliveryDate < orderMode.DeliverEndTime.Value)
           && (orderMode.CustomerId == 0 || p.CustomerId == orderMode.CustomerId)
           && (!orderMode.SalesManId.HasValue || p.SalesManId == orderMode.SalesManId)
           && (!orderMode.OrderState.HasValue || p.State == (OrderState)orderMode.OrderState)
           && (!orderMode.MinPrice.HasValue || p.SumPrice >= orderMode.MinPrice)
           && (!orderMode.MaxPrice.HasValue || p.SumPrice <= orderMode.MaxPrice)
           && (!orderMode.WareHouseId.HasValue || p.WarehouseId == orderMode.WareHouseId)
            ).Include(p => p.OrderProduct)
                          join c in _omsAccessor.Get<Customers>().Where(x => x.Isvalid).Include(x => x.Dictionary) on o.CustomerId equals c.Id into ocl
                          from oc in ocl.DefaultIfEmpty()
                          join sa in _omsAccessor.Get<SalesMan>().Where(x => x.Isvalid) on o.SalesManId equals sa.Id into sale
                          from sal in sale.DefaultIfEmpty()
                          orderby o.CreatedTime descending
                          select new OrderModel
                          {
                              Id = o.Id,
                              SerialNumber = o.SerialNumber,
                              CreatedTime = o.CreatedTime,
                              Company = oc != null ? oc.Name : "",
                              CompanyType = oc != null ? oc.Dictionary.Value : "",
                              OrderProductCount = o.OrderProduct.Where(p => p.Isvalid).Sum(p => p.Quantity),
                              SumPrice = o.SumPrice,
                              State = o.State,
                              StateStr = o.State.Description(),
                              FinanceMark = string.IsNullOrEmpty(o.FinanceMark) ? "" : o.FinanceMark,
                              SalesManName = sal != null ? sal.UserName : "",
                              PayPrice = o.PayPrice,
                              PayDate = o.PayDate,
                              CustomerTypeId = oc != null ? oc.CustomerTypeId : 0,
                              IsCheck = (o.State == OrderState.CheckAccept || o.State == OrderState.Bookkeeping || o.State == OrderState.Finished),
                              IsBookKeepingStr = GetBookKeepingStr(o),
                              DeliveryDate = o.DeliveryDate,
                              DeliveryNumber = o.DeliveryNumber == null ? "" : o.DeliveryNumber,
                              IsInvoice = o.InvoiceType == InvoiceType.NoNeedInvoice ? false : true,
                              IsLackStock = !canOrderStates.Contains(o.State) ? false : ((from op in _omsAccessor.Get<OrderProduct>().Where(x => x.OrderId == o.Id && x.Isvalid)
                                                    join spl in _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == o.WarehouseId) on op.Id equals spl.OrderProductId
                                                    where op.Quantity == spl.LockNumber
                                                   select op).Count()!=_omsAccessor.Get<OrderProduct>().Where(x=>x.Isvalid && x.OrderId == o.Id).Count() ? true : false),
                          }).ToList();

            if (orderMode.CustomerTypeId.HasValue)
            {
                result = result.Where(r => r.CustomerTypeId == orderMode.CustomerTypeId).ToList();
            }
            if (orderMode.BookKeepType.HasValue)
            {
                if (orderMode.BookKeepType.Value == 1)
                    result = result.Where(r => r.IsBookKeepingStr == "已付款").ToList();
                else if (orderMode.BookKeepType.Value == 2)
                    result = result.Where(r => r.IsBookKeepingStr == "部分付款").ToList();
                else if (orderMode.BookKeepType.Value == 3)
                    result = result.Where(r => r.IsBookKeepingStr == "未付款").ToList();
            }

            return new PageList<OrderModel>(result, pageIndex, pageSize);
        }
        public Order GetOrderById(int orderId)
        {
            var result = _omsAccessor.Get<Order>().Where(p => p.Id == orderId && p.Isvalid).Include(p => p.OrderApproval).Include(p => p.InvoiceInfo).Include(p=>p.OrderProduct).Include(p=>p.Delivery).FirstOrDefault();

            return result;
        }
        /// <summary>
        /// 获取订单（全状态，包括isvalid=false）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Order GetOrderByIdWithAllState(int orderId)
        {
            var result = _omsAccessor.Get<Order>().Where(s => s.Id == orderId).FirstOrDefault();
            return result;

        }
        public Object GetOrderProductById(int id)
        {
            //return _omsAccessor.Get<OrderProduct>().Where(p => p.Id == id).Include(x => x.SaleProduct).ThenInclude(x => x.Product).FirstOrDefault();
            var data = from s in _omsAccessor.Get<OrderProduct>().Where(p => p.Id == id && p.Isvalid)
                       join p in _omsAccessor.Get<SaleProduct>() on s.SaleProductId equals p.Id
                       join pr in _omsAccessor.Get<Product>() on p.ProductId equals pr.Id
                       select new
                       {
                           s.Id,
                           s.OrderId,
                           s.SaleProductId,
                           s.Quantity,
                           s.OrginPrice,
                           s.Price,
                           s.SumPrice,
                           p.ProductId,
                           p.Stock,
                           p.LockStock,
                           p.AvailableStock,

                           ProductName = pr.Name,
                           ProductCode = pr.Code
                       };
            //return _omsAccessor.Get<OrderProduct>().Where(o => o.Id == id).FirstOrDefault();
            return data;
        }
        public OrderProduct GetOrderProductByIdOne(int id)
        {
            return _omsAccessor.Get<OrderProduct>().Where(r => r.Isvalid && r.Id == id).FirstOrDefault();
        }
        public bool DeleteOrderProductById(int id)
        {
            try
            {
                _omsAccessor.DeleteById<OrderProduct>(id);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public int CheckOrderProductCount(int orderId, int productId)
        {
            //B2B订单一个订单商品只能有一条记录
            return _omsAccessor.Get<OrderProduct>().Where(p => p.OrderId == orderId && p.SaleProduct.ProductId == productId && p.Isvalid).Count();
        }
        /// <summary>
        /// 审核订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="state"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool ApprovalOrder(int orderId, bool state, out string msg)
        {
            var list = _omsAccessor.Get<OrderApproval>().Where(p => p.OrderId == orderId && p.State == OrderApprovalState.Unaudited).OrderBy(p => p.Sort);
            var orderApproval = list.FirstOrDefault();
            if (orderApproval == null)
            {
                msg = "当前订单不需要审核";
                return false;
            }
            else
            {
                if (orderApproval.UserId == _workContext.CurrentUser.Id)
                {
                    if (state)
                    {
                        //通过
                        orderApproval.State = OrderApprovalState.Audited;
                        var order = _omsAccessor.GetById<Order>(orderId);
                        if (list.Count() >= 1 && !order.LockStock)
                        {
                            order.State = OrderState.Confirmed;
                            order.LockStock = true;
                            //进入财务确认
                            _omsAccessor.Update(order);
                            var orderProduct = _omsAccessor.Get<OrderProduct>().Where(p => p.OrderId == orderId && p.Isvalid).Include(p => p.SaleProduct);
                            foreach (var item in orderProduct)
                            {
                                if (item.SaleProduct != null)
                                {
                                    item.SaleProduct.LockStock = item.SaleProduct.LockStock + item.Quantity;
                                    item.SaleProduct.AvailableStock = item.SaleProduct.Stock - item.SaleProduct.LockStock;
                                }
                                //锁定库存
                                _omsAccessor.Update(item.SaleProduct);
                            }
                        }
                        _omsAccessor.Update(orderApproval);
                        _omsAccessor.SaveChanges();
                        msg = "订单审核成功";
                        return true;
                    }
                    else
                    {
                        //退回
                        var order = _omsAccessor.GetById<Order>(orderId);
                        order.State = OrderState.returned;
                        _omsAccessor.Update(order);
                        _omsAccessor.SaveChanges();
                        msg = "订单退回成功";
                        return true;
                    }

                }
                else
                {
                    msg = "订单当前审核人不是该账号！";
                    return false;
                }
            }
        }
        /// <summary>
        /// 财务确认订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool ConfirmOrder(int orderId, out string msg)
        {
            var order = _omsAccessor.GetById<Order>(orderId);
            if (order.State == OrderState.Confirmed)
            {
                order.State = OrderState.FinancialConfirmation;
                _omsAccessor.Update(order);
                _omsAccessor.SaveChanges();
                msg = "订单确认成功";
                return true;
            }
            else
            {
                msg = "该订单不需要确认";
                return false;
            }
        }
        /// <summary>
        /// 财务记账
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool BookKeeping(OrderModel orderModel, out string msg)
        {
            var order = _omsAccessor.Get<Order>().Where(p => p.Id == orderModel.Id).Include(p => p.OrderPayPrice).FirstOrDefault();
            if (order == null)
            {
                msg = "订单信息错误，请核实！";
                return false;
            }
            #region 原方法
            //if (order.PayPrice == 0)
            //{
            //    if (!orderModel.IsPayOrRefund)
            //    {
            //        msg = "当前订单未支付，不支持退款！";
            //        return false;
            //    }
            //    else
            //    {
            //        if (orderModel.PayPrice > order.SumPrice)
            //        {
            //            msg = "支付金额大于订单总金额，操作失败！";
            //            return false;
            //        }
            //    }

            //    order.PayType = orderModel.PayType;
            //    order.PayMentType = orderModel.PayMentType;
            //    order.PayPrice = orderModel.PayPrice;
            //    order.State = OrderState.Bookkeeping;
            //    OrderPayPrice orderPayPrice = new OrderPayPrice
            //    {
            //        OrderId = order.Id,
            //        IsPay = orderModel.IsPayOrRefund,
            //        PayType = orderModel.PayType,
            //        PayMentType = orderModel.PayMentType,
            //        Price = orderModel.PayPrice,
            //        Mark = orderModel.AdminMark,
            //    };
            //    order.OrderPayPrice.Add(orderPayPrice);
            //}
            //else
            //{
            //    if (orderModel.IsPayOrRefund)
            //    {//付款
            //        if (orderModel.PayPrice + order.PayPrice > order.SumPrice)
            //        {
            //            msg = "支付金额大于订单总金额，操作失败！";
            //            return false;
            //        }
            //        order.PayPrice += orderModel.PayPrice;
            //    }
            //    else
            //    {
            //        //退款
            //        if (orderModel.PayPrice > order.PayPrice)
            //        {
            //            msg = "退款金额大于已支付金额，操作失败";
            //            return false;
            //        }
            //        order.PayPrice -= orderModel.PayPrice;
            //    }
            //    OrderPayPrice orderPayPrice = new OrderPayPrice
            //    {
            //        OrderId = order.Id,
            //        IsPay = orderModel.IsPayOrRefund,
            //        PayType = orderModel.PayType,
            //        PayMentType = orderModel.PayMentType,
            //        Price = orderModel.PayPrice,
            //        Mark = orderModel.AdminMark,
            //    };
            //    order.OrderPayPrice.Add(orderPayPrice);
            //}
            #endregion

            if (orderModel.IsPayOrRefund)
            {//付款
                if (orderModel.PayPrice + order.PayPrice > order.SumPrice)
                {
                    msg = "支付金额大于订单总金额，操作失败！";
                    return false;
                }
            }
            else
            {
                //退款
                if (orderModel.PayPrice + order.PayPrice > order.SumPrice)
                {
                    msg = "退款金额大于已支付金额，操作失败";
                    return false;
                }
            }
            order.PayPrice += orderModel.PayPrice;
            order.PayDate = DateTime.Now;
            order.PayState = PayState.Success;

            OrderPayPrice orderPayPrice = new OrderPayPrice
            {
                OrderId = order.Id,
                IsPay = orderModel.IsPayOrRefund,
                PayType = orderModel.PayType,
                PayMentType = orderModel.PayMentType,
                Price = orderModel.PayPrice,
                Mark = orderModel.AdminMark,
            };
            order.OrderPayPrice.Add(orderPayPrice);

            _omsAccessor.Update(order);
            _omsAccessor.SaveChanges();
            msg = "操作成功";
            return true;
        }
        /// <summary>
        /// 提交审核
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SubmitApproval(int orderId, out string msg)
        {
            var order = _omsAccessor.Get<Order>().Where(p => p.Id == orderId && p.Isvalid).Include(p => p.InvoiceInfo).Include(p => p.OrderProduct).FirstOrDefault();
            if (order == null)
            {
                msg = "提交审核失败，订单错误！";
                return false;
            }
            if (order.State == OrderState.ToBeTurned)
            {
                if (order.InvoiceType != InvoiceType.NoNeedInvoice && order.InvoiceInfo == null)
                {

                    msg = "提交审核失败，请先填写发票信息！";
                    return false;
                }
                if (order.OrderProduct == null || order.OrderProduct.Count() == 0)
                {
                    msg = "提交审核失败，请先选择商品！";
                    return false;
                }

                order.State = OrderState.ToBeConfirmed;
                _omsAccessor.Update(order);
                _omsAccessor.SaveChanges();
                msg = "提交审核成功";
                return true;
            }
            else
            {
                msg = "该订单不能提交审核";
                return false;
            }
        }
        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool DeleteOrder(int orderId, out string msg)
        {
            var order = _omsAccessor.GetById<Order>(orderId);
            if (order == null)
            {
                msg = "删除失败";
                return false;
            }
            if (_workContext.CurrentUser.Id != order.CreatedBy)
            {
                msg = "当前账号没有权限删除该订单，只能由创建者删除！";
                return false;
            }
            order.Isvalid = false;
            _omsAccessor.Update(order);
            _omsAccessor.SaveChanges();
            msg = "删除成功";
            return true;
        }
        /// <summary>
        /// 根据订单获取客户信息，默认发票信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Customers GetDefaultInvoiceInfo(int orderId)
        {
            var order = _omsAccessor.GetById<Order>(orderId);
            if (order == null) { return new Customers(); }
            return _omsAccessor.GetById<Customers>(order.CustomerId);
        }
        /// <summary>
        /// 插入订单发票
        /// </summary>
        /// <param name="invoiceInfo"></param>
        /// <returns></returns>
        public bool SubmitOrderInvoiceInfo(InvoiceInfoModel invoiceInfoModel, int orderId)
        {
            try
            {
                var invoice = _omsAccessor.Get<InvoiceInfo>().Where(r => r.Isvalid && r.OrderId == orderId).FirstOrDefault();
                if (invoice != null)
                {
                    invoice.CustomerEmail = invoiceInfoModel.CustomerEmail;
                    invoice.Title = invoiceInfoModel.Title;
                    invoice.TaxpayerID = invoiceInfoModel.TaxpayerID;
                    invoice.RegisterAddress = invoiceInfoModel.RegisterAddress;
                    invoice.RegisterTel = invoiceInfoModel.RegisterTel;
                    invoice.BankAccount = invoiceInfoModel.BankAccount;
                    invoice.BankCode = invoiceInfoModel.BankCode;
                    invoice.BankOfDeposit = invoiceInfoModel.BankOfDeposit;
                    invoice.InvoiceNo = invoiceInfoModel.InvoiceNo;
                    invoice.CreatedBy = _workContext.CurrentUser.Id;
                    _omsAccessor.Update<InvoiceInfo>(invoice);
                }
                else
                {
                    InvoiceInfo invoiceInfo = new InvoiceInfo
                    {
                        OrderId = orderId,
                        CustomerEmail = invoiceInfoModel.CustomerEmail,
                        Title = invoiceInfoModel.Title,
                        TaxpayerID = invoiceInfoModel.TaxpayerID,
                        RegisterAddress = invoiceInfoModel.RegisterAddress,
                        RegisterTel = invoiceInfoModel.RegisterTel,
                        BankAccount = invoiceInfoModel.BankAccount,
                        BankCode = invoiceInfoModel.BankCode,
                        BankOfDeposit = invoiceInfoModel.BankOfDeposit,
                        InvoiceNo = invoiceInfoModel.InvoiceNo,
                        CreatedBy = _workContext.CurrentUser.Id
                    };

                    _omsAccessor.Insert(invoiceInfo);
                }
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        /// <summary>
        /// 更新发票信息
        /// </summary>
        /// <param name="invoiceInfo"></param>
        public void UpdateOrderInvoiceInfo(InvoiceInfo invoiceInfo)
        {
            invoiceInfo.ModifiedBy = _workContext.CurrentUser.Id;
            invoiceInfo.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<InvoiceInfo>(invoiceInfo);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 复制B2B订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string CopyB2BOrder(int orderId)
        {
            using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    //订单主体
                    var order = _omsAccessor.Get<Order>().Where(r => r.Id == orderId).FirstOrDefault();
                    var newOrderSerialNum = CheckOrderSerialNum("PF");
                    var osStr = "";
                    if(order.OrgionSerialNumber == null || order.OrgionSerialNumber == "")
                    {
                        osStr = order.SerialNumber + "(C)";
                    }
                    else
                    {
                       osStr = order.OrgionSerialNumber + "'" + order.SerialNumber + "(C)";
                    }
                    #region newOrder
                    Order newOrder = new Order();
                    newOrder.SerialNumber = newOrderSerialNum;
                    newOrder.Type = order.Type;
                    newOrder.ShopId = order.ShopId;
                    newOrder.PSerialNumber = order.PSerialNumber;
                    newOrder.OrgionSerialNumber = osStr;
                    newOrder.State = OrderState.Paid;
                    newOrder.WriteBackState = 0;
                    newOrder.PayType = order.PayType;
                    newOrder.PayMentType = order.PayMentType;
                    newOrder.IsLocked = false;
                    newOrder.LockMan = 0;
                    newOrder.LockStock = false;
                    newOrder.SumPrice = order.SumPrice;
                    newOrder.PayState = PayState.Success;
                    newOrder.PayPrice = 0;//设置已付金额为0
                    newOrder.DeliveryTypeId = order.DeliveryTypeId;
                    newOrder.CustomerName = order.CustomerName;
                    newOrder.CustomerPhone = order.CustomerPhone;
                    newOrder.AddressDetail = order.AddressDetail;
                    newOrder.DistrictId = order.DistrictId;
                    newOrder.CustomerMark = order.CustomerMark;
                    newOrder.InvoiceType = order.InvoiceType;
                    newOrder.AdminMark = order.AdminMark;
                    newOrder.ToWarehouseMessage = order.ToWarehouseMessage;
                    newOrder.WarehouseId = order.WarehouseId == 0 ? _omsAccessor.Get<WareHouse>().FirstOrDefault().Id : order.WarehouseId;
                    newOrder.PriceTypeId = order.PriceTypeId;
                    newOrder.CustomerId = order.CustomerId;
                    newOrder.ApprovalProcessId = order.ApprovalProcessId;
                    newOrder.Isvalid = true;
                    newOrder.CreatedBy = _workContext.CurrentUser.Id;
                    newOrder.InvoiceMode = order.InvoiceMode;
                    newOrder.ProductCoupon = order.ProductCoupon;
                    newOrder.ZMIntegralValuePrice = order.ZMIntegralValuePrice;
                    newOrder.ZMCoupon = order.ZMCoupon;
                    newOrder.ZMWineCoupon = order.ZMWineCoupon;
                    newOrder.WineWorldCoupon = order.WineWorldCoupon;
                    newOrder.UserName = order.UserName;
                    newOrder.IsNeedPaperBag = order.IsNeedPaperBag;
                    newOrder.OriginalOrderId = order.OriginalOrderId;
                    newOrder.SalesManId = order.SalesManId;
                    newOrder.FinanceMark = order.FinanceMark;
                    #endregion
                    order.IsCopied = true;
                    _omsAccessor.Insert<Order>(newOrder);
                    _omsAccessor.Update<Order>(order);
                    _omsAccessor.SaveChanges();


                    //发票信息
                    var oldInvoiceInfo = _omsAccessor.Get<InvoiceInfo>().Where(r => r.OrderId == orderId).FirstOrDefault();
                    if (oldInvoiceInfo != null)
                    {
                        InvoiceInfo invoiceInfo = new InvoiceInfo
                        {
                            OrderId = newOrder.Id,
                            CustomerEmail = oldInvoiceInfo.CustomerEmail,
                            Title = oldInvoiceInfo.Title,
                            TaxpayerID = oldInvoiceInfo.TaxpayerID,
                            RegisterAddress = oldInvoiceInfo.RegisterAddress,
                            RegisterTel = oldInvoiceInfo.RegisterTel,
                            BankAccount = oldInvoiceInfo.BankAccount,
                            BankCode = oldInvoiceInfo.BankCode,
                            BankOfDeposit = oldInvoiceInfo.BankOfDeposit,
                            InvoiceNo = oldInvoiceInfo.InvoiceNo,
                            CreatedBy = _workContext.CurrentUser.Id
                        };
                        _omsAccessor.Insert<InvoiceInfo>(invoiceInfo);
                    }


                    //支付信息
                    if (order.PayPrice != 0)
                    {
                        var oldPayInfo = _omsAccessor.Get<OrderPayPrice>().Where(r => r.OrderId == order.Id).ToList();
                        if (oldPayInfo.Count() != 0)
                        {
                            newOrder.PayPrice = order.PayPrice;
                            foreach (var item in oldPayInfo)
                            {
                                OrderPayPrice newOrderPayPrice = new OrderPayPrice();
                                newOrderPayPrice.OrderId = newOrder.Id;
                                newOrderPayPrice.IsPay = true;
                                newOrderPayPrice.PayType = item.PayType;
                                newOrderPayPrice.PayMentType = item.PayMentType;
                                newOrderPayPrice.Price = item.Price;
                                newOrderPayPrice.Mark = item.Mark;
                                _omsAccessor.Insert<OrderPayPrice>(newOrderPayPrice);
                            }
                            _omsAccessor.Update<Order>(newOrder);
                            _omsAccessor.SaveChanges();
                        }
                    }


                    //订单商品
                    var oldOrderPros = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == orderId).ToList();
                    foreach(var item in oldOrderPros)
                    {
                        OrderProduct newOrderPro = new OrderProduct();
                        newOrderPro.OrderId = newOrder.Id;
                        newOrderPro.SaleProductId = item.SaleProductId;
                        newOrderPro.Quantity = item.Quantity;
                        newOrderPro.OrginPrice = item.OrginPrice;
                        newOrderPro.Price = item.Price;
                        newOrderPro.SumPrice = item.SumPrice;
                        _omsAccessor.Insert<OrderProduct>(newOrderPro);


                        //锁定库存(先锁定SaleProduct的库存，再锁定SaleProductWareHouseStock库存)
                        var salePro = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == newOrderPro.SaleProductId).FirstOrDefault();
                        salePro.LockStock += newOrderPro.Quantity;
                        salePro.AvailableStock = salePro.Stock - salePro.LockStock;
                        if (salePro.AvailableStock < 0)
                        {
                            trans.Rollback();
                            return "转单后商品可用库存不足，无法复制生新订单！";
                        }
                        _omsAccessor.Update<SaleProduct>(salePro);
                        _omsAccessor.SaveChanges();

                        //库存跟踪及锁定
                        var result = _productService.CreateSaleProductLockedTrackAndWareHouseStock(newOrder.Id, item.SaleProductId, newOrder.WarehouseId, item.Quantity, null);
                        if (!result)
                        {
                            #region 订单日志(更新锁定信息失败)
                            _logService.InsertOrderLog(newOrder.Id, "更新锁定信息失败", newOrder.State, newOrder.PayState, "更新锁定库存跟踪及仓库OMS库存锁定失败!");
                            #endregion
                        }
                    }
                    _omsAccessor.SaveChanges();


                    trans.Commit();
                    #region 日志
                    _logService.InsertOrderLog(newOrder.Id, "复制订单", OrderState.Paid, newOrder.PayState, "由" + order.SerialNumber + "复制生成订单");
                    _logService.InsertOrderLog(order.Id, "复制订单", order.State, order.PayState, "复制生成订单" + newOrder.SerialNumber);
                    _logService.InsertOrderLog(newOrder.Id, "添加支付信息", newOrder.State, newOrder.PayState, "复制订单添加付款信息，付款金额:" + newOrder.PayPrice);
                    #endregion
                    return "复制订单成功！";
                }catch(Exception e)
                {
                    trans.Rollback();
                    return "复制订单出错,位置:" + e.StackTrace;
                }
            }
        }
        /// <summary>
        /// 接口调用获取B2B订单及详情
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InterfaceOrderModel> GetAllB2BOrder(int orderType=(int)OrderType.B2B,string orderSerialNumber="",int orderState=0, DateTime? startTime = null, DateTime? endTime = null)
        {
            var dic = _omsAccessor.Get<Dictionary>().ToList();
            var wh = _omsAccessor.Get<WareHouse>().ToList();
            var orderStateStr = GetOrderStateStr();
            var data = _omsAccessor.Get<Order>().Where(r => (int)r.Type == orderType
            && !string.IsNullOrEmpty(r.UserName)
            && !string.IsNullOrEmpty((r.DeliveryDate).ToString())
            && (string.IsNullOrEmpty(orderSerialNumber) || r.SerialNumber == orderSerialNumber.Trim())
            && (orderState == 0 || (int)r.State == orderState)
            && (!startTime.HasValue || r.ModifiedTime >= startTime)
            && (!endTime.HasValue || r.ModifiedTime <= endTime)
            )
               .Select(r => new InterfaceOrderModel()
               {
                   OrderId = r.Id,
                   SerialNumber = r.SerialNumber,
                   PSerialNumber = r.PSerialNumber ?? "",
                   OrgionSerialNumber = r.OrgionSerialNumber ?? "",
                   State = (int)r.State,
                   StateName = orderStateStr[r.State],
                   PriceTypeId = r.PriceTypeId,
                   PayType = r.PayType,
                   PayTypeName = dic.Where(d => d.Id == r.PayType).FirstOrDefault().Value ?? "",
                   PayMentType = r.PayMentType,
                   PayMentTypeName = dic.Where(d => d.Id == r.PayMentType).FirstOrDefault().Value ?? "",
                   PayState = r.PayState,
                   SumPrice = r.SumPrice,
                   PayPrice = r.PayPrice,
                   DeliveryTypeId = r.DeliveryTypeId,
                   DeliveryNumber = r.DeliveryNumber ?? "",
                   CustomerId = r.CustomerId,
                   CustomerIdName = _omsAccessor.Get<Customers>().Where(c => c.Id == r.CustomerId).FirstOrDefault().Name,
                   CustomerName = r.CustomerName,
                   CustomerPhone = r.CustomerPhone ?? "",
                   CustomerAddressDetail = r.AddressDetail ?? "",
                   CustomerMark = r.CustomerMark ?? "",
                   AdminMark = r.AdminMark ?? "",
                   ToWarehouseMessage = r.ToWarehouseMessage ?? "",
                   WarehouseId = r.WarehouseId,
                   WarehouseName = wh.Where(w => w.Id == r.WarehouseId).FirstOrDefault().Name,
                   UserName = r.UserName ?? "",
                   InvoiceType = r.InvoiceType,
                   IsNeedPaperBag = r.IsNeedPaperBag,
                   SalesManId = r.SalesManId ?? 0,
                   FinanceMark = r.FinanceMark ?? "",
                   PayDate = r.PayDate.ToString() ?? "",
                   CreatedTime = r.CreatedTime,
                   InvoiceInfo = _omsAccessor.Get<InvoiceInfo>().Where(ii => ii.OrderId == r.Id).OrderByDescending(ii => ii.CreatedTime).FirstOrDefault(),
                   OrderPayPrice = _omsAccessor.Get<OrderPayPrice>().Where(opp => opp.OrderId == r.Id).ToList(),
                   Products = _omsAccessor.Get<OrderProduct>().Where(op => op.OrderId == r.Id)
                   .Join(_omsAccessor.Get<SaleProduct>(), op => op.SaleProductId, sp => sp.Id, (op, sp) => new { op, sp })
                   .Join(_omsAccessor.Get<Product>(), rp => rp.sp.ProductId, p => p.Id, (rp, p) => new { rp, p }).Select(rs => new InterFaceOrderProduct
                   {
                       ProductName = rs.p.Name,
                       ProductCode = rs.p.Code,
                       Quantity = rs.rp.op.Quantity,
                       OriginPrice = rs.rp.op.OrginPrice,
                       Price = rs.rp.op.Price,
                       SumPrice = rs.rp.op.SumPrice
                   }).ToList()

               }).ToList();
            return data;
        }
        #endregion


        /// <summary>
        /// 修改订单（通用）
        /// </summary>
        /// <param name="order"></param>
        public Order UpdateOrder(Order order)
        {
            if (_workContext.CurrentUser != null)
            {
                order.ModifiedBy = _workContext.CurrentUser.Id;
            }

            order.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<Order>(order);
            _omsAccessor.SaveChanges();
            return order;
        }
        /// <summary>
        /// 获取订单支付信息
        /// </summary>
        /// <returns></returns>
        public OrderPayPrice GetOrderPay(int orderId)
        {
            var orderPayPriceResult = _omsAccessor.Get<OrderPayPrice>().Where(op => op.Isvalid && op.OrderId == orderId).OrderByDescending(op => op.CreatedTime).FirstOrDefault();
            return orderPayPriceResult;
        }
        /// <summary>
        /// 获取订单日志
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public IEnumerable<OrderLog> GetOrderRecord(int orderId)
        {
            var dataresult = _omsAccessor.Get<OrderLog>().Where(ol => ol.Isvalid && ol.OrderId == orderId).OrderByDescending(ol => ol.CreatedTime);
            return dataresult;
        }
        /// <summary>
        /// 获取订单发票信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public InvoiceInfo GetOrderInvoiceRecord(int orderId)
        {
            InvoiceInfo invoiceInfo = _omsAccessor.Get<InvoiceInfo>().Where(i => i.Isvalid && i.OrderId == orderId).FirstOrDefault();
            return invoiceInfo;
        }
        /// <summary>
        /// 添加订单支付信息
        /// </summary>
        /// <param name="orderPayPrice"></param>
        public void AddOrderPayPrice(OrderPayPrice orderPayPrice)
        {
            orderPayPrice.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert<OrderPayPrice>(orderPayPrice);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 判断订单商品是否存在
        /// </summary>
        /// <returns></returns>
        public OrderProduct ConfirmOrderProductIsExist(int orderId, int saleProductId)
        {
            var orderProduct = _omsAccessor.Get<OrderProduct>(o => o.Isvalid && o.OrderId == orderId && o.SaleProductId == saleProductId).Include(s => s.SaleProduct).Include(s => s.Order).FirstOrDefault();
            return orderProduct;
        }





        #region B2C订单
        /// <summary>
        /// 获取全部B2C订单
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Order> GetAllB2COrders()
        {
            var data = from o in _omsAccessor.Get<Order>() where (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_XH }).Contains(o.Type) select o;
            return data;
        }
        /// <summary>
        /// 获取全部B2C订单(分页)
        /// </summary>
        /// <returns></returns>
        public PageList<Object> GetAllB2COrdersByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, OrderState? orderState, int? wareHouseId, string search = "")
        {
            DateTime? dateTime = new DateTime();
            if (startTime.Equals(dateTime))
            {
                startTime = null;
            }
            if (endTime.Equals(dateTime))
            {
                endTime = null;
            }
            Dictionary<InvoiceType, string> InvoiceTypeName = new Dictionary<InvoiceType, string>
            {
                { InvoiceType.NoNeedInvoice,"无发票"},
                { InvoiceType.PersonalInvoice,"个人发票"},
                { InvoiceType.CompanyInvoice,"普通单位发票"},
                { InvoiceType.SpecialInvoice,"专用发票"}
            };

            var orderStates = new List<int>();
            if (!orderState.HasValue || orderState == OrderState.LackStock)
            {
                orderStates = null;
            }
            else if (orderState == OrderState.Unshipped)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.Confirmed);
                orderStates.Add((int)OrderState.Invalid);
                orderStates.Add((int)OrderState.Uploaded);
            }
            else if (orderState == OrderState.NoConfirm)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.Invalid);
            }
            else if (orderState == OrderState.NoUpload)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.B2CConfirmed);
                orderStates.Add((int)OrderState.Invalid);
            }
            else
                orderStates.Add((int)orderState);

            //Dictionary<OrderState, string> OrderStateName = new Dictionary<OrderState, string>
            //{
            //    { OrderState.Unpaid,OrderState.Unpaid.Description()},
            //    { OrderState.Paid,OrderState.Paid.Description()},
            //    { OrderState.B2CConfirmed,OrderState.B2CConfirmed.Description()},
            //    { OrderState.Unshipped,OrderState.Unshipped.Description()},
            //    { OrderState.Delivered,OrderState.Delivered.Description()},
            //    { OrderState.Invalid,OrderState.Invalid.Description()},
            //};
            //可以显示缺货状态的订单状态
            var canOrderStates = new OrderState[] {
                OrderState.B2CConfirmed,
                //OrderState.CheckAccept,
                OrderState.Confirmed,
                OrderState.FinancialConfirmation,
                OrderState.Paid,
                OrderState.ToBeConfirmed,
                OrderState.Unpaid,
                OrderState.ToBeTurned,
                OrderState.Unshipped
            };
            var OrderStateName = GetOrderStateStr();
            var data = (from o in _omsAccessor.Get<Order>()
                        where (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_XH }).Contains(o.Type)
                        where (string.IsNullOrEmpty(search) || o.SerialNumber.Contains(search) || o.PSerialNumber.Contains(search) || o.OrgionSerialNumber.Contains(search) ||
                        o.DeliveryNumber.Contains(search) || o.CustomerName.Contains(search) || o.CustomerPhone.Contains(search) || o.AdminMark.Contains(search) || o.CustomerMark.Contains(search))
                        && ((!startTime.HasValue || o.CreatedTime >= startTime.Value) && (!endTime.HasValue || o.CreatedTime <= endTime.Value))
                        && (!shopId.HasValue || o.ShopId == shopId) && (!orderState.HasValue || orderStates.Contains((int)o.State) || orderState == OrderState.LackStock)
                        && (!wareHouseId.HasValue || o.WarehouseId == wareHouseId)
                        join s in _omsAccessor.Get<Dictionary>() on o.ShopId equals s.Id
                        orderby o.CreatedTime descending
                        join i in _omsAccessor.Get<InvoiceInfo>(r => r.Isvalid) on o.Id equals i.OrderId into del
                        from de in del.DefaultIfEmpty()
                        select new
                        {
                            o.Id,
                            o.CreatedTime,
                            o.SerialNumber,
                            o.ShopId,
                            ShopName = s.Value,
                            o.CustomerName,
                            o.CustomerPhone,
                            DeliveryName = _omsAccessor.Get<Delivery>().Where(d => d.Id == o.DeliveryTypeId).FirstOrDefault().Name,
                            WareHouseName = _omsAccessor.Get<WareHouse>().Where(w => w.Id == o.WarehouseId).FirstOrDefault().Name,
                            o.State,
                            o.IsLocked,
                            StateName = OrderStateName[o.State],
                            o.SumPrice,
                            o.PSerialNumber,
                            o.PayType,
                            PayTypeName = (o.PayType == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == o.PayType).FirstOrDefault().Value),
                            o.PayState,
                            PayStateName = o.PayState.ToString() == "Success" ? "已付款" : "未付款",
                            o.DeliveryNumber,
                            InvoiceTypeName = InvoiceTypeName[o.InvoiceType],
                            o.AdminMark,
                            o.CustomerMark,
                            o.IsNeedPaperBag,
                            historyOrders = _omsAccessor.Get<Order>().Where(r => r.Isvalid && r.Id != o.Id && r.UserName.Equals(o.UserName) && !string.IsNullOrEmpty(r.UserName)).Count() > 0,
                            invoiceInfo = de == null ? false : true,
                            refundOrder = _omsAccessor.Get<Order>().Where(r => r.Isvalid && r.OrgionSerialNumber.Contains(o.SerialNumber)).FirstOrDefault() != null && o.State == OrderState.Invalid,
                            lackStock = !canOrderStates.Contains(o.State) ? false : ((from op in _omsAccessor.Get<OrderProduct>().Where(x => x.OrderId == o.Id && x.Isvalid)
                                                    join spl in _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == o.WarehouseId) on op.Id equals spl.OrderProductId
                                                    where op.Quantity == spl.LockNumber
                                                   select op).Count()!=_omsAccessor.Get<OrderProduct>().Where(x=>x.Isvalid && x.OrderId == o.Id).Count() ? true : false),
                            o.UserName,
                            chaiFen = o.OrgionSerialNumber.Contains("(") && o.OrgionSerialNumber.Contains(")"),
                            o.OrgionSerialNumber,
                            heBing = o.OrgionSerialNumber.Contains(",")

                        }).Distinct();

            if (orderState == OrderState.LackStock)
                data = data.Where(r => r.lackStock);
            return new PageList<Object>(data, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取B2C订单列表
        /// </summary>
        /// <returns></returns>
        public PageList<B2COrderViewModel> GetAllB2COrdersTableByPage(SearchOrderContext searchOrderContext)
        {

            DateTime? dateTime = new DateTime();
            if (searchOrderContext.StartTime.Equals(dateTime))
            {
                searchOrderContext.StartTime = null;
            }
            if (searchOrderContext.EndTime.Equals(dateTime))
            {
                searchOrderContext.EndTime = null;
            }
            Dictionary<InvoiceType, string> InvoiceTypeName = new Dictionary<InvoiceType, string>
            {
                { InvoiceType.NoNeedInvoice,"无发票"},
                { InvoiceType.PersonalInvoice,"个人发票"},
                { InvoiceType.CompanyInvoice,"普通单位发票"},
                { InvoiceType.SpecialInvoice,"专用发票"}
            };

            var canOrderStates = new OrderState[] {
                OrderState.B2CConfirmed,
                //OrderState.CheckAccept,
                OrderState.Confirmed,
                OrderState.FinancialConfirmation,
                OrderState.Paid,
                OrderState.ToBeConfirmed,
                OrderState.Unpaid,
                OrderState.ToBeTurned,
                OrderState.Unshipped
            };

            var orderStates = new List<int>();
            if (!searchOrderContext.OrderState.HasValue || searchOrderContext.OrderState == OrderState.LackStock)
            {
                orderStates = null;
            }
            else if (searchOrderContext.OrderState == OrderState.Unshipped)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.Confirmed);
                orderStates.Add((int)OrderState.Invalid);
                orderStates.Add((int)OrderState.Uploaded);
            }
            else if (searchOrderContext.OrderState == OrderState.NoConfirm)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.Invalid);
            }
            else if (searchOrderContext.OrderState == OrderState.NoUpload)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.B2CConfirmed);
                orderStates.Add((int)OrderState.Invalid);
            }
            else
                orderStates.Add((int)searchOrderContext.OrderState);

            var OrderStateName = GetOrderStateStr();
            var data = (from o in _omsAccessor.Get<Order>().Where(o => (string.IsNullOrEmpty(searchOrderContext.SearchStr) || o.SerialNumber.Contains(searchOrderContext.SearchStr) || o.PSerialNumber.Contains(searchOrderContext.SearchStr) || o.OrgionSerialNumber.Contains(searchOrderContext.SearchStr) ||
o.DeliveryNumber.Contains(searchOrderContext.SearchStr) || o.CustomerName.Contains(searchOrderContext.SearchStr) || o.CustomerPhone.Contains(searchOrderContext.SearchStr) || o.AdminMark.Contains(searchOrderContext.SearchStr) || o.CustomerMark.Contains(searchOrderContext.SearchStr)  || o.AddressDetail.Contains(searchOrderContext.SearchStr))
&& ((!searchOrderContext.StartTime.HasValue || o.CreatedTime >= searchOrderContext.StartTime.Value) && (!searchOrderContext.EndTime.HasValue || o.CreatedTime <= searchOrderContext.EndTime.Value))
&& ((!searchOrderContext.PayStartTime.HasValue || o.PayDate >= searchOrderContext.PayStartTime.Value) && (!searchOrderContext.PayEndTime.HasValue || o.PayDate <= searchOrderContext.PayEndTime.Value))
&& ((!searchOrderContext.DeliverStartTime.HasValue || o.DeliveryDate >= searchOrderContext.DeliverStartTime.Value) && (!searchOrderContext.DeliverEndTime.HasValue || o.DeliveryDate <= searchOrderContext.DeliverEndTime.Value))
&& (!searchOrderContext.ShopId.HasValue || o.ShopId == searchOrderContext.ShopId) && (!searchOrderContext.OrderState.HasValue || orderStates.Contains((int)o.State) || searchOrderContext.OrderState == OrderState.LackStock)
&& (!searchOrderContext.WareHouseId.HasValue || o.WarehouseId == searchOrderContext.WareHouseId) && (!searchOrderContext.IsLocked.HasValue || searchOrderContext.IsLocked == o.IsLocked))
                        where (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_XH }).Contains(o.Type)
                        join s in _omsAccessor.Get<Dictionary>() on o.ShopId equals s.Id
                        join delivery in _omsAccessor.Get<Delivery>().Where(x => x.Isvalid) on o.DeliveryTypeId equals delivery.Id into deliv
                        from deli in deliv.DefaultIfEmpty()
                        join wh in _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid) on o.WarehouseId equals wh.Id into wareh
                        from ware in wareh.DefaultIfEmpty()
                        orderby o.CreatedTime descending
                        join i in _omsAccessor.Get<InvoiceInfo>(r => r.Isvalid) on o.Id equals i.OrderId into del
                        from de in del.DefaultIfEmpty()
                        join ps in _omsAccessor.Get<Dictionary>().Where(x => x.Isvalid) on o.PayType equals ps.Id into pst
                        from pste in pst.DefaultIfEmpty()
                        select new B2COrderViewModel
                        {
                            Id = o.Id,
                            CreatedTime = o.CreatedTime,
                            SerialNumber = o.SerialNumber,
                            ShopId = o.ShopId,
                            ShopName = s.Value,
                            CustomerName = o.CustomerName,
                            CustomerPhone = o.CustomerPhone,
                            DeliveryName = deli.Name,
                            WareHouseName = ware.Name,
                            State = o.State,
                            IsLocked = o.IsLocked,
                            SumPrice = o.SumPrice,
                            PSerialNumber = o.PSerialNumber,
                            PayType = o.PayType,
                            PayTypeName = pste.Value,
                            PayState = o.PayState,
                            DeliveryNumber = o.DeliveryNumber,
                            InvoiceType = o.InvoiceType,
                            AdminMark = o.AdminMark,
                            CustomerMark = o.CustomerMark,
                            IsNeedPaperBag = o.IsNeedPaperBag,
                            invoiceInfo = de == null ? false : true,
                            PayDate = o.PayDate,
                            DeliveryDate = o.DeliveryDate,
                            UserName = o.UserName,
                            OrgionSerialNumber = o.OrgionSerialNumber,
                            WarehouseId = o.WarehouseId,
                            AddressDetail = o.AddressDetail,
                            chaiFen = o.AppendType == AppendType.Split ? true : false,
                            heBing = o.AppendType == AppendType.Combine ? true : false,
                            lackStock =!canOrderStates.Contains(o.State) ? false : ((from op in _omsAccessor.Get<OrderProduct>().Where(x => x.OrderId == o.Id && x.Isvalid)
                                                    join spl in _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == o.WarehouseId) on op.Id equals spl.OrderProductId
                                                    where op.Quantity == spl.LockNumber
                                                   select op).Count()!=_omsAccessor.Get<OrderProduct>().Where(x=>x.Isvalid && x.OrderId == o.Id).Count() ? true : false),
                        }).Distinct().OrderByDescending(x => x.CreatedTime);

            if (searchOrderContext.OrderState == OrderState.LackStock)
                data = data.Where(r => r.lackStock).OrderByDescending(x => x.CreatedTime);
            return new PageList<B2COrderViewModel>(searchOrderContext.Isvalid == true ? data.Where(r => r.State != OrderState.Invalid) : data, searchOrderContext.PageIndex, searchOrderContext.PageSize);
        }


        public PageList<Object> GetAllRefundOrdersByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, int? orderState, string search = "", OrderType orderType = OrderType.B2C_TH)
        {
            DateTime? dateTime = new DateTime();
            if (startTime.Equals(dateTime))
            {
                startTime = null;
            }
            if (endTime.Equals(dateTime))
            {
                endTime = null;
            }

            if (deliverStartTime.Equals(dateTime))
            {
                deliverStartTime = null;
            }
            if (deliverEndTime.Equals(dateTime))
            {
                deliverEndTime = null;
            }
            Dictionary<InvoiceType, string> InvoiceTypeName = new Dictionary<InvoiceType, string>
            {
                { InvoiceType.NoNeedInvoice,"无发票"},
                { InvoiceType.PersonalInvoice,"个人发票"},
                { InvoiceType.CompanyInvoice,"普通单位发票"},
                { InvoiceType.SpecialInvoice,"专用发票"}
            };
            var orderStateStr = GetOrderStateStr();
            var data = from o in _omsAccessor.Get<Order>()
                       where (o.Type == orderType)
                       where (string.IsNullOrEmpty(search) || o.SerialNumber.Contains(search) || o.PSerialNumber.Contains(search) || o.OrgionSerialNumber.Contains(search) ||
                       o.DeliveryNumber == search || o.CustomerName == search || o.CustomerPhone == search || o.AdminMark == search || o.CustomerMark == search)
                       && ((!startTime.HasValue || o.CreatedTime >= startTime.Value) && (!endTime.HasValue || o.CreatedTime <= endTime.Value))
                       && ((!deliverStartTime.HasValue || o.DeliveryDate >= deliverStartTime.Value) && (!deliverEndTime.HasValue || o.DeliveryDate <= deliverEndTime.Value))
                       && (!shopId.HasValue || o.ShopId == shopId)
                       && (!orderState.HasValue || o.State == (OrderState)orderState)
                       join s in _omsAccessor.Get<Dictionary>() on o.ShopId equals s.Id
                       join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id
                       join de in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals de.Id into del
                       from deli in del.DefaultIfEmpty()
                       orderby o.CreatedTime descending
                       select new
                       {
                           o.Id,
                           o.CreatedTime,
                           o.SerialNumber,
                           o.ShopId,
                           ShopName = s.Value,
                           o.WarehouseId,
                           WarehouseName = w.Name,
                           o.CustomerName,
                           o.CustomerPhone,
                           o.SumPrice,
                           o.PSerialNumber,
                           o.PayPrice,
                           o.OrgionSerialNumber,
                           o.PayType,
                           PayTypeName = (o.PayType == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == o.PayType).FirstOrDefault().Value),
                           o.PayState,
                           PayStateName = o.PayState.ToString() == "Success" ? "已付款" : "未付款",
                           o.DeliveryTypeId,
                           DeliveryTypeName = deli == null ? "" : deli.Name,
                           o.DeliveryNumber,
                           InvoiceTypeName = InvoiceTypeName[o.InvoiceType],
                           o.State,
                           StateName = orderStateStr[o.State],
                           o.AdminMark,
                           DeliveryDate = o.DeliveryDate.ToString()
                       };
            return new PageList<Object>(data, pageIndex, pageSize);
        }

        /// <summary>
        /// 添加B2C订单
        /// </summary>
        public Order CreatedB2COrder(Order order)
        {
            order.PSerialNumber = string.IsNullOrEmpty(order.PSerialNumber) ? "" : order.PSerialNumber.Trim();
            order.AdminMark = string.IsNullOrEmpty(order.AdminMark) ? "" : order.AdminMark.Trim();
            order.CustomerName = string.IsNullOrEmpty(order.CustomerName) ? "" : order.CustomerName.Trim();
            order.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert<Order>(order);
            _omsAccessor.SaveChanges();
            return order;
        }
        /// <summary>
        /// 通过SerialNumber查找Order订单记录
        /// </summary>
        /// <param name="serialNumber">OMS单号</param>
        /// <returns></returns>
        public Order GetOrderBySerialNumber(string serialNumber)
        {
            var order = _omsAccessor.Get<Order>().Where(s => s.Isvalid && s.SerialNumber == serialNumber).Include(r => r.OrderProduct).Include(x => x.InvoiceInfo).FirstOrDefault();
            return order;
        }
        /// <summary>
        ///  判断订单的退货状态（B2B和B2C退单）
        /// </summary>
        public RefundState JudgeOrderRefundState(int? orderId) {
            var refundState = RefundState.No;
            var orderProducts = (from op in _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == orderId)
                                 group op by op.SaleProductId into opg orderby opg.Key
                                 select new OrderProductModel
                                 {
                                   SaleProductId = opg.Key,
                                   Quantity = opg.Sum(x => x.Quantity)
                               }).ToList();
            var alreadyRefundOrderProducts = (from op in _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid)
                                             join o in _omsAccessor.Get<Order>().Where(x => x.Isvalid && x.OriginalOrderId == orderId && x.State != OrderState.Invalid && new OrderType[] { OrderType.B2B_TH, OrderType.B2C_TH }.Contains(x.Type))
                                             on op.OrderId equals o.Id
                                             group op by op.SaleProductId into opg orderby opg.Key
                                             select new OrderProductModel
                                             {
                                                 SaleProductId = opg.Key,
                                                 Quantity = opg.Sum(x => x.Quantity)
                                             }).ToList();
            if (orderProducts==null || orderProducts.Count==0 || alreadyRefundOrderProducts ==null  || alreadyRefundOrderProducts.Count==0) {
               return  refundState;
            }

            foreach (var item in orderProducts)
            {
                var comm = alreadyRefundOrderProducts.Where(x => x.SaleProductId == item.SaleProductId && x.Quantity == item.Quantity);
                if (comm==null || comm.Count()==0) {
                    refundState = RefundState.Part;
                    break;
                }
                refundState = RefundState.All;
            }
            return refundState;            
        }
        /// <summary>
        /// 通过PSerialNumber查找Order订单记录
        /// </summary>
        /// <param name="pserialNumber"></param>
        /// <returns></returns>
        public Order GetOrderByPSerialNumber(string pserialNumber)
        {
            var order = _omsAccessor.Get<Order>().Where(s => s.Isvalid && s.PSerialNumber == pserialNumber).FirstOrDefault();
            return order;
        }
        /// <summary>
        /// 通过ID获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Order GetOrderByIdB2C(int? orderId)
        {
            var result = _omsAccessor.Get<Order>().Include(x => x.OrderProduct).Where(o => o.Isvalid && o.Id == orderId).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// 添加B2C订单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <returns></returns>
        public bool AddOrderProductB2C(OrderProduct orderProduct)
        {
            try
            {
                orderProduct.CreatedBy = _workContext.CurrentUser.Id;
                orderProduct.CreatedTime = DateTime.Now;
                _omsAccessor.Insert<OrderProduct>(orderProduct);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        /// <summary>
        /// 判断PSerialNumber是否存在
        /// </summary>
        /// <param name="pserialNumber"></param>
        /// <returns></returns>
        public bool ConfirmPSerialNumberIsExist(string pserialNumber, bool isInvalid, bool isAll)
        {
            var data = _omsAccessor.Get<Order>().Where(o => o.Isvalid && o.PSerialNumber == pserialNumber.Trim() && (
            isAll ? true : (isInvalid ? o.State == OrderState.Invalid : o.State != OrderState.Invalid))).FirstOrDefault();
            if (data == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 判断OrgionSerialNumber是否存在
        /// </summary>
        /// <param name="orgionSerialNumber"></param>
        /// <returns></returns>
        public bool ConfirmOrgionSerialNumberIsExist(string orgionSerialNumber)
        {
            var data = _omsAccessor.Get<Order>().Where(o => o.Isvalid && o.OrgionSerialNumber == orgionSerialNumber.Trim()).FirstOrDefault();

            if (data == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 根据OriginalSerialNumber获取所有的退单
        /// </summary>
        /// <param name="originalSerialNumber"></param>
        /// <returns></returns>
        public List<Order> GetRefundOrdersByOriginalSerialNumber(string originalSerialNumber)
        {
            return _omsAccessor.Get<Order>().Where(r => r.Isvalid && r.OrgionSerialNumber == originalSerialNumber && r.Type == OrderType.B2B_TH).Include(r => r.OrderProduct).ToList();
        }
        /// <summary>
        /// B2C订单拆分
        /// </summary>
        /// <param name="oldOrderId"></param>
        /// <param name="newOrderId"></param>
        /// <param name="saleproductsInfo"></param>
        /// <param name="newOrderNum"></param>
        /// <param name="newOrderSumPayPrice"></param>
        /// <returns></returns>
        public int AddProductsToNewOrder(int oldOrderId, int newOrderId, Dictionary<int, int> saleproductsInfo, int newOrderNum, out decimal newOrderSumPayPrice, out decimal newOrderSumPrice)
        {
            var oldOrderProduct = GetOrderProductsByOrderId(oldOrderId);
            newOrderSumPrice = 0;
            var newOrderSaleProductsCount = 0;
            newOrderSumPayPrice = 0;
            foreach (var i in oldOrderProduct)
            {
                if (newOrderNum == 1)
                {
                    if (saleproductsInfo[i.Id] != 0)
                    {
                        OrderProduct newOrderProduct = new OrderProduct();
                        newOrderProduct.OrderId = newOrderId;
                        newOrderProduct.SaleProductId = i.SaleProductId;
                        newOrderProduct.Quantity = saleproductsInfo[i.Id];
                        newOrderProduct.OrginPrice = i.OrginPrice;
                        newOrderProduct.Price = i.Price;
                        newOrderProduct.SumPrice = newOrderProduct.Quantity * newOrderProduct.Price;//订单总价 (商品标准价格*商品数量)

                        if (newOrderProduct.Quantity != 0)
                        {
                            newOrderSumPrice += newOrderProduct.SumPrice;//新订单总价
                            _omsAccessor.Insert(newOrderProduct);
                        }
                        newOrderSaleProductsCount += newOrderProduct.Quantity;
                        newOrderSumPayPrice += newOrderProduct.Price * newOrderProduct.Quantity;
                    }
                }
                else
                {
                    OrderProduct newOrderProduct = new OrderProduct();
                    newOrderProduct.OrderId = newOrderId;
                    newOrderProduct.SaleProductId = i.SaleProductId;
                    newOrderProduct.Quantity = i.Quantity - saleproductsInfo[i.Id];
                    newOrderProduct.OrginPrice = i.OrginPrice;
                    newOrderProduct.Price = i.Price;
                    newOrderProduct.SumPrice = newOrderProduct.Quantity * newOrderProduct.Price;//订单总价 (商品标准价格*商品数量)

                    if (newOrderProduct.Quantity != 0)
                    {
                        newOrderSumPrice += newOrderProduct.SumPrice;//新订单总价
                        _omsAccessor.Insert(newOrderProduct);
                    }
                    newOrderSaleProductsCount += newOrderProduct.Quantity;
                    newOrderSumPayPrice += newOrderProduct.Price * newOrderProduct.Quantity;
                }
            }
            _omsAccessor.SaveChanges();
            return newOrderSaleProductsCount;
        }
        public int AddProductsToNewOrder(int oldOrderId, int newOrderId, out int saleProductsCount, out decimal newOrderSumPrice)
        {
            var oldOrderProduct = GetOrderProductsByOrderId(oldOrderId);
            newOrderSumPrice = 0;
            var newOrderSaleProductsCount = 0;
            saleProductsCount = 0;
            foreach (var i in oldOrderProduct)
            {
                if (oldOrderProduct != null)
                {
                    OrderProduct newOrderProduct = new OrderProduct
                    {
                        OrginId = i.Id,
                        OrderId = newOrderId,
                        SaleProductId = i.SaleProductId,
                        Quantity = i.Quantity,
                        OrginPrice = i.OrginPrice,
                        Price = i.Price,
                        CreatedBy = _workContext.CurrentUser.Id
                    };
                    newOrderProduct.SumPrice = newOrderProduct.Quantity * newOrderProduct.Price;

                    if (newOrderProduct.Quantity != 0)
                    {
                        newOrderSumPrice += newOrderProduct.SumPrice;//新订单总价
                        _omsAccessor.Insert(newOrderProduct);
                    }
                    newOrderSaleProductsCount += newOrderProduct.Quantity;

                    saleProductsCount += i.Quantity;
                }
            }
            _omsAccessor.SaveChanges();
            return newOrderSaleProductsCount;
        }


        /// <summary>
        /// 新增部分退货商品信息
        /// </summary>
        /// <param name="oldOrderId"></param>
        /// <param name="newOrderId"></param>
        /// <param name="refundProductInfoData"></param>
        /// <returns></returns>
        public bool AddPartRefundProducts(int? oldOrderId, int newOrderId, JArray refundProductInfoData) {
            var canRefundOrderProducts = GetCanRefundOrderProductsByOrderId(oldOrderId);
            foreach (var item in refundProductInfoData)
            {
                var itemCanRefund = canRefundOrderProducts.Where(x => x.OrderProductId == Convert.ToInt32(item["refundOrderProductId"].ToString())).FirstOrDefault();
                //检验是否可以退货
                if (itemCanRefund == null || Convert.ToInt32(item["refundQuantity"].ToString()) > itemCanRefund.CanRedundQuantity) return false;

                var orderProduct = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.Id == Convert.ToInt32(item["refundOrderProductId"].ToString())).FirstOrDefault();
                if (orderProduct == null) return false;
                if (Convert.ToInt32(item["refundQuantity"].ToString()) == 0) continue;

                var refundProduct = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == newOrderId && x.SaleProductId == orderProduct.SaleProductId && x.OrginId == orderProduct.Id).FirstOrDefault();
                if (refundProduct != null && refundProduct.Price == Convert.ToDecimal(item["price"].ToString()))
                {
                    refundProduct.Quantity += Convert.ToInt32(item["refundQuantity"].ToString());
                    refundProduct.SumPrice = refundProduct.Quantity * refundProduct.Price;
                    _omsAccessor.Update(refundProduct);
                }
                else
                {
                    OrderProduct newOrderProduct = new OrderProduct
                    {
                        OrginId = orderProduct.Id,
                        OrderId = newOrderId,
                        SaleProductId = orderProduct.SaleProductId,
                        Quantity = Convert.ToInt32(item["refundQuantity"].ToString()),
                        OrginPrice = orderProduct.OrginPrice,
                        Price = Convert.ToDecimal(item["price"].ToString()),
                        CreatedBy = _workContext.CurrentUser.Id
                    };
                    newOrderProduct.SumPrice = newOrderProduct.Quantity * newOrderProduct.Price;
                    _omsAccessor.Insert(newOrderProduct);
                }
            }
            _omsAccessor.SaveChanges();
            return true;
        }
        /// <summary>
        /// 获取可以合并订单
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>

        public IEnumerable<Order> GetCanCombineOrders(OrderType orderType, int? shopId, string searchStr = "")
        {
            var data = from s in _omsAccessor.Get<Order>().Where(s => s.Type == orderType && string.IsNullOrEmpty(s.OrgionSerialNumber) && (string.IsNullOrEmpty(searchStr) || s.CustomerName.Contains(searchStr) || s.CustomerPhone.Contains(searchStr) || s.AddressDetail.Contains(searchStr) || s.SerialNumber.Contains(searchStr))) where (new OrderState?[] { OrderState.Unpaid, OrderState.Paid, OrderState.B2CConfirmed, OrderState.Unshipped }).Contains(s.State) group s by new { s.UserName, s.CustomerName, s.CustomerPhone, s.AddressDetail } into d where d.Count() > 1 select d;

            var result = new List<Order>();
            foreach (var item in data)
            {
                result.AddRange(item.ToList());
            }
            return result;
        }

        /// <summary>
        /// 分页获取合并订单列表
        /// </summary>
        /// <param name="searchOrderContext"></param>
        /// <returns></returns>
        public PageList<B2COrderViewModel> GetCanPageListCombineOrders(SearchOrderContext searchOrderContext) {
            var data = from s in _omsAccessor.Get<Order>().Where(s => s.Type == OrderType.B2C_XH 
                       && string.IsNullOrEmpty(s.OrgionSerialNumber) 
                       && (!searchOrderContext.ShopId.HasValue || s.ShopId == searchOrderContext.ShopId) 
                       && (string.IsNullOrEmpty(searchOrderContext.SearchStr) || s.CustomerName.Contains(searchOrderContext.SearchStr) || s.CustomerPhone.Contains(searchOrderContext.SearchStr) || s.AddressDetail.Contains(searchOrderContext.SearchStr) || s.SerialNumber.Contains(searchOrderContext.SearchStr))) 
                       where (new OrderState?[] { OrderState.Unpaid, OrderState.Paid, OrderState.B2CConfirmed, OrderState.Unshipped }).Contains(s.State) group s by new { s.UserName, s.CustomerName, s.CustomerPhone, s.AddressDetail } into d where d.Count() > 1 select d;

            var result = new List<Order>();
            foreach (var item in data)
            {
                result.AddRange(item.ToList());
            }

            var resultData = new List<B2COrderViewModel>();
            foreach (var itemResd in result)
            {
                var b2cOrderViewModel = new B2COrderViewModel()
                {
                    Id = itemResd.Id,
                    lackStock = (from op in _omsAccessor.Get<OrderProduct>().Where(x => x.OrderId == itemResd.Id && x.Isvalid)
                                 join spl in _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == itemResd.WarehouseId) on op.Id equals spl.OrderProductId
                                 where op.Quantity == spl.LockNumber
                                 select op).Count() != _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == itemResd.Id).Count() ? true : false,
                    SerialNumber = itemResd.SerialNumber,
                    CreatedTime = itemResd.CreatedTime,
                    UserName = itemResd.UserName,
                    CustomerName = itemResd.CustomerName,
                    CustomerPhone = itemResd.CustomerPhone,
                    AddressDetail =itemResd.AddressDetail
                };
                resultData.Add(b2cOrderViewModel);
            }


            return new PageList<B2COrderViewModel>(resultData, searchOrderContext.PageIndex, searchOrderContext.PageSize);
        }
        public Order CombineOrder(int[] list, string newSerialNumber, out List<string> oldSerialNumber)
        {
            Order order = new Order();
            order.SerialNumber = newSerialNumber;
            order.AppendType = AppendType.Combine;
            oldSerialNumber = new List<string>();
            int count = 0;
            foreach (var item in list)
            {
                count++;
                var data = _omsAccessor.Get<Order>().Where(s => s.Isvalid && s.Id == item).FirstOrDefault();
                oldSerialNumber.Add(data.SerialNumber);
                order.OrgionSerialNumber += data.SerialNumber + (count == list.Count()?"":"_");
                order.PSerialNumber +=  data.PSerialNumber+ (count == list.Count() ? "" : "_");
                order.AddressDetail = data.AddressDetail;
                order.AdminMark += data.AdminMark + " ";
                order.CreatedBy = _workContext.CurrentUser.Id;
                order.CustomerMark += data.CustomerMark + " ";
                order.CustomerName = data.CustomerName;
                order.CustomerPhone = data.CustomerPhone;
                order.UserName = data.UserName;
                order.DeliveryTypeId = data.DeliveryTypeId;
                order.PayPrice += data.PayPrice;
                order.PayState = data.PayState;
                order.PayType = data.PayType;
                order.ShopId = data.ShopId;
                order.State = OrderState.Paid;
                order.SumPrice += data.SumPrice;
                order.Type = data.Type;
                order.ToWarehouseMessage += data.ToWarehouseMessage;
                order.WarehouseId = data.WarehouseId;
                order.PayDate = DateTime.Now;
                order.ZMWineCoupon += data.ZMWineCoupon;
                order.WineWorldCoupon += data.WineWorldCoupon;
                //设置原订单无效,不能复制订单
                data.State = OrderState.Invalid;
                data.IsCopied = true;
                _omsAccessor.Update<Order>(data);
            }
            _omsAccessor.Insert<Order>(order);
            //_omsAccessor.SaveChanges();
            //添加原订单商品信息
            foreach (var item in list)
            {
                var productData = _omsAccessor.Get<OrderProduct>().Where(p => p.Isvalid && p.OrderId == item);
                foreach (var i in productData)
                {
                    OrderProduct orderProduct = new OrderProduct();
                    orderProduct.CreatedBy = _workContext.CurrentUser.Id;
                    orderProduct.OrderId = order.Id;
                    orderProduct.OrginPrice = i.OrginPrice;
                    orderProduct.Price = i.Price;
                    orderProduct.Quantity = i.Quantity;
                    orderProduct.SaleProductId = i.SaleProductId;
                    orderProduct.SumPrice = i.SumPrice;
                    _omsAccessor.Insert<OrderProduct>(orderProduct);
                }
            }
            //订单支付信息
            OrderPayPrice orderPayPrice = new OrderPayPrice()
            {
                OrderId = order.Id,
                IsPay = true,
                PayType = order.PayType,
                PayMentType = order.PayMentType,
                Price = order.PayPrice,
            };
            _omsAccessor.Insert<OrderPayPrice>(orderPayPrice);
            _omsAccessor.SaveChanges();
            //解锁旧订单库存
            foreach(var item in list)
            {
                _productService.ChangeProLockedNumByHasLockedNum(item);
            }
            //锁定新订单库存
            var newOrderPros = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == order.Id).ToList();
            foreach(var item in newOrderPros)
            {
                var proInfo = _productService.GetSaleProductDetailBySaleProductId(item.SaleProductId);
                var result=_productService.AddSaleProductLockedTrackAndWareHouseStock(order.Id, item.SaleProductId, order.WarehouseId, item.Quantity, item.Id);
                if (!result.Contains("成功"))
                {
                    _logService.InsertOrderLog(order.Id, "更新锁定库存失败", order.State, order.PayState, "【" + proInfo.ProductName + "】 数量：" + item.Quantity);
                }
            }
            return order;
        }


        /// <summary>
        /// 检查合并订单是否满足要求
        /// </summary>
        /// <param name="list"></param>
        /// <param name="checkmsg"></param>
        /// <returns></returns>
        public bool CheckCombineOrderInfo(int[] list, out string checkmsg)
        {

            checkmsg = "";
            var orders = _omsAccessor.Get<Order>().Where(x => x.Isvalid && (new OrderState?[] { OrderState.Unpaid, OrderState.Paid, OrderState.B2CConfirmed, OrderState.Unshipped }).Contains(x.State) && Array.Exists(list, l => l.Equals(x.Id))).ToList();
            if (orders == null || orders.Count != list.Count())
            {
                checkmsg = "没有找到对应的订单或所选订单错误!";
                return false;
            }

            var firstOrder = orders.FirstOrDefault();

            if (orders.Where(x => x.UserName == firstOrder.UserName).Count() != list.Count())
            {
                checkmsg += "【合并订单的用户名不一致】 ";
            }

            if (orders.Where(x => x.CustomerName == firstOrder.CustomerName).Count() != list.Count())
            {
                checkmsg += "【合并订单的收货人姓名不一致】 ";
            }

            if (orders.Where(x => x.CustomerPhone == firstOrder.CustomerPhone).Count() != list.Count())
            {
                checkmsg += "【合并订单的收货人电话不一致】 ";
            }

            if (orders.Where(x => x.AddressDetail == firstOrder.AddressDetail).Count() != list.Count())
            {
                checkmsg += "【合并订单的收货人地址不一致】 ";
            }

            if (orders.Where(x => x.PayMentType == firstOrder.PayMentType).Count() != list.Count())
            {
                checkmsg += "【合并订单的支付方式不一致】 ";
            }

            if (orders.Where(x => x.WarehouseId == firstOrder.WarehouseId).Count() != list.Count())
            {
                checkmsg += "【合并订单的仓库不一致】";
            }

            if (orders.Where(x => x.WarehouseId == 0).Count() > 0)
            {
                checkmsg += "【合并订单中包含没有选择仓库的订单】";
            }

            if (orders.Where(x => x.DeliveryTypeId == firstOrder.DeliveryTypeId).Count() != list.Count())
            {
                checkmsg += "【合并订单的快递方式不一致】";
            }


            if (orders.Where(x => x.State == firstOrder.State).Count() != list.Count())
            {
                checkmsg += "【合并订单的订单状态不一致】";
            }

            if (!string.IsNullOrEmpty(checkmsg))
            {

                checkmsg += "请修改一致后进行合并！";
                return false;
            }

            return true;
        }
        /// <summary>
        /// 为订单匹配最佳发货仓库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string MatchWareHouseForOrder(int id, out int firstWareHouseId)
        {
            firstWareHouseId = 0;
            try
            {
                var order = _omsAccessor.GetById<Order>(id);
                if (order == null)
                    return (new { isSuc = false, msg = string.Format("订单ID为{0}的订单信息不存在，请联系管理员！", id) }).ToJson();
                var sheng = order.AddressDetail.Split(' ')[0];//获取收货地址所在的省份
                var shi = order.AddressDetail.Split(' ')[1];
                var zhiXiaShi = new List<string>() { "北京市", "上海市", "天津市", "重庆市" };
                if (!sheng.Contains("省") && !sheng.Contains("自治区") && !sheng.Contains("行政区") && !zhiXiaShi.Exists(r => r.Contains(sheng)) && !zhiXiaShi.Exists(r => r.Contains(shi)))
                    return (new { isSuc = false, msg = "收货地址不正确！" }).ToJson();

                if (!zhiXiaShi.Exists(r => r.Contains(sheng)) && zhiXiaShi.Exists(r => r.Contains(shi)))
                {
                    sheng = shi;
                }
                var products = _omsAccessor.Get<OrderProduct>().Where(r => r.Isvalid && r.OrderId == order.Id).Select(r => new { r.SaleProduct.ProductId, r.Quantity, ProductName = r.SaleProduct.Product.Name });
                if (products.Count() <= 0)
                    return (new { isSuc = false, msg = "请为订单添加商品！" }).ToJson();

                var wareHouseArea = _omsAccessor.Get<WareHouseArea>().Where(r => r.Isvalid && sheng.Contains(r.AreaName)).FirstOrDefault();//通过订单收货地址的省份来匹配对应的仓库区域
                if (wareHouseArea == null)
                    return (new { isSuc = false, msg = string.Format("仓库区域暂时未添加{0},请联系管理员添加！", sheng) }).ToJson();
                var wareHouseCodes = _omsAccessor.Get<WareHouseAreaRanks>().Where(r => r.Isvalid && r.WhAId == wareHouseArea.Id).OrderByDescending(r => r.Rank).Include(s => s.WareHouse).Select(t => new { Rank = t.Rank, WareHouseId = t.WhId, t.WareHouse.Code });//按照rank值对仓库进行降序排序，获取到仓库Id和对应的仓库Code，推送到WMS，依次进行仓库的商品库存匹配
                //首选仓库
                firstWareHouseId = wareHouseCodes.FirstOrDefault() == null ? 0 : wareHouseCodes.FirstOrDefault().WareHouseId;
                var post = new
                {
                    Products = products,
                    WareHouseCodes = wareHouseCodes
                };
                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    var content = new StringContent(post.ToJson(), Encoding.UTF8, "application/json");
                    var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wms/StockIn/OMSMatchWareHouseForOrder";
                    #region JWTBearer授权信息
                    _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                    if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    #endregion
                    var response = http.PostAsync(requestUrl, content).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = response.Content.ReadAsStringAsync();
                        return result.Result.ToString();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logService.Error("订单匹配仓库方法失败，原因是API授权失败！请重试。");
                        return (new { isSucc = false, msg = "订单匹配仓库方法失败，原因是API授权失败！请重试。" }).ToString();
                    }
                    else
                    {
                        _logService.Error(string.Format("订单匹配仓库方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        return (new { isSucc = false, msg = string.Format("订单匹配仓库方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return (new { isSuc = false, msg = ex.Message }).ToJson();
            }
        }

        /// <summary>
        /// 匹配最佳仓库
        /// </summary>
        /// <returns></returns>
        public string MatchFirstWareHouse(string addressDetail)
        {
            var sheng = addressDetail.Split(' ')[0];//获取收货地址所在的省份
            var shi = addressDetail.Split(' ')[1];
            var zhiXiaShi = new List<string>() { "北京市", "上海市", "天津市", "重庆市" };
            if (!sheng.Contains("省") && !sheng.Contains("自治区") && !sheng.Contains("行政区") && !zhiXiaShi.Exists(r => r.Contains(sheng)) && !zhiXiaShi.Exists(r => r.Contains(shi)))
                return (new { isSuc = true, msg = "收货地址不正确！", wareHouseId = 1 }).ToJson();

            if (!zhiXiaShi.Exists(r => r.Contains(sheng)) && zhiXiaShi.Exists(r => r.Contains(shi)))
            {
                sheng = shi;
            }

            var wareHouseArea = _omsAccessor.Get<WareHouseArea>().Where(r => r.Isvalid && sheng.Contains(r.AreaName)).FirstOrDefault();//通过订单收货地址的省份来匹配对应的仓库区域
            if (wareHouseArea == null)
                return (new { isSuc = false, msg = string.Format("仓库区域暂时未添加{0},请联系管理员添加！", sheng) }).ToJson();
            var firstWareHouseCodes = _omsAccessor.Get<WareHouseAreaRanks>().Where(r => r.Isvalid && r.WhAId == wareHouseArea.Id).OrderByDescending(r => r.Rank).FirstOrDefault();
            if (firstWareHouseCodes != null)
            {
                return (new { isSucc = true, msg = "匹配成功", wareHouseId = firstWareHouseCodes.WhId }).ToJson();
            }
            else
            {
                return (new { isSucc = true, msg = "没有找到对应的仓库信息！", wareHouseId = 1 }).ToJson();
            }

        }
        /// <summary>
        /// 获取最佳仓库id
        /// </summary>
        /// <param name="addressDetail"></param>
        /// <returns></returns>
        public int MatchFirstWareHouseId(string addressDetail)
        {
            var sheng = addressDetail.Split(' ')[0];//获取收货地址所在的省份
            var shi = addressDetail.Split(' ')[1];
            var zhiXiaShi = new List<string>() { "北京市", "上海市", "天津市", "重庆市" };
            if (!sheng.Contains("省") && !sheng.Contains("自治区") && !sheng.Contains("行政区") && !zhiXiaShi.Exists(r => r.Contains(sheng)) && !zhiXiaShi.Exists(r => r.Contains(shi)))
                return 1;

            if (!zhiXiaShi.Exists(r => r.Contains(sheng)) && zhiXiaShi.Exists(r => r.Contains(shi)))
            {
                sheng = shi;
            }

            var wareHouseArea = _omsAccessor.Get<WareHouseArea>().Where(r => r.Isvalid && sheng.Contains(r.AreaName)).FirstOrDefault();//通过订单收货地址的省份来匹配对应的仓库区域
            if (wareHouseArea == null)
                return 1;
            var firstWareHouseCodes = _omsAccessor.Get<WareHouseAreaRanks>().Where(r => r.Isvalid && r.WhAId == wareHouseArea.Id).OrderByDescending(r => r.Rank).FirstOrDefault();
            if (firstWareHouseCodes != null)
            {
                return firstWareHouseCodes.WhId;
            }
            else
            {
                return 1;
            }

        }

        /// <summary>
        /// 匹配仓库（考虑库存）
        /// </summary>
        /// <returns></returns>
        public int MatchWarehouseId(List<OrderProductInfo> orderProductInfos, string addressDetail) {
            var sheng = addressDetail.Split(' ')[0];//获取收货地址所在的省份
            var shi = addressDetail.Split(' ')[1];
            var zhiXiaShi = new List<string>() { "北京市", "上海市", "天津市", "重庆市" };
            if (!sheng.Contains("省") && !sheng.Contains("自治区") && !sheng.Contains("行政区") && !zhiXiaShi.Exists(r => r.Contains(sheng)) && !zhiXiaShi.Exists(r => r.Contains(shi)))
                return 1;

            if (!zhiXiaShi.Exists(r => r.Contains(sheng)) && zhiXiaShi.Exists(r => r.Contains(shi)))
            {
                sheng = shi;
            }
            var wareHouseArea = _omsAccessor.Get<WareHouseArea>().Where(r => r.Isvalid && sheng.Contains(r.AreaName)).FirstOrDefault();//通过订单收货地址的省份来匹配对应的仓库区域
            if (wareHouseArea == null)
                return 1;

            var wareHouseAreasCode = _omsAccessor.Get<WareHouseAreaRanks>().Where(r => r.Isvalid && r.WhAId == wareHouseArea.Id).OrderByDescending(r => r.Rank).ToList();

            if (wareHouseAreasCode != null && wareHouseAreasCode.Count>0)
            {
                if (orderProductInfos == null || orderProductInfos.Count==0) {
                    return wareHouseAreasCode.FirstOrDefault().WhId;
                }

                foreach (var itemCode in wareHouseAreasCode)
                {
                    foreach (var itemProduct in orderProductInfos)
                    {
                        SaleProduct saleProduct = _productService.GetSaleProductByGoodSn(itemProduct.goods_sn);
                        if (saleProduct==null || saleProduct.Id==0) {
                            return wareHouseAreasCode.FirstOrDefault().WhId;
                        }
                        var saleProductWareHouseStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid && (x.Stock - x.LockStock) > 0 && x.WareHouseId == itemCode.WhId && x.SaleProductId == saleProduct.Id).FirstOrDefault();
                        if (saleProductWareHouseStock!=null) {
                            return itemCode.WhId;
                        }
                    }
                }

                return wareHouseAreasCode.FirstOrDefault().WhId;
            }
            else
            {
                return 1;
            }
        }
        /// <summary>
        /// 检查订单选中的发货仓库库存是否足够
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string CheckWareHouseStock(int id)
        {
            try
            {
                var order = _omsAccessor.GetById<Order>(id);
                if (order == null)
                    return (new { isSuc = false, msg = string.Format("订单ID为{0}的订单信息不存在，请联系管理员！", id) }).ToJson();

                var products = _omsAccessor.Get<OrderProduct>().Where(r => r.Isvalid && r.OrderId == order.Id).Select(r => new { r.SaleProduct.ProductId, r.Quantity, ProductName = r.SaleProduct.Product.Name });
                var wareHouse = _omsAccessor.GetById<WareHouse>(order.WarehouseId);
                if (wareHouse == null)
                    return (new { isSuc = false, msg = string.Format("未找到Id为{0}的仓库！", order.WarehouseId) }).ToJson();

                var post = new
                {
                    Products = products,
                    WareHouseCode = wareHouse.Code
                };

                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    var content = new StringContent(post.ToJson(), Encoding.UTF8, "application/json");
                    var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wms/StockIn/CheckWareHouseStockForOrder";
                    #region JWTBearer授权信息
                    _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                    if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    #endregion
                    var response = http.PostAsync(requestUrl, content).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = response.Content.ReadAsStringAsync();
                        return result.Result.ToString();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logService.Error("订单检查库存方法失败，原因是API授权失败！请重试。");
                        return (new { isSucc = false, msg = "订单检查库存方法失败，原因是API授权失败！请重试。" }).ToString();
                    }
                    else
                    {
                        _logService.Error(string.Format("订单检查库存方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        return (new { isSucc = false, msg = string.Format("订单检查库存方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return (new { isSuc = false, msg = ex.Message }).ToJson();
            }
        }

        /// <summary>
        /// 检查德邦物流是否能够正常配送到收货地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string CheckDeBangIsDelivery(int id)
        {
            try
            {
                var order = _omsAccessor.GetById<Order>(id);
                if (order == null)
                    return (new { isSuc = false, msg = string.Format("订单ID为{0}的订单信息不存在，请联系管理员！", id) }).ToJson();
                var wareHouse = _omsAccessor.GetById<WareHouse>(order.WarehouseId);
                if (wareHouse == null)
                    return (new { isSuc = false, msg = string.Format("未找到Id为{0}的仓库！", order.WarehouseId) }).ToJson();

                var addressSplit = order.AddressDetail.Split(' ');
                var model = new DBCheckDeliveryModel
                {
                    OrderNo = order.SerialNumber,
                    WareHouseCode = wareHouse.Code,
                    Receiver = new ReceiverInfo
                    {
                        Address = order.AddressDetail,
                        Name = order.CustomerName,
                        Province = addressSplit[0],
                        City = addressSplit[1],
                        Country = addressSplit[2],
                        Mobile = order.CustomerPhone,
                        Phone = order.CustomerPhone
                    }
                };

                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    var content = new StringContent(model.ToJson(), Encoding.UTF8, "application/json");
                    var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wms/Order/OMSOrderCheckDeBangIsDelivery";
                    #region JWTBearer授权信息
                    _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                    if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    #endregion
                    var response = http.PostAsync(requestUrl, content).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = response.Content.ReadAsStringAsync();
                        return result.Result.ToString();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logService.Error("订单上传时检查德邦物流是否能正确配送方法失败，原因是API授权失败！请重试。");
                        return (new { isSucc = false, msg = "订单上传时检查德邦物流是否能正确配送方法失败，原因是API授权失败！请重试。" }).ToString();
                    }
                    else
                    {
                        _logService.Error(string.Format("订单上传时检查德邦物流是否能正确配送方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        return (new { isSucc = false, msg = string.Format("订单上传时检查德邦物流是否能正确配送方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return (new { isSuc = false, msg = ex.Message }).ToJson();
            }
        }
        /// <summary>
        /// 复制订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string CopyB2COrder(int orderId)
        {
            /* 已经复制了一个订单,不能再在原单复制
             * 原订单的IsCopied设置为true
             * 合并，拆分都设置为true
             */
            var order = _omsAccessor.Get<Order>().Where(r => r.Id == orderId).FirstOrDefault();
            if (order.State != OrderState.Invalid)
            {
                return "无效订单才能使用复制订单功能！";
            }
            if (order.IsCopied == true)
            {
                return "订单无法复制或已经复制过订单，请在已经复制生成订单处复制订单！";
            }
            using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    var newOrderSerialNum = CheckOrderSerialNum();
                    var orderPro = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == order.Id).ToList();
                    var psStr = "";
                    if (order.PSerialNumber.Contains("'"))
                    {
                        var strList = order.PSerialNumber.Split('\'');
                        foreach (var item in strList)
                        {
                            psStr += "'" + item + "(C)";
                        }
                    }
                    else
                    {
                        psStr = order.PSerialNumber + "(C)";
                    }
                    #region newOrder
                    Order newOrder = new Order();
                    newOrder.SerialNumber = newOrderSerialNum;
                    newOrder.Type = order.Type;
                    newOrder.ShopId = order.ShopId;
                    newOrder.PSerialNumber = psStr;
                    newOrder.OrgionSerialNumber = order.OrgionSerialNumber;
                    newOrder.State = OrderState.Paid;
                    newOrder.WriteBackState = 0;
                    newOrder.PayType = order.PayType;
                    newOrder.PayMentType = order.PayMentType;
                    newOrder.IsLocked = false;
                    newOrder.LockMan = 0;
                    newOrder.LockStock = false;
                    newOrder.SumPrice = order.SumPrice;
                    newOrder.PayState = PayState.Success;
                    newOrder.PayPrice = (order.PayPrice == 0 ? newOrder.SumPrice : order.PayPrice);
                    newOrder.DeliveryTypeId = order.DeliveryTypeId;
                    newOrder.CustomerName = order.CustomerName;
                    newOrder.CustomerPhone = order.CustomerPhone;
                    newOrder.AddressDetail = order.AddressDetail;
                    newOrder.DistrictId = order.DistrictId;
                    newOrder.CustomerMark = order.CustomerMark;
                    newOrder.InvoiceType = order.InvoiceType;
                    newOrder.AdminMark = order.AdminMark;
                    newOrder.ToWarehouseMessage = order.ToWarehouseMessage;
                    newOrder.WarehouseId = order.WarehouseId == 0 ? _omsAccessor.Get<WareHouse>().FirstOrDefault().Id : order.WarehouseId;
                    newOrder.PriceTypeId = order.PriceTypeId;
                    newOrder.CustomerId = order.CustomerId;
                    newOrder.ApprovalProcessId = order.ApprovalProcessId;
                    newOrder.Isvalid = true;
                    newOrder.CreatedBy = _workContext.CurrentUser.Id;
                    newOrder.InvoiceMode = order.InvoiceMode;
                    newOrder.ProductCoupon = order.ProductCoupon;
                    newOrder.ZMIntegralValuePrice = order.ZMIntegralValuePrice;
                    newOrder.ZMCoupon = order.ZMCoupon;
                    newOrder.ZMWineCoupon = order.ZMWineCoupon;
                    newOrder.WineWorldCoupon = order.WineWorldCoupon;
                    newOrder.UserName = order.UserName;
                    newOrder.IsNeedPaperBag = order.IsNeedPaperBag;
                    newOrder.OriginalOrderId = order.OriginalOrderId;
                    newOrder.SalesManId = order.SalesManId;
                    newOrder.FinanceMark = order.FinanceMark;
                    order.IsCopied = true;
                    _omsAccessor.Insert<Order>(newOrder);
                    _omsAccessor.Update<Order>(order);
                    _omsAccessor.SaveChanges();
                    #endregion
                    //订单商品及锁定库存
                    foreach (var item in orderPro)
                    {
                        OrderProduct newOrderPro = new OrderProduct();
                        newOrderPro.OrderId = newOrder.Id;
                        newOrderPro.SaleProductId = item.SaleProductId;
                        newOrderPro.Quantity = item.Quantity;
                        newOrderPro.OrginPrice = item.OrginPrice;
                        newOrderPro.Price = item.Price;
                        newOrderPro.SumPrice = item.SumPrice;
                        newOrderPro.CreatedBy = _workContext.CurrentUser.Id;
                        //锁定库存
                        var salePro = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == newOrderPro.SaleProductId).FirstOrDefault();
                        salePro.LockStock += newOrderPro.Quantity;
                        salePro.AvailableStock = salePro.Stock - salePro.LockStock;
                        if (salePro.AvailableStock < 0)
                        {
                            trans.Rollback();
                            return "转单后商品可用库存不足，无法复制生新订单！";
                        }
                        //库存
                        _omsAccessor.Insert<OrderProduct>(newOrderPro);
                        _omsAccessor.Update<SaleProduct>(salePro);
                        _omsAccessor.SaveChanges();
                        //库存跟踪及锁定
                        var result = _productService.CreateSaleProductLockedTrackAndWareHouseStock(newOrder.Id, item.SaleProductId, newOrder.WarehouseId , item.Quantity, null);
                        if (!result)
                        {
                            #region 订单日志(更新锁定信息失败)
                            _logService.InsertOrderLog(newOrder.Id, "更新锁定信息失败", newOrder.State, newOrder.PayState, "更新锁定库存跟踪及仓库OMS库存锁定失败!");
                            #endregion
                        }
                    }
                    //支付信息
                    OrderPayPrice newPayPrice = new OrderPayPrice();
                    newPayPrice.OrderId = newOrder.Id;
                    newPayPrice.IsPay = true;
                    newPayPrice.PayType = newOrder.PayType;
                    newPayPrice.PayMentType = newOrder.PayMentType;
                    newPayPrice.Price = newOrder.PayPrice;
                    _omsAccessor.Insert<OrderPayPrice>(newPayPrice);
                    _omsAccessor.SaveChanges();
                    #region 日志
                    _logService.InsertOrderLog(newOrder.Id, "复制订单", OrderState.Paid, newOrder.PayState, "由" + order.SerialNumber + "复制生成订单");
                    _logService.InsertOrderLog(order.Id, "复制订单", order.State, order.PayState, "复制生成订单" + newOrder.SerialNumber);
                    #endregion
                    trans.Commit();
                    return "复制订单成功！";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    return "复制订单出错,位置:" + e.StackTrace;
                }
            }
        }
        string CheckOrderSerialNum(string preWord="")
        {
            var newOrderSerialNum = _commonService.GetOrderSerialNumber(preWord);
            var check = GetOrderBySerialNumber(newOrderSerialNum);
            if (check != null)
            {
                CheckOrderSerialNum(preWord);
            }
            return newOrderSerialNum;
        }
        public string ChangeProEqPrice(int orderProId, int saleProId, int proNum)
        {
            /* ******************************************************************************************
             * 
             * （1）解锁旧商品SaleProduct锁定库存
             * （2）解锁旧商品的SaleProductLockedTrack锁定信息、以及SaleProductWareHouseStock锁定信息
             * （3）锁定新商品SaleProduct锁定库存
             * （4）锁定新商品的SaleProductLockedTrack锁定信息、以及SaleProductWareHouseStock锁定信息
             * （5）把旧商品改成新商品
             * 
             * ******************************************************************************************/

            using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    var oldProInfo = _omsAccessor.Get<OrderProduct>().Where(r => r.Id == orderProId).FirstOrDefault();
                    var oldProNum = oldProInfo.Quantity;
                    var order = _omsAccessor.Get<Order>().Where(r => r.Id == oldProInfo.OrderId).FirstOrDefault();

                    //（1）解锁旧商品SaleProduct库存
                    var oldSaleProduct = _productService.GetSaleProductBySaleProductId(oldProInfo.SaleProductId);
                    oldSaleProduct.LockStock -= oldProInfo.Quantity;
                    oldSaleProduct.AvailableStock = oldSaleProduct.Stock - oldSaleProduct.LockStock;
                    _productService.UpdateSaleProduct(oldSaleProduct);

                    //（2）解锁旧商品的锁定信息
                    var oldTrackInfo = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == oldProInfo.OrderId && r.SaleProductId == oldProInfo.SaleProductId
                    && r.WareHouseId == order.WarehouseId && r.OrderProductId == oldProInfo.Id).FirstOrDefault();
                    if (oldTrackInfo == null)
                    {
                        SaleProductLockedTrack SPLT = new SaleProductLockedTrack();
                        SPLT.OrderId = order.Id;
                        SPLT.OrderProductId = oldProInfo.Id;
                        SPLT.OrderSerialNumber = order.SerialNumber;
                        SPLT.ProductId = oldSaleProduct.ProductId;
                        SPLT.SaleProductId = oldSaleProduct.Id;
                        SPLT.LockNumber = 0;
                        SPLT.WareHouseId = order.WarehouseId;
                        _omsAccessor.Insert<SaleProductLockedTrack>(SPLT);
                        _omsAccessor.SaveChanges();
                    }
                    else
                    {
                        if (oldTrackInfo.LockNumber != 0)
                        {
                            var oldStockInfo = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == oldProInfo.SaleProductId && r.WareHouseId == order.WarehouseId).FirstOrDefault();
                            //能有锁定数即代表oldStockInfo不为空且库存>0
                            oldStockInfo.LockStock -= oldTrackInfo.LockNumber;
                            oldTrackInfo.LockNumber = 0;
                            _omsAccessor.Update<SaleProductLockedTrack>(oldTrackInfo);
                            _omsAccessor.Update<SaleProductWareHouseStock>(oldStockInfo);
                            _omsAccessor.SaveChanges();
                        }
                    }
                    
                    //（3）锁定新商品SaleProduct库存
                    var newSaleProduct = _productService.GetSaleProductBySaleProductId(saleProId);
                    newSaleProduct.LockStock += proNum;
                    newSaleProduct.AvailableStock = newSaleProduct.Stock - newSaleProduct.LockStock;
                    _productService.UpdateSaleProduct(newSaleProduct);

                    //（4）锁定新商品的SaleProductLockedTrack锁定信息、以及SaleProductWareHouseStock锁定信息
                    var newTrackInfo = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == order.Id && r.SaleProductId == saleProId && r.OrderProductId==oldProInfo.Id).FirstOrDefault();
                    if (newTrackInfo == null)
                    {
                        SaleProductLockedTrack sPLT = new SaleProductLockedTrack();
                        sPLT.OrderId = order.Id;
                        sPLT.OrderProductId = oldProInfo.Id;
                        sPLT.OrderSerialNumber = order.SerialNumber;
                        sPLT.ProductId = newSaleProduct.ProductId;
                        sPLT.SaleProductId = saleProId;
                        sPLT.LockNumber = 0;
                        sPLT.WareHouseId = order.WarehouseId;
                        _omsAccessor.Insert<SaleProductLockedTrack>(sPLT);
                        _omsAccessor.SaveChanges();
                        newTrackInfo = sPLT;
                    }
                    var newStockInfo = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == newSaleProduct.Id
                    && r.WareHouseId == order.WarehouseId && r.WareHouseId == order.WarehouseId).FirstOrDefault();
                    if (newStockInfo == null)
                    {
                        SaleProductWareHouseStock nSPWHS = new SaleProductWareHouseStock();
                        nSPWHS.SaleProductId = newSaleProduct.Id;
                        nSPWHS.WareHouseId = order.WarehouseId;
                        nSPWHS.Stock = 0;
                        nSPWHS.LockStock = 0;
                        _omsAccessor.Insert<SaleProductWareHouseStock>(nSPWHS);
                        _omsAccessor.SaveChanges();
                    }
                    else
                    {
                        var avaStock = newStockInfo.Stock - newStockInfo.LockStock;
                        if (avaStock >= proNum)
                        {
                            newStockInfo.LockStock += proNum;
                            newTrackInfo.LockNumber = proNum;
                        }
                        else if (avaStock > 0 && avaStock < proNum)
                        {
                            newStockInfo.LockStock += avaStock;
                            newTrackInfo.LockNumber = avaStock;
                        }
                        _omsAccessor.Update<SaleProductLockedTrack>(newTrackInfo);
                        _omsAccessor.Update<SaleProductWareHouseStock>(newStockInfo);
                    }

                    //（5）旧商品改成新商品
                    oldProInfo.SaleProductId = newSaleProduct.Id;
                    oldProInfo.Quantity = proNum;
                    _omsAccessor.Update<OrderProduct>(oldProInfo);
                    _omsAccessor.SaveChanges();
                    #region 日志
                    _logService.InsertOrderLog(order.Id, "等价换货", order.State, order.PayState, "[" + _productService.GetProductById(oldSaleProduct.ProductId).Name + "] 数量：" + oldProNum + " 替换为[" + _productService.GetProductById(newSaleProduct.ProductId).Name + "]数量：" + proNum);
                    #endregion
                    trans.Commit();
                    return "";
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    _logService.Error("<ChangeProEqPrice>:" + e.Message + "\r\n位置：" + e.StackTrace);
                    return "等价换错错误，请查看日志";
                }
            }
        }
        #endregion

        #region 物流
        public List<Delivery> GetAllDeliveryList()
        {
            return _omsAccessor.Get<Delivery>().Where(p => p.Isvalid).ToList();
        }
        #endregion

        public Dictionary<OrderState, string> GetOrderStateStr()
        {
            Dictionary<OrderState, string> orderStateStrs = new Dictionary<OrderState, string>();
            foreach (OrderState orderstate in Enum.GetValues(typeof(OrderState)))
            {
                orderStateStrs.Add(orderstate, orderstate.Description());
            }
            return orderStateStrs;
        }

        public Object GetEveryMonthB2COrders(int? month)
        {
            if (!month.HasValue)
            {
                month = DateTime.Now.Month;
            }
            var data = _omsAccessor.Get<Order>().Where(o => o.Isvalid
            && (new OrderType?[] { OrderType.B2C_XH, OrderType.B2C_QJ, OrderType.B2C_KJ, OrderType.B2C_HZS }).Contains(o.Type)
            && o.CreatedTime.Year == DateTime.Now.Year && o.CreatedTime.Month == month).GroupBy(o => o.CreatedTime.Day);
            List<MonthOrders> lMonthOrders = new List<MonthOrders>();

            var aMonthDays = 0;
            if (month.Value == DateTime.Now.Month)
            {
                aMonthDays = DateTime.Now.Day;
            }
            else
            {
                aMonthDays = DateTime.DaysInMonth(DateTime.Now.Year, month.Value);
            }

            for (int i = 0; i < aMonthDays; i++)
            {
                var monthOrders = new MonthOrders();
                monthOrders.DayOfMonth = (i + 1);
                monthOrders.OrderCount = 0;
                monthOrders.DeliveredOrder = 0;
                foreach (var item in data)
                {
                    if (item.FirstOrDefault().CreatedTime.Day == (i + 1))
                    {
                        monthOrders.DayOfMonth = item.FirstOrDefault().CreatedTime.Day;
                        monthOrders.OrderCount = item.Count();
                        monthOrders.DeliveredOrder = 0;//填写发货数据
                    }
                }
                lMonthOrders.Add(monthOrders);
            }
            return lMonthOrders;
        }
        class MonthOrders
        {
            public int DayOfMonth { get; set; }
            public int OrderCount { get; set; }
            public int DeliveredOrder { get; set; }
        }

        [Authorize]
        public async Task<string> UploadOrder(int id)
        {
            Order order = _omsAccessor.Get<Order>()

                //.Include(o => o.InvoiceInfo) //有可能没有，include 即 join 表后导致订单也查不出来
                //.Include(o => o.Delivery)
                //.Include(o => o.WareHouse)
                .Include(o => o.OrderProduct)
                .Where(o => o.Isvalid && o.Id == id).FirstOrDefault();

            if (order == null)
                return (new { isSucc = false, msg = "发生错误，该订单信息有误！" }).ToString();

            if (order.DeliveryTypeId > 0)
            {
                var delivery = _omsAccessor.Get<Delivery>().Where(x => x.Isvalid && x.Id == order.DeliveryTypeId).FirstOrDefault();
                if (delivery != null)
                    order.Delivery = delivery;
                else
                    return (new { isSucc = false, msg = string.Format("发生错误，该订单物流方式（DeliveryId：{0}）有误！", order.DeliveryTypeId) }).ToString();
            }
            else
            {
                return (new { isSucc = false, msg = "发生错误，该订单没有选择物流方式！" }).ToString();
            }
            if (order.WarehouseId > 0)
            {
                var warehouse = _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.Id == order.WarehouseId).FirstOrDefault();
                if (warehouse != null)
                    order.WareHouse = warehouse;
                else
                    return (new { isSucc = false, msg = string.Format("发生错误，该订单选择的仓库（warehouseId:{0}）不存在！", order.WarehouseId) }).ToString();
            }
            else
            {
                return (new { isSucc = false, msg = "发生错误，该订单没有选择仓库！" }).ToString();
            }

            order.OrderProduct.ForEach(op =>
            {
                op.SaleProduct = _omsAccessor.Get<SaleProduct>(sp => sp.Id == op.SaleProductId && sp.Isvalid).FirstOrDefault();
            });

            InvoiceInfo invoiceInfo = _omsAccessor.Get<InvoiceInfo>(a => a.OrderId == id).FirstOrDefault();
            if (order.InvoiceType == InvoiceType.NoNeedInvoice)
                invoiceInfo = null;
            var post = new
            {
                Type = order.Type == OrderType.B2B ? 0 : order.Type == OrderType.B2C_XH ? 1 : 0,
                ShopId = order.ShopId,
                OMSSerialNumber = order.SerialNumber,
                OrderState = order.State,
                PayType = order.PayType,
                WriteBackState = order.WriteBackState,
                IsLocked = order.IsLocked,
                DeliveryTypeId = order.DeliveryTypeId,
                DeliveryNumber = order.DeliveryNumber,
                DeliveryDate = order.DeliveryDate,
                DeliveryCode = order.Delivery.Code,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                AddressDetail = order.AddressDetail,
                DistrictId = order.DistrictId,
                CustomerMark = order.CustomerMark,
                InvoiceType = order.InvoiceType,
                InvoiceMode = order.InvoiceMode,
                AdminMark = order.AdminMark,
                ToWarehouseMessage = order.ToWarehouseMessage,
                WarehouseId = order.WarehouseId,
                WarehouseCode = order.WareHouse.Code,
                OrderTime = order.CreatedTime,
                InvoiceInfo = invoiceInfo != null ?
                new
                {
                    invoiceInfo.CustomerEmail,
                    invoiceInfo.Title,
                    invoiceInfo.TaxpayerID,
                    invoiceInfo.RegisterAddress,
                    invoiceInfo.RegisterTel,
                    invoiceInfo.BankOfDeposit,
                    invoiceInfo.BankAccount,
                    invoiceInfo.BankCode,
                    invoiceInfo.InvoiceNo,
                }
                : null,
                OrderProduct = order.OrderProduct.Where(op=>op.Isvalid) != null ?
                //                order.OrderProduct.Select(p => new
                //                {
                //                    OMSProductId = p.SaleProduct.ProductId,
                //                    p.Quantity,
                //                    p.OrginPrice,
                //                    p.Price,
                //                    p.SumPrice,
                //                    p.Type
                //                })
                //.ToList()
                //: null
                order.OrderProduct.Where(x => x.Isvalid).GroupBy(op => new { op.SaleProduct.ProductId, op.OrginPrice, op.Type }).Select(g => new
                      {
                          OMSProductId = g.Key.ProductId,
                          Quantity = g.Sum(x => x.Quantity),
                          g.Key.OrginPrice,
                          SumPrice = g.Sum(x => x.SumPrice),
                          g.Key.Type,
                          Price = decimal.Round(g.Sum(x => x.SumPrice) / g.Sum(x => x.Quantity), 2)

                      })
                .ToList()
                : null
            };



            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                string jsonStr = post.ToJson();
                #region JWTBearer授权校验信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                #endregion
                StringContent content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                string url = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wms/order/OMSPush";
                var response = http.PostAsync(url, content).Result;
                string result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                {
                    await GetWMSOauthToken();
                    _logService.Error("UploadOrder方法失败，原因是API授权失败！请重试。");   
                    return (new { isSucc = false, msg = "UploadOrder方法失败，原因是API授权失败！请重试。" }).ToString();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return result;
                }
                else
                {
                    _logService.Error(string.Format("UploadOrder方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, msg = string.Format("UploadOrder方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToString();
                }
            }


        }

        /// <summary>
        /// 推送退货订单到WMS
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string UploadRefundOrder(int id)
        {
            Order order = _omsAccessor.Get<Order>()
            .Include(o => o.OrderProduct)
            .Where(o => o.Isvalid && o.Id == id).FirstOrDefault();

            if (order == null)
                return (new { isSucc = false, msg = "发生错误，该订单信息有误！" }).ToJson();

            if (order.DeliveryTypeId > 0)
            {
                var delivery = _omsAccessor.Get<Delivery>().Where(x => x.Isvalid && x.Id == order.DeliveryTypeId).FirstOrDefault();
                if (delivery != null)
                    order.Delivery = delivery;
                else
                    return (new { isSucc = false, msg = string.Format("发生错误，该订单物流方式（DeliveryId：{0}）有误！", order.DeliveryTypeId) }).ToJson();
            }
            else
            {
                return (new { isSucc = false, msg = "发生错误，该订单没有选择物流方式！" }).ToJson();
            }
            if (order.WarehouseId > 0)
            {
                var warehouse = _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.Id == order.WarehouseId).FirstOrDefault();
                if (warehouse != null)
                    order.WareHouse = warehouse;
                else
                    return (new { isSucc = false, msg = string.Format("发生错误，该订单选择的仓库（warehouseId:{0}）不存在！", order.WarehouseId) }).ToJson();
            }
            else
            {
                return (new { isSucc = false, msg = "发生错误，该订单没有选择仓库！" }).ToJson();
            }

            order.OrderProduct.ForEach(op =>
            {
                op.SaleProduct = _omsAccessor.Get<SaleProduct>(sp => sp.Id == op.SaleProductId).FirstOrDefault();
            });


            order.OrderProduct = order.OrderProduct.Where(x => x.Isvalid).ToList();
            var post = new
            {
                Type = OrderType.B2C_TH,
                ShopId = order.ShopId,
                OMSSerialNumber = order.SerialNumber,
                OrderState = order.State,
                PayType = order.PayType,
                WriteBackState = order.WriteBackState,
                IsLocked = order.IsLocked,
                DeliveryTypeId = order.DeliveryTypeId,
                DeliveryNumber = order.DeliveryNumber,
                DeliveryDate = order.DeliveryDate,
                DeliveryCode = order.Delivery.Code,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                AddressDetail = order.AddressDetail,
                DistrictId = order.DistrictId,
                CustomerMark = order.CustomerMark,
                InvoiceType = order.InvoiceType,
                InvoiceMode = order.InvoiceMode,
                AdminMark = order.AdminMark,
                ToWarehouseMessage = order.ToWarehouseMessage,
                WarehouseId = order.WarehouseId,
                WarehouseCode = order.WareHouse.Code,
                OrderTime = order.CreatedTime,
                InvoiceInfo = order.InvoiceInfo != null ?
                new
                {
                    order.InvoiceInfo.CustomerEmail,
                    order.InvoiceInfo.Title,
                    order.InvoiceInfo.TaxpayerID,
                    order.InvoiceInfo.RegisterAddress,
                    order.InvoiceInfo.RegisterTel,
                    order.InvoiceInfo.BankOfDeposit,
                    order.InvoiceInfo.BankAccount,
                    order.InvoiceInfo.BankCode,
                    order.InvoiceInfo.InvoiceNo,
                }
                : null,
                OrderProduct = order.OrderProduct != null ?
                //order.OrderProduct.Select(p => new
                //{
                //    OMSProductId = p.SaleProduct.ProductId,
                //    p.Quantity,
                //    p.OrginPrice,
                //    p.Price,
                //    p.SumPrice,
                //    p.Type
                //})
                //.ToList()
                //: null,

                order.OrderProduct.Where(x => x.Isvalid).GroupBy(op => new { op.SaleProduct.ProductId, op.OrginPrice, op.Type }).Select(g => new
                {
                    OMSProductId = g.Key.ProductId,
                    Quantity = g.Sum(x => x.Quantity),
                    g.Key.OrginPrice,
                    SumPrice = g.Sum(x => x.SumPrice),
                    g.Key.Type,
                    Price = decimal.Round(g.Sum(x => x.SumPrice) / g.Sum(x => x.Quantity), 2)
                })
                .ToList()
                : null,
                UploadBy = _workContext.CurrentUser.UserName
            };

            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                string jsonStr = post.ToJson();
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                #region JWTBearer授权校验信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                #endregion
                string url = _configuration.GetSection("WMSApi")["domain"].ToString() + "/wms/order/OMSPushRefundOrder";
                var response = http.PostAsync(url, content).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync();
                    return result.Result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logService.Error("UploadRefundOrder方法失败，原因是API授权失败！请重试。");
                    //GetWMSOauthToken();
                    return (new { isSucc = false, msg = "UploadRefundOrder方法失败，原因是API授权失败！请重试。" }).ToJson();
                }
                else
                {
                    _logService.Error(string.Format("UploadRefundOrder方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, msg = string.Format("UploadRefundOrder方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToJson();
                }
            }

        }
        /// <summary>
        ///  B2C退货单取消上传
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string UnUploadRefundOrder(int id)
        {
            Order order = _omsAccessor.GetById<Order>(id);
            if (order == null)
                return (new { isSuc = false, msg = string.Format("订单ID为{0}的订单信息不存在，请联系管理员！", id) }).ToJson();

            var post = new
            {
                OMSSerialNumber = order.SerialNumber,
                UploadBy = _workContext.CurrentUser.UserName
            };

            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                string jsonStr = post.ToJson();
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wms/order/OMSUnUploadRefundOrder";
                #region JWTBearer授权信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                #endregion
                var response = http.PostAsync(requestUrl, content).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync();
                    return result.Result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logService.Error("UnUploadRefundOrderB2C退货单取消上传方法失败，原因是API授权失败！请重试。");
                    //GetWMSOauthToken();
                    return (new { isSucc = false, msg = "UnUploadRefundOrderB2C退货单取消上传方法失败，原因是API授权失败！请重试。" }).ToJson();
                }
                else
                {
                    _logService.Error(string.Format("UnUploadRefundOrderB2C退货单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, msg = string.Format("UploadRefundOrderB2C退货单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToJson();
                }
            }
        }

        /// <summary>
        /// 上传B2B退单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string UploadB2BRefundOrder(int id)
        {
            Order order = _omsAccessor.Get<Order>()
            .Include(o => o.OrderProduct)
            .Include(r => r.Delivery)
            .Where(o => o.Isvalid && o.Id == id).FirstOrDefault();

            if (order == null)
                return (new { isSucc = false, msg = "发生错误，该订单信息有误！" }).ToJson();

            //if (order.DeliveryTypeId > 0)
            //{
            //    var delivery = _omsAccessor.Get<Delivery>().Where(x => x.Isvalid && x.Id == order.DeliveryTypeId).FirstOrDefault();
            //    if (delivery != null)
            //        order.Delivery = delivery;
            //    else
            //        return (new { isSucc = false, msg = string.Format("发生错误，该订单物流方式（DeliveryId：{0}）有误！", order.DeliveryTypeId) }).ToJson();
            //}
            //else
            //{
            //    return (new { isSucc = false, msg = "发生错误，该订单没有选择物流方式！" }).ToJson();
            //}
            if (order.WarehouseId > 0)
            {
                var warehouse = _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.Id == order.WarehouseId).FirstOrDefault();
                if (warehouse != null)
                    order.WareHouse = warehouse;
                else
                    return (new { isSucc = false, msg = string.Format("发生错误，该订单选择的仓库（warehouseId:{0}）不存在！", order.WarehouseId) }).ToJson();
            }
            else
            {
                return (new { isSucc = false, msg = "发生错误，该订单没有选择仓库！" }).ToJson();
            }

            order.OrderProduct.ForEach(op =>
            {
                op.SaleProduct = _omsAccessor.Get<SaleProduct>(sp => sp.Id == op.SaleProductId).FirstOrDefault();
            });

            order.OrderProduct = order.OrderProduct.Where(x => x.Isvalid).ToList();
            var post = new
            {
                Type = OrderType.B2B_TH,
                ShopId = order.ShopId,
                OMSSerialNumber = order.SerialNumber,
                OrderState = order.State,
                PayType = order.PayType,
                WriteBackState = order.WriteBackState,
                IsLocked = order.IsLocked,
                DeliveryTypeId = order.DeliveryTypeId,
                DeliveryNumber = order.DeliveryNumber,
                DeliveryDate = order.DeliveryDate,
                DeliveryCode = order.Delivery.Code,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                AddressDetail = order.AddressDetail,
                DistrictId = order.DistrictId,
                CustomerMark = order.CustomerMark,
                InvoiceType = order.InvoiceType,
                InvoiceMode = order.InvoiceMode,
                AdminMark = order.AdminMark,
                ToWarehouseMessage = order.ToWarehouseMessage,
                WarehouseId = order.WarehouseId,
                WarehouseCode = order.WareHouse.Code,
                OrderTime = order.CreatedTime,
                InvoiceInfo = order.InvoiceInfo != null ?
                new
                {
                    order.InvoiceInfo.CustomerEmail,
                    order.InvoiceInfo.Title,
                    order.InvoiceInfo.TaxpayerID,
                    order.InvoiceInfo.RegisterAddress,
                    order.InvoiceInfo.RegisterTel,
                    order.InvoiceInfo.BankOfDeposit,
                    order.InvoiceInfo.BankAccount,
                    order.InvoiceInfo.BankCode,
                    order.InvoiceInfo.InvoiceNo,
                }
                : null,
                OrderProduct = order.OrderProduct != null ?
                order.OrderProduct.Select(p => new
                {
                    OMSProductId = p.SaleProduct.ProductId,
                    p.Quantity,
                    p.OrginPrice,
                    p.Price,
                    p.SumPrice,
                    p.Type
                })
                .ToList()
                : null,
                UploadBy = _workContext.CurrentUser.UserName
            };

            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                string jsonStr = post.ToJson();
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                #region JWTBearer授权校验信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                #endregion
                string url = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wms/order/OMSPushB2BRefundOrder";
                var response = http.PostAsync(url, content).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync();
                    return result.Result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logService.Error("UploadB2BRefundOrder上传B2B退单方法失败，原因是API授权失败！请重试。");
                    //GetWMSOauthToken();
                    return (new { isSucc = false, msg = "UploadB2BRefundOrder上传B2B退单方法失败，原因是API授权失败！请重试。" }).ToJson();
                }
                else
                {
                    _logService.Error(string.Format("UploadB2BRefundOrder上传B2B退单方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, msg = string.Format("UploadB2BRefundOrder上传B2B退单方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToJson();
                }
            }
        }
        /// <summary>
        /// 取消上传B2C订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string CancelUploadOrder(int id)
        {
            Order order = _omsAccessor.Get<Order>().Where(r => r.Isvalid && r.Id == id).FirstOrDefault();

            if (order == null)
                return (new { isSucc = false, msg = "发生错误，该订单信息有误！" }).ToJson();

            var post = new
            {
                OMSSerialNumber = order.SerialNumber,
                UploadBy = _workContext.CurrentUser.UserName
            };
            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                string jsonStr = post.ToJson();
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wms/order/OMSCancelUploadOrder";
                #region JWTBearer授权信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                #endregion
                var response = http.PostAsync(requestUrl, content).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync();
                    return result.Result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logService.Error("CancelUploadOrder订单取消上传方法失败，原因是API授权失败！请重试。");
                    return (new { isSucc = false, isOut = false, msg = "CancelUploadOrder订单取消上传方法失败，原因是API授权失败！请重试。" }).ToJson();
                }
                else
                {
                    _logService.Error(string.Format("CancelUploadOrder订单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, isOut = false, msg = string.Format("CancelUploadOrder订单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToJson();
                }
            }
        }
        /// <summary>
        /// 根据条件获取订单
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExportOrderModel> GetAllOrdersByCommand(DateTime? startTime, DateTime? endTime, DateTime? payStartTime, DateTime? payEndTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, OrderState? orderState, string search, bool isOrderDetail, bool isRefundOrder)
        {

            DateTime? dateTime = new DateTime();
            if (startTime.Equals(dateTime))
            {
                startTime = null;
            }
            if (endTime.Equals(dateTime))
            {
                endTime = null;
            }

            if (payStartTime.Equals(dateTime))
            {
                payStartTime = null;
            }
            if (payEndTime.Equals(dateTime))
            {
                payEndTime = null;
            }
            if (deliverStartTime.Equals(dateTime))
            {
                deliverStartTime = null;
            }
            if (deliverEndTime.Equals(dateTime))
            {
                deliverEndTime = null;
            }
            Dictionary<InvoiceType, string> InvoiceTypeName = new Dictionary<InvoiceType, string>
            {
                { InvoiceType.NoNeedInvoice,"无发票"},
                { InvoiceType.PersonalInvoice,"个人发票"},
                { InvoiceType.CompanyInvoice,"普通单位发票"},
                { InvoiceType.SpecialInvoice,"专用发票"}
            };

            var orderStates = new List<int>();
            if (!orderState.HasValue || orderState == OrderState.LackStock)
            {
                orderStates = null;
            }
            else if (orderState == OrderState.Unshipped)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.Confirmed);
                orderStates.Add((int)OrderState.Invalid);
                orderStates.Add((int)OrderState.Uploaded);
            }
            else if (orderState == OrderState.NoConfirm)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.Invalid);
            }
            else if (orderState == OrderState.NoUpload)
            {
                orderStates.Add((int)OrderState.Unpaid);
                orderStates.Add((int)OrderState.Paid);
                orderStates.Add((int)OrderState.B2CConfirmed);
                orderStates.Add((int)OrderState.Invalid);
            }
            else
                orderStates.Add((int)orderState);

            //可显示缺货订单信息状态
            var canOrderStates = new OrderState[] {
                OrderState.B2CConfirmed,
                //OrderState.CheckAccept,
                OrderState.Confirmed,
                OrderState.FinancialConfirmation,
                OrderState.Paid,
                OrderState.ToBeConfirmed,
                OrderState.Unpaid,
                OrderState.ToBeTurned,
                OrderState.Unshipped
            };
            var OrderStateName = GetOrderStateStr();

            var data = new List<ExportOrderModel>();

            if (isOrderDetail)
            {
                data = (from o in _omsAccessor.Get<Order>()
                        where isRefundOrder ? (new OrderType?[] { OrderType.B2C_TH }).Contains(o.Type) : (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_XH }).Contains(o.Type)
                        where (string.IsNullOrEmpty(search) || o.SerialNumber.Contains(search) || o.PSerialNumber.Contains(search) || o.OrgionSerialNumber.Contains(search) ||
                        o.DeliveryNumber.Contains(search) || o.CustomerName.Contains(search) || o.CustomerPhone.Contains(search) || o.AdminMark.Contains(search) || o.CustomerMark.Contains(search))
                        && ((!startTime.HasValue || o.CreatedTime >= startTime.Value) && (!endTime.HasValue || o.CreatedTime <= endTime.Value))
                        && ((!payStartTime.HasValue || o.PayDate >= payStartTime.Value) && (!payEndTime.HasValue || o.PayDate <= payEndTime.Value))
                        && ((!deliverStartTime.HasValue || o.DeliveryDate >= deliverStartTime.Value) && (!deliverEndTime.HasValue || o.DeliveryDate <= deliverEndTime.Value))
                        && (!shopId.HasValue || o.ShopId == shopId) && (!orderState.HasValue || orderStates.Contains((int)o.State) || orderState == OrderState.LackStock || o.State == orderState)
                        join d in _omsAccessor.Get<Dictionary>() on o.ShopId equals d.Id into odl
                        from od in odl.DefaultIfEmpty()
                        join i in _omsAccessor.Get<InvoiceInfo>(r => r.Isvalid) on o.Id equals i.OrderId into del
                        from de in del.DefaultIfEmpty()
                        join wh in _omsAccessor.Get<WareHouse>(r => r.Isvalid) on o.WarehouseId equals wh.Id into whol
                        from who in whol.DefaultIfEmpty()
                        join op in _omsAccessor.Get<OrderProduct>(r => r.Isvalid) on o.Id equals op.OrderId into opol
                        from opo in opol.DefaultIfEmpty()
                        join sp in _omsAccessor.Get<SaleProduct>(r => r.Isvalid) on opo.SaleProductId equals sp.Id into spol
                        from spo in spol.DefaultIfEmpty()
                        join p in _omsAccessor.Get<Product>(r => r.Isvalid) on spo.ProductId equals p.Id into spopl
                        from spop in spopl.DefaultIfEmpty()
                        select new ExportOrderModel
                        {
                            Id = o.Id,
                            CreatedTime = string.Format("{0:G}", o.CreatedTime),
                            SortTime = o.CreatedTime,
                            SerialNumber = o.SerialNumber,
                            ShopName = od.Value,
                            UserName = o.UserName,
                            CustomerName = o.CustomerName,
                            CustomerPhone = o.CustomerPhone,
                            Address = o.AddressDetail,
                            SumOrginPrice = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == o.Id).Sum(x => x.Quantity * x.OrginPrice * (isRefundOrder ? -1 : 1)).ToString(),
                            SumAvgPrice = o.SumPrice * (isRefundOrder ? -1 : 1),
                            PayPrice = o.PayPrice * (isRefundOrder ? -1 : 1),
                            PayDate = string.Format("{0:G}", o.PayDate),
                            IntegralValue = o.ZMIntegralValuePrice.HasValue ? o.ZMIntegralValuePrice.Value.ToString() : "0",
                            ProductCoupon = o.ProductCoupon,
                            PSerialNumber = o.PSerialNumber,
                            OrgionSerialNumber = o.OrgionSerialNumber,
                            PayTypeName = (o.PayType == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == o.PayType).FirstOrDefault().Value),
                            PayStateName = o.PayState.ToString() == "Success" ? "已付款" : "未付款",
                            DeliveryTypeName = o.DeliveryTypeId == 0 ? "" : _omsAccessor.Get<Delivery>().Where(x => x.Isvalid && x.Id == o.DeliveryTypeId).FirstOrDefault().Name,
                            DeliveryNumber = o.DeliveryNumber,
                            InvoiceTypeName = InvoiceTypeName[o.InvoiceType],
                            InvoiceNumber = de.TaxpayerID,
                            InvoiceHead = de.Title,
                            StateName = OrderStateName[o.State],
                            AdminMark = o.AdminMark,
                            OrderMark = o.CustomerMark,
                            ToWareHouseMark = o.ToWarehouseMessage,
                            ProductName = spop.Name,
                            ProductCode = spop.Code,
                            OrginPrice = opo != null ? opo.OrginPrice * (isRefundOrder ? -1 : 1) : 0,
                            UnitPrice = opo != null ? opo.Price * (isRefundOrder ? -1 : 1) : 0,
                            Quantity = opo != null ? opo.Quantity * (isRefundOrder ? -1 : 1) : 0,
                            WareHouseName = who != null ? who.Name : "",
                            AvgPrice = opo != null ? (opo.Price * opo.Quantity) * (isRefundOrder ? -1 : 1) : 0,
                            PayMoneyPrice = opo != null ? opo.SumPrice * (isRefundOrder ? -1 : 1) : 0,
                            DeliveryDate = string.Format("{0:G}", o.DeliveryDate),
                            IsLackStock = !canOrderStates.Contains(o.State) ? false : ((from op in _omsAccessor.Get<OrderProduct>().Where(x => x.OrderId == o.Id && x.Isvalid)
                                                    join spl in _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == o.WarehouseId) on op.Id equals spl.OrderProductId
                                                    where op.Quantity == spl.LockNumber
                                                   select op).Count()!=_omsAccessor.Get<OrderProduct>().Where(x=>x.Isvalid && x.OrderId == o.Id).Count() ? true : false),
                        }).OrderByDescending(r => r.SortTime).Distinct().ToList();
            }

            else
            {
                data = (from o in _omsAccessor.Get<Order>()
                        where isRefundOrder ? (new OrderType?[] { OrderType.B2C_TH }).Contains(o.Type) : (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_XH }).Contains(o.Type)
                        where (string.IsNullOrEmpty(search) || o.SerialNumber.Contains(search) || o.PSerialNumber.Contains(search) || o.OrgionSerialNumber.Contains(search) ||
                        o.DeliveryNumber.Contains(search) || o.CustomerName.Contains(search) || o.CustomerPhone.Contains(search) || o.AdminMark.Contains(search) || o.CustomerMark.Contains(search))
                        && ((!startTime.HasValue || o.CreatedTime >= startTime.Value) && (!endTime.HasValue || o.CreatedTime <= endTime.Value))
                        && ((!payStartTime.HasValue || o.PayDate >= payStartTime.Value) && (!payEndTime.HasValue || o.PayDate <= payEndTime.Value))
                        && ((!deliverStartTime.HasValue || o.DeliveryDate >= deliverStartTime.Value) && (!deliverEndTime.HasValue || o.DeliveryDate <= deliverEndTime.Value))
                        && (!shopId.HasValue || o.ShopId == shopId) && (!orderState.HasValue || orderStates.Contains((int)o.State) || orderState == OrderState.LackStock || o.State == orderState)
                        join d in _omsAccessor.Get<Dictionary>() on o.ShopId equals d.Id into odl
                        from od in odl.DefaultIfEmpty()
                        join i in _omsAccessor.Get<InvoiceInfo>(r => r.Isvalid) on o.Id equals i.OrderId into del
                        from de in del.DefaultIfEmpty()
                        select new ExportOrderModel
                        {
                            Id = o.Id,
                            CreatedTime = string.Format("{0:G}", o.CreatedTime),
                            SortTime = o.CreatedTime,
                            SerialNumber = o.SerialNumber,
                            ShopName = od != null ? od.Value : "",
                            UserName = o.UserName,
                            CustomerName = o.CustomerName,
                            CustomerPhone = o.CustomerPhone,
                            Address = o.AddressDetail,
                            SumOrginPrice = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == o.Id).Sum(x => x.Quantity * x.OrginPrice * (isRefundOrder ? -1 : 1)).ToString(),
                            SumAvgPrice = o.SumPrice * (isRefundOrder ? -1 : 1),
                            PayPrice = o.PayPrice * (isRefundOrder ? -1 : 1),
                            PayDate = string.Format("{0:G}", o.PayDate),
                            IntegralValue = o.ZMIntegralValuePrice.HasValue ? o.ZMIntegralValuePrice.Value.ToString() : "0",
                            ProductCoupon = o.ProductCoupon,
                            PSerialNumber = o.PSerialNumber,
                            OrgionSerialNumber = o.OrgionSerialNumber,
                            PayTypeName = (o.PayType == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == o.PayType).FirstOrDefault().Value),
                            PayStateName = o.PayState.ToString() == "Success" ? "已付款" : "未付款",
                            DeliveryTypeName = o.DeliveryTypeId == 0 ? "" : _omsAccessor.Get<Delivery>().Where(x => x.Isvalid && x.Id == o.DeliveryTypeId).FirstOrDefault().Name,
                            DeliveryNumber = o.DeliveryNumber,
                            InvoiceTypeName = InvoiceTypeName[o.InvoiceType],
                            InvoiceNumber = de != null ? de.TaxpayerID : "",
                            InvoiceHead = de != null ? de.Title : "",
                            StateName = OrderStateName[o.State],
                            AdminMark = o.AdminMark,
                            OrderMark = o.CustomerMark,
                            ToWareHouseMark = o.ToWarehouseMessage,
                            DeliveryDate = string.Format("{0:G}", o.DeliveryDate),
                            IsLackStock =!canOrderStates.Contains(o.State) ? false : ((from op in _omsAccessor.Get<OrderProduct>().Where(x => x.OrderId == o.Id && x.Isvalid)
                                                    join spl in _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == o.WarehouseId) on op.Id equals spl.OrderProductId
                                                    where op.Quantity == spl.LockNumber
                                                   select op).Count()!=_omsAccessor.Get<OrderProduct>().Where(x=>x.Isvalid && x.OrderId == o.Id).Count() ? true : false),
                        }).OrderByDescending(r => r.SortTime).Distinct().ToList();
            }

            if (orderState == OrderState.LackStock)
                data = data.Where(r => r.IsLackStock).OrderByDescending(r => r.SortTime).ToList();
            return data;
        }
        /// <summary>
        /// 根据条件获取B2B订单
        /// </summary>
        /// <param name="isOrderDetail"></param>
        /// <returns></returns>
        public IEnumerable<ExportOrderModel> GetAllB2BOrdersByCommand(SearchB2BOrderModel searchB2BOrderModel, bool isOrderDetail)
        {

            searchB2BOrderModel.EndTime = searchB2BOrderModel.EndTime?.AddDays(1);
            searchB2BOrderModel.DeliverEndTime = searchB2BOrderModel.DeliverEndTime?.AddDays(1);

            DateTime? dateTime = new DateTime();
            if (searchB2BOrderModel.StartTime.Equals(dateTime))
            {
                searchB2BOrderModel.StartTime = null;
            }
            if (searchB2BOrderModel.EndTime.Equals(dateTime))
            {
                searchB2BOrderModel.EndTime = null;
            }
            Dictionary<InvoiceType, string> InvoiceTypeName = new Dictionary<InvoiceType, string>
            {
                { InvoiceType.NoNeedInvoice,"无发票"},
                { InvoiceType.PersonalInvoice,"个人发票"},
                { InvoiceType.CompanyInvoice,"普通单位发票"},
                { InvoiceType.SpecialInvoice,"专用发票"}
            };

            //可显示缺货订单信息状态
            var canOrderStates = new OrderState[] {
                OrderState.B2CConfirmed,
                //OrderState.CheckAccept,
                OrderState.Confirmed,
                OrderState.FinancialConfirmation,
                OrderState.Paid,
                OrderState.ToBeConfirmed,
                OrderState.Unpaid,
                OrderState.ToBeTurned,
                OrderState.Unshipped
            };
            var orderStates = new List<int>();

            var OrderStateName = GetOrderStateStr();

            var data = new List<ExportOrderModel>();

            var search = searchB2BOrderModel.SearchStr;

            var startTime = searchB2BOrderModel.StartTime;

            var endTime = searchB2BOrderModel.EndTime;

            var shopId = searchB2BOrderModel.ShopId;

            var deliverStartTime = searchB2BOrderModel.DeliverStartTime;

            var deliverEndTime = searchB2BOrderModel.DeliverEndTime;


            var orderState = searchB2BOrderModel.OrderState;
            if (isOrderDetail)
            {
                data = (from o in _omsAccessor.Get<Order>()
                        where searchB2BOrderModel.IsRefundOrder ? (new OrderType?[] { OrderType.B2B_TH }).Contains(o.Type) : (new OrderType?[] { OrderType.B2B }).Contains(o.Type)
                        where (string.IsNullOrEmpty(search) || o.SerialNumber.Contains(search) || o.PSerialNumber.Contains(search) || o.OrgionSerialNumber.Contains(search) ||
                        o.DeliveryNumber.Contains(search) || o.CustomerName.Contains(search) || o.CustomerPhone.Contains(search) || o.AdminMark.Contains(search) || o.CustomerMark.Contains(search))
                        && (!startTime.HasValue || o.CreatedTime >= startTime.Value) && (!endTime.HasValue || o.CreatedTime <= endTime.Value)
                        && (!shopId.HasValue || o.ShopId == shopId) && (!orderState.HasValue || orderStates.Contains((int)o.State) || orderState == OrderState.LackStock || o.State == orderState)
                        && (!searchB2BOrderModel.MinPrice.HasValue || o.SumPrice >= searchB2BOrderModel.MinPrice) && (!searchB2BOrderModel.MaxPrice.HasValue || o.SumPrice <= searchB2BOrderModel.MaxPrice)
                        && (!searchB2BOrderModel.CustomerId.HasValue || o.CustomerId == searchB2BOrderModel.CustomerId) && (!searchB2BOrderModel.SalesManId.HasValue || o.SalesManId == searchB2BOrderModel.SalesManId) && (!searchB2BOrderModel.WareHouseId.HasValue || o.WarehouseId == searchB2BOrderModel.WareHouseId)
                        && (!deliverStartTime.HasValue || o.DeliveryDate >= deliverStartTime.Value) && (!deliverEndTime.HasValue || o.DeliveryDate <= deliverEndTime.Value)
                        join d in _omsAccessor.Get<Dictionary>() on o.ShopId equals d.Id into odl
                        from od in odl.DefaultIfEmpty()
                        join i in _omsAccessor.Get<InvoiceInfo>(r => r.Isvalid) on o.Id equals i.OrderId into del
                        from de in del.DefaultIfEmpty()
                        join wh in _omsAccessor.Get<WareHouse>(r => r.Isvalid) on o.WarehouseId equals wh.Id into whol
                        from who in whol.DefaultIfEmpty()
                        join op in _omsAccessor.Get<OrderProduct>(r => r.Isvalid) on o.Id equals op.OrderId into opol
                        from opo in opol.DefaultIfEmpty()
                        join sp in _omsAccessor.Get<SaleProduct>(r => r.Isvalid) on opo.SaleProductId equals sp.Id into spol
                        from spo in spol.DefaultIfEmpty()
                        join p in _omsAccessor.Get<Product>(r => r.Isvalid) on spo.ProductId equals p.Id into spopl
                        from spop in spopl.DefaultIfEmpty()
                        join c in _omsAccessor.Get<Customers>(x => x.Isvalid) on o.CustomerId equals c.Id into co
                        from cou in co.DefaultIfEmpty()
                        join s in _omsAccessor.Get<SalesMan>(x => x.Isvalid) on o.SalesManId equals s.Id into sa
                        from sal in sa.DefaultIfEmpty()
                        select new ExportOrderModel
                        {
                            Id = o.Id,
                            CreatedTime = string.Format("{0:G}", o.CreatedTime),
                            SortTime = o.CreatedTime,
                            SerialNumber = o.SerialNumber,
                            ShopName = od.Value,
                            UserName = o.UserName,
                            CustomerName = o.CustomerName,
                            CustomerPhone = o.CustomerPhone,
                            Address = o.AddressDetail,
                            SumOrginPrice = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == o.Id).Sum(x => x.Quantity * x.OrginPrice * (searchB2BOrderModel.IsRefundOrder ? -1 : 1)).ToString(),
                            SumAvgPrice = o.SumPrice * (searchB2BOrderModel.IsRefundOrder ? -1 : 1),
                            PayPrice = o.PayPrice * (searchB2BOrderModel.IsRefundOrder ? -1 : 1),
                            PayDate = string.Format("{0:G}", o.PayDate),
                            IntegralValue = o.ZMIntegralValuePrice.HasValue ? o.ZMIntegralValuePrice.Value.ToString() : "0",
                            ProductCoupon = o.ProductCoupon,
                            PSerialNumber = o.PSerialNumber,
                            OrgionSerialNumber = o.OrgionSerialNumber,
                            PayTypeName = o.PayType == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == o.PayType).FirstOrDefault().Value,
                            PayStateName = o.PayState.ToString() == "Success" ? "已付款" : "未付款",
                            DeliveryTypeName = o.DeliveryTypeId == 0 ? "" : _omsAccessor.Get<Delivery>().Where(x => x.Isvalid && x.Id == o.DeliveryTypeId).FirstOrDefault().Name,
                            DeliveryNumber = o.DeliveryNumber,
                            InvoiceTypeName = InvoiceTypeName[o.InvoiceType],
                            InvoiceNumber = de.TaxpayerID,
                            InvoiceHead = de.Title,
                            StateName = OrderStateName[o.State],
                            AdminMark = o.AdminMark,
                            OrderMark = o.CustomerMark,
                            ToWareHouseMark = o.ToWarehouseMessage,
                            ProductName = spop.Name,
                            ProductCode = spop.Code,
                            OrginPrice = opo != null ? opo.OrginPrice * (searchB2BOrderModel.IsRefundOrder ? -1 : 1) : 0,
                            UnitPrice = opo != null ? opo.Price * (searchB2BOrderModel.IsRefundOrder ? -1 : 1) : 0,
                            Quantity = opo != null ? opo.Quantity * (searchB2BOrderModel.IsRefundOrder ? -1 : 1) : 0,
                            WareHouseName = who != null ? who.Name : "",
                            AvgPrice = opo != null ? (opo.Price * opo.Quantity) * (searchB2BOrderModel.IsRefundOrder ? -1 : 1) : 0,
                            PayMoneyPrice = opo != null ? opo.SumPrice : 0,
                            ClientName = cou.Name,
                            SalesManName = sal.UserName,
                            CustomerTypeId = cou != null ? cou.CustomerTypeId : 0,
                            IsBookKeepingStr = GetBookKeepingStr(o),
                            UserTypeName = cou.CustomerTypeId == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == cou.CustomerTypeId).FirstOrDefault().Value,
                            DeliveryDate = string.Format("{0:G}", o.DeliveryDate),
                            IsLackStock =!canOrderStates.Contains(o.State) ? false : ((from op in _omsAccessor.Get<OrderProduct>().Where(x => x.OrderId == o.Id && x.Isvalid)
                                                    join spl in _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == o.WarehouseId) on op.Id equals spl.OrderProductId
                                                    where op.Quantity == spl.LockNumber
                                                   select op).Count()!=_omsAccessor.Get<OrderProduct>().Where(x=>x.Isvalid && x.OrderId == o.Id).Count() ? true : false),
                        }).OrderByDescending(r => r.SortTime).Distinct().ToList();
            }

            else
            {
                data = (from o in _omsAccessor.Get<Order>()
                        where searchB2BOrderModel.IsRefundOrder ? (new OrderType?[] { OrderType.B2B_TH }).Contains(o.Type) : (new OrderType?[] { OrderType.B2B }).Contains(o.Type)
                        where (string.IsNullOrEmpty(search) || o.SerialNumber.Contains(search) || o.PSerialNumber.Contains(search) || o.OrgionSerialNumber.Contains(search) ||
                        o.DeliveryNumber.Contains(search) || o.CustomerName.Contains(search) || o.CustomerPhone.Contains(search) || o.AdminMark.Contains(search) || o.CustomerMark.Contains(search))
                        && (!startTime.HasValue || o.CreatedTime >= startTime.Value) && (!endTime.HasValue || o.CreatedTime <= endTime.Value)
                        && (!shopId.HasValue || o.ShopId == shopId) && (!orderState.HasValue || orderStates.Contains((int)o.State) || orderState == OrderState.LackStock || o.State == orderState)
                        && (!searchB2BOrderModel.MinPrice.HasValue || o.SumPrice >= searchB2BOrderModel.MinPrice) && (!searchB2BOrderModel.MaxPrice.HasValue || o.SumPrice <= searchB2BOrderModel.MaxPrice)
                        && (!searchB2BOrderModel.CustomerId.HasValue || o.CustomerId == searchB2BOrderModel.CustomerId) && (!searchB2BOrderModel.SalesManId.HasValue || o.SalesManId == searchB2BOrderModel.SalesManId) && (!searchB2BOrderModel.WareHouseId.HasValue || o.WarehouseId == searchB2BOrderModel.WareHouseId)
                        && (!deliverStartTime.HasValue || o.DeliveryDate >= deliverStartTime.Value) && (!deliverEndTime.HasValue || o.DeliveryDate <= deliverEndTime.Value)
                        join d in _omsAccessor.Get<Dictionary>() on o.ShopId equals d.Id into odl
                        from od in odl.DefaultIfEmpty()
                        join i in _omsAccessor.Get<InvoiceInfo>(r => r.Isvalid) on o.Id equals i.OrderId into del
                        from de in del.DefaultIfEmpty()
                        join c in _omsAccessor.Get<Customers>(x => x.Isvalid) on o.CustomerId equals c.Id into co
                        from cou in co.DefaultIfEmpty()
                        join s in _omsAccessor.Get<SalesMan>(x => x.Isvalid) on o.SalesManId equals s.Id into sa
                        from sal in sa.DefaultIfEmpty()
                        select new ExportOrderModel
                        {
                            Id = o.Id,
                            CreatedTime = string.Format("{0:G}", o.CreatedTime),
                            SortTime = o.CreatedTime,
                            SerialNumber = o.SerialNumber,
                            ShopName = od != null ? od.Value : "",
                            UserName = o.UserName,
                            CustomerName = o.CustomerName,
                            CustomerPhone = o.CustomerPhone,
                            Address = o.AddressDetail,
                            SumOrginPrice = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == o.Id).Sum(x => x.Quantity * x.OrginPrice * (searchB2BOrderModel.IsRefundOrder ? -1 : 1)).ToString(),
                            SumAvgPrice = o.SumPrice * (searchB2BOrderModel.IsRefundOrder ? -1 : 1),
                            PayPrice = o.PayPrice * (searchB2BOrderModel.IsRefundOrder ? -1 : 1),
                            PayDate = string.Format("{0:G}", o.PayDate),
                            IntegralValue = o.ZMIntegralValuePrice.HasValue ? o.ZMIntegralValuePrice.Value.ToString() : "0",
                            ProductCoupon = o.ProductCoupon,
                            PSerialNumber = o.PSerialNumber,
                            OrgionSerialNumber = o.OrgionSerialNumber,
                            PayTypeName = (o.PayType == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == o.PayType).FirstOrDefault().Value),
                            PayStateName = o.PayState.ToString() == "Success" ? "已付款" : "未付款",
                            DeliveryTypeName = o.DeliveryTypeId == 0 ? "" : _omsAccessor.Get<Delivery>().Where(x => x.Isvalid && x.Id == o.DeliveryTypeId).FirstOrDefault().Name,
                            DeliveryNumber = o.DeliveryNumber,
                            InvoiceTypeName = InvoiceTypeName[o.InvoiceType],
                            InvoiceNumber = de != null ? de.TaxpayerID : "",
                            InvoiceHead = de != null ? de.Title : "",
                            StateName = OrderStateName[o.State],
                            AdminMark = o.AdminMark,
                            OrderMark = o.CustomerMark,
                            ToWareHouseMark = o.ToWarehouseMessage,
                            ClientName = cou.Name,
                            SalesManName = sal.UserName,
                            CustomerTypeId = cou != null ? cou.CustomerTypeId : 0,
                            IsBookKeepingStr = GetBookKeepingStr(o),
                            UserTypeName = cou.CustomerTypeId == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == cou.CustomerTypeId).FirstOrDefault().Value,
                            DeliveryDate = string.Format("{0:G}", o.DeliveryDate),
                            IsLackStock =!canOrderStates.Contains(o.State) ? false : ((from op in _omsAccessor.Get<OrderProduct>().Where(x => x.OrderId == o.Id && x.Isvalid)
                                                    join spl in _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.WareHouseId == o.WarehouseId) on op.Id equals spl.OrderProductId
                                                    where op.Quantity == spl.LockNumber
                                                   select op).Count()!=_omsAccessor.Get<OrderProduct>().Where(x=>x.Isvalid && x.OrderId == o.Id).Count() ? true : false),
                        }).OrderByDescending(r => r.SortTime).Distinct().ToList();
            }

            if (orderState == OrderState.LackStock)
                data = data.Where(r => r.IsLackStock).OrderByDescending(r => r.SortTime).ToList();

            if (searchB2BOrderModel.CustomerTypeId.HasValue)
                data = data.Where(r => r.CustomerTypeId == searchB2BOrderModel.CustomerTypeId).ToList();
            if (searchB2BOrderModel.BookKeepType.HasValue)
            {
                if (searchB2BOrderModel.BookKeepType.Value == 1)
                    data = data.Where(r => r.IsBookKeepingStr == "已付款").ToList();
                else if (searchB2BOrderModel.BookKeepType.Value == 2)
                    data = data.Where(r => r.IsBookKeepingStr == "部分付款").ToList();
                else if (searchB2BOrderModel.BookKeepType.Value == 3)
                    data = data.Where(r => r.IsBookKeepingStr == "未付款").ToList();
            }
            return data;
        }

        /// <summary>
        /// 分页获取零售销货分析数据
        /// </summary>
        /// <returns></returns>
        public PageList<RetailSalesModel> GetRetailSalesModelsByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            var result = (from op in _omsAccessor.Get<OrderProduct>()
                          join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                          join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                          from osp in ospl.DefaultIfEmpty()
                          join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                          from ospp in osppl.DefaultIfEmpty()
                          join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into sopl
                          from sop in sopl.DefaultIfEmpty()
                          join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into wol
                          from wo in wol.DefaultIfEmpty()
                          where op.Isvalid && o.Isvalid && osp.Isvalid && ospp.Isvalid && sop.Isvalid && wo.Isvalid
                          orderby o.CreatedTime descending
                          select new RetailSalesModel
                          {
                              Id = op.Id,
                              OrderId = o.Id,
                              OrderSerialNumber = o.SerialNumber,
                              TypeName = o.SerialNumber.StartsWith("RF") ? "零售退货单" : "零售销货单",
                              IsRefundOrder = o.SerialNumber.StartsWith("RF") ? true : false,
                              DateTime = o.CreatedTime,
                              ShopId = o.ShopId,
                              ShopName = sop == null ? "" : sop.Value,
                              WareHouseId = o.WarehouseId,
                              WareHouseName = wo == null ? "" : wo.Name,
                              ProductSku = ospp.Code,
                              DeputyBarcode = ospp.DeputyBarcode,
                              ProductName = ospp.Name,
                              OriginalPrice = op.OrginPrice,
                              SalePrice = op.Price,
                              Quantity = op.Quantity,
                              SumPrice = op.SumPrice
                          }).ToList();

            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(o => o.DateTime >= startTime.Value).ToList();
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(o => o.DateTime < endTime.Value).ToList();
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId.Value).ToList();
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId.Value).ToList();
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductSku.Contains(search) || r.OrderSerialNumber.Contains(search) || r.DeputyBarcode.Contains(search)).ToList();
            if (orderType == 1)
                result = result.Where(r => !r.OrderSerialNumber.StartsWith("RF")).ToList();
            else if (orderType == 2)
                result = result.Where(r => r.OrderSerialNumber.StartsWith("RF")).ToList();

            return new PageList<RetailSalesModel>(result, pageIndex, pageSize);
        }


        /// <summary>
        /// 按要求获取所有零售销货分析数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RetailSalesModel> GetAllExportDataByCommand(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            var result = (from op in _omsAccessor.Get<OrderProduct>()
                          join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                          join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                          from osp in ospl.DefaultIfEmpty()
                          join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                          from ospp in osppl.DefaultIfEmpty()
                          join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into sopl
                          from sop in sopl.DefaultIfEmpty()
                          join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into wol
                          from wo in wol.DefaultIfEmpty()
                          where op.Isvalid && o.Isvalid && osp.Isvalid && ospp.Isvalid && sop.Isvalid && wo.Isvalid
                          orderby o.CreatedTime descending
                          select new RetailSalesModel
                          {
                              Id = op.Id,
                              OrderId = o.Id,
                              OrderSerialNumber = o.SerialNumber,
                              TypeName = o.SerialNumber.StartsWith("RF") ? "零售退货单" : "零售销货单",
                              IsRefundOrder = o.SerialNumber.StartsWith("RF") ? true : false,
                              DateTime = o.CreatedTime,
                              ShopId = o.ShopId,
                              ShopName = sop == null ? "" : sop.Value,
                              WareHouseId = o.WarehouseId,
                              WareHouseName = wo == null ? "" : wo.Name,
                              ProductSku = ospp.Code,
                              DeputyBarcode = ospp.DeputyBarcode,
                              ProductName = ospp.Name,
                              OriginalPrice = op.OrginPrice,
                              SalePrice = op.Price,
                              Quantity = op.Quantity,
                              SumPrice = op.SumPrice
                          });

            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(o => o.DateTime >= startTime.Value);
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(o => o.DateTime < endTime.Value);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId.Value);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId.Value);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductSku.Contains(search) || r.OrderSerialNumber.Contains(search) || r.DeputyBarcode.Contains(search));
            if (orderType == 1)
                result = result.Where(r => !r.OrderSerialNumber.StartsWith("RF"));
            else if (orderType == 2)
                result = result.Where(r => r.OrderSerialNumber.StartsWith("RF"));
            return result;
        }

        /// <summary>
        /// 分页获取订单发货统计分析
        /// </summary>
        /// <returns></returns>
        public PageList<OrderDeliveryModel> GetOrderDeliveryModelsByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? payType, int? deliveryId, int dateType, out int orderCount, out int productCount, out decimal avgSumPrice, out decimal deliverySumPrice, string search = "")
        {
            var result = from o in _omsAccessor.Get<Order>()
                         join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                         from od in odl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join pt in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.PayType) on o.PayType equals pt.Id into optl
                         from opt in optl.DefaultIfEmpty()
                         where o.Isvalid && (o.State == OrderState.Delivered || o.State == OrderState.Finished)
                         && od.Isvalid && opt.Isvalid && ow.Isvalid && os.Isvalid
                         orderby o.CreatedTime descending
                         select new OrderDeliveryModel
                         {
                             Id = o.Id,
                             SerialNumber = o.SerialNumber,
                             DeliverDate = o.DeliveryDate,
                             OrderDate = o.CreatedTime,
                             DeliveryId = o.DeliveryTypeId,
                             DeliverName = od == null ? "" : od.Name,
                             InvoiceSerialNumber = "",
                             PayTypeId = o.PayType,
                             PayTypeName = opt == null ? "" : opt.Value,
                             DeliverSerialNumber = o.DeliveryNumber,
                             BusinessSerialNumber = "",
                             AccountName = o.UserName,
                             ReceiveName = o.CustomerName,
                             Address = o.AddressDetail,
                             Phone = o.CustomerPhone,
                             ShopId = o.ShopId,
                             ShopName = os == null ? "" : os.Value,
                             DeliveryCost = 0,
                             Quantity = _omsAccessor.Get<OrderProduct>().Where(r => r.Isvalid && r.OrderId == o.Id).Sum(r => r.Quantity),
                             PayedPrice = o.PayPrice,
                             PayDate = o.PayDate,
                             AverageSumPrice = o.SumPrice,
                             WareHouseId = o.WarehouseId,
                             WareHouseName = ow == null ? "" : ow.Name,
                         };
            if (startTime.HasValue && startTime.Value != null)
            {
                if (dateType == 1)
                {
                    result = result.Where(r => r.OrderDate >= startTime);
                }
                else if (dateType == 2)
                {
                    result = result.Where(r => r.DeliverDate >= startTime);
                }
            }
            if (endTime.HasValue && endTime.Value != null)
            {
                if (dateType == 1)
                {
                    result = result.Where(r => r.OrderDate < endTime);
                }
                else if (dateType == 2)
                {
                    result = result.Where(r => r.DeliverDate < endTime);
                }
            }
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (payType.HasValue)
                result = result.Where(r => r.PayTypeId == payType);
            if (deliveryId.HasValue)
                result = result.Where(r => r.DeliveryId == deliveryId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.SerialNumber.Contains(search) || r.DeliverSerialNumber.Contains(search) || r.ReceiveName.Contains(search) || r.Address.Contains(search));
            orderCount = result.Count();
            productCount = result.Sum(r => r.Quantity);
            avgSumPrice = result.Sum(r => r.AverageSumPrice);
            deliverySumPrice = result.Sum(r => r.DeliveryCost);
            return new PageList<OrderDeliveryModel>(result, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取所有订单发货统计数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OrderDeliveryModel> GetAllExportOrderDeliveryModels(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? payType, int? deliveryId, int dateType, string search = "")
        {
            var result = from o in _omsAccessor.Get<Order>()
                         join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                         from od in odl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join pt in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.PayType) on o.PayType equals pt.Id into optl
                         from opt in optl.DefaultIfEmpty()
                         where o.Isvalid && (o.State == OrderState.Delivered || o.State == OrderState.Finished)
                         && od.Isvalid && opt.Isvalid && ow.Isvalid && os.Isvalid
                         orderby o.CreatedTime descending
                         select new OrderDeliveryModel
                         {
                             Id = o.Id,
                             SerialNumber = o.SerialNumber,
                             DeliverDate = o.DeliveryDate,
                             OrderDate = o.CreatedTime,
                             DeliveryId = o.DeliveryTypeId,
                             DeliverName = od == null ? "" : od.Name,
                             InvoiceSerialNumber = "",
                             PayTypeId = o.PayType,
                             PayTypeName = opt == null ? "" : opt.Value,
                             DeliverSerialNumber = o.DeliveryNumber,
                             BusinessSerialNumber = "",
                             AccountName = o.UserName,
                             ReceiveName = o.CustomerName,
                             Address = o.AddressDetail,
                             Phone = o.CustomerPhone,
                             ShopId = o.ShopId,
                             ShopName = os == null ? "" : os.Value,
                             DeliveryCost = 0,
                             Quantity = _omsAccessor.Get<OrderProduct>().Where(r => r.Isvalid && r.OrderId == o.Id).Sum(r => r.Quantity),
                             PayedPrice = o.PayPrice,
                             PayDate = o.PayDate,
                             AverageSumPrice = o.SumPrice,
                             WareHouseId = o.WarehouseId,
                             WareHouseName = ow == null ? "" : ow.Name,
                         };
            if (startTime.HasValue && startTime.Value != null)
            {
                if (dateType == 1)
                {
                    result = result.Where(r => r.OrderDate >= startTime);
                }
                else if (dateType == 2)
                {
                    result = result.Where(r => r.DeliverDate >= startTime);
                }
            }
            if (endTime.HasValue && endTime.Value != null)
            {
                if (dateType == 1)
                {
                    result = result.Where(r => r.OrderDate < endTime);
                }
                else if (dateType == 2)
                {
                    result = result.Where(r => r.DeliverDate < endTime);
                }
            }
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (payType.HasValue)
                result = result.Where(r => r.PayTypeId == payType);
            if (deliveryId.HasValue)
                result = result.Where(r => r.DeliveryId == deliveryId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.SerialNumber.Contains(search) || r.DeliverSerialNumber.Contains(search) || r.ReceiveName.Contains(search) || r.Address.Contains(search));

            return result;
        }

        /// <summary>
        /// 分页获取商品发货统计分析数据
        /// </summary>
        /// <returns></returns>
        public PageList<GoodDeliveryModel> GetOrderDeliverModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? deliveryId, out int orderCount, out int productCount, out decimal avgSumPrice, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                         from od in odl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         where o.Isvalid && (o.State == OrderState.Delivered || o.State == OrderState.Finished)
                         && op.Isvalid && osp.Isvalid && ospp.Isvalid
                         select new
                         {
                             Id = o.Id,
                             ProductId = ospp.Id,
                             ProductName = ospp.Name,
                             ProductCode = ospp.Code,
                             Quantity = op.Quantity,
                             DeliveryId = o.DeliveryTypeId,
                             DeliveryName = od != null ? od.Name : "",
                             AvgSumPrice = op.SumPrice,
                             DeliveryDate = o.DeliveryDate,
                             WareHouseId = o.WarehouseId,
                             ShopId = o.ShopId,
                         };
            if (startTime.HasValue)
                result = result.Where(r => r.DeliveryDate.Value >= startTime);
            if (endTime.HasValue)
                result = result.Where(r => r.DeliveryDate.Value < endTime);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (deliveryId.HasValue)
                result = result.Where(r => r.DeliveryId == deliveryId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search));

            orderCount = result.GroupBy(r => r.Id).Select(t => new { t.Key }).Count();

            var allResult = result.GroupBy(r => new { r.ProductId, r.DeliveryId })
                          .Select(r => new GoodDeliveryModel
                          {
                              ProductId = r.Key.ProductId,
                              ProductName = r.First().ProductName,
                              ProductCode = r.First().ProductCode,
                              DeliveryName = r.First().DeliveryName,
                              Quantity = r.Sum(p => p.Quantity),
                              AvgSumPrice = r.Sum(p => p.AvgSumPrice)
                          }).OrderByDescending(t => t.Quantity);

            orderCount = allResult.Count();
            productCount = allResult.Sum(r => r.Quantity);
            avgSumPrice = allResult.Sum(r => r.AvgSumPrice);
            return new PageList<GoodDeliveryModel>(allResult, pageIndex, pageSize);
        }
        /// <summary>
        /// 获取所有商品发货统计分析数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GoodDeliveryModel> GetAllExportGoodDeliveryModels(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? deliveryId, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                         from od in odl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         where o.Isvalid && (o.State == OrderState.Delivered || o.State == OrderState.Finished)
                         && op.Isvalid && osp.Isvalid && ospp.Isvalid
                         select new
                         {
                             Id = o.Id,
                             ProductId = ospp.Id,
                             ProductName = ospp.Name,
                             ProductCode = ospp.Code,
                             Quantity = op.Quantity,
                             DeliveryId = o.DeliveryTypeId,
                             DeliveryName = od != null ? od.Name : "",
                             AvgSumPrice = op.SumPrice,
                             DeliveryDate = o.DeliveryDate,
                             WareHouseId = o.WarehouseId,
                             ShopId = o.ShopId,
                         };
            if (startTime.HasValue)
                result = result.Where(r => r.DeliveryDate.Value >= startTime);
            if (endTime.HasValue)
                result = result.Where(r => r.DeliveryDate.Value < endTime);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (deliveryId.HasValue)
                result = result.Where(r => r.DeliveryId == deliveryId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search));

            var allResult = result.GroupBy(r => new { r.ProductId, r.DeliveryId })
                          .Select(r => new GoodDeliveryModel
                          {
                              ProductId = r.Key.ProductId,
                              ProductName = r.First().ProductName,
                              ProductCode = r.First().ProductCode,
                              DeliveryName = r.First().DeliveryName,
                              Quantity = r.Sum(p => p.Quantity),
                              AvgSumPrice = r.Sum(p => p.AvgSumPrice)
                          }).OrderByDescending(t => t.Quantity);
            return allResult;
        }

        /// <summary>
        /// 分页获取商店退货分析数据
        /// </summary>
        /// <returns></returns>
        public PageList<ShopRefundOrderModel> GetShopRefundOrderModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, out int productCount, out decimal price, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join ol in _omsAccessor.Get<Order>() on o.OrgionSerialNumber equals ol.SerialNumber into oosl
                         from oos in oosl.DefaultIfEmpty()
                         where op.Isvalid && o.Isvalid && o.SerialNumber.StartsWith("RF")
                         && osp.Isvalid && ospp.Isvalid && ow.Isvalid && os.Isvalid
                         select new ShopRefundOrderModel
                         {
                             ShopId = o.ShopId,
                             ShopName = os != null ? os.Value : "",
                             Quantity = op.Quantity,
                             Date = o.CreatedTime,
                             ProductCode = ospp.Code,
                             ProductName = ospp.Name,
                             SettlementaAmount = o.SumPrice,
                             Mark = o.AdminMark,
                             RefundOrderSerialNumber = o.SerialNumber,
                             RefundUserName = o.CustomerName,
                             Address = o.AddressDetail,
                             Phone = o.CustomerPhone,
                             OriginalOrderSerialNumber = o.OrgionSerialNumber,
                             RefundReason = o.CustomerMark,
                             DeliveryNo = oos != null ? oos.DeliveryNumber : "",
                             RefundDeliveryNo = o.DeliveryNumber,
                             OrderExchangeNo = o.PSerialNumber,
                             WareHouseId = o.WarehouseId,
                             WareHouseName = ow.Name,
                             NickName = o.UserName,
                             Price = o.SumPrice,
                             IsCheck = o.State == OrderState.Finished ? "已验收" : "未验收",
                             IsInvalid = o.State == OrderState.Invalid ? "已作废" : "未作废",
                         };
            if (startTime.HasValue)
                result = result.Where(r => r.Date >= startTime);
            if (endTime.HasValue)
                result = result.Where(r => r.Date < endTime);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.RefundUserName.Contains(search));

            productCount = result.Sum(r => r.Quantity);
            price = result.Sum(r => r.Price);

            return new PageList<ShopRefundOrderModel>(result.OrderByDescending(r => r.Date), pageIndex, pageSize);
        }

        /// <summary>
        /// 获取所有商店退货分析数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ShopRefundOrderModel> GetAllExportShopRefundOrderModels(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join ol in _omsAccessor.Get<Order>() on o.OrgionSerialNumber equals ol.SerialNumber into oosl
                         from oos in oosl.DefaultIfEmpty()
                         where op.Isvalid && o.Isvalid && o.SerialNumber.StartsWith("RF")
                         && osp.Isvalid && ospp.Isvalid && ow.Isvalid && os.Isvalid
                         select new ShopRefundOrderModel
                         {
                             ShopId = o.ShopId,
                             ShopName = os != null ? os.Value : "",
                             Quantity = op.Quantity,
                             Date = o.CreatedTime,
                             ProductCode = ospp.Code,
                             ProductName = ospp.Name,
                             SettlementaAmount = o.SumPrice,
                             Mark = o.AdminMark,
                             RefundOrderSerialNumber = o.SerialNumber,
                             RefundUserName = o.CustomerName,
                             Address = o.AddressDetail,
                             Phone = o.CustomerPhone,
                             OriginalOrderSerialNumber = o.OrgionSerialNumber,
                             RefundReason = o.CustomerMark,
                             DeliveryNo = oos != null ? oos.DeliveryNumber : "",
                             RefundDeliveryNo = o.DeliveryNumber,
                             OrderExchangeNo = o.PSerialNumber,
                             WareHouseId = o.WarehouseId,
                             WareHouseName = ow.Name,
                             NickName = o.UserName,
                             Price = o.SumPrice,
                             IsCheck = o.State == OrderState.Finished ? "已验收" : "未验收",
                             IsInvalid = o.State == OrderState.Invalid ? "已作废" : "未作废",
                         };
            if (startTime.HasValue)
                result = result.Where(r => r.Date >= startTime);
            if (endTime.HasValue)
                result = result.Where(r => r.Date < endTime);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.RefundUserName.Contains(search));
            return result;
        }

        /// <summary>
        /// 分页获取商店退货按照店铺分析数据
        /// </summary>
        /// <returns></returns>
        public PageList<ShopRefundOrderByShopModel> GetOrderDeliverModelByShopByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, out int productCount, out decimal price, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join ol in _omsAccessor.Get<Order>() on o.OrgionSerialNumber equals ol.SerialNumber into oosl
                         from oos in oosl.DefaultIfEmpty()
                         where op.Isvalid && o.Isvalid && o.SerialNumber.StartsWith("RF")
                         && osp.Isvalid && ospp.Isvalid && ow.Isvalid && os.Isvalid
                         select new
                         {
                             ShopId = o.ShopId,
                             ShopName = os != null ? os.Value : "",
                             Quantity = op.Quantity,
                             Date = o.CreatedTime,
                             ProductCode = ospp.Code,
                             ProductName = ospp.Name,
                             WareHouseId = o.WarehouseId,
                             RefundUserName = o.CustomerName,
                             Price = o.SumPrice,
                         };
            if (startTime.HasValue)
                result = result.Where(r => r.Date >= startTime);
            if (endTime.HasValue)
                result = result.Where(r => r.Date < endTime);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.RefundUserName.Contains(search));

            var lastResult = result.GroupBy(r => r.ShopId).Select(r => new ShopRefundOrderByShopModel
            {
                ShopName = r.FirstOrDefault().ShopName,
                ShopId = r.FirstOrDefault().ShopId,
                Quantity = r.Sum(p => p.Quantity),
                Price = r.Sum(p => p.Price)
            });

            productCount = lastResult.Sum(r => r.Quantity);
            price = lastResult.Sum(r => r.Price);

            return new PageList<ShopRefundOrderByShopModel>(lastResult.OrderByDescending(r => r.Quantity), pageIndex, pageSize);
        }

        /// <summary>
        /// 获取所有商店退货按照店铺分析数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ShopRefundOrderByShopModel> GetAllExportShopRefundOrderModelsByShop(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join ol in _omsAccessor.Get<Order>() on o.OrgionSerialNumber equals ol.SerialNumber into oosl
                         from oos in oosl.DefaultIfEmpty()
                         where op.Isvalid && o.Isvalid && o.SerialNumber.StartsWith("RF")
                         && osp.Isvalid && ospp.Isvalid && ow.Isvalid && os.Isvalid
                         select new
                         {
                             ShopId = o.ShopId,
                             ShopName = os != null ? os.Value : "",
                             Quantity = op.Quantity,
                             Date = o.CreatedTime,
                             ProductCode = ospp.Code,
                             ProductName = ospp.Name,
                             WareHouseId = o.WarehouseId,
                             RefundUserName = o.CustomerName,
                             Price = o.SumPrice,
                         };
            if (startTime.HasValue)
                result = result.Where(r => r.Date >= startTime);
            if (endTime.HasValue)
                result = result.Where(r => r.Date < endTime);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.RefundUserName.Contains(search));

            var lastResult = result.GroupBy(r => r.ShopId).Select(r => new ShopRefundOrderByShopModel
            {
                ShopName = r.FirstOrDefault().ShopName,
                ShopId = r.FirstOrDefault().ShopId,
                Quantity = r.Sum(p => p.Quantity),
                Price = r.Sum(p => p.Price)
            });

            return lastResult.OrderByDescending(r => r.Quantity);
        }

        /// <summary>
        /// 分页获取商店退货按照商品分析数据
        /// </summary>
        /// <returns></returns>
        public PageList<ShopRefundOrderByProductModel> GetOrderDeliverModelByProductByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, out int productCount, out decimal price, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join ol in _omsAccessor.Get<Order>() on o.OrgionSerialNumber equals ol.SerialNumber into oosl
                         from oos in oosl.DefaultIfEmpty()
                         where op.Isvalid && o.Isvalid && o.SerialNumber.StartsWith("RF")
                         && osp.Isvalid && ospp.Isvalid && ow.Isvalid && os.Isvalid
                         select new
                         {
                             ShopId = o.ShopId,
                             Quantity = op.Quantity,
                             Date = o.CreatedTime,
                             ProductId = ospp.Id,
                             ProductCode = ospp.Code,
                             ProductName = ospp != null ? ospp.Name : "",
                             WareHouseId = o.WarehouseId,
                             RefundUserName = o.CustomerName,
                             Price = o.SumPrice
                         };
            if (startTime.HasValue)
                result = result.Where(r => r.Date >= startTime);
            if (endTime.HasValue)
                result = result.Where(r => r.Date < endTime);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.RefundUserName.Contains(search));

            var lastResult = result.GroupBy(r => r.ProductId).Select(r => new ShopRefundOrderByProductModel
            {
                ProductName = r.FirstOrDefault().ProductName,
                ProductId = r.FirstOrDefault().ProductId,
                Quantity = r.Sum(p => p.Quantity),
                Price = r.Sum(p => p.Price)
            });

            productCount = lastResult.Sum(r => r.Quantity);
            price = lastResult.Sum(r => r.Price);

            return new PageList<ShopRefundOrderByProductModel>(lastResult.OrderByDescending(r => r.Quantity), pageIndex, pageSize);
        }

        /// <summary>
        /// 获取所有商店退货按照商品分析数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ShopRefundOrderByProductModel> GetAllExportShopRefundOrderModelsByProduct(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                         from osp in ospl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                         from ospp in osppl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join ol in _omsAccessor.Get<Order>() on o.OrgionSerialNumber equals ol.SerialNumber into oosl
                         from oos in oosl.DefaultIfEmpty()
                         where op.Isvalid && o.Isvalid && o.SerialNumber.StartsWith("RF")
                         && osp.Isvalid && ospp.Isvalid && ow.Isvalid && os.Isvalid
                         select new
                         {
                             ShopId = o.ShopId,
                             Quantity = op.Quantity,
                             Date = o.CreatedTime,
                             ProductId = ospp.Id,
                             ProductCode = ospp.Code,
                             ProductName = ospp != null ? ospp.Name : "",
                             WareHouseId = o.WarehouseId,
                             RefundUserName = o.CustomerName,
                             Price = o.SumPrice,
                         };
            if (startTime.HasValue)
                result = result.Where(r => r.Date >= startTime);
            if (endTime.HasValue)
                result = result.Where(r => r.Date < endTime);
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.RefundUserName.Contains(search));

            var lastResult = result.GroupBy(r => r.ProductId).Select(r => new ShopRefundOrderByProductModel
            {
                ProductName = r.FirstOrDefault().ProductName,
                ProductId = r.FirstOrDefault().ProductId,
                Quantity = r.Sum(p => p.Quantity),
                Price = r.Sum(p => p.Price)
            });

            return lastResult.OrderByDescending(r => r.Quantity);
        }

        /// <summary>
        /// 分页获取零售销售统计数据
        /// </summary>
        /// <returns></returns>
        public PageList<RetailSalesStatisticsModel> GetRetailSalesStatisticsModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, out int productCount, out decimal avgSumPrice, string search = "")
        {
            var result = (from op in _omsAccessor.Get<OrderProduct>()
                          join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                          join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                          from osp in ospl.DefaultIfEmpty()
                          join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                          from ospp in osppl.DefaultIfEmpty()
                          join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into sopl
                          from sop in sopl.DefaultIfEmpty()
                          join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into wol
                          from wo in wol.DefaultIfEmpty()
                          join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                          from od in odl.DefaultIfEmpty()
                          where op.Isvalid && o.Isvalid && osp.Isvalid && ospp.Isvalid && sop.Isvalid && wo.Isvalid && od.Isvalid
                          orderby o.CreatedTime descending
                          select new RetailSalesStatisticsModel
                          {
                              Id = op.Id,
                              OrderId = o.Id,
                              SerialNumber = o.SerialNumber,
                              PSerialNumber = string.IsNullOrEmpty(o.PSerialNumber) ? "" : o.PSerialNumber,
                              ShopId = o.ShopId,
                              ShopName = sop == null ? "" : sop.Value,
                              OrderType = o.SerialNumber.StartsWith("RF") ? "零售退货单" : "零售销货单",
                              ProductName = ospp != null ? ospp.Name : "",
                              ProductCode = ospp != null ? ospp.Code : "",
                              Quantity = op.Quantity,
                              Price = op.Price,
                              OriginalPrice = op.OrginPrice,
                              AvgSumPrice = op.SumPrice,
                              CreatedTime = o.CreatedTime,
                              WareHouseId = o.WarehouseId,
                              WareHouseName = wo != null ? wo.Name : "",
                              CustomeName = o.CustomerName,
                              Address = o.AddressDetail,
                              DeliveryName = od != null ? od.Name : "",
                          }).ToList();

            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(o => o.CreatedTime >= startTime.Value).ToList();
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(o => o.CreatedTime < endTime.Value).ToList();
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId.Value).ToList();
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId.Value).ToList();
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.SerialNumber.Contains(search) || r.PSerialNumber.Contains(search)).ToList();
            if (orderType == 1)
                result = result.Where(r => !r.SerialNumber.StartsWith("RF")).ToList();
            else if (orderType == 2)
                result = result.Where(r => r.SerialNumber.StartsWith("RF")).ToList();

            productCount = result.Sum(r => r.Quantity);
            avgSumPrice = result.Sum(r => r.AvgSumPrice);

            return new PageList<RetailSalesStatisticsModel>(result, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取所有零售销售统计数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RetailSalesStatisticsModel> GetAllExportRetailSalesStatisticsModel(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            var result = (from op in _omsAccessor.Get<OrderProduct>()
                          join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                          join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                          from osp in ospl.DefaultIfEmpty()
                          join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                          from ospp in osppl.DefaultIfEmpty()
                          join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into sopl
                          from sop in sopl.DefaultIfEmpty()
                          join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into wol
                          from wo in wol.DefaultIfEmpty()
                          join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                          from od in odl.DefaultIfEmpty()
                          where op.Isvalid && o.Isvalid && osp.Isvalid && ospp.Isvalid && sop.Isvalid && wo.Isvalid && od.Isvalid
                          orderby o.CreatedTime descending
                          select new RetailSalesStatisticsModel
                          {
                              Id = op.Id,
                              OrderId = o.Id,
                              SerialNumber = o.SerialNumber,
                              PSerialNumber = string.IsNullOrEmpty(o.PSerialNumber) ? "" : o.PSerialNumber,
                              ShopId = o.ShopId,
                              ShopName = sop == null ? "" : sop.Value,
                              OrderType = o.SerialNumber.StartsWith("RF") ? "零售退货单" : "零售销货单",
                              ProductName = ospp != null ? ospp.Name : "",
                              ProductCode = ospp != null ? ospp.Code : "",
                              Quantity = op.Quantity,
                              Price = op.Price,
                              OriginalPrice = op.OrginPrice,
                              AvgSumPrice = op.SumPrice,
                              CreatedTime = o.CreatedTime,
                              WareHouseId = o.WarehouseId,
                              WareHouseName = wo != null ? wo.Name : "",
                              CustomeName = o.CustomerName,
                              Address = o.AddressDetail,
                              DeliveryName = od != null ? od.Name : "",
                          }).ToList();

            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(o => o.CreatedTime >= startTime.Value).ToList();
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(o => o.CreatedTime < endTime.Value).ToList();
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId.Value).ToList();
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId.Value).ToList();
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.SerialNumber.Contains(search) || r.PSerialNumber.Contains(search)).ToList();
            if (orderType == 1)
                result = result.Where(r => !r.SerialNumber.StartsWith("RF")).ToList();
            else if (orderType == 2)
                result = result.Where(r => r.SerialNumber.StartsWith("RF")).ToList();

            return result;
        }

        /// <summary>
        /// 根据商店分类分页获取零售销售统计数据
        /// </summary>
        /// <returns></returns>
        public PageList<RetailSalesStatisticsByShopModel> GetRetailSalesStatisticsByShopModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, out int productCount, out decimal avgSumPrice, string search = "")
        {
            var result = (from op in _omsAccessor.Get<OrderProduct>()
                          join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                          join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                          from osp in ospl.DefaultIfEmpty()
                          join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                          from ospp in osppl.DefaultIfEmpty()
                          join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into sopl
                          from sop in sopl.DefaultIfEmpty()
                          join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into wol
                          from wo in wol.DefaultIfEmpty()
                          join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                          from od in odl.DefaultIfEmpty()
                          where op.Isvalid && o.Isvalid && osp.Isvalid && ospp.Isvalid && sop.Isvalid && wo.Isvalid && od.Isvalid
                          orderby o.CreatedTime descending
                          select new
                          {
                              Id = op.Id,
                              OrderId = o.Id,
                              SerialNumber = o.SerialNumber,
                              PSerialNumber = string.IsNullOrEmpty(o.PSerialNumber) ? "" : o.PSerialNumber,
                              ShopId = o.ShopId,
                              ShopName = sop == null ? "" : sop.Value,
                              ProductName = ospp != null ? ospp.Name : "",
                              ProductCode = ospp != null ? ospp.Code : "",
                              Quantity = op.Quantity,
                              Price = op.Price,
                              OriginalPrice = op.OrginPrice,
                              AvgSumPrice = op.SumPrice,
                              CreatedTime = o.CreatedTime,
                              WareHouseId = o.WarehouseId,
                          }).ToList();

            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(o => o.CreatedTime >= startTime.Value).ToList();
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(o => o.CreatedTime < endTime.Value).ToList();
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId.Value).ToList();
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId.Value).ToList();
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.SerialNumber.Contains(search) || r.PSerialNumber.Contains(search)).ToList();
            if (orderType == 1)
                result = result.Where(r => !r.SerialNumber.StartsWith("RF")).ToList();
            else if (orderType == 2)
                result = result.Where(r => r.SerialNumber.StartsWith("RF")).ToList();

            var lastResult = result.GroupBy(r => r.ShopId).Select(r => new RetailSalesStatisticsByShopModel
            {
                ShopId = r.Key,
                ShopName = r.FirstOrDefault().ShopName,
                Quantity = r.Sum(g => g.Quantity),
                AvgSumPrice = r.Sum(g => g.AvgSumPrice)
            }).OrderByDescending(r => r.Quantity).ToList();

            productCount = lastResult.Sum(r => r.Quantity);
            avgSumPrice = lastResult.Sum(r => r.AvgSumPrice);

            return new PageList<RetailSalesStatisticsByShopModel>(lastResult, pageIndex, pageSize);
        }

        /// <summary>
        /// 根据店铺获取所有零售销售统计数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RetailSalesStatisticsByShopModel> GetAllExportRetailSalesStatisticsByShopModel(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            var result = (from op in _omsAccessor.Get<OrderProduct>()
                          join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                          join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                          from osp in ospl.DefaultIfEmpty()
                          join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                          from ospp in osppl.DefaultIfEmpty()
                          join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into sopl
                          from sop in sopl.DefaultIfEmpty()
                          join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into wol
                          from wo in wol.DefaultIfEmpty()
                          join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                          from od in odl.DefaultIfEmpty()
                          where op.Isvalid && o.Isvalid && osp.Isvalid && ospp.Isvalid && sop.Isvalid && wo.Isvalid && od.Isvalid
                          orderby o.CreatedTime descending
                          select new
                          {
                              Id = op.Id,
                              OrderId = o.Id,
                              SerialNumber = o.SerialNumber,
                              PSerialNumber = string.IsNullOrEmpty(o.PSerialNumber) ? "" : o.PSerialNumber,
                              ShopId = o.ShopId,
                              ShopName = sop == null ? "" : sop.Value,
                              ProductName = ospp != null ? ospp.Name : "",
                              ProductCode = ospp != null ? ospp.Code : "",
                              Quantity = op.Quantity,
                              Price = op.Price,
                              OriginalPrice = op.OrginPrice,
                              AvgSumPrice = op.SumPrice,
                              CreatedTime = o.CreatedTime,
                              WareHouseId = o.WarehouseId,
                          }).ToList();

            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(o => o.CreatedTime >= startTime.Value).ToList();
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(o => o.CreatedTime < endTime.Value).ToList();
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId.Value).ToList();
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId.Value).ToList();
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.SerialNumber.Contains(search) || r.PSerialNumber.Contains(search)).ToList();
            if (orderType == 1)
                result = result.Where(r => !r.SerialNumber.StartsWith("RF")).ToList();
            else if (orderType == 2)
                result = result.Where(r => r.SerialNumber.StartsWith("RF")).ToList();

            var lastResult = result.GroupBy(r => r.ShopId).Select(r => new RetailSalesStatisticsByShopModel
            {
                ShopId = r.Key,
                ShopName = r.FirstOrDefault().ShopName,
                Quantity = r.Sum(g => g.Quantity),
                AvgSumPrice = r.Sum(g => g.AvgSumPrice)
            }).OrderByDescending(r => r.Quantity).ToList();

            return lastResult;
        }

        /// <summary>
        /// 根据商品分类分页获取零售销售统计数据
        /// </summary>
        /// <returns></returns>
        public PageList<RetailSalesStatisticsByProductModel> GetRetailSalesStatisticsByProductModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, out int productCount, out decimal avgSumPrice, string search = "")
        {
            var result = (from op in _omsAccessor.Get<OrderProduct>()
                          join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                          join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                          from osp in ospl.DefaultIfEmpty()
                          join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                          from ospp in osppl.DefaultIfEmpty()
                          join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into sopl
                          from sop in sopl.DefaultIfEmpty()
                          join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into wol
                          from wo in wol.DefaultIfEmpty()
                          join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                          from od in odl.DefaultIfEmpty()
                          where op.Isvalid && o.Isvalid && osp.Isvalid && ospp.Isvalid && sop.Isvalid && wo.Isvalid && od.Isvalid
                          orderby o.CreatedTime descending
                          select new
                          {
                              Id = op.Id,
                              OrderId = o.Id,
                              SerialNumber = o.SerialNumber,
                              PSerialNumber = string.IsNullOrEmpty(o.PSerialNumber) ? "" : o.PSerialNumber,
                              ShopId = o.ShopId,
                              ShopName = sop == null ? "" : sop.Value,
                              ProductId = osp.ProductId,
                              ProductName = ospp != null ? ospp.Name : "",
                              ProductCode = ospp != null ? ospp.Code : "",
                              Quantity = op.Quantity,
                              Price = op.Price,
                              OriginalPrice = op.OrginPrice,
                              AvgSumPrice = op.SumPrice,
                              CreatedTime = o.CreatedTime,
                              WareHouseId = o.WarehouseId,
                          }).ToList();

            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(o => o.CreatedTime >= startTime.Value).ToList();
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(o => o.CreatedTime < endTime.Value).ToList();
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId.Value).ToList();
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId.Value).ToList();
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.SerialNumber.Contains(search) || r.PSerialNumber.Contains(search)).ToList();
            if (orderType == 1)
                result = result.Where(r => !r.SerialNumber.StartsWith("RF")).ToList();
            else if (orderType == 2)
                result = result.Where(r => r.SerialNumber.StartsWith("RF")).ToList();

            var lastResult = result.GroupBy(r => r.ProductId).Select(r => new RetailSalesStatisticsByProductModel
            {
                ProductId = r.Key,
                ProductName = r.FirstOrDefault().ProductName,
                ProductCode = r.FirstOrDefault().ProductCode,
                Price = r.FirstOrDefault().Price,
                Quantity = r.Sum(g => g.Quantity),
                AvgSumPrice = r.Sum(g => g.AvgSumPrice)
            }).OrderByDescending(r => r.Quantity).ToList();

            productCount = lastResult.Sum(r => r.Quantity);
            avgSumPrice = lastResult.Sum(r => r.AvgSumPrice);

            return new PageList<RetailSalesStatisticsByProductModel>(lastResult, pageIndex, pageSize);
        }

        /// <summary>
        /// 根据商品获取所有零售销售统计数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RetailSalesStatisticsByProductModel> GetAllExportRetailSalesStatisticsByProductModel(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            var result = (from op in _omsAccessor.Get<OrderProduct>()
                          join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                          join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into ospl
                          from osp in ospl.DefaultIfEmpty()
                          join p in _omsAccessor.Get<Product>() on osp.ProductId equals p.Id into osppl
                          from ospp in osppl.DefaultIfEmpty()
                          join s in _omsAccessor.Get<Dictionary>(r => r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into sopl
                          from sop in sopl.DefaultIfEmpty()
                          join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into wol
                          from wo in wol.DefaultIfEmpty()
                          join d in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals d.Id into odl
                          from od in odl.DefaultIfEmpty()
                          where op.Isvalid && o.Isvalid && osp.Isvalid && ospp.Isvalid && sop.Isvalid && wo.Isvalid && od.Isvalid
                          orderby o.CreatedTime descending
                          select new
                          {
                              Id = op.Id,
                              OrderId = o.Id,
                              SerialNumber = o.SerialNumber,
                              PSerialNumber = string.IsNullOrEmpty(o.PSerialNumber) ? "" : o.PSerialNumber,
                              ShopId = o.ShopId,
                              ShopName = sop == null ? "" : sop.Value,
                              ProductName = ospp != null ? ospp.Name : "",
                              ProductCode = ospp != null ? ospp.Code : "",
                              ProductId = osp.ProductId,
                              Quantity = op.Quantity,
                              Price = op.Price,
                              OriginalPrice = op.OrginPrice,
                              AvgSumPrice = op.SumPrice,
                              CreatedTime = o.CreatedTime,
                              WareHouseId = o.WarehouseId,
                          }).ToList();

            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(o => o.CreatedTime >= startTime.Value).ToList();
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(o => o.CreatedTime < endTime.Value).ToList();
            if (shopId.HasValue)
                result = result.Where(r => r.ShopId == shopId.Value).ToList();
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId.Value).ToList();
            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.ProductName.Contains(search) || r.ProductCode.Contains(search) || r.SerialNumber.Contains(search) || r.PSerialNumber.Contains(search)).ToList();
            if (orderType == 1)
                result = result.Where(r => !r.SerialNumber.StartsWith("RF")).ToList();
            else if (orderType == 2)
                result = result.Where(r => r.SerialNumber.StartsWith("RF")).ToList();

            var lastResult = result.GroupBy(r => r.ProductId).Select(r => new RetailSalesStatisticsByProductModel
            {
                ProductId = r.Key,
                ProductName = r.FirstOrDefault().ProductName,
                ProductCode = r.FirstOrDefault().ProductCode,
                Price = r.FirstOrDefault().Price,
                Quantity = r.Sum(g => g.Quantity),
                AvgSumPrice = r.Sum(g => g.AvgSumPrice)
            }).OrderByDescending(r => r.Quantity).ToList();

            return lastResult;
        }

        /// <summary>
        /// 分页获取B2B订单销售数据分析
        /// </summary>
        /// <returns></returns>
        public PageList<B2BSaleDataAnalysisModel> GetB2BSaleDataAnalysisModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? customerId, int? wareHouseId, int? customerTypeId, int? orderType, int? checkType, int? bookKeepType, out int count, out decimal sumPrice, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join c in _omsAccessor.Get<Customers>().Include(x => x.Dictionary) on o.CustomerId equals c.Id into ocl
                         from oc in ocl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into sopl
                         from sop in sopl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on sop.ProductId equals p.Id into soppl
                         from sopp in soppl.DefaultIfEmpty()
                         join sa in _omsAccessor.Get<SalesMan>() on o.SalesManId equals sa.Id into sale
                         from sal in sale.DefaultIfEmpty()
                         where op.Isvalid && o.Isvalid
                         && oc.Isvalid
                         && ow.Isvalid && sop.Isvalid && sopp.Isvalid
                         && (o.Type == OrderType.B2B_TH || o.Type == OrderType.B2B)
                         orderby o.CreatedTime descending
                         select new B2BSaleDataAnalysisModel
                         {
                             OrderTypeStr = o.Type == OrderType.B2B ? "B2B销货单" : "B2B退货单",
                             OrderType = o.Type,
                             SerialNumber = o.SerialNumber,
                             CreatedTime = o.CreatedTime,
                             ProductCode = sopp != null ? sopp.Code : "",
                             ProductName = sopp != null ? sopp.Name : "",
                             Quantity = o.Type == OrderType.B2B ? op.Quantity : (-op.Quantity),
                             Price = o.Type == OrderType.B2B ? op.SumPrice : (-op.SumPrice),
                             UnitPrice = o.Type == OrderType.B2B ? op.Price : (-op.Price),
                             CustomerName = oc != null ? oc.Name : "",
                             CustomerId = o.CustomerId,
                             CustomerTypeId = oc != null ? oc.CustomerTypeId : 0,
                             CustomerTypeName = oc != null ? oc.Dictionary.Value : "",
                             WareHouseId = o.WarehouseId,
                             WareHouseName = ow != null ? ow.Name : "",
                             IsCheck = (o.State == OrderState.CheckAccept || o.State == OrderState.Bookkeeping || o.State == OrderState.Finished),
                             IsCheckStr = (o.State == OrderState.CheckAccept || o.State == OrderState.Bookkeeping || o.State == OrderState.Finished) ? "已验收" : "未验收",
                             //IsBookKeeping = o.PayPrice==o.SumPrice,
                             IsBookKeepingStr = GetBookKeepingStr(o),
                             Mark = string.IsNullOrEmpty(o.CustomerMark) ? "" : o.CustomerMark,
                             CheckerName = "",
                             State = o.State,
                             OriginalOrderId = o.OriginalOrderId,
                             SalesManName = sal.UserName == null ? "" : sal.UserName,
                             DeliveryDate = o.DeliveryDate

                         };
            //下单时间
            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(r => r.CreatedTime >= startTime);
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(r => r.CreatedTime < endTime);
            //收发时间
            if (deliverStartTime.HasValue && deliverStartTime.Value != null)
                result = result.Where(r => r.DeliveryDate >= deliverStartTime);
            if (deliverEndTime.HasValue && deliverEndTime.Value != null)
                result = result.Where(r => r.DeliveryDate < endTime);

            if (customerId.HasValue)
                result = result.Where(r => r.CustomerId == customerId);
            if (customerTypeId.HasValue)
                result = result.Where(r => r.CustomerTypeId == customerTypeId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (orderType.HasValue)
            {
                if (orderType.Value == 1)
                    result = result.Where(r => r.OrderType == OrderType.B2B);
                else if (orderType.Value == 2)
                    result = result.Where(r => r.OrderType == OrderType.B2B_TH);
            }
            if (checkType.HasValue)
            {
                if (checkType.Value == 1)
                    result = result.Where(r => new OrderState[] { OrderState.CheckAccept, OrderState.Bookkeeping, OrderState.Finished }.Contains(r.State));
                else if (checkType.Value == 2)
                    result = result.Where(r => !new OrderState[] { OrderState.CheckAccept, OrderState.Bookkeeping, OrderState.Finished }.Contains(r.State));
            }
            if (bookKeepType.HasValue)
            {
                if (bookKeepType.Value == 1)
                    result = result.Where(r => r.IsBookKeepingStr == "已付款");
                else if (bookKeepType.Value == 2)
                    result = result.Where(r => r.IsBookKeepingStr == "部分付款");
                else if (bookKeepType.Value == 3)
                    result = result.Where(r => r.IsBookKeepingStr == "未付款");
            }

            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.SerialNumber.Contains(search) || r.Mark.Contains(search) || r.ProductName.Contains(search) || r.ProductCode.Contains(search));

            count = result.Sum(r => r.Quantity);
            sumPrice = result.Sum(r => r.Price);

            return new PageList<B2BSaleDataAnalysisModel>(result, pageIndex, pageSize);
        }

        /// <summary>
        /// 获取所有B2B销货数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<B2BSaleDataAnalysisModel> GetAllExportB2BSaleDataAnalysisModel(DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? customerId, int? wareHouseId, int? customerTypeId, int? orderType, int? checkType, int? bookKeepType, string search = "")
        {
            var result = from op in _omsAccessor.Get<OrderProduct>()
                         join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                         join c in _omsAccessor.Get<Customers>().Include(x => x.Dictionary) on o.CustomerId equals c.Id into ocl
                         from oc in ocl.DefaultIfEmpty()
                         join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id into owl
                         from ow in owl.DefaultIfEmpty()
                         join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id into sopl
                         from sop in sopl.DefaultIfEmpty()
                         join p in _omsAccessor.Get<Product>() on sop.ProductId equals p.Id into soppl
                         from sopp in soppl.DefaultIfEmpty()
                         join sa in _omsAccessor.Get<SalesMan>() on o.SalesManId equals sa.Id into sale
                         from sal in sale.DefaultIfEmpty()
                         where op.Isvalid && o.Isvalid
                         && oc.Isvalid
                         && ow.Isvalid && sop.Isvalid && sopp.Isvalid
                         && (o.Type == OrderType.B2B_TH || o.Type == OrderType.B2B)
                         orderby o.CreatedTime descending
                         select new B2BSaleDataAnalysisModel
                         {
                             OrderTypeStr = o.Type == OrderType.B2B ? "B2B销货单" : "B2B退货单",
                             OrderType = o.Type,
                             SerialNumber = o.SerialNumber,
                             CreatedTime = o.CreatedTime,
                             ProductCode = sopp != null ? sopp.Code : "",
                             ProductName = sopp != null ? sopp.Name : "",
                             Quantity = o.Type == OrderType.B2B ? op.Quantity : (-op.Quantity),
                             Price = o.Type == OrderType.B2B ? op.SumPrice : (-op.SumPrice),
                             UnitPrice = o.Type == OrderType.B2B ? op.Price : (-op.Price),
                             CustomerName = oc != null ? oc.Name : "",
                             CustomerId = o.CustomerId,
                             CustomerTypeId = oc != null ? oc.CustomerTypeId : 0,
                             CustomerTypeName = oc != null ? oc.Dictionary.Value : "",
                             WareHouseId = o.WarehouseId,
                             WareHouseName = ow != null ? ow.Name : "",
                             IsCheck = (o.State == OrderState.CheckAccept || o.State == OrderState.Bookkeeping || o.State == OrderState.Finished),
                             IsCheckStr = (o.State == OrderState.CheckAccept || o.State == OrderState.Bookkeeping || o.State == OrderState.Finished) ? "已验收" : "未验收",
                             //IsBookKeeping = o.PayState== PayState.Success,
                             IsBookKeepingStr = GetBookKeepingStr(o),
                             Mark = string.IsNullOrEmpty(o.CustomerMark) ? "" : o.CustomerMark,
                             SalesManName = sal.UserName == null ? "" : sal.UserName,
                             CheckerName = "",
                             State = o.State,
                             DeliveryDate = o.DeliveryDate
                         };
            //下单时间
            if (startTime.HasValue && startTime.Value != null)
                result = result.Where(r => r.CreatedTime >= startTime);
            if (endTime.HasValue && endTime.Value != null)
                result = result.Where(r => r.CreatedTime < endTime);
            //收发时间
            if (deliverStartTime.HasValue && deliverStartTime.Value != null)
                result = result.Where(r => r.DeliveryDate >= deliverStartTime);
            if (deliverEndTime.HasValue && deliverEndTime.Value != null)
                result = result.Where(r => r.DeliveryDate < deliverEndTime);

            if (customerId.HasValue)
                result = result.Where(r => r.CustomerId == customerId);
            if (customerTypeId.HasValue)
                result = result.Where(r => r.CustomerTypeId == customerTypeId);
            if (wareHouseId.HasValue)
                result = result.Where(r => r.WareHouseId == wareHouseId);
            if (orderType.HasValue)
            {
                if (orderType.Value == 1)
                    result = result.Where(r => r.OrderType == OrderType.B2B);
                else if (orderType.Value == 2)
                    result = result.Where(r => r.OrderType == OrderType.B2B_TH);
            }
            if (checkType.HasValue)
            {
                if (checkType.Value == 1)
                    result = result.Where(r => new OrderState[] { OrderState.CheckAccept, OrderState.Bookkeeping, OrderState.Finished }.Contains(r.State));
                else if (checkType.Value == 2)
                    result = result.Where(r => !new OrderState[] { OrderState.CheckAccept, OrderState.Bookkeeping, OrderState.Finished }.Contains(r.State));
            }
            if (bookKeepType.HasValue)
            {
                if (bookKeepType.Value == 1)
                    result = result.Where(r => r.IsBookKeepingStr == "已付款");
                else if (bookKeepType.Value == 2)
                    result = result.Where(r => r.IsBookKeepingStr == "部分付款");
                else if (bookKeepType.Value == 3)
                    result = result.Where(r => r.IsBookKeepingStr == "未付款");
            }

            if (!string.IsNullOrEmpty(search))
                result = result.Where(r => r.SerialNumber.Contains(search) || r.Mark.Contains(search) || r.ProductName.Contains(search) || r.ProductCode.Contains(search));

            return result;
        }

        /// <summary>
        /// 获取记账状态
        /// </summary>
        /// <returns></returns>
        private string GetBookKeepingStr(Order order)
        {
            if (order == null)
            {
                return "未付款";
            }

            if (order.SumPrice == 0 || order.PayPrice == order.SumPrice)
            {
                return "已付款";
            }

            if (order.PayPrice < order.SumPrice && order.PayPrice > 0)
            {
                return "部分付款";
            }

            return "未付款";


        }
        /// <summary>
        /// 分页获取客户的所有历史订单
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public PageList<CustomerHistoryOrderModel> GetHistoryOrderListByPage(int pageIndex, int pageSize, string userName)
        {
            var result = from o in _omsAccessor.Get<Order>()
                         join s in _omsAccessor.Get<Dictionary>().Where(r => r.Isvalid && r.Type == DictionaryType.Platform) on o.ShopId equals s.Id into osl
                         from os in osl.DefaultIfEmpty()
                         join d in _omsAccessor.Get<Delivery>().Where(r => r.Isvalid) on o.DeliveryTypeId equals d.Id into odl
                         from od in odl.DefaultIfEmpty()
                         where o.Isvalid && o.Type == OrderType.B2C_XH && !string.IsNullOrEmpty(o.UserName) && o.UserName.Equals(userName)
                         orderby o.CreatedTime descending
                         select new CustomerHistoryOrderModel
                         {
                             Id = o.Id,
                             SerialNumber = o.SerialNumber,
                             Shop = o.ShopId,
                             ShopName = os != null ? os.Value : "",
                             CreatedTime = o.CreatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                             CustomerName = o.CustomerName,
                             PayedPrice = o.PayPrice,
                             AdminMark = string.IsNullOrEmpty(o.AdminMark) ? "" : o.AdminMark,
                             CustomerMark = string.IsNullOrEmpty(o.CustomerMark) ? "" : o.CustomerMark,
                             DeliveryTypeId = o.DeliveryTypeId,
                             DeliveryTypeName = od != null ? od.Name : "",
                             Address = string.IsNullOrEmpty(o.AddressDetail) ? "" : o.AddressDetail
                         };
            return new PageList<CustomerHistoryOrderModel>(result.ToList(), pageIndex, pageSize);
        }
        public Object GetProductStockInEveryWareHouse(int productId)
        {
            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var product = _omsAccessor.Get<Product>().Where(p => p.Id == productId).FirstOrDefault();
                string url = _configuration.GetSection("WMSApi")["domain"].ToString() + "/wmsapi/ProductSync/GetEveryWareHouseStockInStockTable?productCode=" + product.Code;
                var response = http.GetAsync(url).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync();
                    return result.Result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logService.Error("UploadRefundOrder方法失败，原因是API授权失败！请重试。");
                    //GetWMSOauthToken();
                    return (new { isSucc = false, msg = "UploadRefundOrder方法失败，原因是API授权失败！请重试。" }).ToJson();
                }
                else
                {
                    _logService.Error(string.Format("UploadRefundOrder方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, msg = string.Format("UploadRefundOrder方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToJson();
                }
            }
        }
        public object GetOrderState(string orderSerialNumber)
        {
            var order = _omsAccessor.Get<Order>().Where(o => o.SerialNumber == orderSerialNumber).FirstOrDefault();
            var orderStates = GetOrderStateStr();
            var result = orderStates.Where(r => r.Key == order.State).FirstOrDefault();
            return new { State = result.Key, Str = result.Value };
        }
        public object GetAllKJRefundOrderInfo(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, int? orderState, string search = "", OrderType orderType = OrderType.B2C_KJRF)
        {
            DateTime? dateTime = new DateTime();
            if (startTime.Equals(dateTime))
            {
                startTime = null;
            }
            if (endTime.Equals(dateTime))
            {
                endTime = null;
            }

            if (deliverStartTime.Equals(dateTime))
            {
                deliverStartTime = null;
            }
            if (deliverEndTime.Equals(dateTime))
            {
                deliverEndTime = null;
            }
            Dictionary<InvoiceType, string> InvoiceTypeName = new Dictionary<InvoiceType, string>
            {
                { InvoiceType.NoNeedInvoice,"无发票"},
                { InvoiceType.PersonalInvoice,"个人发票"},
                { InvoiceType.CompanyInvoice,"普通单位发票"},
                { InvoiceType.SpecialInvoice,"专用发票"}
            };
            var orderStateStr = GetOrderStateStr();
            var data= from op in _omsAccessor.Get<OrderProduct>()
                      join sp in _omsAccessor.Get<SaleProduct>() on op.SaleProductId equals sp.Id
                      join p in _omsAccessor.Get<Product>() on sp.ProductId equals p.Id
                      join o in _omsAccessor.Get<Order>() on op.OrderId equals o.Id
                      where (o.Type == orderType)
                      where (string.IsNullOrEmpty(search) || o.SerialNumber.Contains(search) || o.PSerialNumber.Contains(search) || o.OrgionSerialNumber.Contains(search) ||
                      o.DeliveryNumber == search || o.CustomerName == search || o.CustomerPhone == search || o.AdminMark == search || o.CustomerMark == search)
                      && ((!startTime.HasValue || o.CreatedTime >= startTime.Value) && (!endTime.HasValue || o.CreatedTime <= endTime.Value))
                      && ((!deliverStartTime.HasValue || o.DeliveryDate >= deliverStartTime.Value) && (!deliverEndTime.HasValue || o.DeliveryDate <= deliverEndTime.Value))
                      && (!shopId.HasValue || o.ShopId == shopId)
                      && (!orderState.HasValue || o.State == (OrderState)orderState)
                      join s in _omsAccessor.Get<Dictionary>() on o.ShopId equals s.Id
                      join w in _omsAccessor.Get<WareHouse>() on o.WarehouseId equals w.Id
                      join de in _omsAccessor.Get<Delivery>() on o.DeliveryTypeId equals de.Id into del
                      from deli in del.DefaultIfEmpty()
                      orderby o.CreatedTime descending
                      select new
                      {
                          o.Id,
                          o.CreatedTime,
                          o.SerialNumber,
                          o.ShopId,
                          o.WarehouseId,
                          o.CustomerName,
                          o.CustomerPhone,
                          o.SumPrice,
                          o.PSerialNumber,
                          o.PayPrice,
                          o.OrgionSerialNumber,
                          o.PayType,
                          o.PayState,
                          o.DeliveryTypeId,
                          o.DeliveryNumber,
                          o.State,
                          o.AdminMark,
                          op.SaleProductId,
                          WarehouseName = w.Name,
                          ShopName = s.Value,
                          PayTypeName = (o.PayType == 0 ? "" : _omsAccessor.Get<Dictionary>().Where(d => d.Id == o.PayType).FirstOrDefault().Value),
                          PayStateName = o.PayState.ToString() == "Success" ? "已付款" : "未付款",
                          DeliveryTypeName = deli == null ? "" : deli.Name,
                          InvoiceTypeName = InvoiceTypeName[o.InvoiceType],
                          StateName = orderStateStr[o.State],
                          DeliveryDate = o.DeliveryDate.ToString(),
                          OrderProductId = op.Id,
                          OrderProductQuantity = op.Quantity,
                          OrderProductOriginPrice = op.OrginPrice,
                          OrderProductPrice = op.Price,
                          OrderProductSumPrice = op.SumPrice,
                          ProductCode = p.Code,
                          ProductName = p.Name
                      };
            return new PageList<Object>(data, pageIndex, pageSize);
        }
        #region 检查订单是否缺货
        /// <summary>
        /// 检查是否缺货
        /// </summary>
        /// <returns></returns>
        public bool IsOrderOutofStock(Order order)
        {
            //var orderProducts = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == orderId).ToList();
            if (order.OrderProduct==null) {
                order.OrderProduct = _omsAccessor.Get<OrderProduct>().Where(x => x.Isvalid && x.OrderId == order.Id).ToList();
            }
            foreach (var item in order.OrderProduct)
            {
                var saleProductLockedTrack = _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.LockNumber == item.Quantity && x.WareHouseId ==order.WarehouseId && x.OrderProductId == item.Id).FirstOrDefault();
                if (saleProductLockedTrack == null)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 通过OrderId检查是否缺货
        /// </summary>
        /// <returns></returns>
        public bool IsOrderOutofStockByOrderId(int orderId) {
            var order = _omsAccessor.Get<Order>().Include(x => x.OrderProduct).Where(x => x.Id == orderId).FirstOrDefault();
            foreach (var item in order.OrderProduct)
            {
                var saleProductLockedTrack = _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.LockNumber == item.Quantity && x.WareHouseId == order.WarehouseId && x.OrderProductId == item.Id).FirstOrDefault();
                if (saleProductLockedTrack == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断订单商品是否缺货
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <param name="wareHouseId"></param>
        /// <returns></returns>
        public bool IsOrderProductStock(OrderProduct orderProduct, int wareHouseId) {
            var saleProductLockedTrack = _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.LockNumber == orderProduct.Quantity && x.WareHouseId == wareHouseId && x.OrderProductId == orderProduct.Id).FirstOrDefault();
            if (saleProductLockedTrack == null)
            {
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// 判断订单商品的锁定库存与仓库库存否可用（上传订单时，主要是防止仓库对不同仓库的商品进行了移位操作导致商品数量差异）
        /// </summary>
        /// <returns></returns>
        public bool IsCheckWarehouseStockCanUse(Order order) {
            try
            {
                var errorcount = 0;
                foreach (var item in order.OrderProduct)
                {
                    var saleProductLockedTrack = _omsAccessor.Get<SaleProductLockedTrack>().Where(x => x.Isvalid && x.LockNumber != 0 && x.LockNumber == item.Quantity && x.WareHouseId == order.WarehouseId && x.OrderProductId == item.Id).FirstOrDefault();
                    var saleProductWareHouse = _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid && x.WareHouseId == order.WarehouseId && x.SaleProductId == item.SaleProductId).FirstOrDefault();
                    if (saleProductLockedTrack == null || saleProductWareHouse == null)
                    {
                        errorcount++;
                    }
                    else
                    {
                        if (saleProductWareHouse.Stock < saleProductLockedTrack.LockNumber)
                        {
                            errorcount++;
                            saleProductWareHouse.LockStock -= saleProductLockedTrack.LockNumber;
                            saleProductLockedTrack.LockNumber = 0;
                            _omsAccessor.Update(saleProductWareHouse);
                            _omsAccessor.Update(saleProductLockedTrack);
                        }
                    }
                }
                _omsAccessor.SaveChanges();
                if (errorcount == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.Error("检测订单"+order.SerialNumber+"库存发生异常："+ex.Message);
                return false;
            }



        }
        #endregion


        #region 接口调用部分函数
        /// <summary>
        /// （接口调用）创建订单
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public string ApiCreateOrder(List<InterfaceOrderModel> orders, string preWord)
        {
            var resultStr = "";
            foreach (var item in orders)
            {
                //已有平台单号则不允许插入
                using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
                {
                    var nextLoop = false;
                    try
                    {
                        if (string.IsNullOrEmpty(item.PSerialNumber))
                        {
                            resultStr += item.PSerialNumber + ":平台订单号为必填\n";
                            continue;
                        }
                        var isExistOrder = _omsAccessor.Get<Order>().Where(r => r.PSerialNumber.Contains(item.PSerialNumber)).FirstOrDefault();
                        if (isExistOrder != null)
                        {
                            resultStr += item.PSerialNumber + ":平台订单已存在\n";
                            continue;
                        }
                        Order newOrder = new Order();
                        //订单主体
                        #region newOrder 订单
                        _workContext.CurrentUser = _omsAccessor.Get<User>().Where(r => r.UserName == "admin").FirstOrDefault();
                        newOrder.SerialNumber = _commonService.GetOrderSerialNumber(preWord);
                        newOrder.PSerialNumber = item.PSerialNumber;
                        newOrder.Type = OrderType.B2C_XH;
                        newOrder.ShopId = item.ShopId;
                        newOrder.State = OrderState.Paid;
                        newOrder.WriteBackState = WriteBackState.NoWrite;


                        //支付信息
                        newOrder.PayType = item.PayType;
                        newOrder.PayMentType = item.PayMentType;//回款方式B2B所属部分
                        newOrder.PayState = PayState.Success;
                        newOrder.PayDate = DateTime.ParseExact((string)item.PayDate, "yyyyMMddHHmmss", null);
                        newOrder.SumPrice = 0;//后面更新
                        newOrder.PayPrice = (decimal)item.PayPrice;


                        //其他控制部分
                        newOrder.IsLackStock = false;
                        newOrder.LockMan = 0;
                        newOrder.LockStock = false;
                        newOrder.IsLackStock = false;
                        newOrder.Isvalid = true;
                        newOrder.IsCopied = false;
                        newOrder.AppendType = 0;
                        newOrder.IsNeedPaperBag = item.IsNeedPaperBag;//是否需要纸袋？


                        //物流信息
                        newOrder.DeliveryTypeId = item.DeliveryTypeId;
                        //newOrder.DeliveryNumber
                        //newOrder.DeliveryDate


                        //客户信息部分
                        newOrder.UserName = item.UserName;
                        newOrder.CustomerName = item.CustomerName;
                        newOrder.CustomerPhone = item.CustomerPhone;
                        newOrder.AddressDetail = item.CustomerAddressDetail;
                        newOrder.DistrictId = 0;
                        newOrder.CustomerMark = item.CustomerMark;
                        newOrder.AdminMark = item.AdminMark;
                        newOrder.WarehouseId = MatchFirstWareHouseId(newOrder.AddressDetail);//匹配仓库
                        //newOrder.FinanceMark
                        //newOrder.ToWarehouseMessage 自行填写


                        //优惠信息部分
                        newOrder.ZMCoupon = item.ZMCoupon;
                        newOrder.ZMWineCoupon = item.ZMWineCoupon;
                        newOrder.WineWorldCoupon = item.WineWorldCoupon;
                        newOrder.ProductCoupon = item.ProductCoupon;
                        newOrder.ZMIntegralValuePrice = item.ZMIntegralValuePrice;
                        //newOrder.OriginalOrderId
                        newOrder.SalesManId = item.SalesManId;


                        //发票部分
                        newOrder.InvoiceType = InvoiceType.NoNeedInvoice;
                        newOrder.InvoiceMode = item.InvoiceMode;//开票方式


                        //其他
                        newOrder.PriceTypeId = 103;//或者0
                        newOrder.CustomerId = 0;
                        newOrder.ApprovalProcessId = 0;


                        _omsAccessor.Insert<Order>(newOrder);
                        _omsAccessor.SaveChanges();
                        #region 订单日志
                        _logService.InsertOrderLog(newOrder.Id, "新增订单", newOrder.State, newOrder.PayState, "接口新增订单");
                        #endregion
                        #endregion


                        #region PayInfo 支付信息
                        OrderPayPrice orderPayPrice = new OrderPayPrice();
                        orderPayPrice.OrderId = newOrder.Id;
                        orderPayPrice.IsPay = true;
                        orderPayPrice.PayType = item.PayType;
                        orderPayPrice.PayMentType = 0;
                        orderPayPrice.Price = newOrder.PayPrice;
                        _omsAccessor.Insert<OrderPayPrice>(orderPayPrice);
                        #endregion


                        #region InvoiceInfo 发票信息
                        if (item.InvoiceInfo != null)
                        {
                            if (!string.IsNullOrEmpty(item.InvoiceInfo.Title))
                            {
                                //增票部分
                                InvoiceInfo invoiceInfo = new InvoiceInfo();
                                invoiceInfo.OrderId = newOrder.Id;
                                invoiceInfo.CustomerEmail = item.InvoiceInfo.CustomerEmail;
                                invoiceInfo.Title = item.InvoiceInfo.Title;

                                if (!string.IsNullOrEmpty(item.InvoiceInfo.TaxpayerID))
                                {
                                    invoiceInfo.TaxpayerID = item.InvoiceInfo.TaxpayerID ?? "";//纳税人识别码
                                    invoiceInfo.RegisterAddress = item.InvoiceInfo.RegisterAddress;
                                    invoiceInfo.RegisterTel = item.InvoiceInfo.RegisterTel;
                                    invoiceInfo.BankAccount = item.InvoiceInfo.BankAccount;//银行账号
                                    invoiceInfo.BankOfDeposit = item.InvoiceInfo.BankOfDeposit;//银行支行信息
                                    newOrder.InvoiceType = InvoiceType.SpecialInvoice;
                                    _omsAccessor.Update<Order>(newOrder);
                                }
                                //invoiceInfo.InvoiceNo//发票编码
                                //invoiceInfo.BankCode 默认不填
                                _omsAccessor.Insert<InvoiceInfo>(invoiceInfo);
                            }
                        }
                        _omsAccessor.SaveChanges();
                        #endregion


                        //订单商品（商品库存不足，无法下单）
                        #region OrderProduct 订单商品
                        var products = new List<OrderProduct>();//订单列表中的商品
                        foreach (var pro in item.Products)
                        {
                            string pCode = pro.ProductCode;
                            var salePro = _omsAccessor.Get<SaleProduct>().Join(_omsAccessor.Get<Product>().Where(p => p.Code == pCode), o => o.ProductId, p => p.Id, (o, p) => new { o, p })
                                        .Select(r => r.o).FirstOrDefault();
                            var suitPro = _omsAccessor.Get<SuitProducts>().Where(r => r.Code == pCode)
                                        .Join(_omsAccessor.Get<SuitProductsDetail>(), s => s.Id, sd => sd.SuitProductsId, (s, sd) => new { s, sd }).Select(r => r.sd)
                                        .OrderByDescending(r => r.Quantity).ToList();
                            if (salePro != null)
                            {
                                //单款商品情况
                                products.Add(new OrderProduct()
                                {
                                    SaleProductId = salePro.Id,
                                    Quantity = (int)pro.Quantity,
                                    OrginPrice = _omsAccessor.Get<SaleProductPrice>().Where(r => r.SaleProductId == salePro.Id).FirstOrDefault().Price,
                                    SumPrice = (decimal)pro.SumPrice
                                });
                            }
                            else if (suitPro != null && suitPro.Count > 0)
                            {
                                //套装商品情况

                                //套账商品中的单个商品价格及单款商品总价
                                var p = _omsAccessor.Get<SaleProductPrice>()
                                    .Join(suitPro, s => s.SaleProductId, sp => sp.SaleProductId, (s, sp) => new { s, sp })
                                    .Select(r => new
                                    {
                                        r.s.SaleProductId,
                                        r.s.Price,
                                        r.sp.Quantity,
                                        SuitProSumPrice = r.s.Price * r.sp.Quantity
                                    });
                                var totalPrice = p.Sum(r => r.SuitProSumPrice);
                                //套装单款商品总价格及占比
                                var spp = p.Select(r => new { r.SaleProductId, r.SuitProSumPrice, Proportion = r.SuitProSumPrice / totalPrice });
                                //套装实付总价
                                var thisSuitProSumPrice = pro.SumPrice;


                                foreach (var sItem in suitPro)
                                {
                                    products.Add(new OrderProduct()
                                    {
                                        SaleProductId = sItem.SaleProductId,
                                        Quantity = sItem.Quantity * (int)pro.Quantity,
                                        OrginPrice = _omsAccessor.Get<SaleProductPrice>().Where(r => r.SaleProductId == sItem.SaleProductId).FirstOrDefault().Price,
                                        SumPrice = spp.Where(r => r.SaleProductId == sItem.SaleProductId).FirstOrDefault().Proportion * thisSuitProSumPrice
                                    });
                                }
                            }
                            else
                            {
                                //找不到商品，不添加订单
                                trans.Rollback();
                                _logService.Error("订单创建接口：平台单号[" + newOrder.PSerialNumber + "]无法在OMS找到商品编码为：" + pCode + "的销售商品，无法创建订单");
                                resultStr += item.PSerialNumber + "创建订单失败，无法找到商品编码为" + pCode + "的销售商品\n";
                                nextLoop = true;
                                break;
                            }
                        }
                        var finalProList = products.GroupBy(r => r.SaleProductId).Select(r => new OrderProduct
                        {
                            SaleProductId = r.FirstOrDefault().SaleProductId,
                            OrginPrice = r.FirstOrDefault().OrginPrice,
                            Quantity = r.Sum(re => re.Quantity),
                            SumPrice = r.Sum(re => re.SumPrice)
                        }).OrderByDescending(r => r.Quantity);

                        var i = 0;//用于判断当前是第几个商品
                        var t = finalProList.Count();
                        decimal countSumPrice = 0;
                        decimal orderSumPrice = finalProList.Sum(r => r.SumPrice);
                        foreach (var fItem in finalProList)
                        {
                            i++;
                            OrderProduct orderProduct = new OrderProduct();
                            orderProduct.OrderId = newOrder.Id;
                            orderProduct.SaleProductId = fItem.SaleProductId;
                            orderProduct.OrginPrice = fItem.OrginPrice;

                            if (_commonService.GetNewDecimalNotRounding(fItem.SumPrice % fItem.Quantity) == 0 && fItem.SumPrice != 0)
                            {
                                //除尽
                                orderProduct.Quantity = fItem.Quantity;
                                orderProduct.Price = _commonService.GetNewDecimalNotRounding(fItem.SumPrice / fItem.Quantity);
                                orderProduct.SumPrice = fItem.SumPrice;

                                countSumPrice += fItem.SumPrice;
                            }
                            else
                            {
                                //除不尽
                                if (i == t && fItem.Quantity > 1)
                                {
                                    //最后一款商品，拆分
                                    // 最后总价 = orderSumPrice - countSumPrice;
                                    var zuihouZongjia = orderSumPrice - countSumPrice;


                                    orderProduct.Quantity = fItem.Quantity - 1;
                                    orderProduct.Price = _commonService.GetNewDecimalNotRounding(fItem.SumPrice / fItem.Quantity);
                                    orderProduct.SumPrice = orderProduct.Quantity * orderProduct.Price;

                                    countSumPrice += orderProduct.SumPrice;


                                    //最后一款商品
                                    OrderProduct lastPro = new OrderProduct();
                                    lastPro.OrderId = newOrder.Id;
                                    lastPro.SaleProductId = fItem.SaleProductId;
                                    lastPro.OrginPrice = fItem.OrginPrice;
                                    lastPro.Quantity = 1;
                                    lastPro.Price = _commonService.GetNewDecimalNotRounding(orderSumPrice - countSumPrice);
                                    lastPro.SumPrice = orderSumPrice - orderProduct.SumPrice;

                                    _omsAccessor.Insert<OrderProduct>(lastPro);
                                }
                                else if (i == t && fItem.Quantity == 1)
                                {
                                    var zuihouZongjia = orderSumPrice - countSumPrice;
                                    orderProduct.Quantity = fItem.Quantity;
                                    orderProduct.Price = fItem.SumPrice;
                                    orderProduct.SumPrice = fItem.SumPrice;
                                }
                                else
                                {
                                    var danjia = fItem.SumPrice / fItem.Quantity;

                                    orderProduct.Quantity = fItem.Quantity;
                                    orderProduct.Price = _commonService.GetNewDecimalNotRounding(danjia);
                                    orderProduct.SumPrice = orderProduct.Price * orderProduct.Quantity;

                                    countSumPrice += orderProduct.SumPrice;
                                }
                            }
                            //锁定销售商品
                            SaleProduct saleProduct = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == orderProduct.SaleProductId).FirstOrDefault();
                            saleProduct.LockStock += orderProduct.Quantity;
                            saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                            if (saleProduct.AvailableStock < 0)
                            {
                                trans.Rollback();
                                _logService.Error("订单创建接口：平台单号[" + item.PSerialNumber + "]库存不足无法创建订单");
                                resultStr += item.PSerialNumber + "库存不足无法创建订单\n";
                                nextLoop = true;
                                break;
                            }
                            _omsAccessor.Update<SaleProduct>(saleProduct);
                            _omsAccessor.Insert<OrderProduct>(orderProduct);
                            _omsAccessor.SaveChanges();
                        }
                        #endregion


                        if (!nextLoop)
                        {
                            newOrder.SumPrice = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == newOrder.Id).Sum(r => r.SumPrice);
                            _omsAccessor.Update<Order>(newOrder);
                            _omsAccessor.SaveChanges();
                            trans.Commit();
                            //解除缺货
                            _productService.UnLockLackStockOrder(new List<int>() { newOrder.Id });
                            resultStr += item.PSerialNumber + ":创建订单成功\n";
                        }
                    }
                    catch(Exception e)
                    {
                        trans.Rollback();
                        _logService.Error("订单创建接口：平台单号[" + item.PSerialNumber + "]无法创建订单：" + e.Message);
                        resultStr += item.PSerialNumber + ":数据错误，无法创建订单\n";
                        continue;
                    }
                }
            }
            return resultStr;
        }
        /// <summary>
        /// （接口调用）获取订单信息
        /// </summary>
        /// <param name="pSerialNumber"></param>
        /// <returns></returns>
        public IEnumerable<InterfaceOrderModel> ApiGetOrderInfo(string pSerialNumber)
        {
            var dic = _omsAccessor.Get<Dictionary>().ToList();
            var wh = _omsAccessor.Get<WareHouse>().ToList();
            var orderStateStr = GetOrderStateStr();
            var data = _omsAccessor.Get<Order>().Where(r => r.PSerialNumber.Contains(pSerialNumber))
               .Select(r => new InterfaceOrderModel()
               {
                   OrderId = r.Id,
                   SerialNumber = r.SerialNumber,
                   PSerialNumber = r.PSerialNumber ?? "",
                   OrgionSerialNumber = r.OrgionSerialNumber ?? "",
                   State = (int)r.State,
                   StateName = orderStateStr[r.State],
                   PriceTypeId = r.PriceTypeId,
                   PayType = r.PayType,
                   PayTypeName = dic.Where(d => d.Id == r.PayType).FirstOrDefault().Value ?? "",
                   PayMentType = r.PayMentType,
                   PayMentTypeName = dic.Where(d => d.Id == r.PayMentType).FirstOrDefault().Value ?? "",
                   PayState = r.PayState,
                   SumPrice = r.SumPrice,
                   PayPrice = r.PayPrice,
                   DeliveryTypeId = r.DeliveryTypeId,
                   DeliveryNumber = r.DeliveryNumber ?? "",
                   CustomerId = r.CustomerId,
                   CustomerIdName = _omsAccessor.Get<Customers>().Where(c => c.Id == r.CustomerId).FirstOrDefault().Name,
                   CustomerName = r.CustomerName,
                   CustomerPhone = r.CustomerPhone ?? "",
                   CustomerAddressDetail = r.AddressDetail ?? "",
                   CustomerMark = r.CustomerMark ?? "",
                   AdminMark = r.AdminMark ?? "",
                   ToWarehouseMessage = r.ToWarehouseMessage ?? "",
                   WarehouseId = r.WarehouseId,
                   WarehouseName = wh.Where(w => w.Id == r.WarehouseId).FirstOrDefault().Name,
                   UserName = r.UserName ?? "",
                   InvoiceType = r.InvoiceType,
                   IsNeedPaperBag = r.IsNeedPaperBag,
                   SalesManId = r.SalesManId ?? 0,
                   FinanceMark = r.FinanceMark ?? "",
                   PayDate = r.PayDate.ToString() ?? "",
                   CreatedTime = r.CreatedTime,
                   InvoiceInfo = _omsAccessor.Get<InvoiceInfo>().Where(ii => ii.OrderId == r.Id).OrderByDescending(ii => ii.CreatedTime).FirstOrDefault(),
                   OrderPayPrice = _omsAccessor.Get<OrderPayPrice>().Where(opp => opp.OrderId == r.Id).ToList(),
                   Products = _omsAccessor.Get<OrderProduct>().Where(op => op.OrderId == r.Id)
                   .Join(_omsAccessor.Get<SaleProduct>(), op => op.SaleProductId, sp => sp.Id, (op, sp) => new { op, sp })
                   .Join(_omsAccessor.Get<Product>(), rp => rp.sp.ProductId, p => p.Id, (rp, p) => new { rp, p }).Select(rs => new InterFaceOrderProduct
                   {
                       ProductName = rs.p.Name,
                       ProductCode = rs.p.Code,
                       Quantity = rs.rp.op.Quantity,
                       OriginPrice = rs.rp.op.OrginPrice,
                       Price = rs.rp.op.Price,
                       SumPrice = rs.rp.op.SumPrice
                   }).ToList()

               }).OrderBy(r => r.CreatedTime).ToList();
            return data;
        }
        /// <summary>
        /// （接口调用）线下店退单接口
        /// </summary>
        /// <param name="pSerialNumber"></param>
        /// <returns></returns>
        public string ApiOffLineCancelOrder(string pSerialNumber)
        {
            using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                _logService.Info("[ApiOffLineCancelOrder]" + pSerialNumber + "退单调用进入");
                try
                {
                    //包含合并/拆分/复制订单
                    var orders = _omsAccessor.Get<Order>().Where(r => r.PSerialNumber.Contains(pSerialNumber) && r.Type == OrderType.B2C_XH).ToList();
                    if (orders != null && orders.Count > 0)
                    {
                        if (orders.Where(r => r.State == OrderState.Invalid).ToList().Count == orders.Count)
                        {
                            _logService.Error(pSerialNumber + "线下系统退单失败：订单状态已为无效状态");
                            return "订单状态已为无效状态，无法退单";
                        }
                    }
                    if (orders != null && orders.Count > 0)
                    {
                        foreach (var order in orders.Where(r => r.State != OrderState.Invalid))
                        {
                            if (_workContext.CurrentUser == null)
                            {
                                _workContext.CurrentUser = _omsAccessor.Get<User>().Where(r => r.UserName == "admin").FirstOrDefault();
                            }
                            if (order.State == OrderState.Paid || order.State == OrderState.B2CConfirmed)
                            {
                                order.State = OrderState.Invalid;
                                _omsAccessor.Update<Order>(order);
                                _omsAccessor.SaveChanges();
                                #region 日志
                                _logService.InsertOrderLog(order.Id, "设置订单无效", order.State, order.PayState, "接口设置订单无效");
                                #endregion


                                //设置无效单之后修改订单里面销售商品的库存信息
                                var productStockDataList = new List<ProductStockData>();
                                List<OrderProduct> orderProducts = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == order.Id).ToList();
                                foreach (var itemOrderProduct in orderProducts)
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
                                        stock_num = (saleProduct.AvailableStock-(xuniStock.Sum(r=>r.Stock)-xuniStock.Sum(r=>r.LockStock))).ToString(),
                                        stock_detail_list = _productService.GetSaleProductWareHouseStocksByProductCode(saleProduct.Product.Code)
                                    };
                                    productStockDataList.Add(productStockData);
                                    //释放库存
                                    var saleProLT = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == order.Id && r.OrderProductId == itemOrderProduct.Id).FirstOrDefault();
                                    if (saleProLT != null)
                                    {
                                        //锁定库存记录锁定数为负数情况？
                                        if (saleProLT.LockNumber > 0)
                                        {
                                            var saleProWHStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProLT.SaleProductId && r.WareHouseId == order.WarehouseId).FirstOrDefault();
                                            if (saleProWHStock != null)
                                            {
                                                saleProWHStock.LockStock -= saleProLT.LockNumber;
                                                saleProLT.LockNumber = 0;
                                                _omsAccessor.Update<SaleProductWareHouseStock>(saleProWHStock);
                                                _omsAccessor.Update<SaleProductLockedTrack>(saleProLT);
                                            }
                                            //如果saleProWHStock的stock为0，但saleProLT锁定数不为零情况？
                                        }
                                    }
                                    _omsAccessor.SaveChanges();
                                }
                                //批量更新库存到商城
                                _productService.SyncMoreProductStockToAssist(productStockDataList);
                            }
                            else if (order.State == OrderState.Uploaded)
                            {
                                trans.Rollback();
                                _logService.Error(pSerialNumber + "线下系统退单失败：订单状态为已上传");
                                return "订单状态为已上传，无法修改订单为无效";
                            }
                            else if (order.State == OrderState.Delivered)
                            {
                                trans.Rollback();
                                _logService.Error(pSerialNumber + "线下系统退单失败：订单状态为已发货");
                                return "订单状态为已发货，无法修改订单为无效";
                            }
                        }
                        trans.Commit();
                        _logService.Info(pSerialNumber + "线下系统退单成功！");
                        return "设置订单无效成功";
                    }
                    _logService.Error(pSerialNumber + "线下系统退单失败：无法找到此订单");
                    return "无法找到此订单";
                }
                catch(Exception e)
                {
                    trans.Rollback();
                    _logService.Error(pSerialNumber + "线下系统退单错误：" + e.Message);
                    return "设置订单无效错误";
                }
            }
        }
        /// <summary>
        /// 线下单订单状态回传
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public string ApiReturnOffLineOrderState(List<Order> orders)
        {
            var resultStr = "";
            foreach(var item in orders)
            {
                if (item.PSerialNumber.Contains("XS"))
                {
                    //处理SSL问题
                    var httpClientHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                    };
                    using (var http = new HttpClient(httpClientHandler))
                    {
                        var url = _configuration.GetSection("OffLine")["OffLineApiUrl"] + "/api/order/OmsUpdateOrderState";
                        var encyData = EncryptTools.AESEncrypt(_configuration["key"] + "|" + DateTime.Now.ToString("yyyyMMddHHmmss"), _configuration["key"]);
                        var sign = Convert.ToBase64String(Encoding.UTF8.GetBytes(encyData));
                        var data = JsonConvert.SerializeObject(new { SerialNumber = item.PSerialNumber, State = 6, StateReason = "订单已发货", Sign = sign });
                        StringContent str = new StringContent(data, Encoding.UTF8, "application/json");
                        var response = http.PostAsync(url, str);
                        try
                        {
                            if (response.Result.IsSuccessStatusCode)
                            {
                                dynamic content = response.Result.Content.ReadAsStringAsync();
                                dynamic responseData = JsonConvert.DeserializeObject(content.Result);
                                _logService.Info("线下店接口订单状态返回:" + responseData);
                                if (responseData["isSuccess"] == true)
                                {
                                    if (((string)responseData["message"]).Contains("成功"))
                                    {
                                        _logService.InsertOrderLog(item.Id, "发货状态回传", item.State, item.PayState, "线下订单发货状态回传成功！");
                                        resultStr += item.SerialNumber + ":发货状态回传成功！\n";
                                    }
                                    else
                                    {
                                        _logService.InsertOrderLog(item.Id, "发货状态回传", item.State, item.PayState, "线下订单发货状态回传失败！" + responseData["message"]);
                                        resultStr += item.SerialNumber + ":发货状态回传失败！\n";
                                    }
                                }
                                else
                                {
                                    _logService.InsertOrderLog(item.Id, "发货状态回传", item.State, item.PayState, "线下订单发货状态回传失败！" + responseData["message"]);
                                    resultStr += item.SerialNumber + ":发货状态回传失败！\n";
                                }
                            }
                            else
                            {
                                _logService.InsertOrderLog(item.Id, "发货状态回传", item.State, item.PayState, "线下订单发货状态回传失败！");
                                resultStr += item.SerialNumber + ":发货状态回传失败！\n";
                            }
                        }
                        catch (Exception e)
                        {
                            _logService.Error("[ApiReturnOffLineOrderState]:" + e.Message);
                            _logService.InsertOrderLog(item.Id, "发货状态回传", item.State, item.PayState, "线下订单发货状态回传失败(有错误)！");
                            resultStr += item.SerialNumber + ":发货状态回传有错误！\n";
                        }
                    }
                }
            }
            return resultStr;
        }
        #endregion


    }
}
