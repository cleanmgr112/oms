using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMS.Core.Tools;
using OMS.Services.Authentication;
using OMS.Services.Account;
using OMS.Model.Grid;
using OMS.WebCore;
using OMS.Model.Account;
using System.Text.RegularExpressions;
using OMS.Data.Domain;
using OMS.Services.Permissions;
using OMS.Model.Role;
using OMS.Model.Permission;
using OMS.Data.Domain.Permissions;
using OMS.Core;
using OMS.Data.Interface;
using OMS.Services.Log;
using OMS.Services.StockRemid;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class UserController : BaseController
    {
        #region ctor
        private readonly IMenuService _menuService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly IPermissionService _permissionService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWorkContext _workContext;
        private readonly IDbAccessor _omsAccessor;
        private readonly ILogService _logService;
        private readonly StockRemindService stockRemind;

        public UserController(
            IMenuService menuService,
            IUserService userService,
            IRoleService roleService,
            IUserRoleService userRoleService,
            IPermissionService permissionService,
            IAuthenticationService authenticationService,
            IWorkContext workContext,
            IDbAccessor omsAccessor,
            ILogService logService,
            StockRemindService stockRemind
            )
        {
            this._menuService = menuService;
            this._userService = userService;
            this._roleService = roleService;
            this._permissionService = permissionService;
            this._userRoleService = userRoleService;
            this._authenticationService = authenticationService;
            this._workContext = workContext;
            this._omsAccessor = omsAccessor;
            this._logService = logService;
            this.stockRemind = stockRemind;
        }

        #endregion

        #region 登录与登出

        [UserAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [UserAnonymous]
        [HttpPost]
        public IActionResult Login(string username, string password, string verify, string returnUrl)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Error("用户名为空");
            }
            if (string.IsNullOrEmpty(password))
            {
                return Error("密码为空");
            }
            if (string.IsNullOrEmpty(verify))
            {
                return Error("验证码为空");
            }
            var code = HttpContext.Session.GetString("VerifyCode");
            if (code == null)
            {
                return Error();
            }
            if (verify.ToLower() != code.ToLower())
            {
                return Error("验证码错误");
            }
            var account = _userService.GetByUserName(username);
            if (account == null)
            {
                return Error("账号或密码错误");
            }
            if (!account.Isvalid)
                return Error("账号已被删除");
            if (account.State == UserState.Disabled)
                return Error("账号已被禁用");
            if (EncryptTools.AESEncrypt(password, account.Salt) == account.UserPwd)
            {
                account.LastLoginTime = DateTime.Now;
                account.LastLoginIp = HttpContext.Connection.RemoteIpAddress.ToString();
                _userService.Update(account);

                _authenticationService.SignIn(username);

                return Success(new { returnUrl = RedirectToLocal(returnUrl) });
            }
            else
            {
                return Error("账号或密码错误");
            }
        }


        [HttpPost]
        public IActionResult Logout()
        {
            _authenticationService.SignOut();
            return Success();
        }

        [UserAnonymous]
        public IActionResult VerifyCode()
        {
            var code = CommonTools.CreateRandomStr(4);
            HttpContext.Session.SetString("VerifyCode", code);
            byte[] graphic = CommonTools.CreateValidateGraphic(code);
            return File(graphic, @"image/jpeg");
        }

        private string RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }
            else
            {
                return "/";
            }
        }
        #endregion

        #region 用户管理

        public IActionResult Index()
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("ViewUsers"))
            {
                var user = _userService.GetById(1);
                return View(user);
            }
            else
            {
                return View("_AccessDeniedView");
            }

        }

        /// <summary>
        /// 分页获取用户列表
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetUsersByPage(SearchModel searchModel)
        {
            if (searchModel != null)
            {
                if (searchModel.Start == 0)
                    searchModel.PageIndex = 1;
                if (searchModel.Start > 0)
                    searchModel.PageIndex = searchModel.Start / 10 + 1;
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
                var models = new List<UserViewModel>();
                var users = _userService.GetUsersByPage(searchModel.SearchValue, searchModel.PageIndex, searchModel.Length);
                foreach (var user in users)
                {
                    var tempUser = user.ToViewModel();
                    tempUser.UserPwd = "";
                    models.Add(tempUser);
                }

                var result = new SearchResultModel
                {
                    Data = models,
                    Draw = searchModel.Draw,
                    RecordsTotal = users.TotalCount,
                    RecordsFiltered = users.TotalCount
                };
                return Json(result);
            }
            return Error("* 出现错误！");
        }

        [HttpPost]
        public IActionResult AddUser(Dictionary<int, UserViewModel> data)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("AddUser"))
            {
                var ob = "";
                var errStr = "";
                var model = data.Where(x => true).FirstOrDefault().Value;
                var isValidate = ValidateUser("create", data, out ob, out errStr);
                if (isValidate)
                {
                    try
                    {
                        var salt = CommonTools.CreateRandomStr(16);
                        var user = new User
                        {
                            Name = model.Name,
                            Salt = salt,
                            State = (UserState)model.State,
                            Email = model.Email,
                            Isvalid = true,
                            UserPwd = EncryptTools.AESEncrypt(model.UserPwd, salt),
                            UserName = model.UserName,
                            PhoneNumber = model.PhoneNumber,
                            CreatedBy = WorkContext.CurrentUser.Id,
                            CreatedTime = DateTime.Now
                        };
                        var resUser = _userService.CreateUser(user);
                        resUser.UserPwd = "";
                        var result = new List<UserViewModel> { resUser.ToViewModel() };
                        return Success(result);
                    }
                    catch (Exception e)
                    {
                        return ErrReturn("* 出现错误！");
                    }
                }
                else
                {
                    return ErrReturn(ob, errStr);
                }
            }
            else
            {
                return Error("无操作权限！");
            }
        }
        public IActionResult UpdateUser(int userId)
        {
            var result = _userService.GetById(userId);
            return View(result);
        }
        [HttpPost]
        public IActionResult UpdateCurrentUser(UserModel userModel)
        {
            try
            {
                var user = _userService.GetById(userModel.Id);
                var salt = CommonTools.CreateRandomStr(16);
                user.Salt = salt;
                user.UserPwd = EncryptTools.AESEncrypt(userModel.UserPwd.ToString().Trim(), salt);
                _userService.UpdateUser(user);
                return Success();
            }catch(Exception e)
            {
                return Error("修改失败！");
            }

        }
        [HttpPost]
        public IActionResult UpdateUser(Dictionary<int, UserViewModel> data)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("UpdateUser"))
            {
                var ob = "";
                var errStr = "";
                var dic = data.Where(x => true).FirstOrDefault();
                var model = dic.Value;
                var isValidate = ValidateUser("update", data, out ob, out errStr);
                if (isValidate)
                {
                    try
                    {
                        var user = _userService.GetById(dic.Key);
                        user.Name = model.Name;
                        user.UserName = model.UserName;
                        user.Email = model.Email;
                        user.PhoneNumber = model.PhoneNumber;
                        user.ModifiedBy = WorkContext.CurrentUser.Id;
                        user.ModifiedTime = DateTime.Now;
                        user.State = (UserState)model.State;
                        if (model.UserPwd != null)
                        {
                            user.Salt = CommonTools.CreateRandomStr(16);
                            user.UserPwd = EncryptTools.AESEncrypt(model.UserPwd, user.Salt);
                        }
                        var resUser = _userService.UpdateUser(user);
                        resUser.UserPwd = "";
                        var result = new List<UserViewModel> { resUser.ToViewModel() };
                        return Success(result);
                    }
                    catch (Exception e)
                    {
                        return ErrReturn("出现错误，更新失败！");
                    }
                }

                return ErrReturn(ob, errStr);
            }
            else
            {
                return Error("无操作权限！");
            }

        }

        [HttpPost]
        public IActionResult DeleteUser(Dictionary<int, UserViewModel> data)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("DeleteUser"))
            {
                if (data.Count > 0)
                {
                    try
                    {
                        var list = new List<User>();
                        foreach (var model in data)
                        {
                            var user = _userService.GetById(model.Key);
                            if (user != null)
                            {
                                user.ModifiedBy = WorkContext.CurrentUser.Id;
                                user.ModifiedTime = DateTime.Now;
                                list.Add(user);
                            }
                        }

                        _userService.SoftDeleteUserRange(list);

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

        /// <summary>
        /// 验证表单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool ValidateUser(string action, Dictionary<int, UserViewModel> data, out string ob, out string res)
        {
            ob = "";
            res = "success";
            var resUserName = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
            var dic = data.Where(x => true).FirstOrDefault();
            var model = dic.Value;
            if (string.IsNullOrEmpty(model.Name))
            {
                ob = "name";
                res = "* 请输入名称";
                return false;
            }
            if (model.Name.Length > 50)
            {
                ob = "name";
                res = "* 请输保持在50个字符以下";
                return false;
            }
            if (string.IsNullOrEmpty(model.UserName))
            {
                ob = "userName";
                res = "* 请输入登录名（英文）";
                return false;
            }
            if (!resUserName.IsMatch(model.Name))
            {
                ob = "userName";
                res = "* 请输入一个英文登录名";
            }
            if (model.UserName.Length > 32)
            {
                ob = "userName";
                res = "* 请保持在32个字符以下";
                return false;
            }
            var name = _userService.GetUserByName(model.Name);
            var userName = _userService.GetByUserName(model.UserName);
            if (action == "create")
            {
                if (name != null)
                {
                    ob = "name";
                    res = string.Format("* 已存在名称为{0}的用户", model.Name);
                    return false;
                }
                if (userName != null)
                {
                    ob = "userName";
                    res = string.Format("* 已存在登录名为{0}的用户", model.UserName);
                    return false;
                }
                if (string.IsNullOrEmpty(model.UserPwd))
                {
                    ob = "userPwd";
                    res = "* 请输入密码";
                    return false;
                }
                if (model.UserPwd.Length > 16 || model.UserPwd.Length < 6)
                {
                    ob = "userPwd";
                    res = "* 密码长度保持在6个字符以上16位以下";
                    return false;
                }
            }
            if (action == "update")
            {
                var user = _userService.GetById(dic.Key);
                if (name != null && user.Name != model.Name)
                {
                    ob = "name";
                    res = string.Format("* 已存在名称为{0}的用户", model.Name);
                    return false;
                }
                if (userName != null && user.UserName != model.UserName)
                {
                    ob = "userName";
                    res = string.Format("* 已存在登录名为{0}的用户", model.UserName);
                    return false;
                }
                if (model.UserPwd != null && (model.UserPwd.Length > 16 || model.UserPwd.Length < 6))
                {
                    ob = "userPwd";
                    res = "* 密码长度保持在6个字符以上16位以下";
                    return false;
                }
            }

            var emailReg = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            if (!string.IsNullOrEmpty(model.Email))
            {
                if (!emailReg.IsMatch(model.Email))
                {
                    ob = "email";
                    res = "* 请输入一个正确的邮箱";
                    return false;
                }
            }
            var phoneReg = new Regex("^1[3|4|5|7|8][0-9]\\d{4,8}$");
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                if (!phoneReg.IsMatch(model.PhoneNumber))
                {
                    ob = "phoneNumber";
                    res = "* 请输入一个正确的手机号码";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult UpdateUserPassword(int id)
        {
            ViewBag.UserId = id;
            return View();
        }
        [HttpPost]
        public IActionResult UpdateUserPassword(string oldPassword, string newPassword, int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
                return Error("数据错误，未找到该用户");

            var password = EncryptTools.AESDecrypt(user.UserPwd, user.Salt);
            if (oldPassword != password)
            {
                return Error("旧密码输入错误");
            }
            var salt = CommonTools.CreateRandomStr(16);
            user.Salt = salt;
            user.UserPwd = EncryptTools.AESEncrypt(newPassword, salt);
            user.ModifiedBy = _workContext.CurrentUser.Id;
            user.ModifiedTime = DateTime.Now;
            _userService.UpdateUser(user);
            return Success();
        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetPassword(int userId)
        {
            var user = _userService.GetById(userId);
            if (user == null) return Error("数据错误，未找到该用户");
            var passWord = user.UserName.Trim() + "2020801";
            var salt = CommonTools.CreateRandomStr(16);
            user.Salt = salt;
            user.UserPwd = EncryptTools.AESEncrypt(passWord, salt);
            user.ModifiedBy = _workContext.CurrentUser.Id;
            user.ModifiedTime = DateTime.Now;
            _userService.UpdateUser(user);
            return Success("重置密码成功,重置密码为"+passWord);
        }
        /// <summary>
        /// 重置所有用户密码
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetAllPassword()
        {
            try
            {
                var users = _userService.GetAllUserList().ToList();
                foreach (var user in users)
                {
                    var passWord = user.UserName.Trim() + "2020801";
                    var salt = CommonTools.CreateRandomStr(16);
                    user.Salt = salt;
                    user.UserPwd = EncryptTools.AESEncrypt(passWord, salt);
                    user.ModifiedBy = _workContext.CurrentUser.Id;
                    user.ModifiedTime = DateTime.Now;
                    _omsAccessor.Update(user);
                }
                _omsAccessor.SaveChanges();
                return Success("重置所有用户密码成功！");
            }
            catch (Exception ex)
            {

                _logService.Error("重置所有用户密码错误:"+ex.Message);
                return Error("重置所有用户密码失败！");
            }

        }


        private IActionResult ErrReturn(string err)
        {
            return Json(new UsersResultModel
            {
                Error = err
            });
        }
        private IActionResult ErrReturn(string name, string status)
        {
            return Json(new UsersResultModel
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

        #region 用户权限编辑

        public IActionResult Permission(int id)
        {
            //获取用户信息
            var user = _userService.GetById(id);
            user.UserPwd = "";
            ViewBag.UserInfo = user.ToViewModel();

            //获取用户角色信息
            var roles = _roleService.GetRolesByUserId(id).ToList();
            ViewBag.Roles = roles.Select(x => { return x.ToModel(); }).ToList();

            //获取用户所处角色的权限条目
            var permissions = GetPermissions(user.Id, roles);

            ViewBag.Permissions = permissions;
            return View();
        }
        /// <summary>
        /// 获取用户包含的角色的权限信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roles"></param>
        /// <returns>返回值用户角色权限管理的JQuery multi-select插件</returns>
        [NonAction]
        public List<PermissionMultiSelectModel> GetPermissions(int userId, List<Role> roles)
        {

            //获取用户已有的角色
            var userPermissions = _permissionService.GetPermissionsByUserId(userId).ToList();

            //获取该该用户所有角色的的权限
            var allPermissions = new List<Permission>();

            foreach (var role in roles)
            {
                allPermissions.AddRange(_permissionService.GetPermissionsByRoleId(role.Id).ToList());
            }
            //去除重复元素
            allPermissions = allPermissions.Distinct().ToList();

            //获取所有的权限类型
            var categorys = _menuService.GetBaseMenus().ToList();

            var multiSelectModel = new List<PermissionMultiSelectModel>();
            foreach (var category in categorys)
            {
                //根据权限类型获取权限
                // var tempPermissions = _permissionService.GetPermissionsByCategory(category.Name).ToList();

                var tempPermissions = allPermissions.Where(x => x.Category == category.Name).ToList();

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
                        if (userPermissions.Contains(pers))
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

        /// <summary>
        /// 根据用户ID和权限Id列表返回权限信息，用于判断用户是否拥有该权限
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetPermissionsInfo(int id, int[] data)
        {
            //获取该角色已有的权限
            var userPermission = _permissionService.GetPermissionsByUserId(id).ToList();

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
                if (userPermission.Contains(pers))
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
        #endregion

        #region 用户角色编辑
        public IActionResult Role(int id)
        {
            var role = from s in (_roleService.GetRolesByUserId(_workContext.CurrentUser.Id).ToList()) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("ViewUsers")) {
                //获取用户信息
                var user = _userService.GetById(id);
                if (user == null)
                    return Error();

                user.UserPwd = "";
                ViewBag.UserInfo = user.ToViewModel();
                //获取用户已有的角色
                var userRoles = _roleService.GetRolesByUserId(id).ToList();

                //获取所有角色
                var allRoles = _roleService.GetAllRoles().ToList();

                var multiSelectModel = new List<RoleMultiSelectModel>();
                foreach (var r in allRoles)
                {
                    var option = new RoleMultiSelectModel
                    {
                        Id = r.Id,
                        Code = r.Code,
                        Name = r.Name
                    };
                    if (userRoles.Contains(r))
                    {
                        //已有的权限
                        option.Selected = true;
                    }
                    multiSelectModel.Add(option);
                }
                ViewBag.Roles = multiSelectModel;
                return View();
            } else
            {
                return Error("无操作权限！");
            }
     
        }

        /// <summary>
        /// 获取所有角色和用户已有的角色
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetUserRoles(int id)
        {

            //获取用户已有的角色
            var userRoles = _roleService.GetRolesByUserId(id).ToList();

            //获取所有角色
            var allRoles = _roleService.GetAllRoles().ToList();

            var multiSelectModel = new List<RoleMultiSelectModel>();
            foreach (var role in allRoles)
            {
                var option = new RoleMultiSelectModel
                {
                    Code = role.Code,
                    Name = role.Name
                };
                if (userRoles.Contains(role))
                {
                    //已有的权限
                    option.Selected = true;
                }
                multiSelectModel.Add(option);
            }

            return Success(multiSelectModel);

        }

        #endregion
    }
}