using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OMS
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举对应值下的描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Description(this Enum value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(Enum.GetName(type, value));
            var descriptionAttribute = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return descriptionAttribute == null ? Enum.GetName(type, value) : descriptionAttribute.Description;
        }

        public static Dictionary<string, int> GetList(this Enum value, params string[] removeStarts)
        {
            var type = value.GetType();
            var values = Enum.GetValues(type);
            var result = new Dictionary<string, int>();
            foreach (var i in values)
            {
                var em = (Enum)i;
                if (removeStarts != null)
                {
                    var emStr = em.ToString();
                    if (removeStarts.Any(s => emStr.StartsWith(s)))
                        continue;
                }
                result.Add(em.Description(), Convert.ToInt16(i));
            }
            return result;
        }

        public static Dictionary<int, string> GetEnumList(this Enum value, params string[] removeStarts)
        {
            var type = value.GetType();
            var values = Enum.GetValues(type);
            var result = new Dictionary<int, string>();
            foreach (var i in values)
            {
                var em = (Enum)i;
                if (removeStarts != null)
                {
                    var emStr = em.ToString();
                    if (removeStarts.Any(s => emStr.StartsWith(s)))
                        continue;
                }
                result.Add(Convert.ToInt16(i), em.Description());
            }
            return result;
        }

        public static dynamic GetEnumList(object state)
        {
            throw new NotImplementedException();
        }
    }
}
