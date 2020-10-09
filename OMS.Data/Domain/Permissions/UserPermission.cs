using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Permissions
{
    public class UserPermission:EntityBase
    {
        public int? UserId { get; set; }
        public int? PermissionId { get; set; }

    }
}
