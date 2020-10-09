using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace MerchantPortalOpenSDK.util
{
    public class Httputil
    {
        public static T Post<T>(string url, Dictionary<string, object> parameters)
        {
            HttpWebResponse httpResponse = HttpPost(url, parameters);
            string respStr = GetResponseAsString(httpResponse);
            T response = JsonConvert.DeserializeObject<T>(respStr);
            return response;
        }
            
        /// <summary>
        /// 创建带参数的POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static HttpWebResponse HttpPost(string url, Dictionary<string, object> parameters)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream stream = null;
            try
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

                //写入请求流
                stream = request.GetRequestStream();

                //构造查询字符串
                if (!(parameters == null || parameters.Count == 0))
                {
                    byte[] data = Encoding.UTF8.GetBytes(BuildQuery(parameters));
                    stream.Write(data, 0, data.Length);
                }

                return (HttpWebResponse)request.GetResponse();
            }
            finally
            {
                if (stream != null) stream.Close();
                if (response != null) response.Close();
            }

        }



        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        static string BuildQuery(Dictionary<string, object> parameters)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;
            IEnumerator<KeyValuePair<string, object>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value.ToString();
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }
                    postData.Append(name);
                    postData.Append("=");
                    postData.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                    hasParam = true;
                }
            }
            return postData.ToString();
        }


        /// <summary>
        /// 把响应流转换为文本。
        /// </summary>
        /// <param name="rsp">响应流对象</param>
        /// <returns>响应文本</returns>
        static string GetResponseAsString(HttpWebResponse rsp)
        {
            Stream stream = null;
            StreamReader reader = null;
            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }
    }
}
