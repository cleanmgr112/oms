using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class SaleProductPriceModel
    {
        public int Id { get; set; }
        public int SaleProductId { get; set; }
        public int Stock { get; set; }
        public int CustomerTypeId { get; set; }
        public decimal Price { get; set; }
        public int BzPriceId { get; set; }
        public decimal BzPrice { get; set; }
        public int PfPriceId { get; set; }
        public decimal PfPrice { get; set; }
        public int JxsPriceId { get; set; }
        public decimal JxsPrice { get; set; }
        public int TgPriceId { get; set; }
        public decimal TgPrice { get; set; }
        public int NbPriceId { get; set; }
        public decimal NbPrice { get; set; }

    }
}
