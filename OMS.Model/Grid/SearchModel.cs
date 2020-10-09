using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Grid
{
    public class SearchModel
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public int PageIndex { get; set; }
        public string Sort { get; set; }
        public string SearchValue { get; set; }
        public Dictionary<string,string> Search { get; set; }
    }
}
