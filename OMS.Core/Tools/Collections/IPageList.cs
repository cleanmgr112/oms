using System.Collections.Generic;

namespace OMS.Core
{
    public interface IPageList<T> : IList<T>
    {
        int PageIndex { get; set; }

        int PageSize { get; set; }

        int TotalCount { get; set; }

        int PageNumber { get; set; }

        int TotalPages { get; set; }
    }
}
