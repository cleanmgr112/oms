using System;
using System.ComponentModel.DataAnnotations;

namespace OMS.Model.StockRemind
{
    /// <summary>
    /// 基础实体
    /// </summary>
    public class BaseModel
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 编辑时间
        /// </summary>
       
        public DateTime? EditorTime { get; set; }

        /// <summary>
        /// 编辑者
        /// </summary>
        [StringLength(10)]
        public string  Edtior { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [Required]
        public bool Isdelete { get; set; }
    }
}

public static class GuidHelper
{
    /// <summary>
    /// 随机数对象，所有随机数都用此对象，避免占用新的内存
    /// </summary>
    public static Random random=new Random();

    /// <summary>
    /// 生成16 位 guid
    /// </summary>
    /// <returns></returns>
    public static string GuidTo16String()
    {
        long i = 1;
        foreach (byte b in Guid.NewGuid().ToByteArray())
            i *= ((int)b + 1);
        return string.Format("{0:x}", i - DateTime.Now.Ticks);
    }

    /// <summary>  
    /// 19位的唯一数字序列  
    /// </summary>  
    /// <returns></returns>  
    public static long GuidToLongID()
    {
        byte[] buffer = Guid.NewGuid().ToByteArray();
        return BitConverter.ToInt64(buffer, 0);
    }

    /// <summary>
    /// 生成编号随机数+时间戳
    /// </summary>
    /// <returns></returns>
    public static string RandomCode(string prefix)
    {
        var str = (DateTime.UtcNow.Ticks / 100 / 1000 / 1000).ToString();
        str = str.Substring(str.Length - 5, 4);
        return $"{prefix}{random.Next(1000, 9999)}{str}";
    }
   
}


