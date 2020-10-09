using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class WareHouseArea : EntityBase
    {
        /// <summary>
        /// 关联仓库Id
        /// </summary>
        public int? WhId { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 区域代码
        /// </summary>
        public string AreaCode { get; set; }
    }
}
