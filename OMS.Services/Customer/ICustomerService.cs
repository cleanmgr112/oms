using OMS.Data.Domain;
using System.Collections.Generic;
using System.Linq;
namespace OMS.Services.Customer
{
    public interface ICustomerService
    {
        Customers GetById(int id);
        bool GetCountByName(string name);
        bool Add(Customers customer);
        List<Customers> GetAllCustomerList();
        IEnumerable<Customers> GetAllCustomerList(string searchStr);
        void UpdateCustomer(Customers customers);
        bool DelCustomerById(int id);
    }
}
