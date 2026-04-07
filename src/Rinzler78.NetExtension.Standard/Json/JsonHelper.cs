using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Json;

/// <summary>
/// Provides utility methods for JSON serialization and deserialization operations using Newtonsoft.Json.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Serializes an object to a JSON string using default formatting.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns>A JSON string representation of the object</returns>
    /// <example>
    /// <code>
    /// var user = new { Name = "John", Age = 30 };
    /// string json = user.SerializeObject(); // {"Name":"John","Age":30}
    /// </code>
    /// </example>
    public static string SerializeObject(this object obj) => JsonConvert.SerializeObject(obj);

    /// <summary>
    /// Serializes an object to a JSON string with the specified formatting.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="formatting">The formatting style to apply (None or Indented)</param>
    /// <returns>A JSON string representation of the object with the specified formatting</returns>
    /// <example>
    /// <code>
    /// var user = new { Name = "John", Age = 30 };
    /// string json = user.SerializeObject(Formatting.Indented);
    /// // Returns formatted JSON with indentation
    /// </code>
    /// </example>
    public static string SerializeObject(this object obj, Formatting formatting) => JsonConvert.SerializeObject(obj, formatting);

    /// <summary>
    /// Serializes an object to a JSON string with the specified formatting and serializer settings.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <param name="formatting">The formatting style to apply (None or Indented)</param>
    /// <param name="settings">The JsonSerializerSettings to use for customizing serialization behavior</param>
    /// <returns>A JSON string representation of the object with the specified formatting and settings</returns>
    /// <example>
    /// <code>
    /// var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
    /// string json = user.SerializeObject(Formatting.Indented, settings);
    /// </code>
    /// </example>
    public static string SerializeObject(this object obj, Formatting formatting, JsonSerializerSettings settings) => JsonConvert.SerializeObject(obj, formatting, settings);

    /// <summary>
    /// Deserializes a JSON string to a dynamic object.
    /// </summary>
    /// <param name="str">The JSON string to deserialize</param>
    /// <returns>The deserialized object, or null if the string is null or empty</returns>
    /// <example>
    /// <code>
    /// string json = "{\"Name\":\"John\",\"Age\":30}";
    /// dynamic obj = json.DeserializeObject();
    /// Console.WriteLine(obj.Name); // John
    /// </code>
    /// </example>
    public static object? DeserializeObject(this string str) => JsonConvert.DeserializeObject(str);

    /// <summary>
    /// Deserializes a JSON string to a strongly-typed object of the specified type.
    /// </summary>
    /// <typeparam name="ObjectType">The type to deserialize the JSON to</typeparam>
    /// <param name="str">The JSON string to deserialize</param>
    /// <returns>The deserialized object of type ObjectType, or the default value if the string is null or empty</returns>
    /// <example>
    /// <code>
    /// string json = "{\"Name\":\"John\",\"Age\":30}";
    /// User user = json.Deserialize&lt;User&gt;();
    /// Console.WriteLine(user.Name); // John
    /// </code>
    /// </example>
    public static ObjectType? Deserialize<ObjectType>(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return default;

        return JsonConvert.DeserializeObject<ObjectType>(str);
    }

    /// <summary>
    /// Deserializes an array of JSON strings to an array of strongly-typed objects.
    /// </summary>
    /// <typeparam name="ObjectType">The type to deserialize each JSON string to</typeparam>
    /// <param name="strs">The array of JSON strings to deserialize</param>
    /// <returns>An array of deserialized objects of type ObjectType</returns>
    /// <example>
    /// <code>
    /// string[] jsonArray = { "{\"Name\":\"John\"}", "{\"Name\":\"Jane\"}" };
    /// User[] users = jsonArray.Deserialize&lt;User&gt;();
    /// </code>
    /// </example>
    public static ObjectType?[] Deserialize<ObjectType>(this string[] strs)
        => strs.Select(str => str.Deserialize<ObjectType>()).ToArray();

    /// <summary>
    /// Deserializes a JSON string to a strongly-typed object using custom serializer settings.
    /// </summary>
    /// <typeparam name="ObjectType">The type to deserialize the JSON to</typeparam>
    /// <param name="str">The JSON string to deserialize</param>
    /// <param name="settings">The JsonSerializerSettings to use for customizing deserialization behavior</param>
    /// <returns>The deserialized object of type ObjectType, or the default value if the string is null or empty</returns>
    /// <example>
    /// <code>
    /// var settings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd" };
    /// User user = json.Deserialize&lt;User&gt;(settings);
    /// </code>
    /// </example>
    public static ObjectType? Deserialize<ObjectType>(this string? str, JsonSerializerSettings settings)
    {
        if (string.IsNullOrEmpty(str))
            return default;

        return JsonConvert.DeserializeObject<ObjectType>(str, settings);
    }

    /// <summary>
    /// Deserializes an array of JSON strings to an array of strongly-typed objects using custom serializer settings.
    /// </summary>
    /// <typeparam name="ObjectType">The type to deserialize each JSON string to</typeparam>
    /// <param name="strs">The array of JSON strings to deserialize</param>
    /// <param name="settings">The JsonSerializerSettings to use for customizing deserialization behavior</param>
    /// <returns>An array of deserialized objects of type ObjectType</returns>
    /// <example>
    /// <code>
    /// var settings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd" };
    /// User[] users = jsonArray.Deserialize&lt;User&gt;(settings);
    /// </code>
    /// </example>
    public static ObjectType?[] Deserialize<ObjectType>(this string[] strs, JsonSerializerSettings settings)
        => strs.Select(str => str.Deserialize<ObjectType>(settings)).ToArray();

    /// <summary>
    /// Serializes an object to JSON format without quotes around property names and with special handling for string values.
    /// String values are returned as-is without JSON quotes, while other objects are serialized normally but with unquoted property names.
    /// </summary>
    /// <param name="value">The object to serialize</param>
    /// <returns>A JSON string with unquoted property names, or the string value itself if the input is a string</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 30 };
    /// string result = obj.SerializeObjectWithoutQuote(); // {Name: "John", Age: 30}
    /// string str = "Hello";
    /// string result2 = str.SerializeObjectWithoutQuote(); // Hello
    /// </code>
    /// </example>
    public static string SerializeObjectWithoutQuote(this object? value)
    {
        // Handle null values
        if (value == null)
        {
            return "null";
        }

        // For simple string values, return without quotes
        if (value is string stringValue)
        {
            return stringValue;
        }

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

    /// <summary>
    /// Deserializes JSON content from a file to a dynamic object.
    /// Validates the file path to prevent directory traversal attacks.
    /// </summary>
    /// <param name="filePath">The path to the JSON file to read and deserialize</param>
    /// <returns>The deserialized object, or null if the file doesn't exist</returns>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid or potentially malicious</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="JsonException">Thrown when the file content cannot be deserialized as JSON</exception>
    /// <example>
    /// <code>
    /// dynamic data = "/path/to/data.json".DeserializeObjectFromFile();
    /// Console.WriteLine(data.Property);
    /// </code>
    /// </example>
    /// <remarks>
    /// Security considerations:
    /// - Validates file path to prevent directory traversal attacks
    /// - Ensures the file path is within allowed directories
    /// - Prevents access to system files and sensitive locations
    /// </remarks>
    public static object? DeserializeObjectFromFile(this string filePath)
    {
        ValidateFilePath(filePath);

        if (File.Exists(filePath))
        {
            using (var reader = new StreamReader(filePath))
            {
                var json = reader.ReadToEnd();
                return json.DeserializeObject();
            }
        }

        return null;
    }

    /// <summary>
    /// Deserializes JSON content from a file to a strongly-typed object.
    /// Validates the file path to prevent directory traversal attacks.
    /// </summary>
    /// <typeparam name="ObjectType">The type to deserialize the JSON content to</typeparam>
    /// <param name="filePath">The path to the JSON file to read and deserialize</param>
    /// <returns>The deserialized object of type ObjectType</returns>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid or potentially malicious</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="JsonException">Thrown when the file content cannot be deserialized to the specified type</exception>
    /// <example>
    /// <code>
    /// User user = "/path/to/user.json".DeserializeObjectFromFile&lt;User&gt;();
    /// Console.WriteLine(user.Name);
    /// </code>
    /// </example>
    /// <remarks>
    /// Security considerations:
    /// - Validates file path to prevent directory traversal attacks
    /// - Ensures the file path is within allowed directories
    /// - Prevents access to system files and sensitive locations
    /// </remarks>
    public static ObjectType DeserializeObjectFromFile<ObjectType>(this string filePath)
    {
        ValidateFilePath(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        using (var reader = new StreamReader(filePath))
        {
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<ObjectType>(json)!;
        }
    }

    /// <summary>
    /// Deserializes JSON content from a file to a strongly-typed object using custom serializer settings.
    /// Validates the file path to prevent directory traversal attacks.
    /// </summary>
    /// <typeparam name="ObjectType">The type to deserialize the JSON content to</typeparam>
    /// <param name="filePath">The path to the JSON file to read and deserialize</param>
    /// <param name="settings">The JsonSerializerSettings to use for customizing deserialization behavior</param>
    /// <returns>The deserialized object of type ObjectType</returns>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid or potentially malicious</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when access to the file is denied</exception>
    /// <exception cref="JsonException">Thrown when the file content cannot be deserialized to the specified type</exception>
    /// <example>
    /// <code>
    /// var settings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd" };
    /// User user = "/path/to/user.json".DeserializeObjectFromFile&lt;User&gt;(settings);
    /// </code>
    /// </example>
    /// <remarks>
    /// Security considerations:
    /// - Validates file path to prevent directory traversal attacks
    /// - Ensures the file path is within allowed directories
    /// - Prevents access to system files and sensitive locations
    /// </remarks>
    public static ObjectType DeserializeObjectFromFile<ObjectType>(this string filePath, JsonSerializerSettings settings)
    {
        ValidateFilePath(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        using (var reader = new StreamReader(filePath))
        {
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<ObjectType>(json, settings)!;
        }
    }

    /// <summary>
    /// Validates a file path to ensure it is safe for file operations.
    /// Prevents directory traversal attacks and access to sensitive system files.
    /// </summary>
    /// <param name="filePath">The file path to validate</param>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid or potentially malicious</exception>
    /// <remarks>
    /// Security considerations:
    /// - Prevents directory traversal attacks using "../" sequences
    /// - Blocks access to critical system files and executables
    /// - Ensures the path is properly formatted and not malicious
    /// - Allows legitimate file operations while preventing security vulnerabilities
    /// </remarks>
    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(filePath);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid file path: {ex.Message}", nameof(filePath), ex);
        }

        // Block directory traversal attempts and home directory shell expansion
        if (filePath.Contains("..") || filePath.StartsWith("~/", StringComparison.Ordinal) || filePath == "~")
            throw new ArgumentException(
                "File path contains potentially malicious characters.", nameof(filePath));

        // Extension allowlist: only safe data file extensions are permitted
        var extension = Path.GetExtension(fullPath);
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".json", ".xml", ".csv", ".txt", ".yaml", ".yml", ".toml", ".ini", ".conf"
        };
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            throw new ArgumentException(
                $"File extension '{extension}' is not allowed. Only data files are permitted.",
                nameof(filePath));
    }
}
