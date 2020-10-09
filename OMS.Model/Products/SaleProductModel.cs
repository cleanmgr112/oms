using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class SaleProductModel:ModelBase
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Channel { get; set; }
        public string Value { get; set; }
        public decimal Price { get; set; }
        public string PriceType { get; set; }
        /// <summary>
        /// 锁定库存
        /// </summary>
        public int LockStock { get; set; }
        /// <summary>
        /// 总库存
        /// </summary>
        public int Stock { get; set; }
        /// <summary>
        /// 可售库存
        /// </summary>
        public int AvailableStock { get; set; }
        public Product Product { get; set; }
        public List<SaleProductPriceModel> SaleProductPriceModel { get; set; }
    }
}
