using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Permissions
{
    public class Menu:EntityBase
    {
        public Menu()
        {
            this.RoleMenus = new List<RoleMenu>();
        }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string ModuleName { get; set; }
        public string Code { get; set; }
        public string Level { get; set; }
        public string Url { get; set; }
        public string ChildUrl { get; set; }
        public string Icon { get; set; }
        public int? Sort { get; set; }
        public bool State { get; set; }
        public string Remark { get; set; }

        public virtual ICollection<RoleMenu> RoleMenus { get; set; }
    }
}
