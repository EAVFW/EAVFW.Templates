using System.Security.Claims;

namespace EAVFW.Common
{
    public static class Constants
    {
        public const string Subject = "sub";
        public const string EAVFW = "eavfw";

        public static ClaimsPrincipal SystemAdministratorGroup = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                   new Claim(Subject,"1b714972-8d0a-4feb-b166-08d93c6ae328")
                                }, EAVFW));
    }
}
