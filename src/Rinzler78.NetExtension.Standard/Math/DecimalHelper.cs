using System;
namespace Rinzler78.NetExtension.Math;

public static class DecimalHelper
{
    public static decimal Pow(decimal x, decimal y)
    {
        int precision = System.Math.Max(28, System.Math.Max(GetDecimalPrecision(x), GetDecimalPrecision(y)));

        decimal one = 1.0M;
        decimal pow = one;

        if (y < 0)
        {
            x = one / x;
            y = -y;
        }

        while (y > 0)
        {
            if (y % 2 == 1)
            {
                pow *= x;
                pow = RoundDecimalToPrecision(pow, precision);
            }

            x *= x;
            x = RoundDecimalToPrecision(x, precision);
            y /= 2;
        }

        return pow;
    }

    private static int GetDecimalPrecision(decimal value)
    {
        return BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
    }

    private static decimal RoundDecimalToPrecision(decimal value, int precision)
    {
        return System.Math.Round(value, precision, MidpointRounding.AwayFromZero);
    }
}

