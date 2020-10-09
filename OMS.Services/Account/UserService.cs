using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Domain.Permissions;
using OMS.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OMS.Services.Account
{
    public class UserService : IUserService
    {
        #region ctor
        private readonly IDbAccessor _omsAccessor;
        public UserService(IDbAccessor omsAccessor)
        {
            _omsAccessor = omsAccessor;
        }
        #endregion

        public User GetById(int id)
        {
            return _omsAccessor.Get<User>().Where(x => x.Isvalid && x.Id == id).FirstOrDefault();
        }

        public User GetByUserName(string userName)
        {
            return _omsAccessor.Get<User>().FirstOrDefault(i => i.UserName == userName && i.Isvalid);
        }

        public IQueryable<User> GetAllUserList()
        {
            return _omsAccessor.Get<User>().Where(p => p.Isvalid && p.State == UserState.Enabled && p.Isvalid);
        }

        public void Update(User user)
        {
            _omsAccessor.Update(user);
            _omsAccessor.SaveChanges();
        }

        public IPageList<User> GetUsersByPage(string searchValue, int pageIndex, int pageSize)
        {
            var query = _omsAccessor.Get<User>().Where(x => x.Isvalid);
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(t => t.Name.Contains(searchValue) || t.UserName.Contains(searchValue)).OrderBy(p => p.Id);
            }
            else
            {
                query = query.OrderBy(p => p.Id);
            }
            return new PageList<User>(query, pageIndex, pageSize);
        }

        public User GetUserByName(string name)
        {
            return _omsAccessor.Get<User>().Where(x => x.Isvalid && x.Name == name).FirstOrDefault();
        }

        public User CreateUser(User user)
        {
            _omsAccessor.Insert(user);
            _omsAccessor.SaveChanges();
            return user;
        }

        public User UpdateUser(User user)
        {
            _omsAccessor.Update(user);
            _omsAccessor.SaveChanges();
            return user;
        }

        public void SoftDeleteUserRange(List<User> users)
        {
            foreach (var user in users)
            {
                user.Isvalid = false;
                _omsAccessor.Update<User>(user);
            }
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 通过用户ID获取用户角色列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ICollection<UserRole> GetUserRoleByUserId(int userId) {
            var role = _omsAccessor.Get<UserRole>().Where(u => u.Isvalid && u.UserId == userId).ToList();
            return role;
        }
        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <returns></returns>
        public ICollection<Permission> AccountPermissionRecords(User user)
        {
            var role = GetUserRoleByUserId(user.Id).Select(r => r.RoleId).ToList();
            var rolePermission = _omsAccessor.Get<RolePermission>().Where(r => r.Isvalid && role.Contains(r.RoleId.Value)).Select(r => r.PermissionId).ToList();
            var result = _omsAccessor.Get<Permission>().Where(p => p.Isvalid && rolePermission.Contains(p.Id)).ToList();
            return result;
        }
    }
}
