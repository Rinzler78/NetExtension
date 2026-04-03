using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rinzler78.NetExtension.Json.Converters;

public class DateTimeOffsetConverter : IsoDateTimeConverter
{
    public DateTimeOffsetConverter()
    {
        DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal;
    }

    /// <summary>
    /// Reads JSON and converts it to a DateTimeOffset value.
    /// </summary>
    /// <param name="reader">The JsonReader to read from.</param>
    /// <param name="objectType">The target type.</param>
    /// <param name="existingValue">The existing value.</param>
    /// <param name="serializer">The JsonSerializer.</param>
    /// <returns>The deserialized DateTimeOffset value, or null if parsing fails.</returns>
    /// <exception cref="JsonException">Thrown when JSON parsing fails for reasons other than format issues.</exception>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch (FormatException)
        {
            // Invalid date format - return null for nullable types
            return null;
        }
    }
}
