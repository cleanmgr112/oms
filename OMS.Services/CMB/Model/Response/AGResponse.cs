using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantPortalOpenSDK.Model.Response
{
    public class AGResponse
    {
        public int respCode { get; set; }

        public string respMsg { get; set; }

        public string EncryptBody { get; set; }
        public string Date { get; set; }
        public string Sign { get; set; }
    }
}
