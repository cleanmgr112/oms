using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class SaleProduct:EntityBase
    {
        public int Channel { get; set; }
        public int ProductId { get; set; }
        public int LockStock { get; set; }
        public int Stock { get; set; }
        public int AvailableStock { get; set; }
        public virtual Product Product { get; set; }
        public List<SaleProductPrice> SaleProductPrice { get; set; }
        public List<OrderProduct> OrderProduct { get; set; }
    }
}
