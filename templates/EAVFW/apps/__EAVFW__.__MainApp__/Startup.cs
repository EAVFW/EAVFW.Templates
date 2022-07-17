using DotNetDevOps.Extensions.EAVFramework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace __EAVFW__.__MainApp__
{
    /// <summary>
    /// The custom startup class for your EAV Project.
    /// Be aware of the startup.g.cs file that is partial to this that will be updated in future updates
    /// </summary>
    public partial class Startup
    {
        public void CustomConfigureServices(IServiceCollection services)
        {

            services.AddEmailClient();
        }

        private IEAVFrameworkBuilder ConfigureEAVFW(IEAVFrameworkBuilder builder)
        {
            builder.AddAuthentication()
                .AddPasswordless();

            return builder;
        }
    }
}
