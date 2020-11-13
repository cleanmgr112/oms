using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace OMS.Web
{
    public class Program
    {
        //public static void Main(string[] args)
        //{
        //    BuildWebHost(args).Run();
        //}

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder()
                .UseKestrel(c =>
                {
                    c.Limits.MaxRequestBodySize =null;
                }).UseStartup<Startup>();
    }
}
