using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class PurchasingProducts:EntityBase
    {
        public int PurchasingId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? FactReceivedNum { get; set; }
        public decimal Price { get; set; }

        /// <summary>
        /// 关联原退货订单商品Id
        /// </summary>
        public int? OrginId { get; set; }
    }
}
