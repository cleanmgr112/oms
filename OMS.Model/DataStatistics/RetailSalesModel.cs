using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.DataStatistics
{
    /// <summary>
    /// 零售销货分析Model
    /// </summary>
    public class RetailSalesModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderSerialNumber { get; set; }
        public bool IsRefundOrder { get; set; }
        public string TypeName { get; set; }
        public DateTime DateTime { get; set; }
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
        public string ProductSku { get; set; }
        public string DeputyBarcode { get; set; }
        public string ProductName { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int Quantity { get; set; }
        public decimal SumPrice { get; set; }
    }
}
