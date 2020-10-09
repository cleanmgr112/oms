using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using OMS.Data.Domain;
using OMS.Services.Log;
using OMS.Services.Configuration;
using OMS.Services.Permissions;
using OMS.Services.Common;
using OMS.Services.Account;
using Hangfire;
using OMS.Services.ScheduleTasks;
namespace OMS.Web.Controllers
{
    public class ScheduleTaskController : BaseController
    {

        #region ctor
        private readonly ILogService _logService;
        private readonly ICommonService _commonService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IExecuteService _executeService;

        public ScheduleTaskController(ILogService logService, ICommonService commonService, IHostingEnvironment hostingEnvironment, IUserService userService, IPermissionService permissionService, IScheduleTaskService scheduleTaskService,IExecuteService executeService) {
            _logService = logService;
            _commonService = commonService;
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
            _permissionService = permissionService;
            _scheduleTaskService = scheduleTaskService;
            _executeService = executeService;
        }
        #endregion
        public IActionResult Index()
        {
            if (!_permissionService.Authorize("ViewScheduleTask")) {
                return View("_AccessDeniedView");
            }
            var scheduleTask = _scheduleTaskService.GetAllScheduleTasks();
            return View(scheduleTask);
        }

        public IActionResult AddScheduleTask(ScheduleTask scheduleTask) {
            if (!_permissionService.Authorize("AddScheduleTask")) {
                return Error("无操作权限！");
            }
            var result = _scheduleTaskService.ConfirmScheduleTaskIsExist(scheduleTask);
            if (result == null)
            {
                _scheduleTaskService.AddScheduleTask(scheduleTask);
                return Success();
            }
            else {
                return Error("已存在相同的系统任务");
            }
        }

        public IActionResult UpdateScheduleTask(ScheduleTask scheduleTask) {
            if (!_permissionService.Authorize("EditScheduleTask"))
            {
                return Error("无操作权限！");
            }
            _scheduleTaskService.UpdateScheduleTask(scheduleTask);
            return Success();
        }

        public IActionResult GetScheduleTaskInfo(int id)
        {
            var result = _scheduleTaskService.GetScheduleById(id);
            return Success(result);
        }

        public IActionResult DelScheduleTask(int scheduleTaskId)
        {
            if (!_permissionService.Authorize("DeleteScheduleTask"))
            {
                return Error("无操作权限！");
            }
            _scheduleTaskService.DelScheduleTask(scheduleTaskId);
            return Success();
        }

        /// <summary>
        /// 手动执行任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult ExecuteScheduleTask(int id) {
            if (!_permissionService.Authorize("ExecuteScheduleTask"))
            {
                return Error("无操作权限！"); 
            }
            var scheduleTask = _scheduleTaskService.GetScheduleById(id);
            try
            {
                //BackgroundJob.Enqueue<ExecuteService>(a => a.Execute(scheduleTask.Functions, scheduleTask.Type));
                _executeService.Execute(scheduleTask.Functions,scheduleTask.Type);
                return Success();
            }
            catch (Exception)
            {

                return Error("执行失败");
            }
            
        }
    }
}