using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model
{
   public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentCategoryId { get; set; }
    }
}
