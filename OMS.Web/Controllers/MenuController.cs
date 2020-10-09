using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OMS.Core;
using OMS.Data.Domain.Permissions;
using OMS.Model.Menu;
using OMS.Services.Permissions;
using OMS.WebCore;

namespace OMS.Web.Controllers
{
    [UserAuthorize]
    public class MenuController : BaseController
    {
        #region
        private readonly IWorkContext _workContext;
        private readonly IMenuService _menuService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        public MenuController(
            IMenuService menuService,
            IWorkContext workContext,IRoleService roleService,IPermissionService permissionService)
        {
            _roleService = roleService;
            _menuService = menuService;
            _workContext = workContext;
            _permissionService = permissionService;
        }
        #endregion
        [HttpPost]
        public IActionResult GetMenusList()
        {
            var currentUser = _workContext.CurrentUser;
            var menus = _menuService.GetMenusByUserId(currentUser.Id).Where(x => x.State).Distinct().ToList();
            List<MenuModel> baseMenus = ConvertMenus(menus);
            return Success(baseMenus);
        }

        /// <summary>
        /// List<Menu> To List<MenuModel>
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        private List<MenuModel> ConvertMenus(List<Menu> menus)
        {
            var baseMenus = new List<MenuModel>();
            var secondMenus = new List<MenuModel>();
            foreach (var menu in menus)
            {
                if (menu.ParentId == 0 || menu.ParentId == null)
                {
                    baseMenus.Add(menu.ToModel());
                }
                else
                {
                    secondMenus.Add(menu.ToModel());
                }
            }

            foreach (var baseMenu in baseMenus)
            {
                var tempMenus = new List<MenuModel>();
                foreach (var secondMenu in secondMenus)
                {
                    if (baseMenu.Id == secondMenu.ParentId)
                    {
                        tempMenus.Add(secondMenu);
                    }
                }
                baseMenu.ChildMenus = tempMenus;
            }

            return baseMenus;
        }

        public IActionResult Menus()
        {
            //判断权限
            var role = from s in _roleService.GetRolesByUserId(_workContext.CurrentUser.Id) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("ViewMenus"))
            {
                var menus = _menuService.GetBaseMenus().ToList();
                ViewBag.BaseMenus = menus;
                return View();
            }
            else
            {
                return View("_AccessDeniedView");
            }
        }

        [HttpPost]
        public IActionResult GetJsonMenus()
        {
            //获取菜单列表
            var currentUser = _workContext.CurrentUser;

            var allMenu = _menuService.GetMenus().ToList();
            var baseMenus = ConvertMenus(allMenu);
            var menuList = new List<MenuJsonModel>();

            foreach (var menu in baseMenus)
            {
                var childMenus = new List<MenuChild>();
                if (menu.ChildMenus.Count > 0)
                {
                    foreach (var child in menu.ChildMenus)
                    {
                        childMenus.Add(
                            new MenuChild
                            {
                                Id = child.Id,
                                Text = child.Name,
                                Icon = child.State ? "fa fa-folder icon-state-warning icon-lg" : "fa fa-warning icon-state-danger",
                                State = new TreeState
                                {
                                    Opened = true,
                                    Disabled = false,
                                    Selected = false
                                }
                            });
                    }
                }
                menuList.Add(
                    new MenuJsonModel
                    {
                        Id = menu.Id,
                        Text = menu.Name,
                        Icon = menu.State ? "fa fa-folder icon-state-warning icon-lg" : "fa fa-warning icon-state-danger",
                        State = new TreeState
                        {
                            Opened = true,
                            Disabled = false,
                            Selected = false
                        },
                        Children = childMenus
                    });
            }

            return Json(menuList);
        }

        [HttpPost]
        public IActionResult GetMenuInfo(int menuId)
        {
            var menu = _menuService.GetMenuById(menuId);
            return Success(menu);
        }

        /// <summary>
        /// 获取根菜单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetBaseMenus()
        {
            var menus = _menuService.GetBaseMenus().Where(x => x.State).ToList();
            return Success(menus);
        }

