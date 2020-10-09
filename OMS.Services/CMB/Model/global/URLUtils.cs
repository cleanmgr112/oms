//------------------------------------------------------------------------------------- 
// CMB Confidential 
// 
// Copyright (C) 2015 China Merchants Bank Co., Ltd. All rights reserved. 
// 
// No part of this file may be reproduced or transmitted in any form or by any means,  
// electronic, mechanical, photocopying, recording, or otherwise, without prior   
// written permission of China Merchants Bank Co., Ltd. 
// 
//-------------------------------------------------------------------------------------
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmblifeOpenSDK
{
    public class URLUtils
    {
       
        /// <summary>
        /// 拼接签名字符串
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <param name="querystring">参数</param>
        /// <returns>拼接后的字符串</returns>
        public static string AssembleUrl(string prefix, string querystring)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return querystring;
            }
            else
            {
                return prefix + (prefix.Contains("?") ? "&" : "?") + querystring;
            }
        }


        /// <summary>
        /// 拼接签名字符串
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <param name="paramsMap">参数</param>
        /// <param name="isUrlEncode">是否urlEncode</param>
        /// <returns>拼接后的字符串</returns>
        public static string AssembleUrl(string prefix, Dictionary<string, object> paramsMap, bool isUrlEncode)
        {
            return AssembleUrl(prefix, DicToQueryString(paramsMap, true, isUrlEncode));
        }

        /// <summary>
        /// 拼接掌上生活协议
        /// </summary>
        /// <param name="funcName">前缀</param>
        /// 
        /// <param name="querystring">参数</param>
        /// <returns>拼接后的协议字符串</returns>
        public static string AssembleProtocol(string funcName, string querystring)
        {
            return AssembleUrl(Constants.CMBLIFE_DEFAULT_PROCOTOL_PREFIX + funcName, querystring);
        }

        /// <summary>
        /// 拼接掌上生活协议
        /// </summary>
        /// <param name="funcName">功能名</param>
        /// <param name="paramsMap">参数</param>
        /// <param name="isUrlEncode">是否urlEncode</param>
        /// <returns>拼接后的字符串</returns>
        public static string AssembleProtocol(string funcName, Dictionary<string, object> paramsMap, bool isUrlEncode)
        {
            return AssembleUrl(Constants.CMBLIFE_DEFAULT_PROCOTOL_PREFIX + funcName, paramsMap, isUrlEncode);
        }

        /// <summary>
        /// dictionary转为querystring
        /// </summary>
        /// <param name="dic">参数dictionary</param>
        /// <param name="isSort">是否排序</param>
        /// <param name="isUrlEncode">是否需要URLEncode</param>
        /// <returns>querystring</returns>
        public static string DicToQueryString(Dictionary<string, object> dic, bool isSort, bool isUrlEncode)
        {
            Dictionary<string, object> tempDic;
            if (isSort)
            {
                tempDic = dic.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            }
            else
            {
                tempDic = dic;
            }

            // dictionary拼接
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> k in tempDic)
            {
                string key = k.Key;
                object value = k.Value;
                string val = "";
                if (value == null || (value is string && string.IsNullOrEmpty(value.ToString())))
                {
                    continue;
                }
                if (value is string)
                {
                    val = value.ToString();
                }

                else
                {
                    val = JsonConvert.SerializeObject(value);
                }

                if (isUrlEncode)
                {
                    val = StringUtils.UrlEncode(val);
                }

                sb.Append(key).Append("=").Append(val).Append("&");
            }
            string querystring = sb.ToString();
            if (querystring.Length > 1)
            {
                querystring = querystring.Substring(0, querystring.Length - 1);
            }
            return querystring;
        }
    }
}
