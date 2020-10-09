using Newtonsoft.Json.Linq;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.K3Wise
{
    public interface IK3WiseService
    {
        /// <summary>
        /// 创建出库单
        /// </summary>
        /// <param name="OrderSerialNumber"></param>
        /// <returns></returns>
        string CreateBill(int orderId);


        #region 中间服务器
        /// <summary>
        /// 获取K3Wise token
        /// </summary>
        /// <returns></returns>
        string GetK3Token();
        /// <summary>
        /// 从OMS获取需要传递到K3服务器的订单（主动）
        /// </summary>
        /// <returns></returns>
        string GetOrdersFromOMS();
        string GetTheLatestBillNo();
        /// <summary>
        /// 中间服务器传递订单到K3服务器
        /// </summary>
        /// <param name="data"></param>
        string AddOrdersToK3();
        object AddOrdersToK3(dynamic data);
        /// <summary>
        /// 中间服务器接收来自OMS传递过来的订单并传递到K3服务器（被动）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string AcceptDataFromOMS(object data);
        /// <summary>
        /// 从K3服务器获取客户信息
        /// </summary>
        /// <returns></returns>
        dynamic GetCustomersInfoFromK3();
        /// <summary>
        /// 发送订单状态到OMS服务器
        /// </summary>
        /// <param name="k3BillNoRelated"></param>
        /// <returns></returns>
        bool SendBillNoRelatedToOMS(K3BillNoRelated k3BillNoRelated);
        #endregion


        #region OMS服务器
        /// <summary>
        /// 获取所有已出库的订单（未过滤，全部订单）
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetAllOutOfStockOrders();
        /// <summary>
        /// 接收中间服务器传送过来的订单状态并更新到OMS数据库中
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool AddK3BillNoRelated(K3BillNoRelated data);
        string CheckAndSendOrderToK3(int orderId);
        /// <summary>
        /// 更新客户信息（初始化表格）
        /// </summary>
        /// <returns></returns>
        string UpdateCustomersInfo();
        IEnumerable<Order> GetAllOutOfStockOrders(string searchStr);
        bool CheckOrderIsSentSuccessed(string orderSerialNumber);
        IEnumerable<K3BillNoRelated> GetAllBillNoRelated(string searchStr);
        List<K3BaseData> GetK3BaseData();
        bool UpdateK3BaseData(K3BaseData data);
        #endregion
    }
}
