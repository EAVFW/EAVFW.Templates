using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ExpressionEngine;
using ExpressionEngine.Functions.Base;
using EAVFW.ExpressionEngine.Auxiliary;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ValueType = ExpressionEngine.ValueType;

namespace EAVFW.ExpressionEngine.Functions
{
    public class LookUpResolver : Function
    {
        private readonly IWasmHttpClient _httpClient;
        private readonly ILogger<LookUpResolver> _logger;

        public LookUpResolver(IWasmHttpClient httpClient, ILogger<LookUpResolver> logger) : base("lookUp")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async ValueTask<ValueContainer> ExecuteFunction(params ValueContainer[] parameters)
        {
            _logger.LogDebug("Starting");
            if (parameters?.Length != 2)
            {
                throw new Exception($"[{nameof(LookUpResolver)}] lookUp expects two arguments, entity schema name and record id");
            }
            var entityNameVc = parameters[0];
            var recordIdVc = parameters[1];
            if (recordIdVc.Type() == ValueType.Null)
            {
                return new ValueContainer();
            }

            /* TODO: The response from the API is not correctly encoded/decoded. The header says it's UTF-8, however
             * the StreamReader does not decode the content as UTF-8.
             */ 
            if (entityNameVc.Type() != ValueType.String || recordIdVc.Type() != ValueType.String)
            {
                throw new Exception($"[{nameof(LookUpResolver)}] Both arguments must be of type string. " +
                                    $"Entity schema was {entityNameVc.Type()} and Record id was {recordIdVc.Type()}");
            } 
            
            var httpResponseMessage = await _httpClient.GetAsync($"entities/{entityNameVc}/records/{recordIdVc}");
            httpResponseMessage.EnsureSuccessStatusCode();

            var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            var content = await streamReader.ReadToEndAsync();

            var json = JToken.Parse(content).SelectToken("$.value");

            _logger.LogDebug("Content: {Json}", json);
            
            return await ValueContainerExtension.CreateValueContainerFromJToken(json);
        }
    }
}