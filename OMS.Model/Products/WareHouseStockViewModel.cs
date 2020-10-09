using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Products
{
    /// <summary>
    /// 仓库库存
    /// </summary>
    public class WareHouseStockViewModel
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// 销售商品ID
        /// </summary>
        public int SaleProductId { get; set; }
        /// <summary>
        /// 销售单价
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal SumSalePrice { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        public int WareHouseId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName  { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WareHouseName { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }
        /// <summary>
        /// 锁定库存
        /// </summary>
        public int LockStock { get; set; }
        /// <summary>
        /// 可用库存
        /// </summary>
        public int AvailableStock { get; set; }
        /// <summary>
        ///总库存
        /// </summary>
        public int SumStock { get; set; }
        /// <summary>
        ///总锁定库存
        /// </summary>
        public int SumLockStock { get; set; }
        /// <summary>
        ///总可用库存
        /// </summary>
        public int SumAvailableStock { get; set; }
    }
}
