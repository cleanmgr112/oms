using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Role
{
    public class RoleModel:ModelBase
    {

        public int? ParentId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remark { get; set; }
        public int? Sort { get; set; }
        public string State { get; set; }
        public string CreatedTime { get; set; }

    }
}
