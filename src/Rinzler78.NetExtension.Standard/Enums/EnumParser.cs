using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Rinzler78.NetExtension.Enums;

public sealed class EnumParser<Enumtype> where Enumtype : Enum
{
    public readonly ReadOnlyDictionary<string, Enumtype> EnumValuesDictionary = new(Enum
        .GetValues(typeof(Enumtype))
        .Cast<Enumtype>()
        .AsParallel()
        .SelectMany(enumValue =>
        {
            var result = new List<(string, Enumtype)>
            {
                (((int)(object)enumValue).ToString(CultureInfo.InvariantCulture), enumValue)
            };

            var enumValueAsString = enumValue.ToString().ToLower(CultureInfo.InvariantCulture);

            result.Add((enumValueAsString, enumValue));

            enumValueAsString = enumValueAsString.Replace("_", ".", StringComparison.Ordinal);

            if (!result.AsParallel().Any(arg => string.Equals(arg.Item1, enumValueAsString, StringComparison.Ordinal)))
                result.Add((enumValueAsString, enumValue));

            return result;
        })
        .ToDictionary(i => i.Item1, i => i.Item2, StringComparer.OrdinalIgnoreCase));

    public Enumtype? Parse(string str)
    {
        try
        {
            return EnumValuesDictionary[str];
        }
        catch
        {
            Console.WriteLine($"{typeof(Enumtype).Name} cannot be parse using {str}");
        }

        return default;
    }
}
