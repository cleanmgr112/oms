using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Permissions
{
    public class Permission:EntityBase
    {
        public Permission()
        {
            this.RolePermissions = new List<RolePermission>();
            this.UserPermissions = new List<UserPermission>();
        }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Category { get; set; }
        public string Remark { get; set; }
        public bool State { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; }

    }
}
