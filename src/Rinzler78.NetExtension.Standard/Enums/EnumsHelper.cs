using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Rinzler78.NetExtension.Measure;
using Rinzler78.NetExtension.Strings;

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
            .AsParallel()
            .SelectMany(v =>
            {
                var lst = new List<(string, Enumtype)>();

                lst.Add((((int)(object)v).ToString(), v));

                var vAsString = v.ToString().ToLower();

                lst.Add((vAsString, v));

                vAsString = vAsString.Replace("_", ".");

                if (!lst.AsParallel().Any(arg => arg.Item1 == vAsString))
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