using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class ApprovalProcessDetail :EntityBase
    {
        public int UserId { get; set; }
        public int Sort { get; set; }
        public int ApprovalProcessId { get; set; }
        public ApprovalProcess ApprovalProcess { get; set; }
        public User User { get; set; }
    }
}
