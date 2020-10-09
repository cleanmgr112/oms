using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class SaleProductWareHouseStock:EntityBase
    {
        public int SaleProductId { get; set; }
        public int ProductId { get; set; }
        public int WareHouseId { get; set; }
        public int Stock { get; set; }
        public int LockStock { get; set; }
        public virtual SaleProduct SaleProduct { get; set; }
        public virtual WareHouse WareHouse { get; set; }
    }
}
