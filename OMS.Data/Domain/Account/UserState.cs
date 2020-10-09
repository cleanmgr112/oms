using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public enum UserState:Int16
    {
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled=0,
        /// <summary>
        /// 启用
        /// </summary>
        Enabled=1
    }
}
