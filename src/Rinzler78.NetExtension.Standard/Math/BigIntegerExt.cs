using System;
using System.Numerics;

namespace Rinzler78.NetExtension.Math;

/// <summary>
/// Provides extension methods for <see cref="BigInteger"/> with overflow-safe conversions.
/// </summary>
public static class BigIntegerExt
{
    /// <summary>
    /// Converts a BigInteger to a ulong value with overflow protection.
    /// </summary>
    /// <param name="bigInteger">The BigInteger value to convert.</param>
    /// <returns>The converted ulong value, or the appropriate boundary value if overflow occurs.</returns>
    public static ulong ToULong(this BigInteger bigInteger)
    {
        if (bigInteger < ulong.MinValue)
            return ulong.MinValue;

        if (bigInteger > ulong.MaxValue)
            return ulong.MaxValue;

        return (ulong)bigInteger;
    }

    /// <summary>
    /// Converts a BigInteger to a double value with overflow protection.
    /// </summary>
    /// <param name="bigInteger">The BigInteger value to convert.</param>
    /// <returns>The converted double value, or the appropriate boundary value if overflow occurs.</returns>
    public static double ToDouble(this BigInteger bigInteger)
    {
        var value = (double)bigInteger;
        if (double.IsInfinity(value))
            return bigInteger < 0 ? double.MinValue : double.MaxValue;

        return value;
    }

    /// <summary>
    /// Converts a BigInteger to a decimal value with overflow protection.
    /// </summary>
    /// <param name="bigInteger">The BigInteger value to convert.</param>
    /// <returns>The converted decimal value, or the appropriate boundary value if overflow occurs.</returns>
    public static decimal ToDecimal(this BigInteger bigInteger)
    {
        var decimalMin = new BigInteger(decimal.MinValue);
        if (bigInteger < decimalMin)
            return decimal.MinValue;

        var decimalMax = new BigInteger(decimal.MaxValue);
        if (bigInteger > decimalMax)
            return decimal.MaxValue;

        return (decimal)bigInteger;
    }
}
