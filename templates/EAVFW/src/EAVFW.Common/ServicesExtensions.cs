using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.DependencyInjection;

namespace EAVFW.Common
{
    public static class ServicesExtensions
    {
        public static bool IsLocal(this IWebHostEnvironment env)
        {
            return env.IsEnvironment("local");
        }

        public static bool IsLocalOrDevelopment(this IWebHostEnvironment env)
        {
            return env.IsDevelopment() || env.IsLocal();
        }

        public static bool IsLocal(this IHostEnvironment env)
        {
            return env.IsEnvironment("local");
        }

        public static bool IsLocalOrDevelopment(this IHostEnvironment env)
        {
            return env.IsDevelopment() || env.IsLocal();
        }

        public static IServiceCollection AddSendGridEmailClient(this IServiceCollection services,
            string hostConfigName = "Smtp:Host",
            string hostPortConfigName = "Smtp:Port",
            string credentialConfigName = "Smtp:Password",
            string hostDefaultValue = "smtp.sendgrid.net",
            int hostPortDefaultValue = 587,
            string credentialDefaultValue = "",
            string credentialUserNameDefaultValue = "apikey"
        )
        {
            return services.AddSingleton(sp =>
            {
                var config = sp.GetService<IConfiguration>();
                return new SmtpClient(
                    config.GetValue<string>(hostConfigName, hostDefaultValue),
                    config.GetValue<int>(hostPortConfigName, hostPortDefaultValue))
                {
                    Credentials = new NetworkCredential(credentialUserNameDefaultValue,
                        config.GetValue<string>(credentialConfigName, credentialDefaultValue))
                };
            });
        }
    }
}