using System;

namespace Rinzler78.NetExtension.Math;

/// <summary>
/// Provides mathematical helper methods for <see cref="decimal"/> values, including power and logarithm operations.
/// </summary>
public static class DecimalHelper
{
    /// <summary>
    /// Raises a decimal value to a specified power.
    /// For integer exponents, uses exact decimal arithmetic (repeated squaring).
    /// For fractional exponents, uses double-precision floating-point internally;
    /// results may differ slightly from exact decimal arithmetic.
    /// </summary>
    public static decimal Pow(decimal x, decimal y)
    {
        if (y == decimal.Zero)
            return decimal.One;

        // Integer exponent path — exact decimal arithmetic
        if (y == decimal.Truncate(y) && y >= -28 && y <= 28)
        {
            var isNegativeExp = y < decimal.Zero;
            var absExp = (int)System.Math.Abs((double)y);
            var result = decimal.One;
            var baseVal = x;
            var exp = absExp;
            while (exp > 0)
            {
                if ((exp & 1) == 1)
                    result *= baseVal;
                if (exp > 1)
                    baseVal *= baseVal;
                exp >>= 1;
            }
            return isNegativeExp ? decimal.One / result : result;
        }

        // Fractional exponent path — double-precision approximation
        return (decimal)System.Math.Pow((double)x, (double)y);
    }

    /// <summary>
    /// Returns the natural logarithm (ln) of a decimal value.
    /// Internally uses double-precision floating-point; results may differ
    /// slightly from exact decimal arithmetic for very large or very precise values.
    /// </summary>
    /// <exception cref="OverflowException">Thrown when value is zero or negative.</exception>
    public static decimal Log(this decimal value)
    {
        if (value <= decimal.Zero)
            throw new OverflowException("Logarithm of zero or negative number is undefined.");

        return (decimal)System.Math.Log((double)value);
    }
}
