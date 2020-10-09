using System;

namespace OMS
{
    public static class ConversionExtensions
    {
        public static int[] ToIntArray(this string s, char separator)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            var array = s.Split(separator);
            if (array == null || array.Length == 0)
            {
                return null;
            }
            return Array.ConvertAll(array, i => int.Parse(i));
        }
    }
}
