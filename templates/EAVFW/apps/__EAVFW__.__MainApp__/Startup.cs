using EAVFramework;
using EAVFramework.Configuration;
using EAVFramework.Endpoints;
using EAVFramework.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EAVFW.Extensions.Documents;

using __EAVFW__.__MainApp__.Infrastructure;
using __EAVFW__.Common;
using __EAVFW__.Models;

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
            builder.AddAuthentication(options =>
            {
                options.FindIdentity = async (request) =>
                {
                    return await request.ServiceProvider.GetRequiredService<EAVDBContext<DynamicContext>>()
                        .Set<SystemUser>().Where(e => e.Email == request.Email).Select(c =>  c.Id).FirstOrDefaultAsync();
                };
                options.FindEmailFromIdentity = async (request) =>
                {
                    return await request.ServiceProvider.GetRequiredService<EAVDBContext<DynamicContext>>()
                          .Set<SystemUser>().Where(e => e.Id == request.IdentityId).Select(c => c.Email).FirstOrDefaultAsync();
                };
               
            })
#if (withSecurityModel)
            .WithSigninStore<DynamicContext,Signin>()
#else
         // .WithSigninStore<DynamicContext,Signin>() //Required EAVFW.Extensions.SecurityModel, dotnet eavfw-manifest install EAVFW.Extensions.SecurityModel
#endif
            .AddPasswordless();

            builder.WithMetrics();
            builder.AddDocumentHashPlugins<DynamicContext, Document>();


            //builder.AddWorkFlowEngine<DynamicContext, WorkflowRun>("1b714972-8d0a-4feb-b166-08d93c6ae328",
            //    (sp, c) => c, !(Configuration.GetValue<bool>("EAVFW_JOBSERVER_ENABLED", false)));


            return builder;
        }

        public async ValueTask OnAuthenticatedAsync(HttpContext http, ClaimsPrincipal claimsPrincipal, List<Claim> claims, string provider, string handleid)
        {

#if (withSecurityModel)
            if (!string.IsNullOrEmpty(handleid))
            {
                var ctx = http.RequestServices.GetRequiredService<EAVDBContext<DynamicContext>>();

                var settingCookies = http.Response.GetTypedHeaders().SetCookie;
                var cookie = settingCookies?.First(sc => sc.Name == $"eavfw")?.Value.ToString();


                var signin = await ctx.Context.FindAsync<Signin>(Guid.Parse(handleid));

                if (signin == null)
                {
                    ctx.Context.Add(new Signin
                    {
                        Id = Guid.Parse(handleid),
                        Status = SigninStatus.Used,
                        SessionId = cookie.Sha256(),
                        Claims = JsonConvert.SerializeObject(claims.Select(c => new { type = c.Type, value = c.Value })),
                        IdentityId = Guid.Parse(claimsPrincipal.FindFirstValue("sub")),
                        Provider = provider,
                    });
                }
                else
                {


                    ctx.Context.Entry(signin).State = EntityState.Modified;
                    signin.Status = SigninStatus.Used;
                    signin.SessionId = cookie.Sha256();
                    signin.Claims = JsonConvert.SerializeObject(claims.Select(c => new { type = c.Type, value = c.Value }));
                    signin.Provider ??= provider;

                }

                await ctx.SaveChangesAsync(__EAVFW__.Common.Constants.SystemAdministratorGroup);

            }
#endif
        }
        public async ValueTask PopulateAuthenticationClaimsAsync(HttpContext http, ClaimsPrincipal claimsPrincipal, List<Claim> claims, string provider, string handleid)
        {
#if (withSecurityModel)
            if (Guid.TryParse(claimsPrincipal.FindFirstValue("sub"), out var sub))
            {
                var ctx = http.RequestServices.GetRequiredService<DynamicContext>();
                var identity = await ctx.Set<Identity>().FindAsync(sub);
                var roleSet = from role in ctx.Set<SecurityRole>()
                              join roleassignment in ctx.Set<SecurityRoleAssignment>() on role.Id equals roleassignment.SecurityRoleId
                              where roleassignment.IdentityId == sub
                              select role.Name;
                var securityGroupsForIdentity =
                from securityGroup in ctx.Set<SecurityGroup>()
                join member in ctx.Set<SecurityGroupMember>() on securityGroup.Id equals member.SecurityGroupId
                where member.IdentityId == sub
                select securityGroup.Id;

                var permissionSetFromGroups = from role in ctx.Set<SecurityRole>()
                                              join roleassignment in ctx.Set<SecurityRoleAssignment>() on role.Id equals roleassignment.SecurityRoleId
                                              join securitygroup in securityGroupsForIdentity on roleassignment.IdentityId equals securitygroup
                                              select role.Name;

                var isActive = true;
                //TODO Do some logic for active users. User==Contact. Need a general model for "Is Active" and "UserEmail" interfaces
                //if (identity is User user)
                //{
                //    if (!string.IsNullOrEmpty(user.Email))
                //    {
                //        claims.Add(new Claim("email", user.Email));
                //    }

                //    isActive = user.Status == UserStatuses.Active;

                //    claims.Add(new Claim("status", ((int) (user.Status ?? UserStatuses.Inactive)).ToString()));
                //}

                if (isActive)
                {
                    var roles = await roleSet.Concat(permissionSetFromGroups).Distinct().ToListAsync();
                    claims.AddRange(roles.Select(k => new Claim("role", k)));

                }


                if (identity is SystemUser systemuser && !string.IsNullOrEmpty(systemuser.Email))
                    claims.Add(new Claim("email", systemuser.Email));


                claims.Add(new Claim("name", identity.Name));
            }


#endif

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
