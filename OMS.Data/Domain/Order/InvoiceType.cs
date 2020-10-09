using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OMS.Data.Domain
{
    public enum InvoiceType : Int16
    {
        /// <summary>
        /// 不需要
        /// </summary>
        [Description("不需要发票")]
        NoNeedInvoice=0,
        /// <summary>
        /// 个人发票
        /// </summary>
        [Description("个人发票")]
        PersonalInvoice = 1,
        /// <summary>
        /// 普通单位发票
        /// </summary>
        [Description("普通单位发票")]
        CompanyInvoice = 2,
        /// <summary>
        /// 专用发票
        /// </summary>
        [Description("专用发票")]
        SpecialInvoice = 3,

    }
}
