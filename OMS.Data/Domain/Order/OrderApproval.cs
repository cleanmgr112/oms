using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
   public class OrderApproval:EntityBase
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int Sort { get; set; }
        public OrderApprovalState State { get; set; }
        public User User { get; set; }
        public Order Order { get; set; }
    }
}
