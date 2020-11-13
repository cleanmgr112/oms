using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OMS.Data.Domain;
using OMS.Data.Domain.Purchasings;
using OMS.Data.Interface;
using OMS.Model;
using OMS.Services;
using OMS.Services.Account;
using OMS.Services.Common;
using OMS.Services.Log;
using OMS.Services.Permissions;
using OMS.Services.Products;
using OMS.Services.ScheduleTasks;
using OMS.Core.Tools;
using OMS.Core.Json;
using OMS.Model.JsonModel;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using NPOI.XSSF.UserModel;
using System.Text;
using NPOI.HSSF.UserModel;
using System.Data;
using OMS.Model.Purchasings;

namespace OMS.Web.Controllers
{
    public class PurchasingController : BaseController
    {
        #region
        private readonly IPermissionService _permissionService;
        private readonly IPurchasingService _purchasingService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ICommonService _commonService;
        private readonly IProductService _productService;
        private readonly ILogService _logService;
        private readonly IUserService _userService;
        private readonly IScheduleTaskFuncService _scheduleTaskFuncService;
        private readonly IDbAccessor _omsAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        public PurchasingController(IPermissionService permissionService, IPurchasingService purchasingService, IWareHouseService wareHouseService,
            ICommonService commonService, IProductService productService, ILogService logService, IUserService userService, IScheduleTaskFuncService scheduleTaskFuncService, IDbAccessor omsAccessor
            , IHostingEnvironment hostingEnvironment)
        {
            _permissionService = permissionService;
            _purchasingService = purchasingService;
            _wareHouseService = wareHouseService;
            _commonService = commonService;
            _productService = productService;
            _logService = logService;
            _userService = userService;
            _scheduleTaskFuncService = scheduleTaskFuncService;
            _omsAccessor = omsAccessor;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion
        public IActionResult Index()
        {
            //权限
            if (!_permissionService.Authorize("ViewPurchasingOrders"))
            {
                return View("_AccessDeniedView");
            }

            ViewBag.Suppliers = new SelectList(_purchasingService.GetAllSuppliers(), "SupplierName", "SupplierName");
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetWareHouses(), "Name", "Name");

            return View();
        }
        public IActionResult GetPurchasingOrders(string search, int pageSize, int pageIndex, string wareHouse, string supplierName, DateTime startTime, DateTime endTime)
        {
            var data = _purchasingService.GetPurchasingOrdersByPage(pageIndex, pageSize, startTime, endTime, search, wareHouse, supplierName, startWith: "JR");
            return Success(data);
        }
        public IActionResult AddPurchasingOrder()
        {
            //权限
            if (!_permissionService.Authorize("ViewAddPurchasingOrder"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Suppliers = new SelectList(_purchasingService.GetAllSuppliers(), "Id", "SupplierName");
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetWareHouses(), "Id", "Name");
            return View();
        }
        [HttpPost]
        public IActionResult AddPurchasingOrder(Purchasing purchasing)
        {
            //权限
            if (!_permissionService.Authorize("AddPurchasingOrder"))
            {
                return Error("没有此权限！");
            }



            purchasing.State = PurchasingState.InitialStatus;
            purchasing.PurchasingNumber = _commonService.GetOrderSerialNumber("JR");
            var data = _purchasingService.AddPurchasing(purchasing);

            //日志
            #region 新增采购订单
            var mark = "新增采购订单：[" + data.PurchasingNumber + "]";
            _logService.InsertOrderTableLog("Purchasing", data.Id, "新增采购订单", Convert.ToInt32(data.State), mark);
            #endregion


            return Success(data);
        }
        public IActionResult PurchasingOrderDetail(int id)
        {
            //权限
            if (!_permissionService.Authorize("ViewPurchasingOrderDetail"))
            {
                return View("_AccessDeniedView");
            }
            var data = _purchasingService.GetPurchasingById(id);
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetWareHouses(), "Id", "Name", data.WareHouseId);
            ViewBag.Supplier = new SelectList(_purchasingService.GetAllSuppliers(), "Id", "SupplierName", data.SupplierId);
            ViewBag.PurchasingProducts = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(_purchasingService.GetAllPurchasingOrderProducts(id)));
            ViewBag.State = EnumExtensions.GetEnumList((Enum)PurchasingState.InitialStatus);

