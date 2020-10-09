using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Account
{
    public class UserViewModel
    {
        public virtual int Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string UserPwd { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Int16 State { get; set; }
        public DateTime LastLoginTime { get; set; }
        public string LastLoginIp { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
