using Microsoft.Extensions.Configuration;
using OMS.Core;
using OMS.Core.Json;
using OMS.Core.Tools;
using OMS.Data.Domain;
using OMS.Data.Domain.Purchasings;
using OMS.Data.Domain.Suppliers;
using OMS.Data.Interface;
using OMS.Model;
using OMS.Model.Purchasings;
using OMS.Services.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace OMS.Services
{
    public class PurchasingService : ServiceBase, IPurchasingService
    {
        #region ctor
        private ILogService _logService;
        public PurchasingService(IDbAccessor omsAccessor, IWorkContext workContext,IConfiguration configuration,ILogService logService)
            : base(omsAccessor, workContext,configuration)
        {
            _logService = logService;
        }
        #endregion

        //供应商管理
        public IEnumerable<Supplier> GetAllSuppliers()
        {
            var data = _omsAccessor.Get<Supplier>();
            return data;
        }

        public Supplier AddSupplier(Supplier supplier)
        {
            supplier.CreatedBy = _workContext.CurrentUser.Id;
            supplier.CreatedTime = DateTime.Now;
            _omsAccessor.Insert(supplier);
            _omsAccessor.SaveChanges();
            return supplier;
        }
        public Supplier ComfirmSupplierIsExist(string Name)
        {
            var result = _omsAccessor.Get<Supplier>().Where(s => s.Isvalid && s.SupplierName == Name).FirstOrDefault();
            return result;
        }
        public Supplier GetSupplierById(int id)
        {
            var data = _omsAccessor.Get<Supplier>().Where(s => s.Isvalid && s.Id == id).FirstOrDefault();
            return data;
        }
        public Supplier UpdataSupplier(Supplier supplier)
        {
            supplier.ModifiedBy = _workContext.CurrentUser.Id;
            supplier.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<Supplier>(supplier);
            _omsAccessor.SaveChanges();
            return supplier;
        }

        public bool DeleteSuppllier(int id)
        {
            try
            {
                var data = _omsAccessor.Get<Supplier>().Where(s => s.Isvalid && s.Id == id).FirstOrDefault();
                _omsAccessor.Delete<Supplier>(data);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void SetSupplierIsvalid(int id)
        {
            var data = _omsAccessor.Get<Supplier>().Where(s => s.Id == id).FirstOrDefault();
            if (data.Isvalid)
            {
                data.Isvalid = false;
            }
            else
            {
                data.Isvalid = true;
            }
            data.ModifiedBy = _workContext.CurrentUser.Id;
            data.ModifiedTime = DateTime.Now;
            _omsAccessor.Update(data);
            _omsAccessor.SaveChanges();
        }




        //采购管理
        public PageList<Object> GetPurchasingOrdersByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, string search = "", string warehouse = "", string supplierName = "",string startWith="JR")
        {
            var state = GetPurchasingOrderStates();
            DateTime? dateTime = new DateTime();
            if(startTime.Equals(dateTime))
            {
                startTime = null;
            }
            if (endTime.Equals(dateTime))
            {
                endTime = null;
            }
            var result = from p in _omsAccessor.Get<Purchasing>().Where(p => p.Isvalid && p.PurchasingNumber.StartsWith(startWith) && (string.IsNullOrEmpty(search) || p.OrgionSerialNumber.Contains(search) || p.PurchasingNumber.Contains(search)
                         || p.PurchasingOrderNumber.Contains(search)) && ((!startTime.HasValue || p.CreatedTime >= startTime.Value) && (!endTime.HasValue || p.CreatedTime <= endTime.Value)))
                         join w in _omsAccessor.Get<WareHouse>().Where(w => (string.IsNullOrEmpty(warehouse) || w.Name == warehouse)) on p.WareHouseId equals w.Id
                         join sp in _omsAccessor.Get<Supplier>().Where(sp => (string.IsNullOrEmpty(supplierName) || sp.SupplierName == supplierName)) on p.SupplierId equals sp.Id
                         orderby p.CreatedTime descending
                         select new
                         {
                             p.Id,
                             p.PurchasingNumber,
                             p.PurchasingOrderNumber,
                             p.OrgionSerialNumber,
                             p.SupplierId,
                             p.WareHouseId,
                             p.State,
                             p.Mark,
                             p.Isvalid,
                             p.ModifiedBy,
                             p.ModifiedTime,
                             p.CreatedBy,
                             p.CreatedTime,
                             //外表字段
                             WareHouseName = w.Name,
                             sp.SupplierName,
                             SumQuantity = _omsAccessor.Get<PurchasingProducts>().Where(s => s.PurchasingId == p.Id).Sum(d => d.Quantity),
                             SumReceviedNum = _omsAccessor.Get<PurchasingProducts>().Where(s => s.PurchasingId == p.Id).Sum(d => d.FactReceivedNum) ?? 0,
                             OrderSumPrice = _omsAccessor.Get<PurchasingProducts>().Where(s => s.PurchasingId == p.Id).Sum(d => d.Quantity * d.Price),
                             StateStr = state[p.State]
                         };
            return new PageList<Object>(result, pageIndex, pageSize);
        }


        /// <summary>
        /// 获取导出采购订单 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExportPurchaseOrder> GetExportPurchasingOrder(SearchPurchaseOrderModel searchPurchaseOrderModel) {
            var state = GetPurchasingOrderStates();
            DateTime? dateTime = new DateTime();
            var startTime = searchPurchaseOrderModel.StartTime;
            var endTime = searchPurchaseOrderModel.EndTime;
            var isDetail = searchPurchaseOrderModel.IsDetail;
            var startWith = searchPurchaseOrderModel.StartWith;
            var warehouse = searchPurchaseOrderModel.WareHouse;
            var search = searchPurchaseOrderModel.Search;
            var supplierName = searchPurchaseOrderModel.SupplierName;
            if (startTime.Equals(dateTime))
            {
                startTime = null;
            }
            if (endTime.Equals(dateTime))
            {
                endTime = null;
            }
            var data = new List<ExportPurchaseOrder>();
            //订单详情
            if (isDetail)
            {
                data = (from p in _omsAccessor.Get<Purchasing>().Where(p => p.Isvalid && p.PurchasingNumber.StartsWith(startWith) && (string.IsNullOrEmpty(search) || p.OrgionSerialNumber.Contains(search) || p.PurchasingNumber.Contains(search)
|| p.PurchasingOrderNumber.Contains(search)) && ((!startTime.HasValue || p.CreatedTime >= startTime.Value) && (!endTime.HasValue || p.CreatedTime <= endTime.Value)))
                        join w in _omsAccessor.Get<WareHouse>().Where(w => (string.IsNullOrEmpty(warehouse) || w.Name == warehouse)) on p.WareHouseId equals w.Id
                        join sp in _omsAccessor.Get<Supplier>().Where(sp => (string.IsNullOrEmpty(supplierName) || sp.SupplierName == supplierName)) on p.SupplierId equals sp.Id
                        join opp in _omsAccessor.Get<PurchasingProducts>().Where(x=>x.Isvalid) on p.Id equals opp.PurchasingId into op
                        from oppu in op.DefaultIfEmpty()
                        join pr in _omsAccessor.Get<Product>().Where(x=>x.Isvalid) on oppu.ProductId equals pr.Id
                        orderby p.CreatedTime descending
                        select new ExportPurchaseOrder
                        {
                            PurchasingNumber = p.PurchasingNumber,
                            PurchasingOrderNumber = p.PurchasingOrderNumber,
                            OrgionSerialNumber = p.OrgionSerialNumber,
                            Mark = p.Mark,
                            CreatedTime = string.Format("{0:G}", p.CreatedTime),
                            CheckTime = string.Format("{0:G}", p.CreatedTime),
                            SupplierName = sp.SupplierName,
                            StateStr = state[p.State],
                            PurchasingProductCode = pr.Code,
                            PurchasingProductName = pr.Name,
                            UnitPrice = oppu.Price,
                            Quantity = oppu.Quantity,
                            SumPrice = oppu.Price*oppu.Quantity

                        }).OrderByDescending(r => r.CreatedTime).Distinct().ToList();
            }
            else {//非订单详情
                 data = (from p in _omsAccessor.Get<Purchasing>().Where(p => p.Isvalid && p.PurchasingNumber.StartsWith(startWith) && (string.IsNullOrEmpty(search) || p.OrgionSerialNumber.Contains(search) || p.PurchasingNumber.Contains(search)
           || p.PurchasingOrderNumber.Contains(search)) && ((!startTime.HasValue || p.CreatedTime >= startTime.Value) && (!endTime.HasValue || p.CreatedTime <= endTime.Value)))
                             join w in _omsAccessor.Get<WareHouse>().Where(w => (string.IsNullOrEmpty(warehouse) || w.Name == warehouse)) on p.WareHouseId equals w.Id
                             join sp in _omsAccessor.Get<Supplier>().Where(sp => (string.IsNullOrEmpty(supplierName) || sp.SupplierName == supplierName)) on p.SupplierId equals sp.Id
                             orderby p.CreatedTime descending
                             select new ExportPurchaseOrder
                             {
                                PurchasingNumber= p.PurchasingNumber,
                                PurchasingOrderNumber= p.PurchasingOrderNumber,
                                OrgionSerialNumber=  p.OrgionSerialNumber,
                                Mark =  p.Mark,
                                CreatedTime = string.Format("{0:G}", p.CreatedTime),
                                CheckTime = string.Format("{0:G}", p.CreatedTime),
                                SupplierName = sp.SupplierName,
                                StateStr = state[p.State]
                             }).OrderByDescending(r=>r.CreatedTime).Distinct().ToList();
            }

            return data;
        }
        public Purchasing AddPurchasing(Purchasing purchasing)
        {
            purchasing.CreatedBy = _workContext.CurrentUser.Id;
            purchasing.CreatedTime = DateTime.Now;
            _omsAccessor.Insert<Purchasing>(purchasing);
            _omsAccessor.SaveChanges();
            return purchasing;
        }
        public Purchasing GetPurchasingById(int id)
        {
            var result = _omsAccessor.Get<Purchasing>().Where(p => p.Isvalid && p.Id == id).FirstOrDefault();
            return result;
        }
        public Purchasing UpdatePurchasingOrder(Purchasing purchasing)
        {
            if(_workContext.CurrentUser!=null)
            {
                purchasing.ModifiedBy = _workContext.CurrentUser.Id;
            }
            purchasing.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<Purchasing>(purchasing);
            _omsAccessor.SaveChanges();
            return purchasing;
        }

        /// <summary>
        /// 修改采购订单商品信息
        /// </summary>
        /// <param name="purchasingProducts"></param>
        /// <returns></returns>
        public PurchasingProducts UpdatePurchasingOrderProduct(PurchasingProducts purchasingProducts) {
            if (_workContext.CurrentUser != null)
            {
                purchasingProducts.ModifiedBy = _workContext.CurrentUser.Id;
            }
            purchasingProducts.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<PurchasingProducts>(purchasingProducts);
            _omsAccessor.SaveChanges();
            return purchasingProducts;
        }
        public PurchasingProducts AddPurchasingOrderProduct(PurchasingProducts purchasingProducts)
        {
            purchasingProducts.CreatedBy = _workContext.CurrentUser.Id;
            purchasingProducts.CreatedTime = DateTime.Now;
            _omsAccessor.Insert<PurchasingProducts>(purchasingProducts);
            _omsAccessor.SaveChanges();
            return purchasingProducts;
        }
       public bool ConfirmPurchasingOrderProductIsExist(PurchasingProducts purchasingProducts)
        {
            var result = _omsAccessor.Get<PurchasingProducts>().Where(p => p.Isvalid && p.PurchasingId == purchasingProducts.PurchasingId && p.ProductId == purchasingProducts.ProductId && p.Price==purchasingProducts.Price).FirstOrDefault();
            return result == null ? false : true;
        }
        public IEnumerable<PurchasingProductModel> GetAllPurchasingOrderProducts(int purchasingId)
        {
            var data =from s in _omsAccessor.Get<PurchasingProducts>().Where(p => p.Isvalid && p.PurchasingId == purchasingId) 
                      join p in _omsAccessor.Get<Product>() on s.ProductId equals p.Id 
                      select new PurchasingProductModel
                      {
                          Id=s.Id,
                          ProductId=s.ProductId,
                          PurchasingId=s.PurchasingId,
                          Quantity=s.Quantity,
                          Name=p.Name,
                          Code=p.Code,
                          Price=s.Price,
                          SumPrice=s.Price*s.Quantity
                      };
            return data;
        }
        public bool DeletePurchasingOrderProduct(int purchasingOrderId, int productId)
        {
            try
            {
                var result = _omsAccessor.Get<PurchasingProducts>().Where(p => p.PurchasingId == purchasingOrderId && p.ProductId == productId).FirstOrDefault();
                _omsAccessor.Delete<PurchasingProducts>(result);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public Dictionary<PurchasingState, string> GetPurchasingOrderStates()
        {
            Dictionary<PurchasingState, string> data = new Dictionary<PurchasingState, string>();
            foreach (PurchasingState i in Enum.GetValues(typeof(PurchasingState)))
            {
                data.Add(i,i.Description());
            }
            return data;
        }

        public PurchasingProducts GetPurchasingProductById(int purchasingId, int productId)
        {
            var data = _omsAccessor.Get<PurchasingProducts>(s => s.Isvalid && s.PurchasingId == purchasingId && s.ProductId == productId).FirstOrDefault();
            return data;
        }
        public IEnumerable<PurchasingProducts> GetPurchasingProductByPurchasingId(int purchasingId)
        {
            var data = _omsAccessor.Get<PurchasingProducts>().Where(s => s.Isvalid && s.PurchasingId == purchasingId);
            return data;
        }


        public IEnumerable<Object> GetPurchasingOrderLogInfo(int purchasingId)
        {
            var data = _omsAccessor.Get<OrderTableLog>().Where(o => o.Isvalid && o.OrderId == purchasingId &&( o.OrderTable=="Purchasing" || o.OrderTable=="PurchasingProducts")).OrderByDescending(o => o.CreatedTime);
            return data;
        }

        public bool SetPurchasingOrderInvalid(int purchasingId)
        {
            var data = _omsAccessor.Get<Purchasing>().Where(p => p.Isvalid && p.Id == purchasingId).FirstOrDefault();
            data.State = PurchasingState.Invalid;
            _omsAccessor.Update<Purchasing>(data);
            _omsAccessor.SaveChanges();
            return true;
        }
        /// <summary>
        /// 取消上传采购订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string CancelUploadPurchasingOrder(int id)
        {
            Purchasing purcOrder = _omsAccessor.Get<Purchasing>().Where(r => r.Isvalid && r.Id == id).FirstOrDefault();

            if (purcOrder == null)
                return (new { isSucc = false, msg = "发生错误，该采购订单信息有误！" }).ToString();

            var post = new
            {
                OMSSerialNumber = purcOrder.PurchasingNumber,
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
                var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wmsapi/PurchasingOrderSync/OMSCancelUploadPurchasingOrder";
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
                    _logService.Error("CancelUploadPurchasingOrder采购订单取消上传方法失败，原因是API授权失败！请重试。");
                    return (new { isSucc = false, isOut = false, msg = "CancelUploadPurchasingOrder采购订单取消上传方法失败，原因是API授权失败！请重试。" }).ToString();
                }
                else
                {
                    _logService.Error(string.Format("CancelUploadPurchasingOrder采购订单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, isOut = false, msg = string.Format("CancelUploadPurchasingOrder采购订单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToString();
                }
            }
        }
        /// <summary>
        /// 上传采购退单到WMS
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string UploadPurchasingRefundOrder(int id)
        {
            Purchasing purcOrder = _omsAccessor.Get<Purchasing>().Where(r => r.Isvalid && r.Id == id).FirstOrDefault();

            if (purcOrder == null)
                return (new { isSucc = false, msg = "发生错误，该采购订单信息有误！" }).ToString();

            var post = new PurchasingRefundOrderModelToWMS
            {
                Id = purcOrder.Id,
                PurchasingNumber = purcOrder.PurchasingNumber,
                PurchasingOrderNumber = purcOrder.PurchasingOrderNumber,
                OrgionSerialNumber = purcOrder.OrgionSerialNumber,
                Mark = purcOrder.Mark,
                SupplierName = _omsAccessor.GetById<Supplier>(purcOrder.SupplierId) == null ? "" : _omsAccessor.GetById<Supplier>(purcOrder.SupplierId).SupplierName,
                WareHouseCode = _omsAccessor.GetById<WareHouse>(purcOrder.WareHouseId) == null ? "" : _omsAccessor.GetById<WareHouse>(purcOrder.WareHouseId).Code,
                PurchasingProducts = _omsAccessor.Get<PurchasingProducts>().Where(r => r.Isvalid && r.PurchasingId == purcOrder.Id).Select(p => new PurchasingProductModelToWMS { ProductId=p.ProductId,Quantity=p.Quantity}).ToList(),
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
                var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wmsapi/PurchasingOrderSync/UploadPurchasingRefundOrder";
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
                    _logService.Error("采购退单上传方法失败，原因是API授权失败！请重试。");
                    return (new { isSucc = false, isOut = false, msg = "采购退单取消上传方法失败，原因是API授权失败！请重试。" }).ToString();
                }
                else
                {
                    _logService.Error(string.Format("采购退单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, isOut = false, msg = string.Format("采购退单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToString();
                }
            }
        }

        /// <summary>
        /// 根据单号获取订单
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public Purchasing GetPurchasingBySerialNumber(string serialNumber)
        {
            return _omsAccessor.Get<Purchasing>().Where(r => r.Isvalid && r.PurchasingNumber.Equals(serialNumber)).FirstOrDefault();
        }

        /// <summary>
        /// 取消上传采购退单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string CancelUploadPurchasingRefundOrder(int orderId)
        {
            Purchasing purcOrder = _omsAccessor.Get<Purchasing>().Where(r => r.Isvalid && r.Id == orderId).FirstOrDefault();

            if (purcOrder == null)
                return (new { isSucc = false, msg = "发生错误，该采购退单信息有误！" }).ToString();

            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var content = new StringContent(purcOrder.PurchasingNumber, Encoding.UTF8, "application/json");
                var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wmsapi/PurchasingOrderSync/CancelUploadPurchasingRefundOrder";
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
                    _logService.Error("取消采购退单上传方法失败，原因是API授权失败！请重试。");
                    return (new { isSucc = false, isOut = false, msg = "取消采购退单取消上传方法失败，原因是API授权失败！请重试。" }).ToString();
                }
                else
                {
                    _logService.Error(string.Format("取消采购退单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, isOut = false, msg = string.Format("取消采购退单取消上传方法失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToString();
                }
            }
        }
        public bool SyncWMSFactReceviedProNumsToOMS(int purchasingOrderId, IEnumerable<Model.JsonModel.ProductInfoModel> productInfos)
        {
            try
            {
                foreach (var item in productInfos)
                {
                    var product = _omsAccessor.Get<PurchasingProducts>().Where(s => s.PurchasingId == purchasingOrderId && s.ProductId == item.ProductId).FirstOrDefault();
                    product.FactReceivedNum = item.Quantity;
                    _omsAccessor.Update<PurchasingProducts>(product);
                }
                _omsAccessor.SaveChanges();
                return true;
            }catch(Exception e)
            {
                _logService.Error("<SyncWMSFactReceviedProNumsToOMS>错误：" + e.Message + "<br />位置：" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 根据名字获取供应商
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Supplier GetSupplierByName(string name)
        {
            return _omsAccessor.Get<Supplier>().Where(r => r.Isvalid && r.SupplierName.Contains(name)).FirstOrDefault();
        }

        /// <summary>
        /// 判断订单的退货状态（采购退单）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public RefundState JudgePurchasingOrderRefundState(int? orderId)
        {

            var refundState = RefundState.No;
            var purchasingProducts = (from pup in _omsAccessor.Get<PurchasingProducts>().Where(x => x.Isvalid)
                                      group pup by pup.ProductId into pupg
                                      orderby pupg.Key
                                      select new PurchasingProductModel
                                      {
                                          ProductId = pupg.Key,
                                          Quantity = pupg.Sum(x => x.Quantity)
                                      }).ToList();
            return refundState; 
                                      
        }

    }
}
