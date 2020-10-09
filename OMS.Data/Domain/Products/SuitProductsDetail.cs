using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
   public class SuitProductsDetail:EntityBase
    {
        public int SuitProductsId { get; set; }
        public int SaleProductId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
