using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Order
{
    public class DBCheckDeliveryModel
    {
        /// <summary>
        /// OMS订单号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 发货仓库编码
        /// </summary>
        public string WareHouseCode { get; set; }
        public ReceiverInfo Receiver { get; set; }
        public SenderInfo Sender { get; set; }
    }

    /// <summary>
    /// 收货人信息
    /// </summary>
    public class ReceiverInfo
    {
        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区/县
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 收货人名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 电话号
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
    }

    /// <summary>
    /// 发货人信息
    /// </summary>
    public class SenderInfo
    {
        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区/县
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 电话号
        /// </summary>
        public string Phone { get; set; }
        public string Province { get; set; }
    }
}