            ViewBag.OrderLogInfo = _purchasingService.GetPurchasingOrderLogInfo(id);
            Dictionary<int, string> dUser = new Dictionary<int, string>();
            foreach (var item in _userService.GetAllUsers())
            {
                dUser.Add(item.Id, item.Name);
            }
            ViewBag.User = dUser;
            ViewBag.ProductsCount = _purchasingService.GetAllPurchasingOrderProducts(data.Id).Sum(r => r.Quantity);
            ViewBag.OrderSumPrice = _purchasingService.GetAllPurchasingOrderProducts(data.Id).Sum(r => r.SumPrice);
            return View(data);
        }
        [HttpPost]
        public IActionResult PurchasingOrderDetail(Purchasing purchasing)
        {
            //权限
            if (!_permissionService.Authorize("SavePurchasingOrderInfo"))
            {
                return Error("没有此权限！");
            }

            var result = _purchasingService.UpdatePurchasingOrder(purchasing);
            //日志
            #region 保存信息
            var mark = "保存信息";
            _logService.InsertOrderTableLog("Purchasing", result.Id, "保存信息", Convert.ToInt32(result.State), mark);
            #endregion
            return Success();
        }
        public IActionResult GetProducts(int pageSize, int pageIndex, string searchStr = "")
        {
            var result = _productService.GetProductList(pageSize, pageIndex, 0, searchStr);

            return Success(result);
        }
        public IActionResult GetProductDetail(int id)
        {
            var data = _productService.GetProductById(id);
            return Success(data);
        }
        public IActionResult AddPurchasingOrderProducts(PurchasingProducts purchasingProducts)
        {
            //权限
            if (!_permissionService.Authorize("AddPurchasingOrderProducts"))
            {
                return Error("没有此权限！");
            }

            if (!_purchasingService.ConfirmPurchasingOrderProductIsExist(purchasingProducts))
            {
                _purchasingService.AddPurchasingOrderProduct(purchasingProducts);


                //日志
                #region 新增商品
                var data = _purchasingService.GetPurchasingById(purchasingProducts.PurchasingId);
                var productData = _productService.GetProductById(purchasingProducts.ProductId);
                var mark = "新增商品：【" + productData.Name + "】 数量：【" + purchasingProducts.Quantity + "】 单价：【"+purchasingProducts.Price+"】";
                _logService.InsertOrderTableLog("PurchasingProducts", purchasingProducts.PurchasingId, "添加商品", Convert.ToInt32(data.State), mark);
                #endregion

                return Success();
            }
            else
            {
                return Error("添加失败，查看是否已添加！");
            }

        }
        /// <summary>
        /// 修改订单商品信息
        /// </summary>
        /// <param name="purchasingProducts"></param>
        /// <returns></returns>
        public IActionResult UpdatePurchasingOrderProducts(PurchasingProducts purchasingProducts) {
            //权限
            if (!_permissionService.Authorize("AddPurchasingOrderProducts"))
            {
                return Error("没有此权限！");
            }
            var oldPurchasingProduct = _omsAccessor.GetById<PurchasingProducts>(purchasingProducts.Id);
            if (oldPurchasingProduct == null)
            {
                return Error("修改商品失败，为找到商品信息！");
            }

            oldPurchasingProduct.Quantity = purchasingProducts.Quantity;
            oldPurchasingProduct.Price = purchasingProducts.Price;
            try
            {
                _purchasingService.UpdatePurchasingOrderProduct(oldPurchasingProduct);
                //日志
                #region 修改商品
                var data = _purchasingService.GetPurchasingById(purchasingProducts.PurchasingId);
                var productData = _productService.GetProductById(purchasingProducts.ProductId);
                var mark = "修改商品：【" + productData.Name + "】 数量：【" + purchasingProducts.Quantity + "】 单价：【" + purchasingProducts.Price + "】";
                _logService.InsertOrderTableLog("PurchasingProducts", purchasingProducts.PurchasingId, "修改商品", Convert.ToInt32(data.State), mark);
                #endregion

                return Success();
            }
            catch (Exception ex)
            {

                _logService.Error("修改采购订单商品发生错误："+ex.Message);
                return Error("修改商品发生错误，请联系管理员！");
            }


        }
        public IActionResult DeletePurchasingOrderProduct(int purchasingOrderId, int productId)
        {
            //权限
            if (!_permissionService.Authorize("DeletePurchasingOrderProducts"))
            {
                return Error("没有此权限！");
            }

            //日志
            #region 删除商品
            var data = _purchasingService.GetPurchasingProductById(purchasingOrderId, productId);
            var purchasingOrder = _purchasingService.GetPurchasingById(purchasingOrderId);
            var productData = _productService.GetProductById(productId);
            var mark = "删除商品：[" + productData.Name + "] 数量：[" + data.Quantity + "]";
            #endregion

            if (_purchasingService.DeletePurchasingOrderProduct(purchasingOrderId, productId))
            {
                _logService.InsertOrderTableLog("PurchasingProducts", purchasingOrderId, "删除商品", Convert.ToInt32(purchasingOrder.State), mark);
                return Success();
            }
            else
            {
                return Error();
            }
        }
        public IActionResult GetOrderLogInfo(int purchasingId)
        {
            return Success();
        }
        public IActionResult SetOrderVerified(int purchasingId)
        {
            //权限
            if (!_permissionService.Authorize("SetPurchasingOrderVerified"))
            {
                return Error("没有此权限！");
            }
            if ((_purchasingService.GetPurchasingProductByPurchasingId(purchasingId)).Count() == 0)
            {
                return Error("无商品，不能审核！");
            }
            else
            {
                var data = _purchasingService.GetPurchasingById(purchasingId);
                data.State = PurchasingState.Verify;
                data = _purchasingService.UpdatePurchasingOrder(data);
                //日志
                #region 审核订单
                var mark = "审核订单";
                _logService.InsertOrderTableLog("Purchasing", purchasingId, "审核订单", Convert.ToInt32(data.State), mark);
                #endregion
                return Success();
            }
        }
        //反审订单
        public IActionResult SetOrderUnVerified(int purchasingId)
        {
            if (!_permissionService.Authorize("SetPurchasingUnVerified"))
            {
                return Error("没有此权限！");
            }

            var data = _purchasingService.GetPurchasingById(purchasingId);
            if (data == null)
                return Error("数据错误，该订单不存在");
            data.State = PurchasingState.InitialStatus;
            data = _purchasingService.UpdatePurchasingOrder(data);
            //日志
            #region 反审核订单
            var mark = "反审核订单";
            _logService.InsertOrderTableLog("Purchasing", purchasingId, "反审核订单", Convert.ToInt32(data.State), mark);
            #endregion
            return Success();
        }
        public IActionResult SetOrderConfirmed(int purchasingId)
        {
            //权限
            if (!_permissionService.Authorize("SetPurchasingOrderConfirmed"))
            {
                return Error("没有此权限！");
            }
            var data = _purchasingService.GetPurchasingById(purchasingId);
            data.State = PurchasingState.Confirmed;
            data = _purchasingService.UpdatePurchasingOrder(data);
            #region 确认订单
            var mark = "确认订单";
            _logService.InsertOrderTableLog("Purchasing", purchasingId, "确认订单", Convert.ToInt32(data.State), mark);
            #endregion
            return Success();
        }

