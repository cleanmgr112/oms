using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class OrderProductModel : ModelBase
    {
        public int OrderId { get; set; }
        public int SaleProductId { get; set; }
        public int Quantity { get; set; }
        public decimal OrginPrice { get; set; }
        public decimal Price { get; set; }
        public decimal SumPrice { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int? Type { get; set; }
        public SaleProductModel SaleProductModel { get; set; }
    }
    /// <summary>
    /// 订单详情页商品Model
    /// </summary>
    public class OrderDetailProductsModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int SaleProductId { get; set; }
        public int Quantity { get; set; }
        public decimal OrginPrice { get; set; }
        public decimal Price { get; set; }
        public decimal SumPrice { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        /// <summary>
        /// 商品的总库存
        /// </summary>
        public int TotalQuantity { get; set; }
        public SaleProductModel SaleProductModel { get; set; }
        /// <summary>
        /// 商品类型（数据源-字典表；销售品、赠品、辅料等）
        /// </summary>
        public int? Type { get; set; }
        /// <summary>
        /// 是否缺货
        /// </summary>
        public bool IsLackStock { get; set; }
        /// <summary>
        /// 商品锁定数
        /// </summary>
        public int LockNum { get; set; }
        /// <summary>
        /// 是否存在退货
        /// </summary>
        public bool IsRefund { get; set; }
        /// <summary>
        /// 退货数量
        /// </summary>
        public int RefundQuantity { get; set; }
    }
    /// <summary>
    /// 可以进行退货的商品模型
    /// </summary>
    public class CanRefundOrderProduct
    {
        /// <summary>
        /// 订单商品ID
        /// </summary>
        public int OrderProductId { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public int SaleProductId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 可以退货的数量
        /// </summary>
        public int CanRedundQuantity { get; set; }

    }
}
