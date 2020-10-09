using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Security.Policy;

namespace OMS.Services.StockRemid.StockRemindDto
{

    /// <summary>
    /// 仓库=>国家-》商品=>具体商品的规格（年限/容量/产区/国家等等）
    /// </summary>
    public class WareHouseDto
    {
        /// <summary>
        /// 仓库
        /// </summary>
        public string WareHouse { get; set; }


        /// <summary>
        /// 国家
        /// </summary>
        public List<CountryDto> Countries { get; set; }


    }

    public class CountryDto
    {


        /// <summary>
        /// 国家集合
        /// </summary>
        public string Countries { get; set; } = "无";

        /// <summary>
        /// 仓库下商品集合
        /// </summary>
        public List<WareHouseProductDto> WareHouseProduct { get; set; }
    }

    public class WareHouseProductDto
    {

        /// <summary>
        /// 产区
        /// </summary>
        public string Areas { get; set; }

        /// <summary>
        /// 葡萄品种
        /// </summary>
        public string Grapes { get; set; }

        /// <summary>
        /// 容量
        /// </summary>
        public string Capacities { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// 可用库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }

        public string Remark { get => $"{Year}年{Areas}{Grapes}{Capacities}¥{Price}"; }

    }

    public class Tree
    {

        /// <summary>
        /// 值
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 孩子结点
        /// </summary>
        public List<string> Children { get; set; }
    }
}
