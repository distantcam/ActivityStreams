using KristofferStrube.ActivityStreams.JsonLD;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.ActivityStreams.JsonConverters;

public class TermDefinitionConverter : JsonConverter<ITermDefinition?>
{
    public override ITermDefinition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out JsonDocument? doc))
        {
            if (doc.RootElement.ValueKind is JsonValueKind.String)
            {
                return new ReferenceTermDefinition((Uri?)doc.Deserialize(options.GetTypeInfo(typeof(Uri)))!);
            }
            else if (doc.RootElement.ValueKind is JsonValueKind.Object)
            {
                return (ExpandedTermDefinition?)doc.Deserialize(options.GetTypeInfo(typeof(ExpandedTermDefinition)));
            }
            throw new JsonException("JSON element was neither an object nor a string.");
        }
        throw new JsonException("Could not be parsed as a JsonDocument.");
    }

    public override void Write(Utf8JsonWriter writer, ITermDefinition? value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ReferenceTermDefinition reference:
                writer.WriteRawValue(Serialize(reference.Href, options.GetTypeInfo(typeof(Uri))));
                break;
            case ExpandedTermDefinition:
                writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(ExpandedTermDefinition))));
                break;
        };
    }
}
