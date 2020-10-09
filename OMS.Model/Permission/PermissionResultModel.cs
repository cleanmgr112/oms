using OMS.Model.Grid;
using OMS.Model.Role;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Permission
{
    public class PermissionResultModel
    {
        public IList<FieldError> FieldErrors { get; set; }
        public IList<PermissionModel> Data { get; set; }
        public string Error { get; set; }

    }

}
