using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    /// <summary>
    /// 订单优惠券等信息
    /// </summary>
    public class OrderCouponInfo
    {
        /// <summary>
        /// 优惠名称
        /// </summary>
        public string coupon_name { get; set; }

        /// <summary>
        /// 优惠类型
        /// </summary>
        public string coupon_type { get; set; }

        /// <summary>
        /// 抵扣金额
        /// </summary>
        public string coupon_price { get; set; }

        /// <summary>
        /// 所属订单Id
        /// </summary>
        public string order_id { get; set; }
    }
}
