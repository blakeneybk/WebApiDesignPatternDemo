using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fora.ImportService.JsonConverters;

public class StringOrNumberConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String when int.TryParse(reader.GetString(), out int value) => value,
            JsonTokenType.String => throw new JsonException("String could not be converted to integer."),
            _ => throw new JsonException($"Unexpected token parsing integer. Token: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}