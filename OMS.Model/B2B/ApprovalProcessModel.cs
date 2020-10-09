
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.B2B
{
   public class ApprovalProcessModel:ModelBase
    {

        public string Name { get; set; }
        public List<ApprovalProcessDetailModel> ApprovalProcessDetailModel { get; set; }
    }
}
