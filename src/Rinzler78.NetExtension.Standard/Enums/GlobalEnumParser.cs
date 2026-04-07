using System;
using System.Collections.Concurrent;
using Rinzler78.NetExtension.Types;

namespace Rinzler78.NetExtension.Enums;

/// <summary>
/// Provides a thread-safe, cached global enum parser for all enum types.
/// </summary>
public static class GlobalEnumParser
{
    private static readonly ConcurrentDictionary<Type, object> EnumParsersDictionary = new();

    /// <summary>
    /// Parses a string to the specified enum type, caching the parser for reuse.
    /// </summary>
    /// <typeparam name="EnumType">The enum type to parse to.</typeparam>
    /// <param name="str">The string to parse.</param>
    /// <returns>The matched enum value, or <c>default</c> if no match is found.</returns>
    public static EnumType? Parse<EnumType>(string str)
        where EnumType : Enum
    {
        var parser = (EnumParser<EnumType>)EnumParsersDictionary.GetOrAdd(
            typeof(EnumType),
            static _ => typeof(EnumParser<EnumType>).CreateInstance<EnumParser<EnumType>>()!);

        return parser.Parse(str);
    }
}
