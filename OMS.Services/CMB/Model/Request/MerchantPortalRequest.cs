using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantPortalOpenSDK.Model.Request
{
    public class MerchantPortalRequest
    {
        //方法名
        public string FuncName { get; set; }

        //业务参数
        public object BusinessParams { get; set; }
    }
}
