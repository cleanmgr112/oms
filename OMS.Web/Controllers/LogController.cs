using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OMS.Data.Domain;
using OMS.Model.Log;
using OMS.Services.Log;

namespace OMS.Web.Controllers
{
    public class LogController : BaseController
    {
        private ILogService _logService;
        #region
        public LogController(ILogService logService)
        {
            this._logService = logService;
        }
        #endregion

        public IActionResult SystemLogIndex()
        {
            ViewBag.LogTypes = new SelectList(EnumExtensions.GetEnumList((Enum)LogLevelEnum.All), "Key", "Value");
            return View();
        }

        [HttpPost]
        public IActionResult GetAllSystemLogList(int pageIndex, int pageSize, LogLevelEnum logType ,string search, DateTime? startTime,DateTime? endTime)
        {
            var result = _logService.GetSystemLogModelsByPage(pageIndex, pageSize, logType, search, startTime, endTime);
            return Success(result);
        }
        [HttpPost]
        public IActionResult DeleteSystemLog(int id)
        {
            var systemLog = _logService.GetSystemLogById(id);
            if(systemLog==null)
            {
                return Error("数据错误，该日志不存在");
            }
            _logService.DeleteSystemLog(systemLog);
            return Success();
        }

        public IActionResult SystemLogDetail(int id)
        {
            var log = _logService.GetSystemLogById(id);
            var model = new SystemLogModel
            {
                Id = log.Id,
                LogTypeStr = log.LogLevel.Description(),
                Description = log.Description,
                Content = log.Content,
                IpAddress = log.IpAddress,
                ReferenceUrl = log.ReferenceUrl,
                CreatedTimeStr = log.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss")
            };
            return View(model);
        }
    }
}