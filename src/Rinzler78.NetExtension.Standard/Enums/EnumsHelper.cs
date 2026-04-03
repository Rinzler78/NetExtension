using System;

namespace Rinzler78.NetExtension.Enums;

public static class EnumsHelper
{
    /// <summary>
    /// Converts an enum value from one type to another by name matching.
    /// </summary>
    /// <typeparam name="FromEnumType">The source enum type.</typeparam>
    /// <typeparam name="ToEnumType">The target enum type.</typeparam>
    /// <param name="obj">The enum value to convert.</param>
    /// <returns>The converted enum value, or the first value of the target enum if conversion fails.</returns>
    public static ToEnumType Convert<FromEnumType, ToEnumType>(this FromEnumType obj)
        where FromEnumType : struct, Enum
        where ToEnumType : struct, Enum
    {
        var fromEnumString = obj.ToString();
        if (Enum.TryParse(fromEnumString, true, out ToEnumType result) && Enum.IsDefined(typeof(ToEnumType), result))
            return result;

        return Enum.GetValues<ToEnumType>()[0];
    }

    /// <summary>
    /// Converts a string to an enum value by name matching.
    /// </summary>
    /// <typeparam name="ToEnumType">The target enum type.</typeparam>
    /// <param name="str">The string to convert.</param>
    /// <returns>The converted enum value, or the first value of the target enum if conversion fails.</returns>
    public static ToEnumType Convert<ToEnumType>(this string str)
        where ToEnumType : struct, Enum
    {
        if (str != null && Enum.TryParse(str, true, out ToEnumType result) && Enum.IsDefined(typeof(ToEnumType), result))
            return result;

        return Enum.GetValues<ToEnumType>()[0];
    }

    public static EnumType? Parse<EnumType>(this string str)
        where EnumType : Enum
    {
        return GlobalEnumParser.Parse<EnumType>(str);
    }
}
