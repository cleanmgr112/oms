using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Domain
{
    public class Customers : EntityBase
    {
        public int CustomerTypeId { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Mark { get; set; }
        public int PriceTypeId { get; set; }
        public decimal DisCount { get; set; }
        public string CustomerEmail { get; set; }
        public string Title { get; set; }
        public string TaxpayerId { get; set; }
        public string RegisterAddress { get; set; }
        public string RegisterTel { get; set; }
        public string BankOfDeposit { get; set; }
        public string BankAccount { get; set; }

        /// <summary>
        /// 客户是否已经同步到WMS
        /// </summary>
        public bool IsSynchronized { get; set; }
        public virtual Dictionary Dictionary { get; set; }
    }
}
