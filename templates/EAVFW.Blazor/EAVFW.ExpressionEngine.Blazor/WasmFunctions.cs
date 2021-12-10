using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ExpressionEngine;
using EAVFW.ExpressionEngine.Auxiliary;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace EAVFW.ExpressionEngine.Blazor
{
    public static class WasmFunctions
    {
        private static readonly ServiceProvider Sp;
        private static readonly ILogger<Program> Logger;
        private static readonly FormDefinitionParser FormDefinitionParser;
        private static readonly ValidationHandler ValidationHandler;


        static WasmFunctions()
        {
            var webAssemblyHostBuilder = WebAssemblyHostBuilder.CreateDefault();
            webAssemblyHostBuilder.Logging.SetMinimumLevel(LogLevel.Debug);

            var services = webAssemblyHostBuilder.Services;

            services.AddExpressionEngine();
            services.AddCustomFunctions();
            services.AddHttpClient<IWasmHttpClient, WasmHttpClient>();
            Sp = services.BuildServiceProvider();
            FormDefinitionParser = new FormDefinitionParser(Sp);
            ValidationHandler = new ValidationHandler(Sp);
            Logger = Sp.GetRequiredService<ILogger<Program>>();
        }

        [JSInvokable(nameof(ParseAsync))]
        public static async Task<JsonElement> ParseAsync(JsonElement formDefinition, JsonElement formData)
        {
            Logger.LogDebug("Entered {Name}", nameof(ParseAsync));

            var stopWatch = new Stopwatch();
            
            stopWatch.Start();
            var evalResult = await FormDefinitionParser.ParseAsync(formDefinition, formData);
            stopWatch.Stop();
            
            Logger.LogDebug("Evaluated form in {EvalTime} milliseconds", stopWatch.ElapsedMilliseconds);
            
            return evalResult;
        }
        
        [JSInvokable(nameof(ValidateForm))]
        public static async Task<JsonElement> ValidateForm(JsonElement formDefinition, JsonElement formData)
        {
            Logger.LogDebug("Entered {Name}", nameof(ValidateForm));

            var stopWatch = new Stopwatch();
            
            stopWatch.Start();
            var evalResult = await ValidationHandler.CheckValidations(formDefinition, formData);
            stopWatch.Stop();
            
            Logger.LogDebug("Validated form in {EvalTime} milliseconds", stopWatch.ElapsedMilliseconds);
            
            return evalResult;
        }
    }
}