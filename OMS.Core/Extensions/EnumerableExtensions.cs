using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OMS
{
    public static class EnumerableExtensions
    {

        #region IEnumerable
        /// <summary>
        /// Performs an action on each item while iterating through a list. 
        /// This is a handy shortcut for <c>foreach(item in list) { ... }</c>
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="source">The list, which holds the objects.</param>
        /// <param name="action">The action delegate which is called on each item while iterating.</param>
        //[DebuggerStepThrough]
        public static void Each<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T t in source)
            {
                action(t);
            }
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> source, IDictionary<string, string> maps = null)
        {
            if (source == null || source.Count() == 0)
                return new DataTable();

            var table = new DataTable();
            var properties = typeof(T).GetProperties();
            if (maps == null || maps.Count == 0)
            {
                foreach (var p in properties)//添加列名
                {
                    table.Columns.Add(p.Name);
                }
                foreach (var i in source)
                {
                    var row = table.NewRow();
                    foreach (var p in properties)
                    {
                        row[p.Name] = p.GetValue(i);
                    }
                    table.Rows.Add(row);
                }
            }
            else
            {
                foreach (var m in maps)//添加列名
                {
                    table.Columns.Add(m.Key);
                }
                foreach (var i in source)
                {
                    var row = table.NewRow();
                    foreach (DataColumn c in table.Columns)
                    {
                        var property = properties.FirstOrDefault(p => p.Name == maps[c.ColumnName]);
                        if (property != null)
                        {
                            row[c] = property.GetValue(i);
                        }
                    }
                    table.Rows.Add(row);
                }
            }
            return table;
        }
        #endregion
    }
}
