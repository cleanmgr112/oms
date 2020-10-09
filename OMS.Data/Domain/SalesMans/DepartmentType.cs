using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OMS.Data.Domain.SalesMans
{
    public enum DepartmentType
    {
        [Description("高管")]
        TopManager = 0,
        [Description("视觉设计部")]
        VisualDesign=1,
        [Description("运营中心")]
        OperationCenter=2,
        [Description("国际采购部")]
        InternationalPurchasing=3,
        [Description("渠道部")]
        Channel=4,
        [Description("物流中心")]
        LogisticsCentre=5,
        [Description("市场推广部")]
        Marketing=6,
        [Description("信息编辑部")]
        InformationEditing=7,
        [Description("财务管理部")]
        FinancialManagement=8,
        [Description("行政人事部")]
        AdministrativePersonnel=9,
        [Description("资产管理部")]
        AssetManagement=10,
        [Description("红酒世界机场店")]
        AirportStore=11,
        [Description("北京分公司")]
        BeiJingBranch=12,
        [Description("上海分公司")]
        ShangHaiBranch = 13,
        [Description("长春分公司")]
        ChangChunBranch = 14,
        [Description("沈阳分公司")]
        ShenYangBranch = 15,
        [Description("国际业务部")]
        InternationalBusiness=16,
        [Description("技术开发部")]
        Technology = 17,
        [Description("上海新天地店")]
        ShangHaiDian = 18,
    }
}
