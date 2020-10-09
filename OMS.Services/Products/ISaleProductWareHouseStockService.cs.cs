using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.Products
{
    /// <summary>
    /// 商品仓库库存表的操作
    /// </summary>
    public interface ISaleProductWareHouseStockService
    {
        /// <summary>
        /// 更新商品仓库库存表和商品锁定记录
        /// </summary>
        /// <returns></returns>
        bool UpdateSPWHStockAndSPLockedTrack(OrderProduct orderProduct,string orderSerialNumber,int productId,int wareHouseId, bool isAdmin);
    }
}
