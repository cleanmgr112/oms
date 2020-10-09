using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Purchasings
{
    public class PurchasingRefundOrderModelToWMS
    {
        public int Id { get; set; }
        public string PurchasingNumber { get; set; }
        public string PurchasingOrderNumber { get; set; }
        public string OrgionSerialNumber { get; set; }
        public string SupplierName { get; set; }
        public string WareHouseCode { get; set; }
        public string Mark { get; set; }
        public List<PurchasingProductModelToWMS> PurchasingProducts { get; set; }
    }
    public class PurchasingProductModelToWMS
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
