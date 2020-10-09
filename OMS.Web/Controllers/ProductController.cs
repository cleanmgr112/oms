using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Model;
using OMS.Model.JsonModel;
using OMS.Services.Common;
using OMS.Services.Permissions;
using OMS.Services.Products;
using OMS.WebCore;
using OMS.Core.Json;
using OMS.Model.Grid;
using OMS.Model.Products;
using OMS.Core.Tools;
using LitJson;
using System.Text;
using OMS.Services.Log;
using OMS.Services.ScheduleTasks;
using OMS.Services;

namespace OMS.Web.Controllers
{
    public class ProductController : BaseController
    {
        #region ctor
        private readonly IProductService _productService;
        private readonly ICommonService _commonService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IPermissionService _permissionService;
        private readonly IScheduleTaskFuncService _scheduleTaskFuncService;
        protected readonly IWorkContext _workContext;
        private readonly ILogService _logService;
        private readonly IWareHouseService _wareHouseService;

        public ProductController(IProductService productService,
            ICommonService commonService, IHostingEnvironment hostingEnvironment, IPermissionService permissionService
            , IWorkContext workContext
            , ILogService logService, IScheduleTaskFuncService scheduleTaskFuncService,IWareHouseService wareHouseService)
        {
            _productService = productService;
            _commonService = commonService;
            _hostingEnvironment = hostingEnvironment;
            _permissionService = permissionService;
            _workContext = workContext;
            _logService = logService;
            _scheduleTaskFuncService = scheduleTaskFuncService;
            _wareHouseService = wareHouseService;
        }
        #endregion


        #region 商品
        /// <summary>
        /// 商品主页
        /// </summary>
        /// <returns></returns>
        public IActionResult List()
        {
            if (!_permissionService.Authorize("ViewProducts"))
            {
                return View("_AccessDeniedView");
            }
            return View();
        }

