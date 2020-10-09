using System.Collections.Generic;

namespace OMS.Web
{
    public class SiteMap
    {
        public SiteMap()
        {
            Nodes = new List<MenuNode>();
        }
        public IList<MenuNode> Nodes { get; set; }
    }

    public class MenuNode
    {
        public MenuNode()
        {
            ChildNodes = new List<MenuNode>();
        }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public string Permit { get; set; }
        public IList<MenuNode> ChildNodes { get; set; }
    }
}