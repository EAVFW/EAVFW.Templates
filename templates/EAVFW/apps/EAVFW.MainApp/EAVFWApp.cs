using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EAVFW.Framework
{
    public class EAVFWApp
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