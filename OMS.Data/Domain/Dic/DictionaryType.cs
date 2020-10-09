using System.ComponentModel;

namespace OMS.Data.Domain
{
    public enum DictionaryType : int
    {
        /// <summary>
        /// 商品类型，A,B,C类,辅料，合作商商品
        /// </summary>
        [Description("商品类型")]
        ProductType = 1,
        /// <summary>
        /// 国家
        /// </summary>
        [Description("国家")]
        Country = 2,
        /// <summary>
        /// 产区
        /// </summary>
        [Description("产区")]
        Area = 3,
        /// <summary>
        /// 葡萄品种
        /// </summary>
        [Description("葡萄品种")]
        Variety = 4,
        /// <summary>
        /// 容量
        /// </summary>
        [Description("容量")]
        capacity = 5,
        /// <summary>
        /// 包装方式
        /// </summary>
        [Description("包装")]
        Packing = 6,
        /// <summary>
        /// 渠道类型（现货、跨境、期酒）
        /// </summary>
        [Description("渠道类型")]
        Channel = 7,
        /// <summary>
        /// 会员商城
        /// </summary>
        [Description("会员商城")]
        Platform = 8,
        /// <summary>
        /// 价格类型
        /// </summary>
        [Description("价格类型")]
        PriceType = 9,
        /// <summary>
        /// 客户类型
        /// </summary>
        [Description("客户类型")]
        CustomerType = 10,
        /// <summary>
        /// 付款方式（款到发货，货到付款...）
        /// </summary>
        [Description("付款方式")]
        PayStyle = 11,
        /// <summary>
        /// 付款类型（微信，支付宝，Pos机）
        /// </summary>
        [Description("付款类型")]
        PayType = 12,
        /// <summary>
        /// 货物类型（销售品，赠品，辅料）
        /// </summary>
        [Description("货物类型")]
        GoodsType = 13,
        /// <summary>
        /// 代销平台
        /// </summary>
        [Description("代销平台")]
        Consignment = 14,
        /// <summary>
        /// 第三方网销平台
        /// </summary>
        [Description("第三方网销平台")]
        ThirdpartyOnlineSales = 15
    }
}
