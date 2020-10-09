using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OMS.Data.Domain;
using OMS.Model;
using OMS.Services;
using OMS.Services.Permissions;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class WareHouseController : BaseController
    {
        #region ctor
        private readonly IWareHouseService _wareHouseService;
        private readonly IAuthenticationService _authenticationsService;
        private readonly IPermissionService _permissionService;
        public WareHouseController(IWareHouseService wareHouseService, IAuthenticationService authenticationService, IWareHouseService commonService,IPermissionService permissionService)
        {
            _wareHouseService = wareHouseService;
            _authenticationsService = authenticationService;
            _permissionService = permissionService;
        }
        #endregion

        #region 页面
        /// <summary>
        /// 仓库管理
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (!_permissionService.Authorize("ViewWareHouseManage"))
                return View("_AccessDeniedView");
            List<WareHouse> list = _wareHouseService.GetWareHouses();
            foreach (var item in list)
            {
                item.WareHouseTypeName = item.WareHouseType.Description();
            }
            return View(list);
        }
        /// <summary>
        /// 添加仓库
        /// </summary>
        /// <returns></returns>
        public IActionResult Add()
        {
            if (!_permissionService.Authorize("AddWareHouse"))
                return View("_AccessDeniedView");
            return View();
        }
        /// <summary>
        /// 仓库详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Detail(int id)
        {
            if (!_permissionService.Authorize("ViewWareHouseDetail"))
                return View("_AccessDeniedView");
            var data = _wareHouseService.GetById(id);
            ViewBag.WareHouseType = new SelectList(GetWareHouseTypeList(), "Type", "Name", data.WareHouseType.GetHashCode());
            return View(data);
        }
        #endregion

        #region 操作
        [HttpPost]
        public IActionResult Add(WareHouse wareHouse)
        {
            if (!_permissionService.Authorize("AddWareHouse"))
                return View("_AccessDeniedView");
            string name = wareHouse.Name.Trim();
            string code = wareHouse.Code.Trim();
                if (_wareHouseService.GetCountByName(name, code))
                {
                    return Error("已有仓库名或仓库代码！");
                }
                else
                {
                    _wareHouseService.Add(wareHouse);
                    return Success();
                }
        }
        [HttpPost]
        public IActionResult Detail(WareHouse wareHouse)
        {
            if (!_permissionService.Authorize("EditWareHouse"))
                return View("_AccessDeniedView");
            var result = _wareHouseService.GetCountByCode(wareHouse.Code);
            var data = result.ToList().Where(r => r.Id != wareHouse.Id);
            if (data.Count() > 0)
            {
                return Error("已有相同编码仓库");
            }
            _wareHouseService.UpdateWareHouse(wareHouse);
            return Success();
        }
        ///// <summary>
        ///// 删除仓库
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public IActionResult Del(int id)
        //{
        //    if (!_permissionService.Authorize("DeleteWareHouse"))
        //        return Error("无操作权限！");
        //    _wareHouseService.DelWareHouseById(id);
        //    return Success("删除成功！");
        //}
        public IActionResult GetAllWareHouse()
        {
            List<WareHouse> list = _wareHouseService.GetWareHouses();
            foreach (var item in list)
            {
                item.WareHouseTypeName = item.WareHouseType.Description();
            }
            return Json(list);
        }
        #endregion


        #region 仓库区域
        /// <summary>
        /// 仓库区域管理
        /// </summary>
        /// <returns></returns>
        public IActionResult WareHouseArea()
        {
            ViewBag.WareHouses = _wareHouseService.GetAllWareHouseList();
            return View();  
        }

        /// <summary>
        /// 获取仓库区域列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult WareHouseAreaListJson(string areaname, string areacode, int page = 1, int limit = 10)
        {
            var searchModel = new SearchWareHouseAreaContext
            {
                AreaName = areaname,
                AreaCode = areacode,
                PageIndex = page,
                PageSize = limit
            };

            var pageData = _wareHouseService.GetWareHouseAreaViewModels(searchModel);
            return Json(new {
                draw = 1,
                recordsTotal = pageData.TotalCount,
                recordsFiltered = pageData.TotalCount,
                data = pageData
            });
        }

        /// <summary>
        /// 添加仓库区域
        /// </summary>
        /// <param name="areaName"></param>
        /// <param name="areaCode"></param>
        /// <param name="ranksArrs"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddWareHouseArea(string areaName,string areaCode,List<WareHouseAreaModel> ranksArrs)
        {
            try
            {
                var wareHouseArea = _wareHouseService.GetWareHouseAreaByName(areaName);
                if (wareHouseArea != null)
                    return Error("已存在相同名称的区域");

                var model = new WareHouseArea
                {
                    AreaCode = areaCode,
                    AreaName = areaName,
                    WhId = ranksArrs.Count() > 0 ? ranksArrs.OrderByDescending(r => r.Rank).FirstOrDefault().WareHouseId : 0,
                };
                _wareHouseService.AddWareHouseArea(model);

                foreach (var item in ranksArrs)
                {
                    var rank = new WareHouseAreaRanks
                    {
                        WhAId = model.Id,
                        WhId = item.WareHouseId,
                        Rank = item.Rank,
                    };
                    _wareHouseService.AddWareHouseAreaRanks(rank);
                }
                return Success();
            }
            catch(Exception ex)
            {
                return Error(ex.Message);
            }

        }

        public IActionResult GetWareHouseAreaById(int id)
        {
            var model = _wareHouseService.GetWareHouseAreaDetailModelById(id);
            if (model == null)
                return Error("数据错误！");
            return Success(model);
        }
        /// <summary>
        /// 更新仓库区域
        /// </summary>
        /// <param name="areaName"></param>
        /// <param name="areaCode"></param>
        /// <param name="ranksArrs"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UpdateWareHouseArea(int id, string areaName, string areaCode, List<WareHouseAreaModel> ranksArrs)
        {
            try
            {
                var wareHouseArea = _wareHouseService.GetWareHouseAreaById(id);
                if (wareHouseArea == null)
                    return Error("数据错误，仓库区域未找到！");
                var model = _wareHouseService.GetWareHouseAreaByName(areaName);
                if (model != null && model.Id != wareHouseArea.Id)
                    return Error("已存在想同名称的区域");

                wareHouseArea.AreaCode = areaCode;
                wareHouseArea.AreaName = areaName;
                wareHouseArea.WhId = ranksArrs.Count() > 0 ? ranksArrs.OrderByDescending(r => r.Rank).FirstOrDefault().WareHouseId : 0;
                _wareHouseService.UpdateWareHouseArea(wareHouseArea);

                foreach (var item in ranksArrs)
                {
                    if (item.Id== 0) {
                        var warehouseAreaRank = new WareHouseAreaRanks()
                        {
                            WhAId = wareHouseArea.Id,
                            WhId = item.WareHouseId,
                            Rank = 0
                        };
                        _wareHouseService.AddWareHouseAreaRanks(warehouseAreaRank);
                    }
                    else
                    {
                        var rank = _wareHouseService.GetWHARanksById(item.Id);

                        rank.Rank = item.Rank;

                        _wareHouseService.UpdateWareHouseAreaRanks(rank);
                    }
                }

                return Success();
            }
            catch(Exception ex)
            {
                return Error(ex.Message);
            }
        }
        #endregion


        #region 其他
        List<WareHouseType> GetWareHouseTypeList()
        {
            List<WareHouseType> whType = new List<WareHouseType>();
            foreach (var item in Enum.GetValues(typeof(WareHouseTypeEnum)))
            {
                whType.Add(new WareHouseType { Type = item.GetHashCode(), Name = ((DescriptionAttribute)(item.GetType().GetField(item.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false)[0])).Description });
            };
            return whType;
        }
        #endregion
    }
}