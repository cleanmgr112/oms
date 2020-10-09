using System;
using System.Collections.Generic;
using System.Linq;

namespace OMS.Core
{
    public class PageList<T> : List<T>, IPageList<T>
    {
        public PageList(IQueryable<T> source, int pageIndex = 1, int pageSize=10 )
        {
            Init(source.Skip((pageIndex - 1) * pageSize).Take(pageSize), pageIndex, pageSize, source.Count());
        }

        public PageList(IList<T> source, int pageIndex, int pageSize)
        {
            Init(source.Skip((pageIndex - 1) * pageSize).Take(pageSize), pageIndex, pageSize, source.Count);
        }

        public PageList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            Init(source, pageIndex, pageSize, totalCount);
        }

        private void Init(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
            AddRange(source);
        }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int TotalPages { get; set; }
    }
}
