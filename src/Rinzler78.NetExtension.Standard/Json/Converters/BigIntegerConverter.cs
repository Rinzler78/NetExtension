using System;
using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class BigIntegerConverter : JsonConverter
{
    public static readonly BigIntegerConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(BigInteger) || t == typeof(BigInteger?);
    }

    /// <summary>
    /// Reads JSON and converts it to a BigInteger value.
    /// </summary>
    /// <param name="reader">The JsonReader to read from.</param>
    /// <param name="t">The target type.</param>
    /// <param name="existingValue">The existing value.</param>
    /// <param name="serializer">The JsonSerializer.</param>
    /// <returns>The deserialized BigInteger value.</returns>
    /// <exception cref="JsonSerializationException">Thrown when the JSON value cannot be converted to BigInteger.</exception>
    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        if (BigInteger.TryParse(value, CultureInfo.InvariantCulture, out BigInteger l)) return l;
        throw new JsonSerializationException($"Cannot convert value '{value}' to BigInteger");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (BigInteger)untypedValue;
        serializer.Serialize(writer, value.ToString(CultureInfo.InvariantCulture));
    }
}
