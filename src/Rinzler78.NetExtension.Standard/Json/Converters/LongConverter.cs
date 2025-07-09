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

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        if (long.TryParse(value, CultureInfo.InvariantCulture, out long l)) return l;
        throw new Exception("Cannot unmarshal type double");
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
