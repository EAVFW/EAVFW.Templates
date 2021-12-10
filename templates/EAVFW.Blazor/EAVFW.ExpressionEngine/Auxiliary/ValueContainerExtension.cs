using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ExpressionEngine;

namespace EAVFW.ExpressionEngine.Auxiliary
{
    public static class ValueContainerCustomExtension
    {
        public static async ValueTask<ValueContainer> CreateValueContainerFromJsonElement(JsonElement json,
            IExpressionEngine expressionEngine = null)
        {
            return await JsonToValueContainer(json, expressionEngine);
        }

        private static async ValueTask<ValueContainer> JsonToValueContainer(JsonElement json,
            IExpressionEngine expressionEngine)
        {
            switch (json.ValueKind)
            {
                case JsonValueKind.Null:
                    return new ValueContainer();
                case JsonValueKind.False:
                    return new ValueContainer(false);
                case JsonValueKind.True:
                    return new ValueContainer(false);
                case JsonValueKind.String:
                    return expressionEngine != null
                        ? await expressionEngine.ParseToValueContainer(json.GetString())
                        : new ValueContainer(json.GetString());
                case JsonValueKind.Undefined:
                    break;
                case JsonValueKind.Number:
                    if (json.TryGetInt32(out var intValue))
                    {
                        return new ValueContainer(intValue);
                    }
                    else if (json.TryGetDouble(out var doubleValue))
                    {
                        return new ValueContainer(doubleValue);
                    }
                    else
                    {
                        throw new Exception("Could not parse number as either int32 or double");
                    }

                case JsonValueKind.Array:
                    var list = new List<ValueContainer>();

                    foreach (var jsonElement in json.EnumerateArray())
                    {
                        list.Add(await JsonToValueContainer(jsonElement, expressionEngine));
                    }

                    return new ValueContainer(list);

                case JsonValueKind.Object:
                    var dict = new Dictionary<string, ValueContainer>();

                    foreach (var jsonProperty in json.EnumerateObject())
                    {
                        dict.Add(jsonProperty.Name, await JsonToValueContainer(jsonProperty.Value, expressionEngine));
                    }

                    return new ValueContainer(dict);
                default:
                    throw new Exception();
            }

            return null;
        }
    }
}