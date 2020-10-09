using OMS.Data.Domain.SalesMans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.SalesMans
{
    public class SalesManModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public DepartmentType DepartmentType { get; set; }
        public string DepartmentName { get; set; }
    }
}
