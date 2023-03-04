using System;
namespace Rinzler78.NetExtension.Math;

public static class DecimalHelper
{
    public static decimal Pow(decimal x, decimal y)
        => (decimal)System.Math.Pow((double)x, (double)y);

    public static decimal Log(this decimal value)
        => (decimal)System.Math.Log((double)value);
}

