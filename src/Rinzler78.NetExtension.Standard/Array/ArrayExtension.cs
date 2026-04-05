using System;
using System.Linq;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.Array;

public static class ArrayExtension
{
    public static byte[] GetBytes(this ulong[] array)
    {
        var bytesArray = new byte[array.Length * sizeof(ulong)];
        Buffer.BlockCopy(array, 0, bytesArray, 0, bytesArray.Length);
        return bytesArray;
    }

    public static byte[] GetBytes(this uint[] array)
    {
        var bytesArray = new byte[array.Length * sizeof(uint)];
        Buffer.BlockCopy(array, 0, bytesArray, 0, bytesArray.Length);
        return bytesArray;
    }

    public static byte[] GetBytes(this string[] strings)
        => string.Join("", strings).GetBytes();

    public static double[] SumArrays(params double[][] arrays)
    {
        if (arrays == null || arrays.Length == 0)
            return System.Array.Empty<double>();

        var count = arrays.Max(arg => arg.Length);
        var result = new double[count];

        for (var i = 0; i < count; ++i)
        {
            double sum = default;

            for (var y = 0; y < arrays.Length; ++y)
            {
                if (i < arrays[y].Length)
                    sum += arrays[y][i];
            }

            result[i] = sum;
        }

        return result;
    }
}
