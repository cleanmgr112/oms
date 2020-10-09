using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.Permissions
{
    public interface IRolePermissionService
    {
        RolePermission GetRolePermission(int roleId,int permissionId);
        void AddRangeRolePermission(List<RolePermission> list);
        void DelRangeRolePermission(List<RolePermission> list);
    }
}
