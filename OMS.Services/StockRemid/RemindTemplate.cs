using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OMS.Data.Domain;
using OMS.Data.Interface;
using OMS.Model.StockRemind;
using OMS.Services.StockRemid.StockRemindDto;
using SqlSugar;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IStockRemindNotify remindNotify;
        public TemplateSet(IDbAccessor omsAccessor, IStockRemindNotify remindNotify)
        {
            this.omsAccessor = omsAccessor;
            this.remindNotify = remindNotify;
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

            // 更新规则
            remindNotify.Rule(omsAccessor, dto);

            var _template = dto.Key.Select(c => c.TemplateCode).ToList();

            var templates = omsAccessor.Get<RemindTemplateModel>().Include(c => c.Product).Where(c => _template.Any(d => d == c.TemplateCode)).ToList();

            dto.Key.ForEach(k =>
            {
                var template = templates.Where(c => c.TemplateCode == k.TemplateCode).FirstOrDefault();
                var product = new RemindProductdto() { Name = k.Name, ProductCode = k.ProductCode, En = k.En, Price = k.Price, Stock = k.Stock };
                if (template == null)
                {
                    var list = new RemindTemplateDto() { TemplateTitle = string.IsNullOrEmpty(dto.TemplateTitle) ? k.Name + "库存不足{{RemindStock}},可用库存{{Stock}}" : dto.TemplateTitle, User = _user, SaleProductId = k.SaleProductId, Product = product, RemindStock = dto.RemindStock };
                    var entity = new RemindTemplateModel();
                    Mapper.Map(list, entity);
                    entity.Statu = true;
                    omsAccessor.OMSContext.RemindTemplate.Add(entity);
                }
                else
                {
                    template.TemplateTitle = string.IsNullOrEmpty(dto.TemplateTitle) ? k.Name + "库存不足{{RemindStock}},可用库存{{Stock}}" : dto.TemplateTitle;
                    template.User = _user;
                    template.Product = Mapper.Map(product, template.Product);
                    template.RemindStock = dto.RemindStock;
                    template.Statu = true;
                }
            });

            return omsAccessor.OMSContext.SaveChangesAsync().Result > 0;
        }

        /// <summary>
        /// 开关设置
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> Set(TemplateSaleDto dto)
        {
            var template = omsAccessor.Get<RemindTemplateModel>().Include(c => c.Product).Where(c => c.TemplateCode == dto.TemplateCode).FirstOrDefault();
            var product = new RemindProductdto() { Name = dto.Name, ProductCode = dto.ProductCode, En = dto.En, Price = dto.Price, Stock = dto.Stock };
            if (template == null)
            {
                var list = new RemindTemplateDto() { Product = product };
                var entity = new RemindTemplateModel();
                Mapper.Map(list, entity);
                omsAccessor.OMSContext.RemindTemplate.Add(entity);
            }
            else template.Product = Mapper.Map(product, template.Product);
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
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
            var search = ob as SearchProductDto; search.NameCode = search.NameCode?.Trim();
            if (search == null)
                return null;
            //查询字典
            var productType = omsAccessor.Get<Dictionary>().AsNoTracking().Where(c => c.Value == search.ProductType).Select(c => (int?)c.Id).FirstOrDefault();

            using (var sqlsugar = new SqlSugar(configuration))
            {
                var db = sqlsugar.db;
                count = 0;
                var data = db.Queryable<SaleProductWareHouseStock, SaleProductPrice, SaleProduct, Product>((w, spp, sp, p) => new object[]{
                JoinType.Inner,w.SaleProductId==spp.SaleProductId,
                JoinType.Inner,spp.SaleProductId==sp.Id,
                JoinType.Inner,sp.ProductId==p.Id,
                }).Where((w, spp, sp, p) => sp.Isvalid &&
                        ((search.MaxPrice == null || spp.Price <= search.MaxPrice) && (search.MinPrice == null || spp.Price >= search.MinPrice)) &&
                      (productType == null || p.Type == productType) && (string.IsNullOrEmpty(search.NameCode) || p.Name.Contains(search.NameCode) || p.NameEn.Contains(search.NameCode) || p.Code == search.NameCode)
                      ).GroupBy((w, spp, sp, p) => new { spp.SaleProductId, p.Name, p.NameEn, p.Code })
                      .Select((w, spp, sp, p) => new
                      {
                          SaleProductId = spp.SaleProductId,
                          Price = SqlFunc.AggregateAvg(spp.Price),
                          Stock = SqlFunc.AggregateSum(w.Stock - w.LockStock),
                          Name = p.Name,
                          En = p.NameEn,
                          ProductCode = p.Code
                      }).ToPageList(page, limit, ref count);

                var _template = omsAccessor.Get<RemindTemplateModel>().Select(c => new { c.TemplateCode, c.Statu, c.SaleProductId }).ToList();
                data.ForEach(c =>
                {
                    var template = _template.FirstOrDefault(d => d.SaleProductId == c.SaleProductId);

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
            var list = omsAccessor.Get<RemindTemplateModel>().AsNoTracking().Include(c => c.Product).Where(c => (string.IsNullOrEmpty(productCode) || c.Product.ProductCode == productCode) && c.Statu);
            count = list.Count();
            return list.Skip((page - 1) * limit).Take(limit);
        }

        /// <summary>
        ///  根据saleProductId 获取模板
        /// </summary>
        public IQueryable<RemindTemplateModel> Search(List<int> saleProductIds)
        {
            var list = omsAccessor.Get<RemindTemplateModel>().Where(c => saleProductIds.Any(d => d == c.SaleProductId)).Include(c => c.Product);
            return list;
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
        public RemindTemplate(IEnumerable<ISearch> search, IDbAccessor omsAccessor, IEnumerable<ISet> set)
        {
            this.templateSearch = search.Where(c => c is TemplateSearch).FirstOrDefault();
            this.omsAccessor = omsAccessor;
            this.TemplateSet = set.Where(c => c is TemplateSet).FirstOrDefault();
        }

        /// <summary>
        /// 新增模板
        /// </summary>
        public virtual async Task<bool> TemplateAdd(object ts)
        {
            var res = ExChange(ts);
            if (res == null)
                return false;
            omsAccessor.OMSContext.AddRange(res);
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Dto 转换成实体
        /// </summary>
        private object ExChange(object ob)
        {
            object t; object entity;
            if (ob is RemindTemplateDto)
            {
                t = (RemindTemplateDto)ob;
                entity = new RemindTemplateModel();
                Mapper.Map(t, entity);
                return entity;
            }
            else if (ob is List<RemindTemplateDto>)
            {
                t = (List<RemindTemplateDto>)ob;
                entity = new List<RemindTemplateModel>();
                Mapper.Map(t, entity);
                return entity;
            }
            else if (ob is RemindTemplateModel)
            {
                t = (RemindTemplateModel)ob;
                return t;
            }
            else t = ob as List<RemindTemplateModel>;
            return t;
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
        public bool TemplateSwtich(templateSwtichDto swtich, out string templateCode)
        {
            templateCode = null;
            var tempalte = omsAccessor.Get<RemindTemplateModel>().Where(c => c.TemplateCode == swtich.TemplateCode).FirstOrDefault();
            if (tempalte == null)
            {
                var flag = TemplateAdd(new RemindTemplateDto() { Statu = swtich.Statu, SaleProductId = swtich.SaleProductId }).Result;
                templateCode = omsAccessor.Get<RemindTemplateModel>().FirstOrDefault(c => c.SaleProductId == swtich.SaleProductId).TemplateCode;
                return flag;

            }
            tempalte.Statu = swtich.Statu;
            return omsAccessor.OMSContext.SaveChangesAsync().Result > 0;
        }

        /// <summary>
        /// 获取模板详情
        /// </summary>
        public IQueryable<RemindTemplateModel> GetTemplateDetail(string templateCode) => omsAccessor.Get<RemindTemplateModel>().Where(c => c.TemplateCode == templateCode);



    }


}
