using System.Security.Claims;

namespace __EAVFW__.Common
{
    public static class Constants
    {
        public const string Subject = "sub";
       

        public static ClaimsPrincipal SystemAdministratorGroup = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                   new Claim(Subject,"1b714972-8d0a-4feb-b166-08d93c6ae328")
                                }, EAVFramework.Constants.DefaultCookieAuthenticationScheme));
    }
}
