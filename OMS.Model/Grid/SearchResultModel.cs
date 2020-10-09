using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Grid
{
    public class SearchResultModel
    {
        public int Draw { get; set; }
        public int RecordsFiltered { get; set; }
        public int RecordsTotal { get; set; }
        public object Data { get; set; }

        public bool isSucc { get; set; }

    }
}
