using System;
using System.Collections.Concurrent;
using Rinzler78.NetExtension.Types;

namespace Rinzler78.NetExtension.Enums;

public static class GlobalEnumParser
{
    private static readonly ConcurrentDictionary<Type, object> EnumParsersDictionary = new();

    public static EnumType? Parse<EnumType>(string str)
        where EnumType : Enum
    {
        var parser = (EnumParser<EnumType>)EnumParsersDictionary.GetOrAdd(
            typeof(EnumType),
            static _ => typeof(EnumParser<EnumType>).CreateInstance<EnumParser<EnumType>>()!);

        return parser.Parse(str);
    }
}
