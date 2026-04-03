using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class LongConverter : JsonConverter
{
    public static readonly LongConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(long) || t == typeof(long?);
    }

    /// <summary>
    /// Reads JSON and converts it to a long value.
    /// </summary>
    /// <param name="reader">The JsonReader to read from.</param>
    /// <param name="t">The target type.</param>
    /// <param name="existingValue">The existing value.</param>
    /// <param name="serializer">The JsonSerializer.</param>
    /// <returns>The deserialized long value.</returns>
    /// <exception cref="JsonSerializationException">Thrown when the JSON value cannot be converted to long.</exception>
    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        if (long.TryParse(value, CultureInfo.InvariantCulture, out long l)) return l;
        throw new JsonSerializationException($"Cannot convert value '{value}' to long");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (long)untypedValue;
        serializer.Serialize(writer, value.ToString(CultureInfo.InvariantCulture));
    }
}
