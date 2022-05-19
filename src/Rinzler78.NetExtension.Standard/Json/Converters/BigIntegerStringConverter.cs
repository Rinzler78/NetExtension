using Newtonsoft.Json;
using System;
using System.Numerics;

namespace Rinzler78.NetExtension.Json.Converters
{
    public class BigIntegerStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(BigInteger) || t == typeof(BigInteger?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            BigInteger l;
            if (BigInteger.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type double");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (BigInteger)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly BigIntegerStringConverter Singleton = new BigIntegerStringConverter();
    }
}