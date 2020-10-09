using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Products
{
    public class SearchProductContext
    {

        public SearchProductContext() {
            this.PageIndex = 1;
            this.PageSize = 10;
        }
        public string SearchStr { get; set; }
        public int? WareHouseId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int AvailableStockMin { get; set; }
        public int AvailableStockMax { get; set; }
        public decimal SalePriceMin { get; set; }
        public decimal SalePriceMax { get; set; }
    }
}
