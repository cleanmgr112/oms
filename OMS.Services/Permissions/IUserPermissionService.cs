using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.Permissions
{
    public interface IUserPermissionService
    {
        void AddRangeUserPermission(List<UserPermission> list);
        void DelRangeUserPermission(List<UserPermission> list);
        UserPermission GetPermission(int userId, int permissionId);
    }
}
