using OMS.Model.JsonModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.Order1
{
    /// <summary>
    /// 商城订单同步
    /// </summary>
    public interface IOrderSyncService
    {
        //同步订单表Order
        void OrderSync(IList<OrderInfo> orderInfoList,string siId, out List<OrderNotification> orderNotifications);

        /// <summary>
        /// OMS调用订单辅助系统接口参数
        /// </summary>
        /// <returns></returns>
        OrderAssistParamsModel OrderAssistOmsApi(string method, string app_key, string v, string sd_id, string order_state, string page_no, string page_size, string data = null);
    }
}
