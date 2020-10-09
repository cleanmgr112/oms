using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    /// <summary>
    /// 订单商品信息
    /// </summary>
    public class OrderProductInfo
    {
        /// <summary>
        /// 商品与系统方对接一直的唯一标识商品的键
        /// </summary>
        public string goods_sn { get; set; }

        public string goods_name { get; set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public string item_total { get; set; }

        /// <summary>
        /// 销售单价
        /// </summary>
        public string sale_price { get; set; }
        /// <summary>
        /// 是否是酒款（用于纸袋数量计算，efast不需要此字段）
        /// </summary>
        public bool isWine { get; set; }
        /// <summary>
        /// 酒款Id（用于纸袋数量计算，efast不需要此字段）
        /// </summary>
        public string wineId { get; set; }
    }
}