        public IActionResult GetListData(int pageIndex = 1, int pageSize = 20)
        {
            var data = _productService.GetProductList(pageSize, pageIndex);
            int totalCount = data.TotalCount;

            var result = new
            {
                draw = pageIndex,
                recordsTotal = totalCount,
                recordsFiltered = totalCount,
                data = data
            };

            return Json(result);
        }
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <returns></returns>
        public IActionResult CreatedProduct()
        {
            if (!_permissionService.Authorize("ViewCreateProduct"))
            {
                return View("_AccessDeniedView");
            }
            //获取字典数据           
            ViewBag.Country = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.Country), "Id", "Value");
            ViewBag.Types = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.ProductType), "Id", "Value");
            ViewBag.Area = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.Area), "Id", "Value");
            ViewBag.Grapes = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.Variety), "Id", "Value");
            ViewBag.Capacity = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.capacity), "Id", "Value");
            ViewBag.Packing = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.Packing), "Id", "Value");
            ViewBag.Category = new SelectList(_productService.GetAllCategory(), "Id", "Name");
            var year = _commonService.GetYearUntilNow();
            ViewBag.Year = new SelectList(year);
            return View();
        }
        [HttpPost]
        public IActionResult CreatedProductFunc(string modelString, IFormFile files)
        {
            if (!_permissionService.Authorize("CreateProduct"))
            {
                return View("_AccessDeniedView");
            }
            ProductModel model = JsonMapper.ToObject<ProductModel>(modelString);
            try
            {
                if (!_productService.ConfirmProductExist(model.Name) && _productService.GetProductByCode(model.Code) == null && !string.IsNullOrEmpty(model.Code) && !string.IsNullOrEmpty(model.Name))
                {
                    if (model.ArrayGrapes != null && model.ArrayGrapes.Length > 0)
                    {
                        model.Grapes = String.Join(",", model.ArrayGrapes);
                    }

                    if (files != null)
                    {
                        //上传到阿里云
                        string msg = string.Empty;
                        string key = Guid.NewGuid().ToString() + Path.GetExtension(files.FileName);
                        OSSHelper.UploadFileByStream(key, files.OpenReadStream(), out msg);
                        model.Cover = key;
                        if (!string.IsNullOrEmpty(msg))
                        {
                            return Error("图片上传失败");
                        }
                    }
                    Product product = model.ToEntity();
                    _productService.InsertProduct(product);
                    //同步商品到WMS
                    _scheduleTaskFuncService.OmsSyncProducts();
                    return Success();
                }
                else
                {
                    return Error("已存在相同名称或相同编码商品或商品名称为空或商品编码为空信息填写有误！");

                }
            }
            catch (Exception ex)
            {
                Logger.Error("添加商品异常" + ex.Message);
                return Error("添加商品异常!");
            }

        }
        /// <summary>
        /// 编辑商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult EditProduct(int id)
        {
            if (!_permissionService.Authorize("ViewEditProduct"))
            {
                return View("_AccessDeniedView");
            }
            var product = _productService.GetProductById(id);
            //获取字典数据
            ViewBag.Country = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.Country), "Id", "Value", product.Country);
            ViewBag.Types = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.ProductType), "Id", "Value", product.Type);
            ViewBag.Area = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.Area), "Id", "Value", product.Area);
            if (product.Grapes != null)
            {
                string[] getSeletedGrapes = (product.Grapes).Split(",");
                ViewBag.SelectedGrapes = JsonConvert.SerializeObject(getSeletedGrapes);
            }
            ViewBag.Grapes = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.Variety), "Id", "Value", product.Grapes);
            ViewBag.Capacity = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.capacity), "Id", "Value", product.Capacity);
            ViewBag.Packing = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.Packing), "Id", "Value", product.Packing);
            ViewBag.Category = new SelectList(_productService.GetAllCategory(), "Id", "Name", product.CategoryId);
            var year = _commonService.GetYearUntilNow();
            ViewBag.Year = new SelectList(year, "", "", product.Year);
            return View(product);
        }

        [HttpPost]
        public IActionResult EditProductFunc(string modelstring, IFormFile files)
        {
            if (!_permissionService.Authorize("EditProduct"))
            {
                return View("_AccessDeniedView");
            }
            try
            {
                ProductModel productUpdate = JsonMapper.ToObject<ProductModel>(modelstring);
                Product products = _productService.GetProductById(productUpdate.Id);
                var isExistProduct = _productService.ConfirmProductExist(productUpdate.Name.Trim());
                if (products.Name.Trim() == productUpdate.Name.Trim()) {
                    isExistProduct = false;
                }
                var productExist = _productService.GetProductByCode(productUpdate.Code.Trim());
                if (productExist.Code.Trim() == productUpdate.Code.Trim()) {
                    productExist = null; 
                }
                if (!isExistProduct && productExist== null && !string.IsNullOrEmpty(productUpdate.Code) && !string.IsNullOrEmpty(productUpdate.Name))
                {
                    products.Name = productUpdate.Name.Trim();
                    products.NameEn = productUpdate.NameEn;
                    products.Code = productUpdate.Code.Trim();
                    products.Type = productUpdate.Type;
                    products.CategoryId = productUpdate.CategoryId;
                    products.Country = productUpdate.Country;
                    products.Area = productUpdate.Area;
                    products.DeputyBarcode = productUpdate.DeputyBarcode;
                    if (productUpdate.ArrayGrapes != null)
                    {
                        products.Grapes = String.Join(",", productUpdate.ArrayGrapes);
                    }
                    else
                    {
                        products.Grapes = null;
                    }
                    products.Capacity = productUpdate.Capacity;
                    products.Packing = productUpdate.Packing;
                    products.Year = productUpdate.Year;

                    if (files != null)
                    {
                        //上传到阿里云
                        string msg = string.Empty;
                        string key = Guid.NewGuid().ToString() + Path.GetExtension(files.FileName);
                        OSSHelper.UploadFileByStream(key, files.OpenReadStream(), out msg);
                        products.Cover = key;
                        if (!string.IsNullOrEmpty(msg))
                        {
                            return Error("图片上传失败");
                        }
                    }
                    _productService.UpdateProduct(products);
                    //更新到WMS
                    _productService.UpdateProductsInfoToWMS(products.Id);
                    return Success();
                }
                else {
                    return Error("已存在名称相同或者编码相同的商品或商品名称和编码为空！");
                }              
            }
            catch (Exception ex)
            {

                Logger.Error("更新商品异常：",ex);
                return Error("更新商品异常，检查后重新更新！");
            }
        }
        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult DelProduct(int id)
        {
            if (!_permissionService.Authorize("DeleteProduct"))
            {
                return View("_AccessDeniedView");
            }
            _productService.DelProduct(id);
            return RedirectToAction("List");
        }
        [HttpPost]
        public IActionResult GetProducts(string search, int pageSize, int pageIndex, int priceType)
        {
            //var data = _productService.GetProductList(pageSize, pageIndex,0, search);
            var data = _productService.GetSaleProductPriceList(pageSize, pageIndex, search, priceType);
            return Success(data);
        }
        [HttpPost]
        public IActionResult GetProductInfo(int saleProductId, int priceTypeId)
        {
            var saleProductPriceModel = _productService.GetSaleProductPriceById(saleProductId, priceTypeId);
            var jsonresult = TurnJson(_productService.GetSaleProductById(saleProductId));
            SaleProduct saleProduct = new SaleProduct();
            saleProduct.ProductId = jsonresult[0]["ProductId"];
            saleProduct.Channel = jsonresult[0]["Channel"];
            saleProduct.Stock = jsonresult[0]["Stock"];
            saleProduct.LockStock = jsonresult[0]["LockStock"];
            saleProduct.AvailableStock = jsonresult[0]["AvailableStock"];
            saleProduct.Product = _productService.GetProductById(saleProduct.ProductId);
            saleProductPriceModel.SaleProduct = saleProduct;

            return Success(saleProductPriceModel);
        }
        private void PrepSelectItem(ProductModel model)
        {
            var dictionarys = _commonService.GetAllDictionarys();
            //商品类型
            foreach (var item in dictionarys.Where(x => x.Type == DictionaryType.ProductType))
            {
                model.Types.Add(new SelectListItem() { Text = item.Value, Value = item.Id.ToString(), Selected = (item.Id == model.Type) });
            }
            //国家
            foreach (var item in dictionarys.Where(x => x.Type == DictionaryType.Country))
            {
                model.Countries.Add(new SelectListItem() { Text = item.Value, Value = item.Id.ToString(), Selected = (item.Id == model.Type) });
            }
            //产区
            foreach (var item in dictionarys.Where(x => x.Type == DictionaryType.Area))
            {
                model.Areas.Add(new SelectListItem() { Text = item.Value, Value = item.Id.ToString(), Selected = (item.Id == model.Type) });
            }
            //葡萄品种
            foreach (var item in dictionarys.Where(x => x.Type == DictionaryType.Variety))
            {
                model.GrapeItems.Add(new SelectListItem() { Text = item.Value, Value = item.Id.ToString(), Selected = (item.Id == model.Type) });
            }
            //容量
            foreach (var item in dictionarys.Where(x => x.Type == DictionaryType.capacity))
            {
                model.Capacitys.Add(new SelectListItem() { Text = item.Value, Value = item.Id.ToString(), Selected = (item.Id == model.Type) });
            }
            //包装方式
            foreach (var item in dictionarys.Where(x => x.Type == DictionaryType.Packing))
            {
                model.Packings.Add(new SelectListItem() { Text = item.Value, Value = item.Id.ToString(), Selected = (item.Id == model.Type) });
            }
        }
        #endregion


        #region 商品系列
        /// <summary>
        /// 商品系列
        /// </summary>
        /// <returns></returns>
        //增加商品系列
        public IActionResult CreatedCategory()
        {
            if (!_permissionService.Authorize("ViewCreatedCategory"))
            {
                return View("_AccessDeniedView");
            }
            var category = _productService.GetAllCategory();
            ViewBag.Category = new SelectList(_productService.GetAllCategory(), "Id", "Name");
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatedCategory(CategoryModel model)
        {
            if (!_permissionService.Authorize("CreatedCategory"))
            {
                return View("_AccessDeniedView");
            }
            if (_productService.GetCategoryByName(model.Name))
            {
                ErrorNotification("已有系列，请重新输入系列");
                return RedirectToAction("CreatedCategory");
            }
            else
            {
                Category category = model.ToEntity();
                _productService.CreateCategory(category);
                return RedirectToAction("CreatedCategory");
            }
        }
        //编辑商品系列
        public IActionResult UpdateCategory(int id)
        {
            if (!_permissionService.Authorize("ViewUpdateCategory"))
            {
                return View("_AccessDeniedView");
            }
            Category category = _productService.GetCategoryById(id);
            ViewBag.Category = new SelectList(_productService.GetAllCategory(), "Id", "Name", category.ParentCategoryId);
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCategory(Category category)
        {
            if (!_permissionService.Authorize("UpdateCategory"))
            {
                return View("_AccessDeniedView");
            }
            _productService.UpdateCategory(category);
            return RedirectToAction("CreatedCategory");
        }
        //删除商品系列
        public IActionResult DelCategory(int id)
        {
            if (!_permissionService.Authorize("DelCategory"))
            {
                return Error("无操作权限！");
            }
            _productService.DelCategory(id);
            return Success("删除成功");
        }
        #endregion


        #region 销售商品
        ///<summary>
        ///销售商品
        /// </summary>
        /// <returns></returns>
        //添加销售商品
        public IActionResult CreatedSaleProducts()
        {
            var GetProducts = _productService.GetAllProducts();
            dynamic Products = TurnJson(GetProducts);
            ViewBag.SaleProducts = Products;
            ViewBag.Channel = _commonService.GetBaseDictionaryList(DictionaryType.Channel);
            return View();
        }
        [HttpPost]
        public IActionResult CreatedSaleProducts(SaleProductModel salProduct, PriceModel priceModel)
        {
            try
            {
                if (_productService.ConfirmSaleProductExist(salProduct.ProductId, salProduct.Channel))
                {
                    ErrorNotification("已存在当前渠道的销售商品！");
                    return RedirectToAction("CreatedSaleProducts");
                }
                else
                {
                    SaleProduct saleproduct = salProduct.ToEntity();
                    var prouct = _productService.GetProductById(salProduct.ProductId);
                    saleproduct.Stock = 0;
                    saleproduct.LockStock = 0;
                    saleproduct.AvailableStock = 0;
                    var saleProductId = _productService.CreateSaleProducts(saleproduct);//添加销售商品
                    AddSaleProductPrice(saleProductId, priceModel);//添加销售商品价格
                    _productService.SyncStocksWmsToOms(salProduct.ProductId);//更新库存信息
                    SuccessNotification("添加成功！");
                    return RedirectToAction("SaleProductsPrice");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("添加销售商品异常：" + ex.Message);
                ErrorNotification("添加销售商品失败，请重新添加！");
                return RedirectToAction("CreatedSaleProducts");
            }

        }
        //编辑销售商品
        public IActionResult EditSaleProducts(int id)
        {
            var getSaleProduct = _productService.GetSaleProductById(id);
            var saleProduct = TurnJson(getSaleProduct);
            return View(saleProduct);
        }
        [HttpPost]
        public IActionResult EditSaleProducts(SaleProductModel saleProductModel)
        {

            try
            {
                var prouct = _productService.GetProductById(saleProductModel.ProductId);
                SaleProduct saleProduct = new SaleProduct();
                saleProduct.Channel = saleProductModel.Channel;
                saleProduct.Id = saleProductModel.Id;
                saleProduct.ProductId = saleProductModel.ProductId;
                saleProduct.ModifiedBy = _workContext.CurrentUser.Id;
                _productService.UpdateSaleProduct(saleProduct);
                return RedirectToAction("SaleProducts");
            }
            catch (Exception ex)
            {
                Logger.Error("修改销售商品异常：" + ex.Message);
                ErrorNotification("修改销售商品失败，请重新修改！");
                return RedirectToAction("SaleProducts");
            }

        }
        //删除销售商品
        public IActionResult DelSaleProducts(int id)
        {
            _productService.DelSaleProduct(id);
            return RedirectToAction("SaleProducts");
        }
        #endregion


        #region 销售商品价格
        public IActionResult SaleProductsPrice()
        {
            var saleProductPrice = _productService.SaleProductPriceList();
            return View(saleProductPrice);
        }
        public IActionResult CreatedSaleProductsPrice()
        {
            var GetSaleProducts = _productService.GetAllSaleProducts();
            dynamic SaleProducts = TurnJson(GetSaleProducts);
            ViewBag.PriceType = _commonService.GetBaseDictionaryList(DictionaryType.PriceType);
            return View(SaleProducts);
        }
        [HttpPost]
        public IActionResult CreatedSaleProductsPrice(SaleProductPriceModel saleProductPrice)
        {
            var ifexist = _productService.GetSaleProductPriceById(saleProductPrice.SaleProductId, saleProductPrice.CustomerTypeId);
            if (ifexist == null)
            {
                SaleProductPrice saleproductprice = saleProductPrice.ToEntity();
                _productService.CreateSaleProductsPrice(saleproductprice);
                SuccessNotification("添加成功！");
                return RedirectToAction("SaleProducts");
            }
            else
            {
                ErrorNotification("商品已添加相同类型价格！");
                return RedirectToAction("SaleProductsPrice");
            }
        }
        /// <summary>
        /// 批量导入销售商品价格
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BatchUploadPrice(IFormFile formFile)
        {
            IWorkbook workbook = null;
            if (formFile == null)
            {
                return Error("请添加要导入文件");
            }
            else
            {
                string fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    formFile.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0)
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else if (fileName.IndexOf(".xls") > 0)
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                ISheet sheet = workbook.GetSheetAt(0);
                IRow row;

                StringBuilder errorStr = new StringBuilder();//记录错误信息
                int succCount = 0;
                int errorCount = 0;

                List<string> failName = new List<string>();
                //商品库存信息
                var productStockDataList = new List<ProductStockData>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    try
                    {
                        if (row != null)
                        {
                            var productCode = row.GetCell(0).ToString().Trim();
                            var product = _productService.GetProductByCode(productCode);
                            if (product == null)
                            {
                                errorStr.Append(string.Format("不存在编码为{0}的商品;", productCode));
                                errorCount++;
                                continue;
                            }
                            else
                            {
                                var productId = product.Id;
                                var channel = SaleProductPriceChannel(row.GetCell(2).ToString().Trim());
                                var stock = Convert.ToInt32(row.GetCell(3).ToString().Trim());
                                var ifexist = _productService.ConfirmSaleProductExist(productId, channel);
                                if (ifexist)
                                {
                                    errorStr.Append(string.Format("已存在编码为{0}的销售商品;", productCode));
                                    errorCount++;
                                    continue;
                                }
                                else
                                {
                                    SaleProduct saleProduct = new SaleProduct();
                                    saleProduct.ProductId = productId;
                                    saleProduct.Channel = channel;
                                    //如果是现货的库存需要去WMS同步，跨境和期酒直接获取表格中的数量
                                    if (row.GetCell(2).ToString().Trim() == "现货")
                                    {
                                        var content = _productService.GetWMSProdcutStockByProductId(productId);
                                        var result = content.ToObj<GetStockResultModel>();
                                        if (result.isSucc)
                                        {
                                            saleProduct.Stock = result.count;
                                        }
                                        else
                                        {
                                            errorStr.Append(string.Format("编码为{0}的现货销售商品同步WMS库存失败;", productCode));
                                            errorCount++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        saleProduct.Stock = stock;
                                    }
                                    saleProduct.LockStock = 0;
                                    saleProduct.AvailableStock = stock;
                                    //更新销售商品的库存到商城
                                    var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                                    var productStockData = new ProductStockData
                                    {
                                        sd_id = "501",
                                        goods_sn = product.Code,
                                        stock_num = (saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock))).ToString(),
                                        stock_detail_list = _productService.GetSaleProductWareHouseStocksByProductCode(product.Code)
                                    };
                                    productStockDataList.Add(productStockData);
                                    var saleproductId = _productService.CreateSaleProducts(saleProduct);//添加销售商品
                                    AddSaleProductDefaultPrice(saleproductId);//添加默认销售商品价格
                                    for (int j = 0; j <= 4; j++)
                                    {
                                        var saleProductPrice = _productService.GetSaleProductPriceById(saleproductId, 103 + j);
                                        saleProductPrice.Price = Convert.ToDecimal(row.GetCell(4 + j).ToString().Trim());
                                        _productService.UpdateSaleProductPrice(saleProductPrice);
                                    }
                                }
                            }
                        }
                        succCount++;

                    }
                    catch (Exception ex)
                    {
                        errorStr.Append(string.Format("导入编码为{0}的商品出现异常;", row.GetCell(0).ToString().Trim()));
                        errorCount++;
                        continue;
                    }

                }
                System.IO.File.Delete(fileName);
                fileStream.Close();
                //更新库存到商城
                _productService.SyncMoreProductStockToAssist(productStockDataList);
                if (errorCount > 0)
                {
                    string err = string.Format("总共{0}个销售商品，其中导入成功{1}个，失败{2}个，导入错误的销售商品信息：{3}", sheet.LastRowNum, succCount, errorCount, errorStr.ToString());
                    _logService.Error(err);
                    return Error(err);
                }
                else
                {
                    return Success(string.Format("总共{0}个销售商品，导入成功{1}个", sheet.LastRowNum, succCount));
                }
            }
        }
        /// <summary>
        /// 批量导入商品
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult BatchUploadProduct(IFormFile formFile)
        {
            IWorkbook workbook = null;
            if (formFile == null)
            {
                return Error("请添加要导入文件!");
            }
            else
            {
                var successCount = 0;
                var errorCount = 0;
                var failinputName = "";
                string fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    formFile.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0)
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else if (fileName.IndexOf(".xls") > 0)
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                ISheet sheet = workbook.GetSheetAt(0);
                IRow row;
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    try
                    {

                        if (row.GetCell(0) == null || row.GetCell(1) == null || string.IsNullOrEmpty(row.GetCell(0).ToString().Trim()) || string.IsNullOrEmpty(row.GetCell(1).ToString().Trim()))
                        {
                            failinputName += row.GetCell(1) == null ? "" : row.GetCell(1).ToString().Trim() + "<br/>";
                            errorCount++;
                            continue;
                        }
                        var productOld = _productService.GetProductByCode(row.GetCell(0).ToString().Trim());
                        if (productOld != null)
                        {
                            failinputName += row.GetCell(1) == null ? "" : row.GetCell(1).ToString().Trim() + "<br/>";
                            errorCount++;
                            continue;
                        }
                        Product product = new Product()
                        {
                            Code = row.GetCell(0) == null ? "" : row.GetCell(0).ToString().Trim(),
                            Name = row.GetCell(1) == null ? "" : row.GetCell(1).ToString().Trim(),
                            NameEn = row.GetCell(2) == null ? "" : row.GetCell(2).ToString().Trim(),
                            Year = row.GetCell(3) == null ? "" : row.GetCell(3).ToString().Trim(),
                            Grapes = row.GetCell(4) == null ? "" : row.GetCell(4).ToString().Trim(),
                            DeputyBarcode = row.GetCell(5) == null ? "" : row.GetCell(5).ToString().Trim(),
                            Type = row.GetCell(6) == null ? 0 : Convert.ToInt32(row.GetCell(6).ToString().Trim()),
                            Cover = row.GetCell(7) == null ? "" : row.GetCell(7).ToString().Trim(),
                            Country = row.GetCell(8) == null ? 0 : Convert.ToInt32(row.GetCell(8).ToString().Trim()),
                            Area = row.GetCell(9) == null ? 0 : Convert.ToInt32(row.GetCell(9).ToString().Trim()),
                            Capacity = row.GetCell(10) == null ? 0 : Convert.ToInt32(row.GetCell(10).ToString().Trim()),
                            Packing = row.GetCell(11) == null ? 0 : Convert.ToInt32(row.GetCell(11).ToString().Trim()),
                            CategoryId = row.GetCell(12) == null ? 0 : Convert.ToInt32(row.GetCell(12).ToString().Trim()),
                        };

                        _productService.InsertProduct(product);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("导入商品错误：" + ex.Message);
                        failinputName += row.GetCell(1) == null ? "" : row.GetCell(1).ToString().Trim() + "<br/>";
                        errorCount++;
                        continue;
                    }

                }
                System.IO.File.Delete(fileName);
                fileStream.Close();
                //同步商品到WMS
                _scheduleTaskFuncService.OmsSyncProducts();
                return Success("共：" + successCount + errorCount + "款<br/> " + "导入成功：" + successCount + "款 <br/>导入失败酒款：<br/>" + failinputName);
            }
        }
        /// <summary>
        /// 批量导入销售商品
        /// </summary>
        [HttpPost]
        public IActionResult BatchUploadSaleProduct(IFormFile formFile)
        {
            IWorkbook workbook = null;
            if (formFile == null)
            {
                ErrorNotification("请添加要导入文件");
                return RedirectToAction("List");
            }
            else
            {
                string fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    formFile.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0)
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else if (fileName.IndexOf(".xls") > 0)
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                ISheet sheet = workbook.GetSheetAt(0);
                IRow row;

                List<string> failName = new List<string>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    SaleProduct saleProduct = new SaleProduct()
                    {
                        ProductId = Convert.ToInt32(row.GetCell(0).ToString().Trim()),
                        Channel = Convert.ToInt32(row.GetCell(1).ToString().Trim()),
                        Stock = Convert.ToInt32(row.GetCell(2).ToString().Trim()),
                        LockStock = Convert.ToInt32(row.GetCell(3).ToString().Trim()),
                        AvailableStock = Convert.ToInt32(row.GetCell(4).ToString().Trim())
                    };

                    _productService.CreateSaleProducts(saleProduct);
                }
                System.IO.File.Delete(fileName);
                fileStream.Close();
                string failinputName = "";
                foreach (var item in failName)
                {
                    failinputName += item + "<br/>";
                }

                SuccessNotification("共：" + sheet.LastRowNum + "款<br/> " + "导入成功：" + (sheet.LastRowNum - failName.Count()) + "款 <br/>导入失败酒款：<br/>" + failinputName);
                return RedirectToAction("SaleProductsPrice");
            }
        }
        [HttpPost]
        public IActionResult BatchUploadSaleProductPrice(IFormFile formFile)
        {
            IWorkbook workbook = null;
            if (formFile == null)
            {
                ErrorNotification("请添加要导入文件");
                return RedirectToAction("List");
            }
            else
            {
                string fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    formFile.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (fileName.IndexOf(".xlsx") > 0)
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
                else if (fileName.IndexOf(".xls") > 0)
                {
                    workbook = new HSSFWorkbook(fileStream);
                }
                ISheet sheet = workbook.GetSheetAt(0);
                IRow row;

                List<string> failName = new List<string>();
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    var saleProductId = Convert.ToInt32(row.GetCell(0).ToString().Trim());
                    AddSaleProductDefaultPrice(saleProductId);//添加默认销售商品价格
                    for (int j = 0; j <= 4; j++)
                    {
                        var saleProductPrice = _productService.GetSaleProductPriceById(saleProductId, 103 + j);
                        saleProductPrice.Price = Convert.ToDecimal(row.GetCell(0 + j).ToString().Trim());
                        _productService.UpdateSaleProductPrice(saleProductPrice);
                    }
                }
                System.IO.File.Delete(fileName);
                fileStream.Close();
                string failinputName = "";
                foreach (var item in failName)
                {
                    failinputName += item + "<br/>";
                }

                SuccessNotification("共：" + sheet.LastRowNum + "款<br/> " + "导入成功：" + (sheet.LastRowNum - failName.Count()) + "款 <br/>导入失败酒款：<br/>" + failinputName);
                return RedirectToAction("SaleProductsPrice");
            }

        }
        public IActionResult EditSaleProductsPrice(int id)
        {
            //var saleProductPrice = _productService.GetSaleProductPriceBySaleProductId(id).FirstOrDefault();
            var data = _productService.GetSaleProductDetailBySaleProductId(id);
            ViewBag.PriceTypes = _commonService.GetBaseDictionaryList(DictionaryType.PriceType);
            return View(data);
        }
        [HttpPost]
        public IActionResult EditSaleProductsPrice(SaleProductPriceModel saleProductPriceModel)
        {

            try
            {
                #region 销售商品价格类型赋值
                List<dynamic> data = new List<dynamic>();
                dynamic bz = new dynamic[3];
                bz[0] = saleProductPriceModel.BzPriceId;
                bz[1] = 103;
                bz[2] = saleProductPriceModel.BzPrice;
                dynamic tg = new dynamic[3];
                tg[0] = saleProductPriceModel.TgPriceId;
                tg[1] = 104;
                tg[2] = saleProductPriceModel.TgPrice;
                dynamic jxs = new dynamic[3];
                jxs[0] = saleProductPriceModel.JxsPriceId;
                jxs[1] = 105;
                jxs[2] = saleProductPriceModel.JxsPrice;
                dynamic nb = new dynamic[3];
                nb[0] = saleProductPriceModel.NbPriceId;
                nb[1] = 106;
                nb[2] = saleProductPriceModel.NbPrice;
                dynamic pf = new dynamic[3];
                pf[0] = saleProductPriceModel.PfPriceId;
                pf[1] = 107;
                pf[2] = saleProductPriceModel.PfPrice;
                data.Add(bz);
                data.Add(tg);
                data.Add(jxs);
                data.Add(nb);
                data.Add(pf);
                #endregion


                var _saleProduct = TurnJson(_productService.GetSaleProductById(saleProductPriceModel.SaleProductId));
                var product = _productService.GetProductById(saleProductPriceModel.SaleProductId);
                SaleProduct saleProduct = new SaleProduct();
                saleProduct.Id = _saleProduct[0].Id;
                saleProduct.Channel = _saleProduct[0].Channel;
                saleProduct.ProductId = _saleProduct[0].ProductId;
                saleProduct.Stock = saleProductPriceModel.Stock;
                saleProduct.LockStock = _saleProduct[0].LockStock;
                saleProduct.AvailableStock = saleProductPriceModel.Stock - _saleProduct[0].LockStock;
                saleProduct.ModifiedBy = _workContext.CurrentUser.Id;
                _productService.UpdateSaleProduct(saleProduct);
                //更新销售商品的库存到商城
                var xuniStock = _productService.GetXuniStocks(saleProduct.Id);
                _productService.SyncProductStockToAssist(501, product.Code, saleProduct.AvailableStock - (xuniStock.Sum(r => r.Stock) - xuniStock.Sum(r => r.LockStock)));


                var priceType = _commonService.GetBaseDictionaryList(DictionaryType.PriceType);
                for (int i = 0; i <= (priceType.Count - 1); i++)
                {
                    SaleProductPrice saleProductPrice = new SaleProductPrice();
                    saleProductPrice.Id = data[i][0];
                    saleProductPrice.SaleProductId = saleProductPriceModel.SaleProductId;
                    saleProductPrice.CustomerTypeId = data[i][1];
                    saleProductPrice.Price = data[i][2];
                    _productService.UpdateSaleProductPrice(saleProductPrice);

                }
                return RedirectToAction("SaleProductsPrice");
            }
            catch (Exception ex)
            {
                Logger.Error("修改销售商品异常：" + ex.Message);
                ErrorNotification("修改销售商品失败，请重新修改！");
                return RedirectToAction("SaleProducts");
            }

        }
        [HttpPost]
        public IActionResult UpdateSaleProductInfo(SaleProductDetailModel saleProductDetailModel)
        {
            var saleProduct = _productService.GetSaleProductBySaleProductId(saleProductDetailModel.Id);
            if (saleProduct == null)
                return Error("数据错误，销售商品不存在！");

            foreach (var item in saleProductDetailModel.SaleProductPriceDetailModels)
            {
                var saleProductPrice = _productService.GetSaleProductPriceBySaleProductIdAndCustomerTypeId(saleProductDetailModel.Id, item.CustomerTypeId);
                if (saleProductPrice == null)
                {
                    var model = new SaleProductPrice
                    {
                        SaleProductId = saleProductDetailModel.Id,
                        CustomerTypeId = item.CustomerTypeId,
                        Price = item.Price
                    };

                    _productService.CreateSaleProductsPrice(model);
                }
                else
                {
                    saleProductPrice.Price = item.Price;
                    _productService.UpdateSaleProductPrice(saleProductPrice);
                }
            }
            return Success();
        }
        public bool AddSaleProductDefaultPrice(int saleProductId)
        {
            var CustomerTypeId = _commonService.GetBaseDictionaryList(DictionaryType.PriceType);
            foreach (var item in CustomerTypeId)
            {
                SaleProductPrice saleProductPrice = new SaleProductPrice();
                saleProductPrice.SaleProductId = saleProductId;
                saleProductPrice.CustomerTypeId = item.Id;
                saleProductPrice.Price = 0;
                _productService.CreateSaleProductsPrice(saleProductPrice);
            }
            return true;
        }
        public bool AddSaleProductPrice(int saleProductId, PriceModel priceModel)
        {
            List<dynamic> data = new List<dynamic>();
            dynamic bz = new dynamic[2] { priceModel.BzPriceId, priceModel.BzPrice };
            dynamic tg = new dynamic[2] { priceModel.TgPriceId, priceModel.TgPrice };
            dynamic jxs = new dynamic[2] { priceModel.JxsPriceId, priceModel.JxsPrice };
            dynamic nb = new dynamic[2] { priceModel.NbPriceId, priceModel.NbPrice };
            dynamic pf = new dynamic[2] { priceModel.PfPriceId, priceModel.PfPrice };
            data.Add(bz);
            data.Add(tg);
            data.Add(jxs);
            data.Add(nb);
            data.Add(pf);
            foreach (var i in data)
            {
                SaleProductPrice saleProductPrice = new SaleProductPrice();
                saleProductPrice.SaleProductId = saleProductId;
                saleProductPrice.CustomerTypeId = i[0];
                saleProductPrice.Price = i[1];
                _productService.CreateSaleProductsPrice(saleProductPrice);
            }
            return true;
        }
        public int SaleProductPriceChannel(string name)
        {
            var channel = _commonService.GetBaseDictionaryList(DictionaryType.Channel);
            var findChannel = channel.Find(s => s.Value.Equals(name));
            return findChannel.Id;
        }
        #endregion


        #region 平台商品
        public IActionResult PlatformProducts()
        {
            var getPlatformProducts = _productService.GetAllPlantformProducts();
            dynamic PlatformProducts = TurnJson(getPlatformProducts);
            return View(PlatformProducts);
        }
        //添加平台商品
        public IActionResult CreatedPlatformProduct()
        {
            var saleProductModel = _productService.GetAllSaleProducts();
            dynamic saleProduct = TurnJson(saleProductModel);
            ViewBag.Platform = new SelectList(_commonService.GetDictionaryList(new List<DictionaryType>() { DictionaryType.Platform, DictionaryType.ThirdpartyOnlineSales, DictionaryType.Consignment }), "Id", "Value");
            return View(saleProduct);
        }
        [HttpPost]
        public IActionResult CreatedPlatformProduct(PlatformProductModel platformProductModel)
        {
            if (_productService.ConfirmPlatformProductIfExist(platformProductModel.SaleProductId, platformProductModel.PlatForm))
            {
                ErrorNotification("已存在平台商品！");
                return RedirectToAction("CreatedPlatformProduct");
            }
            else
            {
                PlatformProduct platformProduct = platformProductModel.ToEntity();
                _productService.CreatedPlatformProduct(platformProduct);
                SuccessNotification("添加成功！");
                return RedirectToAction("PlatformProducts");
            }
        }
        //删除平台商品
        public IActionResult DelPlatformProduct(int id)
        {
            _productService.DelPlatformProduct(id);
            return RedirectToAction("PlatformProducts");
        }
        #endregion


        #region 其他
        public dynamic TurnJson(IQueryable queryable)
        {
            var json = JsonConvert.SerializeObject(queryable);//iqueryable数据序列化
            dynamic jsonresult = JsonConvert.DeserializeObject(json);//iqueryable数据反序列化
            return jsonresult;
        }

        #endregion


        #region 获取商品列表
        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetProductList(SearchModel searchModel)
        {

            if (searchModel.Length == 0)
            {
                searchModel.Length = 10;
            }
            if (searchModel.Start == 0)
            {
                searchModel.PageIndex = 1;
            }
            if (searchModel.Start > 0)
            {
                searchModel.PageIndex = searchModel.Start / searchModel.Length + 1;
            }
            var search = new SearchProductContext
            {
                PageIndex = searchModel.PageIndex,
                PageSize = searchModel.Length,
                SearchStr = searchModel.Search["value"]
            };

            try
            {
                var productList = _productService.GetProductViewModels(search);
                var result = new SearchResultModel
                {
                    Data = productList,
                    Draw = searchModel.Draw,
                    RecordsTotal = productList.TotalCount,
                    RecordsFiltered = productList.TotalCount,
                    isSucc = true,
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new SearchResultModel
                {
                    isSucc = false
                };

                return Json(result);
            }
        }

        /// <summary>
        /// 获取销售商品列表
        /// </summary>
        /// <returns></returns>
        public IActionResult GetSaleProductList(SearchModel searchModel)
        {


            if (searchModel.Length == 0)
            {
                searchModel.Length = 10;
            }
            if (searchModel.Start == 0)
            {
                searchModel.PageIndex = 1;
            }
            if (searchModel.Start > 0)
            {
                searchModel.PageIndex = searchModel.Start / searchModel.Length + 1;
            }

            var search = new SearchProductContext
            {
                PageIndex = searchModel.PageIndex,
                PageSize = searchModel.Length,
                SearchStr = searchModel.Search["value"]
            };
            try
            {
                var saleproductList = _productService.GetSaleProductPriceLists(search);
                var result = new SearchResultModel
                {
                    Data = saleproductList,
                    Draw = searchModel.Draw,
                    RecordsTotal = saleproductList.TotalCount,
                    RecordsFiltered = saleproductList.TotalCount,
                    isSucc = true,
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new SearchResultModel
                {
                    isSucc = false
                };

                return Json(result);
            }
        }
        public IActionResult GetAllSaleProducts(int pageSize,int pageIndex,string searchStr = "")
        {
            return Success(_productService.GetSaleProductsPageByOrderType(94, pageSize, pageIndex, searchStr));
        }
        #endregion


        #region 库存查询
        /// <summary>
        /// 库存查询
        /// </summary>
        /// <returns></returns>
        public IActionResult QueryProductWHStock() {
            if (!_permissionService.Authorize("ViewProducts"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.WareHouses = new SelectList(_wareHouseService.GetWareHouses(), "Id", "Name");
            return View();
        }
        /// <summary>
        /// 获取商品库存列表
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="searchOrderContext"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetQueryStockTable(SearchProductContext searchProductContext) {
            try
            {
                var data = _productService.GetWareHouseStockViewModels(searchProductContext,out int allStock, out int allLockStock, out int allAvailableStock);
                var sumStock = data.Sum(x=>x.Stock);
                var sumLockStock = data.Sum(x=>x.LockStock);
                var sumAvailableStock = data.Sum(x=>x.AvailableStock);
                var result = new SearchWHStockResultModel
                {
                    Data = data,
                    Draw = 1,
                    RecordsTotal = data.TotalCount,
                    RecordsFiltered = data.TotalCount,
                    isSucc = true,
                    AllStock = allStock,
                    AllLockStock =allLockStock,
                    AllAvailableStock = allAvailableStock,
                    SumStock = sumStock,
                    SumLockStock = sumLockStock,
                    SumAvailableStock =sumAvailableStock
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                _logService.Error("库存查询错误:"+ex.Message);

                var result = new SearchWHStockResultModel
                {
                    isSucc = false
                };

                return Json(result);
            }
        }
        /// <summary>
        /// 根据商品Id获取商品在WMS中的总库存
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetWMSProductStock(int productId)
        {
            try
            {
                var content = _productService.GetWMSProdcutStockByProductId(productId);
                var result = content.ToObj<GetStockResultModel>();
                if (result.isSucc)
                {
                    return Json(new { isSucc = true, msg = result.msg, count = result.count });
                }
                else
                    return Json(new { isSucc = false, msg = result.msg });
            }
            catch (Exception ex)
            {
                return Json(new { isSucc = false, msg = "出现异常，原因：" + ex.Message });

            }
        }
        /// <summary>
        /// 同步商品库存
        /// </summary>
        /// <returns></returns>
        public IActionResult SycnWarehouseProductStock(int saleProductId)
        {

            try
            {
                var saleProduct = _productService.GetSaleProductBySaleProductId(saleProductId);
                if (saleProduct == null) return Error("没有找到销售商品！");
                var sycnRes = _productService.SyncStocksWmsToOms(saleProduct.ProductId);
                if (!sycnRes) return Error("同步商品库存失败！");

                return Success();
            }
            catch (Exception ex)
            {

                _logService.Error("同步商品库存异常：" + ex.Message);
                return Error("同步商品库存时发生异常！");
            }
        }
        #endregion


        #region 套装商品
        public IActionResult SuitProducts()
        {
            return View();
        }
        public IActionResult SuitProductsDetail(int id)
        {
            var data = _productService.GetSuitProductsById(id);
            return View(data);
        }
        public IActionResult GetAllSuitProducts(string searchStr, int pageSize, int pageIndex)
        {
            var data = _productService.GetAllSuitProducts().Where(r => (string.IsNullOrEmpty(searchStr) || r.Name.Contains(searchStr) || r.Code.Contains(searchStr) || r.Mark.Contains(searchStr)));
            return Success(new PageList<SuitProducts>(data, pageIndex, pageSize,data.Count()));
        }
        public IActionResult InsertSuitProducts(SuitProducts suitProducts)
        {
            var isExsit = _productService.GetSuitProductsByNameOrCode(suitProducts);
            if (isExsit != null && isExsit.Count() > 0)
            {
                return Error("已有同款商品名或者编码！");
            }
            _productService.InsertSuitProducts(suitProducts);
            return Success();
        }
        public IActionResult InsertSuitProductsDetail(SuitProductsDetail suitProductsDetail)
        {
            //判断是否已经有此商品
            var data = _productService.GetSuitProductsDetailById(suitProductsDetail.SuitProductsId, suitProductsDetail.SaleProductId);
            if (data != null)
            {
                return Error("该套装已有此商品，请勿重复添加！");
            }
            _productService.InsertSuitProductsDetail(suitProductsDetail);
            return Success();
        }
        public IActionResult DeleteSuitProductsDetail(int suitProDetailId,int suitProId)
        {
            //权限
            try
            {
                _productService.DeleteSuitProductsDetailById(suitProDetailId);
                #region 日志
                _logService.InsertOrderTableLog("SuitProducts", suitProId, "删除商品", 0, "删除商品");
                #endregion
                return Success();
            }
            catch(Exception e)
            {
                return Error(e.Message);
            }
        }
        public IActionResult UpdateSuitProducts(SuitProducts suitProducts)
        {
            var isExist= _productService.GetSuitProductsByNameOrCode(suitProducts);
            if (isExist != null && isExist.Count() > 0)
            {
                var isHas = isExist.Select(r => r.Id).ToList().Except(new List<int> { suitProducts.Id });
                if (isHas.Count() > 0)
                {
                    return Error("已有同名或者同编号的套装商品");
                }
            }
            var result = _productService.GetSuitProductsBySuitProId(suitProducts.Id);
            result.Name = suitProducts.Name;
            result.Code = suitProducts.Code;
            _productService.UpdateSuitProducts(result);
            return Success();
        }
        public IActionResult UpdateSuitProductsDetail(int suitProDetailId,int qty,int suitProId)
        {
            //权限
            try
            {
                var data = _productService.GetSuitProductsDetail(suitProDetailId);
                data.Quantity = qty;
                _productService.UpdateSuitProductsDetail(data);
                #region 日志
                _logService.InsertOrderTableLog("SuitProducts", suitProId, "修改商品信息", 0, "修改商品信息");
                #endregion
                return Success();
            }
            catch(Exception e)
            {
                return Error(e.Message);
            }

        }
        #endregion
    }
}