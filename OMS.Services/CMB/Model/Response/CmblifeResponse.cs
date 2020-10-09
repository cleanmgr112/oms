using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantPortalOpenSDK.Model.Response
{
    public class CmblifeResponse
    {
        public int RespCode { get; set; }

        public string RespMsg { get; set; }

        public string EncryptBody { get; set; }

        public string Ddate { get; set; }

        public string Sign { get; set; }
    }
}
