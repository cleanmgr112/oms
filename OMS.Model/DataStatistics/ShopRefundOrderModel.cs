using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.DataStatistics
{
    /// <summary>
    /// 商店退货分析
    /// </summary>
    public class ShopRefundOrderModel
    {
        /// <summary>
        /// 店铺
        /// </summary>
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 货号
        /// </summary>
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal SettlementaAmount { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Mark { get; set; }
        /// <summary>
        /// 退单号
        /// </summary>
        public string RefundOrderSerialNumber { get; set; }
        /// <summary>
        /// 退货人
        /// </summary>
        public string RefundUserName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        /// <summary>
        /// 原单号
        /// </summary>
        public string OriginalOrderSerialNumber { get; set; }
        /// <summary>
        /// 退货原因
        /// </summary>
        public string RefundReason { get; set; }
        /// <summary>
        /// 物流单号
        /// </summary>
        public string DeliveryNo { get; set; }
        /// <summary>
        /// 退货物流单号
        /// </summary>
        public string RefundDeliveryNo { get; set; }
        /// <summary>
        /// 订单交易号
        /// </summary>
        public string OrderExchangeNo { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Price { get; set; } 
        /// <summary>
        /// 是否验收
        /// </summary>
        public string IsCheck { get; set; }
        /// <summary>
        /// 是否作废
        /// </summary>
        public string IsInvalid { get; set; }
    }

    /// <summary>
    /// 按店铺分析
    /// </summary>
    public class ShopRefundOrderByShopModel
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// 按商品分析
    /// </summary>
    public class ShopRefundOrderByProductModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
