using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace __EAVFW__.__MainApp__
{
    public class __EAVFW__App
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
             .ConfigureServices(services =>
             {
                 services.Configure<HostOptions>(hostOptions =>
                 {
                     hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;

                 });

             })
            .ConfigureAppConfiguration((hostingContext, config) => {
                if (hostingContext.HostingEnvironment.IsLocal())
                {
                    config.AddUserSecrets<Startup>();
                }
            })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });



    }
}