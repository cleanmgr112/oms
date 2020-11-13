
using OMS.Data.Interface;
using OMS.Model.StockRemind;
using OMS.Services.StockRemid.StockRemindDto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.Services.StockRemid
{

    /// <summary>
    ///  库存提醒，权限用户接受消息规则(同样的消息，只接受一次，不同样的消息，可接收)
    /// </summary>
    public abstract class IStockRemindNotify
    {
        // 接受规则
        public abstract void Rule(IDbAccessor omsAccessor, SetDto set);

        // 当没有设置模板时，例如重新刷新页面（视为 isupdate=false 没有更新）
        public virtual async Task<bool> ReadRule(IDbAccessor omsAccessor, List<RemindTemplateModel> template)
        {
            template.Select(c => c.IsUpdate = false).ToList();
            return await omsAccessor.OMSContext.SaveChangesAsync() > 0;
        }
    }


    /// <summary>
    /// 当库存或者标题改变时，权限用户可接受
    /// </summary>
    public class StockRemindNotify : IStockRemindNotify
    {
        public override void Rule(IDbAccessor omsAccessor, SetDto set)
        {
            var templateCodes = set.Key.Select(c => c.TemplateCode).ToList();
            var templateCode = omsAccessor.Get<RemindTemplateModel>().Where(c => templateCodes.Any(d => d == c.TemplateCode)).ToList();

            templateCode.ForEach(c =>
            {
                // 标题/提醒库存 不同时，视为更新
                if (c.TemplateTitle != set.TemplateTitle || c.RemindStock != set.RemindStock)
                    c.IsUpdate = true;
            });
        }
    }
}
