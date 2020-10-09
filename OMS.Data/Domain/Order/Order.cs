using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class Order : EntityBase
    {
        public Order()
        {
            IsCopied = false;
        }
        /// <summary>
        /// OMS单号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 订单类型（B2B订单，B2C现货，B2C期酒，B2C跨境）
        /// </summary>
        public OrderType Type { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public int ShopId { get; set; }
        /// <summary>
        /// 平台订单号
        /// </summary>
        public string PSerialNumber { get; set; }
        /// <summary>
        /// OMS中的原单号
        /// </summary>
        public string OrgionSerialNumber { get; set; }
        /// <summary>
        /// OMS中原订单Id（B2C/B2B退单）
        /// </summary>
        public int? OriginalOrderId { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderState State { get; set; }
        /// <summary>
        /// 订单回写状态（未回写，回写下单成功，回写发货成功）
        /// </summary>
        public WriteBackState WriteBackState { get; set; }
        /// <summary>
        /// 支付方式（线上支付、款到发货、货到付款等。数据源-字典表）
        /// </summary>
        public int PayType { get; set; }
        /// <summary>
        /// 回款方式（现金、pos机刷卡、微信、支付宝等。数据源-字典表）
        /// </summary>
        public int PayMentType { get; set; }
        /// <summary>
        /// 支付状态
        /// </summary>
        public PayState PayState { get; set; }
        /// <summary>
        /// 转单时间
        /// </summary>
        public DateTime? TransDate { get; set; }
        /// <summary>
        /// 是否已锁定
        /// </summary>
        public bool IsLocked { get; set; }
        /// <summary>
        /// 订单当前锁定人
        /// </summary>
        public int LockMan { get; set; }
        /// <summary>
        /// 订单是否已锁定库存
        /// </summary>
        public bool LockStock { get; set; }
        /// <summary>
        /// 订单总价
        /// </summary>
        public decimal SumPrice { get; set; }
        /// <summary>
        /// 已支付金额
        /// </summary>
        public decimal PayPrice{ get; set; }
        /// <summary>
        /// 物流方式（外键）
        /// </summary>
        public int DeliveryTypeId { get; set; }
        /// <summary>
        /// 物流单号
        /// </summary>
        public string DeliveryNumber { get; set; }
        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? DeliveryDate { get; set; }
        /// <summary>
        /// 下单客户的商城账号
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 收货人电话
        /// </summary>
        public string CustomerPhone { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        public string AddressDetail { get; set; }
        /// <summary>
        /// 收货行政区ID
        /// </summary>
        public int DistrictId { get; set; }
        /// <summary>
        /// 客户备注说明
        /// </summary>
        public string CustomerMark { get; set; }
        /// <summary>
        /// 发票类型（0：不用发票，1：普通个人发票；2：普通单位发票；3：专票）
        /// </summary>
        public InvoiceType InvoiceType { get; set; }
        /// <summary>
        /// 客服备注
        /// </summary>
        public string AdminMark { get; set; }
        /// <summary>
        /// 给仓库留言
        /// </summary>
        public string ToWarehouseMessage { get; set; }
        /// <summary>
        /// 发货仓ID
        /// </summary>
        public int WarehouseId { get; set; }
        /// <summary>
        /// B2B订单客户ID
        /// </summary>
        public int CustomerId { get; set; }
        /// <summary>
        /// 字典表，价格类型 B2B订单 标准价，团购价，经销商价
        /// </summary>
        public int PriceTypeId { get; set; }
        /// <summary>
        /// 审核流程ID
        /// </summary>
        public int ApprovalProcessId { get; set; }
        public List<OrderApproval> OrderApproval { get; set; }
        public WareHouse WareHouse { get; set; }
        public Shop Shop { get; set; }
        public Delivery Delivery { get; set; }
        public List<OrderProduct> OrderProduct { get; set; }
        public List<OrderPayPrice> OrderPayPrice { get; set; }
        //订单表 关联大部分主外键关系，但是类似customer,ApprovalProcess则因为B2C订单没有这个字段所以不关联
        //还有就是不直接关联 dictionary字典表

        public InvoiceInfo InvoiceInfo { get; set; }

        /// <summary>
        /// 开票方式（0：电子发票【默认值】；1：纸质发票）
        /// </summary>
        public InvoiceModeEnum InvoiceMode { get; set; }
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
        /// 是否需要纸袋
        /// </summary>
        public bool IsNeedPaperBag { get; set; }
        /// <summary>
        /// 业务员Id(B2B订单业务员必填)
        /// </summary>
        public int? SalesManId { get; set; }
        /// <summary>
        /// 财务备注
        /// </summary>
        public string FinanceMark { get; set; }
        /// <summary>
        /// 订单支付时间
        /// </summary>
        public DateTime?  PayDate { get; set; }
        /// <summary>
        /// 附加类型
        /// </summary>
        public AppendType AppendType { get; set; }
        /// <summary>
        /// 是否已经复制过订单。无效订单可以复制，合并拆分后的原订单不能复制
        /// </summary>
        public bool? IsCopied { get; set; }
        /// <summary>
        /// 订单是否缺货
        /// </summary>
        public bool IsLackStock { get; set; }
    }
}
