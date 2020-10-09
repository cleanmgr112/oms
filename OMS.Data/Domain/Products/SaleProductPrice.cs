using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class SaleProductPrice:EntityBase
    {
        public int SaleProductId { get; set; }
        public int CustomerTypeId { get; set; }
        public decimal Price { get; set; }
        public SaleProduct SaleProduct { get; set; }
    }
}
