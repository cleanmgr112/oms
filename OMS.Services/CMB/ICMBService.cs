using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.CMB
{
    public interface ICMBService
    {
        /// <summary>
        /// 获取全部未发货订单
        /// </summary>
        /// <returns></returns>
        string GetAllOrderList(DateTime? startTime = null, DateTime? endTime = null);
        /// <summary>
        /// 插入订单到OMS
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        string InsertOrderToOMS(string response);
        /// <summary>
        /// 获取招行订单详情
        /// </summary>
        /// <param name="cmbOrderNo"></param>
        /// <returns></returns>
        string GetOrderDetail(string cmbOrderNo);
        /// <summary>
        /// 上传快递信息到招行系统（请在ReturnDeliverResultToCMB中调用）
        /// </summary>
        /// <param name="orderItemNo">cmb订单号</param>
        /// <param name="orderNo">cmb子订单号</param>
        /// <param name="deliveryType">快递名称</param>
        /// <param name="deliverName">快递中文名</param>
        /// <param name="deliverNo">快递单号</param>
        /// <returns></returns>
        string UpLoadDeliverInfo(string orderItemNo, string orderNo, string deliveryType, string deliverName, string deliverNo);
        /// <summary>
        /// 返回订单状态到招行
        /// </summary>
        /// <param name="resultOrder"></param>
        /// <returns></returns>
        string ReturnDeliverResultToCMB(Order resultOrder);
        /// <summary>
        /// 获取订单接口日志
        /// </summary>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        IEnumerable<CMBOrderLog> GetCMBOrderLog(string searchStr);
    }
}
