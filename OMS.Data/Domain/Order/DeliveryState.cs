using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OMS.Data.Domain
{
    public enum DeliveryState: Int16
    {
        /// <summary>
        /// 未发货
        /// </summary>
        [Description("未发货")]
        Unshipped = 0,
        /// <summary>
        /// 已发货
        /// </summary>
        [Description("已发货")]
        Delivered = 1,
    }
}
