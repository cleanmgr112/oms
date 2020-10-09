using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.DataStatistics
{
    public class OrderDeliveryModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 发货日期
        /// </summary>
        public DateTime? DeliverDate { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayDate { get; set; }

        /// <summary>
        /// 下单日期
        /// </summary>
        public DateTime OrderDate { get; set; }
        public int DeliveryId { get; set; }
        /// <summary>
        /// 快递公司
        /// </summary>
        public string DeliverName { get; set; }
        /// <summary>
        /// 发货单编号
        /// </summary>
        public string InvoiceSerialNumber { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public int PayTypeId { get; set; }
        public string PayTypeName { get; set; }
        /// <summary>
        /// 快递单号
        /// </summary>
        public string DeliverSerialNumber { get; set; }
        /// <summary>
        /// 交易号
        /// </summary>
        public string BusinessSerialNumber { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        public string ReceiveName { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 店铺
        /// </summary>
        public int ShopId { get; set; }
        public string ShopName { get; set; } 
        /// <summary>
        /// 运费
        /// </summary>
        public decimal DeliveryCost { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 订单已付金额
        /// </summary>
        public decimal PayedPrice { get; set; }
        /// <summary>
        /// 商品均摊总金额
        /// </summary>
        public decimal AverageSumPrice { get; set; }
        /// <summary>
        /// 发货仓库
        /// </summary>
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
    }
}
