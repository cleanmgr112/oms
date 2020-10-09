using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.DataStatistics
{
    /// <summary>
    /// 零售销售统计
    /// </summary>
    public class RetailSalesStatisticsModel
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public string OrderType { get; set; }
        public int OrderId { get; set; }
        /// <summary>
        /// 外部交易号
        /// </summary>
        public string PSerialNumber { get; set; }
        /// <summary>
        /// 零售销货单号
        /// </summary>
        public string SerialNumber { get; set; }

        public string ProductName { get; set; }
        public string ProductCode { get; set; }

        public int Quantity { get; set; }
        /// <summary>
        /// 市场价
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public decimal OriginalPrice { get; set; }
        /// <summary>
        /// 均摊总金额
        /// </summary>
        public decimal AvgSumPrice { get; set; }

        public DateTime CreatedTime { get; set; }
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
        public string CustomeName { get; set; }
        public string Address { get; set; }
        public int DeliveryId { get; set; }
        public string DeliveryName { get; set; }
    }
    /// <summary>
    ///根据商店获取零售销售统计
    /// </summary>
    public class RetailSalesStatisticsByShopModel
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public int Quantity { get; set; }
        public decimal AvgSumPrice { get; set; }
    }

    public class RetailSalesStatisticsByProductModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal AvgSumPrice { get; set; } 
    }
}
