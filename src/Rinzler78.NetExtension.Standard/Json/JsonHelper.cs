using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Rinzler78.NetExtension.Json
{
    public static class JsonHelper
    {
        public static string SerializeObject(this object obj) => JsonConvert.SerializeObject(obj);

        public static object DeSerializeObject(this string str) => JsonConvert.DeserializeObject(str);

        public static ObjectType DeSerializeObject<ObjectType>(this string str, params JsonConverter[] converters) => JsonConvert.DeserializeObject<ObjectType>(str, converters);

        public static string SerializeObjectWithoutQuote(this object value)
        {
            var builder = new StringBuilder();
            var serializer = JsonSerializer.Create();
            var stringWriter = new StringWriter(builder);
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.QuoteName = false;
                serializer.Serialize(jsonWriter, value);
                return builder.ToString();
            }
        }
    }
}