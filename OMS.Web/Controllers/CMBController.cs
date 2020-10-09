using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Services.CMB;
using OMS.Services.Common;
using OMS.Services.Permissions;

namespace OMS.Web.Controllers
{
    public class CMBController : BaseController
    {
        #region ctor
        private readonly ICMBService _cmbService;
        private readonly ICommonService _commonService;
        private readonly IPermissionService _permissionService;
        public CMBController(ICMBService cmbService, ICommonService commonService, IPermissionService permissionService)
        {
            _cmbService = cmbService;
            _commonService = commonService;
            _permissionService = permissionService;
        }
        #endregion

        
        #region 页面
        public IActionResult Index()
        {
            return View();
        }
        #endregion


        #region 操作
        [HttpPost]
        public IActionResult ShowCMBOrderDetail(int pageSize,int pageIndex,string searchStr)
        {
            var data = _cmbService.GetCMBOrderLog(searchStr);
            return Success(new PageList<CMBOrderLog>(data.OrderByDescending(r => r.CreatedTime).AsQueryable(), pageIndex, pageSize));
        }
        public IActionResult SyncOrderFromCMB(DateTime startTime,DateTime endTime)
        {
            if (!_permissionService.Authorize("SyncOrderFromCMB"))
            {
                return Error("无操作权限！");
            }
            var result = _cmbService.InsertOrderToOMS(_cmbService.GetAllOrderList(startTime, endTime));
            if (result != "")
            {
                return Error(result);
            }
            return Success();
        }
        #endregion
    }
}
