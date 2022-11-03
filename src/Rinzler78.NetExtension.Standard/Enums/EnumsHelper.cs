using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace Rinzler78.NetExtension.Enums;

public static class EnumsHelper
{
    public static ToEnumType Convert<FromEnumType, ToEnumType>(this FromEnumType obj)
        where FromEnumType : struct
        where ToEnumType : struct
    {
        try
        {
            var fromEnumString = obj.ToString();
            return fromEnumString.Convert<ToEnumType>();
        }
        catch (Exception ex)
        {
        }

        return default;
    }

    public static ToEnumType Convert<ToEnumType>(this string str)
        where ToEnumType : struct
    {
        try
        {
            ToEnumType result = default;

            if (Enum.TryParse(str, out result))
                return result;
        }
        catch (Exception ex)
        {
        }

        return default;
    }

    public static Enumtype Parse<Enumtype>(this string str)
        where Enumtype : Enum
    {
        return EnumParser<Enumtype>.Parse(str);
    }
}

public static class EnumParser<Enumtype>
    where Enumtype : Enum
{
    public static readonly ReadOnlyDictionary<string, Enumtype> EnumValuesDictionary = new(Enum
            .GetValues(typeof(Enumtype))
            .Cast<Enumtype>()
            .SelectMany(v =>
                new[] {
                                (n: v.ToString().ToLower(), v),
                                (n: ((int)(object)v).ToString(), v)
            })
            .ToDictionary(i => i.n, i => i.v, StringComparer.OrdinalIgnoreCase));

    public static Enumtype Parse(string str)
    {
        try
        {
            return EnumValuesDictionary[str];
            //return EnumValuesDictionary.AsParallel().First(kvp => string.Equals(kvp.Key, str, StringComparison.InvariantCultureIgnoreCase)).Value;
        }
        catch
        {
            Console.WriteLine($"{typeof(Enumtype).Name} cannot be parse using {str}");
        }

        return default;
    }
}