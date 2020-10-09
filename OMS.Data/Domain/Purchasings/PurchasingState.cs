using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OMS.Data.Domain.Purchasings
{
    public enum PurchasingState
    {
        [Description("初始")]
        InitialStatus=0,
        [Description("审核")]
        Verify=1,
        [Description("确认")]
        Confirmed = 2,
        [Description("已上传")]
        Uploaded = 3,
        [Description("完成")]
        Finished=4,
        [Description("无效")]
        Invalid=5,
        [Description("已入库")]
        StoredWareHouse=6,
        [Description("验收")]
        CheckAccept=7,
        [Description("已出库")]//采购退单使用
        OutWareHouse = 8,
    }
}