        /// <summary>
        /// 新增和修改菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SettingMenu(MenuModel menu)
        {
            var role = from s in _roleService.GetRolesByUserId(_workContext.CurrentUser.Id) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("SettingMenu"))
            {
                if (string.IsNullOrEmpty(menu.Name))
                    return Error("* 请输入一个菜单名！");
                if (string.IsNullOrEmpty(menu.Icon))
                    return Error("* 请选择一个Icon！");

                try
                {
                    if (string.IsNullOrEmpty(menu.Url))
                        menu.Url = "/";

                    if (menu.Id != 0)
                    {
                        //更新
                        var menuModel = _menuService.GetMenuById(menu.Id);

                        menuModel.Name = menu.Name;
                        menuModel.ParentId = menu.ParentId;
                        menuModel.Url = menu.Url;
                        menuModel.Icon = menu.Icon;
                        menuModel.Code = menu.Code;
                        menuModel.State = menu.State;
                        menuModel.Sort = menu.Sort == null ? 0 : menu.Sort;
                        menuModel.ChildUrl = menu.ChildUrl;
                        if (menu.ParentId == null)
                        {
                            menuModel.ModuleName = menu.Name;
                        }
                        else
                        {
                            menuModel.ModuleName = menu.ModuleName;
                        }

                        _menuService.UpdateMenu(menuModel);
                    }
                    else //新增
                    {
                        if (_menuService.GetMenuByName(menu.Name) != null)
                            return Error("* 该菜单名已存在！");
                        if (menu.Url != "/" && _menuService.GetMenuByUrl(menu.Url) != null)
                            return Error("* 该URL已经存在！");
                        if (menu.Code != null && _menuService.GetMenuByCode(menu.Code) != null)
                            return Error("* 该控制码已经存在！");

                        if (menu.ParentId != null)
                        {
                            var childMenus = _menuService.GetChildMenus(menu.ParentId.Value).ToList();
                            if (childMenus != null)
                            {
                                int tempSort = 0;
                                foreach (var m in childMenus)
                                {
                                    tempSort = m.Sort.Value > tempSort ? tempSort = m.Sort.Value : tempSort;
                                }
                                menu.Sort = tempSort + 5;
                            }
                            else
                            {
                                menu.Sort = 0;
                            }
                        }

                        var menuModel = new Menu
                        {
                            Name = menu.Name,
                            ParentId = menu.ParentId,
                            Url = menu.Url,
                            Icon = menu.Icon,
                            Code = menu.Code,
                            State = menu.State,
                            Sort = menu.Sort,
                            ChildUrl = menu.ChildUrl,
                            Isvalid = true
                        };
                        if (menu.ParentId == -1)
                        {
                            menuModel.Level = "1";
                            menuModel.ModuleName = menu.Name;
                        }
                        else
                        {
                            menuModel.Level = "2";
                            menuModel.ParentId = menu.ParentId;
                            menuModel.ModuleName = menu.ModuleName;
                        }
                        _menuService.AddMenu(menuModel);
                    }
                }
                catch (Exception e)
                {

                    return Error("更新错误！");
                }

                return Success();
            }
            else
            {
                return Error("无操作权限！");
            }
            
        }

        //public Menu FormatMenu(MenuModel menu)
        //{
        //    var menuModel = new Menu {
        //        Name = menu.Name;
        //    };


        //    return menuModel;
        //}
        public IActionResult RolesMenu()
        {
            var role = from s in _roleService.GetRolesByUserId(_workContext.CurrentUser.Id) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("ViewRolesMenu"))
            {
                ViewBag.Roles = _roleService.GetAllRoles();
                return View();
            }
            else
            {
                return View("_AccessDeniedView");
            }
        }
        [HttpPost]
        public JsonResult AddRoleMenu(int roleId,string menuId)
        {
            var role = from s in _roleService.GetRolesByUserId(_workContext.CurrentUser.Id) select s.Name;
            if (role.Contains("超级管理员") || _permissionService.Authorize("AddRoleMenu"))
            {
                if (menuId == null)
                {
                    return Error();
                }
                var menusId = menuId.Split(",");
                menusId = menusId.Where(s => !string.IsNullOrEmpty(s)).ToArray();
                //删除现在不包含的菜单
                var data = _menuService.GetRoleMenusByRoleId(roleId);
                List<string> oldRoleId = new List<string>();
                foreach (var item in data)
                {
                    oldRoleId.Add(item.MenuId.Value.ToString());
                }
                var delRoleId = from r in oldRoleId where menusId.Contains(r) == false select r;
                foreach (var i in delRoleId)
                {
                    RoleMenu roleMenu = new RoleMenu();
                    roleMenu.RoleId = roleId;
                    roleMenu.MenuId = int.Parse(i);
                    _menuService.DelRoleMenu(roleMenu);
                }
                //添加新的菜单
                foreach (var i in menusId)
                {

                    RoleMenu roleMenu = new RoleMenu();
                    roleMenu.MenuId = int.Parse(i);
                    roleMenu.RoleId = roleId;
                    if (_menuService.ConfirmRoleMenuIsExist(roleMenu) == null)
                    {
                        _menuService.AddRoleMenu(roleMenu);
                    }
                }
                return Success();
            }
            else
            {
                return Error("无操作权限！");
            }

        }
        /// <summary>
        /// 获取角色菜单
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public IActionResult GetRoleMenus(int roleId)
        {
            var data = _menuService.GetRoleMenusByRoleId(roleId);
            List<string> roleMenus=new List<string>();
            foreach(var item in data)
            {
                roleMenus.Add(item.MenuId.ToString());
            }
            return Success(roleMenus);
        }
    }
}