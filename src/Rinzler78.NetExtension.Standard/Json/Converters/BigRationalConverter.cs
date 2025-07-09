using System;
using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class BigRationalConverter : JsonConverter
{
    public static readonly BigRationalConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(BigRational) || t == typeof(BigRational?);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        if (BigRational.TryParse(value, CultureInfo.InvariantCulture, out BigRational l)) return l;
        throw new Exception("Cannot unmarshal type double");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (BigRational)untypedValue;
        //serializer.Serialize(writer, value.ToString(CultureInfo.InvariantCulture));
        serializer.Serialize(writer, value.ToString());
    }
}
