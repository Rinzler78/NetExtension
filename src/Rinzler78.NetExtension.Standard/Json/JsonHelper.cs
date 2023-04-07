using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Rinzler78.NetExtension.Json;

public static class JsonHelper
{
    public static string SerializeObject(this object obj) => JsonConvert.SerializeObject(obj);

    public static string SerializeObject(this object obj, Formatting formatting) => JsonConvert.SerializeObject(obj, formatting);

    public static string SerializeObject(this object obj, Formatting formatting, JsonSerializerSettings settings) => JsonConvert.SerializeObject(obj, formatting, settings);

    public static object? DeSerializeObject(this string str) => JsonConvert.DeserializeObject(str);

    public static ObjectType? DeSerializeObject<ObjectType>(this string str) => JsonConvert.DeserializeObject<ObjectType>(str);

    public static ObjectType? DeSerializeObject<ObjectType>(this string str, JsonSerializerSettings settings) => JsonConvert.DeserializeObject<ObjectType>(str, settings);

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

    public static object? DeSerializeObjectFromFile(this string filePath)
    {
        if (File.Exists(filePath))
        {
            using (var reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                return json.DeSerializeObject();
            }
        }

        return null;
    }

    public static ObjectType? DeSerializeObjectFromFile<ObjectType>(this string filePath)
    {
        if (File.Exists(filePath))
        {
            using (var reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<ObjectType>(json);
            }
        }

        return default;
    }

    public static ObjectType? DeSerializeObjectFromFile<ObjectType>(this string filePath, JsonSerializerSettings settings)
    {
        if (File.Exists(filePath))
        {
            using (var reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<ObjectType>(json, settings);
            }
        }

        return default;
    }
}