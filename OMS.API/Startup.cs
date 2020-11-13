using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using NLog.Web;
using OMS.Model;
using OMS.Services.K3Wise;
using OMS.Services.StockRemid;

namespace OMS.API
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //注入配置
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            //返回json日期格式化
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm";
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1); 
            //数据库
            services.AddDbContext<Data.Implementing.OMSContext>(options =>
                   options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"], r => r.UseRowNumberForPaging()));
            //cookie
            services.AddAuthentication(options => options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            //service
            services.AddScoped<Data.Interface.IDbAccessor, Data.Implementing.DbAccessor>();
            services.AddScoped<Core.IWorkContext, WebCore.WebWorkContext>();
            services.AddScoped<Services.Log.ILogService, Services.Log.LogService>();
            services.AddScoped<Services.Account.IUserService, Services.Account.UserService>();
            services.AddScoped<Services.Customer.ICustomerService, Services.Customer.CustomerService>();
            services.AddScoped<Services.Permissions.IMenuService, Services.Permissions.MenuService>();
            services.AddScoped<Services.Permissions.IRoleService, Services.Permissions.RoleService>();
            services.AddScoped<Services.Permissions.IUserRoleService, Services.Permissions.UserRoleService>();
            services.AddScoped<Services.Permissions.IPermissionService, Services.Permissions.PermissionService>();
            services.AddScoped<Services.Permissions.IRolePermissionService, Services.Permissions.RolePermissionService>();
            services.AddScoped<Services.Permissions.IUserPermissionService, Services.Permissions.UserPermissionService>();
            services.AddScoped<Services.Order1.IOrderService, Services.Order1.OrderService>();
            services.AddScoped<Services.Order1.IOrderSyncService, Services.Order1.OrderSyncService>();
            services.AddScoped<Services.Authentication.IAuthenticationService, Services.Authentication.FormsAuthenticationService>();
            services.AddScoped<Services.Deliveries.IDeliveriesService, Services.Deliveries.DeliveriesService>();
            services.AddScoped<Services.Common.ICommonService, Services.Common.CommonService>();
            services.AddScoped<Services.Order1.IOrderService, Services.Order1.OrderService>();
            services.AddScoped<Services.Products.IProductService, Services.Products.ProductService>();
            services.AddScoped<Services.IWareHouseService, Services.WareHouseService>();
            services.AddScoped<Services.IPurchasingService, Services.PurchasingService>();
            services.AddScoped<Services.Configuration.IScheduleTaskService, Services.Configuration.ScheduleTaskService>();
            services.AddScoped<Services.ScheduleTasks.IScheduleTaskFuncService, Services.ScheduleTasks.ScheduleTaskFuncService>();
            services.AddScoped<Services.ISalesManService, Services.SalesManService>();
            services.AddScoped<Core.IWebHelper, Core.WebHelper>();
            services.AddScoped<Services.K3Wise.IK3WiseService, Services.K3Wise.K3WiseService>();
            services.AddScoped<Services.CMB.ICMBService, Services.CMB.CMBService>();
            services.AddScoped<Services.Products.ISaleProductWareHouseStockService, Services.Products.SaleProductWareHouseStockService>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    //NameClaimType = JwtClaimTypes.Name,
                    //RoleClaimType = JwtClaimTypes.Role,

                    ValidIssuer = "wine-world.com",
                    ValidAudience = "wine-world.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:SecurityKey"])),

                    /***********************************TokenValidationParameters的参数默认值***********************************/
                    // RequireSignedTokens = true,
                    // SaveSigninToken = false,
                    // ValidateActor = false,
                    // 将下面两个参数设置为false，可以不验证Issuer和Audience，但是不建议这样做。
                    // ValidateAudience = true,
                    // ValidateIssuer = true, 
                    // ValidateIssuerSigningKey = false,
                    // 是否要求Token的Claims中必须包含Expires
                    RequireExpirationTime = true,
                    // 允许的服务器时间偏移量
                    ClockSkew = TimeSpan.FromSeconds(300),
                    // 是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    ValidateLifetime = true
                };
            });
            //session
            services.AddSession();

            //#region Hangfire
            //services.AddHangfire(x => x.UseSqlServerStorage(Configuration["ConnectionStrings:OMSKingdee"]));
            //services.AddHangfireServer();
            //#endregion

            #region Swagger
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v0.1.0",
                    Title = "OMS.API",
                    Description = "WMSAPI说明文档",
                    Contact = new OpenApiContact { Name = "OMS.API", Email = "" }
                });

                //根目录
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "OMS.API.xml");
                c.IncludeXmlComments(xmlPath, true);

            });
            services.AddSignalR();
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
            
            app.UseCookiePolicy();

            loggerFactory.AddNLog();
            env.ConfigureNLog("nlog.config");


            #region Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiHelp V1");
                c.RoutePrefix = "";
            });
            #endregion


            //#region Hangfire
            //app.UseHangfireDashboard();
            ////RecurringJob.AddOrUpdate(() => Console.WriteLine("Hangfire Work!!!!"), Cron.Minutely);
            //RecurringJob.AddOrUpdate<K3WiseService>(t => t.AddOrdersToK3(), Cron.Daily());
            //#endregion
            app.UseSignalR(routes =>
            {
                routes.MapHub<HubContext>("/chathub");
            });
        }
    }
}
