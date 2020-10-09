using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class Dictionary:EntityBase
    {
        public DictionaryType Type { get; set; }
        public string Value { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 字典是否已经同步到WMS
        /// </summary>
        public bool IsSynchronized { get; set; }
    }
}
