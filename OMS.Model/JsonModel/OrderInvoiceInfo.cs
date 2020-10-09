using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    /// <summary>
    /// 订单发票信息
    /// </summary>
    public class OrderInvoiceInfo
    {
        /// <summary>
        /// 发票抬头类型 ， 个人或企业，3、增值税
        /// </summary>
        public string invoice_title_type { get; set; }

        /// <summary>
        /// 发票内容描述
        /// </summary>
        public string invoice_title_info { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string organization_name { get; set; }

        /// <summary>
        /// 纳税人识别码
        /// </summary>
        public string taxpayer_id { get; set; }

        /// <summary>
        /// 注册地址
        /// </summary>
        public string register_address { get; set; }

        /// <summary>
        /// 注册电话
        /// </summary>
        public string register_tel { get; set; }

        /// <summary>
        /// 开户银行
        /// </summary>
        public string bank_of_deposit { get; set; }

        /// <summary>
        /// 银行账号
        /// </summary>
        public string bank_account { get; set; }

        public string InvoiceEmail { get; set; }

    }
}
