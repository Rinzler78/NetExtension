using System;
using System.Linq;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json.Converters;

public sealed class ULongArrayConverter : JsonConverter
{
    public static readonly ULongArrayConverter Singleton = new();

    public override bool CanConvert(Type t)
    {
        return t == typeof(ulong[]);
    }

    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string[]>(reader);

        try
        {
            return value.Select(ulong.Parse).ToArray();
        }
        finally
        {
        }

        throw new Exception("Cannot unmarshal type ulong[]");
    }

    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
    {
        if (untypedValue is null)
        {
            serializer.Serialize(writer, null);
            return;
        }

        var value = (ulong[])untypedValue;
        serializer.Serialize(writer, value.ToString());
    }
}