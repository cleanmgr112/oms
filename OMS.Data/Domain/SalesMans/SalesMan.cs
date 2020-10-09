using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain.SalesMans
{
    public class SalesMan:EntityBase
    {
        /// <summary>
        /// 所属部门
        /// </summary>
        public DepartmentType Department { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
    }
}
