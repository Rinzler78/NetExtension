using System;
using System.Linq;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.Array;

public static class ArrayExtension
{
    public static unsafe byte[] GetBytes(this ulong[] array)
    {
        var bytesArray = new byte[array.Length * sizeof(ulong)];

        fixed (ulong* pArray = array)
        {
            var pArrayAsBytes = (byte*)pArray;

            fixed (byte* pBytesArray = bytesArray)
            {
                for (var i = 0; i < bytesArray.Length; ++i)
                    pBytesArray[i] = pArrayAsBytes[i];
            }
        }

        return bytesArray;
    }

    public static unsafe byte[] GetBytes(this uint[] array)
    {
        var bytesArray = new byte[array.Length * sizeof(uint)];

        fixed (uint* pArray = array)
        {
            var pArrayAsBytes = (byte*)pArray;

            fixed (byte* pBytesArray = bytesArray)
            {
                for (var i = 0; i < bytesArray.Length; ++i)
                    pBytesArray[i] = pArrayAsBytes[i];
            }
        }

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
