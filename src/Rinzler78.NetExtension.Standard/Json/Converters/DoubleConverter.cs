using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

[SuppressMessage("Usage", "MA0011:IFormatProvider is missing")]
public sealed class DoubleConverter : JsonConverter
{
    public static readonly DoubleConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(double) || t == typeof(double?);
    }

    /// <summary>
    /// Reads JSON and converts it to a double value.
    /// </summary>
    /// <param name="reader">The JsonReader to read from.</param>
    /// <param name="t">The target type.</param>
    /// <param name="existingValue">The existing value.</param>
    /// <param name="serializer">The JsonSerializer.</param>
    /// <returns>The deserialized double value.</returns>
    /// <exception cref="JsonSerializationException">Thrown when the JSON value cannot be converted to double.</exception>
    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);

        if (value is not null)
        {
            if (double.TryParse(value, out double l))
                return l;

            if (double.TryParse(value.Replace('.', ','), out l))
                return l;
        }

        throw new JsonSerializationException($"Cannot convert value '{value}' to double at path {reader.Path}");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (double)untypedValue;
        serializer.Serialize(writer, value.ToString());
    }
}