        public IActionResult SetUnOrderConfirmed(int purchasingId)
        {
            //权限
            if (!_permissionService.Authorize("SetPurchasingOrderUnConfirmed"))
            {
                return Error("没有此权限！");
            }
            var data = _purchasingService.GetPurchasingById(purchasingId);
            data.State = PurchasingState.InitialStatus;
            data = _purchasingService.UpdatePurchasingOrder(data);
            #region 确认订单
            var mark = "反确认订单";
            _logService.InsertOrderTableLog("Purchasing", purchasingId, "反确认订单", Convert.ToInt32(data.State), mark);
            #endregion
            return Success();
        }
        public IActionResult SetOrderInvalid(int purchasingId)
        {
            //权限
            if (!_permissionService.Authorize("SetPurchasingOrderInvalid"))
            {
                return Error("没有此权限！");
            }
            _purchasingService.SetPurchasingOrderInvalid(purchasingId);
            //日志
            #region 设为无效
            var data = _purchasingService.GetPurchasingById(purchasingId);
            var mark = "设为无效";
            _logService.InsertOrderTableLog("Purchasing", purchasingId, "设为无效", Convert.ToInt32(data.State), mark);
            #endregion

            return Success();
        }
        [HttpPost]
        public IActionResult UploadOrder(List<int> orderId)
        {
            var result = _scheduleTaskFuncService.OmsPurchasingOrders(orderId);
            if (result)
            {
                foreach (var item in orderId)
                {
                    #region 上传订单
                    var data = _purchasingService.GetPurchasingById(item);
                    var mark = "上传订单成功！";
                    _logService.InsertOrderTableLog("Purchasing", item, "上传订单", Convert.ToInt32(data.State), mark);
                    #endregion
                }
                return Success();
            }
            else
            {
                return Error("上传失败！请检查日志");
            }
        }

