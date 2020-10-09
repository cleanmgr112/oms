
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OMS.Services.ScheduleTasks;
using OMS.Services.StockRemid;

namespace OMS.Services
{
    public static class ServiceInjection
    {

        public static void Injection(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<Data.Interface.IDbAccessor, Data.Implementing.DbAccessor>();
            services.AddScoped<Services.Log.ILogService, Services.Log.LogService>();
            services.AddScoped<Services.Account.IUserService, Services.Account.UserService>();
            services.AddScoped<Services.Customer.ICustomerService, Services.Customer.CustomerService>();
            services.AddScoped<Services.Permissions.IMenuService, Services.Permissions.MenuService>();
            services.AddScoped<Services.Permissions.IRoleService, Services.Permissions.RoleService>();
            services.AddScoped<Services.Permissions.IUserRoleService, Services.Permissions.UserRoleService>();
            services.AddScoped<Services.Permissions.IPermissionService, Services.Permissions.PermissionService>();
            services.AddScoped<Services.Permissions.IRolePermissionService, Services.Permissions.RolePermissionService>();
            services.AddScoped<Services.Permissions.IUserPermissionService, Services.Permissions.UserPermissionService>();
            services.AddScoped<Services.Authentication.IAuthenticationService, Services.Authentication.FormsAuthenticationService>();
            services.AddScoped<Services.Deliveries.IDeliveriesService, Services.Deliveries.DeliveriesService>();
            services.AddScoped<Services.Common.ICommonService, Services.Common.CommonService>();
            services.AddScoped<Services.Order1.IOrderService, Services.Order1.OrderService>();
            services.AddScoped<Services.Order1.IOrderSyncService, Services.Order1.OrderSyncService>();
            services.AddScoped<Services.Products.IProductService, Services.Products.ProductService>();
            services.AddScoped<Services.IWareHouseService, Services.WareHouseService>();
            services.AddScoped<Services.IPurchasingService, Services.PurchasingService>();
            services.AddScoped<Services.Configuration.IScheduleTaskService, Services.Configuration.ScheduleTaskService>();
            services.AddScoped<Services.ScheduleTasks.IScheduleTaskFuncService, Services.ScheduleTasks.ScheduleTaskFuncService>();
            services.AddScoped<Services.ISalesManService, Services.SalesManService>();
            services.AddScoped<Core.IWebHelper, Core.WebHelper>();
            services.AddScoped<Services.K3Wise.IK3WiseService, Services.K3Wise.K3WiseService>();
            services.AddTransient<IScheduleTaskFuncService, ScheduleTaskFuncService>();
            services.AddTransient<IExecuteService, ExecuteService>();

            #region 库存提醒
            services.AddTransient<StockRemindService>();
            services.AddTransient<StockTitleService>();
            services.AddTransient<RemindTemplate>();
            services.AddTransient<ISearch,TemplateSearch>();
            services.AddTransient<ISet,TemplateSet>();
            services.AddTransient<ISearch,TitleSearch>();
            #endregion


        }
    }
}
