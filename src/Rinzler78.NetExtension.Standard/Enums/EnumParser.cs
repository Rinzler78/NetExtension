using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rinzler78.NetExtension.Enums;

public static class EnumParser<Enumtype>
    where Enumtype : Enum
{
    public static readonly ReadOnlyDictionary<string, Enumtype> EnumValuesDictionary = new(Enum
        .GetValues(typeof(Enumtype))
        .Cast<Enumtype>()
        .AsParallel()
        .SelectMany(v =>
        {
            var lst = new List<(string, Enumtype)>();

            lst.Add((((int)(object)v).ToString(), v));

            var vAsString = v.ToString().ToLower();

            lst.Add((vAsString, v));

            vAsString = vAsString.Replace("_", ".", StringComparison.Ordinal);

            if (!lst.AsParallel().Any(arg => string.Equals(arg.Item1, vAsString, StringComparison.Ordinal)))
            {
                lst.Add((vAsString, v));
            }

            return lst;
        })
        .ToDictionary(i => i.Item1, i => i.Item2, StringComparer.OrdinalIgnoreCase));

    public static Enumtype Parse(string str)
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