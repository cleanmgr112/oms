using OMS.Core;
using OMS.Data.Domain;
using OMS.Model;
using OMS.Model.JsonModel;
using OMS.Model.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Services.Products
{
    public interface IProductService
    {

        #region 商品
        Product GetProductById(int id);
        /// <summary>
        /// 通过商品编码获取商品
        /// </summary>
        /// <returns></returns>
        Product GetProductByCode(string code);
        /// <summary>
        /// 通过商品名查找商品
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Product GetProductByName(string name);
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="product"></param>
        void InsertProduct(Product product);
        /// <summary>
        /// 修改商品
        /// </summary>
        /// <param name="product"></param>
        void UpdateProduct(Product product);
        /// <summary>
        /// 修改商品图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="id"></param>
        void UpdateProductImage(string url, int id);
        /// <summary>
        /// 检测商品信息是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool ConfirmProductExist(string name);
        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DelProduct(int id);
        /// <summary>
        /// 获取全部商品
        /// </summary>
        /// <returns></returns>
        IQueryable GetAllProducts();
        PageList<Product> GetProductList(int pageSize, int pageIndex, int TypeId = 0, string searchStr = "");
        /// <summary>
        /// 分页查询商品
        /// </summary>
        /// <param name="searchProductContext"></param>
        /// <returns></returns>
        PageList<ProductViewModel> GetProductViewModels(SearchProductContext searchProductContext);
        /// <summary>
        /// 更新修改后的商品信息到WMS
        /// </summary>
        /// <returns></returns>
        string UpdateProductsInfoToWMS(int productId);
        #endregion


        #region 销售商品
        /// <summary>
        /// 获取销售商品信息
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="channelId">默认94,现货渠道</param>
        /// <param name="priceTypeId">默认103，标准价</param>
        /// <returns></returns>
        SaleProduct GetSaleProduct(int productId, int channelId = 94, int priceTypeId = 103);
        PageList<SaleProductsModel> GetSaleProductPriceList(int pageSize, int pageIndex, string searchStr = "", int priceType = 103, int channel = 94);
        /// <summary>
        /// 通过商品编码获取销售商品
        /// </summary>
        /// <param name="goodSn"></param>
        /// <returns></returns>
        SaleProduct GetSaleProductByGoodSn(string goodSn);
        /// <summary>
        /// 获取销售商品通过销售商品ID
        /// </summary>
        /// <returns></returns>
        SaleProduct GetSaleProductBySaleProductId(int saleProductId);
        ///<summary>
        ///获取全部销售商品
        /// </summary>
        /// <returns></returns>
        IQueryable GetAllSaleProducts();
        /// <summary>
        /// 获取所有销售商品
        /// </summary>
        /// <returns></returns>
        IEnumerable<SaleProduct> GetAllSaleProductsList();
        /// <summary>
        /// 添加销售商品
        /// </summary>
        /// <param name="saleProduct"></param>
        /// <returns></returns>
        int CreateSaleProducts(SaleProduct saleProduct);
        /// <summary>
        /// 确认是否存在同渠道销售商品
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        bool ConfirmSaleProductExist(int productId, int channel);
        /// <summary>
        /// 通过销售商品ID查找销售商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IQueryable GetSaleProductById(int saleProductId);
        /// <summary>
        /// 修改销售商品
        /// </summary>
        /// <param name="saleProduct"></param>
        /// <returns></returns>
        bool UpdateSaleProduct(SaleProduct saleProduct);
        /// <summary>
        /// 删除销售商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DelSaleProduct(int id);
        /// <summary>
        /// 通过订单类型获取销售商品
        /// </summary>
        /// <returns></returns>
        IEnumerable<SaleProduct> GetSaleProductsByOrderType(int orderType);
        PageList<Object> GetSaleProductsPageByOrderType(int orderType, int pageSize, int pageIndex, string searchStr = "");
        /// <summary>
        /// 根据销售商品Id获取销售商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        SaleProductDetailModel GetSaleProductDetailBySaleProductId(int id);
        IQueryable GetSaleProductByChannel();
        #endregion


        #region 商品系列
        ///<summary>
        ///获取全部商品系列
        ///</summary>
        IQueryable GetAllCategory();
        /// <summary>
        /// 通过系列名查找商品系列
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool GetCategoryByName(string name);
        /// <summary>
        /// 通过系列ID查找商品系列
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Category GetCategoryById(int id);
        /// <summary>
        /// 添加商品系列
        /// </summary>
        /// <param name="category"></param>
        void CreateCategory(Category category);
        /// <summary>
        /// 修改商品系列
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        bool UpdateCategory(Category category);
        /// <summary>
        /// 删除商品系列
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        void DelCategory(int id);
        #endregion


        #region 销售商品价格
        ///<summary>
        ///获取全部销售商品价格
        ///</summary>
        ///<returns></returns>
        IQueryable GetAllSaleProductsPrice();
        /// <summary>
        /// 根据销售商品Id和价格类型获取销售商品价格
        /// </summary>
        /// <param name="saleProductId"></param>
        /// <param name="customerTypeId"></param>
        /// <returns></returns>
        SaleProductPrice GetSaleProductPriceBySaleProductIdAndCustomerTypeId(int saleProductId, int customerTypeId);
        /// <summary>
        /// 获取全部销售商品价格（列表）
        /// </summary>
        /// <returns></returns>
        List<SaleProductPriceList> SaleProductPriceList();
        /// <summary>
        /// 获取销售商品
        /// </summary>
        /// <returns></returns>
        PageList<SaleProductPriceList> GetSaleProductPriceLists(SearchProductContext searchProductContext);
        /// <summary>
        /// 通过销售商品ID获取销售商品价格
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<SaleProductPriceList> GetSaleProductPriceBySaleProductId(int id);
        /// <summary>
        /// 通过销售商品价格ID获取销售商品价格（IQueryable）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IQueryable GetSaleProductPriceById(int id);
        /// <summary>
        /// 通过销售商品ID、价格类型获取销售商品价格
        /// </summary>
        /// <param name="SaleProductId"></param>
        /// <param name="CustomerTypeId"></param>
        /// <returns></returns>
        SaleProductPrice GetSaleProductPriceById(int SaleProductId, int? CustomerTypeId);
        /// <summary>
        /// 添加销售商品价格
        /// </summary>
        /// <param name="saleProductPrice"></param>
        void CreateSaleProductsPrice(SaleProductPrice saleProductPrice);
        /// <summary>
        /// 修改销售商品价格
        /// </summary>
        /// <param name="saleProductPrice"></param>
        void UpdateSaleProductPrice(SaleProductPrice saleProductPrice);
        /// <summary>
        /// 通过销售商品Id获取销售商品标准原价
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        decimal GetOriginalPriceBySaleProductId(int id);
        #endregion


        #region 平台商品
        /// <summary>
        /// 获取全部平台商品
        /// </summary>
        IQueryable GetAllPlantformProducts();
        /// <summary>
        /// 添加平台商品
        /// </summary>
        /// <param name="platformProduct"></param>
        void CreatedPlatformProduct(PlatformProduct platformProduct);
        /// <summary>
        /// 检测平台商品是否存在
        /// </summary>
        /// <param name="SaleProductId"></param>
        /// <param name="PlatForm"></param>
        /// <returns></returns>
        bool ConfirmPlatformProductIfExist(int SaleProductId, int PlatForm);
        /// <summary>
        /// 通过平台商品ID获取平台商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        PlatformProduct GetPlatformProductById(int id);
        /// <summary>
        /// 删除平台商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DelPlatformProduct(int id);
        #endregion


        #region 套装商品
        IEnumerable<SuitProducts> GetAllSuitProducts();
        SuitProducts InsertSuitProducts(SuitProducts suitPro);
        SuitProducts GetSuitProductsBySuitProId(int suitProId);
        SuitProducts UpdateSuitProducts(SuitProducts suitPro);
        bool DeleteSuitProductsById(int suitProId);
        IEnumerable<SuitProductsDetail> GetAllSuitProductsDetail();
        SuitProductsDetail InsertSuitProductsDetail(SuitProductsDetail suitProDetail);
        SuitProductsDetail UpdateSuitProductsDetail(SuitProductsDetail suitProDetail);
        bool DeleteSuitProductsDetailById(int suitProDetailId);
        List<SuitProducts> GetSuitProductsByNameOrCode(SuitProducts suitProducts);
        SuitProductsModel GetSuitProductsById(int suitProId);
        SuitProductsDetail GetSuitProductsDetailById(int suitProId, int saleProId);
        SuitProductsDetail GetSuitProductsDetail(int suitProDetailId);
        #endregion


        #region 同步相关
        dynamic PMSynchronize(List<WineDto> wineDtos);
        /// <summary>
        /// OMS所有商品库存同步
        /// </summary>
        /// <param name="saleProWareHouseStocks"></param>
        /// <returns></returns>
        dynamic SyncSaleProductStock(List<SaleProductWareHouseStockSyncModel> saleProWareHouseStocks);
        /// <summary>
        /// OMS单个商品库存同步
        /// </summary>
        /// <param name="saleProWareHouseStocks"></param>
        /// <returns></returns>
        dynamic SyncSingleSaleProductStock(List<SaleProductWareHouseStockSyncModel> saleProWareHouseStocks);
        /// <summary>
        /// 根据商品Id获取该商品在WMS中总库存
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        string GetWMSProdcutStockByProductId(int productId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="code"></param>
        /// <param name="quantity"></param>
        void SyncProductStockToAssist(int shopid, string code, int quantity);

        /// <summary>
        /// 同步商品可用多个个商品库存到商城
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="code"></param>
        /// <param name="quantity"></param>
        void SyncMoreProductStockToAssist(List<ProductStockData> productStockDataList);
        #endregion


        #region 销售商品仓库库存
        SaleProductWareHouseStock CreateSaleProductWareHouseStock(SaleProductWareHouseStock saleProdctWHStock);
        SaleProductWareHouseStock UpdateSaleProductWareHouseStock(SaleProductWareHouseStock saleProdctWHStock);
        SaleProductWareHouseStock GetSaleProductWareHouseStockById(int saleProId, int wareHouseId);
        IEnumerable<SaleProductWareHouseStock> GetSaleProductStocksBySaleProductId(int saleProductId);
        //IEnumerable<SaleProductWareHouseStockModel> GetSaleProductStocksBySaleProductId(int saleProductId);
        IEnumerable<SaleProductWareHouseStockModel> GetSaleProductStocksByProductId(int productId);
        //IEnumerable<SaleProductWareHouseStockModel> GetSaleProductStocksByProductId(int productId);

        SaleProductWareHouseStock GetSPHStockByWareHouseCodeAndProduct(string wareHouseCode, int productId);
        /// <summary>
        /// 通过商品编码获取商品仓库库存
        /// </summary>
        /// <returns></returns>
        List<StockFeedBackDataDetail> GetSaleProductWareHouseStocksByProductCode(string productCode);
        #endregion


        #region 销售商品锁定查询
        SaleProductLockedTrack CreateSaleProductLockedTrack(SaleProductLockedTrack saleProductLockedTrack);
        SaleProductLockedTrack UpdateSaleProductLockedTrack(SaleProductLockedTrack saleProductLockedTrack);
        SaleProductLockedTrack GetSaleProductLockedTrackById(int orderId, int saleProId, int? orderProId = null);
        IEnumerable<SaleProductLockedTrackModel> GetAllSaleProductLockedTrackBySaleProId(int saleProductId);
        /// <summary>
        /// 添加锁定商品信息到锁定跟踪表以及OMS仓库库存信息表（事务）
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="saleProductId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="lockedNumber"></param>
        /// <param name="orderProId">需要精确到orderProductId的可以添加次参数（如删除、等价换货）</param>
        /// <returns></returns>
        string AddSaleProductLockedTrackAndWareHouseStock(int orderId, int saleProductId, int wareHouseId, int lockedNumber, int? orderProId);
        /// <summary>
        /// 添加锁定商品信息到锁定跟踪表以及OMS仓库库存信息表（无事务）
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="saleProductId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="lockedNumber"></param>
        /// <returns></returns>
        bool CreateSaleProductLockedTrackAndWareHouseStock(int orderId, int saleProductId, int wareHouseId, int lockedNumber, int? orderProId);
        /// <summary>
        /// 修改订单商品更新库存锁定及库存跟踪
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="saleProductId"></param>
        /// <param name="oldSaleProSumNum"></param>
        /// <returns></returns>
        string UpdateOrderProChangeLockedLog(int orderId, int saleProductId);
        /// <summary>
        /// 变更仓库更改仓库锁定信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="oldWareHouseId"></param>
        /// <param name="newWareHouseId"></param>
        /// <returns></returns>
        bool ChangeWareHouseSetProLockedLog(int orderId, int oldWareHouseId, int newWareHouseId);
        /// <summary>
        /// B2C订单拆分/合并解锁旧订单锁定库存
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        bool ChangeProLockedNumByHasLockedNum(int orderId);
        /// <summary>
        /// 删除订单商品解锁已锁定的库存
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderProId"></param>
        /// <returns></returns>
        bool DeleteProLockedNumByHasLockedNum(int orderId, int orderProId);
        /// <summary>
        /// 缺货状态解除
        /// </summary>
        /// <param name="orderIdList"></param>
        /// <returns></returns>
        string UnLockLackStockOrder(List<int> orderIdList);
        /// <summary>
        /// 分页获取仓库库存列表
        /// </summary>
        /// <param name="searchProductContext"></param>
        /// <returns></returns>
        PageList<WareHouseStockViewModel> GetWareHouseStockViewModels(SearchProductContext searchProductContext, out int allStock, out int allLockStock, out int allAvailableStock);
        #endregion

        #region 获取WMS库存并同步到OMS（保持库存一致）
        bool SyncStocksWmsToOms(int productId = 0);
        /// <summary>
        /// 更新商品仓库库存
        /// </summary>
        /// <returns></returns>
        bool UpdateWareHouseStock(List<WareHouseStock> wareHouseStocks, int productId = 0);
        List<SaleProductWareHouseStock> GetXuniStocks(int saleProductId);
        #endregion
       Task<bool> SynStock();
    }
}
