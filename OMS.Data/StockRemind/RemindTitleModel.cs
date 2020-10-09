
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Model.StockRemind
{
    /// <summary>
    /// 提醒标题
    /// </summary>
    public class RemindTitleModel : BaseModel
    {
        /// <summary>
        ///  标题id 
        /// </summary>
        [Key]
        public string TitleId { get; set; } = GuidHelper.GuidTo16String();

        /// <summary>
        /// 提醒标题
        /// </summary>
        [StringLength(50)]
        public string RemindTitle { get; set; }


        /// <summary>
        /// 是否已读
        /// </summary>
        public bool  IsRead { get; set; }

    }
}
