using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ExpressionEngine;
using EAVFW.ExpressionEngine.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;
using ValueType = ExpressionEngine.ValueType;

namespace EAVFW.ExpressionEngine.Auxiliary
{
    public class ValidationHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ValidationHandler> _logger;
        private IExpressionEngine _expressionEngine;

        public ValidationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = _serviceProvider.GetRequiredService<ILogger<ValidationHandler>>();
        }

        public async Task<JsonElement> CheckValidations(JsonElement formDefinition, JsonElement formData)
        {
            _logger.LogDebug("FormDefinition: {FormDefinition} - {FormData}", formDefinition, formData);

            var scoped = _serviceProvider.CreateScope().ServiceProvider;
            var dataAccessor = scoped.GetRequiredService<FormDataAccessor>();

            dataAccessor.SetData(formData);
            var dataAsJson = await dataAccessor.ExecuteFunction();

            _expressionEngine = scoped.GetRequiredService<IExpressionEngine>();

            await using var memoryStream = new MemoryStream();
            await using var jsonWriter = new Utf8JsonWriter(memoryStream);
            jsonWriter.WriteStartArray();

            var manifest = JsonSerializer.Deserialize<Manifest>(formDefinition.GetRawText());

            if (manifest?.Entities != null)
            {
                foreach (var (_, entity) in manifest.Entities)
                {
                    foreach (var (_, validation) in entity?.Validations?.Where(x => x.Value != null) ??
                                                    new Dictionary<string, Validation>())
                    {
                        if (dataAsJson.ContainsKey(validation.ValidationError.AttributeSchemaName))
                        {
                            await WriteErrorObject(validation, jsonWriter);
                        }
                    }

                    if (entity?.Attributes == null) continue;

                    {
                        foreach (var (_, attribute) in entity.Attributes)
                        {
                            foreach (var (_, validation) in attribute?.Validations?.Where(x => x.Value != null) ??
                                                            new Dictionary<string, Validation>())
                            {
                                if (dataAsJson.ContainsKey(validation.ValidationError.AttributeSchemaName))
                                {
                                    await WriteErrorObject(validation, jsonWriter);
                                }
                            }
                        }
                    }
                }
            }

            jsonWriter.WriteEndArray();
            await jsonWriter.FlushAsync();

            memoryStream.Position = 0;
            var evaluatedJson = await JsonDocument.ParseAsync(memoryStream);
            _logger.LogDebug("EvaluatedJson: {FormData}", evaluatedJson?.RootElement);
            return evaluatedJson?.RootElement ?? new JsonElement();
        }

        private async Task WriteErrorObject(Validation validation, Utf8JsonWriter jsonWriter)
        {
            var result = await _expressionEngine.ParseToValueContainer(validation.Expression);

            _logger.LogDebug("FormDataResult: {FormData}", result);

            if (result.Type() == ValueType.Boolean && !result.GetValue<bool>())
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("error",
                    await _expressionEngine.Parse(validation.ValidationError.Error));
                jsonWriter.WriteString("code", validation.ValidationError.Code);
                jsonWriter.WriteString("attributeSchemaName", validation.ValidationError.AttributeSchemaName);
                jsonWriter.WriteString("entityCollectionSchemaName",
                    validation.ValidationError.EntityCollectionSchemaName);
                jsonWriter.WriteEndObject();
            }
        }
    }

    public class Manifest
    {
        [JsonPropertyName("entities")] public Dictionary<string, Entity> Entities { get; set; }
    }

    public class Entity
    {
        [JsonPropertyName("validation")] public Dictionary<string, Validation> Validations { get; set; }
        [JsonPropertyName("attributes")] public Dictionary<string, Attribute> Attributes { get; set; }
    }

    public class Attribute
    {
        [JsonPropertyName("validation")] public Dictionary<string, Validation> Validations { get; set; }
    }

    public class Validation
    {
        [JsonPropertyName("expression")] public string Expression { get; set; }
        [JsonPropertyName("error")] public ValidationError ValidationError { get; set; }
    }

    public class ValidationError
    {
        [JsonPropertyName("error")] public string Error { get; set; }
        [JsonPropertyName("code")] public string Code { get; set; }
        [JsonPropertyName("errorArgs")] public object[] ErrorArgs { get; set; }

        [JsonPropertyName("attributeSchemaName")]
        public string AttributeSchemaName { get; set; }

        [JsonPropertyName("entityCollectionSchemaName")]
        public string EntityCollectionSchemaName { get; set; }
    }
}