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

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch (Exception ex)
        {
            if (ex is FormatException formatException)
            {
                return null;
            }

            throw;
        }
    }
}
