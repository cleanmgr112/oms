using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OMS.Core;
using OMS.Data.Domain.Permissions;
using OMS.Data.Interface;

namespace OMS.Services.Permissions
{
    public class RolePermissionService : ServiceBase, IRolePermissionService
    {
        public RolePermissionService(IDbAccessor omsAccessor, IWorkContext workContext) : base(omsAccessor, workContext)
        {
        }


        public void AddRangeRolePermission(List<RolePermission> list)
        {
            foreach (var rolePermission in list)
            {
                rolePermission.CreatedBy = _workContext.CurrentUser.Id;
                rolePermission.CreatedTime = DateTime.Now;
                _omsAccessor.Insert<RolePermission>(rolePermission);
            }
            _omsAccessor.SaveChanges();
        }

        public void DelRangeRolePermission(List<RolePermission> list)
        {
            _omsAccessor.DeleteRange<RolePermission>(list);
            _omsAccessor.SaveChanges();
        }

        public RolePermission GetRolePermission(int roleId, int permissionId)
        {
            return _omsAccessor.Get<RolePermission>().Where(x => x.Isvalid && x.RoleId == roleId
                && x.PermissionId == permissionId).FirstOrDefault();
        }

    }
}
