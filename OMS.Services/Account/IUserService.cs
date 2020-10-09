using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Domain.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace OMS.Services.Account
{
    public interface IUserService
    {
        User GetById(int id);
        User GetByUserName(string userName);
        User GetUserByName(string name);
        void Update(User user);
        IQueryable<User> GetAllUserList();
        IPageList<User> GetUsersByPage(string searchValue, int pageIndex, int pageSize);
        User CreateUser(User user);
        User UpdateUser(User user);
        /// <summary>
        /// 软删除用户
        /// </summary>
        /// <param name="users"></param>
        void SoftDeleteUserRange(List<User> users);
        /// <summary>
        /// 通过用户ID获取用户角色列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        ICollection<UserRole> GetUserRoleByUserId(int userId);
        /// <summary>
        /// 获取用户权限
        /// </summary>
        /// <returns></returns>
        ICollection<Permission> AccountPermissionRecords(User user);
    }
}