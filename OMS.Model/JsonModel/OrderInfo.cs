using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    /// <summary>
    /// 订单信息
    /// </summary>
    public class OrderInfo
    {
        /// <summary>
        /// 商户的订单Id
        /// </summary>
        public string order_id { get; set; }

        /// <summary>
        /// 系统方订单号
        /// </summary>
        public string order_sn { get; set; }

        /// <summary>
        /// 订单类型,目前分普通订单，和套装仓订单
        /// </summary>
        public string order_type { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public string order_payment_type { get; set; }

        /// <summary>
        /// 商城支付号
        /// </summary>
        public string oid { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string user_name { get; set; }

        /// <summary>
        /// 订单备注
        /// </summary>
        public string order_remark { get; set; }

        /// <summary>
        /// 商家备注
        /// </summary>
        public string seller_remark { get; set; }

        /// <summary>
        /// 订单销售总价
        /// </summary>
        public string order_sum_price { get; set; }

        /// <summary>
        /// 订单实际价
        /// </summary>
        public string order_fact_price { get; set; }

        /// <summary>
        /// 订单生成时间
        /// </summary>
        public string order_generate_date { get; set; }

        /// <summary>
        /// 订单失效时间
        /// </summary>
        public string order_invalid_date { get; set; }

        /// <summary>
        /// 订单支付时间
        /// </summary>
        public string order_paydate { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string order_state { get; set; }

        /// <summary>
        /// 快递名称
        /// </summary>
        public string delivery_name { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string delivery_number { get; set; }

        /// <summary>
        /// 收货人基本信息
        /// </summary>
        public OrderConsigneeInfo consignee_info { get; set; }

        /// <summary>
        /// 订单中的商品信息
        /// </summary>
        public List<OrderProductInfo> product_info_list { get; set; }

        /// <summary>
        /// 订单中使用到的各种抵扣金额的类型优惠信息
        /// </summary>
        public List<OrderCouponInfo> coupon_detail_list { get; set; }

        /// <summary>
        /// 是否需要发票
        /// </summary>
        public bool is_need_invoice { get; set; }

        /// <summary>
        /// 订单发票信息
        /// </summary>
        public OrderInvoiceInfo order_invoice_info { get; set; }
        /// <summary>
        /// 是否需要纸袋
        /// </summary>
        public bool is_need_paperbag { get; set; }
        /// <summary>
        /// 商城代金券优惠信息
        /// </summary>
        public string product_coupon_info { get; set; }

    }
}
