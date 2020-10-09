using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    public class WMSResult
    {
        public string state { get; set; }
        public List<int?> errOMSId { get; set; }        
    }
}
