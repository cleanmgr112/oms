using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class CMBOrderLog:EntityBase
    {
        public string OrderNum { get; set; }
        public string Message { get; set; }
    }
}
