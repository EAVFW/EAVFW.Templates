using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EAVFW.Common.MiddleWare
{
    public class NextJsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<NextJsMiddleware> _logger;
        private readonly Dictionary<string, Regex> _routes;

        public NextJsMiddleware(RequestDelegate next, IWebHostEnvironment environment, ILogger<NextJsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _routes = File.Exists($"{environment.ContentRootPath}/.next/routes-manifest.json")
                ? JToken.Parse(File.ReadAllText($"{environment.ContentRootPath}/.next/routes-manifest.json"))
                    .SelectToken("$.dynamicRoutes").ToDictionary(k => k.SelectToken("$.page") + "/index.html",
                        v => new Regex(v.SelectToken("$.regex")?.ToString() ?? string.Empty))
                : new Dictionary<string, Regex>();

            logger.LogInformation("Initialized Routes : {RouteKeys}", string.Join(",", _routes.Keys));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var matched = _routes.FirstOrDefault(k => k.Value.IsMatch(httpContext.Request.Path));
            if (!matched.Equals(default(KeyValuePair<string, Regex>)))
            {
                _logger.LogInformation("Route matched with NextJS Route: {MatchedKey}", matched.Key);
                httpContext.Request.Path = $"{matched.Key}";
            }

            await _next(httpContext);
        }
    }
}