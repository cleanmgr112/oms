using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public enum WriteBackState : Int16
    {
        /// <summary>
        /// 未回写
        /// </summary>
        NoWrite=0,
        /// <summary>
        /// 回写下单成功
        /// </summary>
        WriteSuccess=1,
        /// <summary>
        /// 回写发货成功
        /// </summary>
        WriteShipSuccess=2        
    }
}
