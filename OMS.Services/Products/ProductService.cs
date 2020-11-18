using LitJson;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using OMS.Core;
using OMS.Core.Json;
using OMS.Data.Domain;
using OMS.Data.Interface;
using OMS.Model;
using OMS.Model.JsonModel;
using OMS.Model.Products;
using OMS.Services.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace OMS.Services.Products
{
    public class ProductService : ServiceBase, IProductService
    {
        #region ctor
        private ILogger<ProductService> _logger;
        private ILogService _logService;
        private readonly IConfiguration configuration;

        //锁对象1
        private readonly static object _MyLock1 = new object();
        //锁对象1
        private readonly static object _MyLock2 = new object();

        public ProductService(IDbAccessor omsAccessor, IWorkContext workContext, ILogger<ProductService> logger, ILogService logService, IConfiguration configuration)
            : base(omsAccessor, workContext, configuration)
        {
            _logger = logger;
            _logService = logService;
            this.configuration = configuration;
        }
        #endregion


        #region 商品
        /// <summary>
        /// 通过ID查找商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Product GetProductById(int id)
        {
            if (id == 0)
                return null;
            var pro = _omsAccessor.GetById<Product>(id);
            return pro;
        }
        /// <summary>
        /// 通过商品编码获取商品
        /// </summary>
        /// <returns></returns>
        public Product GetProductByCode(string code)
        {
            var product = new Product();
            product = _omsAccessor.Get<Product>().Where(x => x.Code == code.Trim() && x.Isvalid).FirstOrDefault();

            return product;

        }
        /// <summary>
        /// 通过商品名查找商品
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Product GetProductByName(string name)
        {
            var product = _omsAccessor.Get<Product>().Where(p => p.Isvalid && p.Name == name).FirstOrDefault();
            return product;
        }
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="product"></param>
        public void InsertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentException("Product");
            product.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert(product);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 修改商品
        /// </summary>
        /// <param name="product"></param>
        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentException("Product");
            product.ModifiedBy = _workContext.CurrentUser.Id;
            product.ModifiedTime = DateTime.Now;
            _omsAccessor.Update(product);
            _omsAccessor.SaveChanges();

        }
        /// <summary>
        /// 修改商品图片
        /// </summary>
        /// <param name="url"></param>
        /// <param name="id"></param>
        public void UpdateProductImage(string url, int id)
        {
            Product product = GetProductById(id);
            product.Cover = url;
            _omsAccessor.Update<Product>(product);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 检测商品信息是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ConfirmProductExist(string name)
        {
            var exist = _omsAccessor.Get<Product>().Where(p => p.Isvalid && p.Name == name).FirstOrDefault();
            if (exist != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DelProduct(int id)
        {
            Product product = _omsAccessor.Get<Product>().Where(p => p.Isvalid && p.Id == id).FirstOrDefault();
            if (product != null)
            {
                _omsAccessor.Delete(product);
                _omsAccessor.SaveChanges();
                return true;
            }
            else
            {
                throw new ArgumentException("Product");
            }
        }
        /// <summary>
        /// 获取全部商品
        /// </summary>
        /// <returns></returns>
        public IQueryable GetAllProducts()
        {
            var ProductList = from p in _omsAccessor.Get<Product>()
                              join t in _omsAccessor.Get<Dictionary>() on p.Type equals t.Id
                              join c in _omsAccessor.Get<Dictionary>() on p.Country equals c.Id into cg
                              from cgi in cg.DefaultIfEmpty()
                              join a in _omsAccessor.Get<Dictionary>() on p.Area equals a.Id into ag
                              from agi in ag.DefaultIfEmpty()
                                  //join g in _omsAccessor.Get<Dictionary>() on Convert.ToInt32(p.Grapes) equals g.Id into gp
                                  //from gpi in gp.DefaultIfEmpty()
                              where p.Isvalid
                              select new
                              {
                                  p.Id,
                                  p.Name,
                                  p.NameEn,
                                  p.Type,
                                  TypeName = t.Value,
                                  p.Code,
                                  p.Cover,
                                  p.Country,
                                  CountryName = cgi.Value,
                                  p.Area,
                                  AreaName = agi.Value,
                                  p.Grapes,
                                  p.Capacity,
                                  p.Year,
                                  p.DeputyBarcode
                              };
            return ProductList;
        }
        /// <summary>
        /// 分页查询商品
        /// </summary>
        /// <param name="searchProductContext"></param>
        /// <returns></returns>
        public PageList<ProductViewModel> GetProductViewModels(SearchProductContext searchProductContext)
        {
            var productList = from p in _omsAccessor.Get<Product>()
                              join t in _omsAccessor.Get<Dictionary>() on p.Type equals t.Id into tg
                              from tgi in tg.DefaultIfEmpty()
                              join c in _omsAccessor.Get<Dictionary>() on p.Country equals c.Id into cg
                              from cgi in cg.DefaultIfEmpty()
                              join a in _omsAccessor.Get<Dictionary>() on p.Area equals a.Id into ag
                              from agi in ag.DefaultIfEmpty()
                                  //join g in _omsAccessor.Get<Dictionary>() on Convert.ToInt32(p.Grapes) equals g.Id into gp
                                  //from gpi in gp.DefaultIfEmpty()
                              where p.Isvalid
                              orderby p.CreatedTime descending
                              select new ProductViewModel
                              {
                                  Id = p.Id,
                                  Name = p.Name,
                                  NameEn = p.NameEn,
                                  TypeName = tgi.Value ?? "",
                                  Code = p.Code,
                                  Cover = p.Cover,
                                  CountryName = cgi.Value ?? "",
                                  AreaName = agi.Value ?? "",
                                  Grapes = p.Grapes,
                                  Year = p.Year,
                                  DeputyBarcode = p.DeputyBarcode
                              };
            if (!string.IsNullOrEmpty(searchProductContext.SearchStr))
            {
                productList = productList.Where(x => x.Name.Contains(searchProductContext.SearchStr) || x.NameEn.Contains(searchProductContext.SearchStr) || x.Code.Contains(searchProductContext.SearchStr) || x.CountryName.Contains(searchProductContext.SearchStr) || x.Grapes.Contains(searchProductContext.SearchStr) || x.DeputyBarcode.Contains(searchProductContext.SearchStr)
                || x.Year.Contains(searchProductContext.SearchStr) || x.TypeName.Contains(searchProductContext.SearchStr) || x.AreaName.Contains(searchProductContext.SearchStr));
            }

            return new PageList<ProductViewModel>(productList, searchProductContext.PageIndex, searchProductContext.PageSize);
        }
        public PageList<Product> GetProductList(int pageSize, int pageIndex, int TypeId = 0, string searchStr = "")
        {
            var query = _omsAccessor.Get<Product>().Where(x => x.Isvalid);

            if (TypeId != 0)
            {
                query = query.Where(x => x.Type == TypeId);
            }

            if (!string.IsNullOrEmpty(searchStr))
            {
                query = query.Where(x => x.Name.Contains(searchStr) || x.NameEn.Contains(searchStr) || x.Code.Contains(searchStr));
            }
            return new PageList<Product>(query, pageIndex, pageSize);
        }
        /// <summary>
        /// 更新修改后的商品信息到WMS
        /// </summary>
        /// <param name="productId"></param>
        public string UpdateProductsInfoToWMS(int productId)
        {
            var product = _omsAccessor.Get<Product>().Where(r => r.Isvalid && r.Id == productId).FirstOrDefault();
            if (product != null)
            {
                List<object> proList = new List<object>();
                proList.Add(new
                {
                    product.Name,
                    OMSId = product.Id,
                    OMSType = product.Type,
                    product.NameEn,
                    product.Cover,
                    product.Country,
                    product.Area,
                    product.Grapes,
                    product.Capacity,
                    product.Year,
                    product.Packing,
                    product.Code,
                    product.DeputyBarcode
                });
                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    var content = new StringContent(JsonConvert.SerializeObject(proList), Encoding.UTF8, "application/json");
                    var response = http.PostAsync(_configuration.GetSection("WMSApi")["domain"] + "/wmsapi/ProductSync/OmsSyncUpdateProducts", content);
                    if (response.Result.IsSuccessStatusCode)
                    {
                        return response.Result.Content.ReadAsStringAsync().ToString();
                    }
                    else
                    {
                        return "更新失败！";
                    }
                }
            }
            return "无法查找到该商品！无法更新到WMS";
        }
        #endregion


        #region 销售商品
        public SaleProduct GetSaleProduct(int productId, int channelId = 94, int priceTypeId = 103)
        {
            var saleProduct = _omsAccessor.Get<SaleProduct>().Include(x => x.Product).Where(p => p.ProductId == productId && p.Channel == channelId && p.Isvalid)
                .Include(p => p.SaleProductPrice).FirstOrDefault();
            //筛选价格类型
            if (saleProduct != null && saleProduct.SaleProductPrice != null)
            {
                saleProduct.SaleProductPrice = saleProduct.SaleProductPrice.Where(p => p.CustomerTypeId == priceTypeId && p.Isvalid).ToList();
            }
            return saleProduct;
        }
        public PageList<SaleProductsModel> GetSaleProductPriceList(int pageSize, int pageIndex, string searchStr = "", int priceType = 103, int channel = 94)
        {
            var query = from s in _omsAccessor.Get<SaleProductPrice>().Where(s => s.Isvalid && s.SaleProduct.Channel == channel).Include(s => s.SaleProduct.Product)
                        where s.CustomerTypeId == priceType
                        select new SaleProductsModel
                        {
                            SaleProductId = s.SaleProduct.Id,
                            Name = s.SaleProduct.Product.Name,
                            NameEn = s.SaleProduct.Product.NameEn,
                            Code = s.SaleProduct.Product.Code,
                            Price = s.Price
                        };
            if (!string.IsNullOrEmpty(searchStr))
            {
                query = query.Where(s => s.Name.Contains(searchStr) || s.NameEn.Contains(searchStr) || s.Code.Contains(searchStr));
            }
            return new PageList<SaleProductsModel>(query, pageIndex, pageSize);
        }
        /// <summary>
        /// 通过商品编码获取销售商品
        /// </summary>
        /// <param name="goodSn"></param>
        /// <returns></returns>
        public SaleProduct GetSaleProductByGoodSn(string goodSn)
        {
            SaleProduct saleProduct = new SaleProduct();
            var product = _omsAccessor.Get<Product>().Where(p => p.Isvalid && p.Code == goodSn).FirstOrDefault();
            if (product != null)
            {
                saleProduct = _omsAccessor.Get<SaleProduct>().Where(p => p.Isvalid && p.ProductId == product.Id).FirstOrDefault();
            }

            return saleProduct;
        }
        /// <summary>
        /// 获取销售商品通过销售商品ID
        /// </summary>
        /// <returns></returns>
        public SaleProduct GetSaleProductBySaleProductId(int saleProductId)
        {
            SaleProduct saleProduct = new SaleProduct();
            saleProduct = _omsAccessor.Get<SaleProduct>().Include(x => x.Product).Where(s => s.Id == saleProductId && s.Isvalid).FirstOrDefault();
            return saleProduct;
        }
        ///<summary>
        ///获取全部销售商品
        /// </summary>
        /// <returns></returns>
        public IQueryable GetAllSaleProducts()
        {
            var saleProducts = from s in _omsAccessor.Get<SaleProduct>()
                               join p in _omsAccessor.Get<Product>() on s.ProductId equals p.Id
                               join d in _omsAccessor.Get<Dictionary>() on s.Channel equals d.Id
                               //join sp in _omsAccessor.Get<SaleProductPrice>()on s.Id equals sp.SaleProductId into pg
                               //from pri in pg.DefaultIfEmpty()
                               //join pt in _omsAccessor.Get<Dictionary>() on pri.CustomerTypeId equals pt.Id into ptg
                               //from ptitem in ptg.DefaultIfEmpty()
                               where s.Isvalid
                               select new
                               SaleProductModel
                               {
                                   Id = s.Id,
                                   ProductId = s.ProductId,
                                   Name = p.Name,
                                   Code = p.Code,
                                   Value = d.Value,
                                   //PriceType = ptitem.Value,
                                   //Price = pri.Price==null?0:pri.Price,
                                   Stock = s.Stock,
                                   LockStock = s.LockStock,
                                   AvailableStock = s.AvailableStock
                               };
            return saleProducts;
        }
        /// <summary>
        /// 获取所有销售商品库存
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SaleProduct> GetAllSaleProductsList()
        {
            return _omsAccessor.Get<SaleProduct>().Where(r => r.Isvalid).ToList();
        }
        /// <summary>
        /// 添加销售商品
        /// </summary>
        /// <param name="saleProduct"></param>
        /// <returns></returns>
        public int CreateSaleProducts(SaleProduct saleProduct)
        {

            if (saleProduct == null)
                throw new ArgumentException("SaleProduct");
            saleProduct.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert<SaleProduct>(saleProduct);
            _omsAccessor.SaveChanges();
            return saleProduct.Id;
        }
        /// <summary>
        /// 确认是否存在同渠道销售商品
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool ConfirmSaleProductExist(int productId, int channel)
        {
            var isexist = _omsAccessor.Get<SaleProduct>().Where(s => s.ProductId == productId && s.Channel == channel);
            if (isexist.Count() > 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 通过销售商品ID查找销售商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable GetSaleProductById(int saleProductId)
        {
            var saleProduct = from s in _omsAccessor.Get<SaleProduct>()
                              join p in _omsAccessor.Get<Product>() on s.ProductId equals p.Id
                              join d in _omsAccessor.Get<Dictionary>() on s.Channel equals d.Id
                              where s.Isvalid && s.Id == saleProductId
                              select new
                              {
                                  s.Id,
                                  s.ProductId,
                                  p.Name,
                                  s.Channel,
                                  p.Code,
                                  d.Value,
                                  s.Stock,
                                  s.LockStock,
                                  s.AvailableStock
                              };
            return saleProduct;
        }
        /// <summary>
        /// 修改销售商品
        /// </summary>
        /// <param name="saleProduct"></param>
        /// <returns></returns>
        public bool UpdateSaleProduct(SaleProduct saleProduct)
        {
            if (saleProduct == null)
                throw new ArgumentException("SaleProduct");
            //saleProduct.ModifiedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Update<SaleProduct>(saleProduct);
            _omsAccessor.SaveChanges();
            return true;
        }
        /// <summary>
        /// 删除销售商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DelSaleProduct(int id)
        {
            var saleproductprices = _omsAccessor.Get<SaleProductPrice>().Where(sp => sp.SaleProductId == id);
            var saleproduct = _omsAccessor.GetById<SaleProduct>(id);
            _omsAccessor.DeleteRange(saleproductprices.ToList());
            _omsAccessor.Delete<SaleProduct>(saleproduct);
            _omsAccessor.SaveChanges();
            return true;
        }
        /// <summary>
        /// 通过订单类型获取销售商品
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SaleProduct> GetSaleProductsByOrderType(int orderType)
        {
            //var saleproducts = _omsAccessor.Get<SaleProduct>(s => s.Isvalid && s.Channel == orderType).Include(p => p.Product);
            var saleproducts = from s in _omsAccessor.Get<SaleProduct>().Include(p => p.Product) where s.Isvalid && s.Channel == orderType select s;
            return saleproducts;
        }
        public PageList<Object> GetSaleProductsPageByOrderType(int orderType, int pageSize, int pageIndex, string searchStr = "")
        {
            var saleproducts = from s in _omsAccessor.Get<SaleProduct>().Include(p => p.Product)
                               where s.Isvalid && s.Channel == orderType
                               join p in _omsAccessor.Get<Product>() on s.ProductId equals p.Id
                               where
                               (string.IsNullOrEmpty(searchStr) || p.Name.Contains(searchStr) || p.Code.Contains(searchStr))
                               select new
                               {
                                   s.Id,
                                   s.AvailableStock,
                                   s.Channel,
                                   s.CreatedBy,
                                   s.CreatedTime,
                                   s.Isvalid,
                                   s.LockStock,
                                   s.ModifiedBy,
                                   s.ModifiedTime,
                                   s.ProductId,
                                   s.Stock,
                                   p.Name,
                                   p.Code,
                                   p.NameEn
                               };
            return new PageList<Object>(saleproducts, pageIndex, pageSize);
        }
        /// <summary>
        /// 根据销售商品Id获取销售商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SaleProductDetailModel GetSaleProductDetailBySaleProductId(int id)
        {
            var result = from sp in _omsAccessor.Get<SaleProduct>()
                         join p in _omsAccessor.Get<Product>() on sp.ProductId equals p.Id
                         join c in _omsAccessor.Get<Dictionary>().Where(r => r.Type == DictionaryType.Channel) on sp.Channel equals c.Id
                         where sp.Isvalid && p.Isvalid && c.Isvalid && sp.Id == id
                         orderby sp.CreatedTime descending
                         select new SaleProductDetailModel
                         {
                             Id = sp.Id,
                             ProductId = sp.ProductId,
                             ProductName = p.Name,
                             Channel = sp.Channel,
                             ChannelName = c.Value,
                             LockStock = sp.LockStock,
                             AvailableStock = sp.AvailableStock,
                             Stock = sp.Stock,
                             SaleProductPriceDetailModels = (from spp in _omsAccessor.Get<SaleProductPrice>()
                                                             join pt in _omsAccessor.Get<Dictionary>().Where(r => r.Type == DictionaryType.PriceType)
                                                             on spp.CustomerTypeId equals pt.Id
                                                             where spp.Isvalid && pt.Isvalid && spp.SaleProductId == sp.Id
                                                             orderby spp.CustomerTypeId
                                                             select new SaleProductPriceDetailModel
                                                             {
                                                                 Id = spp.Id,
                                                                 CustomerTypeId = spp.CustomerTypeId,
                                                                 SaleProductId = spp.SaleProductId,
                                                                 PriceTypeName = pt.Value,
                                                                 Price = spp.Price,
                                                             }).ToList()
                         };
            return result.FirstOrDefault();
        }
        public IQueryable GetSaleProductByChannel()
        {
            var data = from s in _omsAccessor.Get<SaleProduct>()
                       join d in _omsAccessor.Get<Dictionary>() on s.Channel equals d.Id
                       select new
                       {
                           s.Id,
                           s.ProductId,
                           s.Product.Name,
                           s.Channel,
                           ChannelName = d.Value,
                           s.Stock,
                           s.AvailableStock,
                           s.LockStock,
                       } into gs
                       group gs by gs.Channel;
            return data;
        }
        #endregion


        #region 商品系列
        ///<summary>
        ///获取全部商品系列
        ///</summary>
        public IQueryable GetAllCategory()
        {
            var CategoryList = from p in _omsAccessor.Get<Category>() where p.Isvalid select p;
            return CategoryList;
        }
        /// <summary>
        /// 通过系列名查找商品系列
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetCategoryByName(string name)
        {
            var count = _omsAccessor.Get<Category>().Where(p => p.Isvalid && p.Name == name).Count();
            if (count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 通过系列ID查找商品系列
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Category GetCategoryById(int id)
        {
            var category = _omsAccessor.Get<Category>().Where(c => c.Isvalid && c.Id == id).FirstOrDefault();
            return category;
        }
        /// <summary>
        /// 添加商品系列
        /// </summary>
        /// <param name="category"></param>
        public void CreateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentException("Category");
            category.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert<Category>(category);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 修改商品系列
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool UpdateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentException("Category");
            category.ModifiedBy = _workContext.CurrentUser.Id;
            category.ModifiedTime = DateTime.Now;
            _omsAccessor.Update(category);
            _omsAccessor.SaveChanges();
            return true;
        }
        /// <summary>
        /// 删除商品系列
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void DelCategory(int id)
        {
            if (GetCategoryById(id) == null)
                throw new ArgumentException("Category");
            else
                _omsAccessor.DeleteById<Category>(id);
            _omsAccessor.SaveChanges();
        }
        #endregion


        #region 销售商品价格
        ///<summary>
        ///获取全部销售商品价格
        ///</summary>
        ///<returns></returns>
        public IQueryable GetAllSaleProductsPrice()
        {
            var saleProductsPrice = from s in _omsAccessor.Get<SaleProductPrice>()
                                    join sp in _omsAccessor.Get<SaleProduct>() on s.SaleProductId equals sp.Id
                                    join d in _omsAccessor.Get<Dictionary>() on s.CustomerTypeId equals d.Id
                                    join sd in _omsAccessor.Get<Dictionary>() on sp.Channel equals sd.Id
                                    where s.Isvalid
                                    select new
                                    {
                                        s.Id,
                                        s.SaleProductId,
                                        SaleProductName = sp.Product.Name,
                                        ChannelName = sd.Value,
                                        s.CustomerTypeId,
                                        CustomerTypeName = d.Value,
                                        s.Price,
                                        s.Isvalid,
                                        s.ModifiedBy,
                                        s.CreatedBy,
                                        s.ModifiedTime,
                                        s.CreatedTime
                                    };
            return saleProductsPrice;
        }
        /// <summary>
        /// 根据销售商品Id和价格类型获取销售商品价格
        /// </summary>
        /// <param name="saleProductId"></param>
        /// <param name="customerTypeId"></param>
        /// <returns></returns>
        public SaleProductPrice GetSaleProductPriceBySaleProductIdAndCustomerTypeId(int saleProductId, int customerTypeId)
        {
            return _omsAccessor.Get<SaleProductPrice>().Where(r => r.Isvalid && r.SaleProductId == saleProductId && r.CustomerTypeId == customerTypeId).FirstOrDefault();
        }
        /// <summary>
        /// 获取全部销售商品价格（列表）
        /// </summary>
        /// <returns></returns>
        public List<SaleProductPriceList> SaleProductPriceList()
        {
            #region
            var getSaleProductId = from s in _omsAccessor.Get<SaleProductPrice>()
                                   where s.Isvalid
                                   join dd in _omsAccessor.Get<Dictionary>() on s.CustomerTypeId equals dd.Id
                                   join sd in _omsAccessor.Get<Dictionary>() on s.SaleProduct.Channel equals sd.Id
                                   join p in _omsAccessor.Get<Product>() on s.SaleProduct.ProductId equals p.Id
                                   select new
                                   {
                                       s.Id,
                                       s.SaleProductId,
                                       ProductName = p.Name,
                                       s.SaleProduct.Channel,
                                       ChannelName = sd.Value,
                                       s.SaleProduct.Stock,
                                       s.SaleProduct.LockStock,
                                       s.SaleProduct.AvailableStock,
                                       s.CustomerTypeId,
                                       CustomerTypeName = dd.Value,
                                       s.Price
                                   } into gs
                                   group gs by gs.SaleProductId;
            #endregion
            List<SaleProductPriceList> data = new List<SaleProductPriceList>();
            foreach (var item in getSaleProductId)
            {
                var a = item.Key;
                var c = item.ToList();
                List<SaleProductPriceBaseList> list = new List<SaleProductPriceBaseList>();
                SaleProductPriceList saleProductPriceList = new SaleProductPriceList();
                foreach (var i in c)
                {
                    SaleProductPriceBaseList sppb = new SaleProductPriceBaseList();
                    sppb.Id = i.Id;
                    sppb.SaleProductId = i.SaleProductId;
                    sppb.ProductName = i.ProductName;
                    sppb.Channel = i.Channel;
                    sppb.ChannelName = i.ChannelName;
                    sppb.CustomerTypeId = i.CustomerTypeId;
                    sppb.CustomerTypeName = i.CustomerTypeName;
                    sppb.Stock = i.Stock;
                    sppb.LockStock = i.LockStock;
                    sppb.AvailableStock = i.AvailableStock;
                    sppb.Price = i.Price;
                    list.Add(sppb);
                }
                saleProductPriceList.Id = list.FirstOrDefault().Id;
                saleProductPriceList.SaleProductId = list.FirstOrDefault().SaleProductId;
                saleProductPriceList.ChannelName = list.FirstOrDefault().ChannelName;
                saleProductPriceList.SaleProductName = list.FirstOrDefault().ProductName;
                saleProductPriceList.Stock = list.FirstOrDefault().Stock;
                saleProductPriceList.LockStock = list.FirstOrDefault().LockStock;
                saleProductPriceList.AvailableStock = list.FirstOrDefault().AvailableStock;
                saleProductPriceList.SaleProductPriceBaseList = list;
                data.Add(saleProductPriceList);
            }
            return data;
        }
        /// <summary>
        /// 获取销售商品
        /// </summary>
        /// <returns></returns>
        public PageList<SaleProductPriceList> GetSaleProductPriceLists(SearchProductContext searchProductContext)
        {
            #region
            var getSaleProductId = from s in _omsAccessor.Get<SaleProductPrice>()
                                   where s.Isvalid
                                   join dd in _omsAccessor.Get<Dictionary>() on s.CustomerTypeId equals dd.Id
                                   join sd in _omsAccessor.Get<Dictionary>() on s.SaleProduct.Channel equals sd.Id
                                   join p in _omsAccessor.Get<Product>() on s.SaleProduct.ProductId equals p.Id
                                   select new
                                   {
                                       s.Id,
                                       s.SaleProductId,
                                       ProductName = p.Name,
                                       ProductCode = p.Code,
                                       s.SaleProduct.Channel,
                                       ChannelName = sd.Value,
                                       s.SaleProduct.Stock,
                                       s.SaleProduct.LockStock,
                                       s.SaleProduct.AvailableStock,
                                       s.CustomerTypeId,
                                       CustomerTypeName = dd.Value,
                                       s.Price
                                   } into gs
                                   group gs by gs.SaleProductId;
            #endregion
            List<SaleProductPriceList> data = new List<SaleProductPriceList>();
            foreach (var item in getSaleProductId)
            {
                var a = item.Key;
                var c = item.ToList();
                List<SaleProductPriceBaseList> list = new List<SaleProductPriceBaseList>();
                SaleProductPriceList saleProductPriceList = new SaleProductPriceList();
                foreach (var i in c)
                {
                    SaleProductPriceBaseList sppb = new SaleProductPriceBaseList();
                    sppb.Id = i.Id;
                    sppb.SaleProductId = i.SaleProductId;
                    sppb.ProductName = i.ProductName;
                    sppb.ProductCode = i.ProductCode;
                    sppb.Channel = i.Channel;
                    sppb.ChannelName = i.ChannelName;
                    sppb.CustomerTypeId = i.CustomerTypeId;
                    sppb.CustomerTypeName = i.CustomerTypeName;
                    sppb.Stock = i.Stock;
                    sppb.LockStock = i.LockStock;
                    sppb.AvailableStock = i.AvailableStock;
                    sppb.Price = i.Price;
                    list.Add(sppb);
                }
                saleProductPriceList.Id = list.FirstOrDefault().Id;
                saleProductPriceList.SaleProductId = list.FirstOrDefault().SaleProductId;
                saleProductPriceList.ChannelName = list.FirstOrDefault().ChannelName;
                saleProductPriceList.SaleProductName = list.FirstOrDefault().ProductName;
                saleProductPriceList.SaleProductCode = list.FirstOrDefault().ProductCode;
                saleProductPriceList.Stock = list.FirstOrDefault().Stock;
                saleProductPriceList.LockStock = list.FirstOrDefault().LockStock;
                saleProductPriceList.AvailableStock = list.FirstOrDefault().AvailableStock;
                saleProductPriceList.SaleProductPriceBaseList = list;
                data.Add(saleProductPriceList);
            }

            if (!string.IsNullOrEmpty(searchProductContext.SearchStr))
            {
                data = data.Where(x => x.SaleProductName.Contains(searchProductContext.SearchStr.Trim()) || x.ChannelName.Contains(searchProductContext.SearchStr.Trim()) || x.SaleProductCode.Contains(searchProductContext.SearchStr.Trim())).ToList();
            }
            data = data.OrderByDescending(x => x.ChannelName).ThenByDescending(x => x.Stock).ToList();
            return new PageList<SaleProductPriceList>(data, searchProductContext.PageIndex, searchProductContext.PageSize);
        }
        /// <summary>
        /// 通过销售商品ID获取销售商品价格
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<SaleProductPriceList> GetSaleProductPriceBySaleProductId(int id)
        {
            #region
            var getSaleProductId = from s in _omsAccessor.Get<SaleProductPrice>()
                                   where s.Isvalid && s.SaleProductId == id
                                   join dd in _omsAccessor.Get<Dictionary>() on s.CustomerTypeId equals dd.Id
                                   join sd in _omsAccessor.Get<Dictionary>() on s.SaleProduct.Channel equals sd.Id
                                   join p in _omsAccessor.Get<Product>() on s.SaleProduct.ProductId equals p.Id
                                   select new
                                   {
                                       s.Id,
                                       s.SaleProductId,
                                       ProductName = p.Name,
                                       s.SaleProduct.Channel,
                                       ChannelName = sd.Value,
                                       s.SaleProduct.Stock,
                                       s.SaleProduct.LockStock,
                                       s.SaleProduct.AvailableStock,
                                       s.CustomerTypeId,
                                       CustomerTypeName = dd.Value,
                                       s.Price
                                   } into gs
                                   group gs by gs.SaleProductId;
            #endregion
            List<SaleProductPriceList> data = new List<SaleProductPriceList>();
            foreach (var item in getSaleProductId)
            {
                var a = item.Key;
                var c = item.ToList();
                List<SaleProductPriceBaseList> list = new List<SaleProductPriceBaseList>();
                SaleProductPriceList saleProductPriceList = new SaleProductPriceList();
                foreach (var i in c)
                {
                    SaleProductPriceBaseList sppb = new SaleProductPriceBaseList();
                    sppb.Id = i.Id;
                    sppb.SaleProductId = i.SaleProductId;
                    sppb.ProductName = i.ProductName;
                    sppb.Channel = i.Channel;
                    sppb.ChannelName = i.ChannelName;
                    sppb.CustomerTypeId = i.CustomerTypeId;
                    sppb.CustomerTypeName = i.CustomerTypeName;
                    sppb.Stock = i.Stock;
                    sppb.LockStock = i.LockStock;
                    sppb.AvailableStock = i.AvailableStock;
                    sppb.Price = i.Price;
                    list.Add(sppb);
                }
                saleProductPriceList.Id = list.FirstOrDefault().Id;
                saleProductPriceList.SaleProductId = list.FirstOrDefault().ProductId;
                saleProductPriceList.SaleProductId = list.FirstOrDefault().SaleProductId;
                saleProductPriceList.ChannelName = list.FirstOrDefault().ChannelName;
                saleProductPriceList.SaleProductName = list.FirstOrDefault().ProductName;
                saleProductPriceList.Stock = list.FirstOrDefault().Stock;
                saleProductPriceList.LockStock = list.FirstOrDefault().LockStock;
                saleProductPriceList.AvailableStock = list.FirstOrDefault().AvailableStock;
                saleProductPriceList.SaleProductPriceBaseList = list;
                data.Add(saleProductPriceList);
            }
            return data;
        }
        /// <summary>
        /// 通过销售商品价格ID获取销售商品价格（IQueryable）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable GetSaleProductPriceById(int id)
        {
            var saleproductprice = from s in _omsAccessor.Get<SaleProductPrice>()
                                   join sp in _omsAccessor.Get<SaleProduct>() on s.SaleProductId equals sp.Id
                                   join d in _omsAccessor.Get<Dictionary>() on s.CustomerTypeId equals d.Id
                                   join sd in _omsAccessor.Get<Dictionary>() on sp.Channel equals sd.Id
                                   where s.Isvalid && s.Id == id
                                   select new
                                   {
                                       s.Id,
                                       s.SaleProductId,
                                       SaleProductName = sp.Product.Name,
                                       ChannelName = sd.Value,
                                       s.CustomerTypeId,
                                       CustomerTypeName = d.Value,
                                       s.Price,
                                       s.Isvalid,
                                       s.ModifiedBy,
                                       s.CreatedBy,
                                       s.ModifiedTime,
                                       s.CreatedTime
                                   };
            return saleproductprice;
        }
        /// <summary>
        /// 通过销售商品ID、价格类型获取销售商品价格
        /// </summary>
        /// <param name="SaleProductId"></param>
        /// <param name="CustomerTypeId"></param>
        /// <returns></returns>
        public SaleProductPrice GetSaleProductPriceById(int SaleProductId, int? CustomerTypeId)
        {
            var saleproductprice = _omsAccessor.Get<SaleProductPrice>().Where(p => p.SaleProductId == SaleProductId && p.CustomerTypeId == CustomerTypeId).FirstOrDefault();
            return saleproductprice;
        }
        /// <summary>
        /// 添加销售商品价格
        /// </summary>
        /// <param name="saleProductPrice"></param>
        public void CreateSaleProductsPrice(SaleProductPrice saleProductPrice)
        {


            saleProductPrice.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert<SaleProductPrice>(saleProductPrice);
            _omsAccessor.SaveChanges();

        }
        /// <summary>
        /// 修改销售商品价格
        /// </summary>
        /// <param name="saleProductPrice"></param>
        public void UpdateSaleProductPrice(SaleProductPrice saleProductPrice)
        {
            saleProductPrice.ModifiedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Update<SaleProductPrice>(saleProductPrice);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 通过销售商品Id获取销售商品标准原价
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public decimal GetOriginalPriceBySaleProductId(int id)
        {
            var priceType = _omsAccessor.Get<Dictionary>().Where(r => r.Isvalid && r.Type == DictionaryType.PriceType && r.Value.Contains("标准价")).First();
            if (priceType != null)
            {
                var prices = _omsAccessor.Get<SaleProductPrice>().Where(r => r.Isvalid && r.SaleProductId == id && r.CustomerTypeId == priceType.Id);
                return prices == null ? 0 : prices.FirstOrDefault().Price;
            }
            return 0;
        }
        #endregion


        #region 平台商品
        /// <summary>
        /// 获取全部平台商品
        /// </summary>
        public IQueryable GetAllPlantformProducts()
        {
            var platformproducts = from p in _omsAccessor.Get<PlatformProduct>()
                                   join sp in _omsAccessor.Get<SaleProduct>() on p.SaleProductId equals sp.Id
                                   join d in _omsAccessor.Get<Dictionary>() on p.PlatForm equals d.Id
                                   join v in _omsAccessor.Get<Dictionary>() on sp.Channel equals v.Id
                                   select new
                                   {
                                       p.Id,
                                       p.SaleProductId,
                                       ChannelName = v.Value,
                                       ProductName = sp.Product.Name,
                                       p.PlatForm,
                                       PlatFormName = d.Value,
                                       p.PlatFormProductCode
                                   };
            return platformproducts;
        }
        /// <summary>
        /// 添加平台商品
        /// </summary>
        /// <param name="platformProduct"></param>
        public void CreatedPlatformProduct(PlatformProduct platformProduct)
        {
            if (platformProduct == null)
                throw new ArgumentException("PlatformProduct");
            platformProduct.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert<PlatformProduct>(platformProduct);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 检测平台商品是否存在
        /// </summary>
        /// <param name="SaleProductId"></param>
        /// <param name="PlatForm"></param>
        /// <returns></returns>
        public bool ConfirmPlatformProductIfExist(int SaleProductId, int PlatForm)
        {
            var exist = _omsAccessor.Get<PlatformProduct>().Where(p => p.Isvalid && p.SaleProductId == SaleProductId && p.PlatForm == PlatForm);
            if (exist.Count() > 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 通过平台商品ID获取平台商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PlatformProduct GetPlatformProductById(int id)
        {
            var platformproduct = _omsAccessor.Get<PlatformProduct>().Where(p => p.Isvalid && p.Id == id).FirstOrDefault();
            return platformproduct;
        }
        /// <summary>
        /// 删除平台商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DelPlatformProduct(int id)
        {
            _omsAccessor.DeleteById<PlatformProduct>(id);
            _omsAccessor.SaveChanges();
            return true;
        }
        #endregion


        #region 套装商品

        #region 套装商品
        public IEnumerable<SuitProducts> GetAllSuitProducts()
        {
            return _omsAccessor.Get<SuitProducts>().Where(r => r.Isvalid).ToList();
        }
        public SuitProducts InsertSuitProducts(SuitProducts suitPro)
        {
            _omsAccessor.Insert<SuitProducts>(suitPro);
            _omsAccessor.SaveChanges();
            return suitPro;
        }
        public SuitProducts GetSuitProductsBySuitProId(int suitProId)
        {
            var result = _omsAccessor.Get<SuitProducts>().Where(r => r.Isvalid && r.Id == suitProId).FirstOrDefault();
            return result;
        }
        public SuitProducts UpdateSuitProducts(SuitProducts suitPro)
        {
            var data = _omsAccessor.Get<SuitProducts>().Where(r => r.Isvalid && r.Id == suitPro.Id).FirstOrDefault();
            data.Name = suitPro.Name.Trim();
            data.Code = suitPro.Code.Trim();
            data.Mark = suitPro.Mark;
            _omsAccessor.Update<SuitProducts>(data);
            _omsAccessor.SaveChanges();
            return data;
        }
        public bool DeleteSuitProductsById(int suitProId)
        {
            try
            {
                var data = _omsAccessor.Get<SuitProducts>().Where(r => r.Id == suitProId).FirstOrDefault();
                var pro = _omsAccessor.Get<SuitProductsDetail>().Where(r => r.SuitProductsId == suitProId).ToList();
                foreach (var item in pro)
                {
                    _omsAccessor.Delete<SuitProductsDetail>(item);
                }
                _omsAccessor.Delete<SuitProducts>(data);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                _logService.Error("<DeleteSuitProductsById> 信息：" + e.Message);
                return false;
            }
        }
        #endregion


        #region 套装商品详情
        public IEnumerable<SuitProductsDetail> GetAllSuitProductsDetail()
        {
            return _omsAccessor.Get<SuitProductsDetail>().Where(r => r.Isvalid).ToList();
        }
        public SuitProductsDetail InsertSuitProductsDetail(SuitProductsDetail suitProDetail)
        {
            var salePro = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == suitProDetail.SaleProductId).FirstOrDefault();
            suitProDetail.ProductId = salePro.ProductId;
            _omsAccessor.Insert<SuitProductsDetail>(suitProDetail);
            _omsAccessor.SaveChanges();
            return suitProDetail;
        }
        public SuitProductsDetail UpdateSuitProductsDetail(SuitProductsDetail suitProDetail)
        {
            var data = _omsAccessor.Get<SuitProductsDetail>().Where(r => r.Id == suitProDetail.Id).FirstOrDefault();
            data.SaleProductId = suitProDetail.SaleProductId;
            data.ProductId = suitProDetail.ProductId;
            data.Quantity = suitProDetail.Quantity;
            _omsAccessor.Update<SuitProductsDetail>(data);
            _omsAccessor.SaveChanges();
            return data;
        }
        public bool DeleteSuitProductsDetailById(int suitProDetailId)
        {
            try
            {
                var data = _omsAccessor.Get<SuitProductsDetail>().Where(r => r.Id == suitProDetailId).FirstOrDefault();
                _omsAccessor.Delete<SuitProductsDetail>(data);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                _logService.Error("<DeleteSuitProductsDetailById> 信息：" + e.Message);
                return false;
            }

        }
        public List<SuitProducts> GetSuitProductsByNameOrCode(SuitProducts suitProducts)
        {
            var data = _omsAccessor.Get<SuitProducts>().Where(r => r.Name == suitProducts.Name || r.Code == suitProducts.Code).ToList();
            return data;
        }
        public SuitProductsModel GetSuitProductsById(int suitProId)
        {
            var data = _omsAccessor.Get<SuitProducts>().Where(r => r.Isvalid && r.Id == suitProId).FirstOrDefault();
            var pros = _omsAccessor.Get<SuitProductsDetail>().Where(r => r.SuitProductsId == data.Id).Join(_omsAccessor.Get<Product>(), s => s.ProductId, p => p.Id, (s, p) => new SuitProductsDetailModel
            {
                Id = s.Id,
                SaleProductId = s.SaleProductId,
                ProductId = s.ProductId,
                Quantity = s.Quantity,
                ProductName = p.Name,
                ProductCode = p.Code
            }).ToList();
            SuitProductsModel model = new SuitProductsModel();
            model.Id = data.Id;
            model.Name = data.Name;
            model.Code = data.Code;
            model.SuitProductsDetail = pros;
            return model;
        }
        public SuitProductsDetail GetSuitProductsDetailById(int suitProId, int saleProId)
        {
            var data = _omsAccessor.Get<SuitProductsDetail>().Where(r => r.SuitProductsId == suitProId && r.SaleProductId == saleProId).FirstOrDefault();
            return data;
        }
        public SuitProductsDetail GetSuitProductsDetail(int suitProDetailId)
        {
            return _omsAccessor.Get<SuitProductsDetail>().Where(r => r.Id == suitProDetailId).FirstOrDefault();
        }

        #endregion
        #endregion


        #region 同步相关
        /// <summary>
        /// 采购酒款（商品）同步
        /// </summary>
        /// <param name="wineDtos"></param>
        public dynamic PMSynchronize(List<WineDto> wineDtos)
        {
            var existCode = _omsAccessor.Get<Product>(p => p.Isvalid).Select(p => p.Code).ToList();
            var toBeInserts = wineDtos.Where(w => !existCode.Contains(w.Code)).ToList();
            List<string> errCode = new List<string>();
            foreach (var wine in toBeInserts)
            {
                var product = new Product
                {
                    Name = wine.Year + "年" + wine.Name,
                    NameEn = wine.Year + " " + wine.NameEn,
                    Year = wine.Year,
                    Code = wine.Code
                };
                _omsAccessor.Insert(product);
            }
            try
            {
                _omsAccessor.SaveChanges();
                return new { state = "ok" };
            }
            catch (Exception ex)//批量出错，改为逐条插入并记录了出错的产品编码
            {
                _logger.LogError(ex, "采购同步出错");
                foreach (var wine in toBeInserts)
                {
                    var product = new Product
                    {
                        Name = wine.Year + "年" + wine.Name,
                        NameEn = wine.Year + " " + wine.NameEn,
                        Year = wine.Year,
                        Code = wine.Code
                    };
                    _omsAccessor.Insert(product);
                    try
                    {
                        _omsAccessor.SaveChanges();
                    }
                    catch (Exception ex2)
                    {
                        errCode.Add(wine.Code);
                        _logger.LogError(ex2, "采购同步出错记录");
                        continue;
                    }
                }
                return new { state = "err", errCode };
            }
        }

        /// <summary>
        /// OMS所有商品库存同步
        /// </summary>
        /// <param name="saleProductSyncModel"></param>
        /// <returns></returns>
        public dynamic SyncSaleProductStock(List<SaleProductWareHouseStockSyncModel> saleProWareHouseStocks)
        {
            List<string> spwhsIdError = new List<string>();
            foreach (var item in saleProWareHouseStocks)//更新SaleProductWareHouseStock表的记录
            {
                try
                {
                    var saleProduct = _omsAccessor.Get<SaleProduct>().Where(r => r.Isvalid == true && r.ProductId == item.OMSId).FirstOrDefault(); //根据WMS中的库存记录的商品OMSId，找到OMS.SaleProduct中的相对应的可售商品
                    int saleProductId;
                    if (saleProduct == null)//在OMS的SaleProduct表中找不到WMS的Stock中Product对应的商品
                    {
                        spwhsIdError.Add(item.Id.ToString());
                        continue;
                    }
                    else
                    {
                        saleProductId = saleProduct.Id;
                    }
                    var wareHouseId = _omsAccessor.Get<WareHouse>().Where(r => r.Code == item.WareHouseCode).Select(r => r.Id).FirstOrDefault();//根据WMS中的仓库Code找到OMS中相应的仓库ID
                    var saleProWareHouseStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.Isvalid == true && r.SaleProductId == saleProductId && r.WareHouseId == wareHouseId).FirstOrDefault();
                    if (saleProWareHouseStock != null)
                    {
                        saleProWareHouseStock.Stock = item.Stock;
                        saleProWareHouseStock.ModifiedTime = DateTime.Now;
                        _omsAccessor.Update<SaleProductWareHouseStock>(saleProWareHouseStock);
                    }
                    else
                    {
                        var spWareHouseStock = new SaleProductWareHouseStock();
                        spWareHouseStock.SaleProductId = saleProductId;
                        spWareHouseStock.WareHouseId = wareHouseId;
                        spWareHouseStock.Stock = item.Stock;
                        _omsAccessor.Insert<SaleProductWareHouseStock>(spWareHouseStock);
                    }
                }
                catch (Exception ex)
                {
                    spwhsIdError.Add(item.Id.ToString()); //记录更新出错商品在WMS.Stock表中的的Id
                    _logger.LogError(ex, "更新可售商品各个仓库库存出错记录！");
                }
            }
            _omsAccessor.SaveChanges();

            List<string> saleProError = new List<string>();
            var saleProducts = _omsAccessor.Get<SaleProduct>().Where(r => r.Isvalid == true);
            foreach (var saleProduct in saleProducts)
            {
                try
                {
                    //根据SaleProductId在SaleProductWareHouseStock表中获取总库存和锁定的总库存
                    var totalStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProduct.Id && r.Isvalid == true).GroupBy(r => r.SaleProductId).Select(g => g.Sum(t => t.Stock)).FirstOrDefault();
                    var totalLockStock = saleProWareHouseStocks.Where(r => r.OMSId == saleProduct.ProductId).GroupBy(r => r.OMSId).Select(g => g.Sum(t => t.Stock)).FirstOrDefault();
                    saleProduct.Stock = totalStock;
                    saleProduct.LockStock = totalLockStock;
                    saleProduct.AvailableStock = totalStock - totalLockStock;
                    _omsAccessor.Update<SaleProduct>(saleProduct);
                }
                catch (Exception ex)
                {
                    saleProError.Add(saleProduct.Id.ToString());//记录更新库存出错的可售商品的Id
                    _logger.LogError(ex, "更新可售商品总库存出错记录！");
                }
            }
            _omsAccessor.SaveChanges();
            return new { state = "success", spwhsIdError, saleProError };
        }

        /// <summary>
        /// OMS单个商品库存同步
        /// </summary>
        /// <param name="saleProWareHouseStocks"></param>
        /// <returns></returns>
        public dynamic SyncSingleSaleProductStock(List<SaleProductWareHouseStockSyncModel> saleProWareHouseStocks)
        {
            List<string> spwhsIdError = new List<string>();
            int saleProductId = 0;
            foreach (var item in saleProWareHouseStocks)//更新SaleProductWareHouseStock表的记录
            {
                try
                {
                    var _saleProduct = _omsAccessor.Get<SaleProduct>().Where(r => r.Isvalid == true && r.ProductId == item.OMSId).FirstOrDefault();//根据WMS中的库存记录的商品OMSId，找到OMS.SaleProduct中的相对应的可售商品
                    if (_saleProduct == null)//在OMS的SaleProduct表中找不到WMS的Stock中Product对应的商品
                    {
                        spwhsIdError.Add(item.Id.ToString());
                        continue;
                    }
                    else
                    {
                        saleProductId = _saleProduct.Id;
                    }
                    var wareHouseId = _omsAccessor.Get<WareHouse>().Where(r => r.Code == item.WareHouseCode).Select(r => r.Id).FirstOrDefault();//根据WMS中的仓库Code找到OMS中相应的仓库ID
                    var saleProWareHouseStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.Isvalid == true && r.SaleProductId == saleProductId && r.WareHouseId == wareHouseId).FirstOrDefault();
                    if (saleProWareHouseStock != null)
                    {
                        saleProWareHouseStock.Stock = item.Stock;
                        saleProWareHouseStock.ModifiedTime = DateTime.Now;
                        _omsAccessor.Update<SaleProductWareHouseStock>(saleProWareHouseStock);
                    }
                    else
                    {
                        var spWareHouseStock = new SaleProductWareHouseStock();
                        spWareHouseStock.SaleProductId = saleProductId;
                        spWareHouseStock.WareHouseId = wareHouseId;
                        spWareHouseStock.Stock = item.Stock;
                        _omsAccessor.Insert<SaleProductWareHouseStock>(spWareHouseStock);
                    }
                }
                catch (Exception ex)
                {
                    spwhsIdError.Add(item.Id.ToString());
                    _logger.LogError(ex, "更新可售商品仓库库存出错记录！");
                }

            }
            _omsAccessor.SaveChanges();

            List<string> saleProError = new List<string>();
            if (saleProductId != 0)
            {
                try
                {
                    var saleProduct = _omsAccessor.GetById<SaleProduct>(saleProductId);
                    //根据SaleProductId在SaleProductWareHouseStock表中获取总库存和锁定的总库存
                    var totalStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProduct.Id && r.Isvalid == true).GroupBy(r => r.SaleProductId).Select(g => g.Sum(t => t.Stock)).FirstOrDefault();
                    var totalLockStock = saleProWareHouseStocks.Where(r => r.OMSId == saleProduct.ProductId).GroupBy(r => r.OMSId).Select(g => g.Sum(t => t.LockStock)).FirstOrDefault();
                    saleProduct.Stock = totalStock;
                    saleProduct.LockStock = totalLockStock;
                    saleProduct.AvailableStock = totalStock - totalLockStock;
                    _omsAccessor.Update<SaleProduct>(saleProduct);

                    _omsAccessor.SaveChanges();
                }
                catch (Exception ex)
                {
                    saleProError.Add(saleProductId.ToString());
                    _logger.LogError(ex, "更新可售商品仓库库存出错记录！");
                }
            }

            return new { state = "success", spwhsIdError, saleProError };
        }

        /// <summary>
        /// 根据商品Id获取该商品在WMS中总库存
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public string GetWMSProdcutStockByProductId(int productId)
        {
            var product = _omsAccessor.GetById<Product>(productId);
            if (product == null)
                return (new { isSucc = false, msg = "发生错误，未找到该商品！" }).ToString();

            //处理SSL问题
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using (var http = new HttpClient(httpClientHandler))
            {
                var content = new StringContent(product.Id.ToString(), Encoding.UTF8, "application/json");
                var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wmsapi/ProductSync/GetWMSProductTotalStock";

                #region JWTBearer授权信息
                _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                #endregion
                var response = http.PostAsync(requestUrl, content).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = response.Content.ReadAsStringAsync();
                    return result.Result.ToString();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logService.Error("获取WMS商品总库存失败，原因是API授权失败！请重试。");
                    return (new { isSucc = false, msg = "获取WMS商品总库存失败，原因是API授权失败！请重试。" }).ToString();
                }
                else
                {
                    _logService.Error(string.Format("获取WMS商品总库存失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                    return (new { isSucc = false, msg = string.Format("获取WMS商品总库存失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage) }).ToString();
                }
            }
        }
        /// <summary>
        /// 同步商品可用单个商品库存到商城
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="code"></param>
        /// <param name="quantity"></param>
        public async void SyncProductStockToAssist(int shopid, string code, int quantity)
        {
            try
            {
                var productStockDataList = new List<ProductStockData>();
                var productStockData = new ProductStockData
                {
                    sd_id = shopid.ToString(),
                    goods_sn = code,
                    stock_num = quantity.ToString(),
                    stock_detail_list = GetSaleProductWareHouseStocksByProductCode(code)
                };
                productStockDataList.Add(productStockData);
                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    var content = new StringContent(productStockDataList.ToJson(), System.Text.Encoding.UTF8, "application/json");
                    var postUrl = _configuration.GetSection("OrderAssistOmsApi")["domain"] + "/ProductStockFeed";
                    var response = http.PostAsync(postUrl, content).Result;
                    var result = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var data = JsonMapper.ToObject<StockFeedbackResponse>(result);
                        if (data.state)
                        {
                            var orderNotificationResults = data.resp_data.feedback_result_list;
                            var noSuccNotific = new List<StockFeedBackResult>();
                            if (orderNotificationResults != null && orderNotificationResults.Count > 0)
                            {
                                noSuccNotific = orderNotificationResults.Where(r => r.result != "1").ToList();
                            }
                            if (noSuccNotific != null && noSuccNotific.Count() > 0)
                            {
                                var order_snStr = string.Empty;
                                foreach (var item in noSuccNotific)
                                {
                                    order_snStr += string.Format("商铺Id:{0},商品Id{1};", item.sd_id, item.goods_sn);
                                }

                                _logService.Error(string.Format("OMS库存变更通知信息异常：{0}", order_snStr));
                            }
                        }
                        else
                        {
                            _logService.Error(string.Format("OMS库存更新通知信息异常,商品编码：{0},错误信息：{1}", code, data.message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                _logService.Error("OMS库存更新通知信息异常：" + ex.Message);
            }

        }

        /// <summary>
        /// 同步商品可用多个个商品库存到商城
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="code"></param>
        /// <param name="quantity"></param>
        public async void SyncMoreProductStockToAssist(List<ProductStockData> productStockDataList)
        {
            try
            {
                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    var content = new StringContent(productStockDataList.ToJson(), System.Text.Encoding.UTF8, "application/json");
                    var postUrl = _configuration.GetSection("OrderAssistOmsApi")["domain"] + "/ProductStockFeed";
                    var response = http.PostAsync(postUrl, content).Result;
                    var result = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var data = JsonMapper.ToObject<StockFeedbackResponse>(result);
                        if (data.state)
                        {
                            var orderNotificationResults = data.resp_data.feedback_result_list;
                            var noSuccNotific = new List<StockFeedBackResult>();
                            if (orderNotificationResults != null && orderNotificationResults.Count > 0)
                            {
                                noSuccNotific = orderNotificationResults.Where(r => r.result != "1").ToList();
                            }
                            if (noSuccNotific != null && noSuccNotific.Count() > 0)
                            {
                                var order_snStr = string.Empty;
                                foreach (var item in noSuccNotific)
                                {
                                    order_snStr += string.Format("商铺Id：{0},商品Id：{1};", item.sd_id, item.goods_sn);
                                }

                                _logService.Error(string.Format("OMS库存变更通知信息异常：{0}", order_snStr));
                            }
                        }
                        else
                        {
                            _logService.Error(string.Format("OMS库存更新通知信息异常错误信息：{0}", data.message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                _logService.Error("OMS库存更新通知信息异常：" + ex.Message);
            }

        }
        #endregion


        #region 销售商品仓库库存
        public SaleProductWareHouseStock CreateSaleProductWareHouseStock(SaleProductWareHouseStock saleProdctWHStock)
        {
            _omsAccessor.Insert<SaleProductWareHouseStock>(saleProdctWHStock);
            _omsAccessor.SaveChanges();
            return saleProdctWHStock;
        }
        public SaleProductWareHouseStock UpdateSaleProductWareHouseStock(SaleProductWareHouseStock saleProdctWHStock)
        {
            var data = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.Id == saleProdctWHStock.Id).FirstOrDefault();
            data.SaleProductId = saleProdctWHStock.SaleProductId;
            data.WareHouseId = saleProdctWHStock.WareHouseId;
            data.ProductId = saleProdctWHStock.ProductId;
            data.Stock = saleProdctWHStock.Stock;
            data.LockStock = saleProdctWHStock.LockStock;
            _omsAccessor.Update<SaleProductWareHouseStock>(data);
            _omsAccessor.SaveChanges();
            return data;
        }
        public SaleProductWareHouseStock GetSaleProductWareHouseStockById(int saleProId, int wareHouseId)
        {
            var result = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProId && r.WareHouseId == wareHouseId).FirstOrDefault();
            return result;
        }
        public IEnumerable<SaleProductWareHouseStock> GetSaleProductStocksBySaleProductId(int saleProductId)
        {
            var data = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProductId).Include(r => r.SaleProduct).ThenInclude(r => r.Product).Include(r => r.WareHouse).ToList();
            return data;
        }
        public IEnumerable<SaleProductWareHouseStockModel> GetSaleProductStocksByProductId(int productId)
        {
            var data = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.ProductId == productId).Include(r => r.SaleProduct).ThenInclude(r => r.Product).Include(r => r.WareHouse)
                .Select(r => new SaleProductWareHouseStockModel
                {
                    Id = r.Id,
                    SaleProductId = r.SaleProductId,
                    ProductId = r.SaleProduct.ProductId,
                    ProductCode = r.SaleProduct.Product.Code,
                    ProductName = r.SaleProduct.Product.Name,
                    WareHouseId = r.WareHouseId,
                    WareHouseName = r.WareHouse.Name,
                    WareHouseCode = r.WareHouse.Code,
                    Stock = r.Stock,
                    LockStock = r.LockStock
                }).ToList();
            return data;
        }
        /// <summary>
        /// 通过出库编码和商品ID查询商品仓库库存
        /// </summary>
        /// <returns></returns>
        public SaleProductWareHouseStock GetSPHStockByWareHouseCodeAndProduct(string wareHouseCode, int productId)
        {

            var data = new SaleProductWareHouseStock();
            var wareHouse = _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.Code == wareHouseCode.Trim()).FirstOrDefault();
            if (wareHouse == null)
            {
                return data;
            }
            data = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.ProductId == productId && r.WareHouseId == wareHouse.Id && r.Isvalid).FirstOrDefault();
            if (data == null)
            {
                data = new SaleProductWareHouseStock();
                data.Stock = 0;
                data.LockStock = 0;
                data.ProductId = productId;
                data.SaleProductId = _omsAccessor.Get<SaleProduct>().Where(x => x.ProductId == productId && x.Isvalid).FirstOrDefault().Id;
                data.WareHouseId = wareHouse.Id;
                _omsAccessor.Insert(data);
                _omsAccessor.SaveChanges();
            }
            return data;
        }

        /// <summary>
        /// 通过商品编码获取商品仓库库存
        /// (同步商品库存到商城查询用到)
        /// </summary>
        /// <returns></returns>
        public List<StockFeedBackDataDetail> GetSaleProductWareHouseStocksByProductCode(string productCode)
        {
            var data = new List<StockFeedBackDataDetail>();
            data = (from spwh in _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid)
                    join p in _omsAccessor.Get<Product>().Where(x => x.Isvalid && x.Code == productCode) on spwh.ProductId equals p.Id
                    join wh in _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.IsSyncStock) on spwh.WareHouseId equals wh.Id
                    select new StockFeedBackDataDetail
                    {
                        store_code = wh.Code,
                        store_name = wh.Name,
                        stock_num = (spwh.Stock - spwh.LockStock) < 0 ? "0" : (spwh.Stock - spwh.LockStock).ToString()
                    }).ToList();
            return data;
        }
        #endregion


        #region 销售商品锁定查询
        public SaleProductLockedTrack CreateSaleProductLockedTrack(SaleProductLockedTrack saleProductLockedTrack)
        {
            _omsAccessor.Insert<SaleProductLockedTrack>(saleProductLockedTrack);
            _omsAccessor.SaveChanges();
            return saleProductLockedTrack;
        }
        public SaleProductLockedTrack UpdateSaleProductLockedTrack(SaleProductLockedTrack saleProductLockedTrack)
        {
            var data = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.Id == saleProductLockedTrack.Id).FirstOrDefault();
            data.OrderId = saleProductLockedTrack.OrderId;
            data.OrderSerialNumber = saleProductLockedTrack.OrderSerialNumber;
            data.ProductId = saleProductLockedTrack.ProductId;
            data.SaleProductId = saleProductLockedTrack.SaleProductId;
            data.LockNumber = saleProductLockedTrack.LockNumber;
            data.WareHouseId = saleProductLockedTrack.WareHouseId;
            _omsAccessor.Update<SaleProductLockedTrack>(data);
            _omsAccessor.SaveChanges();
            return data;
        }
        public SaleProductLockedTrack GetSaleProductLockedTrackById(int orderId, int saleProId, int? orderProId = null)
        {
            var result = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == orderId && r.SaleProductId == saleProId && (!orderProId.HasValue || r.OrderProductId == orderProId)).FirstOrDefault();
            return result;
        }
        public IEnumerable<SaleProductLockedTrackModel> GetAllSaleProductLockedTrackBySaleProId(int saleProductId)
        {
            //可显示缺货订单信息状态
            var canOrderStates = new OrderState[] {
                OrderState.B2CConfirmed,
                OrderState.CheckAccept,
                OrderState.Confirmed,
                OrderState.FinancialConfirmation,
                OrderState.Paid,
                OrderState.ToBeConfirmed,
                OrderState.Unpaid,
                OrderState.ToBeTurned,
                OrderState.Unshipped,
                OrderState.Uploaded
            };
            var data = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.SaleProductId == saleProductId && r.Isvalid)
                .Join(_omsAccessor.Get<SaleProduct>(), r => r.SaleProductId, s => s.Id, (r, s) => new { r, s })
                .Join(_omsAccessor.Get<Product>(), r => r.s.ProductId, p => p.Id, (r, p) => new { r, p })
                .Join(_omsAccessor.Get<WareHouse>(), r => r.r.r.WareHouseId, w => w.Id, (r, w) => new { r, w })
                .Join(_omsAccessor.Get<Order>().Where(o => canOrderStates.Contains(o.State)), r => r.r.r.r.OrderId, o => o.Id, (r, o) => new { r, o })
                .Select(r => new SaleProductLockedTrackModel
                {
                    OrderId = r.r.r.r.r.OrderId,
                    OrderSerialNumber = r.r.r.r.r.OrderSerialNumber,
                    ProductId = r.r.r.r.r.ProductId,
                    ProductName = r.r.r.p.Name,
                    ProductCode = r.r.r.p.Code,
                    WareHouseId = r.r.r.r.r.WareHouseId,
                    WareHouseName = r.r.w.Name,
                    WareHouseCode = r.r.w.Code,
                    LockNumber = r.r.r.r.r.LockNumber
                }).Where(r => r.LockNumber != 0).ToList();
            return data;
        }
        /// <summary>
        /// 添加锁定商品信息到锁定跟踪表以及OMS仓库库存信息表
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="saleProductId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="lockedNumber"></param>
        /// <returns></returns>
        public string AddSaleProductLockedTrackAndWareHouseStock(int orderId, int saleProductId, int wareHouseId, int lockedNumber, int? orderProId)
        {
            using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    var result = CreateSaleProductLockedTrackAndWareHouseStock(orderId, saleProductId, wareHouseId, lockedNumber, orderProId);
                    trans.Commit();
                    return "成功！";
                }
                catch (Exception e)
                {
                    _logService.Error("<AddSaleProductLockedTrackAndWareHouseStock>" + e.Message + " 位置：" + e.StackTrace);
                    trans.Rollback();
                    return "更新锁定库存跟踪及仓库OMS库存锁定失败！";
                }
            }
        }
        public bool CreateSaleProductLockedTrackAndWareHouseStock(int orderId, int saleProductId, int wareHouseId, int lockedNumber, int? orderProId)
        {
            var saleProduct = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == saleProductId).FirstOrDefault();
            var saleProWHStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProductId && r.WareHouseId == wareHouseId).FirstOrDefault();
            var order = _omsAccessor.Get<Order>().Where(r => r.Id == orderId).FirstOrDefault();
            var orderPro = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == order.Id && r.SaleProductId == saleProductId && (!orderProId.HasValue || r.Id == orderProId)).ToList();
            if (saleProWHStock == null)
            {
                //对没记录的商品进行初始化
                SaleProductWareHouseStock SPWHS = new SaleProductWareHouseStock();
                SPWHS.SaleProductId = saleProduct.Id;
                SPWHS.WareHouseId = wareHouseId;
                SPWHS.ProductId = saleProduct.ProductId;
                SPWHS.Stock = 0;
                SPWHS.LockStock = 0;
                saleProWHStock = CreateSaleProductWareHouseStock(SPWHS);
            }
            foreach (var item in orderPro)
            {
                var saleProLocked = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == orderId && r.SaleProductId == saleProductId && r.OrderProductId == item.Id).FirstOrDefault();
                var avaStock = saleProWHStock.Stock - saleProWHStock.LockStock;
                //锁定库存跟踪
                if (saleProLocked == null && lockedNumber >= 0)
                {
                    //表中没有数据且锁定数大于0
                    SaleProductLockedTrack newSaleProLT = new SaleProductLockedTrack();
                    newSaleProLT.OrderId = order.Id;
                    newSaleProLT.OrderSerialNumber = order.SerialNumber;
                    newSaleProLT.ProductId = saleProduct.ProductId;
                    newSaleProLT.SaleProductId = saleProduct.Id;
                    newSaleProLT.OrderProductId = item.Id;
                    newSaleProLT.WareHouseId = wareHouseId;
                    if (avaStock >= lockedNumber)
                    {
                        newSaleProLT.LockNumber = lockedNumber;
                        saleProWHStock.LockStock += lockedNumber;
                    }
                    else if (avaStock < lockedNumber && avaStock > 0)
                    {
                        newSaleProLT.LockNumber = avaStock;
                        saleProWHStock.LockStock += avaStock;
                    }
                    _omsAccessor.Insert<SaleProductLockedTrack>(newSaleProLT);
                }
                else if (saleProLocked == null && lockedNumber < 0)
                {
                    //表中没有数据且锁定数小于0（删除商品）
                    saleProWHStock.LockStock += lockedNumber;
                }
                else
                {
                    //表中有数据
                    if (avaStock >= lockedNumber)
                    {
                        saleProLocked.LockNumber += lockedNumber;
                        saleProWHStock.LockStock += lockedNumber;
                    }
                    else if (avaStock < lockedNumber && avaStock > 0)
                    {
                        saleProLocked.LockNumber += avaStock;
                        saleProWHStock.LockStock += avaStock;
                    }
                    _omsAccessor.Update<SaleProductLockedTrack>(saleProLocked);
                }
                _omsAccessor.Update<SaleProductWareHouseStock>(saleProWHStock);
            }
            _omsAccessor.SaveChanges();
            return true;
        }
        /// <summary>
        /// 修改订单商品更新库存锁定及库存跟踪
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="saleProductId"></param>
        /// <param name="oldSaleProSumNum"></param>
        /// <returns></returns>
        public string UpdateOrderProChangeLockedLog(int orderId, int saleProductId)
        {
            using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    var order = _omsAccessor.Get<Order>().Where(r => r.Id == orderId).FirstOrDefault();
                    if (order.WarehouseId == 0)
                    {
                        order.WarehouseId = _omsAccessor.Get<WareHouse>().FirstOrDefault().Id;
                        _omsAccessor.Update<Order>(order);
                        _omsAccessor.SaveChanges();
                    }
                    var orderProducts = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == order.Id && r.SaleProductId == saleProductId).ToList();
                    var salePro = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == saleProductId).FirstOrDefault();
                    foreach (var item in orderProducts)
                    {
                        var saleProWHStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProductId && r.WareHouseId == order.WarehouseId).FirstOrDefault();
                        if (saleProWHStock == null)
                        {
                            //对没记录的商品进行初始化
                            SaleProductWareHouseStock SPWHS = new SaleProductWareHouseStock();
                            SPWHS.SaleProductId = saleProductId;
                            SPWHS.WareHouseId = order.WarehouseId;
                            SPWHS.ProductId = salePro.ProductId;
                            SPWHS.Stock = 0;
                            SPWHS.LockStock = 0;
                            CreateSaleProductWareHouseStock(SPWHS);
                            saleProWHStock = SPWHS;
                        }
                        var avaStock = saleProWHStock.Stock - saleProWHStock.LockStock;
                        var saleProLT = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == orderId && r.SaleProductId == saleProductId && r.OrderProductId == item.Id).FirstOrDefault();
                        if (saleProLT == null)
                        {
                            SaleProductLockedTrack newSaleProLT = new SaleProductLockedTrack();
                            newSaleProLT.OrderId = order.Id;
                            newSaleProLT.OrderSerialNumber = order.SerialNumber;
                            newSaleProLT.ProductId = salePro.ProductId;
                            newSaleProLT.SaleProductId = salePro.Id;
                            newSaleProLT.OrderProductId = item.Id;
                            newSaleProLT.WareHouseId = order.WarehouseId;
                            if (avaStock >= item.Quantity)
                            {
                                newSaleProLT.LockNumber = item.Quantity;
                                saleProWHStock.LockStock += item.Quantity;
                            }
                            else if (avaStock < item.Quantity && avaStock > 0)
                            {
                                newSaleProLT.LockNumber = avaStock;
                                saleProWHStock.LockStock += avaStock;
                            }
                            else
                            {
                                newSaleProLT.LockNumber = 0;
                            }
                            CreateSaleProductLockedTrack(newSaleProLT);
                        }
                        else
                        {
                            if (avaStock >= item.Quantity)
                            {
                                //已有锁定记录  且不为0;
                                if (saleProLT.LockNumber > 0)
                                {
                                    saleProWHStock.LockStock += (item.Quantity - saleProLT.LockNumber);
                                }
                                else
                                {
                                    saleProWHStock.LockStock += item.Quantity;
                                }
                                saleProLT.LockNumber = item.Quantity;
                            }
                            else if (avaStock < item.Quantity && avaStock > 0)
                            {
                                //已有锁定记录  且不为0;
                                if (saleProLT.LockNumber > 0)
                                {
                                    saleProWHStock.LockStock += (item.Quantity - saleProLT.LockNumber);
                                    saleProLT.LockNumber += (item.Quantity - saleProLT.LockNumber);
                                }
                                else
                                {
                                    //原来记录<0
                                    saleProWHStock.LockStock += avaStock;
                                    saleProLT.LockNumber = avaStock;
                                }
                            }
                            else
                            {
                                //判断有问题 可用库存为0 已锁定数跟需要锁定数一致情况下
                                //if (item.Quantity == saleProLT.LockNumber)
                                //{
                                //    //不需要改
                                //}else if (item.Quantity > saleProLT.LockNumber)
                                //{
                                //    //保持现有锁定数即可
                                //} 
                                if (item.Quantity < saleProLT.LockNumber)
                                {
                                    //释放库存多锁定库存
                                    saleProWHStock.LockStock += item.Quantity - saleProLT.LockNumber;
                                    saleProLT.LockNumber = item.Quantity;
                                }
                            }
                            _omsAccessor.Update(saleProLT);
                        }
                        _omsAccessor.Update<SaleProductWareHouseStock>(saleProWHStock);
                        _omsAccessor.SaveChanges();
                    }
                    trans.Commit();
                    return "成功！";
                }
                catch (Exception e)
                {
                    _logService.Error("<UpdateOrderProChangeProductLockedTrackAndWareHouseStock>" + e.Message + " 位置：" + e.StackTrace);
                    trans.Rollback();
                    return "修改商品更新锁定库存跟踪及仓库OMS库存锁定失败";
                }
            }
        }
        /// <summary>
        /// 变更仓库更改仓库锁定信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="oldWareHouseId"></param>
        /// <param name="newWareHouseId"></param>
        /// <returns></returns>
        public bool ChangeWareHouseSetProLockedLog(int orderId, int oldWareHouseId, int newWareHouseId)
        {
            using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                var order = _omsAccessor.Get<Order>().Where(r => r.Id == orderId).FirstOrDefault();
                try
                {
                    var orderPros = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == orderId).ToList();
                    foreach (var item in orderPros)
                    {
                        var oldSaleProLT = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == order.Id && r.SaleProductId == item.SaleProductId && r.OrderProductId == item.Id && r.WareHouseId == oldWareHouseId).FirstOrDefault();
                        //旧仓库情况
                        if (oldWareHouseId != 0)
                        {
                            if (oldSaleProLT != null)
                            {
                                var oldSaleProWHS = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == item.SaleProductId && r.WareHouseId == oldWareHouseId).FirstOrDefault();
                                if (oldSaleProWHS == null)
                                {
                                    SaleProductWareHouseStock SPWHS = new SaleProductWareHouseStock()
                                    {
                                        SaleProductId = item.SaleProductId,
                                        WareHouseId = oldWareHouseId,
                                        ProductId = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == item.SaleProductId).FirstOrDefault().ProductId,
                                        Stock = 0,
                                        LockStock = 0
                                    };
                                    _omsAccessor.Insert<SaleProductWareHouseStock>(SPWHS);
                                }
                                else
                                {
                                    if (oldSaleProLT.LockNumber != 0)
                                    {
                                        oldSaleProWHS.LockStock -= oldSaleProLT.LockNumber;
                                        _omsAccessor.Update<SaleProductWareHouseStock>(oldSaleProWHS);
                                    }
                                }
                            }
                            else
                            {
                                //为空则新增记录
                                SaleProductLockedTrack SPLT = new SaleProductLockedTrack()
                                {
                                    OrderId = order.Id,
                                    OrderSerialNumber = order.SerialNumber,
                                    OrderProductId = item.Id,
                                    ProductId = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == item.SaleProductId).FirstOrDefault().ProductId,
                                    SaleProductId = item.SaleProductId,
                                    LockNumber = 0,
                                    WareHouseId = oldWareHouseId
                                };
                                _omsAccessor.Insert<SaleProductLockedTrack>(SPLT);
                                _omsAccessor.SaveChanges();
                                oldSaleProLT = SPLT;
                            }
                        }
                        _omsAccessor.SaveChanges();
                        //新仓库情况
                        if (oldSaleProLT != null)
                        {
                            var newSaleProWHS = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == item.SaleProductId && r.WareHouseId == newWareHouseId).FirstOrDefault();
                            if (newSaleProWHS == null)
                            {
                                SaleProductWareHouseStock SPWHS = new SaleProductWareHouseStock()
                                {
                                    SaleProductId = item.SaleProductId,
                                    WareHouseId = newWareHouseId,
                                    ProductId = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == item.SaleProductId).FirstOrDefault().ProductId,
                                    Stock = 0,
                                    LockStock = 0
                                };
                                _omsAccessor.Insert<SaleProductWareHouseStock>(SPWHS);
                                //新仓库库存为0，设置锁定数为0
                                oldSaleProLT.LockNumber = 0;
                            }
                            else
                            {

                                var avaStock = newSaleProWHS.Stock - newSaleProWHS.LockStock;
                                if (avaStock >= item.Quantity)
                                {
                                    oldSaleProLT.LockNumber = item.Quantity;
                                    newSaleProWHS.LockStock += item.Quantity;
                                }
                                else if (avaStock < item.Quantity && avaStock > 0)
                                {
                                    oldSaleProLT.LockNumber = avaStock;
                                    newSaleProWHS.LockStock += avaStock;
                                }
                                else
                                {
                                    oldSaleProLT.LockNumber = 0;
                                }
                                _omsAccessor.Update<SaleProductWareHouseStock>(newSaleProWHS);
                            }
                            oldSaleProLT.WareHouseId = newWareHouseId;
                            _omsAccessor.Update<SaleProductLockedTrack>(oldSaleProLT);
                        }
                        else
                        {
                            SaleProductLockedTrack SPLT = new SaleProductLockedTrack()
                            {
                                OrderId = order.Id,
                                OrderSerialNumber = order.SerialNumber,
                                OrderProductId = item.Id,
                                ProductId = _omsAccessor.Get<SaleProduct>().Where(r => r.Id == item.SaleProductId).FirstOrDefault().ProductId,
                                SaleProductId = item.SaleProductId,
                                LockNumber = 0,
                                WareHouseId = newWareHouseId
                            };
                            _omsAccessor.Insert<SaleProductLockedTrack>(SPLT);
                        }
                        _omsAccessor.SaveChanges();
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    _logService.InsertOrderLog(order.Id, "手动选择仓库锁定库存失败", order.State, order.PayState, "手动选择仓库锁定库存失败");
                    _omsAccessor.SaveChanges();
                    return false;
                }
            }
        }
        /// <summary>
        /// B2C订单拆分/合并解锁旧订单锁定库存
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool ChangeProLockedNumByHasLockedNum(int orderId)
        {
            var order = _omsAccessor.Get<Order>().Where(r => r.Id == orderId).FirstOrDefault();
            try
            {
                var orderPros = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == orderId);
                foreach (var item in orderPros)
                {
                    var orderLockTrackLog = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == orderId && r.SaleProductId == item.SaleProductId && r.OrderProductId == item.Id).FirstOrDefault();
                    if (orderLockTrackLog != null)
                    {
                        var saleProWHStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.WareHouseId == order.WarehouseId && r.SaleProductId == item.SaleProductId).FirstOrDefault();
                        if (saleProWHStock != null)
                        {
                            saleProWHStock.LockStock -= orderLockTrackLog.LockNumber;
                            orderLockTrackLog.LockNumber = 0;
                            _omsAccessor.Update<SaleProductWareHouseStock>(saleProWHStock);
                            _omsAccessor.Update<SaleProductLockedTrack>(orderLockTrackLog);
                        }
                        //如若saleProWHStock不存在如何处理？
                    }
                }
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                _logService.Error("<ChangeProLockedNumByHasLockedNum>" + e.Message + " 位置:" + e.StackTrace);
                _logService.InsertOrderLog(order.Id, "更新库存锁定失败", order.State, order.PayState, "更新库存锁定失败");
                return false;
            }

        }
        /// <summary>
        /// 删除订单商品解锁已锁定的库存
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderProId"></param>
        /// <returns></returns>
        public bool DeleteProLockedNumByHasLockedNum(int orderId, int orderProId)
        {
            using (var trans = _omsAccessor.OMSContext.Database.BeginTransaction())
            {
                try
                {
                    var order = _omsAccessor.Get<Order>().Where(r => r.Id == orderId).FirstOrDefault();
                    if (order == null)
                    {
                        return false;
                    }
                    var saleProLT = _omsAccessor.Get<SaleProductLockedTrack>().Where(r => r.OrderId == orderId && r.OrderProductId == orderProId).FirstOrDefault();
                    if (saleProLT != null)
                    {
                        //锁定库存记录锁定数为负数情况？
                        if (saleProLT.LockNumber > 0)
                        {
                            var saleProWHStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProLT.SaleProductId && r.WareHouseId == order.WarehouseId).FirstOrDefault();
                            if (saleProWHStock != null)
                            {
                                saleProWHStock.LockStock -= saleProLT.LockNumber;
                                saleProLT.LockNumber = 0;
                                _omsAccessor.Update<SaleProductWareHouseStock>(saleProWHStock);
                                _omsAccessor.Update<SaleProductLockedTrack>(saleProLT);
                            }
                            //如果saleProWHStock的stock为0，但saleProLT锁定数不为零情况？
                        }
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    _logService.Error("<DeleteProLockedNumByHasLockedNum>" + e.Message + " 位置：" + e.StackTrace);
                    return false;
                }
            }
        }
        /// <summary>
        /// 缺货状态解除
        /// </summary>
        /// <param name="orderIdList"></param>
        /// <returns></returns>
        public string UnLockLackStockOrder(List<int> orderIdList)
        {
            var orderList = "";
            foreach (var id in orderIdList)
            {
                var order = _omsAccessor.Get<Order>().Where(r => r.Id == id).FirstOrDefault();
                //无效订单，已上传订单，已发货订单
                if (order.State == OrderState.Invalid || order.State == OrderState.Uploaded || order.State == OrderState.Finished)
                {
                    orderList += order.SerialNumber + "<br />";
                    continue;
                }
                if (order != null)
                {
                    var orderPros = _omsAccessor.Get<OrderProduct>().Where(r => r.OrderId == order.Id).ToList();
                    foreach (var pro in orderPros)
                    {
                        //调用修改商品代码（流程一致）
                        //如果修改商品的时候，原商品不是缺货商品怎么办？只有部分商品是缺货商品
                        UpdateOrderProChangeLockedLog(order.Id, pro.SaleProductId);
                    }
                }
            }
            return orderList;
        }
        /// <summary>
        /// 分页获取仓库库存列表
        /// </summary>
        /// <param name="searchProductContext"></param>
        /// <returns></returns>
        public PageList<WareHouseStockViewModel> GetWareHouseStockViewModels(SearchProductContext searchProductContext, out int allStock, out int allLockStock, out int allAvailableStock)
        {
            var data = (from sp in _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid && !searchProductContext.WareHouseId.HasValue ? true : x.WareHouseId == searchProductContext.WareHouseId)
                        join p in _omsAccessor.Get<Product>().Where(x => x.Isvalid && (string.IsNullOrEmpty(searchProductContext.SearchStr) ? true : (x.Name.Contains(searchProductContext.SearchStr) || x.Code.Contains(searchProductContext.SearchStr)))) on sp.ProductId equals p.Id
                        join wh in _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid) on sp.WareHouseId equals wh.Id
                        select new WareHouseStockViewModel
                        {
                            ProductId = p.Id,
                            SaleProductId = sp.SaleProductId,
                            WareHouseId = wh.Id,
                            SalePrice = _omsAccessor.Get<SaleProductPrice>().Where(x => x.Isvalid && x.SaleProductId == sp.SaleProductId).FirstOrDefault() == null ? 0 : _omsAccessor.Get<SaleProductPrice>().Where(x => x.Isvalid && x.SaleProductId == sp.SaleProductId).FirstOrDefault().Price,
                            SumSalePrice = _omsAccessor.Get<SaleProductPrice>().Where(x => x.Isvalid && x.SaleProductId == sp.SaleProductId).FirstOrDefault() == null ? 0 : _omsAccessor.Get<SaleProductPrice>().Where(x => x.Isvalid && x.SaleProductId == sp.SaleProductId).FirstOrDefault().Price * sp.Stock,
                            ProductName = p.Name,
                            ProductCode = p.Code,
                            WareHouseName = wh.Name,
                            Stock = sp.Stock,
                            LockStock = sp.LockStock,
                            AvailableStock = sp.Stock - sp.LockStock,
                            SumStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid && x.ProductId == p.Id).Sum(x => x.Stock),
                            SumLockStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid && x.ProductId == p.Id).Sum(x => x.LockStock),
                            SumAvailableStock = _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid && x.ProductId == p.Id).Sum(x => x.Stock - x.LockStock)
                        }).OrderBy(x => x.ProductId).ThenBy(x => x.WareHouseId).ToList();

            //按可用库存进行判断查询
            if (!(searchProductContext.AvailableStockMin == 0 && searchProductContext.AvailableStockMax == 0))
            {
                data = data.Where(x => x.AvailableStock >= searchProductContext.AvailableStockMin && x.AvailableStock <= searchProductContext.AvailableStockMax).ToList();
            }
            //按销售单价进行查询
            if (!(searchProductContext.SalePriceMin == 0 && searchProductContext.SalePriceMax == 0))
            {
                data = data.Where(x => x.SalePrice >= searchProductContext.SalePriceMin && x.SalePrice <= searchProductContext.SalePriceMax).ToList();
            }
            //对输出的参数进行赋值
            allStock = data.Sum(x => x.Stock);
            allLockStock = data.Sum(x => x.LockStock);
            allAvailableStock = data.Sum(x => x.AvailableStock);
            return new PageList<WareHouseStockViewModel>(data, searchProductContext.PageIndex, searchProductContext.PageSize);
        }
        #endregion

        #region 获取WMS库存并同步到OMS（保持库存一致）
        public bool SyncStocksWmsToOms(int productId = 0)
        {
            Monitor.Enter(_MyLock1); //获取排它锁

            try
            {
                var product = _omsAccessor.GetById<Product>(productId);
                if (product == null && productId != 0)
                    return false;

                //处理SSL问题
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
                using (var http = new HttpClient(httpClientHandler))
                {
                    var content = new StringContent(product.Id.ToString(), Encoding.UTF8, "application/json");
                    var requestUrl = AppConfigurtaionServices.Configuration["WMSApi:domain"] + "/wmsapi/ProductSync/GetWareHouseStocks";

                    #region JWTBearer授权信息
                    _workContext.CurrentHttpContext.Request.Cookies.TryGetValue("wms_token", out string token);
                    if (string.IsNullOrEmpty(token)) { token = GetWMSOauthToken().Result; }
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    #endregion
                    var response = http.PostAsync(requestUrl, content).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = response.Content.ReadAsStringAsync();
                        var resultContent = result.Result.ToString();
                        var resultModel = resultContent.ToObj<WmsWareHouseStockModel>();
                        if (!resultModel.isSucc)
                        {
                            _logService.Error(resultModel.msg);
                            return false;
                        }
                        //更新商品仓库库存
                        if (UpdateWareHouseStock(resultModel.data, productId))
                        {
                            return true;
                        }
                        else
                        {
                            _logService.Error("商品仓库库存更新失败，商品Id：" + productId);
                            return false;
                        }


                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logService.Error("商品仓库库存更新失败，原因是API授权失败！请重试。");
                        return false;
                    }
                    else
                    {
                        _logService.Error(string.Format("商品仓库库存更新失败,状态码：{0},RequestMessage:{1}", response.StatusCode, response.RequestMessage));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error("商品仓库库存更新失败：" + ex.Message);
                return false;
            }

            finally
            {
                Monitor.Exit(_MyLock1); //释放排它锁
            }

        }
        /// <summary>
        /// 更新商品仓库库存
        /// </summary>
        /// <returns></returns>
        public bool UpdateWareHouseStock(List<WareHouseStock> wareHouseStocks, int productId = 0)
        {
            Monitor.Enter(_MyLock2); //获取排它锁

            try
            {
                if (wareHouseStocks == null || wareHouseStocks.Count == 0)
                    return false;
                //单条数据更新（已知商品ID）
                if (productId != 0)
                {
                    var saleproduct = _omsAccessor.Get<SaleProduct>().Include(x => x.Product).Where(x => x.Isvalid && x.Channel == 94 && x.ProductId == productId).FirstOrDefault();
                    if (saleproduct == null)
                        return false;
                    foreach (var itemWareHouseStock in wareHouseStocks)
                    {
                        var warehouse = _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.Code == itemWareHouseStock.WareHouseCode).FirstOrDefault();
                        if (warehouse == null)
                            continue;
                        var stockWareHouse = _omsAccessor.Get<SaleProductWareHouseStock>().Where(x => x.Isvalid && x.ProductId == itemWareHouseStock.ProductOmsId && x.WareHouseId == warehouse.Id).FirstOrDefault();
                        if (stockWareHouse == null)
                        {
                            SaleProductWareHouseStock saleProductWareHouseStock = new SaleProductWareHouseStock
                            {
                                SaleProductId = saleproduct.Id,
                                ProductId = saleproduct.ProductId,
                                WareHouseId = warehouse.Id,
                                Stock = itemWareHouseStock.Stock,
                                LockStock = 0,
                                CreatedBy = 0
                            };
                            _omsAccessor.Insert(saleProductWareHouseStock);
                        }
                        else
                        {
                            stockWareHouse.Stock = itemWareHouseStock.Stock;
                            stockWareHouse.ModifiedBy = 0;
                            stockWareHouse.ModifiedTime = DateTime.Now;
                            _omsAccessor.Update(stockWareHouse);
                        }
                    }
                    saleproduct.Stock = wareHouseStocks.Sum(x => x.Stock);
                    saleproduct.AvailableStock = saleproduct.Stock - saleproduct.LockStock;
                    _omsAccessor.Update(saleproduct);
                    _omsAccessor.SaveChanges();
                    //更新销售商品的库存到商城
                    var xuniStock = GetXuniStocks(saleproduct.Id);
                    SyncProductStockToAssist(501, saleproduct.Product.Code, (saleproduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock))));
                    return true;
                }
                else //整体更新
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.Error("商品仓库库存同步错误：" + ex.Message);
                return false;
            }

            finally
            {
                Monitor.Exit(_MyLock2); //释放排它锁
            }
        }
        public List<SaleProductWareHouseStock> GetXuniStocks(int saleProductId)
        {
            var data = (from spw in _omsAccessor.Get<SaleProductWareHouseStock>().Where(r => r.SaleProductId == saleProductId)
                        join w in _omsAccessor.Get<WareHouse>().Where(w => !w.IsSyncStock)
                        on spw.WareHouseId equals w.Id
                        select spw).ToList();
            return data;
        }
        #endregion




    }
}
