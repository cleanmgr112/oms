using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class K3BaseData : EntityBase
    {
        public int Type { get; set; }
        public int No { get; set; }
        public string FNumber { get; set; }
        public string FName { get; set; }

    }
}
