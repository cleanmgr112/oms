using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Purchasings
{
    public class ExportPurchaseOrder
    {
        /// <summary>
        /// 采购单号
        /// </summary>
        public string PurchasingNumber { get; set; }
        /// <summary>
        /// 采购计划单号
        /// </summary>
        public string PurchasingOrderNumber { get; set; }
        /// <summary>
        /// 原单号
        /// </summary>
        public string OrgionSerialNumber { get; set; }
        /// <summary>
        /// 下单日期
        /// </summary>
        public string CreatedTime { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        public string  CheckTime { get; set; }
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string StateStr { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Mark { get; set; }
        /// <summary>
        /// 采购商品编码
        /// </summary>
        public string PurchasingProductCode { get; set; }
        /// <summary>
        /// 采购商品名称
        /// </summary>
        public string PurchasingProductName { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal SumPrice { get; set; }
    }
}
