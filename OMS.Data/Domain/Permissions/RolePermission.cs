using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Permissions
{
    public class RolePermission:EntityBase
    {
        public int? RoleId { get; set; }
        public int? PermissionId { get; set; }

    }
}
