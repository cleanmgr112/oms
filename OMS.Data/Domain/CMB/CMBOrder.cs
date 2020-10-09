using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.CMB
{
    public class CMBOrder
    {
        /// <summary>
        /// 会话编号
        /// 请求老系统数据时传来的sessionId
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 店铺号
        /// </summary>
        public string storeNo { get; set; }
        /// <summary>
        /// 订单编号【店铺订单号】 预售订单时为预售尾款的店铺订单号
        /// </summary>
        public string orderNo { get; set; }
        /// <summary>
        /// 客户订单号【店铺订单号】 预售订单时为预售尾款的店铺订单号
        /// </summary>
        public string CustomerOrderNo { get; set; }
        /// <summary>
        /// 预售尾款单对应的预售定金单的编号普通订单返回"0"
        /// </summary>
        public string DepositOrderNo { get; set; }
        /// <summary>
        /// 下单时间 格式: yyyyMMddHHmmss
        /// </summary>
        public string OrderTime { get; set; }
        /// <summary>
        /// 1：普通订单 2：预售订单
        /// </summary>
        public string OrderType { get; set; }
        /// <summary>
        /// 订单购买渠道 【掌上生活：C43、手机银行：SJN】
        /// </summary>
        public string FromLinkSource { get; set; }
        /// <summary>
        /// 客户端展示的支付成功时间 格式:yyyyMMddHHmmss
        /// </summary>
        public string PayTime { get; set; }
        /// <summary>
        /// 订单金额【店铺维度】示例：1240.00元
        /// </summary>
        public string SoAmt { get; set; }
        /// <summary>
        /// 订单总优惠金额【店铺维度】
        /// promotionAmt= soAmt+ expressFeeAmtinvoiceAmt
        /// 示例：240.00元
        /// 默认返回"0.00"
        /// </summary>
        public string PromotionAmt { get; set; }
        /// <summary>
        /// 发票金额 客户实际支付金额 包含运费【店铺维度】
        ///示例：1005.00元
        /// </summary>
        public string InvoiceAmt { get; set; }
        /// <summary>
        /// 客户实际支付的积分总额 【店铺维度】
        /// </summary>
        public string PayPoint { get; set; }
        /// <summary>
        /// 运费 示例：5.00元
        /// 无数据时返回"0.00"
        /// </summary>
        public string ExpressFeeAmt { get; set; }
        /// <summary>
        /// 订单状态：
        /// 0: 未支付；
        /// 1: 已支付；
        /// 目前商家获取到的订单均为已支付状态。
        /// </summary>
        public string OrderStatus { get; set; }
        /// <summary>
        /// List，商品明细，参照ShoppingItem
        /// </summary>
        public ShoppingItem ShoppingList { get; set; }
        /// <summary>
        /// 发票抬头，如果发票种类为增值税时，表示公司名称默认返回“”
        /// </summary>
        public string InvoiceTitle { get; set; }
        /// <summary>
        /// 发票纳税人识别号(公司抬头会有参数)默认返回“”
        /// </summary>
        public string InvoiceTaxNo { get; set; }
        /// <summary>
        /// 开票明细: 0:商品明细 1:办公用品 2:电脑配件 3:耗材 9:无需发票  默认返回“9”
        /// </summary>
        public string InvoiceDetailType { get; set; }
        /// <summary>
        /// 发票种类: 0:纸质 1:电子 2:增值税 9:无需发票 默认返回“9”
        /// </summary>
        public string InvoiceType { get; set; }
        /// <summary>
        /// 发票接收邮箱 默认返回“”
        /// </summary>
        public string InvoiceReceiveEmail { get; set; }
        /// <summary>
        /// 增值税对象，参照VATInvoiceInfo
        /// </summary>
        public VATInvoiceInfo VatInvoiceInfo { get; set; }
        /// <summary>
        /// 订单备注
        /// </summary>
        public string OrderMemo { get; set; }
        /// <summary>
        /// 收货人
        /// </summary>
        public string ReceiveContact { get; set; }
        /// <summary>
        /// 收货人手机号码
        /// </summary>
        public string ReceiveCellPhone { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        public string ReceiveAddress { get; set; }
        /// <summary>
        /// 收货省份
        /// </summary>
        public string ReceiveProvince { get; set; }
        /// <summary>
        /// 收货城市
        /// </summary>
        public string ReceiveCity { get; set; }
        /// <summary>
        /// 收货地区（县、区）
        /// </summary>
        public string ReceiveZone { get; set; }
        /// <summary>
        /// 收货街道
        /// </summary>
        public string ReceiveStreet { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        public string ReceiveZip { get; set; }
        /// <summary>
        /// 身份证号 海淘订单收货人需要提供身份证号
        /// </summary>
        public string IdentityCardNo { get; set; }
    }
    public class ShoppingItem
    {
        /// <summary>
        /// 子订单号【支持按子订单发货】
        /// </summary>
        public string OrderItemNo { get; set; }
        /// <summary>
        /// 商品编号
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商家内部商品编号
        /// </summary>
        public string SellerSku { get; set; }
        /// <summary>
        /// 3级分类编号
        /// </summary>
        public string C3SysNo { get; set; }
        /// <summary>
        /// 品牌编号
        /// </summary>
        public string ManufacturerSysNo { get; set; }
        /// <summary>
        /// 商家商品条码 默认返回""
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 最早发货时间 为预售订单预留 格式：yyyyMMddHHmmss
        /// </summary>
        public string EarliestShippingTime { get; set; }
        /// <summary>
        /// 最晚发货时间 为预售订单预留 格式：yyyyMMddHHmmss
        /// </summary>
        public string LatestShippingTime { get; set; }
        /// <summary>
        /// 发货状态 0:未发货 1：已发货
        /// </summary>
        public string ShipStatus { get; set; }
        /// <summary>
        /// 商品原价：详情页展示的购买时的单价 示例：1240.00元
        /// </summary>
        public string OriginalPrice { get; set; }
        /// <summary>
        /// 购买时的积分
        /// </summary>
        public string OriginalPoint { get; set; }
        /// <summary>
        /// 预售定金金额
        /// </summary>
        public string DepositAmt { get; set; }
        /// <summary>
        /// 商品费率 示例：0.01
        /// </summary>
        public string CommisionRate { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public string Quantity { get; set; }
        /// <summary>
        /// 分期的期数
        /// </summary>
        public string Installment { get; set; }
        /// <summary>
        /// 促销信息List< PromotionInfo >
        /// </summary>
        public PromotionInfo PromotionList { get; set; }
        /// <summary>
        /// 商品总折扣金额 =(平台补贴+商户补贴金额)
        /// promotionList 里面所有的促销金额之和
        /// 平台补贴= promotionList 里所有成本方为
        /// 1【招行承担】促销金额之和
        /// 商户补贴金额 = promotionList 里所有成本方
        /// 为0【商家承担】促销金额之和
        /// </summary>
        public string CouponDiscount { get; set; }
        /// <summary>
        /// 商品购买总价格=（商品原价*购买数量-商品总折扣金额）
        /// </summary>
        public string SalePrice { get; set; }
        /// <summary>
        /// 佣金 = (商品原价 * 购买数量-商户补贴金额) * 佣金率
        /// </summary>
        public string Commision { get; set; }
        /// <summary>
        /// 支付信息List 参照payItem
        /// </summary>
        public PayItem PayList { get; set; }
        /// <summary>
        /// 赠品对应的主商品子订单号
        /// </summary>
        public string MainProductOrderItemNo { get; set; }
    }
    public class PromotionInfo
    {
        /// <summary>
        /// 类型 1:抢购 2:限购 3:赠品 4:预售 5:试用 6:拼团 7:限时多期数
        /// 8:预售全款 9:预售定金 10:预售尾款 11:商品券 12:满减/满折 13:支付立减
        /// </summary>
        public string PromotionType { get; set; }
        /// <summary>
        /// 成本方 0.商家承担 1.招行承担
        /// </summary>
        public string PromotionCost { get; set; }
        /// <summary>
        /// 促销活动编号
        /// </summary>
        public string PromotionNo { get; set; }
        /// <summary>
        /// 促销活动名称
        /// </summary>
        public string PromotionName { get; set; }
        /// <summary>
        /// 促销活动金额 示例：240.00元
        /// </summary>
        public string PromotionAmt { get; set; }
        /// <summary>
        /// 促销活动积分
        /// </summary>
        public string PromotionPoint { get; set; }
    }
    public class PayItem
    {
        /// <summary>
        /// 支付渠道：1.普通
        /// </summary>
        public string PayType { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        public string PayAmt { get; set; }
        /// <summary>
        /// 支付积分
        /// </summary>
        public string PayPoint { get; set; }
        /// <summary>
        /// 支付时间 格式：yyyyMMddHHmmss
        /// </summary>
        public string PayTime { get; set; }
        /// <summary>
        /// 交易流水
        /// </summary>
        public string TransactionNo { get; set; }
    }
    public class VATInvoiceInfo
    {
        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string TaxpayerIdent { get; set; }
        /// <summary>
        /// 注册电话
        /// </summary>
        public string RegisteredPhone { get; set; }
        /// <summary>
        /// 注册地址
        /// </summary>
        public string RegisteredAddress { get; set; }
        /// <summary>
        /// 开户行账号
        /// </summary>
        public string BankAccount { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        public string DepositBank { get; set; }
    }
}
