using OMS.Core;
using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services.Permissions
{
    public interface IRoleService
    {
        IQueryable<Role> GetRolesByUserId(int userId);

        IPageList<Role> SearchRolesByPage(int pageIndex, int pageSize);
        IQueryable<Role> GetAllRoles();
        Role GetRoleById(int id);
        Role GetRoleByCode(string code);
        Role GetRoleByName(string name);
        void UpdateRole(Role role);
        void AddRole(Role role);
        void AddRangeRole(List<Role> roles);
        void DeleteRole(Role role);
        void DeleteRangeRole(List<Role> roles);
    }
}
