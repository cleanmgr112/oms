using System;
using System.Collections.Generic;
using System.Text;
using OMS.Data.Domain;
using OMS.Model.B2B;
using Microsoft.AspNetCore.Mvc.Rendering;
using OMS.Data.Domain.SalesMans;

namespace OMS.Model
{
    public class OrderModel : ModelBase
    {
        public OrderModel()
        {
            InvoiceTypes = new List<SelectListItem>() {
                new SelectListItem(){ Value="0",Text="不用发票" },
                new SelectListItem(){ Value="1",Text="普通个人发票" },
                new SelectListItem(){ Value="2",Text="普通单位发票" },
                new SelectListItem(){ Value="3",Text="专用发票" }
                //紧做前端展示需要，实际数据库存的值减1（-1）
            };
            this.InvoiceMode = InvoiceModeEnum.Electronic;
        }
        public string SerialNumber { get; set; }
        public OrderType Type { get; set; }
        public int ShopId { get; set; }
        public string PSerialNumber { get; set; }
        public string OrgionSerialNumber { get; set; }
        public OrderState State { get; set; }
        /// <summary>
        /// 订单状态回写（0未回写，1回写下单成功，2回写发货成功）
        /// </summary>
        public WriteBackState WriteBackState { get; set; }
        public int PayType { get; set; }
        public int PayMentType { get; set; }
        public PayState PayState { get; set; }
        public DateTime? TransDate { get; set; }
        public bool IsLocked { get; set; }
        public int LockMan { get; set; }
        public bool LockStock { get; set; }
        public bool? IsCopied { get; set; }
        public decimal SumPrice { get; set; }
        public decimal PayPrice { get; set; }
        public int DeliveryTypeId { get; set; }
        public string DeliveryNumber { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryDateStr { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string AddressDetail { get; set; }
        public int DistrictId { get; set; }
        /// <summary>
        /// 客户备注
        /// </summary>
        public string CustomerMark { get; set; }
        public InvoiceType InvoiceType { get; set; }
        public InvoiceModeEnum InvoiceMode { get; set; }
        /// <summary>
        /// 管理员备注
        /// </summary>
        public string AdminMark { get; set; }
        public string ToWarehouseMessage { get; set; }
        public int WarehouseId { get; set; }
        public int CustomerId { get; set; }
        public int PriceTypeId { get; set; }
        public int ApprovalProcessId { get; set; }
        public List<Customers> Customers { get; set; }
        public List<WareHouse> WareHouses { get; set; }
        public List<Dictionary> PriceType { get; set; }
        public List<ApprovalProcessModel> ApprovalProcessModel { get; set; }
        public List<Delivery> Deliverys { get; set; }
        public List<Dictionary> PayTypes { get; set; }
        public List<Dictionary> PayMentTypes { get; set; }
        public List<SelectListItem> InvoiceTypes { get; set; }
        public List<SalesMan> SalesMans { get; set; }
        public InvoiceInfoModel InvoiceInfoModel { get; set; }
        /// <summary>
        /// 订单创建人
        /// </summary>
        public int? CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        /// <summary>
        /// 当前审核人
        /// </summary>
        public int Reviewer { get; set; }
        /// <summary>
        /// 客户（公司）
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 客户类型
        /// </summary>
        public string CompanyType { get; set; }

        /// <summary>
        /// 订单的总商品数
        /// </summary>
        public int OrderProductCount { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string StateStr { get; set; }

        /// <summary>
        /// 付款或者退款
        /// </summary>
        public bool IsPayOrRefund { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        public int? SalesManId { get; set; }
        /// <summary>
        /// 财务备注
        /// </summary>
        public string FinanceMark { get; set; }
        /// <summary>
        /// 业务员名称
        /// </summary>
        public string SalesManName { get; set; }
        /// <summary>
        /// 验货
        /// </summary>
        public bool IsCheck { get; set; }
        /// <summary>
        /// 记账
        /// </summary>
        public bool IsBookKeeping { get; set; }
        /// <summary>
        /// 回款状态
        /// </summary>
        public string IsBookKeepingStr { get; set; }
        /// <summary>
        /// 是否缺货
        /// </summary>
        public bool IsLackStock { get; set; }
        /// <summary>
        /// 是否需要发票
        /// </summary>
        public bool IsInvoice { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayDate { get; set; }
        /// <summary>
        /// 沟通备注
        /// </summary>
        public string CommunicateMark { get; set; }

        /// <summary>
        /// 最小金额
        /// </summary>
        public int? MinPrice { get; set; }
        /// <summary>
        /// 最大金额
        /// </summary>
        public int? MaxPrice { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public int? OrderState { get; set; }
        /// <summary>
        /// 仓库Id
        /// </summary>
        public int? WareHouseId { get; set; }
        /// <summary>
        /// 客户类型Id
        /// </summary>
        public int? CustomerTypeId { get; set; }
        /// <summary>
        /// 付款状态
        /// </summary>
        public int? BookKeepType { get; set; }
        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? DeliverStartTime { get; set; }
        public DateTime? DeliverEndTime { get; set; }
        /// <summary>
        /// 发票信息
        /// </summary>
        public InvoiceInfo InvoiceInfo { get; set; }
        /// <summary>
        /// 是否有退单
        /// </summary>
        public bool IsRefundOrder { get; set; }
        /// <summary>
        /// 退单号
        /// </summary>
        public string RefundNum { get; set; }
        /// <summary>
        /// 下单客户的商城账号
        /// </summary>
        public string UserName { get; set; }

    }
}