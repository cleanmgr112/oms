using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
   public class PlatformProduct:EntityBase
    {
        /// <summary>
        /// 销售商品ID
        /// </summary>
        public int SaleProductId { get; set; }
        /// <summary>
        /// 销售平台
        /// </summary>
        public int PlatForm { get; set; }
        /// <summary>
        /// 销售平台商品代码
        /// </summary>
        public string PlatFormProductCode { get; set; }
    }
}
