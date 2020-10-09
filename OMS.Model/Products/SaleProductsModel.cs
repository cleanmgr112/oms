using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Products
{
    public class SaleProductsModel
    {
        public int SaleProductId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public decimal Price { get; set; }
    }
}
