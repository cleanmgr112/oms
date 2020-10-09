using MerchantPortalOpenSDK;
using MerchantPortalOpenSDK.Model.Request;
using MerchantPortalOpenSDK.Model.Response;
using MerchantPortalOpenSDKDemo;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula.Functions;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Domain.CMB;
using OMS.Data.Interface;
using OMS.Services.Common;
using OMS.Services.Log;
using OMS.Services.Order1;
using OMS.Services.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services.CMB
{
    public class CMBService : ServiceBase, ICMBService
    {
        #region ctor
        private readonly ICommonService _commonService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ILogService _logService;
        public CMBService(IDbAccessor omsAccessor, IWorkContext workContext, ICommonService commonService, IConfiguration configuration, IOrderService orderService
            , IProductService productService, ILogService logService)
    : base(omsAccessor, workContext, configuration)
        {
            _commonService = commonService;
            _orderService = orderService;
            _productService = productService;
            _logService = logService;
        }
        #endregion


        /// <summary>
        /// 获取全部未发货订单
        /// </summary>
        /// <returns></returns>
        public string GetAllOrderList(DateTime? startTime=null,DateTime? endTime=null)
        {
            string starttime, endtime;
            if (!startTime.HasValue)
            {
                starttime = DateTime.Now.AddDays(-15).ToString("yyyyMMddHHmmss");
            }
            else
            {
                starttime = DateTime.Parse(((DateTime)startTime).ToString()).ToString("yyyyMMddHHmmss");
            }
            if (!endTime.HasValue)
            {
                endtime = DateTime.Now.AddDays(1).AddSeconds(-1).ToString("yyyyMMddHHmmss");
            }
            else
            {
                endtime = DateTime.Parse(((DateTime)endTime).ToString()).AddDays(1).AddSeconds(-1).ToString("yyyyMMddHHmmss");
            }


            //1.创建客户端（传入请求地址（默认可不传），mid，aid，商户私钥等参数）
            IMerchantPortalApiClient client = new DefaultMerchantPortalApiClient(
                _configuration.GetSection("Zhaohang")["API_URL"], 
                _configuration.GetSection("Zhaohang")["MID"],
                _configuration.GetSection("Zhaohang")["AID"], 
                _configuration.GetSection("Zhaohang")["CMBLIFE_XML_PUB_KEY"], 
                _configuration.GetSection("Zhaohang")["MERCHANT_XML_PRI_KEY"]);
            //2.添加参数
            var request = new MerchantPortalRequest();
            //2.1 添加要请求的方法名
            request.FuncName = "funmallGetOrderList";
            //2.2 添加业务参数
            var businessParamsDic = new Dictionary<string, object>();
            businessParamsDic.Add("merchantNo", _configuration.GetSection("Zhaohang")["merchantNo"]);//正式商户编号
            businessParamsDic.Add("storeNo", _configuration.GetSection("Zhaohang")["storeNo"]);//正式店铺号
            businessParamsDic.Add("pageSize", 50);
            businessParamsDic.Add("startTime", starttime);
            businessParamsDic.Add("endTime", endtime);


            //测试数据
            //businessParamsDic.Add("rmaSysNo", 193497238872420352);
            //businessParamsDic.Add("merchantNo", "mer20473");
            //businessParamsDic.Add("storeNo", "ST20313");

            

            //请求参数包含业务参数
            request.BusinessParams = businessParamsDic;
            //3. 执行
            MerchantPortalResponse<object> response = client.Execute<object>(request);

            return JsonConvert.SerializeObject(response);
        }
        /// <summary>
        /// 插入订单到OMS
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string InsertOrderToOMS(string response)
        {
            dynamic data = JsonConvert.DeserializeObject(response);
            if (data.Data == null)
            {
                return "同步完成，没有获取到招行订单！";
            }
            //判断是否已有平台单号
           foreach (var item in data.Data)
            {
                using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
                {
                    string pNum = (string)item.orderNo;
                    var nextLoop = false;
                    try
                    {
                        if (Environment.StackTrace.Contains("ScheduleTaskFuncService.SyncCMBOrderToOMS"))
                        {
                            //如果当前调用为定时任务调用，则设置默认用户为admin
                            _workContext.CurrentUser = _omsAccessor.Get<User>().Where(r => r.UserName == "admin").FirstOrDefault();
                        }
                        var isExistOrder = _omsAccessor.Get<Order>().Where(r => r.PSerialNumber.Contains(pNum)).FirstOrDefault();
                        if (isExistOrder != null)
                        {
                            continue;
                        }
                        #region Order 订单
                        Order order = new Order();
                        //订单主体
                        order.SerialNumber = _commonService.GetOrderSerialNumber();
                        order.PSerialNumber = (string)item.orderNo;
                        order.Type = OrderType.B2C_XH;
                        order.ShopId = int.Parse(_configuration.GetSection("Zhaohang")["shopId"]);//订单所属店铺ID，已写在配置文件中
                        order.State = OrderState.Paid;
                        order.WriteBackState = WriteBackState.NoWrite;


                        //支付信息
                        order.PayType = int.Parse(_configuration.GetSection("Zhaohang")["payType"]);//统一结算，已写在配置文件中
                        order.PayMentType = 0;//回款方式B2B所属部分
                        order.PayState = PayState.Success;
                        string d = (string)item.payTime;
                        order.PayDate = DateTime.ParseExact((string)item.payTime, "yyyyMMddHHmmss", null);
                        order.SumPrice = 0;//后面更新
                        order.PayPrice = (decimal)item.invoiceAmt;//发票金额，即支付金额  订单总优惠金额【店铺维度】 promotionAmt= soAmt + expressFeeAmt - invoiceAmt


                        //其他控制部分
                        order.IsLackStock = false;
                        order.LockMan = 0;
                        order.LockStock = false;
                        order.IsLackStock = false;
                        order.Isvalid = true;
                        order.IsCopied = false;
                        order.AppendType = 0;//合并/拆分订单
                        order.IsNeedPaperBag = false;//是否需要纸袋？未找到判断标志


                        //物流信息
                        order.DeliveryTypeId = int.Parse(_configuration.GetSection("Zhaohang")["defaultDelivery"]);//设置默认物流（顺丰）,已写在配置文件中
                        //order.DeliveryNumber
                        //order.DeliveryDate


                        //客户信息部分
                        order.UserName = (string)item.receiveContact;
                        order.CustomerName = (string)item.receiveContact;
                        order.CustomerPhone = (string)item.receiveCellPhone;
                        order.AddressDetail = _commonService.MatchProvince((string)item.receiveProvince) + " " + (string)item.receiveCity + " " + (string)item.receiveZone + " "
                                            + (string)item.receiveStreet + " " + (string)item.receiveAddress;
                        order.DistrictId = 0;
                        order.CustomerMark = (string)item.orderMemo;
                        //order.AdminMark = decimal.Parse((string)item.promotionAmt) == 0 ? "" : "套装总价：" + (decimal)item.soAmt + " 订单总优惠金额：" + (string)item.promotionAmt;//填写优惠信息
                        order.AdminMark = "箱内放入售后服务卡，箱子上贴上招行贴纸，并使用招行胶带封箱。";//填写优惠信息
                        order.WarehouseId = _orderService.MatchFirstWareHouseId(order.AddressDetail);//匹配仓库
                        //order.FinanceMark
                        //order.ToWarehouseMessage 自行填写


                        //优惠信息部分
                        order.ZMCoupon = 0;
                        order.ZMWineCoupon = 0;
                        order.WineWorldCoupon = 0;
                        //order.ProductCoupon
                        //order.ZMIntegralValuePrice
                        //order.OriginalOrderId
                        //order.SalesManId


                        //发票部分
                        order.InvoiceType = InvoiceType.NoNeedInvoice;


                        //其他
                        order.PriceTypeId = 103;//或者0
                        order.CustomerId = 0;
                        order.ApprovalProcessId = 0;
                        order.InvoiceMode = 0;//开票方式

                        _omsAccessor.Insert<Order>(order);
                        _omsAccessor.SaveChanges();
                        #region 订单日志
                        _logService.InsertOrderLog(order.Id, "新增订单", order.State, order.PayState, "招行接口新增订单");
                        #endregion

                        #endregion


                        #region PayInfo 支付信息
                        OrderPayPrice orderPayPrice = new OrderPayPrice();
                        orderPayPrice.OrderId = order.Id;
                        orderPayPrice.IsPay = true;
                        orderPayPrice.PayType = int.Parse(_configuration.GetSection("Zhaohang")["payType"]);
                        orderPayPrice.PayMentType = 0;
                        orderPayPrice.Price = order.PayPrice;
                        _omsAccessor.Insert<OrderPayPrice>(orderPayPrice);
                        #endregion


                        #region InvoiceInfo 发票信息
                        if (int.Parse((string)item.invoiceType) != 9)
                        {
                            //增票部分
                            InvoiceInfo invoiceInfo = new InvoiceInfo();
                            invoiceInfo.OrderId = order.Id;
                            invoiceInfo.CustomerEmail = (string)item.invoiceReceiveEmail;
                            invoiceInfo.Title = (string)item.invoiceTitle;

                            if (item.vatInvoiceInfo != null)
                            {
                                invoiceInfo.TaxpayerID = (string)item.vatInvoiceInfo.taxpayerIdent;//纳税人识别码
                                invoiceInfo.RegisterAddress = (string)item.vatInvoiceInfo.registeredAddress;
                                invoiceInfo.RegisterTel = (string)item.vatInvoiceInfo.registeredPhone;
                                invoiceInfo.BankAccount = (string)item.vatInvoiceInfo.bankAccount;//银行账号
                                invoiceInfo.BankOfDeposit = (string)item.vatInvoiceInfo.depositBank;//银行支行信息
                                order.InvoiceType = InvoiceType.SpecialInvoice;
                                _omsAccessor.Update<Order>(order);
                            }
                            //invoiceInfo.InvoiceNo//发票编码
                            //invoiceInfo.BankCode 默认不填
                            _omsAccessor.Insert<InvoiceInfo>(invoiceInfo);
                        }
                        _omsAccessor.SaveChanges();
                        #endregion


                        #region OrderProduct 订单商品
                        var cmbOrderPros = new List<OrderProduct>();//订单列表中的商品

                        foreach (var pro in item.shoppingList)
                        {
                            string pCode = (string)pro.sellerSku;
                            var salePro = _omsAccessor.Get<SaleProduct>().Join(_omsAccessor.Get<Product>().Where(p => p.Code == pCode), o => o.ProductId, p => p.Id, (o, p) => new { o, p })
                                        .Select(r => r.o).FirstOrDefault();
                            var suitPro = _omsAccessor.Get<SuitProducts>().Where(r => r.Code == pCode)
                                        .Join(_omsAccessor.Get<SuitProductsDetail>(), s => s.Id, sd => sd.SuitProductsId, (s, sd) => new { s, sd }).Select(r => r.sd)
                                        .OrderByDescending(r => r.Quantity).ToList();

                            if (salePro != null)
                            {
                                //单款商品情况
                                cmbOrderPros.Add(new OrderProduct()
                                {
                                    SaleProductId = salePro.Id,
                                    Quantity = (int)pro.quantity,
                                    OrginPrice = _omsAccessor.Get<SaleProductPrice>().Where(r => r.SaleProductId == salePro.Id).FirstOrDefault().Price,
                                    SumPrice = (decimal)pro.salePrice
                                });
                            }
                            else if(suitPro != null && suitPro.Count > 0)
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
                                var thisSuitProSumPrice = decimal.Parse((string)pro.salePrice);


                                foreach (var sItem in suitPro)
                                {
                                    cmbOrderPros.Add(new OrderProduct()
                                    {
                                        SaleProductId = sItem.SaleProductId,
                                        Quantity = sItem.Quantity * (int)pro.quantity,
                                        OrginPrice = _omsAccessor.Get<SaleProductPrice>().Where(r => r.SaleProductId == sItem.SaleProductId).FirstOrDefault().Price,
                                        SumPrice = spp.Where(r => r.SaleProductId == sItem.SaleProductId).FirstOrDefault().Proportion * thisSuitProSumPrice
                                    });
                                }
                            }
                            else
                            {
                                //找不到商品，不添加订单
                                trans.Rollback();
                                _logService.InsertedCMBOrderLog(pNum, "无法在OMS找到商品编码为：" + pCode + "的商品");
                                nextLoop = true;
                                break;
                            }
                        }
                        var finalProList = cmbOrderPros.GroupBy(r => r.SaleProductId).Select(r => new OrderProduct
                        {
                            SaleProductId = r.FirstOrDefault().SaleProductId,
                            OrginPrice = r.FirstOrDefault().OrginPrice,
                            Quantity = r.Sum(re => re.Quantity),
                            SumPrice = r.Sum(re => re.SumPrice)
                        }).OrderByDescending(r => r.Quantity);

                        var i = 0;//用于判断当前是第几个商品
                        var t = finalProList.Count();
                        decimal countSumPrice = 0;
                        decimal orderSumPrice = finalProList.Sum(r => r.SumPrice);//按照订单支付金额计算，未按订单金额算（订单金额 = 支付金额 + 快递费）
                        foreach (var fItem in finalProList)
                        {
                            i++;
                            OrderProduct orderProduct = new OrderProduct();
                            orderProduct.OrderId = order.Id;
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
                                    lastPro.OrderId = order.Id;
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
                            SaleProduct saleProduct = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == fItem.SaleProductId).FirstOrDefault();
                            saleProduct.LockStock += fItem.Quantity;
                            saleProduct.AvailableStock = saleProduct.Stock - saleProduct.LockStock;
                            if (saleProduct.AvailableStock < 0)
                            {
                                trans.Rollback();
                                _logService.InsertedCMBOrderLog(pNum, "商品库存不足，无法添加添加订单");
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
                            order.SumPrice = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == order.Id).Sum(r => r.SumPrice);
                            _omsAccessor.Update<Order>(order);
                            _omsAccessor.SaveChanges();
                            trans.Commit();
                            //解除缺货
                            _productService.UnLockLackStockOrder(new List<int>() { order.Id });
                            _logService.InsertedCMBOrderLog(pNum, "订单同步成功！");
                        }
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        _omsAccessor.Insert<CMBOrderLog>(new CMBOrderLog() { OrderNum = pNum, Message = e.Message });
                        _omsAccessor.SaveChanges();
                        _logService.Error("<InsertOrderToOMS> 信息：" + e.Message);
                        continue;
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 获取招行订单详情
        /// </summary>
        /// <param name="cmbOrderNo"></param>
        /// <returns></returns>
        public string GetOrderDetail(string cmbOrderNo)
        {

            //1.创建客户端（传入请求地址（默认可不传），mid，aid，商户私钥等参数）
            IMerchantPortalApiClient client = new DefaultMerchantPortalApiClient(
                _configuration.GetSection("Zhaohang")["API_URL"],
                _configuration.GetSection("Zhaohang")["MID"],
                _configuration.GetSection("Zhaohang")["AID"],
                _configuration.GetSection("Zhaohang")["CMBLIFE_XML_PUB_KEY"],
                _configuration.GetSection("Zhaohang")["MERCHANT_XML_PRI_KEY"]);
            //2.添加参数
            var request = new MerchantPortalRequest();
            //2.1 添加要请求的方法名
            request.FuncName = "funmallGetOrderBySysNo";
            //2.2 添加业务参数
            var businessParamsDic = new Dictionary<string, object>();
            businessParamsDic.Add("merchantNo", _configuration.GetSection("Zhaohang")["merchantNo"]);// 正式商户编号
            businessParamsDic.Add("storeNo", _configuration.GetSection("Zhaohang")["storeNo"]); //正式店铺号
            businessParamsDic.Add("orderNo", cmbOrderNo);


            //请求参数包含业务参数
            request.BusinessParams = businessParamsDic;
            //3. 执行
            MerchantPortalResponse<object> response = client.Execute<object>(request);

            return JsonConvert.SerializeObject(response);
        }
        /// <summary>
        /// 上传快递信息到招行系统（请在ReturnDeliverResultToCMB中调用）
        /// </summary>
        /// <param name="orderItemNo">cmb订单号</param>
        /// <param name="orderNo">cmb子订单号</param>
        /// <param name="deliveryType">快递名称</param>
        /// <param name="deliverName">快递中文名</param>
        /// <param name="deliverNo">快递单号</param>
        /// <returns></returns>
        public string UpLoadDeliverInfo(string orderItemNo,string orderNo,string deliveryType,string deliverName,string deliverNo)
        {
            //1.创建客户端（传入请求地址（默认可不传），mid，aid，商户私钥等参数）
            IMerchantPortalApiClient client = new DefaultMerchantPortalApiClient(
               _configuration.GetSection("Zhaohang")["API_URL"],
                _configuration.GetSection("Zhaohang")["MID"],
                _configuration.GetSection("Zhaohang")["AID"],
                _configuration.GetSection("Zhaohang")["CMBLIFE_XML_PUB_KEY"],
                _configuration.GetSection("Zhaohang")["MERCHANT_XML_PRI_KEY"]);
            //2.添加参数
            var request = new MerchantPortalRequest();
            //2.1 添加要请求的方法名
            request.FuncName = "funmallUploadLogisticsByItem";
            //2.2 添加业务参数
            var businessParamsDic = new Dictionary<string, object>();
            businessParamsDic.Add("merchantNo", _configuration.GetSection("Zhaohang")["merchantNo"]); //正式商户编号
            businessParamsDic.Add("storeNo", _configuration.GetSection("Zhaohang")["storeNo"]); //正式店铺号


            //快递信息
            var deliveryInfo = new Dictionary<string, object>();
            deliveryInfo.Add("orderItemNo", orderItemNo);//子订单编号
            deliveryInfo.Add("orderNo", orderNo);//订单编号
            var deliver = new Dictionary<string, object>();
            deliver.Add("shipTypeId", deliveryType);//快递类型
            deliver.Add("shipTypeName", deliverName);//快递名称
            deliver.Add("trackingNo", deliverNo);//快递单号
            deliveryInfo.Add("logisticsInfoList", deliver);//需上传物流信息列表
            businessParamsDic.Add("items", deliveryInfo);

            //请求参数包含业务参数
            request.BusinessParams = businessParamsDic;
            //3. 执行
            MerchantPortalResponse<object> response = client.Execute<object>(request);

            return JsonConvert.SerializeObject(response);

        }
        /// <summary>
        /// 返回订单状态到招行
        /// </summary>
        /// <param name="resultOrder"></param>
        /// <returns></returns>
        public string ReturnDeliverResultToCMB(Order resultOrder)
        {
            /* ********************************************************************
             *                                                                    *
             *      订单状态：                      快递类型：                    *
             *          -1  : 商家取消                  shunfeng  顺丰速运        *
             *          -2  : 客户取消                       ems  EMS             *
             *          -3  : 虚拟卡取消                shentong  申通快递        *
             *          -4  : 系统取消                 zhongtong  中通快递        *
             *          -10 : 待商家取消审核       huitongkuaidi  百世快递        *
             *           0  : 未支付              youzhengguonei  邮政快递包裹    *
             *           1  : 已支付                       yunda  韵达快递        *
             *           4  : 已发货                    yuantong  圆通快递        *
             *          100 : 已完成              dengbangkuaidi  德邦快递        *
             *                                                jd  京东物流        *
             *                                                                    *
             * ********************************************************************/


            #region 招行订单状态
            Dictionary<int, string> orderStatus = new Dictionary<int, string>();
            orderStatus.Add(-1, "商家取消");
            orderStatus.Add(-2, "客户取消");
            orderStatus.Add(-3, "虚拟卡取消");
            orderStatus.Add(-4, "系统取消");
            orderStatus.Add(-10, "待商家取消审核");
            orderStatus.Add(0, "未支付");
            orderStatus.Add(1, "已支付");
            orderStatus.Add(4, "已发货");
            orderStatus.Add(10, "已完成");
            #endregion


            var order = _orderService.GetOrderById(resultOrder.Id);
            dynamic data = JsonConvert.DeserializeObject(GetOrderDetail(order.PSerialNumber));
            if ((int)data.ResultType == 1000)
            {
                if ((int)data.Data.orderStatus == 1)
                {
                    //返回订单状态给招行系统
                    var deliver = _omsAccessor.Get<Delivery>().Where(r => r.Id == resultOrder.DeliveryTypeId).FirstOrDefault().Name;
                    var deliverType = "";
                    var deliverName = "";
                    //日后增加快递需添加
                    if (deliver.Contains("德邦"))
                    {
                        deliverType = "dengbangkuaidi";
                        deliverName = "德邦快递";
                    }
                    else if(deliver.Contains("顺丰"))
                    {
                        deliverType = "shunfeng";
                        deliverName = "顺丰速运";
                    }else if (deliver.Contains("京东"))
                    {
                        deliverType = "jd";
                        deliverName = "京东物流";
                    }
                    else
                    {
                        return "返回物流信息失败：请添加物流方式为[" + deliver + "]的物流信息到返回接口";
                    }


                    dynamic response = JsonConvert.DeserializeObject(UpLoadDeliverInfo((string)data.Data.shoppingList[0].orderItemNo, (string)data.Data.orderNo, deliverType, deliverName, resultOrder.DeliveryNumber));
                    if ((int)response.ResultType == 1000)
                    {
                        if ((int)response.Data.updateCount == 0)
                        {
                            _logService.InsertedCMBOrderLog(order.PSerialNumber, JsonConvert.SerializeObject(response));
                            _logService.Error("OMS发货成功通知信息异常：" +
                                order.PSerialNumber + "返回错误：" +
                                "错误码：" +
                                response.Data.errInfoList[0].errorCode +
                                "错误信息：" +
                                response.Data.errInfoList[0].errorCode);
                            return JsonConvert.SerializeObject(response);
                        }
                        return "";
                    }
                    else
                    {
                        _logService.InsertedCMBOrderLog(order.PSerialNumber, JsonConvert.SerializeObject(response));
                        _logService.Error(string.Format("OMS发货成功通知信息异常：{0}", JsonConvert.SerializeObject(response)));
                        return JsonConvert.SerializeObject(response);
                    }
                }
                else if (new List<int>() { -1, -2, -3, -4, -10 }.Contains((int)data.Data.orderStatus))
                {

                    #region 发邮件通知同事该订单发货完成并且订单已取消
                    var subject = "招行订单已取消，返回物流状态失败";
                    var body = "招行订单：" + order.PSerialNumber + " 当前状态为：" + orderStatus[(int)data.Data.orderStatus] + "无法取消上传物流单号，请手动处理订单！";
                    var sendTo = _configuration.GetSection("Zhaohang")["relatedMan"];
                    var mailFrom = _configuration.GetSection("MailAccount")["MailFrom"];
                    _commonService.SendEmailByAliYun(subject, body, sendTo, mailFrom, "红酒世界");
                    #endregion

                    _logService.InsertedCMBOrderLog(order.PSerialNumber, "当前订单状态已取消，无法上传物流信息！状态:" + orderStatus[(int)data.Data.orderStatus]);
                    _logService.Error("OMS发货成功通知信息异常：" + order.PSerialNumber + "订单状态为：" + orderStatus[(int)data.Data.orderStatus] + "不支持返回订单状态");
                    return "当前订单状态已取消，无法上传物流信息！";
                }
                else
                {
                    _logService.InsertedCMBOrderLog(order.PSerialNumber, "当前订单状态为：" + orderStatus[(int)data.Data.orderStatus] + " 不支持返回订单状态，请手动操作！");
                    _logService.Error("OMS发货成功通知信息异常：" + order.PSerialNumber + "订单状态为：" + orderStatus[(int)data.Data.orderStatus] + "不支持返回订单状态");
                    return "当前订单状态为：" + orderStatus[(int)data.Data.orderStatus] + " 不支持返回订单状态，请手动操作！";
                }
            }
            else
            {
                _logService.InsertedCMBOrderLog(order.PSerialNumber, "返回物流信息失败：" + "ResultType:" + data.ResultType + " Message: " + data.Message);
                _logService.Error(string.Format("OMS发货成功通知信息异常：{0}", "ResultType: " + data.ResultType + " Message: " + data.Message));
                return "返回物流信息失败：ResultType:" + data.ResultType + " Message:" + data.Message;
            }
        }
        /// <summary>
        /// 获取订单接口日志
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        public IEnumerable<CMBOrderLog> GetCMBOrderLog(string searchStr)
        {
            var data = _omsAccessor.Get<CMBOrderLog>().Where(r => (string.IsNullOrEmpty(searchStr) || r.OrderNum.Contains(searchStr) || r.Message.Contains(searchStr))).ToList();
            return data;
        }

    }
}
