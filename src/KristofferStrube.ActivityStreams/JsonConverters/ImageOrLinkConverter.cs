using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.ActivityStreams.JsonConverters;

public class ImageOrLinkConverter : JsonConverter<IImageOrLink?>
{
    public override IImageOrLink? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out JsonDocument? doc))
        {
            if (doc.RootElement.ValueKind is JsonValueKind.String)
            {
                return (ILink?)doc.Deserialize(options.GetTypeInfo(typeof(ILink)));
            }
            else if (doc.RootElement.TryGetProperty("type", out JsonElement type))
            {
                return type.GetString() switch
                {
                    "Link" => (ILink?)doc.Deserialize(options.GetTypeInfo(typeof(ILink))),
                    "Image" => (Image?)doc.Deserialize(options.GetTypeInfo(typeof(IObject))),
                    _ => throw new JsonException("JSON element was not an Image or a Link."),
                };
            }
            throw new JsonException("JSON element did not have a type property nor was it a string.");
        }
        throw new JsonException("Could not be parsed as a JsonDocument.");
    }

    public override void Write(Utf8JsonWriter writer, IImageOrLink? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            return;
        }

        string? matchingType = value.Type?.FirstOrDefault(t => t == "Link" || t == "Image" || ObjectTypes.Types.ContainsKey(t!), null);
        if (matchingType is null)
        {
            writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(object))));
        }
        else
        {
            if (matchingType == "Link")
            {
                writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(ILink))));
            }
            else if (matchingType == "Image")
            {
                writer.WriteRawValue(Serialize(value, options.GetTypeInfo(typeof(Image))));
            }
            else
            {
                writer.WriteRawValue(Serialize(value, options.GetTypeInfo(ObjectTypes.Types[matchingType])));
            }
        }
    }
}
