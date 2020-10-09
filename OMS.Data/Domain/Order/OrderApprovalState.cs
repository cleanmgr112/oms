using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public enum OrderApprovalState:Int16
    {
        /// <summary>
        /// 失效
        /// </summary>
        Failure=0,
        /// <summary>
        /// 未审核
        /// </summary>
        Unaudited=1,
        /// <summary>
        /// 已审核
        /// </summary>
        Audited=2,
    }
}
