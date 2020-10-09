using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Products
{
    public class SyncProductStockModel
    {
        public int ShopId { get; set; }
        public string Code { get; set; }
        public int Stock { get; set; }
    }
}
