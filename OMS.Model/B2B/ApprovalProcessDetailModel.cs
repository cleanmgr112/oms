using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.B2B
{
    public class ApprovalProcessDetailModel:ModelBase
    {
        public int UserId { get; set; }
        public int Sort { get; set; }
        public int ApprovalProcessId { get; set; }
        public string UserName { get; set; }
    }
}
