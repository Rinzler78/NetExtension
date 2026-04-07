using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rinzler78.NetExtension.Json.Converters;

/// <summary>
/// Newtonsoft.Json converter that extends <see cref="IsoDateTimeConverter"/> to handle <see cref="DateTimeOffset"/> with UTC assumption and graceful format-error handling.
/// </summary>
public sealed class DateTimeOffsetConverter : IsoDateTimeConverter
{
    /// <summary>
    /// Initializes a new instance of <see cref="DateTimeOffsetConverter"/> with <see cref="System.Globalization.DateTimeStyles.AssumeUniversal"/>.
    /// </summary>
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
            if (Nullable.GetUnderlyingType(objectType) != null)
                return null;
            throw;
        }
    }
}
