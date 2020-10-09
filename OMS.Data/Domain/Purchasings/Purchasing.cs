using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Purchasings
{
    public class Purchasing:EntityBase
    {
        public string PurchasingNumber { get; set; }
        public string PurchasingOrderNumber { get; set; }
        public string OrgionSerialNumber { get; set; }
        public int SupplierId { get; set; }
        public int WareHouseId { get; set; }
        public PurchasingState State { get; set; }
        public string Mark { get; set; }

        /// <summary>
        /// 采购退单中的原始订单ID
        /// </summary>
        public int? OriginalOrderId { get; set; }
    }
}
