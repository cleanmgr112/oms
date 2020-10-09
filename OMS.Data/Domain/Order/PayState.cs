using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public enum PayState :Int16
    {
        /// <summary>
        /// 失败
        /// </summary>
        Fail=0,
        /// <summary>
        /// 成功
        /// </summary>
        Success=1
    }
}
