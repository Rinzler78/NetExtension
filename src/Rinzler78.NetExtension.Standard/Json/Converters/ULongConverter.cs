using System;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class ULongConverter : JsonConverter
{
    public static readonly ULongConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(ulong) || t == typeof(ulong?);
    }

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        if (ulong.TryParse(value, out ulong l)) return l;
        throw new Exception("Cannot unmarshal type double");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (ulong)untypedValue;
        serializer.Serialize(writer, value.ToString());
    }
}