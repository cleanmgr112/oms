using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    /// <summary>
    /// 订单收货人信息
    /// </summary>
    public class OrderConsigneeInfo
    {
        public string province { get; set; }

        public string city { get; set; }

        /// <summary>
        /// 县区
        /// </summary>
        public string county { get; set; }

        /// <summary>
        /// 详情地址
        /// </summary>
        public string full_address { get; set; }

        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string fullname { get; set; }

        /// <summary>
        /// 收货人电话
        /// </summary>
        public string telephone { get; set; }

        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string mobile { get; set; }

        /// <summary>
        /// 收货人邮箱
        /// </summary>
        public string email { get; set; }
    }
}
