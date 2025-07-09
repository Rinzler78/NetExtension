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
            {
                // Try to parse the string to the target enum type
                if (Enum.TryParse<ToEnumType>(fromEnumString, true, out ToEnumType result) && Enum.IsDefined(typeof(ToEnumType), result))
                    return result;
            }
        }
        catch (Exception)
        {
        }
        // Return first value of ToEnumType as default
        var values = Enum.GetValues(typeof(ToEnumType));
        var first = values.Length > 0 ? values.GetValue(0) : null;
        if (first != null)
            return (ToEnumType)first;
        return default;
    }

    public static ToEnumType Convert<ToEnumType>(this string str)
        where ToEnumType : struct
    {
        try
        {
            if (Enum.TryParse(str, true, out ToEnumType result) && Enum.IsDefined(typeof(ToEnumType), result))
                return result;
        }
        catch (Exception)
        {
        }
        // Return first value of ToEnumType as default
        var values = Enum.GetValues(typeof(ToEnumType));
        var first = values.Length > 0 ? values.GetValue(0) : null;
        if (first != null)
            return (ToEnumType)first;
        return default;
    }

    public static EnumType? Parse<EnumType>(this string str)
        where EnumType : Enum
    {
        return GlobalEnumParser.Parse<EnumType>(str);
    }
}