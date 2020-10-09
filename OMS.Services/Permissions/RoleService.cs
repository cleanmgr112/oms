using OMS.Core;
using OMS.Data.Domain.Permissions;
using OMS.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services.Permissions
{
    public class RoleService :ServiceBase, IRoleService
    {
        #region ctor
        public RoleService(IDbAccessor omsAccessor, IWorkContext workContext) : base(omsAccessor, workContext)
        {
        }

        #endregion
        /// <summary>
        /// 根据userId获取权限列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IQueryable<Role> GetRolesByUserId(int userId)
        {
            //var roles = from r in _omsAccessor.Get<Role>()
            //            join ur in _omsAccessor.Get<UserRole>() on r.Id equals ur.RoleId
            //            join u in _omsAccessor.Get<UserRole>() on ur.UserId equals u.Id
            //            where r.Isvalid && u.Id == userId
            //            orderby r.Sort
            //            select r;
            //var roles = from  r in _omsAccessor.Get<Role>() join ur in _omsAccessor.Get<UserRole>() on r.Id equals ur.RoleId select r;
            var roles = from r in _omsAccessor.Get<Role>() join ur in _omsAccessor.Get<UserRole>() on r.Id equals ur.RoleId  where ur.UserId==userId select r;
            return roles;
        }

        public IPageList<Role> SearchRolesByPage(int pageIndex, int pageSize)
        {
            var allRoles = _omsAccessor.Get<Role>().Where(x=>x.Isvalid).OrderByDescending(t=>t.Sort);
            var totalCount = allRoles.Count();
            return new PageList<Role>(allRoles,pageIndex,pageSize,totalCount);
        }

        public Role GetRoleById(int id)
        {
            return _omsAccessor.Get<Role>().Where(x => x.Isvalid && x.Id == id).FirstOrDefault();
        }

        public void UpdateRole(Role role)
        {
            role.ModifiedBy = _workContext.CurrentUser.Id;
            role.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<Role>(role);
            _omsAccessor.SaveChanges();
        }

        public void AddRole(Role role)
        {
            role.CreatedBy = _workContext.CurrentUser.Id;
            role.CreatedTime = DateTime.Now;
            _omsAccessor.Insert<Role>(role);
            _omsAccessor.SaveChanges();
        }

        public void DeleteRole(Role role)
        {
            _omsAccessor.Delete<Role>(role);
            _omsAccessor.SaveChanges();
        }

        public void DeleteRangeRole(List<Role> roles)
        {
            _omsAccessor.DeleteRange<Role>(roles);
            _omsAccessor.SaveChanges();
        }

        public IQueryable<Role> GetAllRoles()
        {
            return _omsAccessor.Get<Role>().Where(x => x.Isvalid);
        }

        public Role GetRoleByCode(string code)
        {
            return _omsAccessor.Get<Role>().Where(x => x.Isvalid && x.Code == code).FirstOrDefault();
        }

        public Role GetRoleByName(string name)
        {
            return _omsAccessor.Get<Role>().Where(x => x.Isvalid && x.Name == name).FirstOrDefault();
        }

        public void AddRangeRole(List<Role> roles)
        {
            foreach (var role in roles)
            {
                role.CreatedBy = _workContext.CurrentUser.Id;
                role.CreatedTime = DateTime.Now;
                _omsAccessor.Insert<Role>(role);
            }
            _omsAccessor.SaveChanges();
        }
    }
}
