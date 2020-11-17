
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using OMS.Data.Domain;
using OMS.Data.Interface;
using OMS.Model.StockRemind;
using OMS.Services.StockRemid.StockRemindDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.Services.StockRemid
{
    /// <summary>
    /// 标题搜索
    /// </summary>
    public class TitleSearch : ISearch
    {
        private readonly IDbAccessor omsAccessor;

        public TitleSearch(IDbAccessor omsAccessor)
        {
            this.omsAccessor = omsAccessor;
        }

        /// <summary>
        /// SearchDto 标题条件搜索
        /// </summary>
        public object Search(object ob, out int count, int page = 1, int limit = 5)
        {
            count = 0;
            var search = ob as SearchDto;
            if (search == null)
                return null;
            var list = omsAccessor.Get<RemindTitleModel>().AsNoTracking().Where(c =>
           (search.Min == null || search.Min < c.CreateTime) && (search.Max == null || search.Max > c.CreateTime));
            count = list.Count();
            return Mapper.Map<List<RemindTitleDto>>(list.Skip((page - 1) * limit).Take(limit).ToList());
        }
    }

    /// <summary>
    ///  提醒标题service
    /// </summary>
    public class StockTitleService
    {
        private readonly IDbAccessor omsAccessor;
        public readonly IEnumerable<ISearch> ISearch;
        private readonly IHostingEnvironment environment;
        public StockTitleService(IDbAccessor omsAccessor, IHostingEnvironment environment, IEnumerable<ISearch> search)
        {
            this.omsAccessor = omsAccessor;
            this.environment = environment;
            ISearch = search;
        }


        /// <summary>
        /// 获取已经预警的模板
        /// </summary>
        public  List<TitleSearchDto> GetTemplate(out int count, string productCode = null, int page = 1, int limit = 10)
        {
            var list = ((TemplateSearch)ISearch.FirstOrDefault(c => c is TemplateSearch)).Search(productCode, out count, page, limit).ToList();
            var flag = SynStock(list).Result;
            return Mapper.Map<List<TitleSearchDto>>(list);
        }

        /// <summary>
        /// 条件搜索(模板和标题混合搜索)
        /// </summary>
        public object Search(DateTime? min, DateTime? max, string productCode)
        {
            return new { template = GetTemplate(out int count1, productCode), title = ((TitleSearch)ISearch.FirstOrDefault(c => c is TitleSearch)).Search(new SearchDto() { Min = min, Max = max }, out int count2), titleCount = count2, templateCount = count1 };
        }

        /// <summary>
        /// 修改状态
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Statu(string titleId)
        {
            var title = omsAccessor.Get<RemindTitleModel>().Where(c => c.TitleId == titleId).FirstOrDefault();
            if (title == null)
                return false;
            title.IsRead = true;
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 取消模板预警
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Cancel(string templateCode)
        {

            var template = omsAccessor.Get<RemindTemplateModel>().Where(c => c.TemplateCode == templateCode).FirstOrDefault();
            template.Statu = false;
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// 导出excel
        /// </summary>
        /// <returns></returns>
        public void Excel(List<TitleSearchDto> data)
        {
            var path = $"{environment.WebRootPath}\\files\\RemindStock.xlsx";

            string[][] arr = new string[data.Count][]; int top = 0;
            data.ForEach(c =>
            {
                arr[top++] = new string[] { top.ToString(), c.Product.ProductCode, c.Product.Name, c.Product.En, c.Product.Price.ToString(), c.Product.Stock.ToString(), "已预警" };
            });

            ExcelHelper.Export(path, arr, $"{environment.WebRootPath}\\files\\导出.xlsx");
        }

        /// <summary>
        /// 同步库存
        /// </summary>
       public async Task<bool> SynStock(List<RemindTemplateModel> templates)
        {
            var list = templates.Select(c => c.SaleProductId).ToList();
           var saleproduct= omsAccessor.Get<SaleProductWareHouseStock>().Where(c => list.Any(d => d == c.SaleProductId)).GroupBy(c=>c.SaleProductId).Select(c=>new {saleproductId= c.Key,stock=c.Sum(d=>d.Stock-d.LockStock)}).ToList();
            saleproduct.ForEach(c => {
                templates.FirstOrDefault(d => d.SaleProductId == c.saleproductId).Product.Stock = c.stock;
            });
            return await omsAccessor.OMSContext.SaveChangesAsync() >= 0;
        }
    }
}
