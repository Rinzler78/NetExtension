using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Rinzler78.NetExtension.Enums;

public sealed class EnumParser<Enumtype> where Enumtype : Enum
{
    public IReadOnlyDictionary<string, Enumtype> EnumValuesDictionary { get; } = new ReadOnlyDictionary<string, Enumtype>(Enum
        .GetValues(typeof(Enumtype))
        .Cast<Enumtype>()
        .SelectMany(GetAliases)
        .ToDictionary(i => i.Item1, i => i.Item2, StringComparer.OrdinalIgnoreCase));

    public Enumtype? Parse(string str)
    {
        if (str is null)
            return default;

        return EnumValuesDictionary.TryGetValue(str, out var value)
            ? value
            : default;
    }

    private static IEnumerable<(string, Enumtype)> GetAliases(Enumtype enumValue)
    {
        var aliases = new List<(string, Enumtype)>
        {
            (GetNumericKey(enumValue), enumValue)
        };

        var enumValueAsString = enumValue.ToString().ToLower(CultureInfo.InvariantCulture);
        aliases.Add((enumValueAsString, enumValue));

        var dottedAlias = enumValueAsString.Replace("_", ".", StringComparison.Ordinal);
        if (!aliases.Any(arg => string.Equals(arg.Item1, dottedAlias, StringComparison.Ordinal)))
            aliases.Add((dottedAlias, enumValue));

        return aliases;
    }

    private static string GetNumericKey(Enumtype enumValue)
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(Enumtype));

        return underlyingType == typeof(sbyte) ||
               underlyingType == typeof(short) ||
               underlyingType == typeof(int) ||
               underlyingType == typeof(long)
            ? Convert.ToInt64(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
            : Convert.ToUInt64(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
    }
}
