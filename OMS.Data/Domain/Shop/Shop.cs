using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class Shop:EntityBase
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public List<Order> Order { get; set; }
    }
}
