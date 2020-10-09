using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Permission
{
    public class PermissionMultiSelectModel
    {
        public string Optgroup { get; set; }  
        public List<PermissionOption> Options { get; set; }
    }

    public class PermissionOption
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public bool Selected { get; set; }
    }
}
