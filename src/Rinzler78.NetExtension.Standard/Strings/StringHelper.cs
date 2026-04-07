using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rinzler78.NetExtension.Strings;

/// <summary>
/// Provides utility methods for string manipulation, validation, formatting, and HTTP operations.
/// </summary>
public static partial class StringHelper
{
    #region Constants

    /// <summary>
    /// Default timeout for HTTP operations in seconds.
    /// </summary>
    private const int DefaultHttpTimeoutSeconds = 30;

    /// <summary>
    /// Binary division factor for byte calculations.
    /// </summary>
    private const int BinaryDivisionFactor = 1024;

    /// <summary>
    /// Byte size thresholds for human-readable formatting.
    /// </summary>
    private const long ExabyteThreshold = 0x1000000000000000;
    private const long PetabyteThreshold = 0x4000000000000;
    private const long TerabyteThreshold = 0x10000000000;
    private const long GigabyteThreshold = 0x40000000;
    private const long MegabyteThreshold = 0x100000;
    private const long KilobyteThreshold = 0x400;

    /// <summary>
    /// Bit shift values for byte size calculations.
    /// </summary>
    private const int ExabyteShift = 50;
    private const int PetabyteShift = 40;
    private const int TerabyteShift = 30;
    private const int GigabyteShift = 20;
    private const int MegabyteShift = 10;

    /// <summary>
    /// Network security constants for IP address validation.
    /// </summary>
    private const string LocalhostIpv4 = "127.0.0.1";
    private const string LocalhostIpv6 = "::1";
    private const string LocalhostName = "localhost";

    /// <summary>
    /// Private IP address range constants (RFC 1918).
    /// </summary>
    private const byte ClassAPrivateFirstOctet = 10;
    private const byte ClassBPrivateFirstOctet = 172;
    private const byte ClassBPrivateSecondOctetMin = 16;
    private const byte ClassBPrivateSecondOctetMax = 31;
    private const byte ClassCPrivateFirstOctet = 192;
    private const byte ClassCPrivateSecondOctet = 168;

    /// <summary>
    /// String similarity bounds.
    /// </summary>
    private const double MaxSimilarity = 1.0;
    private const double MinSimilarity = 0.0;

    #endregion

    /// <summary>
    /// Shared instance of EmailAddressAttribute for validation to avoid repeated instantiation.
    /// </summary>
    private static readonly EmailAddressAttribute EmailAddressValidator = new();

    /// <summary>
    /// Validates if the string matches a valid email address format using built-in validation attributes.
    /// </summary>
    /// <param name="str">The string to validate</param>
    /// <returns>True if the string is a valid email format, false otherwise</returns>
    /// <example>
    /// <code>
    /// bool isValid = "test@example.com".IsValidEmail(); // true
    /// bool isInvalid = "invalid-email".IsValidEmail(); // false
    /// </code>
    /// </example>
    public static bool IsValidEmail(this string str)
    {
        if (str == null)
            return false;
        return EmailAddressValidator.IsValid(str);
    }

