using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Order
{
    public class CustomerHistoryOrderModel
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        public string CreatedTime { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        public string CustomerName { get; set; }
        public int Shop { get; set; }
        public string ShopName { get; set; }
        /// <summary>
        /// 已付款
        /// </summary>
        public decimal PayedPrice { get; set; }
        public string AdminMark { get; set; }
        public string CustomerMark { get; set; }
        /// <summary>
        /// 配送方式
        /// </summary>
        public int DeliveryTypeId { get; set; }
        public string DeliveryTypeName { get; set; }
        public string Address { get; set; }
    }
}
