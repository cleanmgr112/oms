using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class OrderPayPrice:EntityBase
    {
        public int OrderId { get; set; }
        public bool IsPay { get; set; }
        public int PayType { get; set; }
        public int PayMentType { get; set; }
        public decimal Price { get; set; }
        public string Mark { get; set; }
        public Order Order { get; set; }
    }
}
