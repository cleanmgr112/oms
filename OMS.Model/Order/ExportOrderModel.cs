using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Order
{
    public class ExportOrderModel
    {
        public int Id { get; set; }
        public string CreatedTime { get; set; }
        public string SerialNumber { get; set; }
        public string ShopName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// 订单优惠前总价
        /// </summary>
        public string SumOrginPrice { get; set; }
        public string PSerialNumber { get; set; }
        public string OrgionSerialNumber { get; set; }

        public string PayTypeName { get; set; }
        public string PayStateName { get; set; }
        public string DeliveryNumber { get; set; }
        public string InvoiceTypeName { get; set; }
        public string StateName { get; set; }
        public string AdminMark { get; set; }
        public string UserName { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceHead { get; set; }
        public string DeliveryTypeName { get; set; }
        public string OrderMark { get; set; }
        public string ToWareHouseMark { get; set; }
        /// <summary>
        /// 积分
        /// </summary>
        public string IntegralValue { get; set; }
        /// <summary>
        /// 优惠券
        /// </summary>
        public string ProductCoupon { get; set; }

        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public string WareHouseName { get; set; }
        /// <summary>
        /// 均摊金额
        /// </summary>
        public decimal AvgPrice { get; set; }
        /// <summary>
        /// 订单均摊金额
        /// </summary>
        public decimal SumAvgPrice { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// 标准价
        /// </summary>
        public decimal OrginPrice { get; set; }


        /// <summary>
        /// 现金支付金额
        /// </summary>
        public decimal PayMoneyPrice { get; set; }
        /// <summary>
        /// 订单已付款金额
        /// </summary>
        public decimal PayPrice { get; set; }

        /// <summary>
        /// 是否缺货
        /// </summary>
        public bool IsLackStock { get; set; }
        public DateTime SortTime { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// 销售人员名称
        /// </summary>
        public string SalesManName { get; set; }
        /// <summary>
        /// 客户类型名称
        /// </summary>
        public string UserTypeName { get; set; }
        /// <summary>
        /// 发货时间
        /// </summary>
        public string DeliveryDate { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public string PayDate { get; set; }

        public int CustomerTypeId { get; set; }

        /// <summary>
        /// 回款状态
        /// </summary>
        public string IsBookKeepingStr { get; set; }
    }
}
