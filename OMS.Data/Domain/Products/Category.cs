using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class Category:EntityBase
    {
        public string Name { get; set; }
        public int ParentCategoryId { get; set; }
    }
}
