using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
    public class WareHouseAreaViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 区域编码
        /// </summary>
        public string AreaCode { get; set; }
        /// <summary>
        /// 主要仓库名称
        /// </summary>
        public string MainWareHouse { get; set; }
    }

    public class WareHouseAreaModel
    {
        public int WareHouseId { get; set; }
        public int Rank { get; set; }
        public int Id { get; set; }
    }

    public class WareHouseAreaDetailModel
    {
        public int Id { get; set; }
        public string AreaName { get; set; }
        public string AreaCode { get; set; }
        public List<WareHouseRankModel> WareHouseRankModels { get; set; }
    }

    public class WareHouseRankModel
    {
        public int Id { get; set; }
        public int WareHouseAreaId { get; set; }
        public int WareHouseId { get; set; }
        public int Rank { get; set; }
    }
}
