using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aliyun.OSS;

namespace OMS.Core
{
    public class OSSHelper
    {
        private const string OSSACCESSKEYID = "JdKqgwEMDTe04XDI";
        private const string ACCESSKEYSECRET = "P5KRg7D7RlYg2ATikqFieM19IB5X4y";
        /// <summary>
        /// 由用户指定的OSS访问地址、阿里云颁发的AccessKeyId/AccessKeySecret构造一个新的OssClient实例。
        /// </summary>
        /// <param name="endpoint">OSS的访问地址。</param>
        /// <param name="accessKeyId">OSS的访问ID。</param>
        /// <param name="accessKeySecret">OSS的访问密钥。</param>
        public static void CreateClient(string endpoint, string accessKeyId, string accessKeySecret)
        {
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);
        }

        /// <summary>
        /// 在OSS中创建一个新的存储空间。
        /// </summary>
        /// <param name="bucketName">要创建的存储空间的名称</param>
        public static void CreateBucket(string bucketName)
        {
            var client = new OssClient(Tools.AliYunTools._ossEndpointURL, OSSACCESSKEYID, ACCESSKEYSECRET);
            try
            {
                client.CreateBucket(bucketName);
                client.SetBucketAcl(bucketName, CannedAccessControlList.PublicRead);//设置权限为公共读写
            }
            catch (Exception ex)
            {
                // LogText();
                throw new Exception(string.Format("Create bucket failed, {0}", ex.Message));
            }
        }
        /// <summary>
        /// 设置存储空间的访问权限
        /// </summary>
        /// <param name="bucketName">存储空间的名称</param>
        public static void SetBucketAcl(string bucketName)
        {
            var client = new OssClient(Tools.AliYunTools._ossEndpointURL, OSSACCESSKEYID, ACCESSKEYSECRET);
            try
            {
                // 指定Bucket ACL为公共读
                client.SetBucketAcl(bucketName, CannedAccessControlList.PublicRead);
                Console.WriteLine("Set bucket ACL succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Set bucket ACL failed. {0}", ex.Message);
            }
        }
        /// <summary>
        /// 判断存储空间是否存在
        /// </summary>
        /// <param name="bucketName">存储空间的名称</param>
        public static bool DoesBucketExist(string bucketName)
        {
            var client = new OssClient(Tools.AliYunTools._ossEndpointURL, OSSACCESSKEYID, ACCESSKEYSECRET);
            try
            {
                var exist = client.DoesBucketExist(bucketName);
                return exist;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Check object Exist failed. {0}", ex.Message));
                return false;
            }
        }

        public static void UploadFile(string key, string filePath, out string msg)
        {

            try
            {
                msg = string.Empty;
                // 初始化OssClient
                var client = new OssClient(Tools.AliYunTools._ossEndpointURL, OSSACCESSKEYID, ACCESSKEYSECRET);

                ObjectMetadata media = new ObjectMetadata();
                media.AddHeader("Content-Type", GetContentType(key));
                client.PutObject(Tools.AliYunTools._ossBucketName, key, filePath, media);

            }
            catch (Exception ex)
            {
                msg = string.Format("Put object failed, {0}", ex.Message);
            }
        }
        public static void UploadFileByStream(string key, Stream stream, out string msg)
        {
            try
            {
                msg = string.Empty;
                // 初始化OssClient
                var client = new OssClient(Tools.AliYunTools._ossEndpointURL, OSSACCESSKEYID, ACCESSKEYSECRET);
                if (!DoesBucketExist(Tools.AliYunTools._ossBucketName))
                {
                    CreateBucket(Tools.AliYunTools._ossBucketName);
                }
                ObjectMetadata media = new ObjectMetadata();
                media.AddHeader("Content-Type", GetContentType(key));
                client.PutObject(Tools.AliYunTools._ossBucketName, key, stream, media);

            }
            catch (Exception ex)
            {
                msg = string.Format("Put object failed, {0}", ex.Message);
            }
        }

        public static void UploadFileExistBucketName(string key, string filePath)
        {
            try
            {
                // 初始化OssClient
                var client = new OssClient(Tools.AliYunTools._ossEndpointURL, OSSACCESSKEYID, ACCESSKEYSECRET);
                ObjectMetadata media = new ObjectMetadata();
                media.AddHeader("Content-Type", GetContentType(key));
                client.PutObject(Tools.AliYunTools._ossBucketName, key, filePath, media);

            }
            catch (Exception ex)
            {
                // IOHelper.LogText(string.Format("Put object failed, {0}", ex));
                throw new Exception(string.Format("Put object failed, {0}", ex.Message));
            }
        }
        private static string GetContentType(string url)
        {
            string extensn = Path.GetExtension(url);
            if (extensn == null)
            {
                return "Image/jpeg";
            }
            switch (extensn.ToLower())
            {
                case ".gif":
                    return "Image/gif";
                case ".jpg":
                    return "Image/jpeg";
                case ".png":
                    return "Image/png";
                default:
                    break;
            }
            return null;
        }

        public static string GetExtenisonByContentType(string contentType)
        {
            switch (contentType.ToLower())
            {
                case "image/jpeg":
                    return ".jpg";
                case "image/gif":
                    return ".gif";
                case "image/png":
                    return ".png";
                default:
                    break;
            }
            return null;
        }
    }
}
