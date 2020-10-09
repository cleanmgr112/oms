using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.Suppliers
{
    public class Supplier:EntityBase
    {
        public string SupplierName { get; set; }

        /// <summary>
        /// 供应商是否已经同步到WMS
        /// </summary>
        public bool IsSynchronized { get; set; }
    }
}
