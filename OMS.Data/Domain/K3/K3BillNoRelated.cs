using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class K3BillNoRelated:EntityBase
    {
        public string K3BillNo { get; set; }
        public string OMSSeriNo { get; set; }
        public string Message { get; set; }
    }
}
