using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    public class ResultModel
    {
        public bool isSucc { get; set; }
        public string msg { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        //public int stateCode { get; set; }

    }

    public class CancelOrderResultModel
    {
        public bool isSucc { get; set; }
        public string msg { get; set; }
        public bool isOut { get; set; }
    }

    public class GetStockResultModel
    {
        public bool isSucc { get; set; }
        public string msg { get; set; }
        public int count { get; set; }
    }
    /// <summary>
    /// WMS到OMS同步订单状态模型
    /// </summary>
    public class WMSSyncOrderModel
    {
        public string OMSSerialNumber { get; set; }
        public string UploadBy { get; set; }
        public List<ProductInfoModel> ProductInfoModel { get; set; }
    }

    /// <summary>
    /// 传递发票信息
    /// </summary>
    public class WMSSyncInvoiceModel
    {
        public string OMSSerialNumber { get; set; }
        public string UploadBy { get; set; }

        public string InvoiceNo { get; set; }
    }

    public class WMSOrderModelDelivery 
    {
        public string OMSSerialNumber { get; set; }
        public string UploadBy { get; set; }

        public string DeliveryCode { get; set; }
    }
    public class ProductInfoModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    /// <summary>
    /// OMS订单到WMS匹配发货仓库返回Model
    /// </summary>
    public class MatchWareHouseModel
    {
        public bool isSucc { get; set; }
        public string msg { get; set; }
        public int wareHouseId { get; set; }
    }
    /// <summary>
    /// 德邦快递地址检查结果Model
    /// </summary>
    public class DBDeliveryCheckModel
    {
        public bool isSucc { get; set; }
        public bool isDelivery { get; set; }
        public string msg { get; set; }
    }

    /// <summary>
    /// WMS返回仓库库存信息
    /// </summary>
    public class WmsWareHouseStockModel {
        public bool isSucc { get; set; }
        public string msg { get; set; }

        public List<WareHouseStock> data { get; set; }

    }
    /// <summary>
    /// 同步商品仓库库存
    /// </summary>
    public class WareHouseStock
    {
        /// <summary>
        /// 商品omsId
        /// </summary>
        public int? ProductOmsId { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string WareHouseCode { get; set; }
        /// <summary>
        /// 商品库存
        /// </summary>
        public int Stock { get; set; }
    }
    /// <summary>
    /// wms传递商品仓库库存信息
    /// </summary>
    public class WmsToOmsWareHouseStockModel
    {
        public int ProductOmsId { get; set; }
        public List<WareHouseStock> WareHouseStocks { get; set; }
    }
}