        [HttpPost]
        public IActionResult CancelUploadOrder(int orderId)
        {
            string res = _purchasingService.CancelUploadPurchasingOrder(orderId);
            var result = JsonHelper.ToObj<CancelOrderResultModel>(res);
            if (result.isSucc)
            {
                var purchasingOrder = _purchasingService.GetPurchasingById(orderId);
                purchasingOrder.State = PurchasingState.InitialStatus;
                _purchasingService.UpdatePurchasingOrder(purchasingOrder);

                #region 取消上传订单
                var mark = "取消上传订单成功！";
                _logService.InsertOrderTableLog("Purchasing", orderId, "取消上传订单", Convert.ToInt32(purchasingOrder.State), mark);
                #endregion
            }

            return Json(new { Data = result });
        }
        /// <summary>
        /// 验收订单
        /// </summary>
        /// <param name="purchasingId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SetPurchasingCheckAccept(int purchasingId)
        {
            if (!_permissionService.Authorize("CheckPurchasingOrder"))
                return Error("没有此权限！");

            var purchasing = _purchasingService.GetPurchasingById(purchasingId);
            if (purchasing == null)
                return Json(new { isSucc = false, msg = "数据错误，该采购单不存在!" });
            purchasing.State = PurchasingState.CheckAccept;
            _purchasingService.UpdatePurchasingOrder(purchasing);

            _logService.InsertOrderTableLog("Purchasing", purchasingId, "验收订单", Convert.ToInt32(purchasing.State), "把订单状态改为验收");
            return Success();
        }

        /// <summary>
        /// 完成订单
        /// </summary>
        /// <param name="purchasingId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SetPurchasingFinished(int purchasingId)
        {
            if (!_permissionService.Authorize("FinishedPurchasingOrder"))
                return Error("没有此权限！");

            var purchasing = _purchasingService.GetPurchasingById(purchasingId);
            if (purchasing == null)
                return Json(new { isSucc = false, msg = "数据错误，该采购单不存在!" });
            purchasing.State = PurchasingState.Finished;
            _purchasingService.UpdatePurchasingOrder(purchasing);

            _logService.InsertOrderTableLog("Purchasing", purchasingId, "完成订单", Convert.ToInt32(purchasing.State), "把订单状态改为完成");
            return Success();
        }

