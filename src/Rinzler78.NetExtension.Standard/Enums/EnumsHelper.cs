using System;
using System.Drawing;
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