using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.B2B
{
    /// <summary>
    /// b2b订单销货数据分析Model
    /// </summary>
    public class B2BSaleDataAnalysisModel
    {
        /// <summary>
        /// 单据类型
        /// </summary>
        public string OrderTypeStr { get; set; }
        public OrderType OrderType { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal UnitPrice { get; set; }
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        /// <summary>
        /// 客户类型Id
        /// </summary>
        public int CustomerTypeId { get; set; }
        /// <summary>
        /// 客户类型名称
        /// </summary>
        public string CustomerTypeName { get; set; }
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
        /// <summary>
        /// 是否验收
        /// </summary>
        public bool IsCheck { get; set; }
        public string IsCheckStr { get; set; }
        /// <summary>
        /// 是否记账
        /// </summary>
        public bool IsBookKeeping { get; set; }
        public string IsBookKeepingStr { get; set; }
        public string Mark { get; set; }
        /// <summary>
        /// 验收人
        /// </summary>
        public string CheckerName { get; set; }
        public OrderState State { get; set; }
        /// <summary>
        /// 原订单Id
        /// </summary>
        public int? OriginalOrderId { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public string SalesManName { get; set; }
        /// <summary>
        /// 收发时间
        /// </summary>
        public DateTime? DeliveryDate { get; set; }
    }
}
