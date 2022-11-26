﻿using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.ActivityStreams.JsonConverters;

internal class ObjectOrLinkConverter : JsonConverter<IObjectOrLink?>
{
    public override IObjectOrLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out JsonDocument? doc))
        {
            if (doc.RootElement.ValueKind is JsonValueKind.String)
            {
                return doc.Deserialize<ILink>(options);
            }
            else if (doc.RootElement.TryGetProperty("type", out JsonElement type))
            {
                string? matchingType = null;
                if (type.ValueKind is JsonValueKind.Array)
                {
                    IEnumerable<string> typeEnumerator = type.EnumerateArray().Select(t => t.GetString()!);
                    matchingType = typeEnumerator.FirstOrDefault(t => t == "Link" || ObjectTypes.Types.ContainsKey(t!), null);
                    if (matchingType is null)
                    {
                        return null;
                    }
                }
                else
                {
                    matchingType = type.GetString();
                }
                return matchingType switch
                {
                    "Link" => doc.Deserialize<ILink>(options),
                    _ => doc.Deserialize<IObject>(options),
                };
            }
            else
            {
                return doc.Deserialize<ObjectOrLink>(options);
            }
        }
        throw new JsonException("Could not be parsed as a JsonDocument.");
    }

    public override void Write(Utf8JsonWriter writer, IObjectOrLink? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
