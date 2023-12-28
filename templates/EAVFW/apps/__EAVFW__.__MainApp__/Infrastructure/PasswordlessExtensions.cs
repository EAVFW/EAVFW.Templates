using EAVFramework;
using EAVFramework.Authentication.Passwordless;
using EAVFramework.Configuration;
using EAVFramework.Endpoints;
using __EAVFW__.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace __EAVFW__.__MainApp__.Infrastructure
{
    public static class PasswordlessExtensions
    {


        public static AuthenticatedEAVFrameworkBuilder AddPasswordless(this AuthenticatedEAVFrameworkBuilder auth, string subject = "Login til __EAVFW__ __MainApp__", string senderAppSetting = "SENDGRID_REPLY_ADDRESS")
        {
            return auth.AddAuthenticationProvider<PasswordlessEasyAuthProvider, PasswordlessEasyAuthOptions, IConfiguration>((options, config) =>
            {
                options.PersistTicketAsync = async (httpcontext, userid, handlid, data, redirect) =>
                {
                    var signinId = Guid.Parse(handlid);
                    var ctx = httpcontext.RequestServices.GetRequiredService<EAVDBContext<DynamicContext>>();
                    var signin = await ctx.Set<Signin>().FindAsync(signinId);

                    if (signin == null)
                    {
                        ctx.Context.Add(new Signin
                        {
                            Id = signinId,
                            Status = SigninStatus.Approved,
                            Properties = JsonConvert.SerializeObject(new object[] { new { type = "ticket", value = data }, new { type = "redirectUri", value = redirect } }),
                            Provider = "passwordless",
                            IdentityId = Guid.Parse(userid)
                        });

                        var result = await ctx.SaveChangesAsync(Common.Constants.SystemAdministratorGroup);


                    }


                };
                options.GetTicketInfoAsync = async (httpcontext, handleid) =>
                {

                    var signinId = Guid.Parse(handleid);
                    var ctx = httpcontext.RequestServices.GetRequiredService<EAVDBContext<DynamicContext>>();
                    var signin = await ctx.Set<Signin>().FindAsync(signinId);
                    var data = JToken.Parse(signin.Properties).ToDictionary(k => k.SelectToken("$.type")?.ToString(), v => v.SelectToken("$.value"));
                    return (data["ticket"].ToObject<byte[]>(), data["redirectUri"]?.ToString());
 
                };
                options.ResponseSuccessFullAsync = async (httpcontext) =>
                {
                    var webRootPath = httpcontext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
                    await httpcontext.Response.SendFileAsync($"{webRootPath}/account/login/index.html");
                };
                options.Subject = subject;
                options.Sender = config.GetValue<string>(senderAppSetting, "info@eavfw.com");
                options.TemplateMailMessageContents = url => $"<html><div>Klik på nedenstående link, for at komme ind i __EAVFW__ __MainApp__.</div><br/><a href=\"{url}\">__MainApp__</a><br/><br/>Venlig hilsen<br/><br/><b>EAVFW Teamet</b></br><a href=\"https://www.eavfw.com/\">https://www.eavfw.com/</a></html>";
                options.FetchUserIdByEmailAsync = async (ctx, sp, s) =>
                {
                    return await sp.GetRequiredService<DynamicContext>().Set<SystemUser>().Where(e => e.Email == s).Select(c => c.Id.ToString()).FirstOrDefaultAsync();
                };
            });
        }
    }
}
