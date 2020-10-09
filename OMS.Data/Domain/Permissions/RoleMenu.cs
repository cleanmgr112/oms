using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Permissions
{
    public class RoleMenu:EntityBase
    {
        public int? RoleId { get; set; }
        public int? MenuId { get; set; }

    }
}
