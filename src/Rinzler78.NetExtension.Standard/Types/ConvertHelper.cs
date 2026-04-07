using System;
using System.Globalization;

namespace Rinzler78.NetExtension.Types;

/// <summary>
/// Provides generic type conversion extension methods using <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>.
/// </summary>
public static class ConvertHelper
{
    /// <summary>
    /// Converts the value to the specified type using invariant culture formatting.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    public static T ConvertValue<T>(this object value)
    {
        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
    }
}
