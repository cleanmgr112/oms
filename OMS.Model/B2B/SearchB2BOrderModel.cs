using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.B2B
{
    /// <summary>
    /// B2B搜索模型
    /// </summary>
    public class SearchB2BOrderModel
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 客户ID
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 业务员ID
        /// </summary>
        public int? SalesManId { get; set; }
        /// <summary>
        /// 最小总价
        /// </summary>
        public int? MinPrice { get; set; }
        /// <summary>
        /// 最大总价
        /// </summary>
        public int? MaxPrice { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderState? OrderState { get; set; }
        /// <summary>
        /// 搜索字符
        /// </summary>
        public string SearchStr { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public int? ShopId { get; set; }
        /// <summary>
        /// 仓库ID
        /// </summary>
        public int? WareHouseId { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderType OrderType { get; set; }
        /// <summary>
        /// 是否为退货单
        /// </summary>
        public bool IsRefundOrder { get; set; }
        /// <summary>
        /// 客户类型Id
        /// </summary>
        public int? CustomerTypeId { get; set; }
        /// <summary>
        /// 付款状态
        /// </summary>
        public int? BookKeepType { get; set; }
        /// <summary>
        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? DeliverStartTime { get; set; }
        public DateTime? DeliverEndTime { get; set; }



    }
}
