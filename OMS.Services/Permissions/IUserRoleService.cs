using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services.Permissions
{
    public interface IUserRoleService
    {
        IQueryable<UserRole> GetUserRolesByUserId(int userId);
        void AddUserRoleRange(List<UserRole> list);
        UserRole GetUserRole(int id);
        UserRole GetUserRole(int userId,int roleId);
        void DeleteUserRoleRange(List<UserRole> list);
    }
}
