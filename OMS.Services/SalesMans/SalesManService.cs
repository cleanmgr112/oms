using OMS.Core;
using OMS.Data.Domain.SalesMans;
using OMS.Data.Interface;
using OMS.Model.SalesMans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMS.Services
{
    public class SalesManService:ServiceBase, ISalesManService
    {
        #region ctor
        public SalesManService(IDbAccessor omsAccessor, IWorkContext workContext)
            : base(omsAccessor, workContext)
        {

        }
        #endregion
        public SalesMan GetSalesManById(int? id)
        {
            return _omsAccessor.Get<SalesMan>().Where(r => r.Isvalid && r.Id == id).FirstOrDefault();
        }
        public void AddSalesMan(SalesMan salesMan)
        {
            salesMan.CreatedBy = _workContext.CurrentUser.Id;
            salesMan.CreatedTime = DateTime.Now;
            _omsAccessor.Insert<SalesMan>(salesMan);
            _omsAccessor.SaveChanges();
        }

        public void UpdateSalesMan(SalesMan salesMan)
        {
            salesMan.ModifiedBy = _workContext.CurrentUser.Id;
            salesMan.ModifiedTime = DateTime.Now;

            _omsAccessor.Update<SalesMan>(salesMan);
            _omsAccessor.SaveChanges();
        }

        public void DeleteSalesMan(int id)
        {
            //_omsAccessor.DeleteById<SalesMan>(id);
            //_omsAccessor.SaveChanges();
            var salesMan = _omsAccessor.GetById<SalesMan>(id);
            if(salesMan!=null)
            {
                salesMan.Isvalid = false;
                salesMan.ModifiedBy = _workContext.CurrentUser.Id;
                salesMan.ModifiedTime = DateTime.Now;
                _omsAccessor.Update<SalesMan>(salesMan);
                _omsAccessor.SaveChanges();
            }
        }

        public PageList<SalesManModel> GetSalesMansByPage(int pageIndex, int pageSize, string searchVal)
        {
            var result = (from s in _omsAccessor.Get<SalesMan>()
                          where s.Isvalid
                          orderby s.CreatedTime descending
                          select new SalesManModel
                          {
                              Id = s.Id,
                              UserName = s.UserName,
                              Code =string.IsNullOrEmpty(s.Code)?"":s.Code,
                              DepartmentType = s.Department,
                              DepartmentName = s.Department.Description()
                          }).ToList();
            if (!string.IsNullOrEmpty(searchVal))
            {
                result = result.Where(r => r.UserName.Contains(searchVal) || r.Code.Contains(searchVal)).ToList();
            }
            return new PageList<SalesManModel>(result, pageIndex, pageSize);
        }

        public SalesMan GetSalesManByNameAndCode(string name, string code)
        {
            return _omsAccessor.Get<SalesMan>().Where(r => r.Isvalid && r.UserName.Equals(name) && r.Code.Equals(code)).FirstOrDefault();
        }

        public List<SalesMan> GetAllSalesMans()
        {
            return _omsAccessor.Get<SalesMan>().Where(r => r.Isvalid).ToList();
        }
    }
}
