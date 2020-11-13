
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Interface;
using System.Collections.Generic;
using System.Linq;
using OMS.Model.StockRemind;
using OMS.Services.StockRemid.StockRemindDto;
using System.Threading.Tasks;
using OMS.Services.Common;
using Microsoft.Extensions.Configuration;
using OMS.Data.Implementing;

namespace OMS.Services.StockRemid
{
    /// <summary>
    /// 库存提醒服务层
    /// </summary>
    public class StockRemindService
    {
        private readonly IDbAccessor omsAccessor;
        private readonly User user;
        private readonly IDistributedCache cache;
        private readonly IHubContext<HubContext, IHubContext> hubContext;
        private readonly ICommonService common;
        private static string Mail = "support@mail.wine-world.cn";
        private readonly string conn;
        private readonly IStockRemindNotify remindNotify;
        public StockRemindService(IDbAccessor omsAccessor, IWorkContext context, IDistributedCache cache, IHubContext<HubContext, IHubContext> hubContext, ICommonService common, IStockRemindNotify remindNotify, IConfiguration configuration)
        {
            this.omsAccessor = omsAccessor;
            user = context.CurrentUser;
            this.cache = cache;
            this.hubContext = hubContext;
            this.common = common;
            this.remindNotify = remindNotify;
            this.conn = configuration["ConnectionStrings:DefaultConnection"];
        }



        /// <summary>
        /// 获取商品以及类型
        /// </summary>
        public List<string> GetProductType() => omsAccessor.Get<Dictionary>().Where(c => c.Type == DictionaryType.ProductType).Select(c => c.Value).ToList();


        /// <summary>
        /// 获取用户
        /// </summary>
        public object GetUser() => omsAccessor.Get<User>().Select(c => new { id = c.Id, userName = c.UserName }).ToList();



        /// <summary>
        /// 获取 模板标题and user
        /// </summary>
        /// <param name="templateCode">模板编号</param>
        /// <returns></returns>
        public TitleUserDto GetTitleAndUser(string templateCode)
        {
            var user = omsAccessor.Get<RemindTemplateModel>().Where(c => c.TemplateCode == templateCode).AsNoTracking().Select(c => new { User = c.User, TemplateTitle = c.TemplateTitle, RemindStock = c.RemindStock }).FirstOrDefault();

            if (user == null)
                return new TitleUserDto();
            TitleUserDto res = new TitleUserDto() { RemindStock = user.RemindStock, TemplateTitle = user.TemplateTitle };
            if (!string.IsNullOrEmpty(user?.User))
            {
                res.User = JsonConvert.DeserializeObject<List<UserDto>>(user.User);
            }

            return res;
        }

        /// <summary>
        /// 检验模板是否库存不足
        /// </summary>
        /// <returns></returns>
        public async Task TemplateValid()
        {
            if (cache.GetString(user.Id.ToString()) == null)
                return;

            var template = omsAccessor.Get<RemindTemplateModel>().Where(c => c.Statu && c.IsUpdate).Include(c => c.RemindTitles).Include(c => c.UserMessages).Include(c => c.Product).ToList();


            var list = new List<Task>();
            template.ForEach(c =>
               {
                    // 预警库存达到触发条件.
                    if (c.Product != null && c.RemindStock >= c.Product.Stock)
                   {
                        // 预警消息.
                        var str = c.TemplateTitle?.Replace("{{RemindStock}}", c.RemindStock.ToString()).Replace("{{Stock}}", c.Product.Stock.ToString());

                       list.Add(MaliSendAsync(JsonConvert.DeserializeObject<List<UserDto>>(c.User), "库存提醒", str));  // 发送邮件.

                        // 向在线的人发送消息.
                        var _user = SendMessage(c.User, str);
                       if (_user.Count != 0)
                       {
                            // 向有权限的人且离线的人记录消息.
                            c.UserMessages.Add(new UserMessageModel() { Message = str, User = JsonConvert.SerializeObject(_user) });
                       }
                        //生成标题
                        c.RemindTitles.Add(new RemindTitleModel() { RemindTitle = str });
                   }
               });

            await remindNotify.ReadRule(omsAccessor, template);
            Task.WaitAll(list.ToArray());

        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public List<UserDto> SendMessage(string userstr, string message)
        {

            var userDto = JsonConvert.DeserializeObject<List<UserDto>>(userstr).ToList();
            var userid = JsonConvert.DeserializeObject<List<string>>(cache.GetString("user"));
            var connection = new List<string>();

            // 向有权的用户发送消息.
            var union = userid.Intersect(userDto.Select(c => c.Id.ToString())).ToList();
            union.ForEach(c =>
            {
                // 移除已发送消息的用户.
                userDto.Remove(userDto.Where(d => d.Id.ToString() == c).FirstOrDefault());
                var conn = JsonConvert.DeserializeObject<List<string>>(cache.GetString(c));
                connection.AddRange(conn);
                // 发送邮件给授权人.
            });

            hubContext.Clients.Clients(connection).SendMessage("message", message);

            return userDto;
        }

        /// <summary>
        /// 登录时，是否有离线消息，有则发
        /// </summary>
        public async Task UserMesaage()
        {
            var connections = JsonConvert.DeserializeObject<List<string>>(cache.GetString(user.Id.ToString()));
            var userJson = JsonConvert.SerializeObject(new { Id = user.Id, UserName = user.UserName });
            omsAccessor.Get<UserMessageModel>().Where(c => c.User.Contains(userJson)).ToList().ForEach(c =>
           {
               hubContext.Clients.Clients(connections).SendMessage("message", c.Message);
               var _user = JsonConvert.DeserializeObject<List<UserDto>>(c.User);
               //移除发送消息用户
               _user.Remove(_user.Where(d => d.Id == user.Id).FirstOrDefault());
               if (_user.Count() == 0)
                   omsAccessor.OMSContext.UserMessage.Remove(c);
               else c.User = JsonConvert.SerializeObject(_user);
           });
            await omsAccessor.OMSContext.SaveChangesAsync();

        }

        /// <summary>
        /// 发送邮件给授权人
        /// </summary>
        public async Task<bool> MaliSendAsync(List<UserDto> user, string subject, string message)
        {
            var context = new OMSContext(new DbContextOptionsBuilder<OMSContext>().UseSqlServer(conn).Options);
            await Task.Delay(100);
            var UserId = user.Select(c => c.Id).ToList();
            //var entity = context.ChangeTracker.Entries<User>().Select(c=>c.Entity);
            var entity = context.Set<User>();
            var emails = entity.Where(c => UserId.Any(d => d ==c.Id)).Select(c => c.Email).ToList();
            var address = string.Join(",", emails);
            context.Dispose();
            return common.SendEmailByAliYun(subject, message, address, Mail, "红酒世界");
       
        }
    }
}
