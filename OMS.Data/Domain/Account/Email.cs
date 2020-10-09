using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace OMS.Data.Domain
{
    public class Email : EntityBase
    {
        public Email()
        {
            Subject = "";
        }
        /// <summary>
        /// 邮件发送方跟配置发送帐号一致，无需单独指定
        /// </summary>
        public MailAddress MailFrom { get; set; }
        public List<MailAddress> MailTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
