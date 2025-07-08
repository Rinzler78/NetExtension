# 📝 JSON Serialization

[![Newtonsoft.Json](https://img.shields.io/badge/Newtonsoft.Json-13.0.3-blue?style=flat-square)](https://www.newtonsoft.com/json)
[![Performance](https://img.shields.io/badge/Performance-Optimized-green?style=flat-square)](https://benchmarkdotnet.org/)

Advanced JSON serialization utilities with custom converters, file operations, and high-performance processing capabilities.

## 📋 Table of Contents

- [Overview](#overview)
- [Core Classes](#core-classes)
- [Custom Converters](#custom-converters)
- [Usage Examples](#usage-examples)
- [File Operations](#file-operations)
- [Advanced Features](#advanced-features)
- [Performance Tips](#performance-tips)
- [Best Practices](#best-practices)

## Overview

The JSON system in Rinzler78.NetExtension provides comprehensive serialization capabilities with custom converters for complex types, file-based operations, and optimized performance for high-throughput scenarios.

## Core Classes

### `JsonHelper`
Main utility class providing extension methods for JSON operations.

```csharp
public static class JsonHelper
{
    // Basic serialization
    public static string SerializeObject(this object obj)
    public static string SerializeObject(this object obj, Formatting formatting)
    public static string SerializeObject(this object obj, Formatting formatting, JsonSerializerSettings settings)
    
    // Deserialization
    public static object? DeSerializeObject(this string str)
    public static ObjectType DeSerialize<ObjectType>(this string str)
    public static ObjectType DeSerialize<ObjectType>(this string str, JsonSerializerSettings settings)
    
    // File operations
    public static ObjectType? DeSerializeObjectFromFile<ObjectType>(this string filePath)
    public static ObjectType? DeSerializeObjectFromFile<ObjectType>(this string filePath, JsonSerializerSettings settings)
    
    // Special formatting
    public static string SerializeObjectWithoutQuote(this object value)
}
```

## Custom Converters

### Available Converters

#### `BigIntegerConverter`
Handles `System.Numerics.BigInteger` serialization:

```csharp
public class BigIntegerConverter : JsonConverter<BigInteger>
{
    public override void WriteJson(JsonWriter writer, BigInteger value, JsonSerializer serializer)
    public override BigInteger ReadJson(JsonReader reader, Type objectType, BigInteger existingValue, bool hasExistingValue, JsonSerializer serializer)
}
```

#### `BigRationalConverter`
Handles `BigRational` number serialization:

```csharp
public class BigRationalConverter : JsonConverter<BigRational>
{
    // Converts BigRational to/from string representation
}
```

#### `DateTimeOffsetConverter`
Custom DateTime handling with specific formatting:

```csharp
public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    // Handles DateTimeOffset with custom format strings
}
```

#### `DecimalConverter`
High-precision decimal number handling:

```csharp
public class DecimalConverter : JsonConverter<decimal>
{
    // Preserves decimal precision during serialization
}
```

#### `DoubleConverter`
Floating-point number converter with precision control:

```csharp
public class DoubleConverter : JsonConverter<double>
{
    // Handles double values with specific precision
}
```

#### `ULongConverter` & `ULongArrayConverter`
Unsigned long integer handling:

```csharp
public class ULongConverter : JsonConverter<ulong>
public class ULongArrayConverter : JsonConverter<ulong[]>
{
    // Handles ulong and ulong[] with proper formatting
}
```

#### `InterfaceConverter` & `InterfaceConverterFactory`
Interface serialization support:

```csharp
public class InterfaceConverter<T> : JsonConverter<T>
public class InterfaceConverterFactory : JsonConverterFactory
{
    // Enables serialization of interface types
}
```

#### `CollectionConverter` & `CollectionInterfaceConverterFactory`
Collection and interface collection handling:

```csharp
public class CollectionConverter<T> : JsonConverter<T>
public class CollectionInterfaceConverterFactory : JsonConverterFactory
{
    // Handles various collection types and interfaces
}
```

## Usage Examples

### Basic Serialization

```csharp
using Rinzler78.NetExtension.Json;

var data = new 
{
    Name = "John Doe",
    Age = 30,
    IsActive = true,
    Tags = new[] { "developer", "csharp", "dotnet" }
};

// Simple serialization
string json = data.SerializeObject();

// Formatted serialization
string prettyJson = data.SerializeObject(Formatting.Indented);

// Deserialization
var restored = json.DeSerialize<dynamic>();
```

### Custom Settings with Converters

```csharp
var settings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    NullValueHandling = NullValueHandling.Ignore,
    DateFormatHandling = DateFormatHandling.IsoDateFormat
};

// Add custom converters
settings.Converters.Add(new BigIntegerConverter());
settings.Converters.Add(new BigRationalConverter());
settings.Converters.Add(new DateTimeOffsetConverter());
settings.Converters.Add(new DecimalConverter());

var complexData = new
{
    BigNumber = new BigInteger(123456789012345678),
    Timestamp = DateTimeOffset.Now,
    PreciseValue = 123.456789012345m
};

string json = complexData.SerializeObject(Formatting.Indented, settings);
var restored = json.DeSerialize<dynamic>(settings);
```

### Working with BigInteger

```csharp
var data = new
{
    SmallNumber = new BigInteger(42),
    LargeNumber = BigInteger.Parse("123456789012345678901234567890"),
    Array = new[] 
    { 
        new BigInteger(1), 
        new BigInteger(2), 
        new BigInteger(3) 
    }
};

var settings = new JsonSerializerSettings();
settings.Converters.Add(new BigIntegerConverter());

string json = data.SerializeObject(Formatting.Indented, settings);
// Output: 
// {
//   "SmallNumber": "42",
//   "LargeNumber": "123456789012345678901234567890",
//   "Array": ["1", "2", "3"]
// }
```

### Interface Serialization

```csharp
public interface IShape
{
    double Area { get; }
}

public class Circle : IShape
{
    public double Radius { get; set; }
    public double Area => Math.PI * Radius * Radius;
}

public class Rectangle : IShape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double Area => Width * Height;
}

var shapes = new List<IShape>
{
    new Circle { Radius = 5 },
    new Rectangle { Width = 10, Height = 20 }
};

var settings = new JsonSerializerSettings();
settings.Converters.Add(new InterfaceConverterFactory());

string json = shapes.SerializeObject(Formatting.Indented, settings);
var restored = json.DeSerialize<List<IShape>>(settings);
```

### Special Formatting

```csharp
var config = new
{
    server = "localhost",
    port = 8080,
    ssl = true,
    timeout = 30000
};

// Serialize without quotes (useful for config files)
string configJson = config.SerializeObjectWithoutQuote();
// Output:
// {
//   server: localhost,
//   port: 8080,
//   ssl: true,
//   timeout: 30000
// }
```

## File Operations

### Reading from Files

```csharp
// Generic object deserialization
var data = "/path/to/file.json".DeSerializeObjectFromFile<MyClass>();

// With custom settings
var settings = new JsonSerializerSettings();
settings.Converters.Add(new BigIntegerConverter());
var data = "/path/to/file.json".DeSerializeObjectFromFile<MyClass>(settings);

// Dynamic deserialization
var dynamicData = "/path/to/file.json".DeSerializeObjectFromFile();
```

### Writing to Files

```csharp
public static class JsonFileHelper
{
    public static void SaveToFile<T>(this T obj, string filePath)
    {
        var json = obj.SerializeObject(Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    
    public static void SaveToFile<T>(this T obj, string filePath, JsonSerializerSettings settings)
    {
        var json = obj.SerializeObject(Formatting.Indented, settings);
        File.WriteAllText(filePath, json);
    }
}

// Usage
var data = new { Name = "Test", Value = 42 };
data.SaveToFile("/path/to/output.json");
```

### Batch Operations

```csharp
// Deserialize multiple objects
var jsonStrings = new[]
{
    "{\"name\":\"John\",\"age\":30}",
    "{\"name\":\"Jane\",\"age\":25}",
    "{\"name\":\"Bob\",\"age\":35}"
};

var users = jsonStrings.DeSerialize<User>();
// Returns User[] array

// With custom settings
var settings = new JsonSerializerSettings();
var users = jsonStrings.DeSerialize<User>(settings);
```

## Advanced Features

### Error Handling

```csharp
public static class SafeJsonHelper
{
    public static T? TryDeserialize<T>(this string json, JsonSerializerSettings? settings = null)
    {
        try
        {
            return settings == null 
                ? json.DeSerialize<T>() 
                : json.DeSerialize<T>(settings);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON deserialization error: {ex.Message}");
            return default;
        }
    }
    
    public static string? TrySerialize(this object obj, JsonSerializerSettings? settings = null)
    {
        try
        {
            return settings == null 
                ? obj.SerializeObject() 
                : obj.SerializeObject(Formatting.None, settings);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON serialization error: {ex.Message}");
            return null;
        }
    }
}
```

### Custom Converter Example

```csharp
public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";
    
    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(DateFormat));
    }
    
    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, 
        bool hasExistingValue, JsonSerializer serializer)
    {
        var value = reader.Value?.ToString();
        return DateTime.ParseExact(value, DateFormat, CultureInfo.InvariantCulture);
    }
}

// Usage
var settings = new JsonSerializerSettings();
settings.Converters.Add(new CustomDateTimeConverter());

var data = new { Timestamp = DateTime.Now };
string json = data.SerializeObject(Formatting.Indented, settings);
```

### Streaming Operations

```csharp
public static class StreamingJsonHelper
{
    public static async Task<T> DeserializeFromStreamAsync<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(reader);
        
        var serializer = JsonSerializer.Create();
        return serializer.Deserialize<T>(jsonReader);
    }
    
    public static async Task SerializeToStreamAsync<T>(T obj, Stream stream)
    {
        using var writer = new StreamWriter(stream);
        using var jsonWriter = new JsonTextWriter(writer);
        
        var serializer = JsonSerializer.Create();
        serializer.Serialize(jsonWriter, obj);
    }
}
```

## Performance Tips

### 1. Reuse JsonSerializer Instances
```csharp
private static readonly JsonSerializer Serializer = JsonSerializer.Create();

public static T Deserialize<T>(string json)
{
    using var reader = new StringReader(json);
    using var jsonReader = new JsonTextReader(reader);
    return Serializer.Deserialize<T>(jsonReader);
}
```

### 2. Use Streaming for Large Data
```csharp
public static IEnumerable<T> DeserializeArray<T>(string json)
{
    using var reader = new StringReader(json);
    using var jsonReader = new JsonTextReader(reader);
    
    var serializer = JsonSerializer.Create();
    
    jsonReader.Read(); // Start array
    while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndArray)
    {
        yield return serializer.Deserialize<T>(jsonReader);
    }
}
```

### 3. Optimize Settings
```csharp
private static readonly JsonSerializerSettings OptimizedSettings = new()
{
    NullValueHandling = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore,
    DateParseHandling = DateParseHandling.None,
    FloatParseHandling = FloatParseHandling.Double
};
```

## Best Practices

### 1. Use Appropriate Converters
```csharp
// For financial applications
settings.Converters.Add(new DecimalConverter());

// For large numbers
settings.Converters.Add(new BigIntegerConverter());

// For precise timestamps
settings.Converters.Add(new DateTimeOffsetConverter());
```

### 2. Handle Null Values
```csharp
public static T? SafeDeserialize<T>(this string json) where T : class
{
    if (string.IsNullOrEmpty(json))
        return null;
        
    try
    {
        return json.DeSerialize<T>();
    }
    catch
    {
        return null;
    }
}
```

### 3. Use Proper Error Handling
```csharp
public static class RobustJsonHelper
{
    public static Result<T> TryDeserialize<T>(string json)
    {
        try
        {
            var result = json.DeSerialize<T>();
            return Result<T>.Success(result);
        }
        catch (JsonException ex)
        {
            return Result<T>.Failure(ex.Message);
        }
    }
}
```

### 4. Validate JSON Structure
```csharp
public static bool IsValidJson(this string json)
{
    try
    {
        JToken.Parse(json);
        return true;
    }
    catch
    {
        return false;
    }
}
```

---

[← Back to Main Documentation](../README.md)