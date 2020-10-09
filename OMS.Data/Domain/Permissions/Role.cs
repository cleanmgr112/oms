using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Permissions
{
    public class Role:EntityBase
    {
        public Role()
        {
            this.UserRoles = new List<UserRole>();
            this.RolePermissions = new List<RolePermission>();
            this.RoleMenus = new List<RoleMenu>();
        }
        public int? ParentId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public int? Sort { get; set; }
        public int State { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
        public virtual ICollection<RoleMenu> RoleMenus { get; set; }


    }
}
