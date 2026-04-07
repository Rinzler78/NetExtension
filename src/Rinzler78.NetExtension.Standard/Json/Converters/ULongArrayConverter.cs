using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

/// <summary>
/// Newtonsoft.Json converter that serializes and deserializes <see cref="ulong"/> arrays as string arrays using invariant culture.
/// </summary>
public sealed class ULongArrayConverter : JsonConverter
{
    public static readonly ULongArrayConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(ulong[]);
    }

    /// <summary>
    /// Reads JSON and converts it to a ulong array.
    /// </summary>
    /// <param name="reader">The JsonReader to read from.</param>
    /// <param name="t">The target type.</param>
    /// <param name="existingValue">The existing value.</param>
    /// <param name="serializer">The JsonSerializer.</param>
    /// <returns>The deserialized ulong array.</returns>
    /// <exception cref="JsonSerializationException">Thrown when the JSON value cannot be converted to ulong array.</exception>
    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var value = serializer.Deserialize<string[]>(reader)
            ?? throw new JsonSerializationException("Cannot convert null array to ulong[]");

        try
        {
            return value.Select(v => ulong.Parse(v, CultureInfo.InvariantCulture)).ToArray();
        }
        catch (FormatException ex)
        {
            throw new JsonSerializationException($"Cannot convert array values to ulong: {ex.Message}", ex);
        }
        catch (OverflowException ex)
        {
            throw new JsonSerializationException($"Numeric overflow converting array values to ulong: {ex.Message}", ex);
        }

    }

    /// <summary>
    /// Writes a <see cref="ulong"/> array as a JSON array of strings.
    /// </summary>
    /// <param name="writer">The JsonWriter to write to.</param>
    /// <param name="untypedValue">The ulong array to serialize.</param>
    /// <param name="serializer">The JsonSerializer.</param>
    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (ulong[])untypedValue;
        serializer.Serialize(writer, value.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray());
    }
}
