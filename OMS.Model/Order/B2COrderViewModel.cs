using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Order
{
    /// <summary>
    /// B2C订单显示模型
    /// </summary>
    public class B2COrderViewModel
    {

        /// <summary>
        /// 订单ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public int ShopId { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 收货人电话
        /// </summary>
        public string CustomerPhone { get; set; }
        /// <summary>
        /// 快递名称
        /// </summary>
        public string DeliveryName { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string WareHouseName { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderState State { get; set; }

        /// <summary>
        /// 是否已锁定
        /// </summary>
        public bool IsLocked { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// 订单总价
        /// </summary>
        public decimal SumPrice { get; set; }
        /// <summary>
        /// 平台订单号
        /// </summary>
        public string PSerialNumber { get; set; }
        /// <summary>
        /// 支付方式（线上支付、款到发货、货到付款等。数据源-字典表）
        /// </summary>
        public int PayType { get; set; }
        /// <summary>
        /// 支付名称
        /// </summary>
        public string PayTypeName { get; set; }
        /// <summary>
        /// 支付状态
        /// </summary>
        public PayState PayState { get; set; }
        /// <summary>
        /// 支付状态名称
        /// </summary>
        public string PayStateName { get; set; }
        /// <summary>
        /// 物流单号
        /// </summary>
        public string DeliveryNumber { get; set; }

        /// <summary>
        /// 发票类型（0：不用发票，1：普通个人发票；2：普通单位发票；3：专票）
        /// </summary>
        public InvoiceType InvoiceType { get; set; }
        /// <summary>
        /// 发票类型
        /// </summary>
        public string InvoiceTypeName { get; set; }
        /// <summary>
        /// 客服备注
        /// </summary>
        public string AdminMark { get; set; }
        /// <summary>
        /// 客户备注说明
        /// </summary>
        public string CustomerMark { get; set; }
        /// <summary>
        /// 是否需要纸袋
        /// </summary>
        public bool IsNeedPaperBag { get; set; }
        /// <summary>
        /// 是否有历史订单
        /// </summary>
        public bool historyOrders { get; set; }
        /// <summary>
        /// 是否开发票
        /// </summary>
        public bool invoiceInfo { get; set; }
        /// <summary>
        /// 是否有退单
        /// </summary>
        public bool refundOrder { get; set; }
        /// <summary>
        /// 退单号
        /// </summary>
        public string refundNum { get; set; }
        /// <summary>
        /// 是否缺货
        /// </summary>
        public bool lackStock { get; set; }
        /// <summary>
        /// 下单客户的商城账号
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 拆分
        /// </summary>
        public bool chaiFen { get; set; }
        /// <summary>
        /// OMS中的原单号
        /// </summary>
        public string OrgionSerialNumber { get; set; }
        /// <summary>
        /// 合并
        /// </summary>
        public bool heBing { get; set; }
        /// <summary>
        /// 发货仓ID
        /// </summary>
        public int WarehouseId { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayDate { get; set; }
        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? DeliveryDate { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>

        public string AddressDetail { get; set; }


    }
}
