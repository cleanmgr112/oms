using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services.Permissions
{
    public interface IMenuService
    {
        IQueryable<Menu> GetMenusByUserId(int userId);
        Menu GetMenuById(int menuId);
        IQueryable<Menu> GetBaseMenus();
        IQueryable<Menu> GetChildMenus(int parentId);
        void AddMenu(Menu menu);
        void UpdateMenu(Menu menu);
        IQueryable<Menu> GetMenus();
        Menu GetMenuByName(string name);
        Menu GetMenuByCode(string code);
        Menu GetMenuByUrl(string url);
        /// <summary>
        /// 添加角色菜单
        /// </summary>
        /// <param name="roleMenu"></param>
        /// <returns></returns>
        RoleMenu AddRoleMenu(RoleMenu roleMenu);
        /// <summary>
        /// 确认角色是否存在同一菜单
        /// </summary>
        /// <param name="roleMenu"></param>
        /// <returns></returns>
        RoleMenu ConfirmRoleMenuIsExist(RoleMenu roleMenu);
        /// <summary>
        /// 通过roleId获取角色菜单
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        IQueryable<RoleMenu> GetRoleMenusByRoleId(int roleId);
        /// <summary>
        /// 删除角色菜单
        /// </summary>
        /// <param name="roleMenu"></param>
        /// <returns></returns>
        bool DelRoleMenu(RoleMenu roleMenu);
         
    }
}
