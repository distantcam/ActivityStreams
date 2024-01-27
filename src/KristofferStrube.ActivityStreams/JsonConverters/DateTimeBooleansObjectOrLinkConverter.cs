using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.ActivityStreams.JsonConverters;

public class DateTimeBooleanObjectOrLinkConverter : JsonConverter<DateTimeBooleanObjectOrLink?>
{
    public override DateTimeBooleanObjectOrLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out JsonDocument? doc))
        {
            switch (doc.RootElement.ValueKind)
            {
                case JsonValueKind.True:
                    return new() { Value = true };
                case JsonValueKind.False:
                    return new() { Value = false };
                case JsonValueKind.Number:
                    return new() { Value = doc.RootElement.GetDecimal() != 0 };
                case JsonValueKind.String:
                    if (doc.RootElement.TryGetDateTime(out DateTime datetime))
                    {
                        return new() { Value = datetime };
                    }
                    else if ((new object[] { "true", "false", "1", "0" }).Contains(doc.Deserialize(options.GetTypeInfo(typeof(string)))))
                    {
                        return new() { Value = doc.Deserialize(options.GetTypeInfo(typeof(string))) is "true" or "1" };
                    }
                    return new() { Value = doc.Deserialize(options.GetTypeInfo(typeof(ILink)))! };
                case JsonValueKind.Object:
                    if (doc.RootElement.TryGetProperty("type", out JsonElement type))
                    {
                        return type.GetString() switch
                        {
                            "Link" => new() { Value = doc.Deserialize(options.GetTypeInfo(typeof(ILink)))! },
                            _ => new() { Value = doc.Deserialize(options.GetTypeInfo(typeof(IObject)))! }
                        };
                    }
                    throw new JsonException("JSON element did not have a type property.");
                default:
                    return new() { Value = doc.Deserialize(options.GetTypeInfo(typeof(IObjectOrLink)))! };
            }
        }
        throw new JsonException("Could not be parsed as a JsonDocument.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeBooleanObjectOrLink? value, JsonSerializerOptions options)
    {
        if (value?.Value is null)
        {
            return;
        }
        else if (value.Value is DateTime)
        {
            writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(DateTime))));
        }
        else if (value.Value is bool)
        {
            writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(bool))));
        }
        else if (value.Value is IObject)
        {
            writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(IObject))));
        }
        else if (value.Value is ILink)
        {
            writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(ILink))));
        }
        else
        {
            writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(ObjectOrLink))));
        }
    }
}