    /// <summary>
    /// Computes the Levenshtein distance between two strings, which represents the minimum number
    /// of single-character edits (insertions, deletions, or substitutions) required to change
    /// one string into another.
    /// </summary>
    /// <param name="source">The source string to compare from</param>
    /// <param name="target">The target string to compare to</param>
    /// <returns>The Levenshtein distance as an integer</returns>
    /// <example>
    /// <code>
    /// int distance = "kitten".ComputeLevenshteinDistance("sitting"); // returns 3
    /// int distance = "hello".ComputeLevenshteinDistance("hello"); // returns 0
    /// </code>
    /// </example>
    public static int ComputeLevenshteinDistance(this string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return string.IsNullOrEmpty(target) ? 0 : target.Length;

        if (string.IsNullOrEmpty(target))
            return string.IsNullOrEmpty(source) ? 0 : source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;

        var distance = new int[sourceLength + 1, targetLength + 1];

        // Step 1
        for (var i = 0; i <= sourceLength; distance[i, 0] = i++)
        {
        }

        for (var j = 0; j <= targetLength; distance[0, j] = j++)
        {
        }

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                // Step 2
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;

                // Step 3
                distance[i, j] = System.Math.Min(
                    System.Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[sourceLength, targetLength];
    }

    /// <summary>
    /// Calculates the similarity between two strings based on Levenshtein distance,
    /// returning a value between 0 (completely different) and 1 (identical).
    /// </summary>
    /// <param name="source">The source string to compare from</param>
    /// <param name="target">The target string to compare to</param>
    /// <returns>A similarity score between 0.0 and 1.0, where 1.0 indicates identical strings</returns>
    /// <example>
    /// <code>
    /// double similarity = "hello".CalculateSimilarity("hello"); // returns 1.0
    /// double similarity = "cat".CalculateSimilarity("dog"); // returns a value closer to 0.0
    /// </code>
    /// </example>
    public static double CalculateSimilarity(this string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return string.IsNullOrEmpty(target) ? MaxSimilarity : MinSimilarity;

        if (string.IsNullOrEmpty(target))
            return string.IsNullOrEmpty(source) ? MaxSimilarity : MinSimilarity;

        double stepsToSame = ComputeLevenshteinDistance(source, target);
        return MaxSimilarity - stepsToSame / System.Math.Max(source.Length, target.Length);
    }

    /// <summary>
    /// Determines whether the string contains all of the specified words using ordinal comparison.
    /// </summary>
    /// <param name="str">The string to search within</param>
    /// <param name="words">Array of words that must all be present in the string</param>
    /// <returns>True if all words are found in the string, false otherwise. Returns true if words array is null or empty</returns>
    /// <example>
    /// <code>
    /// bool result = "Hello world test".ContainsAll(new[] { "Hello", "world" }); // true
    /// bool result = "Hello world".ContainsAll(new[] { "Hello", "missing" }); // false
    /// </code>
    /// </example>
    public static bool ContainsAll(this string str, string[] words)
    {
        return words?.All(arg => str.Contains(arg, StringComparison.Ordinal)) ?? true;
    }

    /// <summary>
    /// Determines whether the string contains any of the specified words using ordinal comparison.
    /// </summary>
    /// <param name="str">The string to search within</param>
    /// <param name="words">Array of words to search for</param>
    /// <returns>True if any word is found in the string, false otherwise. Returns true if words array is null or empty</returns>
    /// <example>
    /// <code>
    /// bool result = "Hello world".ContainsAny(new[] { "Hello", "missing" }); // true
    /// bool result = "Hello world".ContainsAny(new[] { "missing", "absent" }); // false
    /// </code>
    /// </example>
    public static bool ContainsAny(this string str, string[] words)
    {
        if (words == null)
            return true;
        if (words.Length == 0)
            return true;
        return words.Any(arg => str.Contains(arg, StringComparison.Ordinal));
    }

    /// <summary>
    /// Converts the first character of the string to lowercase while preserving the rest of the string.
    /// </summary>
    /// <param name="str">The string to modify</param>
    /// <returns>A new string with the first character in lowercase</returns>
    /// <example>
    /// <code>
    /// string result = "Hello World".BeginByLowerCase(); // returns "hello World"
    /// string result = "XML".BeginByLowerCase(); // returns "xML"
    /// </code>
    /// </example>
    public static string BeginByLowerCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        if (str.Length == 1)
            return char.ToLower(str[0], CultureInfo.InvariantCulture).ToString();

        return string.Create(str.Length, str, (chars, state) =>
        {
            chars[0] = char.ToLower(state[0], CultureInfo.InvariantCulture);
            state.AsSpan(1).CopyTo(chars[1..]);
        });
    }

    /// <summary>
    /// Converts the first character of the string to uppercase while preserving the rest of the string.
    /// </summary>
    /// <param name="str">The string to modify</param>
    /// <returns>A new string with the first character in uppercase</returns>
    /// <example>
    /// <code>
    /// string result = "hello world".BeginByUpperCase(); // returns "Hello world"
    /// string result = "xml".BeginByUpperCase(); // returns "Xml"
    /// </code>
    /// </example>
    public static string BeginByUpperCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        if (str.Length == 1)
            return char.ToUpper(str[0], CultureInfo.InvariantCulture).ToString();

