using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Permissions
{
    public class UserRole: EntityBase
    {
        public int? UserId { get; set; }
        public int? RoleId { get; set; }

    }
}
