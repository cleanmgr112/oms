using Newtonsoft.Json.Linq;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Model;
using OMS.Model.B2B;
using OMS.Model.DataStatistics;
using OMS.Model.JsonModel;
using OMS.Model.Order;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Services.Order1
{
    public interface IOrderService
    {
        #region B2B订单
        /// <summary>
        /// 添加审核流程
        /// </summary>
        /// <param name="approvalProcess"></param>
        /// <returns></returns>
        bool InsertApprovalProcess(ApprovalProcess approvalProcess);
        /// <summary>
        /// 删除审核流程
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteApprovalProcess(int id);
        /// <summary>
        /// 更新审核人员排序
        /// </summary>
        /// <param name="apdId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        bool UpdateAPDetailSort(int apdId, string userIds);
        /// <summary>
        /// 获取审核流程
        /// </summary>
        /// <returns></returns>
        List<ApprovalProcess> GetAllApprovalProcessList();
        /// <summary>
        /// 通过流程名判断是否存在相同流程名
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        ApprovalProcess ConfirmApprovalProcessIsExistByName(string Name);
        /// <summary>
        /// 添加B2B订单
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns>新生成订单Id</returns>
        Order AddB2BOrder(OrderModel orderModel);
        /// <summary>
        /// 更新B2B订单
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        bool UpdateB2BOrder(OrderModel orderModel);
        /// <summary>
        /// 添加订单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <returns></returns>
        bool AddOrderProduct(OrderProduct orderProduct);
        /// <summary>
        /// 更新订单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <returns></returns>
        bool UpdateOrderProduct(OrderProduct orderProduct);
        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Order GetOrderById(int orderId);
        /// <summary>
        /// 获取订单（全状态，包括isvalid=false）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Order GetOrderByIdWithAllState(int orderId);
        /// <summary>
        /// 订单商品分页
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        PageList<Object> GetOrderProductByOrderId(int orderId, int pageIndex, int pageSize, string search="");
        /// <summary>
        /// 获取B2B退单列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="warehouseId"></param>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        PageList<Object> GetB2BRefundOrderList(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime,int? warehouseId ,string searchStr = "");
        /// <summary>
        /// 获取该订单下所有商品
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        IEnumerable<OrderProduct> GetOrderProductsByOrderId(int orderId);
        /// <summary>
        /// 获取订单详情页Model
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        List<OrderDetailProductsModel> GetOrderProductsModelByOrderId(int orderId,int wareHouseId);
        /// <summary>
        /// 获取可以退货的订单商品信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        List<CanRefundOrderProduct> GetCanRefundOrderProductsByOrderId(int? orderId);
        /// <summary>
        /// 订单分页
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PageList<OrderModel> GetOrderListByType(OrderModel orderMode, int pageIndex, int pageSize);
        /// <summary>
        /// 根据orderProductId获取单个订单商品信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Object GetOrderProductById(int id);
        /// <summary>
        /// 根据Id获取单个订单商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        OrderProduct GetOrderProductByIdOne(int id);
        /// <summary>
        /// 删除订单商品根据orderproductid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteOrderProductById(int id);
        /// <summary>
        /// 审核订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="state"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool ApprovalOrder(int orderId, bool state,out string msg);
        /// <summary>
        /// 财务确认订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool ConfirmOrder(int orderId, out string msg);
        /// <summary>
        /// 财务记账
        /// </summary>
        /// <param name="orderModel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool BookKeeping(OrderModel orderModel, out string msg);
        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool DeleteOrder(int orderId, out string msg);
        int CheckOrderProductCount(int orderId, int productId);
        /// <summary>
        /// 根据订单获取客户信息，默认发票信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Customers GetDefaultInvoiceInfo(int orderId);
        /// <summary>
        /// 插入订单发票
        /// </summary>
        /// <param name="invoiceInfo"></param>
        /// <returns></returns>
        bool SubmitOrderInvoiceInfo(InvoiceInfoModel invoiceInfoModel, int orderId);
        /// <summary>
        /// 物流
        /// </summary>
        /// <returns></returns>
        List<Delivery> GetAllDeliveryList();
        /// <summary>
        /// 提交审核
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool SubmitApproval(int orderId, out string msg);
        /// <summary>
        /// 更新发票信息
        /// </summary>
        /// <param name="invoiceInfo"></param>
        void UpdateOrderInvoiceInfo(InvoiceInfo invoiceInfo);

        /// <summary>
        /// 修改订单（通用）
        /// </summary>
        /// <param name="order"></param>
        Order UpdateOrder(Order order);
        /// <summary>
        /// 获取订单支付信息
        /// </summary>
        /// <returns></returns>
        OrderPayPrice GetOrderPay(int orderId);
        /// <summary>
        /// 获取订单日志
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        IEnumerable<OrderLog> GetOrderRecord(int orderId);
        /// <summary>
        /// 获取订单发票信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        InvoiceInfo GetOrderInvoiceRecord(int orderId);
        /// <summary>
        /// 添加订单支付信息
        /// </summary>
        /// <param name="orderPayPrice"></param>
        void AddOrderPayPrice(OrderPayPrice orderPayPrice);
        /// <summary>
        /// 判断订单商品是否存在
        /// </summary>
        /// <returns></returns>
        OrderProduct ConfirmOrderProductIsExist(int orderId,int saleProductId);
        /// <summary>
        /// 复制B2B订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        string CopyB2BOrder(int orderId);
        /// <summary>
        /// 接口调用获取B2B订单及详情
        /// </summary>
        /// <returns></returns>
        IEnumerable<InterfaceOrderModel> GetAllB2BOrder(int orderType = (int)OrderType.B2B, string orderSerialNumber = "", int orderState = 0, DateTime? startTime = null, DateTime? endTime = null);
        #endregion

        #region B2C订单
        /// <summary>
        /// 获取全部B2C订单
        /// </summary>
        /// <returns></returns>
        IEnumerable<Order> GetAllB2COrders();
        /// <summary>
        /// 获取全部B2C订单(分页)
        /// </summary>
        /// <returns></returns>
        PageList<Object> GetAllB2COrdersByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, OrderState? orderState, int? wareHouseId, string search = "");

        /// <summary>
        /// 获取B2C订单列表
        /// </summary>
        /// <returns></returns>
        PageList<B2COrderViewModel> GetAllB2COrdersTableByPage(SearchOrderContext searchOrderContext);
        /// <summary>
        /// 获取全部退单
        /// </summary>
        /// <param name="pageIndex">页数</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="shopId">店铺ID</param>
        /// <param name="payState">订单状态</param>
        /// <param name="search">关键字</param>
        /// <param name="orderType">订单类型</param>
        /// <returns></returns>
        PageList<Object> GetAllRefundOrdersByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, int? orderState, string search = "", OrderType orderType = OrderType.B2C_TH);
        /// <summary>
        /// 添加B2C订单
        /// </summary>
        Order CreatedB2COrder(Order order);
        /// <summary>
        /// 通过SerialNumber查找Order订单记录
        /// </summary>
        /// <param name="serialNumber">OMS单号</param>
        /// <returns></returns>
        Order GetOrderBySerialNumber(string serialNumber);
        /// <summary>
        /// 判断订单的退货状态（B2B和B2C退单）
        /// </summary>
        RefundState JudgeOrderRefundState(int? orderId);

        /// <summary>
        /// 通过PSerialNumber查找Order订单记录
        /// </summary>
        /// <param name="pserialNumber"></param>
        /// <returns></returns>
        Order GetOrderByPSerialNumber(string pserialNumber);

        /// <summary>
        /// 通过ID获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Order GetOrderByIdB2C(int? orderId);
        /// <summary>
        /// 添加B2C订单商品
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <returns></returns>
        bool AddOrderProductB2C(OrderProduct orderProduct);
        /// <summary>
        /// 判断PSerialNumber是否存在
        /// </summary>
        /// <param name="pserialNumber"></param>
        /// <returns></returns>
        bool ConfirmPSerialNumberIsExist(string pserialNumber,bool isInvalid,bool isAll);
        /// <summary>
        /// 判断OrgionSerialNumber是否存在
        /// </summary>
        /// <param name="orgionSerialNumber"></param>
        /// <returns></returns>
        bool ConfirmOrgionSerialNumberIsExist(string orgionSerialNumber);
        /// <summary>
        /// 根据OriginalSerialNumber获取所有的退单
        /// </summary>
        /// <param name="originalSerialNumber"></param>
        /// <returns></returns>
        List<Order> GetRefundOrdersByOriginalSerialNumber(string originalSerialNumber);
        /// <summary>
        /// B2C订单拆分添加商品
        /// </summary>
        /// <param name="oldOrderId"></param>
        /// <param name="newOrderId"></param>
        /// <param name="saleproductsInfo"></param>
        /// <param name="newOrderNum"></param>
        /// <param name="saleProductsCount"></param>
        /// <returns></returns>
        int AddProductsToNewOrder(int oldOrderId,int newOrderId,Dictionary<int,int> saleproductsInfo,int newOrderNum,out decimal newOrderSumPayPrice, out decimal newOrderSumPrice);
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="oldOrderId"></param>
        /// <param name="newOrderId"></param>
        /// <param name="saleproductsInfo"></param>
        /// <param name="newOrderNum"></param>
        /// <param name="saleProductsCount"></param>
        /// <returns></returns>
        int AddProductsToNewOrder(int oldOrderId,int newOrderId,out int saleProductsCount, out decimal newOrderSumPrice);
        /// <summary>
        /// 新增部分退货商品信息
        /// </summary>
        /// <param name="oldOrderId"></param>
        /// <param name="newOrderId"></param>
        /// <param name="refundProductInfoData"></param>
        /// <returns></returns>
        bool AddPartRefundProducts(int? oldOrderId, int newOrderId,JArray refundProductInfoData);
        /// <summary>
        /// 获取可以合并订单
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        IEnumerable<Order> GetCanCombineOrders(OrderType orderType, int? shopId,string searchStr="");
        /// <summary>
        /// 分页获取合并订单列表
        /// </summary>
        /// <param name="searchOrderContext"></param>
        /// <returns></returns>
        PageList<B2COrderViewModel> GetCanPageListCombineOrders(SearchOrderContext searchOrderContext);
        Order CombineOrder(int[] list,string newSerialNumber, out List<string> oldSerialNumber);
        /// <summary>
        /// 检查合并订单是否满足要求
        /// </summary>
        /// <param name="list"></param>
        /// <param name="checkmsg"></param>
        /// <returns></returns>
        bool CheckCombineOrderInfo(int[] list,out string checkmsg);
        /// <summary>
        /// 为订单匹配最佳发货仓库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string MatchWareHouseForOrder(int id,out int firstWareHouseId);
        /// <summary>
        /// 匹配最佳仓库
        /// </summary>
        /// <returns></returns>
        string MatchFirstWareHouse(string addressDetail);

        /// <summary>
        /// 获取最佳仓库id
        /// </summary>
        /// <param name="addressDetail"></param>
        /// <returns></returns>
        int MatchFirstWareHouseId(string addressDetail);
        /// <summary>
        /// 匹配仓库（考虑库存）
        /// </summary>
        /// <returns></returns>
        int MatchWarehouseId(List<OrderProductInfo> orderProductInfos, string addressDetail);

        /// <summary>
        /// 检查订单选中的发货仓库库存是否足够
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string CheckWareHouseStock(int id);

        /// <summary>
        /// 检查德邦物流是否能够正常配送到收货地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string CheckDeBangIsDelivery(int id);
        /// <summary>
        /// 复制订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        string CopyB2COrder(int orderId);
        string ChangeProEqPrice(int orderProId, int saleProId, int proNum);
        #endregion

        /// <summary>
        /// 获取全部订单状态
        /// </summary>
        /// <returns></returns>
        Dictionary<OrderState, string> GetOrderStateStr();
        Object GetEveryMonthB2COrders(int? month);

        Task<string> UploadOrder(int id);
        /// <summary>
        /// 上传退单到WMS
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string UploadRefundOrder(int id);
        /// <summary>
        /// 取消上传退单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string UnUploadRefundOrder(int id);
        /// <summary>
        /// 上传B2B退单到WMS
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        string UploadB2BRefundOrder(int id);
        /// <summary>
        /// 取消上传订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string CancelUploadOrder(int id);
        /// <summary>
        /// 根据条件获取订单
        /// </summary>
        /// <returns></returns>
        IEnumerable<ExportOrderModel> GetAllOrdersByCommand(DateTime? startTime, DateTime? endTime,DateTime? payStartTime, DateTime? payEndTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId,OrderState? orderState, string search,bool isOrderDetail,bool isRefundOrder);
        /// <summary>
        /// 根据条件获取B2B订单
        /// </summary>
        /// <param name="isOrderDetail"></param>
        /// <returns></returns>
        IEnumerable<ExportOrderModel> GetAllB2BOrdersByCommand(SearchB2BOrderModel searchB2BOrderModel, bool isOrderDetail);
        /// <summary>
        /// 分页获取零售销货分析
        /// </summary>
        /// <returns></returns>
        PageList<RetailSalesModel> GetRetailSalesModelsByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "");
        /// <summary>
        /// 按要求获取所有零售销货分析数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<RetailSalesModel> GetAllExportDataByCommand(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "");
        /// <summary>
        /// 分页获取订单发货统计分析
        /// </summary>
        /// <returns></returns>
        PageList<OrderDeliveryModel> GetOrderDeliveryModelsByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? payType, int? deliveryId, int dateType, out int orderCount, out int productCount, out decimal avgSumPrice, out decimal deliverySumPrice, string search = "");
        /// <summary>
        /// 获取所有订单发货统计数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<OrderDeliveryModel> GetAllExportOrderDeliveryModels(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? payType, int? deliveryId, int dateType, string search = "");
        /// <summary>
        /// 分页获取商品发货统计分析数据
        /// </summary>
        /// <returns></returns>
        PageList<GoodDeliveryModel> GetOrderDeliverModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? deliveryId, out int orderCount, out int productCount, out decimal avgSumPrice, string search = "");
        /// <summary>
        /// 获取所有商品发货统计数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<GoodDeliveryModel> GetAllExportGoodDeliveryModels(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int? deliveryId, string search = "");
        /// <summary>
        /// 分页获取商店退货分析数据
        /// </summary>
        /// <returns></returns>
        PageList<ShopRefundOrderModel> GetShopRefundOrderModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, out int productCount, out decimal price, string search = "");
        /// <summary>
        /// 获取所有商店退货分析数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<ShopRefundOrderModel> GetAllExportShopRefundOrderModels(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "");
        /// <summary>
        /// 分页获取商店退货按照店铺分析数据
        /// </summary>
        /// <returns></returns>
        PageList<ShopRefundOrderByShopModel> GetOrderDeliverModelByShopByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, out int productCount, out decimal price, string search = "");
        /// <summary>
        /// 获取所有商店退货按照店铺分析数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<ShopRefundOrderByShopModel> GetAllExportShopRefundOrderModelsByShop(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "");

        /// <summary>
        /// 分页获取商店退货按照商品分析数据
        /// </summary>
        /// <returns></returns>
        PageList<ShopRefundOrderByProductModel> GetOrderDeliverModelByProductByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, out int productCount, out decimal price, string search = "");

        /// <summary>
        /// 获取所有商店退货按照商品分析数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<ShopRefundOrderByProductModel> GetAllExportShopRefundOrderModelsByProduct(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, string search = "");
        /// <summary>
        /// 分页获取零售销售统计数据
        /// </summary>
        /// <returns></returns>
        PageList<RetailSalesStatisticsModel> GetRetailSalesStatisticsModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType,out int productCount,out decimal avgSumPrice, string search = "");
        /// <summary>
        /// 获取所有零售销售统计数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<RetailSalesStatisticsModel> GetAllExportRetailSalesStatisticsModel(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "");
        /// <summary>
        /// 根据店铺分类分页获取零售销售统计数据
        /// </summary>
        /// <returns></returns>
        PageList<RetailSalesStatisticsByShopModel> GetRetailSalesStatisticsByShopModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, out int productCount, out decimal avgSumPrice, string search = "");
        /// <summary>
        /// 根据店铺获取所有零售销售统计数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<RetailSalesStatisticsByShopModel> GetAllExportRetailSalesStatisticsByShopModel(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "");

        /// <summary>
        /// 根据商品分类分页获取零售销售统计数据
        /// </summary>
        /// <returns></returns>
        PageList<RetailSalesStatisticsByProductModel> GetRetailSalesStatisticsByProductModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, out int productCount, out decimal avgSumPrice, string search = "");

        /// <summary>
        /// 根据商品获取所有零售销售统计数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<RetailSalesStatisticsByProductModel> GetAllExportRetailSalesStatisticsByProductModel(DateTime? startTime, DateTime? endTime, int? shopId, int? wareHouseId, int orderType, string search = "");

        /// <summary>
        /// 分页获取B2B订单销售数据分析
        /// </summary>
        /// <returns></returns>
        PageList<B2BSaleDataAnalysisModel> GetB2BSaleDataAnalysisModelByPage(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? customerId, int? wareHouseId,int? customerTypeId, int? orderType, int? checkType, int? bookKeepType,out int count,out decimal sumPrice, string search = "");
        /// <summary>
        /// 获取所有B2B销货数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<B2BSaleDataAnalysisModel> GetAllExportB2BSaleDataAnalysisModel(DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? customerId, int? wareHouseId, int? customerTypeId, int? orderType, int? checkType, int? bookKeepType, string search = "");
        /// <summary>
        /// 分页获取客户的所有历史订单
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        PageList<CustomerHistoryOrderModel> GetHistoryOrderListByPage(int pageIndex, int pageSize, string userName);
        Object GetProductStockInEveryWareHouse(int productId);
        /// <summary>
        /// 获取订单状态，并返回json格式数据
        /// </summary>
        /// <param name="orderSerialNumber"></param>
        /// <returns></returns>
        object GetOrderState(string orderSerialNumber);
        object GetAllKJRefundOrderInfo(int pageIndex, int pageSize, DateTime? startTime, DateTime? endTime, DateTime? deliverStartTime, DateTime? deliverEndTime, int? shopId, int? orderState, string search = "", OrderType orderType = OrderType.B2C_KJRF);
        #region 检查订单是否缺货
        /// <summary>
        /// 检查是否缺货
        /// </summary>
        /// <returns></returns>
        bool IsOrderOutofStock(Order order);

        /// <summary>
        /// 通过OrderId检查是否缺货
        /// </summary>
        /// <returns></returns>
        bool IsOrderOutofStockByOrderId(int orderId);
        /// <summary>
        /// 判断订单商品是否缺货
        /// </summary>
        /// <param name="orderProduct"></param>
        /// <param name="wareHouseId"></param>
        /// <returns></returns>
        bool IsOrderProductStock(OrderProduct orderProduct,int wareHouseId);
        /// <summary>
        /// 判断订单商品的锁定库存与仓库库存否可用（上传订单时，主要是防止仓库对不同仓库的商品进行了移位操作导致商品数量差异）
        /// </summary>
        /// <returns></returns>
        bool IsCheckWarehouseStockCanUse(Order order);
        #endregion


        #region 接口调用部分函数
        /// <summary>
        /// （接口调用）创建订单
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        string ApiCreateOrder(List<InterfaceOrderModel> orders, string preWord);
        /// <summary>
        /// （接口调用）获取订单信息
        /// </summary>
        /// <param name="pSerialNumber"></param>
        /// <returns></returns>
        IEnumerable<InterfaceOrderModel> ApiGetOrderInfo(string pSerialNumber);
        /// <summary>
        /// （接口调用）线下店退单接口
        /// </summary>
        /// <param name="pSerialNumber"></param>
        /// <returns></returns>
        string ApiOffLineCancelOrder(string pSerialNumber);
        /// <summary>
        /// 线下单订单状态回传
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        string ApiReturnOffLineOrderState(List<Order> orders);
        #endregion

    }
}
