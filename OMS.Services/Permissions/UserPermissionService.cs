using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OMS.Core;
using OMS.Data.Domain.Permissions;
using OMS.Data.Interface;

namespace OMS.Services.Permissions
{
    public class UserPermissionService : ServiceBase, IUserPermissionService
    {
        public UserPermissionService(IDbAccessor omsAccessor, IWorkContext workContext) : base(omsAccessor, workContext)
        {
        }

        public void AddRangeUserPermission(List<UserPermission> list)
        {
            foreach (var userPermission in list)
            {
                userPermission.CreatedBy = _workContext.CurrentUser.Id;
                userPermission.CreatedTime = DateTime.Now;
                _omsAccessor.Insert<UserPermission>(userPermission);
            }
            _omsAccessor.SaveChanges();
        }

        public void DelRangeUserPermission(List<UserPermission> list)
        {
            _omsAccessor.DeleteRange<UserPermission>(list);
            _omsAccessor.SaveChanges();
        }

        public UserPermission GetPermission(int userId, int permissionId)
        {
            return _omsAccessor.Get<UserPermission>().Where(x => x.Isvalid && x.UserId == userId && x.PermissionId == permissionId).FirstOrDefault();
        }
    }
}
