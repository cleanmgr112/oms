using OMS.Model.Grid;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Role
{
    public class RoleResultModel
    {
        public IList<FieldError> FieldErrors { get; set; }
        public IList<RoleModel> Data { get; set; }
        public string Error { get; set; }

    }
}
