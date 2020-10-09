using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Menu
{
    public class TreeState
    {
        /// <summary>
        /// 节点是否被选中
        /// </summary>
        public bool? Selected { get; set; }
        /// <summary>
        /// 节点是否可选
        /// </summary>
        public bool? Disabled { get; set; }
        /// <summary>
        /// 节点是否打开
        /// </summary>
        public bool? Opened { get; set; }
        /// <summary>
        /// 用于checkbox插件 - 勾选该checkbox(只有当 tie_selection 处于 false时有效)
        /// </summary>
        public bool? Checked  { get; set; }
        /// <summary>
        /// 用于checkbox插件 - 状态待定 (只有启用懒加载并且节点没有被加载时生效).
        /// </summary>
        public bool? Undetermined { get; set; }

    }
    public class MenuChild
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public TreeState State { get; set; }
        public string Icon { get; set; }
    }
    public class MenuJsonModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public TreeState State { get; set; }
        public string Icon { get; set; }
        public IList<MenuChild> Children { get; set; }

    }
}
