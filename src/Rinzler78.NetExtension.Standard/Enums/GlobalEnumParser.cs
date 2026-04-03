using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ObjectiveC;
using Rinzler78.NetExtension.Types;

namespace Rinzler78.NetExtension.Enums;

public static class GlobalEnumParser
{
    private static readonly Dictionary<string, object?> EnumParsersDictionary = new(StringComparer.OrdinalIgnoreCase);

    public static EnumType? Parse<EnumType>(string str)
        where EnumType : Enum
    {
        EnumParser<EnumType>? parser = null;

        lock (EnumParsersDictionary)
        {
            var parserName = typeof(EnumType).Name;

            if (!EnumParsersDictionary.TryGetValue(typeof(EnumType).Name, out var parserObject))
                EnumParsersDictionary[parserName] = parserObject = typeof(EnumParser<EnumType>).CreateInstance<EnumParser<EnumType>>();

            parser = parserObject as EnumParser<EnumType>;
        }

        if (parser is not null)
            return parser.Parse(str);

        return default;
    }
}
