using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json;

public static class JsonHelper
{
    public static string SerializeObject(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static object DeSerializeObject(this string str)
    {
        return JsonConvert.DeserializeObject(str);
    }

    public static ObjectType DeSerializeObject<ObjectType>(this string str, params JsonConverter[] converters)
    {
        return JsonConvert.DeserializeObject<ObjectType>(str, converters);
    }

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

    public static object DeSerializeObjectFromFile(this string filePath)
    {
        if (File.Exists(filePath))
            using (var reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                return json.DeSerializeObject();
            }

        return null;
    }

    public static ObjectType DeSerializeObjectFromFile<ObjectType>(this string filePath,
        params JsonConverter[] converters)
    {
        if (File.Exists(filePath))
            using (var reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<ObjectType>(json, converters);
            }

        return default;
    }
}