using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class FPage1
    {
        public FPage1()
        {
            Fdate = DateTime.Parse(DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month).ToString());
            FSettleDate = Fdate;
            FCheckDate = Fdate;
            FSelTranType = 0;
            FSendStatus = 0;
            FEnterpriseID = 0;
            //FROB = 1;
            FStatus = 1;
            FCancellation = 0;
        }
        /// <summary>
        /// 作废标志（必填）
        /// </summary>
        public int FCancellation { get; set; }
        /// <summary>
        /// 事务类型（21）
        /// </summary>
        public int FClassTypeID { get; set; }
        /// <summary>
        /// 发送企业
        /// </summary>
        public int FEnterpriseID { get; set; }
        /// <summary>
        /// 发货仓库
        /// </summary>
        public int FHeadSelfS0158 { get; set; }
        /// <summary>
        /// 自定义项59
        /// </summary>
        public string FHeadSelfS0159 { get; set; }
        /// <summary>
        /// 自定义项60
        /// </summary>
        public string FHeadSelfS0160 { get; set; }
        /// <summary>
        /// 自定义项61
        /// </summary>
        public string FHeadSelfS0161 { get; set; }
        /// <summary>
        /// 发送标志
        /// </summary>
        public int FSendStatus { get; set; }
        /// <summary>
        /// 日期（必填）
        /// </summary>
        public DateTime Fdate { get; set; }
        /// <summary>
        /// 购货单位
        /// </summary>
        public int FCustID { get; set; }
        /// <summary>
        /// 结算日期
        /// </summary>
        public DateTime FSettleDate { get; set; }
        /// <summary>
        /// 引入标志
        /// </summary>
        public decimal FImport { get; set; }
        /// <summary>
        /// 销售范围
        /// </summary>
        public int FAreaPS { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public int FSettleID { get; set; }
        /// <summary>
        /// 订货机构
        /// </summary>
        public FRelateBrID FRelateBrID { get; set; }
        /// <summary>
        /// 分销订单号
        /// </summary>
        public string FPOOrdBillNo { get; set; }
        /// <summary>
        /// 销售方式（必填）
        /// </summary>
        public FSaleStyle FSaleStyle { get; set; }
        /// <summary>
        /// 运输提前期
        /// </summary>
        public int FTransitAheadTime { get; set; }
        /// <summary>
        /// 制单机构    
        /// </summary>
        public FBrID FBrID { get; set; }
        /// <summary>
        /// 事务类型
        /// </summary>
        public int FTranType { get; set; }
        /// <summary>
        /// 汇率
        /// </summary>
        public decimal FExchangeRate { get; set; }
        /// <summary>
        /// 交货方式
        /// </summary>
        public int FFetchStyle { get; set; }
        /// <summary>
        /// 交货地点
        /// </summary>
        public string FFetchAdd { get; set; }
        /// <summary>
        /// 摘要
        /// </summary>
        public string FExplanation { get; set; }
        /// <summary>
        /// 币别
        /// </summary>
        public int FCurrencyID { get; set; }
        /// <summary>
        /// 选单号
        /// </summary>
        public string FSelBillNo { get; set; }
        /// <summary>
        /// 编号（必填）
        /// </summary>
        public string FBillNo { get; set; }
        /// <summary>
        /// 源单类型
        /// </summary>
        public int FSelTranType { get; set; }
        /// <summary>
        /// 审核流程状态
        /// </summary>
        public string FMultiCheckStatus { get; set; }
        /// <summary>
        /// 审核标志（必填）
        /// </summary>
        public int FStatus { get; set; }
        /// <summary>
        /// 分支机构代码
        /// </summary>
        public string FBrNo { get; set; }
        /// <summary>
        /// 客户地点
        /// </summary>
        public int FCustAddress { get; set; }
        /// <summary>
        /// 汇率类型
        /// </summary>
        public int FExchangeRateType { get; set; }
        /// <summary>
        /// 计划类别
        /// </summary>
        public int FPlanCategory { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public FCheckerID FCheckerID { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        public DateTime FCheckDate { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public FDeptID FDeptID { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public FEmpID FEmpID { get; set; }
        /// <summary>
        /// 主管
        /// </summary>
        public int FMangerID { get; set; }
        /// <summary>
        /// 制单
        /// </summary>
        public FBillerID FBillerID { get; set; }
        /// <summary>
        /// 保税监管类型
        /// </summary>
        public string FManageType { get; set; }
        /// <summary>
        /// 确认人
        /// </summary>
        public string FValidaterName { get; set; }
        /// <summary>
        /// 系统设置
        /// </summary>
        public int FSysStatus { get; set; }
        /// <summary>
        /// 收货方
        /// </summary>
        public FConsignee FConsignee { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string FVersionNo { get; set; }
        /// <summary>
        /// 变更日期
        /// </summary>
        public DateTime FChangeDate { get; set; }
        /// <summary>
        /// 变更人
        /// </summary>
        public int FChangeUser { get; set; }
        /// <summary>
        /// 变更原因
        /// </summary>
        public string FChangeCauses { get; set; }
        /// <summary>
        /// 变更标志
        /// </summary>
        public string FChangeMark { get; set; }
        /// <summary>
        /// 销售渠道
        /// </summary>
        public FHeadSelfB0163 FHeadSelfB0163 { get; set; }
        /// <summary>
        /// 保管（必填）
        /// </summary>
        public FSManagerID FSManagerID { get; set; }
        /// <summary>
        /// 发货（必填）
        /// </summary>
        public FFManagerID FFManagerID { get; set; }
        public FDCStockID FDCStockID { get; set; }
        /// <summary>
        /// 购货单位
        /// </summary>
        public FSupplyID FSupplyID { get; set; }
        public FBillReviewer FBillReviewer { get; set; }
        /// <summary>
        /// 对账确认人
        /// </summary>
        public FConfirmer FConfirmer { get; set; }
        public FPayCondition FPayCondition { get; set; }
        public FManagerID FManagerID { get; set; }
        /// <summary>
        /// 销售业务类型（必填）
        /// </summary>
        public FMarketingStyle FMarketingStyle { get; set; }
        /// <summary>
        /// 红蓝单标志（必填） 出库：1 退货：-1
        /// </summary>
        public int FROB { get; set; }
    }
    public class FHeadSelfB0163 : K3Base { }
    public class FSManagerID : K3Base { }
    public class FFManagerID : K3Base { }
    public class FDCStockID : K3Base { }
    public class FSupplyID : K3Base { }
    public class FSaleStyle : K3Base { }
    public class FMarketingStyle : K3Base { }
    public class FBrID : K3Base { }
    public class FBillerID : K3Base { }
    public class FRelateBrID : K3Base { }
    public class FBillReviewer : K3Base { }
    public class FConfirmer : K3Base { }
    public class FPayCondition : K3Base { }
    public class FDeptID : K3Base { }
    public class FManagerID : K3Base { }
    public class FEmpID : K3Base { }
    public class FCheckerID : K3Base { }
    public class FConsignee : K3Base { }
}
