using EAVFramework.Authentication.Passwordless;
using EAVFramework.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace __EAVFW__.__MainApp__.Infrastructure
{
    public static class PasswordlessExtensions
    {
        public static AuthenticatedEAVFrameworkBuilder AddPasswordless(this AuthenticatedEAVFrameworkBuilder auth, string subject = "Login til __EAVFW__ __MainApp__", string senderAppSetting = "SENDGRID_REPLY_ADDRESS")
        {
            return auth.AddAuthenticationProvider<PasswordlessEasyAuthProvider, PasswordlessEasyAuthOptions, IConfiguration>((options, config) =>
            { 
                options.ResponseSuccessFullAsync = async (httpcontext) =>
                {
                    var webRootPath = httpcontext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
                    await httpcontext.Response.SendFileAsync($"{webRootPath}/account/login/index.html");
                };
                options.Subject = subject;
                options.Sender = config.GetValue<string>(senderAppSetting, "info@eavfw.com");
                options.TemplateMailMessageContents = url => $"<html><div>Klik på nedenstående link, for at komme ind på selve __EAVFW__ __MainApp__.</div><br/><a href=\"{url}\">__EAVFW__ __MainApp__</a><br/><br/>Venlig hilsen<br/><br/><b>EAVFW Teamet</b></br><a href=\"https://www.eavfw.com/\">https://www.eavfw.com/</a></html>";
                
            });
        }
    }
}
