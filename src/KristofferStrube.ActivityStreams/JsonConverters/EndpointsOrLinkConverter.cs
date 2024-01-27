﻿using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.ActivityStreams.JsonConverters;

public class EndpointsOrLinkConverter : JsonConverter<IEndpointsOrLink?>
{
    public override IEndpointsOrLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out JsonDocument? doc))
        {
            if (doc.RootElement.ValueKind is JsonValueKind.String)
            {
                return (ILink?)doc.Deserialize(options.GetTypeInfo(typeof(ILink)));
            }
            else if (doc.RootElement.TryGetProperty("type", out JsonElement type) && type.GetString() is "Link")
            {
                return (ILink?)doc.Deserialize(options.GetTypeInfo(typeof(ILink)));
            }
            else
            {
                return (Endpoints?)doc.Deserialize(options.GetTypeInfo(typeof(Endpoints)));
            }
        }
        throw new JsonException($"Could not be parsed as a JsonDocument.");
    }

    public override void Write(Utf8JsonWriter writer, IEndpointsOrLink? value, JsonSerializerOptions options)
    {
        if (value is Endpoints endpoints)
        {
            writer.WriteRawValue(Serialize(endpoints, options.GetTypeInfo(typeof(Endpoints))));
        }
        else if (value is ILink link)
        {
            writer.WriteRawValue(Serialize(link, options.GetTypeInfo(typeof(ILink))));
        }
    }
}
