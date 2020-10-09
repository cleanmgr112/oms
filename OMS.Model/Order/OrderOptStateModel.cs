using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Order
{
    /// <summary>
    /// 订单操作状态模型
    /// </summary>
    public class OrderOptStateModel
    {
        /// <summary>
        /// 当前可以进行操作的订单状态
        /// </summary>
        public List<OrderState> CanOptOrderStates { get; set; }

        /// <summary>
        /// 操作后订单状态
        /// </summary>
        public OrderState OptedOrderState { get; set; }

        public OptTypeEnum OptTypeEnum { get; set; }

    }

    //操作类型
    public enum OptTypeEnum {
        #region B2B
        /// <summary>
        /// 修改操作订单
        /// </summary>
        B2BModify = 1,
        /// <summary>
        /// 审核操作
        /// </summary>
        B2BCheck = 2,
        /// <summary>
        /// 确认操作
        /// </summary>
        B2BConfirmed = 3,
        /// <summary>
        /// 上传操作
        /// </summary>
        B2BUploaded = 4,
        /// <summary>
        /// 取消上传操作 
        /// </summary>
        B2BCancel = 5,
        /// <summary>
        /// 验收操作
        /// </summary>
        B2BCheckAccept = 6,
        /// <summary>
        /// 确认完成
        /// </summary>
        B2BFinished = 7,
        /// <summary>
        /// 退单
        /// </summary>
        B2BRefund = 8,
        /// <summary>
        /// 设置订单无效
        /// </summary>
        B2BInvalid = 9,
        #endregion

        #region B2C
        /// <summary>
        /// 保存订单，修改发票，修改仓库，修改快递，修改支付方式，添加/修改商品，锁定订单
        /// </summary>
        B2CSave = 10,
        /// <summary>
        /// 确认订单
        /// </summary>
        B2CConfirmed = 11,
        /// <summary>
        /// 反确认订单
        /// </summary>
        B2CReConfirmed = 12,
        /// <summary>
        /// 上传订单
        /// </summary>
        B2CUploaded = 13,
        /// <summary>
        /// 取消上传订单
        /// </summary>
        B2CCancel = 14,
        /// <summary>
        /// 复制订单
        /// </summary>
        B2CCopy = 15,
        /// <summary>
        /// 拆分订单
        /// </summary>
        B2CSplit = 16,
        /// <summary>
        /// 设为无效
        /// </summary>
        B2CInvalid = 17,
        #endregion

        #region B2C退单操作
        /// <summary>
        /// 修改订单信息，修改快递方式，修改支付方式，添加/修改/删除商品
        /// </summary>
        B2CRModify = 18,
        /// <summary>
        /// 确认退单
        /// </summary>
        B2CRConfirmed = 19,
        /// <summary>
        /// 上传B2C退单
        /// </summary>
        B2CRUploaded = 20,
        /// <summary>
        /// 取消上传B2C退单
        /// </summary>
        B2CRCancel = 21,
        /// <summary>
        /// 验收B2C退单
        /// </summary>
        B2CRCheckAccept = 22,
        /// <summary>
        /// 完成B2C退单
        /// </summary>
        B2CRFinished = 23,
        /// <summary>
        /// 反确认
        /// </summary>
        B2CRReConfirmed =24,
        #endregion

        #region 跨境购退单
        /// <summary>
        /// 跨境购退单修改保存
        /// </summary>
        KJRFModify = 25,
        /// <summary>
        /// 跨境购退单确认
        /// </summary>
        KJRFConfirm = 26,
        /// <summary>
        /// 跨境购退单反确认
        /// </summary>
        KJRFUnConfirm = 27,
        /// <summary>
        /// 跨境购退单上传
        /// </summary>
        KJRFUpload = 28,
        /// <summary>
        /// 跨境购退单取消上传
        /// </summary>
        KJRFCancelUpload = 29,
        /// <summary>
        /// 跨境购退单验收
        /// </summary>
        KJRFCheckAccept = 30,
        /// <summary>
        /// 跨境购退单完成
        /// </summary>
        KJRFFinished = 31
        #endregion

    }
}
