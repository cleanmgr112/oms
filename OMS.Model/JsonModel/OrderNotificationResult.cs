using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    /// <summary>
    /// 订单通知结果
    /// </summary>
    public class OrderNotification
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        public string sd_id { get; set; }
        /// <summary>
        /// 商户订单Id
        /// </summary>
        public string order_id { get; set; }

        /// <summary>
        /// 系统方订单号
        /// </summary>
        public string order_sn { get; set; }

        /// <summary>
        /// 订单下单结果，1为成功
        /// </summary>
        public string operation_result { get; set; }
    }

    /// <summary>
    /// 订单下单通知 返回json对象
    /// </summary>
    public class OrderNotificationResponse
    {
        /// <summary>
        /// 状态
        /// </summary>
        public bool state { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 通知结果列表
        /// </summary>
        public List<OrderNotificationResult> notification_result_list { get; set; }
    }

    /// <summary>
    /// 订单通知结果
    /// </summary>
    public class OrderNotificationResult
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        public string sd_id { get; set; }

        /// <summary>
        /// 系统方订单号
        /// </summary>
        public string order_sn { get; set; }

        /// <summary>
        /// 通知结果，1为成功
        /// </summary>
        public string result { get; set; }

        /// <summary>
        /// 通知描述
        /// </summary>
        public string msg { get; set; }
    }

    /// <summary>
    /// 订单物流信息model
    /// </summary>
    public class OrderDeliveryData
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        public string sd_id { get; set; }

        /// <summary>
        /// 系统方订单Id
        /// </summary>
        public string order_sn { get; set; }

        /// <summary>
        /// 快递名称
        /// </summary>
        public string delivery_type { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string delivery_number { get; set; }
    }

    public class ProductStockFeedData
    {
        public int ShopId { get; set; }
        /// <summary>
        /// 总库存
        /// </summary>
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        /// <summary>
        /// 购买的数量
        /// </summary>
        public int ShopQuantity { get; set; }
        /// <summary>
        /// 仓库编码
        /// </summary>
        public string WareHouseCode { get; set; }
        /// <summary>
        /// 当前仓库库存
        /// </summary>
        public int WareHouseStock { get; set; }
    }

        /// <summary>
        /// 更新库存信息
        /// </summary>
    public class ProductStockData
    {
        public ProductStockData() {
            this.stock_detail_list = new List<StockFeedBackDataDetail>();
        }
        /// <summary>
        /// 店铺Id
        /// </summary>
        public string sd_id { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public string goods_sn { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public string stock_num { get; set; }
        /// <summary>
        /// 每个仓库库存明细
        /// </summary>
        public List<StockFeedBackDataDetail> stock_detail_list { get; set; }


    }
    /// <summary>
    /// 仓库库存明细
    /// </summary>
    public class StockFeedBackDataDetail
    {
        public string store_code { get; set; }
        public string store_name { get; set; }
        public string stock_num { get; set; }
    }

    /// <summary>
    /// 商品库存反馈 返回json对象
    /// </summary>
    public class StockFeedbackResponse
    {
        /// <summary>
        /// 返回状态
        /// </summary>
        public bool state { get; set; }
        /// <summary>
        /// 通知结果列表
        /// </summary>
        //public List<StockFeedBackResult> feedback_result_list { get; set; }
        public FeedBackResultList resp_data { get; set; }
        /// <summary>
        /// 返回错误消息
        /// </summary>
        public string message { get; set; }
    }

    public class FeedBackResultList
    {
        public List<StockFeedBackResult> feedback_result_list { get; set; }
    }
    /// <summary>
    /// 商品库存反馈结果
    /// </summary>
    public class StockFeedBackResult
    {
        /// <summary>
        /// 店铺Id
        /// </summary>
        public string sd_id { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public string goods_sn { get; set; }

        /// <summary>
        /// 通知结果，1为成功
        /// </summary>
        public string result { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string msg { get; set; }
    }

    /// <summary>
    /// 订单发货通知 返回json对象
    /// </summary>
    public class OrderDeliveryResponse
    {
        /// <summary>
        /// 状态
        /// </summary>
        public bool state { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 通知结果列表
        /// </summary>
        public DeliveryResultList resp_data { get; set; }
    }
    public class DeliveryResultList
    {
        public List<OrderNotificationResult> delivery_result_list { get; set; }
    }
}
