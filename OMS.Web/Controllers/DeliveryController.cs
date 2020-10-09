using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OMS.Data.Domain;
using OMS.Services.Account;
using OMS.Services.Common;
using OMS.Services.Deliveries;
using OMS.Services.Log;
using OMS.Services.Permissions;
using OMS.Services.ScheduleTasks;

namespace OMS.Web.Controllers
{
    
    public class DeliveryController : BaseController
    {
        #region
        private readonly ILogService _logService;
        private readonly ICommonService _commonService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUserService _userService;
        private readonly IDeliveriesService _deliveriesService;
        private readonly IPermissionService _permissionService;
        private readonly IScheduleTaskFuncService _scheduleTaskFuncService;
        public DeliveryController( ILogService logService, ICommonService commonService, IHostingEnvironment hostingEnvironment, IUserService userService,IDeliveriesService deliveriesService,IPermissionService permissionService,IScheduleTaskFuncService scheduleTaskFuncService)
        {
            _commonService = commonService;
            _hostingEnvironment = hostingEnvironment;
            _logService = logService;
            _userService = userService;
            _deliveriesService = deliveriesService;
            _permissionService = permissionService;
            _scheduleTaskFuncService = scheduleTaskFuncService;
        }
        #endregion
        public IActionResult Index()
        {
            if (!_permissionService.Authorize("ViewDelivery"))
            {
                return View("_AccessDeniedView");
            }
            var deliveries = _deliveriesService.GetAllDeliveries();
            return View(deliveries);
        }
        public IActionResult AddDelivery(Delivery delivery)
        {
            if (!_permissionService.Authorize("AddDelivery"))
            {
                return Error("无操作权限！");
            }
            delivery.Code = delivery.Code.Trim();
            delivery.Name = delivery.Name.Trim();
            var result = _deliveriesService.ConfirmDeliveryIsExist(delivery.Code);
            delivery.ShopCode = delivery.ShopCode?.Trim();
            if (result == null)
            {
                _deliveriesService.AddDelivery(delivery);
                //更新快递方式信息到WMS
                _scheduleTaskFuncService.OmsSyncDeliverys();
                return Success();
            }
            else
            {
                return Error("已有相同编码的物流方式");
            }
        }
        public IActionResult UpdateDelivery(Delivery delivery)
        {
            if (!_permissionService.Authorize("UpdateDelivery"))
            {
                return Error("无操作权限！");
            }
            delivery.Code = delivery.Code.Trim();
            delivery.Name = delivery.Name.Trim();
            delivery.ShopCode = delivery.ShopCode?.Trim();
            var result = _deliveriesService.ConfirmDeliveryIsExist(delivery.Code);
            if (result == null || result.Id == delivery.Id)
            {
                if (result == null)
                {
                    _deliveriesService.UpdateDelivery(delivery);
                    return Success();
                }
                else
                {
                    result.Name = delivery.Name;
                    result.Code = delivery.Code;
                    result.ShopCode = delivery.ShopCode;
                    _deliveriesService.UpdateDelivery(result);
                    return Success();
                }
            }
            else
            {
                return Error("已有相同编码物流方式！");
            }
        }
        public IActionResult GetDeliveryInfo(int id)
        {
            var result = _deliveriesService.GetDeliveryById(id);
            return Success(result);
        }
        public IActionResult DelDelivery(int deliveryId)
        {
            if (!_permissionService.Authorize("DeleteDelivery"))
            {
                return Error("无操作权限！");
            }
            _deliveriesService.DelDelivery(deliveryId);
            return Success();
        }
    }
}