        /// <summary>
        /// 生成采购退单
        /// </summary>
        /// <param name="purchaingId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RefundPurchasingOrder(int purchaingId)
        {
            if (!_permissionService.Authorize("RefundPurchasingOrder"))
            {
                return Error("没有此权限！");
            }
            var purcOrder = _purchasingService.GetPurchasingById(purchaingId);
            //使用事务回滚生成采购退单
            using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    //采购订单改为无效
                    purcOrder.State = PurchasingState.Invalid;
                    _purchasingService.UpdatePurchasingOrder(purcOrder);

                    //日志
                    #region 修改采购订单为退单状态
                    var mark = "生成退单";
                    _logService.InsertOrderTableLog("Purchasing", purchaingId, "生成退单", Convert.ToInt32(purcOrder.State), mark);
                    #endregion

                    //生成新的采购退单
                    var newPurcOrder = new Purchasing()
                    {
                        PurchasingNumber = _commonService.GetOrderSerialNumber("RF"),
                        PurchasingOrderNumber = purcOrder.PurchasingOrderNumber,
                        OrgionSerialNumber = purcOrder.PurchasingNumber,
                        SupplierId = purcOrder.SupplierId,
                        WareHouseId = purcOrder.WareHouseId,
                        State = PurchasingState.InitialStatus,
                        Mark = purcOrder.Mark,
                        OriginalOrderId = purcOrder.Id,
                    };
                    _purchasingService.AddPurchasing(newPurcOrder);

                    //日志
                    #region 创建采购订单
                    var _mark = "创建退单";
                    _logService.InsertOrderTableLog("Purchasing", newPurcOrder.Id, "创建退单", Convert.ToInt32(newPurcOrder.State), _mark);
                    #endregion

                    //添加采购退单商品
                    var purcProducts = _purchasingService.GetPurchasingProductByPurchasingId(purcOrder.Id);
                    foreach (var item in purcProducts)
                    {
                        var purcProduct = new PurchasingProducts()
                        {
                            PurchasingId = newPurcOrder.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            OrginId = item.Id                   
                        };
                        _purchasingService.AddPurchasingOrderProduct(purcProduct);
                    }

                    tran.Commit();
                    return Json(new { refundPurchasingNumber = newPurcOrder.PurchasingNumber,id=newPurcOrder.Id,isSucc = true, msg = "生成退单成功！" });
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    _logService.Error(string.Format("采购订单号为：{0}的生成退单失败，失败原因：{1}", purcOrder.PurchasingNumber, ex.Message));
                    return Json(new
                    {
                        isSucc = false,
                        msg = "生成退单失败"
                    });
                }
            }

        }

        public IActionResult RefundPurchasingOrderList()
        {
            //权限
            if (!_permissionService.Authorize("ViewRefundPurchasingOrders"))
            {
                return View("_AccessDeniedView");
            }

            ViewBag.Suppliers = new SelectList(_purchasingService.GetAllSuppliers(), "SupplierName", "SupplierName");
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetWareHouses(), "Name", "Name");

            return View();
        }

        public IActionResult GetRefundPurchasingOrders(string search, int pageSize, int pageIndex, string wareHouse, string supplierName, DateTime startTime, DateTime endTime)
        {
            var result = _purchasingService.GetPurchasingOrdersByPage(pageIndex, pageSize, startTime, endTime, search, wareHouse, supplierName, startWith: "RF");
            return Success(result);
        }

        public IActionResult RefundPurchasingOrderDetail(int id)
        {
            //权限
            if (!_permissionService.Authorize("ViewRefundPurchasingOrders"))
            {
                return View("_AccessDeniedView");
            }
            var data = _purchasingService.GetPurchasingById(id);
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetWareHouses(), "Id", "Name", data.WareHouseId);
            ViewBag.Supplier = new SelectList(_purchasingService.GetAllSuppliers(), "Id", "SupplierName", data.SupplierId);
            ViewBag.PurchasingProducts = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(_purchasingService.GetAllPurchasingOrderProducts(id)));
            ViewBag.State = EnumExtensions.GetEnumList((Enum)PurchasingState.InitialStatus);

            ViewBag.OrderLogInfo = _purchasingService.GetPurchasingOrderLogInfo(id);
            Dictionary<int, string> dUser = new Dictionary<int, string>();
            foreach (var item in _userService.GetAllUsers())
            {
                dUser.Add(item.Id, item.Name);
            }
            ViewBag.User = dUser;
            ViewBag.ProductsCount = _purchasingService.GetAllPurchasingOrderProducts(data.Id).Sum(r => r.Quantity);
            ViewBag.OrderSumPrice = _purchasingService.GetAllPurchasingOrderProducts(data.Id).Sum(r => r.SumPrice);
            return View(data);
        }

        public IActionResult AddPurchasingRefundOrder()
        {
            //权限
            if (!_permissionService.Authorize("RefundPurchasingOrder"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Suppliers = new SelectList(_purchasingService.GetAllSuppliers(), "Id", "SupplierName");
            ViewBag.WareHouse = new SelectList(_wareHouseService.GetWareHouses(), "Id", "Name");
            return View();
        }
        [HttpPost]
        public IActionResult AddPurchasingRefundOrder(PurchasingRefundOrderModel refundOrder)
        {
            //权限
            if (!_permissionService.Authorize("RefundPurchasingOrder"))
            {
                return Error("没有此权限！");
            }
            try
            {
                var purchasing = new Purchasing();
                purchasing.PurchasingNumber = _commonService.GetOrderSerialNumber("RF");
                purchasing.OrgionSerialNumber = refundOrder.PurchasingOriginalNumber;
                purchasing.PurchasingOrderNumber = refundOrder.PurchasingPlanSerialNumber;
                purchasing.SupplierId = refundOrder.SupplierId;
                purchasing.WareHouseId = refundOrder.WareHouseId;
                purchasing.Mark = refundOrder.Mark;
                purchasing.State = PurchasingState.InitialStatus;
                _purchasingService.AddPurchasing(purchasing);

                //日志
                #region 新增采购退单
                var mark = "新增采购退单：[" + purchasing.PurchasingNumber + "]";
                _logService.InsertOrderTableLog("Purchasing", purchasing.Id, "新增采购退单", Convert.ToInt32(purchasing.State), mark);
                #endregion

                return Json(new { isSucc = true, msg = "成功", id = purchasing.Id });
            }
            catch (Exception ex)
            {
                return Json(new { isSucc = false, msg = "失败，原因：" + ex.Message });
            }

        }

        [HttpPost]
        public IActionResult UploadRefundOrder(int orderId)
        {
            string res = _purchasingService.UploadPurchasingRefundOrder(orderId);
            var result = JsonHelper.ToObj<ResultModel>(res);
            if (result.isSucc)
            {
                var purchasingOrder = _purchasingService.GetPurchasingById(orderId);
                purchasingOrder.State = PurchasingState.Uploaded;
                _purchasingService.UpdatePurchasingOrder(purchasingOrder);

                #region 上传采购退单
                var mark = "上传采购退单成功！";
                _logService.InsertOrderTableLog("Purchasing", orderId, "上传采购退单", Convert.ToInt32(purchasingOrder.State), mark);
                #endregion
            }
            return Json(new { Data = result });
        }

        public IActionResult CancelUploadRefundPurchasingOrder(int orderId)
        {
            string res = _purchasingService.CancelUploadPurchasingRefundOrder(orderId);
            var result = JsonHelper.ToObj<ResultModel>(res);
            if (result.isSucc)
            {
                var purchasingOrder = _purchasingService.GetPurchasingById(orderId);
                purchasingOrder.State = PurchasingState.Confirmed;
                _purchasingService.UpdatePurchasingOrder(purchasingOrder);

                #region 上传采购退单
                var mark = "取消上传采购退单成功！";
                _logService.InsertOrderTableLog("Purchasing", orderId, "取消上传采购退单", Convert.ToInt32(purchasingOrder.State), mark);
                #endregion
            }
            return Json(new { Data = result });
        }
        /// <summary>
        /// 验收退单
        /// </summary>
        /// <returns></returns>
        public IActionResult SetRefundOrderCheckAccept(int purchasingId)
        {
            if (!_permissionService.Authorize("CheckPurchasingOrder"))
                return Error("没有此权限！");

            var purchasing = _purchasingService.GetPurchasingById(purchasingId);
            if (purchasing == null)
                return Json(new { isSucc = false, msg = "数据错误，该采购退单不存在!" });
            purchasing.State = PurchasingState.CheckAccept;
            _purchasingService.UpdatePurchasingOrder(purchasing);

            _logService.InsertOrderTableLog("Purchasing", purchasingId, "验收订单", Convert.ToInt32(purchasing.State), "把订单状态改为验收");
            return Success();
        }
        /// <summary>
        /// 完成退单
        /// </summary>
        /// <param name="purchasingId"></param>
        /// <returns></returns>
        public IActionResult SetRefundOrderFinished(int purchasingId)
        {
            if (!_permissionService.Authorize("CheckPurchasingOrder"))
                return Error("没有此权限！");

            var purchasing = _purchasingService.GetPurchasingById(purchasingId);
            if (purchasing == null)
                return Json(new { isSucc = false, msg = "数据错误，该采购退单不存在!" });
            purchasing.State = PurchasingState.Finished;
            _purchasingService.UpdatePurchasingOrder(purchasing);

            _logService.InsertOrderTableLog("Purchasing", purchasingId, "完成订单", Convert.ToInt32(purchasing.State), "把订单状态改为完成");
            return Success();
        }

        /// <summary>
        /// 从Excel导入采购订单
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ImportPuchasingOrderFromExcel(IFormFile formFile)
        {
            IWorkbook workbook = null;
            if (formFile == null)
            {
                return Error("请添加要导入文件");
            }
            else
            {
                string fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    formFile.CopyTo(fs);
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

                Purchasing purchasing = new Purchasing();
                purchasing.PurchasingNumber = _commonService.GetOrderSerialNumber("JR");
                purchasing.State = PurchasingState.InitialStatus;
                purchasing.PurchasingOrderNumber = sheet.GetRow(0).GetCell(1)?.ToString().Trim();
                //暂时不用读取时间
                //purchasing.CreatedTime = Convert.ToDateTime(sheet.GetRow(1).GetCell(1).ToString().Trim());
                var supplier = _purchasingService.GetSupplierByName(sheet.GetRow(2).GetCell(1).ToString().Trim());
                if (supplier == null)
                    return Error(string.Format("不存在名为：{0}的供应商", sheet.GetRow(2).GetCell(1).ToString().Trim()));
                else
                    purchasing.SupplierId = supplier.Id;
                var wareHouse = _wareHouseService.GetWareHouseByName(sheet.GetRow(3).GetCell(1).ToString().Trim());
                if (wareHouse == null)
                    return Error(string.Format("不存在名为：{0}的仓库", sheet.GetRow(3).GetCell(1).ToString().Trim()));
                else
                    purchasing.WareHouseId = wareHouse.Id;

                purchasing.Mark = sheet.GetRow(4).GetCell(1).ToString().Trim();
                _purchasingService.AddPurchasing(purchasing);


                //日志
                #region 新增采购订单
                var mark = "新增采购订单：[" + purchasing.PurchasingNumber + "]";
                _logService.InsertOrderTableLog("Purchasing", purchasing.Id, "新增采购订单", Convert.ToInt32(purchasing.State), mark);
                #endregion

                StringBuilder errStr = new StringBuilder();
                int errorCount = 0;
                int succCount = 0;
                if (sheet.LastRowNum >= 5)
                {
                    for (int i = 5; i <= sheet.LastRowNum; i++)
                    {
                        row = sheet.GetRow(i);
                        var productCode = row.GetCell(0).ToString().Trim();
                        try
                        {
                            var product = _productService.GetProductByCode(productCode);
                            if (product == null)
                            {
                                errorCount++;
                                errStr.Append(string.Format("不存在编码为{0}的商品;", productCode));
                            }
                            var purchasProduct = new PurchasingProducts();
                            purchasProduct.PurchasingId = purchasing.Id;
                            purchasProduct.ProductId = product.Id;
                            purchasProduct.Quantity = Convert.ToInt32(row.GetCell(1).ToString().Trim());
                            purchasProduct.Price = Convert.ToDecimal(row.GetCell(3).ToString().Trim()) * Convert.ToInt32(row.GetCell(2).ToString().Trim());

                            _purchasingService.AddPurchasingOrderProduct(purchasProduct);
                            succCount++;
                        }

                        catch (Exception ex)
                        {
                            errorCount++;
                            errStr.Append(string.Format("采购订单添加编码为{0}的商品时出现异常;", productCode));
                            _logService.Error("采购订单"+ purchasing.PurchasingNumber+ "商品"+productCode+"导入错误："+ex.Message);
                        }
                    }
                    System.IO.File.Delete(fileName);
                    fileStream.Close();
                }

                if (errorCount > 0)
                {
                    string err = "Excel导入采购订单商品时出现错误，错误信息：" + errStr.ToString();
                    _logService.Error(err);
                    return Error(err);
                }
                else
                {
                    return Success("导入成功");
                }
            }
        }

        /// <summary>
        /// 导出采购单信息 
        /// </summary>
        /// <returns></returns>
        public IActionResult ExportPurchaseOrder(string searchPurchaseOrderModelStr) {

            try
            {
                var searchPurchaseOrderModel = new SearchPurchaseOrderModel();
                searchPurchaseOrderModel = JsonConvert.DeserializeObject<SearchPurchaseOrderModel>(searchPurchaseOrderModelStr);

                //获取数据
                var data = _purchasingService.GetExportPurchasingOrder(searchPurchaseOrderModel);
                string fileName ="采购订单-"+DateTime.Now.ToString("yyyyMMddHHmmss");
                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "采购单号","PurchasingNumber" },
                    { "采购计划单号","PurchasingOrderNumber" },
                    { "原单号","OrgionSerialNumber" },
                    { "下单日期","CreatedTime"},
                    { "审核日期", "CheckTime" },
                    { "供应商", "SupplierName" },
                    { "订单状态","StateStr"},
                    { "备注","Mark" },
                };
                //是否导出采购单详情
                if (searchPurchaseOrderModel.IsDetail)
                {
                    columnNames.Add("采购商品名称", "PurchasingProductName");
                    columnNames.Add("采购商品编码", "PurchasingProductCode");
                    columnNames.Add("数量", "Quantity");;
                    columnNames.Add("单价", "UnitPrice");
                    columnNames.Add("合计金额", "SumPrice");
                }

                DataTable table = data.ToDataTable(columnNames);

                //导出.xls格式的文件
                if (searchPurchaseOrderModel.ExportType == ".xls")
                {
                    table.TableName = "Sheet1";
                    Stream stream = CommonTools.WriteExcel(table);

                    return File(stream, "application/vnd.ms-excel;charset=UTF-8", fileName + searchPurchaseOrderModel.ExportType);
                }
                //导出.csv格式的文件
                else if (searchPurchaseOrderModel.ExportType == ".csv")
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("采购单号,采购计划单号,原单号,下单日期,审核日期,供应商,订单状态,备注");
                    if (searchPurchaseOrderModel.IsDetail)
                    {
                        sb.Append(",采购商品名称,采购商品编码,数量,单价,合计金额");
                    }
                    sb.Append("\r\n");

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        sb.Append("\"" + "\t" + table.Rows[i]["采购单号"] + "\"" + ",");
                        sb.Append("\"" + "\t" + table.Rows[i]["采购计划单号"] + "\"" + ",");
                        sb.Append("\"" + "\t" + table.Rows[i]["原单号"] + "\"" + ",");
                        sb.Append("\"" + "\t" + table.Rows[i]["下单日期"] + "\"" + ",");
                        sb.Append("\"" + "\t" + table.Rows[i]["审核日期"] + "\"" + ",");
                        sb.Append("\"" + "\t" + table.Rows[i]["供应商"] + "\"" + ",");
                        sb.Append("\"" + "\t" + table.Rows[i]["订单状态"] + "\"" + ",");
                        sb.Append("\"" + "\t" + table.Rows[i]["备注"] + "\"");
                        if (searchPurchaseOrderModel.IsDetail)
                        {
                            sb.Append("," + "\"" + "\t" + table.Rows[i]["采购商品名称"] + "\"" + ",");
                            sb.Append("\"" + "\t" + table.Rows[i]["采购商品编码"] + "\"" + ",");
                            sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                            sb.Append("\"" + "\t" + table.Rows[i]["单价"] + "\"" + ",");
                            sb.Append("\"" + "\t" + table.Rows[i]["合计金额"] + "\"");
                        }
                        sb.Append("\r\n");
                    }
                    MemoryStream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(sb.ToString());
                    writer.Flush();
                    stream.Position = 0;
                    return File(stream, "text/csv;charset=UTF-8", fileName + searchPurchaseOrderModel.ExportType);
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
        /// 删除采购订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DelOrder(int orderId) {

            var purchasingOrder = _purchasingService.GetPurchasingById(orderId);
            if (purchasingOrder != null)
            {
                if (purchasingOrder.State != PurchasingState.InitialStatus && purchasingOrder.State != PurchasingState.Invalid) {
                    return Error("订单不是无效和初始状态无法删除！");
                }

                try
                {
                    purchasingOrder.Isvalid = false;
                    _purchasingService.UpdatePurchasingOrder(purchasingOrder);
                    return Success("删除成功！");
                }
                catch (Exception ex)
                {
                    _logService.Error("删除采购订单" + purchasingOrder.PurchasingNumber + "失败：" + ex);
                    return Error("删除采购订单" + purchasingOrder.PurchasingNumber + "失败!");
                }


            }
            else {
                return Error("没找到订单信息！");
            }

        }
    }
}