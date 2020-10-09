using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class SaleProductDetailModel : ModelBase
    {
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public int Channel{ get; set; }
        public string ChannelName { get; set; }
        public int LockStock { get; set; }
        public int AvailableStock { get; set; }
        public int Stock { get; set; }
        public List<SaleProductPriceDetailModel> SaleProductPriceDetailModels { get; set; }
        
    }
    public class SaleProductPriceDetailModel
    {
        public int Id { get; set; }
        public int SaleProductId { get; set; }
        public int CustomerTypeId { get; set; }
        public string PriceTypeName { get; set; }
        public decimal Price { get; set; }
    }
}
