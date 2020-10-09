using System;
using System.Collections.Generic;
using OMS.Data.Domain;
using System.Text;

namespace OMS.Services.Deliveries
{
    public interface IDeliveriesService
    {
        /// <summary>
        /// 获取全部快递信息
        /// </summary>
        /// <returns></returns>
        IEnumerable<Delivery> GetAllDeliveries();
        /// <summary>
        /// 添加快递信息
        /// </summary>
        /// <param name="delivery"></param>
        /// <returns></returns>
        Delivery AddDelivery(Delivery delivery);
        /// <summary>
        /// 更新快递信息
        /// </summary>
        /// <param name="delivery"></param>
        /// <returns></returns>
        Delivery UpdateDelivery(Delivery delivery);
        /// <summary>
        /// 删除快递信息
        /// </summary>
        /// <param name="deliveryId"></param>
        /// <returns></returns>
        bool DelDelivery(int deliveryId);
        /// <summary>
        /// 通过ID获取快递信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Delivery GetDeliveryById(int id);
        /// <summary>
        /// 确实是否有相同code的快递信息存在
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Delivery ConfirmDeliveryIsExist(string code);
        /// <summary>
        /// 通过商城编码获取快递信息
        /// </summary>
        /// <param name="shopcode"></param>
        /// <returns></returns>
        Delivery GetDeliveryByShopCode(string shopcode);
        /// <summary>
        /// 获取快递方式id
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        int GetDeliveryByValue(string value);
        /// <summary>
        /// 通过Code获取快递方式ID
        /// </summary>
        /// <param name="deliveryCode"></param>
        /// <returns></returns>
        int GetDeliveryByCode(string deliveryCode);
    }
}
