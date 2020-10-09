using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class Delivery : EntityBase
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public string ShopCode { get; set; }
        public List<Order> Order { get; set; }


        /// <summary>
        /// 快递方式是否已经同步到WMS
        /// </summary>
        public bool IsSynchronized { get; set; }
    }
}

