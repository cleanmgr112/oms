

using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Dm.Model.V20151123;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices.ComTypes;

namespace OMS.Services.StockRemid
{
    public class Mail : IDisposable
    {
        private MailMessage MailMessage;   //主要处理发送邮件的内容（如：收发人地址、标题、主体、图片等等）
        private static readonly string mSenderServerHost = "smtp.exmail.qq.com";    //发件箱的邮件服务器地址（IP形式或字符串形式均可）
        private static readonly string mSenderPassword = "YqvK3vHDTD7hamv9";    //发件箱的密码
        private static readonly string mSenderUsername = "liuchaozhu@wine-world.com";   //发件箱的用户名（即@符号前面的字符串，例如：hello@163.com，用户名为：hello）
        private bool disposedValue;


        ///<summary>
        /// 构造函数
        ///</summary>
        ///<param name="toMail">收件人地址（可以是多个收件人，程序中是以“;"进行区分的）</param>
        ///<param name="subject">邮件标题</param>
        ///<param name="emailBody">邮件内容（可以以html格式进行设计）</param>
        ///<param name="sslEnable">true表示对邮件内容进行socket层加密传输，false表示不加密</param>
        public Mail(string toMail, string subject, string emailBody, bool sslEnable = true)
        {
            try
            {
                MailMessage = new MailMessage()
                {
                    From = new MailAddress(mSenderUsername),
                    Subject = subject,
                    Body = emailBody,
                    IsBodyHtml = false,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    Priority = MailPriority.Normal,
                };
                MailMessage.To.Add(toMail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        ///<summary>
        /// 添加附件
        ///</summary>
        ///<param name="attachmentsPath">附件的路径集合，以分号分隔</param>
        public void AddAttachments(string attachmentsPath)
        {
            try
            {
                string[] path = attachmentsPath.Split(';'); //以什么符号分隔可以自定义
                Attachment data;
                ContentDisposition disposition;
                for (int i = 0; i < path.Length; i++)
                {
                    data = new Attachment(path[i], MediaTypeNames.Application.Octet);
                    disposition = data.ContentDisposition;
                    disposition.CreationDate = File.GetCreationTime(path[i]);
                    disposition.ModificationDate = File.GetLastWriteTime(path[i]);
                    disposition.ReadDate = File.GetLastAccessTime(path[i]);
                    MailMessage.Attachments.Add(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        ///<summary>
        /// 邮件的发送
        ///</summary>
        public void Send()
        {
            try
            {
                if (MailMessage != null)
                {
                    SmtpClient smtp = new SmtpClient(); //实例化一个SmtpClient

                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network; //将smtp的出站方式设为 Network
                    smtp.EnableSsl = false; //smtp服务器是否启用SSL加密
                    smtp.Host = mSenderServerHost; //指定 smtp 服务器地址
                    //smtp.Port = 465; //指定 smtp 服务器的端口，默认是25，如果采用默认端口，可省去
                    smtp.UseDefaultCredentials = true;//SMTP服务器需要身份认证，目前基本没有不需要认证的了
                    smtp.Credentials = new NetworkCredential(mSenderUsername, mSenderPassword); //发件人邮箱的用户和密码(授权码，并不是发件邮箱的密码)
                    smtp.EnableSsl = true;
                    smtp.Send(MailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                MailMessage = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }



    /// <summary>
    /// 阿里云
    /// </summary>
    public class Email
    {

        private static string AccessKeyId ;

        private static string AccessKeySecret;

        private static string Mail = "support@wine-world.cn";

        public Email(IConfiguration Configuration)
        {
            AccessKeySecret = Configuration["OSS:AccessKeySecret"];
            AccessKeyId = Configuration["OSS:AccessKeyId"];
        }
        ///<param name="toMail">收件人地址（可以是多个收件人，程序中是以“;"进行区分的）</param>
        ///<param name="subject">邮件标题</param>
        ///<param name="emailBody">邮件内容（可以以html格式进行设计）</param>
        ///<param name="sslEnable">true表示对邮件内容进行socket层加密传输，false表示不加密</param>
        public  void Send(string toMail, string subject, string emailBody)
        {
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", AccessKeyId, AccessKeySecret);
            IAcsClient client = new DefaultAcsClient(profile);
            SingleSendMailRequest request = new SingleSendMailRequest();
            try
            {
                request.AccountName = Mail;
                request.FromAlias = "红酒世界";
                request.AddressType = 1;
                //request.TagName = "控制台创建的标签";
                request.ReplyToAddress = true;
                request.ToAddress = toMail;
                request.Subject = subject;
                request.HtmlBody = emailBody;
                SingleSendMailResponse httpResponse = client.GetAcsResponse(request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