        return string.Create(str.Length, str, (chars, state) =>
        {
            chars[0] = char.ToUpper(state[0], CultureInfo.InvariantCulture);
            state.AsSpan(1).CopyTo(chars[1..]);
        });
    }

    /// <summary>
    /// Converts the entire string to lowercase and then capitalizes the first character.
    /// </summary>
    /// <param name="str">The string to modify</param>
    /// <returns>A new string with all characters in lowercase except the first character which is uppercase</returns>
    /// <example>
    /// <code>
    /// string result = "HELLO WORLD".ToStartByUpperCase(); // returns "Hello world"
    /// string result = "XML Parser".ToStartByUpperCase(); // returns "Xml parser"
    /// </code>
    /// </example>
    public static string ToStartByUpperCase(this string str)
    {
        return str.ToLower(CultureInfo.InvariantCulture).BeginByUpperCase();
    }

    /// <summary>
    /// Converts a string to PascalCase format by capitalizing the first letter of each word
    /// and removing non-alphanumeric characters. Also handles transitions from digits to letters.
    /// </summary>
    /// <param name="str">The string to convert</param>
    /// <returns>A PascalCase formatted string with non-alphanumeric characters removed</returns>
    /// <example>
    /// <code>
    /// string result = "hello world".ToPascalCase(); // returns "HelloWorld"
    /// string result = "user_name".ToPascalCase(); // returns "UserName"
    /// string result = "item123count".ToPascalCase(); // returns "Item123Count"
    /// </code>
    /// </example>
    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        var sb = new StringBuilder();
        bool newWord = true;
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (!char.IsLetterOrDigit(c))
            {
                newWord = true;
                continue;
            }
            if (newWord)
            {
                sb.Append(char.ToUpper(c, CultureInfo.InvariantCulture));
                newWord = false;
            }
            else if (i > 0 && char.IsDigit(str[i - 1]) && char.IsLetter(c))
            {
                sb.Append(char.ToUpper(c, CultureInfo.InvariantCulture));
            }
            else
            {
                sb.Append(char.ToLower(c, CultureInfo.InvariantCulture));
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Creates all possible combinations by concatenating each string with every other string in the collection,
    /// including combinations with itself.
    /// </summary>
    /// <param name="allStrings">The collection of strings to combine</param>
    /// <returns>An enumerable of all possible string combinations</returns>
    /// <example>
    /// <code>
    /// var input = new[] { "A", "B" };
    /// var combinations = input.MakeAllCombinations(); // returns ["AA", "AB", "BA", "BB"]
    /// </code>
    /// </example>
    public static IEnumerable<string> MakeAllCombinations(this IEnumerable<string> allStrings)
    {
        if (allStrings == null)
            throw new ArgumentNullException(nameof(allStrings));

        var rightStrs = allStrings as string[] ?? allStrings.ToArray();
        var list = new List<string>(rightStrs.Length * rightStrs.Length);

        foreach (var leftStr in rightStrs)
        {
            foreach (var rightStr in rightStrs)
            {
                // Use string.Concat for better performance than string interpolation
                list.Add(string.Concat(leftStr, rightStr));
            }
        }

        return list;
    }

    /// <summary>
    /// Determines whether the specified string is null or an empty string.
    /// This is an extension method wrapper around string.IsNullOrEmpty.
    /// </summary>
    /// <param name="str">The string to test</param>
    /// <returns>True if the string is null or empty, false otherwise</returns>
    /// <example>
    /// <code>
    /// bool result = "".IsNullOrEmpty(); // true
    /// bool result = "hello".IsNullOrEmpty(); // false
    /// string nullStr = null;
    /// bool result = nullStr.IsNullOrEmpty(); // true
    /// </code>
    /// </example>
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// Concatenates the elements of a string array using the specified separator character.
    /// </summary>
    /// <param name="strs">Array of strings to join</param>
    /// <param name="separator">The character to use as separator</param>
    /// <returns>A string containing the joined elements separated by the specified character</returns>
    /// <example>
    /// <code>
    /// string result = new[] { "apple", "banana", "cherry" }.Join(','); // returns "apple,banana,cherry"
    /// string result = new[] { "one", "two" }.Join('|'); // returns "one|two"
    /// </code>
    /// </example>
    public static string Join(this string[] strs, char separator)
    {
        return string.Join(separator, strs);
    }

    /// <summary>
    /// Converts an object to JSON-formatted StringContent suitable for HTTP requests.
    /// The content is encoded as UTF-8 with application/json media type.
    /// </summary>
    /// <param name="obj">The object to serialize and convert to StringContent</param>
    /// <returns>A StringContent instance containing the JSON-serialized object</returns>
    /// <example>
    /// <code>
    /// var data = new { Name = "John", Age = 30 };
    /// var content = data.GetStringContent();
    /// // Use content in HTTP POST/PUT requests
    /// </code>
    /// </example>
    public static StringContent GetStringContent(this object obj)
    {
        var jsonContent = JsonConvert.SerializeObject(obj);
        var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        return contentString;
    }

    /// <summary>
    /// Formats a JSON string with proper indentation for improved readability.
    /// </summary>
    /// <param name="jsonString">The JSON string to format</param>
    /// <returns>A formatted JSON string with indentation</returns>
    /// <example>
    /// <code>
    /// string compact = "{\"name\":\"John\",\"age\":30}";
    /// string formatted = compact.ToJsonFormattedString();
    /// // Returns:
    /// // {
    /// //   "name": "John",
    /// //   "age": 30
    /// // }
    /// </code>
    /// </example>
    public static string ToJsonFormattedString(this string jsonString)
    {
        var jt = JToken.Parse(jsonString);
        return jt.ToString(Formatting.Indented);
    }

    /// <summary>
    /// Converts an integer representing bytes to a human-readable string format (B, KB, MB, GB, etc.).
    /// </summary>
    /// <param name="i">The number of bytes as an integer</param>
    /// <returns>A human-readable string representation of the byte size</returns>
    /// <example>
    /// <code>
    /// string result = 1024.GetBytesReadable(); // returns "1 KB"
    /// string result = 1048576.GetBytesReadable(); // returns "1 MB"
    /// </code>
    /// </example>
    public static string GetBytesReadable(this int i)
    {
        return ((long)i).GetBytesReadable();
    }

    /// <summary>
    /// Converts a long integer representing bytes to a human-readable string format with appropriate
    /// unit suffix (B, KB, MB, GB, TB, PB, EB). Uses binary calculation (1024-based).
    /// </summary>
    /// <param name="i">The number of bytes as a long integer</param>
    /// <returns>A human-readable string representation of the byte size with up to 3 decimal places</returns>
    /// <example>
    /// <code>
    /// string result = 1024L.GetBytesReadable(); // returns "1 KB"
    /// string result = 1073741824L.GetBytesReadable(); // returns "1 GB"
    /// string result = 1536L.GetBytesReadable(); // returns "1.5 KB"
    /// </code>
    /// </example>
    public static string GetBytesReadable(this long i)
    {
        // Get absolute value
        var absoluteI = i < 0 ? -i : i;
        // Determine the suffix and readable value
        string suffix;
        double readable;
        if (absoluteI >= ExabyteThreshold) // Exabyte
        {
            suffix = "EB";
            readable = i >> ExabyteShift;
        }
        else if (absoluteI >= PetabyteThreshold) // Petabyte
        {
            suffix = "PB";
            readable = i >> PetabyteShift;
        }
        else if (absoluteI >= TerabyteThreshold) // Terabyte
        {
            suffix = "TB";
            readable = i >> TerabyteShift;
        }
        else if (absoluteI >= GigabyteThreshold) // Gigabyte
        {
            suffix = "GB";
            readable = i >> GigabyteShift;
        }
        else if (absoluteI >= MegabyteThreshold) // Megabyte
        {
            suffix = "MB";
            readable = i >> MegabyteShift;
        }
        else if (absoluteI >= KilobyteThreshold) // Kilobyte
        {
            suffix = "KB";
            readable = i;
        }
        else
        {
            return i.ToString("0 B", CultureInfo.InvariantCulture); // Byte
        }

        // Divide by 1024 to get fractional value
        readable /= BinaryDivisionFactor;
        // Return formatted number with suffix
        return readable.ToString("0.### ", CultureInfo.InvariantCulture) + suffix;
    }

    /// <summary>
    /// Converts a string to its ASCII byte array representation.
    /// </summary>
    /// <param name="str">The string to encode.</param>
    /// <returns>
    /// A byte array containing the ASCII-encoded bytes of the string.
    /// Characters outside the ASCII range (U+0000–U+007F) are replaced
    /// with <c>0x3F</c> (<c>'?'</c>) silently.
    /// </returns>
    /// <remarks>
    /// Use <see cref="System.Text.Encoding.UTF8"/> if you need to preserve
    /// non-ASCII characters (accented letters, CJK, emoji, etc.).
    /// </remarks>
    /// <example>
    /// <code>
    /// byte[] bytes = "Hello".GetBytes();  // [72, 101, 108, 108, 111]
    /// byte[] lossy = "Héllo".GetBytes(); // 'é' (U+00E9) becomes 0x3F → [72, 63, 108, 108, 111]
    /// </code>
    /// </example>
    public static byte[] GetBytes(this string str)
    {
        return Encoding.ASCII.GetBytes(str);
    }

    /// <summary>
    /// Gets the file extension from a file path using efficient span operations.
    /// </summary>
    /// <param name="filePath">The file path to extract extension from</param>
    /// <returns>The file extension including the dot, or empty string if no extension found</returns>
    /// <example>
    /// <code>
    /// string ext = "document.pdf".GetFileExtensionOptimized(); // returns ".pdf"
    /// string ext = "filename".GetFileExtensionOptimized(); // returns ""
    /// </code>
    /// </example>
    public static string GetFileExtensionOptimized(this string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        var span = filePath.AsSpan();
        var lastDotIndex = span.LastIndexOf('.');
        var lastSlashIndex = System.Math.Max(span.LastIndexOf('/'), span.LastIndexOf('\\'));

        // If dot is after the last slash (or no slash found) and not at the beginning of filename, we have an extension
        if (lastDotIndex > lastSlashIndex && lastDotIndex < span.Length - 1 && lastDotIndex > 0)
        {
            // Make sure it's not a hidden file at the start of filename (like ".hidden" without real extension)
            var filenameStart = lastSlashIndex + 1;
            if (lastDotIndex == filenameStart)
                return string.Empty; // This is a hidden file without extension like ".hidden"

            return span[lastDotIndex..].ToString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Efficiently checks if a string starts with any of the specified prefixes using span operations.
    /// </summary>
    /// <param name="str">The string to check</param>
    /// <param name="prefixes">Array of prefixes to check against</param>
    /// <param name="comparisonType">The string comparison type to use</param>
    /// <returns>True if the string starts with any of the prefixes, false otherwise</returns>
    /// <example>
    /// <code>
    /// bool result = "https://example.com".StartsWithAnyOptimized(new[] { "http://", "https://" }); // true
    /// </code>
    /// </example>
    public static bool StartsWithAnyOptimized(this string str, string[] prefixes, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (string.IsNullOrEmpty(str) || prefixes == null || prefixes.Length == 0)
            return false;

        var span = str.AsSpan();
        foreach (var prefix in prefixes)
        {
            if (span.StartsWith(prefix.AsSpan(), comparisonType))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Efficiently trims whitespace and specified characters from both ends of a string using span operations.
    /// </summary>
    /// <param name="str">The string to trim</param>
    /// <param name="trimChars">Additional characters to trim (whitespace is always trimmed)</param>
    /// <returns>A new string with specified characters trimmed from both ends</returns>
    /// <example>
    /// <code>
    /// string result = "  Hello World!!!  ".TrimOptimized('!'); // returns "Hello World"
    /// </code>
    /// </example>
    public static string TrimOptimized(this string str, params char[] trimChars)
    {
        if (str == null)
            return string.Empty;

        if (string.IsNullOrEmpty(str))
            return str;

        var span = str.AsSpan();

        // Create combined array of whitespace and custom chars
        var allTrimChars = trimChars?.Length > 0
            ? new char[trimChars.Length + 6]
            : new char[6];

        // Add common whitespace characters
        allTrimChars[0] = ' ';
        allTrimChars[1] = '\t';
        allTrimChars[2] = '\n';
        allTrimChars[3] = '\r';
        allTrimChars[4] = '\v';
        allTrimChars[5] = '\f';

        // Add custom characters
        if (trimChars?.Length > 0)
        {
            System.Array.Copy(trimChars, 0, allTrimChars, 6, trimChars.Length);
        }

        var trimmed = span.Trim(allTrimChars);
        return trimmed.Length == str.Length ? str : trimmed.ToString();
    }
}
