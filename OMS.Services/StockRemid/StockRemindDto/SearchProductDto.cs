using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.StockRemid.StockRemindDto
{
    /// <summary>
    /// 搜索条件dto
    /// </summary>
    public class SearchProductDto
    {

        /// <summary>
        /// 最低价格
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// 最高价格
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// 商品类型
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// 中文名/英文名/商品编码
        /// </summary>
        public string  NameCode { get; set; }
    }

    /// <summary>
    /// 销售产品dto
    /// </summary>
    public class SaleProductDto
    {
        /// <summary>
        /// 销售id
        /// </summary>
        public int SaleProductId { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string En { get; set; }

        /// <summary>
        /// 销售价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool Statu { get; set; } 

        /// <summary>
        /// 模板编号
        /// </summary>
        public string TemplateCode  { get; set; }


        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductCode { get; set; }
    }

    /// <summary>
    /// 模板开关dto
    /// </summary>
    public class templateSwtichDto
    {
        /// <summary>
        /// 模板编号
        /// </summary>
        public string TemplateCode { get; set; }

        /// <summary>
        /// 销售产品id
        /// </summary>
        public int  SaleProductId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool Statu { get; set; }
    }


}
