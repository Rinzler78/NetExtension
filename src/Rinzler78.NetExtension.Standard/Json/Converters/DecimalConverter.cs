using Newtonsoft.Json;
using System;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class DecimalConverter : JsonConverter
{
    public static readonly DecimalConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(decimal) || t == typeof(decimal?);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        if (decimal.TryParse(value, out decimal l)) return l;
        throw new Exception("Cannot unmarshal type decimal");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (decimal)untypedValue;
        serializer.Serialize(writer, value.ToString());
    }
}