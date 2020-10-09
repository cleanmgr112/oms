using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class PriceModel
    {
        public int BzPriceId { get; set; }
        public decimal BzPrice { get; set; }
        public int TgPriceId { get; set; }
        public decimal TgPrice { get; set; }
        public int JxsPriceId { get; set; }
        public decimal JxsPrice { get; set; }
        public int NbPriceId { get; set; }
        public decimal NbPrice { get; set; }
        public int PfPriceId { get; set; }
        public decimal PfPrice { get; set; }
    }
}
