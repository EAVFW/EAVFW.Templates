using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ExpressionEngine;
using EAVFW.ExpressionEngine.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ValueType = ExpressionEngine.ValueType;

namespace EAVFW.ExpressionEngine.Auxiliary
{
    public class FormDefinitionParser
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FormDefinitionParser> _logger;

        private IExpressionEngine _expressionEngine;

        public FormDefinitionParser(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = _serviceProvider.GetRequiredService<ILogger<FormDefinitionParser>>();
        }

        public async Task<JsonElement> ParseAsync(JsonElement formDefinition, JsonElement formData)
        {
            _logger.LogDebug("FormDefinition: {FormDefinition}", formDefinition);
            _logger.LogDebug("FormData: {FormData}", formData);

            var scoped = _serviceProvider.CreateScope().ServiceProvider;
            var dataAccessor = scoped.GetRequiredService<FormDataAccessor>();

            dataAccessor.SetData(formData);

            _expressionEngine = scoped.GetRequiredService<IExpressionEngine>();

            await using var memoryStream = new MemoryStream();
            await using var jsonWriter = new Utf8JsonWriter(memoryStream);

            try
            {
                await EvaluateAndWriteJson(jsonWriter, formDefinition);
            }
            catch (Exception e)
            {
                _logger.LogDebug("Caught an error\n{ErrorMessage}\n{ErrorStackTrace}",
                    e.Message, e.StackTrace);
                return formDefinition;
            }

            await jsonWriter.FlushAsync();

            memoryStream.Position = 0;
            var evaluatedJson = await JsonDocument.ParseAsync(memoryStream);
            return evaluatedJson.RootElement;
        }

        private async Task EvaluateAndWriteJson(Utf8JsonWriter jsonWriter, JsonElement formDefinition)
        {
            switch (formDefinition.ValueKind)
            {
                case JsonValueKind.Object:
                    jsonWriter.WriteStartObject();
                    foreach (var property in formDefinition.EnumerateObject())
                        await EvaluateJsonProperty(jsonWriter, property);
                    jsonWriter.WriteEndObject();
                    return;

                case JsonValueKind.String:
                    await EvaluateAndWritePropertyValue(jsonWriter, formDefinition.GetString());
                    break;

                case JsonValueKind.Array:
                    jsonWriter.WriteStartArray();
                    foreach (var jsonElement in formDefinition.EnumerateArray())
                    {
                        await EvaluateAndWriteJson(jsonWriter, jsonElement);
                    }

                    jsonWriter.WriteEndArray();
                    break;

                case JsonValueKind.Number:
                    jsonWriter.WriteNumberValue(formDefinition.GetDecimal());
                    break;

                case JsonValueKind.True:
                    jsonWriter.WriteBooleanValue(false);
                    break;

                case JsonValueKind.False:
                    jsonWriter.WriteBooleanValue(false);
                    break;
                default: return;
            }
        }

        private async Task EvaluateJsonProperty(Utf8JsonWriter jsonWriter, JsonProperty property)
        {
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.String:
                    jsonWriter.WritePropertyName(property.Name);
                    await EvaluateAndWritePropertyValue(jsonWriter, property.Value.GetString());
                    break;

                case JsonValueKind.Array:
                    jsonWriter.WritePropertyName(property.Name);
                    jsonWriter.WriteStartArray();

                    foreach (var jsonElement in property.Value.EnumerateArray())
                    {
                        await EvaluateAndWriteJson(jsonWriter, jsonElement);
                    }

                    jsonWriter.WriteEndArray();

                    break;
                case JsonValueKind.Object:
                    jsonWriter.WritePropertyName(property.Name);
                    await EvaluateAndWriteJson(jsonWriter, property.Value);
                    break;

                case JsonValueKind.Number:
                    jsonWriter.WritePropertyName(property.Name);
                    await EvaluateAndWriteJson(jsonWriter, property.Value);
                    break;

                case JsonValueKind.True:
                    jsonWriter.WriteBoolean(property.Name, true);
                    break;

                case JsonValueKind.False:
                    jsonWriter.WriteBoolean(property.Name, false);
                    break;

                case JsonValueKind.Null:
                    jsonWriter.WriteNull(property.Name);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("Undefined should not exist");
            }
        }

        private async Task EvaluateAndWritePropertyValue(Utf8JsonWriter jsonWriter, string element)
        {
            var evaluatedString = await _expressionEngine.ParseToValueContainer(element);
            switch (evaluatedString.Type())
            {
                case ValueType.Boolean:
                    jsonWriter.WriteBooleanValue(evaluatedString.GetValue<bool>());
                    break;
                case ValueType.Integer:
                    jsonWriter.WriteNumberValue(evaluatedString.GetValue<int>());
                    break;
                case ValueType.Float:
                    jsonWriter.WriteNumberValue(evaluatedString.GetValue<float>());
                    break;
                case ValueType.String:
                    jsonWriter.WriteStringValue(evaluatedString.GetValue<string>());
                    break;
                case ValueType.Null:
                    jsonWriter.WriteNullValue();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Object or Array should not be given to this function");
            }
        }
    }
}