using OMS.Core;
using OMS.Core.Json;
using OMS.Core.Tools;
using OMS.Data.Domain;
using OMS.Data.Interface;
using OMS.Model.JsonModel;
using OMS.Services.Common;
using OMS.Services.Deliveries;
using OMS.Services.Log;
using OMS.Services.Products;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OMS.Services.Order1
{
    
    public class OrderSyncService : ServiceBase, IOrderSyncService
    {
        #region ctor
        private IServiceProvider _rootProvider;
        private ICommonService _commonService;
        private IOrderService _orderService;
        private IProductService _productService;
        private IDeliveriesService _deliveriesService;
        private ILogService _logService;
        public OrderSyncService(IDbAccessor omsAccessor, IWorkContext workContext, ICommonService commonService, IOrderService orderService, IServiceProvider serviceProvider, IProductService productService,IDeliveriesService deliveriesService,ILogService logService)
            : base(omsAccessor, workContext)
        {
            _commonService = commonService;
            _orderService = orderService;
            _rootProvider = serviceProvider;
            _productService = productService;
            _deliveriesService = deliveriesService;
            _logService = logService;
        }
        #endregion

        /// <summary>
        ///抓取商城订单
        /// </summary>
        /// <param name="orderInfoList"></param>
        public void OrderSync(IList<OrderInfo> orderInfoList,string siId ,out List<OrderNotification> orderNotifications)
        {
             if (orderInfoList.Count > 0)
            {
                var orderNotificationResults = new List<OrderNotification>();

                foreach (var itemOrderInfo in orderInfoList)
                {
                    var order_sn = string.Empty;//OMS系统这边对应的SerialNumber
                    //抓取商城订单操作事务
                    using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
                    {
                        if (_orderService.GetOrderByPSerialNumber(itemOrderInfo.order_id) == null)
                        {
                            var insertErrorCount = 0;
                            var errInfo = "";
                            try
                            {
                                //订单插入
                                Order newOrder = new Order
                                {
                                    SerialNumber = _commonService.GetOrderSerialNumber("SC"),
                                    UserName = itemOrderInfo.user_name,
                                    PayState = PayState.Success,
                                    SumPrice = Convert.ToDecimal(itemOrderInfo.order_sum_price),
                                    PayPrice = Convert.ToDecimal(itemOrderInfo.order_fact_price),
                                    PayDate = DateTime.Now,
                                    CustomerName = itemOrderInfo.consignee_info.fullname,
                                    CustomerPhone = itemOrderInfo.consignee_info.mobile,
                                    AddressDetail = itemOrderInfo.consignee_info.province.Trim() + " " + itemOrderInfo.consignee_info.city.Trim() + " " + itemOrderInfo.consignee_info.county.Trim() + " " + itemOrderInfo.consignee_info.full_address.Trim(),
                                    CreatedBy = 0,//代表系统本身
                                    CustomerMark = itemOrderInfo.order_remark,//订单备注
                                    ShopId = 97,//97为网上商城在新系统数据库Dictionary表中的Id
                                    PSerialNumber = itemOrderInfo.order_id.Trim(),
                                    IsNeedPaperBag = itemOrderInfo.is_need_invoice,
                                };
                                //机场店地址
                                if (itemOrderInfo.consignee_info.full_address.Contains("深圳宝安机场到达出口直行150米大厅右侧（星巴克对面）"))
                                {
                                    newOrder.AddressDetail = "广东省" + " " + "深圳市" + " " + "宝安区" + " " + itemOrderInfo.consignee_info.full_address.Trim();
                                }
                                //匹配仓库
                                newOrder.WarehouseId = _orderService.MatchWarehouseId(itemOrderInfo.product_info_list,newOrder.AddressDetail);

                                //订单类型
                                string[] XHString = { "1", "2", "3", "10" };
                                if (((IList)XHString).Contains(itemOrderInfo.order_type))
                                {
                                    //商城现货
                                    newOrder.Type = OrderType.B2C_XH;
                                }
                                else if (itemOrderInfo.order_type == "4")
                                {
                                    //商城跨境
                                    newOrder.Type = OrderType.B2C_KJ;
                                }
                                else if (itemOrderInfo.order_type == "9")
                                {
                                    //合作商
                                    newOrder.Type = OrderType.B2C_HZS;
                                }

                                if (itemOrderInfo.order_state == "WAIT_SELLER_STOCK_OUT")

                                {
                                    newOrder.State = OrderState.Paid;
                                }
                                else if (itemOrderInfo.order_state == "PAY_REVOKE")
                                {
                                    newOrder.State = OrderState.Cancel;
                                }
                                //订单物流信息
                                newOrder.DeliveryTypeId = _deliveriesService.GetDeliveryByShopCode(itemOrderInfo.delivery_name.Trim())==null ?0 : _deliveriesService.GetDeliveryByShopCode(itemOrderInfo.delivery_name.Trim()).Id;
                                //客户自提修改快递方式
                                if (itemOrderInfo.seller_remark.Contains("自提码")) {
                                    newOrder.DeliveryTypeId = _deliveriesService.ConfirmDeliveryIsExist("KHZT") == null ? 0 : _deliveriesService.ConfirmDeliveryIsExist("KHZT").Id;
                                }
                                if (newOrder.DeliveryTypeId ==0) {
                                    _logService.InsertSystemLog(LogLevelEnum.AddOrderErr, "订单" + itemOrderInfo.order_id + "快递方式：快递方式不匹配" + itemOrderInfo.delivery_name);
                                    tran.Commit();
                                    insertErrorCount++;
                                    continue;
                                }

                                //订单发票类型
                                if (!itemOrderInfo.is_need_invoice)
                                {
                                    newOrder.InvoiceType = InvoiceType.NoNeedInvoice;
                                }
                                else
                                {
                                    switch (itemOrderInfo.order_invoice_info.invoice_title_type.Trim())
                                    {
                                        case "1":
                                            newOrder.InvoiceType = InvoiceType.PersonalInvoice;
                                            break;
                                        case "2":
                                            newOrder.InvoiceType = InvoiceType.CompanyInvoice;
                                            break;
                                        case "3":
                                            newOrder.InvoiceType = InvoiceType.SpecialInvoice;
                                            break;
                                    }
                                }

                                //订单支付类型
                                var payType = _omsAccessor.Get<Dictionary>().Where(x => x.Isvalid && x.Type == DictionaryType.PayType && x.Value.Trim().Contains("网银在线")).FirstOrDefault();
                                newOrder.PayType = payType == null ? 119 : payType.Id;

                                //下单时间
                                if (itemOrderInfo.order_generate_date.Trim()!="") {
                                    newOrder.CreatedTime = Convert.ToDateTime(itemOrderInfo.order_generate_date.Trim());
                                }
                                //支付时间
                                if (itemOrderInfo.order_paydate.Trim() != "") {
                                    newOrder.PayDate = Convert.ToDateTime(itemOrderInfo.order_paydate.Trim());
                                }
                                //商城代金券优惠
                                newOrder.ProductCoupon = itemOrderInfo.product_coupon_info;
                                newOrder.AdminMark = itemOrderInfo.seller_remark;
                                var zmjfPrice = itemOrderInfo.coupon_detail_list.Where(r => r.coupon_type.Equals("IntegralValue_New")).FirstOrDefault();
                                if (zmjfPrice != null)
                                    newOrder.ZMIntegralValuePrice = Convert.ToDecimal(zmjfPrice.coupon_price);
                                //订单所使用的中民券
                                var zmCoupon = itemOrderInfo.coupon_detail_list.Where(r => r.coupon_type.Equals("ZMCoupon")).FirstOrDefault();
                                if (zmCoupon != null)
                                    newOrder.ZMCoupon = float.Parse(zmCoupon.coupon_price);
                                else
                                    newOrder.ZMCoupon = 0;

                                //订单所使用的中民红酒券
                                var zmWineCoupon = itemOrderInfo.coupon_detail_list.Where(r => r.coupon_type.Equals("WineCoupon")).FirstOrDefault();
                                if (zmWineCoupon != null)
                                    newOrder.ZMWineCoupon = Math.Round(float.Parse(zmWineCoupon.coupon_price),2);
                                else
                                    newOrder.ZMWineCoupon = 0;
                                //订单所使用的的红酒网红酒券
                                var wineWorldCoupon = itemOrderInfo.coupon_detail_list.Where(r => r.coupon_type.Equals("WineWorldCoupon")).FirstOrDefault();
                                if (wineWorldCoupon != null)
                                    newOrder.WineWorldCoupon =Math.Round(float.Parse(wineWorldCoupon.coupon_price),2);
                                else
                                    newOrder.WineWorldCoupon = 0;

                                //总价要加上中民红酒券以及红酒网红酒券
                                newOrder.SumPrice += (decimal)newOrder.ZMWineCoupon;
                                newOrder.SumPrice += (decimal)newOrder.WineWorldCoupon;
                                _omsAccessor.Insert(newOrder);
                                _omsAccessor.SaveChanges();
                                #region 新增订单日志
                                _logService.InsertOrderLog(newOrder.Id, "新增订单", newOrder.State, newOrder.PayState, "商城接口新增订单");
                                #endregion
                                order_sn = newOrder.SerialNumber;
                                //订单商品插入
                                if (itemOrderInfo.product_info_list.Count > 0)
                                {
                                    foreach (var itemProduct in itemOrderInfo.product_info_list)
                                    {
                                        if (string.IsNullOrEmpty(itemProduct.goods_sn)) {
                                            errInfo += "订单" + itemOrderInfo.order_id + "商品插入失败：商品" + itemProduct.goods_name + "，编码为空或NULL!";
                                            insertErrorCount++;
                                            break;
                                        }
                                        SaleProduct saleProduct = _productService.GetSaleProductByGoodSn(itemProduct.goods_sn);
                                        if (saleProduct!= null && saleProduct.ProductId!=0 && saleProduct.Stock >= Convert.ToInt32(itemProduct.item_total.Trim()))
                                        {
                                            if (saleProduct.AvailableStock < Convert.ToInt32(itemProduct.item_total.Trim()))
                                            {
                                                errInfo += "订单" + itemOrderInfo.order_id + "商品插入失败：商品库存不足" + itemProduct.goods_name + "，商品编码为" + itemProduct.goods_sn;
                                                insertErrorCount++;
                                                break;
                                            }
                                            OrderProduct newOrderProduct = new OrderProduct
                                            {
                                                OrderId = newOrder.Id,
                                                SaleProductId = saleProduct.Id,
                                                Quantity = Convert.ToInt32(itemProduct.item_total.Trim()),
                                                OrginPrice = _productService.GetSaleProductPriceById(saleProduct.Id, 103).Price,
                                                Price = Convert.ToDecimal(itemProduct.sale_price.Trim()),
                                                SumPrice = Convert.ToInt32(itemProduct.item_total.Trim()) * Convert.ToDecimal(itemProduct.sale_price.Trim()),
                                                CreatedBy = 0

                                            };
                                            //销售商品总库存判断及锁定
                                            saleProduct.LockStock += Convert.ToInt32(itemProduct.item_total.Trim());
                                            saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                                            _omsAccessor.Update(saleProduct);
                                            _omsAccessor.Insert(newOrderProduct);
                                            _omsAccessor.SaveChanges();
                                            //新增锁定库存记录
                                            _productService.CreateSaleProductLockedTrackAndWareHouseStock(newOrder.Id, saleProduct.Id, newOrder.WarehouseId, newOrderProduct.Quantity, newOrderProduct.Id);
                                        }
                                        else
                                        {
                                            errInfo += "订单" + itemOrderInfo.order_id + "商品插入失败：不存在商品" + itemProduct.goods_name + "，商品编码为" + itemProduct.goods_sn;
                                            insertErrorCount++;
                                            break;
                                        }
                                    }
                                }


                                if (insertErrorCount == 0) {
                                    //插入发票信息
                                    if (newOrder.InvoiceType != InvoiceType.NoNeedInvoice)
                                    {
                                        InvoiceInfo newOrderInvoiceInfo = new InvoiceInfo
                                        {
                                            OrderId = newOrder.Id,
                                            CustomerEmail = string.IsNullOrEmpty(itemOrderInfo.order_invoice_info.InvoiceEmail)?"": itemOrderInfo.order_invoice_info.InvoiceEmail,
                                            Title = itemOrderInfo.order_invoice_info.invoice_title_info,
                                            TaxpayerID = itemOrderInfo.order_invoice_info.taxpayer_id,
                                            RegisterAddress = itemOrderInfo.order_invoice_info.register_address,
                                            RegisterTel = itemOrderInfo.order_invoice_info.register_tel,
                                            BankOfDeposit = itemOrderInfo.order_invoice_info.bank_of_deposit,
                                            BankAccount = itemOrderInfo.order_invoice_info.bank_account,
                                            CreatedBy = 0,
                                            
                                        };

                                        _omsAccessor.Insert(newOrderInvoiceInfo);
                                        _omsAccessor.SaveChanges();
                                    }


                                    //支付信息插入
                                    OrderPayPrice newOrderPayPrice = new OrderPayPrice
                                    {
                                        OrderId = newOrder.Id,
                                        IsPay = true,
                                        Price = Convert.ToDecimal(itemOrderInfo.order_fact_price.Trim()),
                                        CreatedBy = 0
                                    };

                                    if (itemOrderInfo.order_paydate.Trim() == "")
                                    {
                                        newOrderPayPrice.CreatedTime = DateTime.Now;
                                    }
                                    else
                                    {
                                        newOrderPayPrice.CreatedTime = Convert.ToDateTime(itemOrderInfo.order_paydate.Trim());
                                    }

                                    //订单支付类型
                                    newOrderPayPrice.PayType = payType == null ? 119 : payType.Id;

                                    _omsAccessor.Insert(newOrderPayPrice);
                                    _omsAccessor.SaveChanges();
                                }
 

                            }
                            catch (Exception ex)
                            {
                                insertErrorCount++;
                                errInfo += "订单" + itemOrderInfo.order_id + "插入错误" + ex.Message;
                            }
                            if (insertErrorCount > 0)
                            {
                                tran.Rollback();
                                //最后插入异常
                                _logService.InsertSystemLog(LogLevelEnum.AddOrderErr, errInfo);
                            }
                            else {
                                tran.Commit();

                                //获取成功下单的订单
                                var orderNatification = new OrderNotification
                                {
                                    order_id = itemOrderInfo.order_id,
                                    order_sn = order_sn,
                                    operation_result ="1",
                                    sd_id=siId
                                };
                                orderNotificationResults.Add(orderNatification);
                            }

                        }
                    }


                }

                orderNotifications = orderNotificationResults;
            }
            else
            {
                orderNotifications = null;
            }

        }


        /// <summary>
        /// OMS调用订单辅助系统接口参数
        /// </summary>
        /// <returns></returns>
        public OrderAssistParamsModel OrderAssistOmsApi(string method, string app_key, string v, string sd_id, string order_state, string page_no, string page_size, string data = null)
        {
            //string temp_app_key = app_key;

            //SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
            //sParams.Add("method",method);
            //sParams.Add("app_key", app_key);
            //sParams.Add("v",v);
            //sParams.Add("sd_id",sd_id);
            //sParams.Add("order_state", order_state);
            //sParams.Add("page_no",page_no);
            //sParams.Add("page_size",page_size);
            //sParams.Add("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            //if (!string.IsNullOrEmpty(data)) {
            //    sParams.Add("data",data);
            //}
            //string sign = CommonTools.getSign(sParams, temp_app_key);

            //var postUrl = _configuration.GetSection("OrderAssistOmsApi")["domain"];

            //string post_data = CommonTools.getPostData(sParams,sign);

            //string requestData = CommonTools.PostToUrl(postUrl, post_data);
            //return requestData;

            OrderAssistParamsModel orderAssistParamsModel = new OrderAssistParamsModel
            {
                method = method,
                app_key = app_key,
                v = v,
                sd_id = sd_id,
                order_state = order_state,
                page_no = page_no,
                page_size = page_size,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                data = data
            };
            string sgin = MD5Util.GetMD5_ToUpper(app_key, "utf-8");
            orderAssistParamsModel.sgin = sgin;
            return orderAssistParamsModel;
        }
    }
}
