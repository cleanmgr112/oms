using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Domain.SalesMans;
using OMS.Data.Interface;
using OMS.Services;
using OMS.Services.Common;
using OMS.Services.Customer;
using OMS.Services.Permissions;
using OMS.Services.ScheduleTasks;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class CustomerController : BaseController
    {
        #region
        private readonly ICustomerService _customerService;
        private readonly ICommonService _commonService;
        private readonly IAuthenticationService _authenticationsService;
        private readonly IPermissionService _permissionService;
        private readonly ISalesManService _salesManService;
        private readonly IScheduleTaskFuncService _scheduleTaskFuncService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDbAccessor _omsAccessor;

        public CustomerController(ICustomerService customerService,IAuthenticationService authenticationService,ICommonService commonService,IPermissionService permissionService,
            ISalesManService salesManService,IScheduleTaskFuncService scheduleTaskFuncService, IHostingEnvironment hostingEnvironment, IDbAccessor omsAccessor)
        {
            _customerService = customerService;
            _authenticationsService = authenticationService;
            _commonService = commonService;
            _permissionService = permissionService;
            _salesManService = salesManService;
            _scheduleTaskFuncService = scheduleTaskFuncService;
            _hostingEnvironment = hostingEnvironment;
            _omsAccessor = omsAccessor;
        }
        #endregion


        #region 页面
        /// <summary>
        ///客户管理主页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (!_permissionService.Authorize("ViewCutomers"))
            {
                return View("_AccessDeniedView");
            }
            return View();
        }
        /// <summary>
        /// 添加客户
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            if (!_permissionService.Authorize("ViewAddCutomers"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.CustomerType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.CustomerType), "Id", "Value");
            ViewBag.PriceType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PriceType), "Id", "Value");
            return View();
        }
        /// <summary>
        /// 客户详情及修改客户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Detail(int id)
        {
            if (!_permissionService.Authorize("ViewUpdateCutomers"))
            {
                return View("_AccessDeniedView");
            }
            var data = _customerService.GetById(id);
            ViewBag.CustomerType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.CustomerType), "Id", "Value", data.CustomerTypeId);
            ViewBag.PriceType = new SelectList(_commonService.GetBaseDictionaryList(DictionaryType.PriceType), "Id", "Value", data.PriceTypeId);
            return View(data);
        }
        #endregion


        #region 操作
        [HttpPost]
        public IActionResult GetAllCustomers(int pageSize, int pageIndex, string searchStr, int type = 1)
        {
            if (type != 1)
            {
                if (!_permissionService.Authorize("ExportCutomers"))
                {
                    return Error("没有操作权限！");
                }
            }
            var result = _customerService.GetAllCustomerList(searchStr);
            return Success(new PageList<Customers>(result.AsQueryable(), pageIndex, pageSize));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Customers customer)
        {
            if (!_permissionService.Authorize("AddCutomers"))
            {
                return View("_AccessDeniedView");
            }
            string name = customer.Name.ToString();
            if (_customerService.GetCountByName(name))
            {
                return Error("已有同名客户！");
            }
            else
            {
                customer.PriceTypeId = _commonService.GetBaseDictionaryList(DictionaryType.PriceType).FirstOrDefault().Id;
                customer.DisCount = 1;
                _customerService.Add(customer);
                //更新客户信息到WMS
                _scheduleTaskFuncService.OmsSyncCustomers();
                return Success("添加成功！");
            }
        }
        /// <summary>
        /// 删除客户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Del(int id)
        {
            if (!_permissionService.Authorize("DeleteCutomers"))
            {
                return Error("无操作权限！");
            }
            _customerService.DelCustomerById(id);
            return Success();
        }
        [HttpPost]
        public IActionResult Detail(Customers customers)
        {
            if (!_permissionService.Authorize("UpdateCutomers"))
            {
                return View("_AccessDeniedView");
            }
            if (ModelState.IsValid)
            {
                _customerService.UpdateCustomer(customers);
            }
            return Success();
        }
        [HttpPost]
        public IActionResult CustomerImport(IFormFile file)
        {
            IWorkbook workbook = null;
            if (file == null)
            {
                return Error("请添加要导入文件");
            }
            else
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                fileName = _hostingEnvironment.WebRootPath + @"\CacheFile\" + fileName;
                using (FileStream fs = System.IO.File.Create(fileName))
                {
                    file.CopyTo(fs);
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
                int errorCount = 0;
                using (var tran = _omsAccessor.OMSContext.Database.BeginTransaction())
                {
                    try
                    {

                        for (int i = 1; i <= sheet.LastRowNum; i++)
                        {

                            row = sheet.GetRow(i);
                            var isEmpty = 0;
                            for (int j = 0; j < 4; j++)
                            {
                                if (string.IsNullOrEmpty(row.GetCell(j).ToString().Trim()))
                                {
                                    isEmpty++;
                                }   
                            }
                            if (isEmpty > 1)
                            {
                                continue;
                            }
                            //表格字段
                            var id = row.GetCell(0).ToString().Trim();
                            var name = row.GetCell(1).ToString().Trim();
                            var contact = row.GetCell(2).ToString().Trim();
                            var customerType = row.GetCell(3).ToString().Trim();
                            var mobile = row.GetCell(4).ToString().Trim();
                            var email = row.GetCell(5).ToString().Trim();
                            var address = row.GetCell(6).ToString().Trim();
                            var mark = row.GetCell(7).ToString().Trim();
                            var customerEmail = row.GetCell(8).ToString().Trim();
                            var title = row.GetCell(9).ToString().Trim();
                            var taxpayerId = row.GetCell(10).ToString().Trim();
                            var registerAddress = row.GetCell(11).ToString().Trim();
                            var registerTel = row.GetCell(12).ToString().Trim();
                            var bankOfDeposit = row.GetCell(13).ToString().Trim();
                            var bankAccount = row.GetCell(14).ToString().Trim();
                            if (string.IsNullOrEmpty(name))
                            {
                                errorStr.Append("第" + i + "行没有客户名称\r\n");
                                errorCount++;
                                continue;
                            }
                            if (string.IsNullOrEmpty(contact))
                            {
                                errorStr.Append("第" + i + "行没有联系人姓名\r\n");
                                errorCount++;
                                continue;
                            }
                            if (string.IsNullOrEmpty(customerType))
                            {
                                errorStr.Append("第" + i + "行没有客户类型\r\n");
                                errorCount++;
                                continue;
                            }
                            var customerTypeId = _commonService.GetDictionaryByTypeAndValue(DictionaryType.CustomerType, customerType);
                            if (customerTypeId == null)
                            {
                                errorStr.Append("第" + i + "行客户类型不存在\r\n");
                                errorCount++;
                                continue;
                            }
                            if (string.IsNullOrEmpty(id))
                            {
                                //新增
                                if (_customerService.GetCountByName(name))
                                {
                                    errorStr.Append("第" + i + "行已有该用户\r\n");
                                }
                                Customers customer = new Customers();
                                customer.Name = name;
                                customer.Contact = contact;
                                customer.CustomerTypeId = customerTypeId.Id;
                                customer.Mobile = mobile;
                                customer.Email = email;
                                customer.Address = address;
                                customer.Mark = mark;
                                customer.PriceTypeId = 103;
                                customer.DisCount = 1;
                                customer.CustomerEmail = customerEmail;
                                customer.Title = title;
                                customer.TaxpayerId = taxpayerId;
                                customer.RegisterAddress = registerAddress;
                                customer.RegisterTel = registerTel;
                                customer.BankAccount = bankAccount;
                                customer.BankOfDeposit = bankOfDeposit;
                                _customerService.Add(customer);
                            }
                            else
                            {
                                //修改
                                var customer = _customerService.GetById(int.Parse(id));
                                if (customer == null)
                                {
                                    errorStr.Append("第" + i + "行无法查找到改条客户信息\r\n");
                                    errorCount++;
                                    continue;
                                }
                                customer.Name = name;
                                customer.Contact = contact;
                                customer.CustomerTypeId = customerTypeId.Id;
                                customer.Mobile = mobile;
                                customer.Email = email;
                                customer.Address = address;
                                customer.Mark = mark;
                                customer.PriceTypeId = 103;
                                customer.DisCount = 1;
                                customer.CustomerEmail = customerEmail;
                                customer.Title = title;
                                customer.TaxpayerId = taxpayerId;
                                customer.RegisterAddress = registerAddress;
                                customer.RegisterTel = registerTel;
                                customer.BankAccount = bankAccount;
                                customer.BankOfDeposit = bankOfDeposit;
                                _customerService.UpdateCustomer(customer);
                            }
                        }

                        if (errorCount > 0 || errorStr.Length > 0)
                        {
                            string err = string.Format(errorStr.ToString());
                            tran.Rollback();
                            System.IO.File.Delete(fileName);
                            fileStream.Close();
                            return Error(err);
                        }
                        tran.Commit();
                        System.IO.File.Delete(fileName);
                        fileStream.Close();
                        return Success();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        System.IO.File.Delete(fileName);
                        fileStream.Close();
                        return Error("导入出错！");
                    }
                }
            }
        }
        #endregion


        #region  业务员管理模块

        /// <summary>
        /// 业务员管理列表页
        /// </summary>
        /// <returns></returns>
        public IActionResult SalesManList()
        {
            if (!_permissionService.Authorize("ViewSalesManList"))
                return View("_AccessDeniedView");
            ViewBag.Departments = new SelectList(EnumExtensions.GetEnumList((Enum)DepartmentType.AdministrativePersonnel), "Key", "Value");
            return View();
        }
        [HttpPost]
        public IActionResult GetSalesManList(int pageSize,int pageIndex, string searchVal)
        {
            var data = _salesManService.GetSalesMansByPage(pageIndex, pageSize, searchVal);
            return Success(data);
        }
        [HttpPost]
        public IActionResult AddSalesMan(string userName,string code,DepartmentType department)
        {
            if (!_permissionService.Authorize("UpdateSalesMan"))
                return Error("权限不足");

            var salesMan = _salesManService.GetSalesManByNameAndCode(userName, code);
            if (salesMan != null)
                return Error("已存在名称和编码相同的数据");

            var model = new SalesMan
            {
                UserName = userName,
                Code = code,
                Department = department
            };
            _salesManService.AddSalesMan(model);
            return Success();
        }

        [HttpPost]
        public IActionResult UpdateSalesMan(int id, string userName, string code, DepartmentType department)
        {
            if (!_permissionService.Authorize("UpdateSalesMan"))
                return Error("权限不足");
            var salesMan = _salesManService.GetSalesManById(id);
            if (salesMan == null)
                return Error("错误，数据不存在");

            var _salesMan = _salesManService.GetSalesManByNameAndCode(userName, code);
            if (_salesMan != null && salesMan.Id!=_salesMan.Id)
                return Error("已存在名称和编码相同的数据");

            salesMan.UserName = userName;
            salesMan.Code = code;
            salesMan.Department = department;

            _salesManService.UpdateSalesMan(salesMan);
            return Success();
        }

        [HttpPost]
        public IActionResult DeleteSalesMan(int id)
        {
            if (!_permissionService.Authorize("UpdateSalesMan"))
                return Error("权限不足");
            var salesMan = _salesManService.GetSalesManById(id);
            if (salesMan == null)
                return Error("错误，数据不存在");

            _salesManService.DeleteSalesMan(id);
            return Success();
        }
        [HttpPost]
        public IActionResult GetSalesManById(int id)
        {
            var salesMan = _salesManService.GetSalesManById(id);
            if (salesMan == null)
                return Error("错误，数据不存在");
            return Json(new { isSucc = true, data = salesMan, msg = "成功" });
        }

        #endregion

        #region 数据字典Dictionary管理模块

        public IActionResult DictionaryList()
        {
            if (!_permissionService.Authorize("ViewDictionary"))
            {
                return View("_AccessDeniedView");
            }
            ViewBag.DictionaryTypes = new SelectList(EnumExtensions.GetEnumList((Enum)DictionaryType.Area), "Key", "Value");
            return View();
        }

        [HttpPost]
        public IActionResult GetDictionaryList(int pageSize, int pageIndex,int? type, string searchVal="")
        {
            var data = _commonService.GetDictionariesByPage(pageIndex, pageSize, type, searchVal);
            return Success(data);
        }

        [HttpPost]
        public IActionResult GetDictionaryById(int id)
        {
            var dictionary = _commonService.GetDictionaryById(id);
            if (dictionary == null)
                return Error("错误，数据不存在");
            return Json(new { isSucc = true, data = dictionary, msg = "成功" });
        }

        [HttpPost]
        public IActionResult AddDictionary(string name, int sort, DictionaryType type,bool isSync)
        {
            if (!_permissionService.Authorize("UpdateDictionary"))
                return Error("权限不足");

            var dictionary = _commonService.GetDictionaryByTypeAndValue(type, name);
            if (dictionary != null)
                return Error("已存在名称和类型相同的数据");

            var model = new Dictionary
            {
                Value=name,
                Type=type,
                Sort=sort,
                IsSynchronized=isSync
            };
            _commonService.AddDictionary(model);
            //更新字典信息到WMS
            _scheduleTaskFuncService.OmsSyncDictionaries();
            return Success();
        }

        [HttpPost]
        public IActionResult UpdateDictionary(int id, string name, int sort, DictionaryType type, bool isSync)
        {
            if (!_permissionService.Authorize("UpdateDictionary"))
                return Error("权限不足");
            var dictionary = _commonService.GetDictionaryById(id);
            if (dictionary == null)
                return Error("错误，数据不存在");

            var _dictionary = _commonService.GetDictionaryByTypeAndValue(type, name);
            if (_dictionary != null && dictionary.Id!=_dictionary.Id)
                return Error("已存在名称和类型相同的数据");

            dictionary.Value = name;
            dictionary.Type = type;
            dictionary.Sort = sort;
            dictionary.IsSynchronized = isSync;

            _commonService.UpdateDictionary(dictionary);
            return Success();
        }

        [HttpPost]
        public IActionResult DeleteDictionary(int id)
        {
            if (!_permissionService.Authorize("DeleteDictionary"))
                return Error("权限不足");
            var dictionary = _commonService.GetDictionaryById(id);
            if (dictionary == null)
                return Error("错误，数据不存在");

            _commonService.DeleteDictionaryById(id);
            return Success();
        }

        #endregion
    }
}