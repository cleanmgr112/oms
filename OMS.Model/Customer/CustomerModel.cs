using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OMS.Model.Customer
{
   public class CustomerModel:ModelBase
    {
        public int CustomerTypeId { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
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
    }
}
