using System;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Linq;

namespace Rinzler78.NetExtension.Json;

public static class JsonHelper
{
    public static string SerializeObject(this object obj) => JsonConvert.SerializeObject(obj);

    public static string SerializeObject(this object obj, Formatting formatting) => JsonConvert.SerializeObject(obj, formatting);

    public static string SerializeObject(this object obj, Formatting formatting, JsonSerializerSettings settings) => JsonConvert.SerializeObject(obj, formatting, settings);

    public static object? DeSerializeObject(this string str) => JsonConvert.DeserializeObject(str);

    public static ObjectType DeSerialize<ObjectType>(this string str)
        => JsonConvert.DeserializeObject<ObjectType>(str) ?? throw new InvalidOperationException();

    public static ObjectType[] DeSerialize<ObjectType>(this string[] strs)
        => strs.Select(str => str.DeSerialize<ObjectType>()).ToArray();

    public static ObjectType DeSerialize<ObjectType>(this string str, JsonSerializerSettings settings)
        => JsonConvert.DeserializeObject<ObjectType>(str, settings) ?? throw new InvalidOperationException();

    public static ObjectType[] DeSerialize<ObjectType>(this string[] strs, JsonSerializerSettings settings)
        => strs.Select(str => str.DeSerialize<ObjectType>(settings)).ToArray();

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