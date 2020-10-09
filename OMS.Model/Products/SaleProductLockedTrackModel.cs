using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class SaleProductLockedTrackModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 订单ID
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// 订单编码（非必填）
        /// </summary>
        public string OrderSerialNumber { get; set; }
        /// <summary>
        /// OMS商品ID
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        /// <summary>
        /// OMS销售商品ID（非必填）
        /// </summary>
        public int SaleProductId { get; set; }
        /// <summary>
        /// 锁定数量
        /// </summary>
        public int LockNumber { get; set; }
        /// <summary>
        /// 锁定订单商品所属仓库
        /// </summary>
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
        public string WareHouseCode { get; set; }
    }
}
