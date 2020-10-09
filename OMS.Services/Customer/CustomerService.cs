using Microsoft.EntityFrameworkCore;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;


namespace OMS.Services.Customer
{
   public class CustomerService:ServiceBase, ICustomerService
    {
        #region ctor
        public CustomerService(IDbAccessor omsAccessor, IWorkContext workContext)
            : base(omsAccessor, workContext)
        {

        }
        #endregion
        public List<Customers> GetAllCustomerList()
        {
            return _omsAccessor.Get<Customers>().Include(i => i.Dictionary).Where(p => p.Isvalid).ToList();
        }
        public IEnumerable<Customers> GetAllCustomerList(string searchStr)
        {
            var result = _omsAccessor.Get<Customers>().Where(r => (string.IsNullOrEmpty(searchStr) 
            || r.Name.Contains(searchStr)
            || r.Contact.Contains(searchStr)
            || r.Mobile.Contains(searchStr)
            || r.Address.Contains(searchStr))).Include(r => r.Dictionary).ToList();
            return result;
        }
        public Customers GetById(int id)
        {
            return _omsAccessor.Get<Customers>().Where(x => x.Isvalid && x.Id == id).FirstOrDefault();
        }
        public bool GetCountByName(string name)
        {
            IQueryable<Customers> count = _omsAccessor.Get<Customers>().Where(x => x.Name.Equals(name));
            if (count.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Add(Customers customer)
        {
            if (customer == null)
                throw new ArgumentException("Customer");
            customer.Isvalid = true;
            customer.CreatedBy = _workContext.CurrentUser.Id;
            customer.ModifiedTime = DateTime.Now;
            customer.CreatedTime = DateTime.Now;
            _omsAccessor.Insert(customer);
            _omsAccessor.SaveChanges();
            return true;
            
        }
        public void UpdateCustomer(Customers customers)
        {
            if (customers == null)
                throw new ArgumentException("Customer");
            customers.ModifiedBy = _workContext.CurrentUser.Id;
            customers.ModifiedTime = DateTime.Now;
            _omsAccessor.Update(customers);
            _omsAccessor.SaveChanges();
        }
        public bool DelCustomerById(int id)
        {
            var delData=_omsAccessor.Get<Customers>().Where(x => x.Isvalid && x.Id == id).FirstOrDefault();
            if(delData==null)
                throw new ArgumentException("Customer");
            else
                _omsAccessor.DeleteById<Customers>(id);
                _omsAccessor.SaveChanges();
                return true;
        }
    }
}
