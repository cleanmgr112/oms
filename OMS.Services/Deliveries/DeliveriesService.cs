using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Interface;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.Deliveries
{
   public class DeliveriesService: ServiceBase,IDeliveriesService
    {
        #region ctor
        public DeliveriesService(IDbAccessor omsAccessor, IWorkContext workContext)
            : base(omsAccessor, workContext)
        {
        }
        #endregion
        /// <summary>
        /// 获取全部快递信息
        /// </summary>
        /// <returns></returns>
       public IEnumerable<Delivery> GetAllDeliveries()
        {
            var result =_omsAccessor.Get<Delivery>();
            return result;
        }
        /// <summary>
        /// 添加快递信息
        /// </summary>
        /// <param name="delivery"></param>
        /// <returns></returns>
        public Delivery AddDelivery(Delivery delivery)
        {
            delivery.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert<Delivery>(delivery);
            _omsAccessor.SaveChanges();
            return delivery;
        }
        /// <summary>
        /// 更新快递信息
        /// </summary>
        /// <param name="delivery"></param>
        /// <returns></returns>
        public Delivery UpdateDelivery(Delivery delivery)
        {
            delivery.ModifiedBy = _workContext.CurrentUser.Id;
            delivery.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<Delivery>(delivery);
            _omsAccessor.SaveChanges();
            return delivery;
        }
        /// <summary>
        /// 删除快递信息
        /// </summary>
        /// <param name="deliveryId"></param>
        /// <returns></returns>
        public bool DelDelivery(int deliveryId)
        {
            Delivery delivery = _omsAccessor.Get<Delivery>().Where(s => s.Isvalid && s.Id == deliveryId).FirstOrDefault();
            if (delivery != null)
            {
                _omsAccessor.Delete<Delivery>(delivery);
                _omsAccessor.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 通过ID获取快递信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Delivery GetDeliveryById(int id)
        {
            var result = _omsAccessor.Get<Delivery>().Where(s => s.Isvalid && s.Id == id).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// 确实是否有相同code的快递信息存在
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Delivery ConfirmDeliveryIsExist(string code)
        {
            var result = _omsAccessor.Get<Delivery>().Where(s => s.Isvalid && s.Code == code).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// 通过商城编码获取快递信息
        /// </summary>
        /// <param name="shopcode"></param>
        /// <returns></returns>
        public Delivery GetDeliveryByShopCode(string shopcode) {
            var result = _omsAccessor.Get<Delivery>().Where(s => s.Isvalid && s.ShopCode == shopcode.Trim()).FirstOrDefault();
            return result;
        }
        /// <summary>
        /// 获取快递方式id
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int GetDeliveryByValue(string value)
        {
            var delivery = new Delivery();
            if (value == "顺丰")
                delivery = _omsAccessor.Get<Delivery>(r => r.Isvalid && r.Name == "顺丰特惠").FirstOrDefault();
            else
                delivery = _omsAccessor.Get<Delivery>(r => r.Isvalid && r.Name.Contains(value)).FirstOrDefault();

            return delivery == null ? 0 : delivery.Id;
        }
        /// <summary>
        /// 通过Code获取快递方式ID
        /// </summary>
        /// <param name="deliveryCode"></param>
        /// <returns></returns>
        public int GetDeliveryByCode(string deliveryCode) {
            var delivery= _omsAccessor.Get<Delivery>().Where(x => x.Isvalid && x.Code.Contains(deliveryCode)).FirstOrDefault();
            return delivery == null ? 0 : delivery.Id;
        }
    }
}
