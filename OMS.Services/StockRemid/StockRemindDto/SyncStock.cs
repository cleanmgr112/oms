

namespace OMS.Services.StockRemid.StockRemindDto
{
    /// <summary>
    /// 同步库存
    /// </summary>
    public class SyncStock
    {
        /// <summary>
        /// 销售产品ID
        /// </summary>
        public int SaleProductId { get; set; }

        /// <summary>
        /// 可用库存
        /// </summary>
        public int Stock { get; set; }
    }
}
