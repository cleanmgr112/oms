using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.DataStatistics
{
    /// <summary>
    /// 商品发货统计分析Model
    /// </summary>
    public class GoodDeliveryModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        /// <summary>
        /// 快递公司
        /// </summary>
        public int DeliveryId { get; set; }
        public string DeliveryName { get; set; }
        /// <summary>
        /// 发货数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 均摊金额
        /// </summary>
        public decimal AvgSumPrice { get; set; }
        /// <summary>
        /// 发货日期
        /// </summary>
        public DateTime? DeliveryDate { get; set; }
        /// <summary>
        /// 店铺
        /// </summary>
        public int ShopId { get; set; }
        public int WareHouseId { get; set; }
    }
}
