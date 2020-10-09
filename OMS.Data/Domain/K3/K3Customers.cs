using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class K3Customers:EntityBase
    {
        public int Type { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
