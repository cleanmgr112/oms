

using System;
using System.Collections.Generic;

namespace OMS.Services.StockRemid.StockRemindDto
{
    /// <summary>
    /// 提醒模板dto  1.
    /// </summary>
    public class RemindTemplateDto
    {

        /// <summary>
        /// 模板编号 
        /// </summary>
        public string TemplateCode { get; set; }


        /// <summary>
        /// 模板标题 可按规定的规则设置
        /// </summary>
        public string TemplateTitle { get; set; }


        /// <summary>
        /// 授权用户 Json{ userid，username}
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 销售产品id
        /// </summary>
        public int SaleProductId { get; set; }

        /// <summary>
        /// 状态开关
        /// </summary>
        public bool Statu { get; set; }

        /// <summary>
        /// 预警库存
        /// </summary>
        public int RemindStock { get; set; }


        public RemindProductdto Product { get; set; }
    }

    /// <summary>
    /// 提醒标题   1.1
    /// </summary>
    public class RemindTitleDto
    {
        public string TitleId { get; set; }

        /// <summary>
        /// 提醒标题
        /// </summary>
        public string RemindTitle { get; set; }


        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        ///  创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 提醒模板下的产品   1.2
    /// </summary>
    public class RemindProductdto
    {

        /// <summary>
        /// 商品编号
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 中文名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string En { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
    }

    /// <summary>
    /// 前台到后台
    /// </summary>
    public class TemplateSaleDto
    {
        /// <summary>
        /// 模板编号
        /// </summary>
        public string TemplateCode { get; set; }


        /// <summary>
        /// 销售产品id
        /// </summary>
        public int SaleProductId { get; set; }

        public string ProductCode { get; set; }
        /// <summary>
        /// 中文名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string En { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
    }

    /// <summary>
    /// 后台->前台
    /// </summary>
    public class TitleUserDto
    {
        /// <summary>
        /// 用户
        /// </summary>
        public List<UserDto> User { get; set; } = new List<UserDto>();

        /// <summary>
        /// 模板标题
        /// </summary>
        public string TemplateTitle { get; set; } = string.Empty;

        /// <summary>
        /// 预警库存
        /// </summary>
        public int? RemindStock { get; set; }
    }


    public class UserDto
    {
        public int Id { get; set; }

        public string UserName { get; set; }
    }



    public class TitleSearchDto
    {

        /// <summary>
        /// 模板编号
        /// </summary>
        public string TemplateCode { get; set; }

        public RemindProductdto Product { get; set; }
    }


    /// <summary>
    /// 模板设置dto
    /// </summary>
    public class SetDto
    {
        public List<TemplateSaleDto> Key { get; set; }
        public List<UserDto> User { get; set; }
        public string TemplateTitle { get; set; }

        public int RemindStock { get; set; }
       
    }

    public class SearchDto
    {
        public DateTime? Min { get; set; }
        public DateTime? Max { get; set; }
    }

}
