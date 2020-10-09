using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Menu
{
    public class MenuModel
    {
        public int Id { get; set; }
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

        public IList<MenuModel> ChildMenus { get; set; }
    }
}
