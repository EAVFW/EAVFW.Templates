using Microsoft.AspNetCore.Hosting;
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
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });


     
    }
}