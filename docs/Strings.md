# 🔤 String Utilities

[![HTTP Client](https://img.shields.io/badge/HTTP-Client-orange?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
[![Text Processing](https://img.shields.io/badge/Text-Processing-blue?style=flat-square)](https://docs.microsoft.com/en-us/dotnet/api/system.string)

Comprehensive string manipulation utilities including similarity calculations, case conversions, HTTP operations, and advanced text processing capabilities.

## 📋 Table of Contents

- [Overview](#overview)
- [Core Features](#core-features)
- [String Similarity](#string-similarity)
- [Case Conversions](#case-conversions)
- [HTTP Operations](#http-operations)
- [Validation](#validation)
- [Text Processing](#text-processing)
- [Usage Examples](#usage-examples)
- [Performance Tips](#performance-tips)
- [Best Practices](#best-practices)

## Overview

The String utilities in Rinzler78.NetExtension provide advanced text processing capabilities including similarity calculations, case conversions, HTTP client operations, and various string manipulation methods optimized for performance and usability.

## Core Features

### `StringHelper` Class
Main utility class providing extension methods for string operations:

```csharp
public static class StringHelper
{
    // Similarity calculations
    public static int ComputeLevenshteinDistance(this string source, string target)
    public static double CalculateSimilarity(this string source, string target)

    // Text searching
    public static bool ContainsAll(this string str, string[] words)
    public static bool ContainsAny(this string str, string[] words)

    // Case conversions
    public static string BeginByLowerCase(this string str)
    public static string BeginByUpperCase(this string str)
    public static string ToStartByUpperCase(this string str)
    public static string ToPascalCase(this string str)

    // HTTP operations
    public static Task<Stream> HttpGetStreamAsync(this string url)
    public static Task<string> HttpGetStringAsync(this string url)
    public static Task<ReturnType> HttpGetAsync<ReturnType>(this string url, JsonSerializerSettings? settings = null)
    public static Task<string> HttpPostString<RequestType>(this string url, RequestType? obj)

    // Validation
    public static bool IsValidEmail(this string str)
    public static bool IsNullOrEmpty(this string str)

    // Utility methods
    public static string GetBytesReadable(this long i)
    public static byte[] GetBytes(this string str)
    public static string ToJsonFormattedString(this string jsonString)
}
```

## String Similarity

### Levenshtein Distance
Calculate the minimum number of single-character edits required to transform one string into another:

```csharp
string source = "kitten";
string target = "sitting";

int distance = source.ComputeLevenshteinDistance(target);
// Result: 3 (substitute k→s, substitute e→i, insert g)

Console.WriteLine($"Distance between '{source}' and '{target}': {distance}");
```

### Similarity Calculation
Calculate similarity percentage between two strings:

```csharp
string text1 = "hello world";
string text2 = "hello ward";

double similarity = text1.CalculateSimilarity(text2);
// Result: 0.82 (82% similar)

Console.WriteLine($"Similarity: {similarity:P}"); // Output: 82.00%
```

### Practical Applications
```csharp
public class FuzzySearch
{
    public static List<string> FindSimilar(string query, IEnumerable<string> candidates,
        double threshold = 0.6)
    {
        return candidates
            .Where(candidate => query.CalculateSimilarity(candidate) >= threshold)
            .OrderByDescending(candidate => query.CalculateSimilarity(candidate))
            .ToList();
    }
}

// Usage
var cities = new[] { "Paris", "Berlin", "London", "Madrid", "Rome" };
var similar = FuzzySearch.FindSimilar("Pariis", cities, 0.7);
// Result: ["Paris"] (high similarity despite typo)
```

## Case Conversions

### Basic Case Operations
```csharp
string text = "hello world";

// First character to uppercase
string capitalized = text.BeginByUpperCase();
// Result: "Hello world"

// First character to lowercase
string lowercased = text.BeginByLowerCase();
// Result: "hello world"

// Proper case (first letter uppercase, rest lowercase)
string proper = "HELLO WORLD".ToStartByUpperCase();
// Result: "Hello world"
```

### PascalCase Conversion
Convert strings to PascalCase format:

```csharp
// Snake case to PascalCase
string snake = "hello_world_example";
string pascal = snake.ToPascalCase();
// Result: "HelloWorldExample"

// Kebab case to PascalCase
string kebab = "hello-world-example";
string pascal2 = kebab.ToPascalCase();
// Result: "HelloWorldExample"

// Mixed case to PascalCase
string mixed = "hello World_Example";
string pascal3 = mixed.ToPascalCase();
// Result: "HelloWorldExample"
```

### Advanced Case Conversions
```csharp
public static class ExtendedStringHelper
{
    public static string ToCamelCase(this string str)
    {
        var pascal = str.ToPascalCase();
        return pascal.BeginByLowerCase();
    }

    public static string ToSnakeCase(this string str)
    {
        return string.Join("_", str.ToPascalCase()
            .Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString())
            .Select(s => s.ToLower()));
    }

    public static string ToKebabCase(this string str)
    {
        return str.ToSnakeCase().Replace("_", "-");
    }
}

// Usage
string text = "hello_world_example";
string camel = text.ToCamelCase();    // "helloWorldExample"
string snake = text.ToSnakeCase();    // "hello_world_example"
string kebab = text.ToKebabCase();    // "hello-world-example"
```

## HTTP Operations

### Basic HTTP GET
```csharp
// Get string content
string content = await "https://api.example.com/data".HttpGetStringAsync();
Console.WriteLine(content);

// Get stream
using var stream = await "https://api.example.com/data".HttpGetStreamAsync();
using var reader = new StreamReader(stream);
string content = await reader.ReadToEndAsync();
```

### Typed HTTP GET
```csharp
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

// Deserialize JSON response directly
var user = await "https://api.example.com/users/1".HttpGetAsync<User>();
Console.WriteLine($"User: {user.Name}, {user.Email}");

// With custom JSON settings
var settings = new JsonSerializerSettings
{
    PropertyNameCaseInsensitive = true
};
var user2 = await "https://api.example.com/users/1".HttpGetAsync<User>(settings);
```

### HTTP POST Operations
```csharp
public class CreateUserRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
}

var request = new CreateUserRequest
{
    Name = "John Doe",
    Email = "john@example.com"
};

// Send POST request
string response = await "https://api.example.com/users".HttpPostString(request);
Console.WriteLine(response);

// Typed POST with response
var createdUser = await "https://api.example.com/users"
    .HttpPost<CreateUserRequest, User, User>(request);
```

### Advanced HTTP Operations
```csharp
public static class HttpExtensions
{
    public static async Task<T> HttpGetWithRetry<T>(this string url, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await url.HttpGetAsync<T>();
            }
            catch (HttpRequestException) when (i < maxRetries - 1)
            {
                await Task.Delay(1000 * (i + 1)); // Exponential backoff
            }
        }
        throw new HttpRequestException($"Failed to fetch data after {maxRetries} attempts");
    }
}

// Usage
var data = await "https://api.example.com/data".HttpGetWithRetry<MyData>(3);
```

## Validation

### Email Validation
```csharp
string[] emails =
{
    "user@example.com",
    "invalid-email",
    "test@domain.co.uk",
    "not.an.email",
    "user+tag@example.com"
};

foreach (var email in emails)
{
    bool isValid = email.IsValidEmail();
    Console.WriteLine($"{email}: {(isValid ? "Valid" : "Invalid")}");
}
```

### Custom Validation Extensions
```csharp
public static class ValidationExtensions
{
    public static bool IsValidPhoneNumber(this string phone)
    {
        var pattern = @"^\+?[\d\s\-\(\)]+$";
        return Regex.IsMatch(phone, pattern);
    }

    public static bool IsValidUrl(this string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsNumeric(this string str)
    {
        return double.TryParse(str, out _);
    }
}
```

## Text Processing

### Word Analysis
```csharp
string text = "The quick brown fox jumps over the lazy dog";
string[] searchWords = { "quick", "brown", "lazy" };

// Check if contains all words
bool containsAll = text.ContainsAll(searchWords);
// Result: true

// Check if contains any word
bool containsAny = text.ContainsAny(new[] { "cat", "dog", "bird" });
// Result: true (contains "dog")
```

### String Combinations
```csharp
string[] prefixes = { "Mr.", "Mrs.", "Dr." };
string[] suffixes = { "Smith", "Johnson", "Williams" };

var combinations = prefixes.Concat(suffixes).MakeAllCombinations();
// Generates all possible combinations of prefixes and suffixes
```

### Byte Operations

> **Note:** Uses ASCII encoding. Characters outside U+0000–U+007F are silently replaced with `0x3F` (`'?'`). Use `Encoding.UTF8.GetBytes(str)` to preserve non-ASCII characters.

```csharp
string text = "Hello World";
byte[] bytes = text.GetBytes();
Console.WriteLine($"Bytes: {string.Join(", ", bytes)}");

// Human-readable byte sizes
long fileSize = 1536; // bytes
string readable = fileSize.GetBytesReadable();
// Result: "1.5 KB"

int largeSize = 1073741824; // bytes
string readable2 = largeSize.GetBytesReadable();
// Result: "1.0 GB"
```

## Usage Examples

### Text Search and Filtering
```csharp
public class TextSearchEngine
{
    public static List<string> SmartSearch(string query, IEnumerable<string> documents)
    {
        var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return documents
            .Where(doc => doc.ContainsAll(queryWords) ||
                         doc.CalculateSimilarity(query) > 0.7)
            .OrderByDescending(doc => doc.CalculateSimilarity(query))
            .ToList();
    }
}

// Usage
var documents = new[]
{
    "The quick brown fox jumps over the lazy dog",
    "A quick brown fox is fast",
    "The lazy dog sleeps all day",
    "Fast animals in the wild"
};

var results = TextSearchEngine.SmartSearch("quick fox", documents);
```

### Configuration Processing
```csharp
public class ConfigProcessor
{
    public static Dictionary<string, string> ProcessConfigKeys(Dictionary<string, string> config)
    {
        return config.ToDictionary(
            kvp => kvp.Key.ToPascalCase(),
            kvp => kvp.Value
        );
    }
}

// Usage
var config = new Dictionary<string, string>
{
    { "database_host", "localhost" },
    { "database_port", "5432" },
    { "api_key", "secret-key" }
};

var processed = ConfigProcessor.ProcessConfigKeys(config);
// Result: { "DatabaseHost": "localhost", "DatabasePort": "5432", "ApiKey": "secret-key" }
```

### HTTP Client Integration
```csharp
public class ApiClient
{
    public static async Task<T> GetDataAsync<T>(string endpoint)
    {
        var baseUrl = "https://api.example.com";
        var fullUrl = $"{baseUrl}/{endpoint}";

        return await fullUrl.HttpGetAsync<T>();
    }

    public static async Task<TResponse> PostDataAsync<TRequest, TResponse>(
        string endpoint, TRequest data)
    {
        var baseUrl = "https://api.example.com";
        var fullUrl = $"{baseUrl}/{endpoint}";

        return await fullUrl.HttpPost<TRequest, TResponse, TResponse>(data);
    }
}

// Usage
var users = await ApiClient.GetDataAsync<User[]>("users");
var newUser = await ApiClient.PostDataAsync<CreateUserRequest, User>("users", request);
```

## Performance Tips

### 1. String Similarity Optimization
```csharp
// Cache distance calculations for frequently compared strings
private static readonly Dictionary<(string, string), int> DistanceCache = new();

public static int CachedLevenshteinDistance(string source, string target)
{
    var key = (source, target);
    if (DistanceCache.TryGetValue(key, out var cached))
        return cached;

    var distance = source.ComputeLevenshteinDistance(target);
    DistanceCache[key] = distance;
    return distance;
}
```

### 2. HTTP Client Reuse
```csharp
// Reuse HttpClient instance (already done in StringHelper)
private static readonly HttpClient SharedHttpClient = new();

public static async Task<string> OptimizedHttpGet(string url)
{
    return await SharedHttpClient.GetStringAsync(url);
}
```

### 3. Batch Operations
```csharp
public static List<string> BatchPascalCase(IEnumerable<string> strings)
{
    return strings.Select(s => s.ToPascalCase()).ToList();
}
```

## Best Practices

### 1. Null Safety
```csharp
public static class SafeStringHelper
{
    public static string SafeToPascalCase(this string str)
    {
        return str?.ToPascalCase() ?? string.Empty;
    }

    public static double SafeCalculateSimilarity(this string source, string target)
    {
        if (source == null || target == null)
            return 0.0;

        return source.CalculateSimilarity(target);
    }
}
```

### 2. Error Handling
```csharp
public static class RobustStringHelper
{
    public static async Task<string> SafeHttpGetAsync(this string url)
    {
        try
        {
            return await url.HttpGetStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP request failed: {ex.Message}");
            return string.Empty;
        }
    }
}
```

### 3. Performance Monitoring
```csharp
public static class InstrumentedStringHelper
{
    public static double MeasuredCalculateSimilarity(this string source, string target)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = source.CalculateSimilarity(target);
        stopwatch.Stop();

        Console.WriteLine($"Similarity calculation took: {stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}
```

### 4. Configuration
```csharp
public static class ConfigurableStringHelper
{
    public static double SimilarityThreshold { get; set; } = 0.6;

    public static bool IsSimilar(this string source, string target)
    {
        return source.CalculateSimilarity(target) >= SimilarityThreshold;
    }
}
```

---

[← Back to Main Documentation](../README.md)

## HTTP Security — SSRF Protection

The `ValidateUrl` helper guards against Server-Side Request Forgery (SSRF) by blocking
requests to internal / non-routable address ranges before any network call is made.

**Blocked IP ranges:**

- Loopback: `127.0.0.1`, `::1`, `localhost`
- RFC 1918: `10.x.x.x`, `172.16–31.x.x`, `192.168.x.x`
- APIPA / link-local: `169.254.x.x`
- IPv6 ULA: `fc00::/7` (includes `fd…`)
- IPv6 link-local: `fe80::/10`
- Wildcard: `0.0.0.0`
- CGNAT (RFC 6598): `100.64.0.0/10`
