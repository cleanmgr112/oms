using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Order
{
    /// <summary>
    /// 商城调用接口获取订单详情模型
    /// </summary>
    public class InterfaceOrderModel
    {
        /// <summary>
        /// OMS系统订单ID
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// OMS系统订单编号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// (必填)平台订单号（关联商城订单或者其他平台订单的单号）
        /// </summary>
        public string PSerialNumber { get; set; }
        /// <summary>
        /// 原始单号（退单，拆分之类操作产生）
        /// </summary>
        public string OrgionSerialNumber { get; set; }
        /// <summary>
        ///（必填）店铺Id
        /// </summary>
        public int ShopId { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 订单状态名
        /// </summary>
        public string StateName { get; set; }
        /// <summary>
        /// 价格类型（一般都是 103 标准价）
        /// </summary>
        public int PriceTypeId { get; set; }
        /// <summary>
        /// （必填）付款方式
        /// </summary>
        public int PayType { get; set; }
        /// <summary>
        /// 付款方式名
        /// </summary>
        public string PayTypeName { get; set; }
        /// <summary>
        /// （选填，B2B所属，默认填0）汇款方式
        /// </summary>
        public int PayMentType { get; set; }
        /// <summary>
        /// 汇款方式名
        /// </summary>
        public string PayMentTypeName { get; set; }
        /// <summary>
        /// 付款状态
        /// </summary>
        public PayState PayState { get; set; }
        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal SumPrice { get; set; }
        /// <summary>
        /// 订单已支付金额
        /// </summary>
        public decimal PayPrice { get; set; }
        /// <summary>
        /// （必填）订单物流方式
        /// </summary>
        public int DeliveryTypeId { get; set; }
        /// <summary>
        /// 订单物流单号
        /// </summary>
        public string DeliveryNumber { get; set; }
        /// <summary>
        /// 关联客户ID
        /// </summary>
        public int CustomerId { get; set; }
        /// <summary>
        /// 关联客户ID名
        /// </summary>
        public string CustomerIdName { get; set; }
        /// <summary>
        /// （必填）订单收货人姓名
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// （必填）订单收货人电话
        /// </summary>
        public string CustomerPhone { get; set; }
        /// <summary>
        /// （必填）订单收货人地址
        /// </summary>
        public string CustomerAddressDetail { get; set; }
        /// <summary>
        /// 客户留言
        /// </summary>
        public string CustomerMark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AdminMark { get; set; }
        /// <summary>
        /// 给仓库留言
        /// </summary>
        public string ToWarehouseMessage { get; set; }
        /// <summary>
        /// 发货仓库Id
        /// </summary>
        public int WarehouseId { get; set; }
        /// <summary>
        /// 发货仓库名
        /// </summary>
        public string WarehouseName { get; set; }
        /// <summary>
        /// 关联商城购买者帐号
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 发票类型  0:不需要发票 1：个人发票 2：普通单位发票 3：专用发票
        /// </summary>
        public InvoiceType InvoiceType { get; set; }
        public InvoiceModeEnum InvoiceMode { get; set; }
        /// <summary>
        /// 是否需要纸袋
        /// </summary>
        public bool IsNeedPaperBag { get; set; }
        /// <summary>
        /// 业务员ID
        /// </summary>
        public int? SalesManId { get; set; }
        /// <summary>
        /// 财务备注
        /// </summary>
        public string FinanceMark { get; set; }
        /// <summary>
        /// （必填）支付日期，格式（yyyyMMddHHmmss）
        /// </summary>
        public string PayDate { get; set; }
        /// <summary>
        /// 商城代金券优惠信息
        /// </summary>
        public string ProductCoupon { get; set; }
        /// <summary>
        /// 订单抵扣的中民网积分转换后的价格
        /// </summary>
        public decimal? ZMIntegralValuePrice { get; set; }
        /// <summary>
        /// 订单使用的中民券
        /// </summary>
        public double ZMCoupon { get; set; }
        /// <summary>
        /// 订单使用的中民红酒券
        /// </summary>
        public double ZMWineCoupon { get; set; }
        /// <summary>
        /// 订单所使用的红酒网红酒券
        /// </summary>
        public double WineWorldCoupon { get; set; }
        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 订单开票信息
        /// </summary>
        public InvoiceInfo InvoiceInfo { get; set; }
        /// <summary>
        /// 订单支付信息
        /// </summary>
        public IEnumerable<OrderPayPrice> OrderPayPrice { get; set; }
        /// <summary>
        /// 订单商品
        /// </summary>
        public IEnumerable<InterFaceOrderProduct> Products { get; set; }
    }
    public class InterFaceOrderProduct
    {
        /// <summary>
        /// 商品名
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 商品数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 商品原价（标准价）
        /// </summary>
        public decimal OriginPrice { get; set; }
        /// <summary>
        /// 商品销售价格（实际购买价格）
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 商品总价
        /// </summary>
        public decimal SumPrice { get; set; }
    }
}
