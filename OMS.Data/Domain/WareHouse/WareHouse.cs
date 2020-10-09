using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OMS.Data.Domain
{
    public class WareHouse : EntityBase
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsSyncStock { get; set; }
        public List<Order> Order{ get; set; }
        public List<WareHouseAreaRanks> WareHouseAreaRanks { get; set; }

        /// <summary>
        /// 仓库类型
        /// </summary>
        public WareHouseTypeEnum WareHouseType { get; set; }

        [NotMapped]
        public string WareHouseTypeName { get; set; }

    }

    /// <summary>
    /// 仓库类型
    /// </summary>
    public enum WareHouseTypeEnum : short
    {
        //正常仓库
        [Description("正常仓库")]
        Normal = 0,
        //线下店仓库
        [Description("线下店仓库")]
        Offlinestore = 1,
        //虚拟仓库
        [Description("虚拟仓库")]
        VirtualStore = 2,
        //WSET仓库
        [Description("WSET仓库")]
        WSETStore = 3,

    }
    public class WareHouseType
    {
        public int Type { get; set; }
        public string Name { get; set; }
    }
}
