using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class Product:EntityBase
    {
        /// <summary>
        /// 商品中文名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 商品英文名
        /// </summary>
        public string NameEn { get; set; }
        /// <summary>
        /// 商品类型（数据源-字典表-type=1）
        /// </summary>
        public int? Type { get; set; }
        public string Cover { get; set; }
        /// <summary>
        /// 国家（数据源-字典表-type=2）
        /// </summary>
        public int? Country { get; set; }
        /// <summary>
        /// 产区（数据源-字典表-type=3）
        /// </summary>
        public int? Area { get; set; }
        /// <summary>
        /// 葡萄品种（数据源-字典表-type=4）
        /// </summary>
        public string Grapes { get; set; }
        /// <summary>
        /// 容量（数据源-字典表-type=5）
        /// </summary>
        public int? Capacity { get; set; }
        public string Year { get; set; }
        /// <summary>
        /// 包装方式（数据源-字典表-type=6）
        /// </summary>
        public int? Packing { get; set; }
        /// <summary>
        /// 所属系列Id
        /// </summary>
        public int? CategoryId { get; set; }
        /// <summary>
        /// 商品唯一编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 商品副条码
        /// </summary>
        public string DeputyBarcode { get; set; }
        /// <summary>
        /// 是否同步商品到WMS
        /// </summary>
        public bool? IsSynchronized { get; set; }
        public virtual Dictionary Dictionary { get; set; }
        public List<SaleProduct> SaleProduct { get; set; }
        
    }
}
