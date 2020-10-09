using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class PlatformProductModel
    {
        /// <summary>
        /// 销售商品ID
        /// </summary>
        public int Id { get; set; }
        public int SaleProductId { get; set; }
        public string ProductName { get; set; }
        /// <summary>
        /// 销售平台
        /// </summary>
        public int PlatForm { get; set; }
        public string PlatFormName { get; set; }
        /// <summary>
        /// 销售平台商品代码
        /// </summary>
        public string PlatFormProductCode { get; set; }
    }
}
