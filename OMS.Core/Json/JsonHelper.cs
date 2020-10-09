using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Core.Json
{
    public static class JsonHelper
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T ToObj<T>(this string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }
    }
}
