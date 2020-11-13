
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace OMS.Model.StockRemind
{
    /// <summary>
    /// 库存提醒模板
    /// </summary>
    public class RemindTemplateModel : BaseModel
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        [Key]
        [StringLength(16)]
        public string TemplateId { get; set; } = GuidHelper.GuidTo16String();

        /// <summary>
        /// 模板编号
        /// </summary>
        [Required]
        [StringLength(17)]
        public string TemplateCode { get; set; } = GuidHelper.GuidTo16String();


        /// <summary>
        /// 模板标题 可按规定的规则设置
        /// </summary>
        [StringLength(200)]
        public string TemplateTitle { get; set; }


        /// <summary>
        /// 授权用户 Json{ userid，username}
        /// </summary>
        [StringLength(200)]
        public string User { get; set; } = JsonConvert.SerializeObject(new List<string>());

        /// <summary>
        /// 销售id
        /// </summary>
        public int SaleProductId { get; set; }

        public ICollection<RemindTitleModel> RemindTitles { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool Statu { get; set; }

        /// <summary>
        /// 用户可接受消息
        /// </summary>
        public ICollection<UserMessageModel> UserMessages { get; set; }


        /// <summary>
        /// 预警库存
        /// </summary>
        public int RemindStock { get; set; }


        /// <summary>
        /// 模板中的可销售商品
        /// </summary>
        public RemindProduct Product { get; set; }

        /// <summary>
        /// 是否更新
        /// </summary>
        public bool IsUpdate { get; set; } = true;

    }

    public class RemindProduct : BaseModel
    {

        [Key]
        public string Id { get; set; } = GuidHelper.GuidTo16String();

        /// <summary>
        /// 商品编号
        /// </summary>
        [StringLength(30)]
        [Required]
        public string ProductCode { get; set; }
        /// <summary>
        /// 中文名
        /// </summary>
        [StringLength(200)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        [StringLength(200)]
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
}
