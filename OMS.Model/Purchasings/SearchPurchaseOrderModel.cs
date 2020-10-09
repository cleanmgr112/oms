using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Purchasings
{
    public class SearchPurchaseOrderModel
    {
        public SearchPurchaseOrderModel() {
            this.StartWith = "JR";
        }
        /// <summary>
        /// 仓库
        /// </summary>
        public string WareHouse { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 订单号开头
        /// </summary>
        public string StartWith { get; set; }
        /// <summary>
        /// 导出格式
        /// </summary>
        public string ExportType { get; set; }
        /// <summary>
        /// 关键字
        /// </summary>
        public string Search { get; set; }
        /// <summary>
        /// 是否打印订单详情
        /// </summary>
        public bool IsDetail { get; set; }


    }
}
