using System;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class DoubleConverter : JsonConverter
{
    public static readonly DoubleConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(double) || t == typeof(double?);
    }

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        double l;

        if (double.TryParse(value, out l))
            return l;

        if (double.TryParse(value.Replace('.', ','), out l))
            return l;

        throw new Exception($"Cannot unmarshal type double : Path : {reader.Path}, Value : {reader.Value}");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, value: null);
            return;
        }

        var value = (double)untypedValue;
        serializer.Serialize(writer, value.ToString());
    }
}