using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Domain.Permissions;
using OMS.Model.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services.Permissions
{
    public interface IPermissionService
    {

        IQueryable<Permission> GetPermissionsByUserId(int userId);
        void UpdatePermission(Permission permission);
        void AddPermission(Permission permission);
        void DeletePermission(Permission permission);
        void SoftDeletePermission(Permission permission);
        void DeletePermissionRange(List<Permission> list);
        IPageList<Permission> GetPermissionListByPage(int pageIndex,int PageSize);
        IPageList<Permission> SearchPermissionListByPage(SearchModel searchModel);
        IQueryable<Permission> GetAllPermissions();
        Permission GetPermissionByName(string name);
        Permission GetPermissionById(int id);
        Permission GetPermissionByCode(string code);
        /// <summary>
        /// 根据类型获取权限
        /// </summary>
        /// <returns></returns>
        IQueryable<Permission> GetPermissionsByCategory(string category);
        IQueryable<Permission> GetPermissionsByRoleId(int id);


        /// <summary>
        /// 用户权限判断（Permission）
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        bool Authorize(Permission permission);

        /// <summary>
        /// 用户权限判断（Permission,User）
        /// </summary>
        /// <param name="permission">Permission record</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize(Permission permission, User user);

        /// <summary>
        /// 用户权限判断（string permissionRecordSystemName）
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize(string permissionRecordSystemName);

        /// <summary>
        /// 用户权限判断（string permissionRecordSystemName,User）
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize(string permissionRecordSystemName, User user);

    }
}
