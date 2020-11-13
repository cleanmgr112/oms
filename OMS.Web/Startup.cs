using OMS.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System;
using Hangfire;
using OMS.Services.ScheduleTasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Net;
using System.IO;
using OMS.Services;
using OMS.Services.StockRemid;
using Microsoft.AspNetCore.Http.Features;

namespace OMS.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("sitemap.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how t    o configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => false;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = 40000; // 5000 items max
                options.ValueLengthLimit = 1024 * 1024 * 10; // 100MB max len form data
            });


            //注入配置
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<SiteMap>(Configuration.GetSection("SiteMap"));
            //返回json日期格式化
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm";
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            //数据库
            services.AddDbContext<Data.Implementing.OMSContext>(options =>
                   options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"], r => r.MigrationsAssembly("OMS.Web").UseRowNumberForPaging()));
            //cookie
            services.AddAuthentication(options => options.DefaultScheme = "OMSCookies").AddCookie("OMSCookies", o =>
            {
                o.LoginPath = new PathString("/User/Login");
                o.AccessDeniedPath = new PathString("/Home/PageError");
            });

            //service注入
            services.Injection();
            services.AddScoped<Core.IWorkContext, WebCore.WebWorkContext>();
            services.AddScoped<Services.CMB.ICMBService, Services.CMB.CMBService>();

            //定时任务
            //services.AddTimedJob();

            //session
            services.AddSession();

            //Hangfire定时任务
            services.AddHangfire(r => r.UseSqlServerStorage(Configuration["ConnectionStrings:DefaultConnection"]));

            //本地内存缓存
            services.AddMemoryCache();
            services.AddSignalR();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider, IApplicationLifetime appLife)
        {
            //nlog
            env.ConfigureNLog("nlog.config");
            loggerFactory.AddNLog();
            app.AddNLogWeb();
            LogManager.Configuration.Variables["connectionString"] = Configuration["ConnectionStrings:DefaultConnection"];

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/PageError");
                app.UseHsts();
            }
            //IIS回收关闭之后重新请求保证定时任务持续进行
            appLife.ApplicationStopped.Register(() =>
            {
                string endTime = DateTime.Now.ToString();
                OMS.Core.Tools.Logger.Info("应用资源回收：" + endTime);
                //解决IIS应用程序自动回收的问题
                Thread.Sleep(5000);
                string url = Configuration["OMS:OMSUrl"];
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream stream = httpWebResponse.GetResponseStream();//得到会写字节流
            });
            //定时任务
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            RecurringJob.AddOrUpdate(() => Console.WriteLine("Simple!"), Cron.Yearly(1));
            RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.SendMessage(), Cron.Yearly());
            RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.ReceviceMessage(), Cron.Yearly());
            RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.Test(), "*/5 * * * *");
            //RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.OmsSyncDictionaries(), Cron.Minutely);
            RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.OmsSyncProducts(), Cron.Yearly());
            //RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.OmsSyncProducts(), Cron.Yearly());
            RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.OmsSyncWineWorldOrder(), "*/15 * * * *");
            RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.SyncCMBOrderToOMS(), "*/30 * * * *");

            RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.SyncRfidToWineCabinet(), "*/30 * * * *");
            //RecurringJob.AddOrUpdate<ScheduleTaskFuncService>(a => a.OmsSyncWineWorldOrder(), Cron.Yearly(10));
            app.UseAuthentication();
            app.UseSession();
            //路由
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "permissionRole",
                    template: "{controller=Permission}/{action=Role}/{id}");

                routes.MapRoute(
                    name: "permissionUser",
                    template: "{controller=Permission}/{action=User}/{id}");

                routes.MapRoute(
                    name: "UserRole",
                    template: "{controller=User}/{action=UserRole}/{id}");
            });
            app.UseHttpsRedirection();
            //初始化map
            WebCore.AutoMapperInit.Init();
            //读取静态文件，默认wwwroot文件夹
            app.UseStaticFiles();
            //app.UseCookiePolicy();

            app.UseSignalR(routes =>
            {
                routes.MapHub<HubContext>("/chathub");
            });
        }
    }
}
