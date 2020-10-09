using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OMS.Data.Domain
{
    public enum OrderState
    {
        /// <summary>
        /// 待转单（B2B下单）
        /// </summary>
        [Description("待转单")]
        ToBeTurned = 0,
        /// <summary>
        /// 待确认（B2B待审核）
        /// </summary>
        [Description("待确认")]
        ToBeConfirmed = 1,
        /// <summary>
        /// 已确认（B2B已审核）
        /// </summary>
        [Description("已审核")]
        Confirmed = 2,
        /// <summary>
        /// 财务确认（B2B,已确认）
        /// </summary>
        [Description("已确认")]
        FinancialConfirmation =3,
        /// <summary>
        /// 被退回（B2B被退回）
        /// </summary>
        [Description("被退回")]
        returned =4,
        /// <summary>
        /// 未付款（B2C订单下单）
        /// </summary>
        [Description("未付款")]
        Unpaid = 5,
        /// <summary>
        /// 已付款（B2C订单）
        /// </summary>
        [Description("已付款")]
        Paid =6,
        /// <summary>
        /// 已确认（B2C订单）
        /// </summary>
        [Description("已确认")]
        B2CConfirmed =7,
        /// <summary>
        /// 未发货（B2C订单）
        /// </summary>
        [Description("未发货")]
        Unshipped =8,
        /// <summary>
        /// 已发货（B2C订单）
        /// </summary>
        [Description("已发货")]
        Delivered =9,
        /// <summary>
        /// 无效（B2C订单）
        /// </summary>
        [Description("无效")]
        Invalid =10,
        /// <summary>
        /// 已上传(以该状态为界限)
        /// </summary>
        [Description("已上传")]
        Uploaded=11,
        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        Finished=12,

        /// <summary>
        /// B2C(订单取消)
        /// </summary>
        [Description("订单取消")]
        Cancel =13,
        [Description("未确认")]
        NoConfirm=14,
        [Description("未上传")]
        NoUpload=15,
        /// <summary>
        /// B2B订单验收
        /// </summary>
        [Description("验收")]
        CheckAccept=16,
        /// <summary>
        /// B2B订单财务记账
        /// </summary>
        [Description("记账")]
        Bookkeeping=17,
        /// <summary>
        /// B2B/B2C退单已入库状态（WMS处理完退单后，返回的状态）
        /// </summary>
        [Description("已入库")]
        StoredWareHouse=18,
        /// <summary>
        /// B2C缺货订单(专门提供给查询B2C缺货的订单)
        /// </summary>
        [Description("缺货")]
        LackStock=19,
    }
}
