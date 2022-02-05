using System;
using System.IO;
using System.Linq;
using System.Net;
using EAVFW.Common;
using EAVFW.Common.MiddleWare;
using EAVFW.Models;
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

namespace EAVFW.Framework
{
    public class Startup
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

            services.AddScoped<DefaultQuerySettings>();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                foreach (var proxie in Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>()
                    ?.Select(IPAddress.Parse) ?? Array.Empty<IPAddress>())
                    options.KnownProxies.Add(proxie);

                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            });

            services.AddCors();

            services.AddSingleton<IMigrationManager, MigrationManager>();
            services.AddOptions<DynamicContextOptions>().Configure<IWebHostEnvironment>((o, environment) =>
            {
                o.Manifests = new[]
                    { JToken.Parse(File.ReadAllText($"{environment.ContentRootPath}/obj/manifest.g.json")) };
                o.PublisherPrefix = "phcms";
                o.EnableDynamicMigrations = true;
                o.Namespace = "EAVFW.Model";
                o.DTOAssembly = typeof(EAVFW.Models.Constants).Assembly;
            //  o.DTOBaseClasses = new[] { typeof(BaseOwnerEntity), typeof(BaseIdEntity) };
            });

            services.AddDbContext<DynamicContext>((sp, optionsBuilder) =>
            {
                var config = sp.GetService<IConfiguration>();
                var connStr = config.GetValue<string>("ConnectionStrings:ApplicationDb");
                var dbSchema = config.GetValue<string>("DBSchema");
                optionsBuilder.UseSqlServer(connStr ?? "empty",
                    x => x.MigrationsHistoryTable("__MigrationsHistory", dbSchema ?? "phcms").EnableRetryOnFailure()
                        .CommandTimeout(180));

                optionsBuilder.UseInternalServiceProvider(sp);
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            });

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
                        pb.AddAuthenticationSchemes("eavfw");
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
            app.Map("/_next",
                nested => nested.UseStaticFiles(new StaticFileOptions
                    { FileProvider = new PhysicalFileProvider(env.WebRootPath + "/_next") }));

            //The remaining is behind auth
            app.UseAuthentication();
            app.UseAuthorization();

            // app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseMiddleware<NextJsMiddleware>();
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