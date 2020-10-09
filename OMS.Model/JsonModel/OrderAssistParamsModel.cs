using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.JsonModel
{
    public  class OrderAssistParamsModel
    {
        public string method { get; set; }

        public string app_key { get; set; }

        public string v { get; set; }

        public string sd_id { get; set; }

        public string order_state { get; set; }

        public string page_no { get; set; }

        public string page_size { get; set; }

        public string timestamp { get; set; }
        public string data { get; set; }

        public string sgin { get; set; }
    }
}
