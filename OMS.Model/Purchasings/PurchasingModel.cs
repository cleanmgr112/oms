using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class PurchasingModel:ModelBase
    {
        public string PurchasingNumber { get; set; }
        public string PurchasingOrderNumber { get; set; }
        public string OrgionSerialNumber { get; set; }
        public int SupplierId { get; set; }
        public int WareHouseId { get; set; }
        public string Mark { get; set; }
        public string WareHouseName { get; set; }
        public string SupplierName { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public List<PurchasingProducts> PurchasingProduct { get; set; }
    }
    public class PurchasingProductModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int PurchasingId { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public decimal SumPrice { get; set; }
    }

    public class PurchasingRefundOrderModel
    {
        public string PurchasingOriginalNumber { get; set; }
        public string PurchasingPlanSerialNumber { get; set; }
        public int SupplierId { get; set; }
        public int WareHouseId { get; set; }
        public string Mark { get; set; }
    }
}
