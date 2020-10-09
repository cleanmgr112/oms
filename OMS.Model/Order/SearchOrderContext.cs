using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Order
{
    public class SearchOrderContext
    {
        /// <summary>
        /// 查询字符串
        /// </summary>
        public string SearchStr { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public int? ShopId { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 支付开始时间
        /// </summary>
        public DateTime? PayStartTime { get; set; }
        /// <summary>
        /// 支付结束时间
        /// </summary>
        public DateTime? PayEndTime { get; set; }
        /// <summary>
        /// 发货开始时间
        /// </summary>
        public DateTime? DeliverStartTime { get; set; }
        /// <summary>
        /// 发货结束时间
        /// </summary>
        public DateTime? DeliverEndTime { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderState? OrderState { get; set; }
        /// <summary>
        /// 仓库ID
        /// </summary>
        public int? WareHouseId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool? IsLocked { get; set; }
        /// <summary>
        /// 是否查询时去除无效订单
        /// </summary>
        public bool? Isvalid { get; set; }
    }
}
