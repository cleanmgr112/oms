using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    [Flags]
    public enum OrderType 
    {
        /// <summary>
        /// b2b订单
        /// </summary>
        B2B=1,
        /// <summary>
        /// B2C现货
        /// </summary>
        B2C_XH=2,
        /// <summary>
        /// B2C跨境
        /// </summary>
        B2C_KJ = 3,
        /// <summary>
        /// B2C期酒
        /// </summary>
        B2C_QJ = 4,
        /// <summary>
        /// B2C合作商
        /// </summary>
        B2C_HZS = 5,
        /// <summary>
        /// B2C退货
        /// </summary>
        B2C_TH = 6,
        /// <summary>
        /// B2B退货
        /// </summary>
        B2B_TH = 7,
        /// <summary>
        /// 跨境购退单
        /// </summary>
        B2C_KJRF=8,

        B2C= B2C_XH| B2C_KJ| B2C_QJ | B2C_HZS | B2C_TH| B2C_KJRF
    }
    /// <summary>
    /// 附加订单类型
    /// </summary>
    public enum AppendType {
        /// <summary>
        /// 合并订单
        /// </summary>
        Combine =1,
        /// <summary>
        /// 拆分订单
        /// </summary>
        Split =2
    }
    /// <summary>
    /// 订单退单状态
    /// </summary>
    public enum RefundState {
        /// <summary>
        /// 没有退
        /// </summary>
        No =0,
        /// <summary>
        /// 全部退
        /// </summary>
        All = 1,
        /// <summary>
        /// 部分退
        /// </summary>
        Part =2,
    }
}
