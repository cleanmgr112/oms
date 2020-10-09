using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMS.Core;
using OMS.Data.Domain.Permissions;
using OMS.Model.Grid;
using OMS.Model.Role;
using OMS.Services.Permissions;
using OMS.WebCore;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class RoleController : BaseController
    {
        #region
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        public RoleController(
            IRoleService roleService,
            IWorkContext workContext,
            IUserRoleService userRoleService,
            IPermissionService permissionService)
        {
            _roleService = roleService;
            _userRoleService = userRoleService;
            _workContext = workContext;
            _permissionService = permissionService;
        }
        #endregion
        public IActionResult Roles()
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("ViewRoles"))
            {
                return View();
            }
            else
            {
                return View("_AccessDeniedView");
            }
        }

        [HttpPost]
        public IActionResult GetRoles()
        {
            var roles = _roleService.GetAllRoles().ToList();
            var data = roles.Select(x => { return x.ToModel(); }).ToList();
            return Success(data);
        }

        #region 1、角色编辑
        [HttpPost]
        public IActionResult UpdateRole(Dictionary<int, Role> data)
        {
            var roleDic = data.Where(x => true).FirstOrDefault();
            var roleKey = roleDic.Key;
            var role = roleDic.Value;

            if (data.Count != 1)
                return ErrReturn("* 请选择一个对象进行操作");

            try
            {
                if (roleKey == 0)
                    return ErrReturn("* 请选择一个对象进行操作！");
                var roleData = _roleService.GetRoleById(roleKey);
                if (roleData != null)
                {
                    if (_roleService.GetRoleByName(role.Name) != null && role.Name != roleData.Name)
                        return ErrReturn("name", "* 已经存在相同名称的其他角色！");

                    if (_roleService.GetRoleByCode(role.Code) != null && role.Code != roleData.Code)
                        return ErrReturn("code", "* 已经存在相同控制码的其他角色！");


                    roleData.Name = role.Name;
                    roleData.Code = role.Code;
                    roleData.State = role.State;
                    roleData.Remark = role.Remark;

                    _roleService.UpdateRole(roleData);

                    var result = new List<RoleModel> { _roleService.GetRoleById(roleKey).ToModel() };

                    return Success(result);
                }
                else
                {
                    return ErrReturn("更新失败！");
                }
            }
            catch (Exception e)
            {

                return ErrReturn("更新失败！");
            }



        }

        [HttpPost]
        public IActionResult AddRole(Dictionary<int, Role> data)
        {

            var role = data.Where(x => true).FirstOrDefault().Value;

            if (_roleService.GetRoleByName(role.Name) != null)
                return ErrReturn("Name", "* 已经存在相同名称的角色！");

            if (_roleService.GetRoleByCode(role.Code) != null)
                return ErrReturn("Code", "* 已经存在相同控制码的角色！");

            try
            {
                var roleData = new Role
                {
                    Name = role.Name,
                    Code = role.Code,
                    State = role.State,
                    Remark = role.Remark,
                    Sort = 1,
                    Isvalid = true
                };

                _roleService.AddRole(roleData);

                var result = new List<RoleModel> { _roleService.GetRoleByCode(role.Code).ToModel() };
                return Success(result);
            }
            catch (Exception e)
            {
                return ErrReturn("* 操作失败！");
            }
        }

        [HttpPost]
        public IActionResult DeleteRoles(Dictionary<int, Role> data)
        {
            if (data.Count == 0)
                return ErrReturn("* 请选择一个对象进行操作");
            try
            {
                var roles = new List<Role>();
                foreach (var item in data)
                {
                    var temp = _roleService.GetRoleById(item.Key);
                    if (temp != null)
                    {
                        roles.Add(temp);
                    }
                    else
                    {
                        return ErrReturn("* 操作失败！");
                    }
                }

                _roleService.DeleteRangeRole(roles);

                return Success();
            }
            catch (Exception e)
            {
                return ErrReturn("* 操作失败！");
            }

        }

        private IActionResult ErrReturn(string err)
        {
            return Json(new RoleResultModel
            {
                Error = err
            });
        }
        private IActionResult ErrReturn(string name, string status)
        {
            return Json(new RoleResultModel
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

        #region 2、角色权限编辑

        public IActionResult Permission(int id)
        {
            return View();
        }

        #endregion

        #region 3、用户角色编辑
        /// <summary>
        /// 用户角色的添加与修改
        /// </summary>
        /// <param name="roleSelect">被选中角色Id</param>
        /// <param name="selected">该用户已有的角色</param>
        /// <param name="deleteSelect">要删除的角色</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SettingUserRoles(List<int> roleSelect, string selected, string deleteSelect, int userId)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("SettingUserRoles"))
            {
                try
                {
                    //批量增加角色
                    var userRoleList = new List<UserRole>();

                    if (!string.IsNullOrEmpty(selected))
                    {
                        var ed = selected.Split(';').Select(x => x).Where(x => !IsNullOrEmpty(x)).ToArray();
                        var roleIdList = Array.ConvertAll(ed, int.Parse);

                        foreach (var select in roleSelect)
                        {
                            //如果用户还没有该角色
                            if (!roleIdList.Any(p => p == select))
                            {
                                var temp = new UserRole
                                {
                                    UserId = userId,
                                    RoleId = select,
                                    Isvalid = true,
                                    CreatedBy = WorkContext.CurrentUser.Id,
                                    CreatedTime = DateTime.Now
                                };
                                userRoleList.Add(temp);
                            }
                        }
                    }
                    else
                    {
                        foreach (var select in roleSelect)
                        {
                            var temp = new UserRole
                            {
                                RoleId = select,
                                UserId = userId,
                                Isvalid = true,
                                CreatedBy = WorkContext.CurrentUser.Id,
                                CreatedTime = DateTime.Now
                            };
                            userRoleList.Add(temp);
                        }
                    }
                    _userRoleService.AddUserRoleRange(userRoleList);
                }
                catch (Exception e)
                {

                    return Error("* 添加角色失败！");
                }

                try
                {
                    //如果有待删除的角色
                    if (!string.IsNullOrEmpty(deleteSelect))
                    {
                        //待删除的权限
                        var ing = deleteSelect.Split(';').Select(t => t).Where(t => !IsNullOrEmpty(t)).ToArray();
                        var deleteList = Array.ConvertAll(ing, int.Parse);

                        var delUserRoles = new List<UserRole>();
                        foreach (var select in deleteList)
                        {
                            var temp = _userRoleService.GetUserRole(userId, select);
                            if (temp != null)
                            {
                                delUserRoles.Add(temp);
                            }
                        }
                        _userRoleService.DeleteUserRoleRange(delUserRoles);
                    }

                }
                catch (Exception)
                {
                    return Error("* 删除角色失败！");
                }

                return Success();
            }
            else
            {
                return Error("无操作权限！");
            }

        }

        /// <summary>
        /// 获取角色信息，并判断用户是否具有该角色，用于用户角色管理/User/Role
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <param name="data">roleId列表</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetRoleInfo(int id, int[] data)
        {
            if (data.Length > 0)
            {
                var roles = new List<RoleMultiSelectModel>();
                var userRole = _roleService.GetRolesByUserId(id).ToList();
                for (int i = 0; i < data.Length; i++)
                {
                    var temp = _roleService.GetRoleById(data[i]);

                    roles.Add(new RoleMultiSelectModel
                    {
                        Id = temp.Id,
                        Code = temp.Code,
                        Name = temp.Name,
                        Selected = userRole.Contains(temp) ? true : false
                    });
                }
                return Success(roles);
            }

            return Error("* 出现错误");
        }

        /// <summary>
        /// 获取用户所有的角色
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetUserRolesCount(int userId)
        {
            var roles = _roleService.GetRolesByUserId(userId).ToList();
            return Success(roles.Count);
        }

        #endregion

    }
}