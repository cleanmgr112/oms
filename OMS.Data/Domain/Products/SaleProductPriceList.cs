using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class SaleProductPriceList:EntityBase
    {
        public int SaleProductId { get; set; }
        public string SaleProductName { get; set; }
        public string SaleProductCode { get; set; }

        public string ChannelName { get; set; }
        public int Stock { get; set; }
        public int LockStock { get; set; }
        public int AvailableStock { get; set; }
        public List<SaleProductPriceBaseList> SaleProductPriceBaseList { get; set; }
    }
}
