using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OMS.Data.Domain
{
    public enum InvoiceModeEnum : short
    {
        /// <summary>
        /// 电子发票
        /// </summary>
        [Description("电子发票")]
        Electronic = 0,
        /// <summary>
        /// 纸质发票
        /// </summary>
        [Description("纸质发票")]
        Paper = 1
    }
}
