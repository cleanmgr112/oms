using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace OMS.Core
{
    public class HttpClientHelper
    {
        private static HttpClient _httpClient;
        public static HttpClient HttpClient
        {
            get
            {
                try
                {
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient();
                        _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
                    }
                    return _httpClient;
                }
                catch (Exception)
                {
                    return new HttpClient();
                }
            }
        }
    }
}
