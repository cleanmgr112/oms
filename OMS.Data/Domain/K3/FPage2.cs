using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class FPage2
    {
        /// <summary>
        /// 发送分录
        /// </summary>
        public int FOutSourceEntryID { get; set; }
        /// <summary>
        /// 发送内码
        /// </summary>
        public int FOutSourceInterID { get; set; }
        /// <summary>
        /// 发送类型
        /// </summary>
        public int FOutSourceTranType { get; set; }
        /// <summary>
        /// 行号
        /// </summary>
        public int FEntryID2 { get; set; }
        /// <summary>
        /// 是否冲减
        /// </summary>
        public int FATPDeduct { get; set; }
        /// <summary>
        /// 单价2
        /// </summary>
        public decimal FEntrySelfS0168 { get; set; }
        /// <summary>
        /// 二级类目
        /// </summary>
        public string FEntrySelfS0169 { get; set; }
        /// <summary>
        /// 厚度
        /// </summary>
        public string FEntrySelfS0175 { get; set; }
        /// <summary>
        /// 一级类目
        /// </summary>
        public string FEntrySelfS0176 { get; set; }
        /// <summary>
        /// 对应名称
        /// </summary>
        public string FMapName { get; set; }
        /// <summary>
        /// 对应代码
        /// </summary>
        public int FMapNumber { get; set; }
        /// <summary>
        /// 产品代码
        /// </summary>
        public FItemID FItemID { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string FItemName { get; set; }
        /// <summary>
        /// 产品规格
        /// </summary>
        public string FItemModel { get; set; }
        /// <summary>
        /// 辅助属性
        /// </summary>
        public FAuxPropID FAuxPropID { get; set; }
        /// <summary>
        /// 基本单位名称
        /// </summary>
        public string FBaseUnit { get; set; }
        /// <summary>
        /// 基本单位数量
        /// </summary>
        public decimal FQty { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public FUnitID FUnitID { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Fauxqty { get; set; }
        /// <summary>
        /// 辅助单位
        /// </summary>
        public string FSecUnitID { get; set; }
        /// <summary>
        /// 换算率
        /// </summary>
        public decimal FSecCoefficient { get; set; }
        /// <summary>
        /// 辅助数量
        /// </summary>
        public decimal FSecQty { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal Fauxprice { get; set; }
        /// <summary>
        /// 含税单价
        /// </summary>
        public decimal FAuxTaxPrice { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Famount { get; set; }
        /// <summary>
        /// 分支机构代码
        /// </summary>
        public string FBrNo2 { get; set; }
        /// <summary>
        /// 税率（%）
        /// </summary>
        public decimal FCess { get; set; }
        /// <summary>
        /// 折扣率（%）
        /// </summary>
        public decimal FTaxRate { get; set; }
        /// <summary>
        /// 单位折扣额
        /// </summary>
        public decimal FUniDiscount { get; set; }
        /// <summary>
        /// 折扣额
        /// </summary>
        public decimal FTaxAmount { get; set; }
        /// <summary>
        /// 实际含税单价
        /// </summary>
        public decimal FAuxPriceDiscount { get; set; }
        /// <summary>
        /// 销项税额
        /// </summary>
        public decimal FTaxAmt { get; set; }
        /// <summary>
        /// 价税合计
        /// </summary>
        public decimal FAllAmount { get; set; }
        /// <summary>
        /// 运输提前期
        /// </summary>
        public string FTranLeadTime { get; set; }
        /// <summary>
        /// 是否预测内
        /// </summary>
        public int FInForecast { get; set; }
        /// <summary>
        /// 交货日期
        /// </summary>
        public DateTime FDate1 { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Fnote { get; set; }
        /// <summary>
        /// 基本单位关联数量
        /// </summary>
        public decimal FCommitQty { get; set; }
        /// <summary>
        /// 关联数量
        /// </summary>
        public decimal FAuxCommitQty { get; set; }
        /// <summary>
        /// 负数单位关联数量
        /// </summary>
        public decimal FSecCommitQty { get; set; }
        /// <summary>
        /// 辅助单位出库数量
        /// </summary>
        public decimal FSecStockQty { get; set; }
        /// <summary>
        /// 基本单位出库数量
        /// </summary>
        public decimal FStockQty { get; set; }
        /// <summary>
        /// 出库数量
        /// </summary>
        public decimal FAuxStockQty { get; set; }
        /// <summary>
        /// 计划模式
        /// </summary>
        public FPlanMode FPlanMode { get; set; }
        /// <summary>
        /// 计划跟踪号
        /// </summary>
        public string FMTONo { get; set; }
        /// <summary>
        /// 客户BOM
        /// </summary>
        public int FBomInterID { get; set; }
        /// <summary>
        /// 成本对象
        /// </summary>
        public int FCostObjectID { get; set; }
        /// <summary>
        /// 建议交货日期
        /// </summary>
        public DateTime FAdviceConsignDate { get; set; }
        /// <summary>
        /// 锁库标志
        /// </summary>
        public int FLockFlag { get; set; }
        /// <summary>
        /// 源单单号
        /// </summary>
        public string FSourceBillNo { get; set; }
        /// <summary>
        /// BOM类别
        /// </summary>
        public int FBOMCategory { get; set; }
        /// <summary>
        /// 源单类型
        /// </summary>
        public int FSourceTranType { get; set; }
        /// <summary>
        /// 订单BOM内码
        /// </summary>
        public int FOrderBOMInterID { get; set; }
        /// <summary>
        /// 源单内码
        /// </summary>
        public int FSourceInterId { get; set; }
        /// <summary>
        /// 源单分录
        /// </summary>
        public int FSourceEntryID { get; set; }
        /// <summary>
        /// 订单BOM状态
        /// </summary>
        public  int FOrderBOMStatus { get; set; }
        /// <summary>
        /// 合同单号
        /// </summary>
        public string FContractBillNo { get; set; }
        /// <summary>
        /// 合同内码
        /// </summary>
        public int FContractInterID { get; set; }
        /// <summary>
        /// 合同分录
        /// </summary>
        public int FContractEntryID { get; set; }
        /// <summary>
        /// 开票数量
        /// </summary>
        public decimal FAuxQtyInvoice { get; set; }
        /// <summary>
        /// 辅助单位开票数量
        /// </summary>
        public decimal FSecInvoiceQty { get; set; }
        /// <summary>
        /// 基本单位开票数量
        /// </summary>
        public decimal FQtyInvoice { get; set; }
        /// <summary>
        /// 辅助单位组装数量
        /// </summary>
        public decimal FSecCommitInstall { get; set; }
        /// <summary>
        /// 基本单位组装数量
        /// </summary>
        public decimal FCommitInstall { get; set; }
        /// <summary>
        /// 组装数量
        /// </summary>
        public decimal FAuxCommitInstall { get; set; }
        /// <summary>
        /// 辅助属性类别
        /// </summary>
        public string FAuxPropCls { get; set; }
        /// <summary>
        /// 价税合计（本位币）
        /// </summary>
        public decimal FAllStdAmount { get; set; }
        /// <summary>
        /// MRP计算标记
        /// </summary>
        public int FMrpLockFlag { get; set; }
        /// <summary>
        /// MRP是否计算标记
        /// </summary>
        public int FHaveMrp { get; set; }
        /// <summary>
        /// 收款关联金额
        /// </summary>
        public decimal FReceiveAmountFor_Commit { get; set; }
        /// <summary>
        /// 客户订单号
        /// </summary>
        public string FOrderBillNo { get; set; }
        /// <summary>
        /// 订单行号
        /// </summary>
        public int FOrderEntryID { get; set; }
        /// <summary>
        /// 销售单价
        /// </summary>
        public decimal FConsignPrice { get; set; }
        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal FConsignAmount { get; set; }
        /// <summary>
        /// 应发数量
        /// </summary>
        public decimal FAuxQtyMust { get; set; }
        public FIsVMI FIsVMI { get; set; }
        public FEntrySupply FEntrySupply { get; set; }
        public FChkPassItem FChkPassItem { get; set; }
        /// <summary>
        /// 发货仓库
        /// </summary>
        public FDCStockID1 FDCStockID1 { get; set; }
        public FDCSPID FDCSPID { get; set; }

    }
    public class FPlanMode: K3Base { }
    public class FIsVMI : K3Base { }
    public class FEntrySupply : K3Base { }
    public class FChkPassItem : K3Base { }
    public class FAuxPropID : K3Base { }
    public class FDCStockID1 : K3Base { }
    public class FUnitID : K3Base { }
    public class FItemID : K3Base { }
    public class FDCSPID : K3Base { }
}
