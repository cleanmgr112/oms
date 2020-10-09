using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public enum K3KeyValueEm
    {
        /// <summary>
        /// 销售出库类型
        /// </summary>
        FMarketingStyle = 1,
        /// <summary>
        /// 销售方式
        /// </summary>
        FSaleStyle=2,
        /// <summary>
        /// 发货人
        /// </summary>
        FFManagerID =3,
        /// <summary>
        /// 业务员（多个）
        /// </summary>
        FEmpID = 4,
        /// <summary>
        /// 保管
        /// </summary>
        FSManagerID=5,
        /// <summary>
        /// 制单
        /// </summary>
        FBillerID=6,
        /// <summary>
        /// 审核
        /// </summary>
        FCheckerID=7,
        /// <summary>
        /// 发货仓库
        /// </summary>
        FDCStockID1 =8,
        /// <summary>
        /// 计划模式
        /// </summary>
        FPlanMode=9,
        /// <summary>
        /// 是否良品
        /// </summary>
        FChkPassItem=10,
        /// <summary>
        /// 单位
        /// </summary>
        FUnitID=11,
    }
}
