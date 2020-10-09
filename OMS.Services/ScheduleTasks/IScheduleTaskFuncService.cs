using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Services.ScheduleTasks
{
    public interface IScheduleTaskFuncService
    {
        
        void SendMessage();

        void ReceviceMessage();

        void Test();
        /// <summary>
        /// 同步字典数据到WMS 
        /// </summary>  
        /// <returns></returns>
        Task<bool> OmsSyncDictionaries();

        /// <summary>
        /// 同步商品数据到WMS
        /// </summary>
        /// <returns></returns>
        Task<bool>  OmsSyncProducts();
        /// <summary>
        ///  上传采购订单到WMS
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool OmsPurchasingOrders(List<int> orderId);
        /// <summary>
        /// 上传B2B B2C订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<bool> OmsOrders(List<int> orderId);
        /// <summary>
        /// 同步供应商数据到WMS
        /// </summary>
        /// <returns></returns>
        Task<bool> OmsSyncSuppliers();
        /// <summary>
        /// OMS抓取商城下单信息同步到订单表
        /// </summary>
        /// <returns></returns>
        Task<bool> OmsSyncWineWorldOrder();

        /// <summary>
        /// 同步快递方式到WMS
        /// </summary>
        /// <returns></returns>
        Task<bool> OmsSyncDeliverys();

        /// <summary>
        /// 同步客户到WMS
        /// </summary>
        /// <returns></returns>
        Task<bool> OmsSyncCustomers();

        /// <summary>
        /// 同步所有商品的库存到商城
        /// </summary>
        void OMSToShopAllProductsStock();
        /// <summary>
        /// 同步商品Rfid到酒柜
        /// </summary>
        Task SyncRfidToWineCabinet();
        /// <summary>
        /// 同步招行商城订单到OMS系统
        /// </summary>
        void SyncCMBOrderToOMS();
    }
}
