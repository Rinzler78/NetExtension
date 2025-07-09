using System;
using System.Numerics;

namespace Rinzler78.NetExtension.Math;

public static class BigIntegerExt
{
    public static ulong ToULong(this BigInteger bigInteger)
    {
        try
        {
            return (ulong)bigInteger;
        }
        catch (Exception)
        {
            // ignored
        }

        ulong result;
        if (bigInteger < 0)
            result = ulong.MinValue;
        else
            result = ulong.MaxValue;

        //Console.WriteLine($"BigInteger convert failed : From {bigInteger} to {result}");
        return result;
    }

    public static double ToDouble(this BigInteger bigInteger)
    {
        try
        {
            double val = (double)bigInteger;
            if (double.IsInfinity(val))
            {
                return bigInteger < 0 ? double.MinValue : double.MaxValue;
            }
            return val;
        }
        catch (Exception)
        {
            // ignored
        }
        return bigInteger < 0 ? double.MinValue : double.MaxValue;
    }

    public static decimal ToDecimal(this BigInteger bigInteger)
    {
        try
        {
            return (decimal)bigInteger;
        }
        catch (Exception)
        {
            // ignored
        }

        decimal result;
        if (bigInteger < 0)
            result = decimal.MinValue;
        else
            result = decimal.MaxValue;

        //Console.WriteLine($"BigInteger convert failed : From {bigInteger} to {result}");
        return result;
    }
}