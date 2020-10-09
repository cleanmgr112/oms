using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OMS.Data.Domain;
using OMS.Services;
using OMS.Services.Common;
using OMS.Services.Deliveries;
using OMS.Services.Order1;
using OMS.Services.Permissions;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class DataStatisticsController : BaseController
    {
        #region
        private readonly ICommonService _commonService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IDeliveriesService _deliveriesService;
        public DataStatisticsController(ICommonService commonService, IWareHouseService wareHouseService, IPermissionService permissionService
            , IOrderService orderService
            , IDeliveriesService deliveriesService)
        {
            this._commonService = commonService;
            this._wareHouseService = wareHouseService;
            this._permissionService = permissionService;
            this._orderService = orderService;
            this._deliveriesService = deliveriesService;
        }

        #endregion

        #region 零售销货分析
        public IActionResult RetailSalesAnalysis()
        {
            if (!_permissionService.Authorize("ViewRetailSalesAnalysis"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.WareHouses = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            return View();
        }
        [HttpPost]
        public IActionResult GetRetailSalesData(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            var data = _orderService.GetRetailSalesModelsByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId, orderType, search);
            return Success(data);
        }

        public IActionResult ExportRetailSalesAnalysis(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportDataByCommand(startTime, endTime, shopId, wareHouseId, orderType, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "单据类型","TypeName" },
                    { "单据编号","OrderSerialNumber" },
                    { "sku","ProductSku" },
                    { "货号","DeputyBarcode" },
                    { "商品名称","ProductName"},
                    { "商店", "ShopName" },
                    { "仓库", "WareHouseName" },
                    { "原价","OriginalPrice"},
                    { "销售价","SalePrice" },
                    { "数量","Quantity"},
                    { "总金额","SumPrice"}
                };

                DataTable table = data.ToDataTable(columnNames);


                StringBuilder sb = new StringBuilder();
                sb.Append("单据类型,单据编号,sku,货号,商品名称,商店,仓库,原价,销售价,数量,总金额");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["单据类型"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["单据编号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["sku"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["货号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["商品名称"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["商店"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["仓库"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["原价"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["销售价"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["总金额"] + "\"");
                    sb.Append("\r\n");
                }
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "零售销货分析.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }

        }

        #endregion

        #region 订单发货统计分析

        public IActionResult OrderDeliveryStatistics()
        {
            if (!_permissionService.Authorize("ViewOrderDeliveryStatistics"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.WareHouses = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            ViewBag.PayTypes = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PayType), "Id", "Value");
            ViewBag.Deliveries = new SelectList(_deliveriesService.GetAllDeliveries(), "Id", "Name");
            return View();
        }
        [HttpPost]
        public IActionResult GetOrderDeliveryStatisticsData(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? payType, int? deliveryId, int dateType, string search = "")
        {
            int orderCount, productCount;
            decimal avgSumPrice, deliverySumPrice;
            var data = _orderService.GetOrderDeliveryModelsByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId, payType, deliveryId, dateType, out orderCount, out productCount, out avgSumPrice, out deliverySumPrice, search);

            return Json(new { isSucc = true, data =new { result=data, orderCount = orderCount, productCount = productCount, avgSumPrice = avgSumPrice}, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }

        public IActionResult ExportOrderDeliveryStatistics(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? payType, int? deliveryId, int dateType, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportOrderDeliveryModels(startTime, endTime, shopId, wareHouseId, payType, deliveryId, dateType, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "订单编号","SerialNumber" },
                    { "发货日期","DeliverDate" },
                    { "下单日期","OrderDate" },
                    { "快递公司","DeliverName" },
                    { "发货单编号","InvoiceSerialNumber"},
                    { "支付方式", "PayTypeName" },
                    { "快递单号", "DeliverSerialNumber" },
                    { "仓库","WareHouseName"},
                    { "交易号","BusinessSerialNumber"},
                    { "用户名","AccountName" },
                    { "收货人","ReceiveName"},
                    { "地址","Address"},
                    { "手机","Phone"},
                    { "店铺","ShopName"},
                    { "运费","DeliveryCost"},
                    { "数量","Quantity"},
                    { "订单已付金额","PayedPrice"},
                    { "商品均摊总金额","AverageSumPrice"}
                };

                DataTable table = data.ToDataTable(columnNames);


                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("订单总数：{0} | 商品总数：{1} | 订单商品均摊总金额：{2} ", data.Count(), data.Sum(r => r.Quantity), data.Sum(r => r.AverageSumPrice)));
                sb.Append("\r\n");
                sb.Append("订单编号,发货日期,下单日期,快递公司,发货单编号,支付方式,快递单号,仓库,交易号,用户名,收货人,地址,手机,店铺,运费,数量,订单已付金额,商品均摊总金额");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["订单编号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["发货日期"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["下单日期"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["快递公司"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["发货单编号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["支付方式"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["快递单号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["仓库"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["交易号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["用户名"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["收货人"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["地址"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["手机"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["店铺"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["运费"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["订单已付金额"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["商品均摊总金额"] + "\"");
                    sb.Append("\r\n");
                }
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "订单发货统计.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }

        #endregion

        #region 商品发货统计分析
        public IActionResult GoodDeliveryStatistics()
        {
            if (!_permissionService.Authorize("ViewGoodDeliveryStatistics"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.WareHouses = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            ViewBag.Deliveries = new SelectList(_deliveriesService.GetAllDeliveries(), "Id", "Name");
            return View();
        }
        [HttpPost]
        public IActionResult GetGoodDeliveryStatisticsData(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? deliveryId, string search = "")
        {
            int orderCount, productCount;
            decimal avgSumPrice;
            var data = _orderService.GetOrderDeliverModelByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId, deliveryId,out orderCount,out productCount,out avgSumPrice, search);

            return Json(new { isSucc = true, data = new { result = data, orderCount = orderCount, productCount = productCount, avgSumPrice = avgSumPrice}, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }
        public  IActionResult ExportGoodDeliveryStatistics(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? deliveryId, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportGoodDeliveryModels(startTime, endTime, shopId, wareHouseId, deliveryId, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "商品名称","ProductName" },
                    { "商品货号","ProductCode" },
                    { "快递公司","DeliveryName" },
                    { "发货数量","Quantity" },
                    { "均摊金额","AvgSumPrice"}
                };

                DataTable table = data.ToDataTable(columnNames);
                StringBuilder sb = new StringBuilder();
                sb.Append("商品名称,商品货号,快递公司,发货数量,均摊金额");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["商品名称"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["商品货号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["快递公司"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["发货数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["均摊金额"] + "\"");
                    sb.Append("\r\n");
                }
                sb.Append(" , ,合计," + data.Sum(r => r.Quantity) + "," + data.Sum(r => r.AvgSumPrice));
                sb.Append("\r\n");

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "商品发货统计.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }
        #endregion

        #region 商店退货分析
        public IActionResult ShopRefundOrderStatistics()
        {
            if (!_permissionService.Authorize("ViewShopRefundOrderStatistics"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.WareHouses = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            return View();
        }
        [HttpPost]
        public IActionResult GetShopRefundOrderStatisticsData(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            int productCount;
            decimal price;
            var data = _orderService.GetShopRefundOrderModelByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId,out productCount,out price, search);
            return Json(new { IsSucc = true, data = new { result = data, productCount = productCount, price = price }, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }

        public IActionResult ExportShopRefundOrderStatistics(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportShopRefundOrderModels(startTime, endTime, shopId, wareHouseId, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "店铺","ShopName" },
                    { "数量","Quantity" },
                    { "日期","Date" },
                    { "货号","ProductCode" },
                    { "商品名称","ProductName"},
                    { "结算金额","SettlementaAmount" },
                    { "备注","Mark" },
                    { "退单号","RefundOrderSerialNumber" },
                    { "退货人","RefundUserName" },
                    { "地址","Address"},
                    { "手机","Phone" },
                    { "原订单号","OriginalOrderSerialNumber" },
                    { "退货原因","RefundReason" },
                    { "物流单号","DeliveryNo" },
                    { "原物流单号","RefundDeliveryNo"},
                    { "订单交易号","OrderExchangeNo" },
                    { "仓库","WareHouseName"},
                    { "昵称","NickName" },
                    { "金额","Price" },
                    { "验收","IsCheck"},
                    { "作废","IsInvalid" }
                };

                DataTable table = data.ToDataTable(columnNames);
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("合计--数量：{0} | 金额：{1} ", data.Sum(r => r.Quantity), data.Sum(r => r.Price)));
                sb.Append("\r\n");

                sb.Append("店铺,数量,日期,货号,商品名称,结算金额,备注,退单号,退货人,地址,手机,原订单号,退货原因,物流单号,原物流单号,订单交易号,仓库,昵称,金额,验收,作废");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["店铺"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["日期"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["货号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["商品名称"] + "\"" + ",");

                    sb.Append("\"" + "\t" + table.Rows[i]["结算金额"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["备注"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["退单号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["退货人"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["地址"] + "\"" + ",");

                    sb.Append("\"" + "\t" + table.Rows[i]["手机"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["原订单号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["退货原因"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["物流单号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["原物流单号"] + "\"" + ",");

                    sb.Append("\"" + "\t" + table.Rows[i]["订单交易号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["仓库"] + "\"" + ",");

                    sb.Append("\"" + "\t" + table.Rows[i]["昵称"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["金额"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["验收"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["作废"] + "\"");

                    sb.Append("\r\n");
                }

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "商店退货分析.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }

        /// <summary>
        /// 商品退货分析按店铺分析
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetShopRefundOrderStatisticsDataByShop(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            int productCount;
            decimal price;
            var data = _orderService.GetOrderDeliverModelByShopByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId, out productCount, out price, search);
            return Json(new { IsSucc = true, data = new { result = data, productCount = productCount, price = price }, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }

        public IActionResult ExportShopRefundOrderStatisticsByShop(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportShopRefundOrderModelsByShop(startTime, endTime, shopId, wareHouseId, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "店铺","ShopName" },
                    { "数量","Quantity" },
                    { "金额","Price" }
                };

                DataTable table = data.ToDataTable(columnNames);
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("合计--数量：{0} | 金额：{1} ", data.Sum(r => r.Quantity), data.Sum(r => r.Price)));
                sb.Append("\r\n");

                sb.Append("店铺,数量,金额");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["店铺"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["金额"] + "\"");

                    sb.Append("\r\n");
                }

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "商店退货分析.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }

        /// <summary>
        /// 商品退货分析按商品分析
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetShopRefundOrderStatisticsDataByProduct(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            int productCount;
            decimal price;
            var data = _orderService.GetOrderDeliverModelByProductByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId, out productCount, out price, search);
            return Json(new { IsSucc = true, data = new { result = data, productCount = productCount, price = price }, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }

        public IActionResult ExportShopRefundOrderStatisticsByProduct(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportShopRefundOrderModelsByProduct(startTime, endTime, shopId, wareHouseId, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "商品名称","ProductName" },
                    { "数量","Quantity" },
                    { "金额","Price" }
                };

                DataTable table = data.ToDataTable(columnNames);
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("合计--数量：{0} | 金额：{1} ", data.Sum(r => r.Quantity), data.Sum(r => r.Price)));
                sb.Append("\r\n");

                sb.Append("商品名称,数量,金额");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["商品名称"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["金额"] + "\"");

                    sb.Append("\r\n");
                }

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "商店退货分析.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }
        #endregion

        #region 零售销售统计
        public IActionResult RetailSalesStatistics()
        {
            if (!_permissionService.Authorize("ViewRetailSalesStatistics"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.Platforms = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            ViewBag.WareHouses = new SelectList(_wareHouseService.GetAllWareHouseList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult GetRetailSalesStatisticsData(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            int productCount;
            decimal avgSumPrice;
            var data = _orderService.GetRetailSalesStatisticsModelByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId, orderType, out productCount, out avgSumPrice, search);

            return Json(new { isSucc = true, data = new { result = data, productCount = productCount, avgSumPrice = avgSumPrice }, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }

        public IActionResult ExportRetailSalesStatistics( DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportRetailSalesStatisticsModel(startTime, endTime, shopId, wareHouseId,orderType, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "订单类型","OrderType" },
                    { "店铺","ShopName" },
                    { "外部交易号","PSerialNumber" },
                    { "零售销货单号","SerialNumber" },
                    { "商品","ProductName"},
                    { "编码", "ProductCode" },
                    { "数量", "Quantity" },
                    { "原价","OriginalPrice"},
                    { "单价","Price"},
                    { "均摊总金额","AvgSumPrice" },
                    { "时间","CreatedTime"},
                    { "收货人","CustomeName"},
                    { "收货人地址","Address"},
                    { "快递公司","DeliveryName"}
                };

                DataTable table = data.ToDataTable(columnNames);


                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("合计--商品总数量：{0} | 商品均摊总金额：{1} ",data.Sum(r => r.Quantity),data.Sum(r=>r.AvgSumPrice)));
                sb.Append("\r\n");
                sb.Append("订单类型,店铺,外部交易号,零售销货单号,商品,编码,数量,原价,单价,均摊总金额,时间,收货人,收货人地址,快递公司");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["订单类型"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["店铺"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["外部交易号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["零售销货单号"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["商品"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["编码"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["原价"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["单价"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["均摊总金额"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["时间"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["收货人"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["收货人地址"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["快递公司"] + "\"");
                    sb.Append("\r\n");
                }
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "零售销售统计.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }

        /// <summary>
        /// 根据店铺分类获取数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetRetailSalesStatisticsDataByShop(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            int productCount;
            decimal avgSumPrice;
            var data = _orderService.GetRetailSalesStatisticsByShopModelByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId, orderType, out productCount, out avgSumPrice, search);
            return Json(new { isSucc = true, data = new { result = data, productCount = productCount, avgSumPrice = avgSumPrice }, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }

        public IActionResult ExportRetailSalesStatisticsByShop(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportRetailSalesStatisticsByShopModel(startTime, endTime, shopId, wareHouseId, orderType, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "店铺","ShopName" },
                    { "数量","Quantity" },
                    { "分摊总金额","AvgSumPrice" }
                };

                DataTable table = data.ToDataTable(columnNames);


                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("合计--商品总数量：{0} | 商品均摊总金额：{1} ", data.Sum(r => r.Quantity), data.Sum(r => r.AvgSumPrice)));
                sb.Append("\r\n");
                sb.Append("店铺,数量,分摊总金额");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["店铺"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["分摊总金额"] + "\"");
                    sb.Append("\r\n");
                }
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "零售销售统计.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }

        /// <summary>
        /// 根据商品分类获取数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetRetailSalesStatisticsDataByProduct(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            int productCount;
            decimal avgSumPrice;
            var data = _orderService.GetRetailSalesStatisticsByProductModelByPage(pageIndex, pageSize, startTime, endTime, shopId, wareHouseId, orderType, out productCount, out avgSumPrice, search);
            return Json(new { isSucc = true, data = new { result = data, productCount = productCount, avgSumPrice = avgSumPrice }, totalPages = data.TotalPages, totalCount = data.TotalCount });
        }

        public IActionResult ExportRetailSalesStatisticsByProduct( DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "")
        {
            try
            {
                var data = _orderService.GetAllExportRetailSalesStatisticsByProductModel(startTime, endTime, shopId, wareHouseId, orderType, search);
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

                //设置Excel表头
                Dictionary<string, string> columnNames = new Dictionary<string, string>
                {
                    { "商品名称","ProductName" },
                    { "编码","ProductCode" },
                    { "单价","Price" },
                    { "数量","Quantity" },
                    { "分摊总金额","AvgSumPrice" }
                };

                DataTable table = data.ToDataTable(columnNames);


                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("合计--商品总数量：{0} | 商品均摊总金额：{1} ", data.Sum(r => r.Quantity), data.Sum(r => r.AvgSumPrice)));
                sb.Append("\r\n");
                sb.Append("商品名称,编码,单价,数量,分摊总金额");
                sb.Append("\r\n");

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    sb.Append("\"" + "\t" + table.Rows[i]["商品名称"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["编码"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["单价"] + "\""+",");
                    sb.Append("\"" + "\t" + table.Rows[i]["数量"] + "\"" + ",");
                    sb.Append("\"" + "\t" + table.Rows[i]["分摊总金额"] + "\"" + ",");
                    sb.Append("\r\n");
                }
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(sb.ToString());
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv;charset=UTF-8", fileName + "零售销售统计.csv");
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = "错误信息：" + ex.Message });
            }
        }

        #endregion
    }
}