using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class ApprovalProcess:EntityBase
    {
        /// <summary>
        /// 审批流程
        /// </summary>
        public string Name { get; set; }
        public List<ApprovalProcessDetail> ApprovalProcessDetail { get; set; }
    }
}
