using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Permission
{
    public class PermissionModel
    {
        public virtual int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Category { get; set; }
        public string Remark { get; set; }
        public bool State { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public bool Isvalid { get; set; }
        public DateTime CreatedTime { get; set; }
        

    }
}
