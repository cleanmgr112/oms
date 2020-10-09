using OMS.Core;
using OMS.Data.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OMS.Data.Domain;
using System.Linq;
using OMS.Core.Json;
using System.Net.Http;
using OMS.Data.Domain.Purchasings;
using OMS.Model;
using Microsoft.Extensions.Configuration;
using OMS.Model.Products;
using OMS.Model.JsonModel;
using OMS.Data.Domain.Suppliers;
using OMS.Core.Tools;
using LitJson;
using OMS.Services.Order1;
using System.Threading;
using OMS.Services.Log;
using OMS.Services.Products;
using OMS.Services.CMB;

namespace OMS.Services.ScheduleTasks
{
    public class ScheduleTaskFuncService : ServiceBase, IScheduleTaskFuncService
    {
        #region ctor
        private readonly string WMSApiUrl;
        private readonly static object _MyLock = new object();

        private IOrderSyncService _orderSyncService;
        private ILogService _logService;
        private IProductService _productService;
        private ICMBService _cmbService;
        public ScheduleTaskFuncService(IDbAccessor omsAccessor,
            IWorkContext workContext,
            IConfiguration configuration,
            IOrderSyncService orderSyncService,
            ILogService logService,
            IProductService productService,
            ICMBService cmbService)
            : base(omsAccessor, workContext, configuration)
        {
            _orderSyncService = orderSyncService;
            WMSApiUrl = configuration.GetSection("WMSApi")["domain"].ToString();
            this._logService = logService;
            this._productService = productService;
            _cmbService = cmbService;
        }
        #endregion
        #region 测试定时任务
        public void SendMessage()
        {
            Console.WriteLine("发送消息123");
        }

        public void ReceviceMessage()
        {
            Console.WriteLine("接收消息123");
        }

