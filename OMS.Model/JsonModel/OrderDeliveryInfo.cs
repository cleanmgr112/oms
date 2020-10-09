using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    /// <summary>
    /// 订单快递信息model
    /// </summary>
    public class OrderDeliveryInfo
    {
        /// <summary>
        /// wms中Order表的Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// wms中Order表的serialNumber
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// oms中Order标的serialNumber
        /// </summary>
        public string OMSSerialNumber { get; set; }
        /// <summary>
        /// wms中快递日期
        /// </summary>
        public DateTime? DeliveryDate { get; set; }
        /// <summary>
        /// wms中快递单号
        /// </summary>
        public string DeliveryNumber { get; set; }
        /// <summary>
        /// wms中快递编码
        /// </summary>
        public string DeliveryCode { get; set; }
        /// <summary>
        /// wms中快递状态
        /// </summary>
        public DeliveryState DeliveryState { get; set; }
    }
}
