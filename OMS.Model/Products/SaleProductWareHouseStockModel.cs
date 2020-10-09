using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class SaleProductWareHouseStockModel
    {
        public int Id { get; set; }
        public int SaleProductId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }
        public string WareHouseCode { get; set; }
        public int Stock { get; set; }
        public int LockStock { get; set; }
        //public virtual SaleProduct SaleProduct { get; set; }
        //public virtual WareHouse WareHouse { get; set; }
    }
}
