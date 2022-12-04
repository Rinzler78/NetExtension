using System;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

public class ULongConverter : JsonConverter
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
        ulong l;
        if (ulong.TryParse(value, out l)) return l;
        throw new Exception("Cannot unmarshal type double");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (ulong)untypedValue;
        serializer.Serialize(writer, value.ToString());
    }
}