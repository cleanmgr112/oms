namespace OMS.Core.Tools
{

    public class AliYunTools
    {
        /// <summary>
        /// OSS (Endpoint) URL
        /// </summary>
        public static string _ossEndpointURL = AppConfigurtaionServices.Configuration["OSS:OssEndPoint"];

        /// <summary>
        /// OSS Bucket Name
        /// </summary>
        public static string _ossBucketName = AppConfigurtaionServices.Configuration["OSS:OssBucketName"];
    }
}
