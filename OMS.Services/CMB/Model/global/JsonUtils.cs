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
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;

namespace CmblifeOpenSDK
{
    public class JsonUtils
    {

        /// <summary>
        /// 将json字符串反序列化为Dictionary
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonStrToDic(string json)
        {
            
            return JsonObjectToDic(StringToJsonObject(json));
        }

        /// <summary>
        /// 将string转化为jsonObject
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static JObject StringToJsonObject(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            return (JObject)JsonConvert.DeserializeObject(str);
        }


        /// <summary>
        /// 将JsonObject转为Dictionary
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static Dictionary<string, object> JsonObjectToDic(JObject json)
        {
            if (null == json)
            {
               return null;
            }
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json.ToString());
        }


        /// <summary>
        /// 将Dictionary序列化为json字符串
        /// </summary>
        /// <param name="dicParams"></param>
        /// <returns></returns>
        public static string DicToJson(Dictionary<string, object> dicParams)
        {
            return ObjectToJson(dicParams);
        }

        /// <summary>
        /// 将对象序列化为json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
