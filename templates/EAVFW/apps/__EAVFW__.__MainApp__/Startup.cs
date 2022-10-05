using EAVFramework;
using EAVFramework.Configuration;


using __EAVFW__.Common;
using __EAVFW__.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;

using __EAVFW__.__MainApp__.Infrastructure;
//using EAVFW.Extensions.WorkflowEngine;
//using Hangfire;
//using Hangfire.Dashboard;
//using __EAVFW__.OpenIdConnect;
namespace __EAVFW__.__MainApp__
{
    /// <summary>
    /// The custom startup class for your EAV Project.
    /// Be aware of the startup.g.cs file that is partial to this that will be updated in future updates
    /// </summary>
    public partial class Startup
    {
        public const string HangfirePolicyName = "HangfirePolicyName";
        public const string HangfireDashboardPath = "/.well-known/jobs";

        public const string EAVAuthorizationPolicy = "EAVAuthorizationPolicy";


        public void CustomConfigureServices(IServiceCollection services)
        {

            services.AddSendGridEmailClient();
            // services.AddOpenIdConnect<DynamicContext>(); /* OpendIDConnecti extension */

            // services.AddDynamicManifest<DataModelProject,Document>(); /* Dynamic Manifest Extension*/
        }

        private IEAVFrameworkBuilder ConfigureEAVFW(IEAVFrameworkBuilder builder)
        {
            builder.AddAuthentication()
                .AddPasswordless();



            //Workflow extension
            // builder.AddWorkFlowEngine<DynamicContext, WorkflowRun>("1b714972-8d0a-4feb-b166-08d93c6ae328");
            //builder.Services.AddOptions<EAVFWOutputsRepositoryOptions>()
            //    .Configure(c =>
            //    {
            //        c.IdenttyId = "1b714972-8d0a-4feb-b166-08d93c6ae328";
            //    });


            return builder;
        }

        public void ConfigureHangfirePolicy(AuthorizationOptions options, IHostEnvironment environment)
        {
            options.AddPolicy(HangfirePolicyName, pb =>
            {
                if (environment.IsLocal())
                {
                    pb.RequireAssertion(c => true);
                }
                else
                {
                    pb.AddAuthenticationSchemes(EAVFramework.Constants.DefaultCookieAuthenticationScheme);
                    pb.RequireAuthenticatedUser();
                    pb.RequireClaim("role", "System Administrator");
                }
            });
        }

        /// <summary>
        /// Configure the authorization policy that is used for authenticating the EAV System
        /// </summary>
        /// <param name="options"></param>
        /// <param name="environment"></param>
        private void ConfigureEAVAuthorizationPolicy(AuthorizationOptions options, IHostEnvironment environment)
        {
            options.AddPolicy(EAVAuthorizationPolicy, pb =>
            {
                pb.AddAuthenticationSchemes("eavfw");
             //   pb.AddAuthenticationSchemes("eavfw", "Bearer");
                pb.RequireAuthenticatedUser();
            });
        }


        /// <summary>
        /// The fallback authorization used when no other authorization handlers are meet.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        private AuthorizationPolicy CreateFallbackAuthorization(IHostEnvironment environment)
        {
            return new AuthorizationPolicyBuilder()
              .AddAuthenticationSchemes("eavfw")
              .RequireAuthenticatedUser().Build();
        }


        /// <summary>
        /// Map Hangfire Dashboard
        /// </summary>
        /// <param name="endpoints"></param>
        private void MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            //endpoints.MapHangfireDashboard(HangfireDashboardPath, new DashboardOptions
            //{
            //    Authorization = new List<IDashboardAuthorizationFilter> { }
            //}).RequireAuthorization(HangfirePolicyName);

          
          //  endpoints.MapWorkFlowEndpoints<DynamicContext>(true, true);

        }
    }
}
