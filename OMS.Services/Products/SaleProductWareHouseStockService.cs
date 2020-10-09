using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Interface;
using OMS.Services.Log;
using OMS.Services.Order1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services.Products
{
    /// <summary>
    /// 商品仓库库存表的操作
    /// </summary>
    public class SaleProductWareHouseStockService : ServiceBase, ISaleProductWareHouseStockService
    {

        #region ctor
        private ILogger<ProductService> _logger;
        private ILogService _logService;
        private IOrderService _orderService;
        public SaleProductWareHouseStockService(IDbAccessor omsAccessor, IWorkContext workContext, ILogger<ProductService> logger, ILogService logService, IConfiguration configuration,IOrderService orderService)
            : base(omsAccessor, workContext, configuration)
        {
            _logger = logger;
            _logService = logService;
            _orderService = orderService;
        }
        #endregion

        /// <summary>
        /// 更新商品仓库库存表和商品锁定记录
        /// </summary>
        /// <returns></returns>
        public bool UpdateSPWHStockAndSPLockedTrack(OrderProduct orderProduct,string orderSerialNumber, int productId, int wareHouseId,bool isAdmin) {
            if (orderProduct==null) {
                return false;
            }
            //查找锁定的记录
            var saleProductLockedTrack = _omsAccessor.Get<SaleProductLockedTrack>().Where(x=>x.Isvalid && x.OrderProductId==orderProduct.Id && x.WareHouseId == wareHouseId).FirstOrDefault();
            //找到属于该商品的仓库信息
            var SPWHStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid && x.WareHouseId == wareHouseId && x.SaleProductId == orderProduct.SaleProductId).FirstOrDefault();
            if (saleProductLockedTrack!=null)//已经存在对应的锁定记录，
            {
                if (SPWHStock!=null) {
                    //新修改商品的不变
                    if (saleProductLockedTrack.LockNumber==orderProduct.Quantity) {
                        return true;
                    }
                    //修改商品数量小于锁定数量
                    if (saleProductLockedTrack.LockNumber>orderProduct.Quantity) {
                        //将之前锁定的还回去
                        SPWHStock.LockStock = SPWHStock.LockStock - (saleProductLockedTrack.LockNumber - orderProduct.Quantity);
                        if (orderProduct.Quantity == 0)
                        {
                            saleProductLockedTrack.Isvalid = false;
                            saleProductLockedTrack.LockNumber = 0;
                        }
                        else {
                            saleProductLockedTrack.LockNumber = orderProduct.Quantity;
                        }
                    }

                    //修改商品数量大于锁定数量
                    if (saleProductLockedTrack.LockNumber<orderProduct.Quantity) {
                        if (SPWHStock.Stock - SPWHStock.LockStock >= orderProduct.Quantity - saleProductLockedTrack.LockNumber)
                        {
                            SPWHStock.LockStock = SPWHStock.LockStock + orderProduct.Quantity - saleProductLockedTrack.LockNumber;
                            saleProductLockedTrack.LockNumber = orderProduct.Quantity;
                        }
                        else {
                            saleProductLockedTrack.LockNumber = SPWHStock.Stock - SPWHStock.LockStock+saleProductLockedTrack.LockNumber;
                            SPWHStock.LockStock = SPWHStock.Stock;
                        }
                    }
                    saleProductLockedTrack.ModifiedBy = isAdmin ? 0 : _workContext.CurrentUser.Id;
                    saleProductLockedTrack.ModifiedTime = DateTime.Now;
                    SPWHStock.ModifiedBy = isAdmin ? 0 : _workContext.CurrentUser.Id;
                    saleProductLockedTrack.ModifiedTime = DateTime.Now;
                    _omsAccessor.Update(saleProductLockedTrack);
                    _omsAccessor.Update(SPWHStock);
                    _omsAccessor.SaveChanges();                   
                }
                return true;
            }
            else
            {
                var saleProductLockedTrackNew = new SaleProductLockedTrack()
                {
                    OrderId = orderProduct.OrderId,
                    OrderSerialNumber = orderSerialNumber,
                    OrderProductId = orderProduct.Id,
                    SaleProductId = orderProduct.SaleProductId,
                    ProductId = productId,
                    CreatedBy = isAdmin ? 0 : _workContext.CurrentUser.Id
                };

                if (SPWHStock != null)
                {
                    //充足的时候直接锁定加上即可
                    if (SPWHStock.Stock - SPWHStock.LockStock >= orderProduct.Quantity)
                    {
                        SPWHStock.LockStock = SPWHStock.LockStock + orderProduct.Quantity;
                        saleProductLockedTrackNew.LockNumber = orderProduct.Quantity;
                    }
                    //不充足的时候直接等于当前库存
                    else
                    {
                        saleProductLockedTrackNew.LockNumber = SPWHStock.Stock - SPWHStock.LockStock;
                        SPWHStock.LockStock = SPWHStock.Stock;
                    }
                }

                SPWHStock.ModifiedBy = isAdmin ? 0 : _workContext.CurrentUser.Id;
                SPWHStock.ModifiedTime = DateTime.Now;
                _omsAccessor.Insert(saleProductLockedTrack);
                _omsAccessor.Update(SPWHStock);
                _omsAccessor.SaveChanges();
                return true;
            }
        }
    }
}
