using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Products
{
    public class SuitProductsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public IEnumerable<SuitProductsDetailModel> SuitProductsDetail { get; set; }
    }
}
