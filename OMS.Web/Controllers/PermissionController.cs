using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OMS.Core;
using OMS.Data.Domain.Permissions;
using OMS.Model.Grid;
using OMS.Model.Menu;
using OMS.Model.Permission;
using OMS.Model.Role;
using OMS.Services.Permissions;
using OMS.WebCore;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class PermissionController : BaseController
    {
        #region ctor

        private readonly IWorkContext _workContext;
        private readonly IMenuService _menuService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IRolePermissionService _rolePermissionService;
        private readonly IUserPermissionService _userPermissionService;

        public PermissionController(
            IWorkContext workContext,
            IMenuService menuService,
            IRoleService roleService,
            IPermissionService permissionService,
            IRolePermissionService rolePermissionService,
            IUserPermissionService userPermissionService
            )
        {
            _workContext = workContext;
            _permissionService = permissionService;
            _menuService = menuService;
            _roleService = roleService;
            _rolePermissionService = rolePermissionService;
            _userPermissionService = userPermissionService;
        }

        #endregion

        #region 1、权限管理
        public IActionResult Index()
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("ViewPermission"))
            {
                return View();
            }
            else
            {
                return View("_AccessDeniedView");
            }
        }
        public IActionResult GetPermissionByPage(SearchModel searchModel)
        {
            if (searchModel != null)
            {
                //if (searchModel.Start == 0)
                //    searchModel.PageIndex = 1;
                //if (searchModel.Start > 0)
                //searchModel.PageIndex = searchModel.Start / 10 + 1;
                searchModel.PageIndex = (searchModel.Start / 20) + 1;
                if (searchModel.Search.Count != 0)
                {
                    foreach (var search in searchModel.Search)
                    {
                        if (search.Key == "value")
                        {
                            searchModel.SearchValue = search.Value;
                        }
                    }
                }

                var permissions = _permissionService.SearchPermissionListByPage(searchModel);
                var result = new SearchResultModel
                {
                    Data = permissions.ToList(),
                    Draw = searchModel.Draw,
                    RecordsTotal = permissions.TotalCount,
                    RecordsFiltered = permissions.TotalCount
                };
                return Json(result);
            }
            return ErrReturn("* 出现错误！");
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetCategorys()
        {
            var categoryList = _menuService.GetBaseMenus().ToList();

            var categorys = categoryList.Select(x =>
            {
                var item = new SelectItemModel
                {
                    Label = x.Name,
                    Value = x.Name
                };
                return item;
            }).ToList();

            categorys.Add(new SelectItemModel { Label = "其他", Value = "其他" });

            return Success(categorys);
        }

        [HttpPost]
        public IActionResult AddPermission(Dictionary<int, PermissionModel> data)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("AddPermission"))
            {
                if (data.Count != 1)
                    return ErrReturn("* 请选择一个权限进项操作");
                var model = data.Where(x => true).FirstOrDefault().Value;
                if (string.IsNullOrEmpty(model.Name))
                    return ErrReturn("name", "* 权限名称必填！");
                if (string.IsNullOrEmpty(model.SystemName))
                    return ErrReturn("systemName", "* 控制码必填！");
                if (string.IsNullOrEmpty(model.Category))
                    return ErrReturn("category", "* 请选择一个权限类型");
                if (_permissionService.GetPermissionByName(model.Name) != null)
                    return ErrReturn("name", "* 已存在相同名称的权限！");
                if (_permissionService.GetPermissionByCode(model.SystemName) != null)
                    return ErrReturn("systemName", "* 已存在相同名称的控制码！");

                try
                {
                    var permission = new Permission
                    {
                        Name = model.Name,
                        SystemName = model.SystemName,
                        Category = model.Category,
                        Isvalid = true,
                        State = model.State,
                        Remark = model.Remark
                    };
                    _permissionService.AddPermission(permission);

                    var result = new List<PermissionModel> { _permissionService.GetPermissionByName(model.Name).ToModel() };
                    return Success(result);
                }
                catch (Exception)
                {

                    return ErrReturn("添加失败！");
                }
            }
            else
            {
                return Error("无操作权限！");
            }
        }

        [HttpPost]
        public IActionResult DeletePermission(Dictionary<int, PermissionModel> data)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("DeletePermission"))
            {
                if (data.Count > 0)
                {
                    try
                    {
                        var list = new List<Permission>();
                        foreach (var model in data)
                        {
                            var privilege = _permissionService.GetPermissionById(model.Key);
                            if (privilege != null)
                            {
                                list.Add(privilege);
                            }
                        }

                        _permissionService.DeletePermissionRange(list);

                        return Success();
                    }
                    catch (Exception)
                    {
                        return ErrReturn("* 出现错误，删除失败！");
                    }
                }
                return ErrReturn("* 出现错误，删除失败！");
            }
            else
            {
                return Error("无操作权限！");
            }
        }

        [HttpPost]
        public IActionResult UpdatePermission(Dictionary<int, PermissionModel> data)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("UpdatePermission"))
            {
                if (data.Count != 1)
                    return ErrReturn("* 请选择一个权限进项操作");
                var Dicmodel = data.Where(x => true).FirstOrDefault();
                var model = Dicmodel.Value;
                if (string.IsNullOrEmpty(model.Name))
                    return ErrReturn("name", "* 权限名称必填！");
                if (string.IsNullOrEmpty(model.SystemName))
                    return ErrReturn("systemName", "* 控制码必填！");
                if (string.IsNullOrEmpty(model.Category))
                    return ErrReturn("category", "* 请选择一个权限类型");

                if (Dicmodel.Key == 0)
                    return ErrReturn("* 请选择一个对象进行操作！");

                try
                {
                    var privilege = _permissionService.GetPermissionById(Dicmodel.Key);

                    if (_permissionService.GetPermissionByName(model.Name) != null && privilege.Name != model.Name)
                        return ErrReturn("name", "* 已存在相同名称的权限！");
                    if (_permissionService.GetPermissionByCode(model.SystemName) != null && privilege.SystemName != model.SystemName)
                        return ErrReturn("systemName", "* 已存在相同名称的控制码！");

                    privilege.Name = model.Name;
                    privilege.SystemName = model.SystemName;
                    privilege.Category = model.Category;
                    privilege.State = model.State;
                    privilege.Remark = model.Remark;

                    _permissionService.UpdatePermission(privilege);

                    var result = new List<PermissionModel> { _permissionService.GetPermissionById(Dicmodel.Key).ToModel() };
                    return Success(result);
                }
                catch (Exception)
                {
                    return ErrReturn("* 出现错误，删除失败！");
                }
            } else
            {
                return Error("无操作权限！");
            }
           
        }

        private IActionResult ErrReturn(string err)
        {
            return Json(new PermissionResultModel
            {
                Error = err
            });
        }
        private IActionResult ErrReturn(string name, string status)
        {
            return Json(new PermissionResultModel
            {
                FieldErrors = new List<FieldError> {
                    new FieldError
                    {
                        Name = name,
                        Status = status
                    }
                }
            });
        }

        #endregion

        #region 2、角色权限管理

        public IActionResult Role(int id)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("ViewRolePermission"))
            {
                //获取该角色的信息
                var roleInfo = _roleService.GetRoleById(id);
                ViewBag.RoleInfo = roleInfo;

                List<PermissionMultiSelectModel> multiSelectModel = GetPermissionsForRole(id);

                ViewBag.MultiSelectModel = multiSelectModel;
                return View();
            }
            else
            {
                return View("_AccessDeniedView");
            }
        }

        /// <summary>
        /// 获取角色的权限信息，返回值用户角色权限管理的JQuery multi-select插件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [NonAction]
        public List<PermissionMultiSelectModel> GetPermissionsForRole(int id)
        {
            //获取该角色已有的权限
            var rolePermission = _permissionService.GetPermissionsByRoleId(id).ToList();

            //获取所有的权限类型
            var categorys = _menuService.GetBaseMenus().ToList();

            var multiSelectModel = new List<PermissionMultiSelectModel>();
            foreach (var category in categorys)
            {
                //根据权限类型获取权限
                var tempPermissions = _permissionService.GetPermissionsByCategory(category.Name).ToList();

                var options = new List<PermissionOption>();
                if (tempPermissions != null)
                {
                    foreach (var pers in tempPermissions)
                    {
                        var option = new PermissionOption
                        {
                            Id = pers.Id,
                            Name = pers.Name
                        };
                        //判断给角色是否已有该权限
                        if (rolePermission.Contains(pers))
                        {
                            option.Selected = true;
                        }
                        options.Add(option);
                    }
                }

                multiSelectModel.Add(
                    new PermissionMultiSelectModel
                    {
                        Options = options,
                        Optgroup = category.Name
                    });
            }

            return multiSelectModel;
        }

        [HttpPost]
        public IActionResult GetPermissionsInfo(int id, int[] data)
        {
            //获取该角色已有的权限
            var rolePermission = _permissionService.GetPermissionsByRoleId(id).ToList();

            var infos = new List<PermissionOption>();

            for (int i = 0; i < data.Length; i++)
            {
                var pers = _permissionService.GetPermissionById(data[i]);
                var temp = new PermissionOption
                {
                    Name = pers.Name,
                    Category = pers.Category,
                    Id = pers.Id
                };
                if (rolePermission.Contains(pers))
                {
                    //已有权限
                    temp.Selected = true;
                }
                else
                {
                    temp.Selected = false;
                }

                infos.Add(temp);
            }

            return Success(infos);

        }

        /// <summary>
        /// 对角色进行授权
        /// </summary>
        /// <param name="authorizeSelect"></param>
        /// <param name="selected"></param>
        /// <param name="deleteSelect"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AuthorizeRole(List<int> authorizeSelect, string selected, string deleteSelect, int roleId)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("UpdateRolePermission"))
            {
                try
                {
                    var add = new List<RolePermission>();
                    if (!string.IsNullOrEmpty(selected))
                    {
                        var ed = selected.Split(';').Select(x => x).Where(x => !IsNullOrEmpty(x)).ToArray();
                        var authorizedList = Array.ConvertAll(ed, int.Parse);
                        //批量增加权限
                        foreach (var au in authorizeSelect)
                        {
                            if (!authorizedList.Any(p => p == au))
                            {
                                var temp = new RolePermission
                                {
                                    RoleId = roleId,
                                    PermissionId = au,
                                    Isvalid = true
                                };
                                add.Add(temp);
                            }
                        }
                        if (add.Count > 0)
                        {
                            _rolePermissionService.AddRangeRolePermission(add);
                        }
                    }
                    else
                    {
                        foreach (var au in authorizeSelect)
                        {
                            var temp = new RolePermission
                            {
                                RoleId = roleId,
                                PermissionId = au,
                                Isvalid = true
                            };
                            add.Add(temp);
                        }
                        _rolePermissionService.AddRangeRolePermission(add);
                    }

                    if (!string.IsNullOrEmpty(deleteSelect))
                    {
                        //待删除的权限
                        var ing = deleteSelect.Split(';').Select(t => t).Where(t => !IsNullOrEmpty(t)).ToArray();
                        var deleteList = Array.ConvertAll(ing, int.Parse);

                        var del = new List<RolePermission>();
                        foreach (var de in deleteList)
                        {
                            var temp = _rolePermissionService.GetRolePermission(roleId, de);
                            if (temp != null)
                            {
                                del.Add(temp);
                            }
                        }
                        if (del.Count > 0)
                        {
                            _rolePermissionService.DelRangeRolePermission(del);
                        }
                    }

                }
                catch (Exception e)
                {
                    return Error();
                }
                return Success();

            }
            else
            {
                return Error("无操作权限！");
            }
        }

        [HttpPost]
        public IActionResult AuthorizeUser(List<int> authorizeSelect, string selected, string deleteSelect, int userId)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("UpdateRolePermission"))
            {
                try
                {
                    var add = new List<UserPermission>();
                    if (!string.IsNullOrEmpty(selected))
                    {
                        var ed = selected.Split(';').Select(x => x).Where(x => !IsNullOrEmpty(x)).ToArray();
                        var authorizedList = Array.ConvertAll(ed, int.Parse);
                        //批量增加权限
                        foreach (var au in authorizeSelect)
                        {
                            if (!authorizedList.Any(p => p == au))
                            {
                                var temp = new UserPermission
                                {
                                    UserId = userId,
                                    PermissionId = au,
                                    Isvalid = true
                                };
                                add.Add(temp);
                            }
                        }

                    }
                    else
                    {
                        foreach (var au in authorizeSelect)
                        {
                            var temp = new UserPermission
                            {
                                UserId = userId,
                                PermissionId = au,
                                Isvalid = true
                            };
                            add.Add(temp);
                        }
                    }
                    _userPermissionService.AddRangeUserPermission(add);
                }
                catch (Exception e)
                {
                    return Error();
                }

                try
                {
                    if (!string.IsNullOrEmpty(deleteSelect))
                    {
                        //待删除的权限
                        var ing = deleteSelect.Split(';').Select(t => t).Where(t => !IsNullOrEmpty(t)).ToArray();
                        var deleteList = Array.ConvertAll(ing, int.Parse);

                        var del = new List<UserPermission>();
                        foreach (var de in deleteList)
                        {
                            var temp = _userPermissionService.GetPermission(userId, de);
                            if (temp != null)
                            {
                                del.Add(temp);
                            }
                        }
                        if (del.Count > 0)
                        {
                            _userPermissionService.DelRangeUserPermission(del);
                        }
                    }
                }
                catch (Exception e)
                {

                    return Error();
                }

                return Success();
            }
            else
            {
                return Error("无操作权限！");
            }

        }
        #endregion
    }
}