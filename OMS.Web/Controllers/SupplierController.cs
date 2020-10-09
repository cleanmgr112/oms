using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OMS.Data.Domain.Suppliers;
using OMS.Services;
using OMS.Services.Permissions;
using OMS.Services.ScheduleTasks;

namespace OMS.Web.Controllers
{
    public class SupplierController : BaseController
    {
        #region
        private readonly IPurchasingService _purchasingService;
        private readonly IPermissionService _permissionService;
        private readonly IScheduleTaskFuncService _scheduleTaskFuncService;
        public SupplierController(IPurchasingService purchasingService, IPermissionService permissionService,IScheduleTaskFuncService scheduleTaskFuncService)
        {
            _purchasingService = purchasingService;
            _permissionService = permissionService;
            _scheduleTaskFuncService = scheduleTaskFuncService;
        }
        #endregion
        public IActionResult Index()
        {
            if (!_permissionService.Authorize("ViewSupplier"))
            {
                return View("_AccessDeniedView");
            }
            var data = _purchasingService.GetAllSuppliers();
            return View(data);
        }
        [HttpPost]
        public IActionResult AddSupplier(Supplier supplier)
        {
            if (!_permissionService.Authorize("AddSupplier"))
            {
                return Error("没有此权限！");
            }
            supplier.SupplierName = supplier.SupplierName.Trim();
            if (supplier.SupplierName != null && _purchasingService.ComfirmSupplierIsExist(supplier.SupplierName) == null)
            {
                _purchasingService.AddSupplier(supplier);
            }
            else
            {
                return Error("已有此供应商！");
            }
            //将供应商数据传递到WMS进行更新
            _scheduleTaskFuncService.OmsSyncSuppliers();
            return Success();
        }
        public IActionResult GetSupplierDetail(int id)
        {
            var data = _purchasingService.GetSupplierById(id);
            return Success(data);
        }
        public IActionResult EditSupplier(Supplier supplier)
        {
            if (!_permissionService.Authorize("EidtSupplier"))
            {
                return Error("没有此权限！");
            }
            try
            {
                supplier.SupplierName = supplier.SupplierName.Trim();
                var result = _purchasingService.ComfirmSupplierIsExist(supplier.SupplierName);
                if (result == null)
                {
                    _purchasingService.UpdataSupplier(supplier);
                    return Success("修改成功！");
                }
                else
                {
                    return Error("修改失败，已有同名供应商名！");
                }

            }catch(Exception ex)
            {
                return Error("修改失败");
            }

        }
        public IActionResult DelSupplier(int id)
        {
            if (!_permissionService.Authorize("DeleteSupplier"))
            {
                return Error("没有此权限！");
            }
            var result=_purchasingService.DeleteSuppllier(id);
            if (result)
            {
                return Success();
            }
            else
            {
                return Error();
            }
        }
        public IActionResult SetIsvalid(int id)
        {
            _purchasingService.SetSupplierIsvalid(id);
            return Success();
        }

    }
}