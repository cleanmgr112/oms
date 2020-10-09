using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class SaleProductPriceBaseList:EntityBase
    {
        public int ProductId { get; set; }
        public int SaleProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Channel { get; set; }
        public string ChannelName { get; set; }
        public int CustomerTypeId { get; set; }
        public string CustomerTypeName { get; set; }
        public int Stock { get; set; }
        public int LockStock { get; set; }
        public int AvailableStock { get; set; }
        public decimal Price { get; set; }
    }
}
