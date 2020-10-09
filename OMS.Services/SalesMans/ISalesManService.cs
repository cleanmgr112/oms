using OMS.Core;
using OMS.Data.Domain.SalesMans;
using OMS.Model.SalesMans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services
{
    public interface ISalesManService
    {
        SalesMan GetSalesManById(int? id);
        void AddSalesMan(SalesMan salesMan);
        void UpdateSalesMan(SalesMan salesMan);
        void DeleteSalesMan(int id);
        PageList<SalesManModel> GetSalesMansByPage(int pageIndex, int pageSize, string searchVal);

        SalesMan GetSalesManByNameAndCode(string name, string code);

        List<SalesMan> GetAllSalesMans();
    }
}
