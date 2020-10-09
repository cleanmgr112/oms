using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{

    /// <summary>
    /// 仓库对应的区域优先级
    /// </summary>
    public class WareHouseAreaRanks : EntityBase
    {
        /// <summary>
        /// 仓库对应ID
        /// </summary>
        public int WhId { get; set; }
        /// <summary>
        /// 仓库区域Id
        /// </summary>
        public int WhAId { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Rank { get; set; }
        /// <summary>
        /// 对应仓库
        /// </summary>
        public WareHouse WareHouse { get; set; }
    }
}
