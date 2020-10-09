using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    /// <summary>
    /// 订单查询返回json对象
    /// </summary>
    public class OrderSearchResponse
    {
        /// <summary>
        /// 订单总数
        /// </summary>
        public string order_total { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public string page_no { get; set; }

        /// <summary>
        /// 页面条数大小
        /// </summary>
        public string page_size { get; set; }

        /// <summary>
        /// 订单列表
        /// </summary>
        public List<OrderInfo> order_info_list { get; set; }

    }
}
