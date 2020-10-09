
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OMS.Model.StockRemind
{
    /// <summary>
    /// 用户消息表
    /// </summary>
    public class UserMessageModel :BaseModel
    {   
        [Key]
        public string MessageId { get; set; } = GuidHelper.GuidTo16String();

        /// <summary>
        /// 消息
        /// </summary>
        [StringLength(100)]
        public string Message { get; set; }

        /// <summary>
        /// userId json
        /// </summary>
        [StringLength(100)]
        public string User { get; set; }= JsonConvert.SerializeObject(new List<string>());

    }
}
