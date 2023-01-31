using System;
using System.IO;
using System.Linq;
using System.Net;
using __EAVFW__.Common;
using __EAVFW__.Models;
using EAVFramework;
using EAVFramework.Hosting;
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
using EAVFramework.Configuration;
using __EAVFW__.BusinessLogic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Options;
using Microsoft.OData.UriParser;

#if (withSecurityModel)
using EAVFW.Extensions.SecurityModel;
#endif

namespace __EAVFW__.__MainApp__
{
    public partial class Startup
    {
        private static IWebHostEnvironment AppEnvironment { get; set; }
        private static IConfiguration Configuration { get; set; }


        static Startup()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc,
                DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTimeOffset,
            };
          //  JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }
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

            services.AddScoped(sp =>
            {
                var o = new ODataOptions();
                o.AddRouteComponents("/api/", sp.GetRequiredService<IMigrationManager>().Model, services => services.AddSingleton<ODataUriResolver>());
                return Options.Create(o);
            });

            services.AddOptions<DynamicContextOptions>().Configure<IWebHostEnvironment>((o, environment) =>
            {

                o.Manifests = new[]{environment.IsLocal() &&
                      File.Exists($"{environment.ContentRootPath}/../../src/__EAVFW__.Models/obj/manifest.g.json")
                          ? JToken.Parse(File.ReadAllText(
                              $"{environment.ContentRootPath}/../../src/__EAVFW__.Models/obj/manifest.g.json"))
                          : JToken.Parse(File.ReadAllText($"{environment.ContentRootPath}/manifest.g.json"))
                };


                o.Schema = "__databaseSchema__";
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
                    x => x.MigrationsHistoryTable("__MigrationsHistory", dbSchema ?? "__databaseSchema__").EnableRetryOnFailure()
                        .CommandTimeout(180));

                optionsBuilder.UseInternalServiceProvider(sp);
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            });


            var eav = ConfigureEAVFW(services.AddEAVFramework<DynamicContext>(o => {
                o.RoutePrefix = "/api";

                o.Authentication.OnAuthenticatedAsync = OnAuthenticatedAsync;
                o.Authentication.PopulateAuthenticationClaimsAsync = PopulateAuthenticationClaimsAsync;

            }) 
                .WithPluginsDiscovery<PluginConfiguration>()
                .WithDatabaseHealthCheck<DynamicContext>());

#if (withSecurityModel)
            eav.WithAuditFieldsPlugins<DynamicContext, Identity>()
                .WithPermissionBasedAuthorization<DynamicContext, Identity, Permission, SecurityRole, SecurityRolePermission, SecurityRoleAssignment, SecurityGroup, SecurityGroupMember, RecordShare>();
#endif

            services.AddOptions<AuthorizationOptions>().Configure<IHostEnvironment>((options, environment) =>
            {
                ConfigureEAVAuthorizationPolicy(options, environment);
                ConfigureHangfirePolicy(options, environment);
                options.FallbackPolicy = CreateFallbackAuthorization(environment);
            });
            services.AddAuthorization();

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
            if (Directory.Exists(env.WebRootPath + "/_next"))
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
                config.MapEAVFrameworkRoutes<DynamicContext>();

                MapEndpoints(config);

            });
        }
    }
}