using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Role
{
    public class RoleMultiSelectModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public bool Selected { get; set; }
    }
}
