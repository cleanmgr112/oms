using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Products
{
    public class ProductViewModel
    {
        /// <summary>
        /// 封面图
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 副条码
        /// </summary>
        public string DeputyBarcode { get; set; }
        /// <summary>
        /// 商品名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 商品英文名
        /// </summary>
        public string NameEn { get; set; }
        /// <summary>
        /// 商品类型
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string CountryName { get; set; }
        /// <summary>
        /// 产区
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 种类
        /// </summary>
        public string Grapes { get; set; }
        /// <summary>
        /// 年份
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public int Id{ get; set; }
    }
}
