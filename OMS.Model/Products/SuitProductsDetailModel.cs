using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Products
{
    public class SuitProductsDetailModel
    {
        public int Id { get; set; }
        public int SaleProductId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
    }
}
