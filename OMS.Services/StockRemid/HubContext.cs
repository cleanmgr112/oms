

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using OMS.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.Services.StockRemid
{

    public interface IHubContext
    {
        Task SendMessage(string title, string message);
    }
    public class HubContext : Hub<IHubContext>
    {
        private readonly string userId;
        private readonly IDistributedCache cache;
        public HubContext(IDistributedCache cache, IWorkContext context)
        {
            this.cache = cache;
            userId = context.CurrentUser.Id.ToString();
        }

        public override Task OnConnectedAsync()
        {
            Record(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        private void Record(string ConnectionId)
        {
            var userAll = cache.GetString("user");
            var list = new List<string>();
            if (userAll == null)
            {
                cache.SetString("user", JsonConvert.SerializeObject(new List<string>() { userId }));
                cache.SetString(userId, JsonConvert.SerializeObject(new List<string>() { ConnectionId }));
            }
            else
            {
                list = JsonConvert.DeserializeObject<List<string>>(cache.GetString("user"));
                if (!list.Any(c => c == userId))
                {
                    list.Add(userId);
                    cache.SetString(userId, JsonConvert.SerializeObject(new List<string>() { ConnectionId }));
                    cache.SetString("user", JsonConvert.SerializeObject(list));

                }
                else
                {
                    var user = JsonConvert.DeserializeObject<List<string>>(cache.GetString(userId));
                    user.Add(ConnectionId);
                    cache.SetString(userId, JsonConvert.SerializeObject(user));
                }
            }
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            RemoveRecord(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }


        private void RemoveRecord(string ConnectionId)
        {
            var connections = JsonConvert.DeserializeObject<List<string>>(cache.GetString(userId));
            if (connections.Remove(ConnectionId))
                cache.SetString(userId, JsonConvert.SerializeObject(connections));
        }
    }
}
