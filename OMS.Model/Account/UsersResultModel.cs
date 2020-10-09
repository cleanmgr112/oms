using OMS.Model.Grid;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Account
{
    public class UsersResultModel
    {
        public IList<FieldError> FieldErrors { get; set; }
        public IList<UserModel> Data { get; set; }
        public string Error { get; set; }
    }
}
