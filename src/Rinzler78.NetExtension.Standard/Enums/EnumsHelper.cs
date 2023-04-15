using System;

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
            if (fromEnumString is not null)
                return fromEnumString.Convert<ToEnumType>();
        }
        catch (Exception)
        {
        }

        return default;
    }

    public static ToEnumType Convert<ToEnumType>(this string str)
        where ToEnumType : struct
    {
        try
        {
            if (Enum.TryParse(str, out ToEnumType result))
                return result;
        }
        catch (Exception)
        {
        }

        return default;
    }

    public static EnumType? Parse<EnumType>(this string str)
        where EnumType : Enum
    {
        return GlobalEnumParser.Parse<EnumType>(str);
    }
}