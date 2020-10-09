using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Domain.Permissions;
using OMS.Data.Interface;
using OMS.Model.Grid;
using OMS.Services.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services.Permissions
{
    public class PermissionService : ServiceBase, IPermissionService
    {
        #region ctor
        private IUserService _userService;
        public PermissionService(IDbAccessor omsAccessor, IWorkContext workContext, IUserService userService) : base(omsAccessor, workContext)
        {
            _userService = userService;
        }

        #endregion
        public void AddPermission(Permission permission)
        {
            permission.CreatedBy = _workContext.CurrentUser.Id;
            permission.CreatedTime = DateTime.Now;
            _omsAccessor.Insert<Permission>(permission);
            _omsAccessor.SaveChanges();
        }

        public void SoftDeletePermission(Permission permission)
        {
            //软删除
            permission.Isvalid = false;
            permission.ModifiedBy = _workContext.CurrentUser.Id;
            permission.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<Permission>(permission);
            _omsAccessor.SaveChanges();

        }
        public void DeletePermission(Permission permission)
        {
            _omsAccessor.Delete<Permission>(permission);
            _omsAccessor.SaveChanges();
        }
        public void UpdatePermission(Permission permission)
        {
            permission.ModifiedBy = _workContext.CurrentUser.Id;
            permission.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<Permission>(permission);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 根据用户ID获取该用户所有的权限
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IQueryable<Permission> GetPermissionsByUserId(int userId)
        {
            var permissions = from p in _omsAccessor.Get<Permission>()
                              join up in _omsAccessor.Get<UserPermission>() on p.Id equals up.PermissionId
                              join u in _omsAccessor.Get<User>() on up.UserId equals u.Id
                              where u.Id == userId && p.Isvalid
                              select p;
            return permissions;
        }

        public IPageList<Permission> GetPermissionListByPage(int pageIndex, int PageSize)
        {
            var query = _omsAccessor.Get<Permission>().Where(x => x.Isvalid).OrderByDescending(t => t.Id);
            return new PageList<Permission>(query, pageIndex, PageSize);
        }

        public IQueryable<Permission> GetAllPermissions()
        {
            return _omsAccessor.Get<Permission>().Where(x => x.Isvalid);
        }

        public Permission GetPermissionByName(string name)
        {
            return _omsAccessor.Get<Permission>().Where(x => x.Isvalid && x.Name == name).FirstOrDefault();
        }

        public Permission GetPermissionById(int id)
        {
            return _omsAccessor.Get<Permission>().Where(x => x.Isvalid && x.Id == id).FirstOrDefault();
        }

        public Permission GetPermissionByCode(string code)
        {
            return _omsAccessor.Get<Permission>().Where(x => x.Isvalid && x.SystemName == code).FirstOrDefault();
        }

        public void DeletePermissionRange(List<Permission> list)
        {
            _omsAccessor.DeleteRange<Permission>(list);
            _omsAccessor.SaveChanges();
        }

        public IPageList<Permission> SearchPermissionListByPage(SearchModel searchModel)
        {
            var query = _omsAccessor.Get<Permission>().Where(x => x.Isvalid);
            if (searchModel.SearchValue != null)
            {
                query = query.Where(x => x.Name.Contains(searchModel.SearchValue)).OrderBy(t => t.Id);
            }
            else
            {
                query = query.OrderBy(t => t.Id);
            }

            return new PageList<Permission>(query, searchModel.PageIndex, searchModel.Length);
        }

        public IQueryable<Permission> GetPermissionsByCategory(string category)
        {
            return _omsAccessor.Get<Permission>().Where(x => x.Isvalid && x.Category == category);
        }

        public IQueryable<Permission> GetPermissionsByRoleId(int id)
        {
            var query = from p in _omsAccessor.Get<Permission>()
                        join rp in _omsAccessor.Get<RolePermission>() on p.Id equals rp.PermissionId
                        join r in _omsAccessor.Get<Role>() on rp.RoleId equals r.Id
                        where p.Isvalid && r.Id == id
                        select p;
            return query;
        }


        /// <summary>
        /// 用户权限判断（Permission）
        /// </summary>
        /// <param name="permission">Permission record</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(Permission permission)
        {
            return Authorize(permission, _workContext.CurrentUser);
        }

        /// <summary>
        /// 用户权限判断（Permission,User）
        /// </summary>
        /// <param name="permission">Permission record</param>
        /// <param name="user">Account</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(Permission permission, User user)
        {
            if (permission == null)
                return false;

            if (user == null)
                return false;
            #region
            //old implementation of Authorize method
            //var accountRoles = account.AccountRoles.Where(cr => cr.Active);
            //foreach (var role in accountRoles)
            //    foreach (var permission1 in role.PermissionRecords)
            //        if (permission1.SystemName.Equals(permission.SystemName, StringComparison.InvariantCultureIgnoreCase))
            //            return true;

            //return false;
            #endregion

            return Authorize(permission.SystemName, user);
        }

        /// <summary>
        /// 用户权限判断（string permissionRecordSystemName）
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(string permissionRecordSystemName)
        {
            return Authorize(permissionRecordSystemName, _workContext.CurrentUser);
        }

        /// <summary>
        /// 用户权限判断（string permissionRecordSystemName,User）
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name</param>
        /// <param name="account">Account</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public virtual bool Authorize(string permissionRecordSystemName, User account)
        {
            if (String.IsNullOrEmpty(permissionRecordSystemName))
                return false;
            if (_userService.AccountPermissionRecords(_workContext.CurrentUser).Any(p => p.SystemName == permissionRecordSystemName))
            {
                return true;
            }

            return false;
        }
    }
}
