using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OMS.Data.Domain;
using OMS.Data.Interface;
using OMS.Model.StockRemind;
using OMS.Services.StockRemid.StockRemindDto;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.Services.StockRemid
{
    /// <summary>
    /// 搜索接口  
    /// </summary>
    public interface ISearch
    {
        object Search(object ob, out int count, int page = 1, int limit = 10);
    }

    /// <summary>
    /// 设置接口
    /// </summary>
    public interface ISet
    {
        bool Set(object ob);
    }

    /// <summary>
    /// 模板设置
    /// </summary>
    public class TemplateSet : ISet
    {
        private readonly IDbAccessor omsAccessor;
        public TemplateSet(IDbAccessor omsAccessor)
        {
            this.omsAccessor = omsAccessor;
        }
        public bool Set(object ob)
        {
            var _user = string.Empty; var str = new List<UserDto>(); var dto = ob as SetDto;
            if (dto == null)
                return false;
            if (dto.User.Count != 0)
            {
                dto.User.ForEach(c => str.Add(new UserDto() { Id = c.Id, UserName = c.UserName }));
                _user = JsonConvert.SerializeObject(str);
            }

            dto.Key.ForEach(k =>
            {
                var template = omsAccessor.Get<RemindTemplateModel>().Include(c => c.Product).Where(c => c.TemplateCode == k.TemplateCode).FirstOrDefault();
                var product = new RemindProductdto() { Name = k.Name, ProductCode = k.ProductCode, En = k.En, Price = k.Price, Stock = k.Stock };
                if (template == null)
                {
                    var list = new RemindTemplateDto() { TemplateTitle = dto.TemplateTitle, User = _user, SaleProductId = k.SaleProductId, Product = product, RemindStock = dto.RemindStock };
                    var entity = new RemindTemplateModel();
                    Mapper.Map(list, entity);
                    omsAccessor.OMSContext.RemindTemplate.Add(entity);
                }
                else
                {
                    template.TemplateTitle = dto.TemplateTitle;
                    template.User = _user;
                    template.Product = Mapper.Map(product, template.Product);
                    template.RemindStock = dto.RemindStock;
                }
            });
            return omsAccessor.OMSContext.SaveChangesAsync().Result > 0;
        }
    }


    /// <summary>
    /// 模板搜索 
    /// </summary>
    public class TemplateSearch : ISearch
    {
        private readonly IConfiguration configuration;
        private readonly IDbAccessor omsAccessor;
        public TemplateSearch(IDbAccessor omsAccessor, IConfiguration configuration)
        {
            this.omsAccessor = omsAccessor;
            this.configuration = configuration;
        }

        /// <summary>
        /// SearchProductDto 条件
        /// </summary>
        public object Search(object ob, out int count, int page, int limit)
        {
            count = 0; var list = new List<SaleProductDto>();
            var search = ob as SearchProductDto;
            if (search == null)
                return null;
            //查询字典
            var productType = omsAccessor.Get<Dictionary>().AsNoTracking().Where(c => c.Value == search.ProductType).Select(c => (int?)c.Type).FirstOrDefault();
            using (var db = new SqlSugar(configuration).db)
            {
                count = 0;
                var data = db.Queryable<SaleProductPrice, SaleProduct, Product>((spp, sp, p) => new object[]{
                JoinType.Left,spp.SaleProductId==sp.Id,
                JoinType.Left,sp.ProductId==p.Id,
                }).Where((spp, sp, p) => sp.Isvalid &&
                        ((search.MaxPrice == null || spp.Price <= search.MaxPrice) && (search.MinPrice == null || spp.Price >= search.MinPrice)) &&
                      (productType == null || p.Type == productType)
                      ).GroupBy((spp, sp, p) => new { spp.SaleProductId, p.Name, p.NameEn, p.Code })
                      .Select((spp, sp, p) => new
                      {
                          SaleProductId = spp.SaleProductId,
                          Price = SqlFunc.AggregateAvg(spp.Price),
                          Stock = SqlFunc.AggregateSum(sp.AvailableStock),
                          Name = p.Name,
                          En = p.NameEn,
                          ProductCode = p.Code
                      }).ToPageList(page, limit, ref count);
                data.ForEach(c =>
                {
                    var template = omsAccessor.Get<RemindTemplateModel>().FirstOrDefault(d => d.SaleProductId == c.SaleProductId);
                    list.Add(new SaleProductDto()
                    {
                        SaleProductId = c.SaleProductId,
                        Price = c.Price,
                        Stock = c.Stock,
                        Name = c.Name,
                        En = c.En,
                        ProductCode = c.ProductCode,
                        TemplateCode = template?.TemplateCode,
                        Statu = template == null ? false : template.Statu,
                    });
                });
            }
            return list;
        }

        /// <summary>
        /// 根据商品编号获取已经预警的模板
        /// </summary>
        public IQueryable<RemindTemplateModel> Search(string productCode, out int count, int page = 1, int limit = 10)
        {
            var list = omsAccessor.Get<RemindTemplateModel>().AsNoTracking().Include(c => c.Product).Where(c => string.IsNullOrEmpty(productCode) || c.Product.ProductCode == productCode);
            count = list.Count();
            return list.Skip((page - 1) * limit).Take(limit);
        }
    }


    /// <summary>
    /// 模板类
    /// </summary>
    public class RemindTemplate
    {
        /// <summary>
        /// 搜索接口对象
        /// </summary>
        public readonly ISearch templateSearch;
        public readonly ISet TemplateSet;
        private readonly IDbAccessor omsAccessor;
        public RemindTemplate(IEnumerable<ISearch> search, IDbAccessor omsAccessor, ISet set)
        {
            this.templateSearch = search.Where(c => c is TemplateSearch).FirstOrDefault();
            this.omsAccessor = omsAccessor;
            this.TemplateSet = set;
        }

        /// <summary>
        /// 新增模板
        /// </summary>
        public virtual async Task<bool> TemplateAdd(object ts)
        {
            object t;
            if (ts is RemindTemplateModel)
                t = (RemindTemplateModel)ts;
            else t = ts as List<RemindTemplateModel>;
            if (t == null)
                return false;
            omsAccessor.OMSContext.AddRange(t);
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 新增模板
        /// </summary>
        public virtual async Task<bool> TemplateDtoAdd(object ts)
        {
            object t; object entity;
            if (ts is RemindTemplateDto)
            {
                t = (RemindTemplateDto)ts;
                entity = new RemindTemplateModel();
            }
            else { t = ts as List<RemindTemplateDto>; entity = new List<RemindTemplateModel>(); }
            if (t == null)
                return false;
            Mapper.Map(t, entity);
            omsAccessor.OMSContext.AddRange(entity);
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        public virtual async Task<bool> TemplateDelete(string templateCode)
        {
            var template = omsAccessor.Get<RemindTemplateModel>().Include(c => c.TemplateTitle).Include(c => c.UserMessages).Include(c => c.Product).Where(c => c.TemplateCode == templateCode).FirstOrDefault();
            template.Isdelete = true;
            template.RemindTitles.Select(c => c.Isdelete = true).ToList();
            template.UserMessages.Select(c => c.Isdelete = true).ToList();
            template.Product.Isdelete = true;
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 模板开关
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TemplateSwtich(templateSwtichDto swtich)
        {
            var tempalte = omsAccessor.Get<RemindTemplateModel>().Where(c => c.TemplateCode == swtich.TemplateCode).FirstOrDefault();
            if (tempalte == null)
                return await TemplateDtoAdd(new RemindTemplateDto() { Statu = swtich.Statu, SaleProductId = swtich.SaleProductId });
            tempalte.Statu = swtich.Statu;
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 获取模板详情
        /// </summary>
        public IQueryable<RemindTemplateModel> GetTemplateDetail(string templateCode) => omsAccessor.Get<RemindTemplateModel>().Where(c => c.TemplateCode == templateCode);



    }

}