        public void Test()
        {
            var time = DateTime.Now.ToString();
            Logger.Error("test方法此次执行时间" + time);
            Console.WriteLine("Test123");
        }
        #endregion
        #region 同步字典、商品数据到WMS
        /// <summary>
        /// 同步字典数据到WMS
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OmsSyncDictionaries()
        {
            var dictionaryies = _omsAccessor.Get<Dictionary>(d => d.Isvalid && !d.IsSynchronized).OrderBy(d => d.Id).ToList();
            var result = dictionaryies.Select(d => new { d.Type, d.Value, OMSId = d.Id }).OrderBy(p => p.OMSId);
            string jsonStr = result.ToJson();

            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                #region JWTBearer授权校验信息

                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

                #endregion

                var requestUrl = _configuration.GetSection("WMSApi")["domain"].ToString() + "/wmsapi/DictionarySync/OmsSyncDictionaries";
                var response = http.PostAsync(requestUrl, content).Result;
                string resultState = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) //未授权
                {
                    await GetWMSOauthToken();
                    _logService.Error("同步字典、商品数据到WMS失败，原因是API授权失败！请重试。");
                    //同步失败
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                { // http返回成功
                    var objResult = resultState.ToObj<WMSResult>();
                    if (objResult.state == "success")
                    {
                        foreach (var item in dictionaryies)
                        {
                            item.IsSynchronized = true;
                            _omsAccessor.Update(item);
                        }
                        _omsAccessor.SaveChanges();
                        return true;
                    }
                    else if (objResult.state == "error")
                    {
                        var successInfo = dictionaryies.Where(p => !objResult.errOMSId.Contains(p.Id));
                        if (successInfo.Count() > 0)
                        {
                            foreach (var item in successInfo)
                            {
                                item.IsSynchronized = true;
                                _omsAccessor.Update(item);
                            }
                            _omsAccessor.SaveChanges();
                        }
                        return false;
                    }
                    else
                    {
                        _logService.Error(string.Format("同步字典、商品数据到WMS失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        //同步失败
                        return false;
                    }
                }
                else
                {
                    //同步失败
                    return false;
                }

            }
        }

        /// <summary>
        /// 同步商品数据到WMS
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OmsSyncProducts()
        {
            try
            {
                var products = _omsAccessor.Get<Product>(p => p.Isvalid && (p.IsSynchronized == false || p.IsSynchronized == null)).OrderBy(p => p.Id).ToList();
                var result = products.Select(p => new { p.Name, OMSId = p.Id, OMSType = p.Type, p.NameEn, p.Cover, p.Country, p.Area, p.Grapes, p.Capacity, p.Year, p.Packing, p.Code, p.DeputyBarcode }).OrderBy(p => p.OMSId);
                var jsonStr = result.ToJson();

                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                    var requestUrl = _configuration.GetSection("WMSApi")["domain"].ToString() + "/wmsapi/ProductSync/OmsSyncProducts";
                    #region JWTBearer授权校验信息

                    _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                    if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                    http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));

                    #endregion
                    var response = http.PostAsync(requestUrl, content).Result;
                    string resultState = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                    {
                        await GetWMSOauthToken();
                        _logService.Error("同步商品数据到WMS失败，原因是API授权失败！请重试。");
                        return false;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var objResult = resultState.ToObj<WMSResult>();
                        if (objResult.state == "success")
                        {
                            //数据同步成功
                            foreach (var item in products)
                            {
                                item.IsSynchronized = true;
                                _omsAccessor.Update(item);
                            }
                            _omsAccessor.SaveChanges();
                            return true;
                        }
                        else if (objResult.state == "error")
                        {
                            //同步的数据有失败的
                            var successInfo = products.Where(p => !objResult.errOMSId.Contains(p.Id));
                            if (successInfo.Count() > 0)
                            {
                                foreach (var item in successInfo)
                                {
                                    item.IsSynchronized = true;
                                    _omsAccessor.Update(item);
                                }
                                _omsAccessor.SaveChanges();
                            }
                            return false;
                        }//no data
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        _logService.Error(string.Format("同步商品数据到WMS失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                _logService.Error("同步商品数据到WMS异常", ex);
                return false;
            }
        }

        #endregion

        public bool OmsPurchasingOrders(List<int> orderId)
        {
            var isSucc = false;
            var url = WMSApiUrl + "/wmsapi/PurchasingOrderSync/OmsSyncPurchasingOrder";
            if (orderId.Count() == 0)
            {
                return isSucc;
            }
            try
            {
                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    foreach (var item in orderId)
                    {
                        //订单信息
                        var order = _omsAccessor.Get<Purchasing>(p => p.Isvalid && p.Id == item).FirstOrDefault();
                        var wareHouse = _omsAccessor.Get<WareHouse>().Where(w => w.Isvalid && w.Id == order.WareHouseId).FirstOrDefault();
                        var purchasingOrder = new
                        {
                            order.Id,
                            order.PurchasingNumber,
                            order.OrgionSerialNumber,
                            order.PurchasingOrderNumber,
                            order.SupplierId,
                            SupplierName = _omsAccessor.Get<Supplier>().Where(s => s.Isvalid && s.Id == order.SupplierId).Select(s => s.SupplierName).FirstOrDefault(),
                            WareHouseCode = wareHouse.Code,
                            WareHouseName = wareHouse.Name.Trim(),
                            order.State,
                            order.Mark
                        };
                        //订单商品
                        var orderProducts = from pp in _omsAccessor.Get<PurchasingProducts>(pp => pp.Isvalid && pp.PurchasingId == item).ToList()
                                            select new
                                            {
                                                pp.Id,
                                                pp.PurchasingId,
                                                OMSProductId = pp.ProductId,
                                                pp.Quantity
                                            };
                        var purchasing = new { purchasingOrder, orderProducts };
                        var jsonStr = purchasing.ToJson();

                        #region JWTBearer授权校验信息
                        _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                        if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                        http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
                        #endregion

                        var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                        var response = http.PostAsync(url, content).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                        {
                            _logService.Error("OmsPurchasingOrders方法失败，原因是API授权失败！请重试。");
                            return false;
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {  //更改订单状态为已上传
                            order.State = PurchasingState.Uploaded;
                            _omsAccessor.Update<Purchasing>(order);
                            isSucc = true;
                        }
                        else
                        {
                            _logService.Error(string.Format("OmsPurchasingOrders方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                            return false;
                        }

                    }
                }
                _omsAccessor.SaveChanges();
                return isSucc;
            }
            catch (Exception e)
            {
                return isSucc;
            }

        }
        //OMS订单
        public async Task<bool> OmsOrders(List<int> orderId)
        {
            if (orderId.Count <= 0)
            {
                return false;
            }
            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var url = "";
                foreach (var item in orderId)
                {
                    //订单信息
                    /*如json数据过长则需要重新设置json数据内容*/
                    var order = _omsAccessor.Get<Order>(p => p.Isvalid && p.Id == item).FirstOrDefault();
                    //订单商品
                    var orderProducts = from pp in _omsAccessor.Get<OrderProduct>(pp => pp.Isvalid && pp.OrderId == item).ToList()
                                        select new
                                        {
                                            pp.Id,
                                            pp.SaleProductId,
                                            pp.OrginPrice,
                                            pp.Price,
                                            pp.SumPrice,
                                            pp.Quantity
                                        };
                    var orderInfo = new { order, orderProducts };
                    var jsonStr = orderInfo.ToJson();

                    #region JWTBearer授权校验信息
                    _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                    if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                    http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
                    #endregion

                    var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                    var response = await http.PostAsync(url, content);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                    {
                        await GetWMSOauthToken();
                        _logService.Error("OmsOrders方法调用失败，原因是API授权失败！请重试。");
                        return false;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        order.State = OrderState.Uploaded;
                        _omsAccessor.Update<Order>(order);
                    }
                    else
                    {
                        _logService.Error(string.Format("OmsOrders方法调用失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        return false;
                    }

                }
            }
            return true;
        }

        #region 供应商、快递方式、客户信息同步
        /// <summary>
        /// 同步供应商信息到WMS
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OmsSyncSuppliers()
        {
            var suppliers = _omsAccessor.Get<Supplier>().Where(x => !x.IsSynchronized && x.Isvalid).OrderBy(x => x.Id).ToList();
            var result = suppliers.Select(x => new { OMSSupplierId = x.Id, x.SupplierName });
            var jsonStr = result.ToJson();
            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                #region JWTBearer授权校验信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
                #endregion

                var postUrl = _configuration.GetSection("WMSApi")["domain"].ToString() + "/wmsapi/PurchasingOrderSync/OmsSyncSuppliers";
                var response = http.PostAsync(postUrl, content).Result;
                string resultState = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                {
                    await GetWMSOauthToken();
                    _logService.Error("OmsSyncSuppliers方法失败，原因是API授权失败！请重试。");
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // http返回成功
                    var objResult = resultState.ToObj<WMSResult>();
                    if (objResult.state == "success")
                    {
                        foreach (var item in suppliers)
                        {
                            item.IsSynchronized = true;
                            _omsAccessor.Update(item);
                        }
                        _omsAccessor.SaveChanges();
                        return true;
                    }
                    else if (objResult.state == "error")
                    {
                        var successInfo = suppliers.Where(p => !objResult.errOMSId.Contains(p.Id));
                        if (successInfo.Count() > 0)
                        {
                            foreach (var item in successInfo)
                            {
                                item.IsSynchronized = true;
                                _omsAccessor.Update(item);
                            }
                            _omsAccessor.SaveChanges();
                        }
                        return false;
                    }
                    else
                    {
                        _logService.Error(string.Format("同步供应商数据到WMS失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        //同步失败
                        return false;
                    }
                }
                else
                {
                    _logService.Error(string.Format("OmsSyncSuppliers方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    //同步失败
                    return false;
                }
            }
        }

        /// <summary>
        /// 同步快递方式到WMS
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OmsSyncDeliverys()
        {
            var postUrl = _configuration.GetSection("WMSApi")["domain"].ToString() + "/wmsapi/DictionarySync/OmsSyncDeliverys";
            var deliveries = _omsAccessor.Get<Delivery>(x => !x.IsSynchronized && x.Isvalid).OrderBy(x => x.Id).ToList();
            var result = deliveries.Select(x => new { OMSId = x.Id, x.Name, x.Code, Remark = x.ShopCode });
            var jsonStr = result.ToJson();

            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                #region JWTBearer授权校验信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
                #endregion
                var response = http.PostAsync(postUrl, content).Result;
                string resultState = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                {
                    await GetWMSOauthToken();
                    _logService.Error("OmsSyncSuppliers方法失败，原因是API授权失败！请重试。");
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // http返回成功
                    var objResult = resultState.ToObj<WMSResult>();
                    if (objResult.state == "success")
                    {
                        foreach (var item in deliveries)
                        {
                            item.IsSynchronized = true;
                            _omsAccessor.Update(item);
                        }
                        _omsAccessor.SaveChanges();
                        return true;
                    }
                    else if (objResult.state == "error")
                    {
                        var successInfo = deliveries.Where(p => !objResult.errOMSId.Contains(p.Id));
                        if (successInfo.Count() > 0)
                        {
                            foreach (var item in successInfo)
                            {
                                item.IsSynchronized = true;
                                _omsAccessor.Update(item);
                            }
                            _omsAccessor.SaveChanges();
                        }
                        return false;
                    }
                    else
                    {
                        _logService.Error(string.Format("同步快递方式数据到WMS失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        //同步失败
                        return false;
                    }
                }
                else
                {
                    _logService.Error(string.Format("OmsSyncDeliverys,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    //同步失败
                    return false;
                }
            }
        }

        /// <summary>
        /// 同步客户到WMS
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OmsSyncCustomers()
        {
            var postUrl = _configuration.GetSection("WMSApi")["domain"].ToString() + "/wmsapi/DictionarySync/OmsSyncCustomers";
            var customers = _omsAccessor.Get<Customers>(x => !x.IsSynchronized && x.Isvalid).OrderBy(x => x.Id).ToList();
            var result = customers.Select(x => new
            {
                OMSId = x.Id,
                x.CustomerTypeId,
                x.CustomerEmail,
                x.DisCount,
                x.Address,
                x.BankAccount,
                x.BankOfDeposit,
                x.Contact,
                x.Email,
                x.Mark,
                x.Mobile,
                x.Name,
                x.RegisterAddress,
                x.PriceTypeId,
                x.RegisterTel,
                x.TaxpayerId,
                x.Title
            });
            var jsonStr = result.ToJson();

            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                #region JWTBearer授权校验信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
                #endregion
                var response = http.PostAsync(postUrl, content).Result;
                string resultState = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                {
                    await GetWMSOauthToken();
                    _logService.Error("OmsSyncCustomers，原因是API授权失败！请重试。");
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // http返回成功
                    var objResult = resultState.ToObj<WMSResult>();
                    if (objResult.state == "success")
                    {
                        foreach (var item in customers)
                        {
                            item.IsSynchronized = true;
                            _omsAccessor.Update(item);
                        }
                        _omsAccessor.SaveChanges();
                        return true;
                    }
                    else if (objResult.state == "error")
                    {
                        var successInfo = customers.Where(p => !objResult.errOMSId.Contains(p.Id));
                        if (successInfo.Count() > 0)
                        {
                            foreach (var item in successInfo)
                            {
                                item.IsSynchronized = true;
                                _omsAccessor.Update(item);
                            }
                            _omsAccessor.SaveChanges();
                        }
                        return false;
                    }
                    else
                    {
                        _logService.Error(string.Format("同步客户数据到WMS失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        //同步失败
                        return false;
                    }
                }
                else
                {
                    _logService.Error(string.Format("OmsSyncCustomers,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    //同步失败
                    return false;
                }
            }
        }
        #endregion

        #region [OMS抓取商城下单信息同步到订单表]
        public async Task<bool> OmsSyncWineWorldOrder()
        {
            try
            {
                //防止多次抓取订单造成错误
                Monitor.Enter(_MyLock); //获取排它锁
                string method = "wineworld.order.search";
                string app_key = "oms";
                string v = "1.0";
                string sd_id = "501";
                string order_state = "WAIT_SELLER_STOCK_OUT";
                string page_no = "1";
                string page_size = "50";
                string data = null;
                OrderAssistParamsModel orderAssistParamsModel = _orderSyncService.OrderAssistOmsApi(method, app_key, v, sd_id, order_state, page_no, page_size, data);
                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {

                    var postParamJson = orderAssistParamsModel.ToJson();
                    //#region JWTBearer授权校验信息
                    //_workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                    //if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                    //http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
                    //#endregion
                    var content = new StringContent(postParamJson, Encoding.UTF8, "application/json");
                    var postUrl = _configuration.GetSection("OrderAssistOmsApi")["domain"].ToString();
                    var response = http.PostAsync(postUrl, content).Result;
                    var result = await response.Content.ReadAsStringAsync();

                    //if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                    //{
                    //    await GetWMSOauthToken();
                    //    Logger.Error("OMS抓取商城下单信息同步到订单表失败，原因是API授权失败！请重试。");
                    //    return false;
                    //}
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        //var account = _workContext.CurrentUser.Id;
                        var test = JsonMapper.ToObject<RespDataOrderInfo>(result);
                        if (test != null)
                        {
                            OrderSearchResponse orderSearchResponse = test.resp_data;
                            if (orderSearchResponse != null)
                            {
                                List<OrderInfo> orderInfoList = orderSearchResponse.order_info_list;

                                List<OrderNotification> orderNotifications = new List<OrderNotification>();
                                if (orderInfoList.Count > 0 && orderInfoList != null)
                                {
                                    _orderSyncService.OrderSync(orderInfoList, sd_id, out orderNotifications);
                                }
                                if (orderNotifications.Count > 0 && orderNotifications != null)
                                {
                                    OrderNotification(orderNotifications);
                                }
                            }
                        }
                        return true;
                    }
                    else
                    {
                        _logService.Error(string.Format("OMS抓取商城下单信息同步到订单表失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error(string.Format("OMS抓取商城下单信息同步到订表异常：{0}", ex.Message));
                return false;
            }

            finally
            {
                Monitor.Exit(_MyLock); //释放排它锁
            }
        }
        #endregion

        /// <summary>
        /// OMS下单成功通知商城
        /// </summary>
        /// <param name="orderNotifications"></param>
        public async void OrderNotification(List<OrderNotification> orderNotifications)
        {
            try
            {
                using (var https = new HttpClient())
                {
                    var content = new StringContent(orderNotifications.ToJson(), Encoding.UTF8, "application/json");
                    var postUrl = _configuration.GetSection("OrderAssistOmsApi")["domain"].ToString() + "/OrderNotification";
                    var response = https.PostAsync(postUrl, content).Result;
                    var result = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var data = JsonMapper.ToObject<OrderNotificationResponse>(result);
                        if (data.state)
                        {
                            var orderNotificationResults = data.notification_result_list;
                            var noSuccNotific = new List<OrderNotificationResult>();
                            if (orderNotificationResults != null && orderNotificationResults.Count > 0)
                            {
                                noSuccNotific = orderNotificationResults.Where(r => r.result != "1").ToList();
                            }
                            if (noSuccNotific != null && noSuccNotific.Count() > 0)
                            {
                                var order_snStr = string.Empty;
                                foreach (var item in noSuccNotific)
                                {
                                    order_snStr += string.Format("商铺Id:{0},系统方订单号{1};", item.sd_id, item.order_sn);
                                }

                                _logService.Error(string.Format("OMS下单成功通知信息异常：{0}", order_snStr));
                            }
                        }
                        else
                        {
                            _logService.Error(string.Format("OMS下单成功通知信息错误，错误信息：{0}", data.message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Logger.Info("OMS下单通知异常：" + ex.Message);
            }

        }
        /// <summary>
        /// 同步商品Rfid到酒柜
        /// </summary>
        public async Task SyncRfidToWineCabinet() {

            try
            {
                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    //#region JWTBearer授权校验信息
                    //_workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                    //if (string.IsNullOrEmpty(token)) { token = await GetWMSOauthToken(); }
                    //http.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", token));
                    //#endregion

                    var postUrl = _configuration.GetSection("WMSApi")["domain"].ToString() + "/wms/Order/SyncRfidToWineCabinet";
                    var content = new StringContent("", Encoding.UTF8, "application/json");
                    var response = http.PostAsync(postUrl,content).Result;
                    string resultState = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) // 未授权
                    {
                        await GetWMSOauthToken();
                        _logService.Error("OmsSyncSuppliers方法失败，原因是API授权失败！请重试。");
                    }
                }
            }
            catch (Exception ex)
            {

                Logger.Info("同步Rfid异常：" + ex.Message);
            }
        }
        /// <summary>
        /// 同步所有商品的库存到商城
        /// </summary>
        public void OMSToShopAllProductsStock()
        {
            try
            {
                #region 按商品编码批量更新
                //string[] arr = { };
                //var productStockDataList = new List<ProductStockData>();
                //for (int n = 0; n < arr.Length; n++)
                //{
                //    var product = _omsAccessor.Get<Product>().Where(x => x.Code == arr[n].ToString() && x.Isvalid).FirstOrDefault();
                //    if (product != null)
                //    {
                //        var sp = _omsAccessor.Get<SaleProduct>().Where(x => x.Isvalid && x.ProductId == product.Id).FirstOrDefault();
                //        if (sp!=null) {
                //            var productStockData = new ProductStockData
                //            {
                //                sd_id = "501",
                //                goods_sn = product.Code,
                //                stock_num = sp.AvailableStock.ToString(),
                //                stock_detail_list = _productService.GetSaleProductWareHouseStocksByProductCode(product.Code)
                //            };
                //            productStockDataList.Add(productStockData);
                //        }

                //    }
                //}

                //_productService.SyncMoreProductStockToAssist(productStockDataList);
                #endregion
                //查出所有的销售商品
                var productStockDataList = new List<ProductStockData>();
                var allSaleProducts = _omsAccessor.Get<SaleProduct>().Include(x => x.Product).Where(x => x.Isvalid && x.AvailableStock > 0 && x.Channel == 94).OrderByDescending(r=>r.Id).ToList();
                var i = 0;
                var ii = 1;
                foreach (var item in allSaleProducts)
                {
                    var xuniStock = _productService.GetXuniStocks(item.Id);
                    var productStockData = new ProductStockData
                    {
                        sd_id = "501",
                        goods_sn = item.Product.Code,
                        stock_num = (item.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock))).ToString(),
                    };
                    var proStockDetail = _productService.GetSaleProductWareHouseStocksByProductCode(item.Product.Code);
                    if (proStockDetail.Count() > 0)
                    {
                        productStockData.stock_detail_list = proStockDetail;
                        productStockDataList.Add(productStockData);
                    }
                    if (productStockDataList.Count % 100 == 0)
                    {
                        _logService.Info("当前更新库存页数：" + (ii = i % 100));
                        Thread.Sleep(100);
                        i += 100;
                        //更新库存到商城
                        _productService.SyncMoreProductStockToAssist(productStockDataList);
                        productStockDataList = new List<ProductStockData>();
                    }
                    if ((i + 100) > allSaleProducts.Count() && productStockDataList.Count == (allSaleProducts.Count() - i))
                    {
                        //更新库存到商城
                        _productService.SyncMoreProductStockToAssist(productStockDataList);
                        _logService.Info("当前更新库存页数：" + ii + 1);
                    }
                }
            }
            catch (Exception ex)
            {

                _logService.Error("同步库存到商城错误：" + ex.Message);
            }
        }
        /// <summary>
        /// 同步招行商城订单到OMS系统
        /// </summary>
        public void SyncCMBOrderToOMS()
        {
            _cmbService.InsertOrderToOMS(_cmbService.GetAllOrderList());
        }
    }
}


