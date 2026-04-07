using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

/// <summary>
/// Newtonsoft.Json converter that serializes and deserializes <see cref="ulong"/> values as strings using invariant culture.
/// </summary>
public sealed class ULongConverter : JsonConverter
{
    public static readonly ULongConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(ulong) || t == typeof(ulong?);
    }

    /// <summary>
    /// Reads JSON and converts it to a ulong value.
    /// </summary>
    /// <param name="reader">The JsonReader to read from.</param>
    /// <param name="t">The target type.</param>
    /// <param name="existingValue">The existing value.</param>
    /// <param name="serializer">The JsonSerializer.</param>
    /// <returns>The deserialized ulong value.</returns>
    /// <exception cref="JsonSerializationException">Thrown when the JSON value cannot be converted to ulong.</exception>
    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        if (ulong.TryParse(value, CultureInfo.InvariantCulture, out ulong l)) return l;
        throw new JsonSerializationException($"Cannot convert value '{value}' to ulong");
    }

    /// <summary>
    /// Writes a <see cref="ulong"/> value as a string to JSON.
    /// </summary>
    /// <param name="writer">The JsonWriter to write to.</param>
    /// <param name="untypedValue">The ulong value to serialize.</param>
    /// <param name="serializer">The JsonSerializer.</param>
    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (ulong)untypedValue;
        serializer.Serialize(writer, value.ToString(CultureInfo.InvariantCulture));
    }
}
