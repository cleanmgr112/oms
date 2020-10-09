using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Domain.Purchasings;
using OMS.Data.Domain.Suppliers;
using OMS.Model;
using OMS.Model.Purchasings;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services
{
    public interface IPurchasingService
    {
        #region 供应商
        /// <summary>
        /// 获取全部供应商（包括未启用供应商）
        /// </summary>
        /// <returns></returns>
        IEnumerable<Supplier> GetAllSuppliers();
        Supplier AddSupplier(Supplier supplier);
        Supplier ComfirmSupplierIsExist(string Name);
        Supplier GetSupplierById(int id);
        Supplier UpdataSupplier(Supplier supplier);
        bool DeleteSuppllier(int id);
        void SetSupplierIsvalid(int id);
        Supplier GetSupplierByName(string name);
        #endregion

        #region 采购单
        /// <summary>
        /// 获取全部采购订单（分页）
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="warehouse"></param>
        /// <param name="supplierName"></param>
        /// <returns></returns>
        PageList<Object> GetPurchasingOrdersByPage(int pageIndex, int pageSize, DateTime? startTime , DateTime? endTime, string search = "",string warehouse="",string supplierName="",string startWith="JR");
        /// <summary>
        /// 获取导出采购订单 
        /// </summary>
        /// <returns></returns>
        IEnumerable<ExportPurchaseOrder> GetExportPurchasingOrder(SearchPurchaseOrderModel searchPurchaseOrderModel );
        /// <summary>
        /// 添加采购订单
        /// </summary>
        /// <param name="purchasing"></param>
        /// <returns></returns>
        Purchasing AddPurchasing(Purchasing purchasing);
        /// <summary>
        /// 通过采购订单ID获取采购订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Purchasing GetPurchasingById(int id);
        /// <summary>
        /// 修改采购订单
        /// </summary>
        /// <param name="purchasing"></param>
        /// <returns></returns>
        Purchasing UpdatePurchasingOrder(Purchasing purchasing);
        /// <summary>
        /// 修改采购订单商品信息
        /// </summary>
        /// <param name="purchasingProducts"></param>
        /// <returns></returns>
        PurchasingProducts UpdatePurchasingOrderProduct(PurchasingProducts purchasingProducts);
        /// <summary>
        /// 添加采购订单商品
        /// </summary>
        /// <param name="purchasingProducts"></param>
        /// <returns></returns>
        PurchasingProducts AddPurchasingOrderProduct(PurchasingProducts purchasingProducts);
        /// <summary>
        /// 确认采购订单是否有同一商品
        /// </summary>
        /// <param name="purchasingProducts"></param>
        /// <returns></returns>
        bool ConfirmPurchasingOrderProductIsExist(PurchasingProducts purchasingProducts);
        IEnumerable<PurchasingProductModel> GetAllPurchasingOrderProducts(int purchasingId);
        bool DeletePurchasingOrderProduct(int purchasingOrderId,int productId);
        Dictionary<PurchasingState, string> GetPurchasingOrderStates();
        PurchasingProducts GetPurchasingProductById(int purchasingId, int productId);
        IEnumerable<PurchasingProducts> GetPurchasingProductByPurchasingId(int purchasingId);
        IEnumerable<Object> GetPurchasingOrderLogInfo(int purchasingId);
        bool SetPurchasingOrderInvalid(int purchasingId);
        /// <summary>
        /// 取消上传采购订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string CancelUploadPurchasingOrder(int id);
        /// <summary>
        /// 上传采购退单到WMS
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string UploadPurchasingRefundOrder(int id);
        /// <summary>
        /// 根据单号获取订单
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        Purchasing GetPurchasingBySerialNumber(string serialNumber);
        /// <summary>
        /// 取消上传采购退单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        string CancelUploadPurchasingRefundOrder(int orderId);
        bool SyncWMSFactReceviedProNumsToOMS(int purchasingOrderId,IEnumerable<Model.JsonModel.ProductInfoModel> productInfos);

        /// <summary>
        /// 判断订单的退货状态（采购退单）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        RefundState JudgePurchasingOrderRefundState(int? orderId);
        #endregion
    }
}
