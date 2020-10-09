using System;
using System.Collections.Generic;

namespace OMS.Data.Domain
{
    public class User : EntityBase
    {
        public string UserName { get; set; }
        public string UserPwd { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Salt { get; set; }
        public UserState State { get; set; }
        public DateTime LastLoginTime { get; set; }
        public string LastLoginIp { get; set; }

        public List<ApprovalProcessDetail> ApprovalProcessDetail { get; set; }
        public List<OrderApproval> OrderApproval { get; set; }

    }
}
