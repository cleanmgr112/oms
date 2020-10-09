using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Order
{
    /// <summary>
    ///  WMS订单同步Model
    /// </summary>
    public class SyncOrderStateModel
    {
        /// <summary>
        /// 订单在WMS中的单号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// OMS单号
        /// </summary>
        public string OMSSerialNumber { get; set; }
        /// <summary>
        /// 发货状态（未发货【默认值】，已发货，部分发货）
        /// </summary>
        public DeliveryStateEnum DeliveryState { get; set; }
        /// <summary>
        /// 快递单号
        /// </summary>
        public string DeliveryNumber { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderTypeEnum OrderType { get; set; }

    }
    /// <summary>
    /// 订单配送状态
    /// </summary>
    public enum DeliveryStateEnum : short
    {
        /// <summary>
        /// 初始状态，未发货【默认值】
        /// </summary>
        Unshipped = 0,
        /// <summary>
        /// 拣货中
        /// </summary>
        picking = 1,
        /// <summary>
        /// 拣货完成 == 已称重
        /// </summary>
        picked = 2,
        /// <summary>
        /// 已出库，完成快递交接
        /// </summary>
        DeliveredExpress = 3,
        /// <summary>
        /// 已出库，完成物流交接
        /// </summary>
        DeliveredLogistics = 4
    }
    public enum OrderTypeEnum
    {
        /// <summary>
        /// b2b订单
        /// </summary>
        B2B = 0,
        /// <summary>
        /// b2c订单
        /// </summary>
        B2C = 1,
        /// <summary>
        /// 未知
        /// </summary>
        UnKnow = -1
    }
}
