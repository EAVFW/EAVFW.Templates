using System;
using System.IO;
using System.Linq;
using System.Net;
using __EAVFW__.Common;
using __EAVFW__.Models;
using DotNetDevOps.Extensions.EAVFramework;
using DotNetDevOps.Extensions.EAVFramework.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.OData.ModelBuilder.Config;
using Newtonsoft.Json.Linq;
using EAVFW.Extensions.Infrastructure;
using Microsoft.Extensions.Hosting;
using EAVFW.Extensions.Infrastructure.TypeHelpers;
using DotNetDevOps.Extensions.EAVFramework.Configuration;
using __EAVFW__.BusinessLogic;
#if (withSecurityModel)
using EAVFW.Extensions.SecurityModel;
#endif

namespace __EAVFW__.__MainApp__
{
    public partial class Startup
    {
        private static IWebHostEnvironment AppEnvironment { get; set; }
        private static IConfiguration Configuration { get; set; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            AppEnvironment = env;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHealthChecks();
            services.AddCors();

            services.ConfigureForwardedHeadersOptions();

            CustomConfigureServices(services);


            services.AddOptions<DynamicContextOptions>().Configure<IWebHostEnvironment>((o, environment) =>
            {

                o.Manifests = new[]{environment.IsLocal() &&
                      File.Exists($"{environment.ContentRootPath}/../../src/__EAVFW__.Models/obj/manifest.g.json")
                          ? JToken.Parse(File.ReadAllText(
                              $"{environment.ContentRootPath}/../../src/__EAVFW__.Models/obj/manifest.g.json"))
                          : JToken.Parse(File.ReadAllText($"{environment.ContentRootPath}/manifest.g.json"))
                };


                o.PublisherPrefix = "__EAVFW__";
                o.EnableDynamicMigrations = true;
                o.Namespace = "__EAVFW__.Models";
                o.DTOAssembly = typeof(__EAVFW__.Models.Constants).Assembly;
               
                o.DTOBaseClasses = new Type[] {
                #if(withSecurityModel)
                     typeof(BaseOwnerEntity<Identity>), 
                     typeof(BaseIdEntity<Identity>) 
                #endif
                };
                
             
            });

            services.AddDbContext<DynamicContext>((sp, optionsBuilder) =>
            {
                var config = sp.GetService<IConfiguration>();
                var connStr = config.GetValue<string>("ConnectionStrings:ApplicationDb");
                var dbSchema = config.GetValue<string>("DBSchema");
                optionsBuilder.UseSqlServer(connStr ?? "empty",
                    x => x.MigrationsHistoryTable("__MigrationsHistory", dbSchema ?? "__EAVFW__").EnableRetryOnFailure()
                        .CommandTimeout(180));

                optionsBuilder.UseInternalServiceProvider(sp);
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            });

                       
            services.AddEAVFramework<DynamicContext>(o => { o.RoutePrefix = "/api"; })
                .WithPluginsDiscovery<PluginConfiguration>()
                .WithDatabaseHealthCheck<DynamicContext>(); 

            services.AddAuthorization(options =>
            {
                options.AddPolicy("EAVAuthorizationPolicy", pb =>
                {
                    pb.AddAuthenticationSchemes("EasyAuth");
                    pb.RequireAuthenticatedUser();
                });

                options.AddPolicy("HangfirePolicyName", pb =>
                {
                    if (AppEnvironment.IsLocal())
                    {
                        pb.RequireAssertion(c => true);
                    }
                    else
                    {
                        pb.AddAuthenticationSchemes(DotNetDevOps.Extensions.EAVFramework.Constants.DefaultCookieAuthenticationScheme);
                        pb.RequireAuthenticatedUser();
                        pb.RequireClaim("role", "System Administrator");
                    }
                });
            });

            services.AddAuthentication();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logger)
        {
            // app.UseRequestLoggingMiddleware();

            if (env.IsLocalOrDevelopment())
            {
                app.UseCors(o =>
                    o.WithOrigins("https://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials());
                app.UseForwardedHeaders();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseForwardedHeaders();
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // Allow unauthaccess to /_next folder.
            // Because it is located before .UseAuthorization() 
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-5.0#static-file-authorization
            if(Directory.Exists("_next"))
                app.Map("/_next",
                    nested => nested.UseStaticFiles(new StaticFileOptions
                        { FileProvider = new PhysicalFileProvider(env.WebRootPath + "/_next") }));

            //The remaining is behind auth
            app.UseAuthentication();
            app.UseAuthorization();

            // app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseMiddleware<NextJSMiddleware>();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseEndpoints(config =>
            {
                config.MapHealthChecks("/.well-known/live", new HealthCheckOptions { Predicate = _ => false })
                    .WithMetadata(new AllowAnonymousAttribute());
                config.MapHealthChecks("/.well-known/ready").WithMetadata(new AllowAnonymousAttribute());
                config.MapEAVFrameworkRoutes();
            });
        }
    }
}