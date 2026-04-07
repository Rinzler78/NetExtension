using System;
using System.Linq;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.Array;

/// <summary>
/// Provides extension methods for converting and aggregating arrays.
/// </summary>
public static class ArrayExtension
{
    /// <summary>
    /// Converts a <see cref="ulong"/> array to its byte representation.
    /// </summary>
    /// <param name="array">The array to convert.</param>
    /// <returns>A byte array containing the raw bytes of the input array.</returns>
    public static byte[] GetBytes(this ulong[] array)
    {
        var bytesArray = new byte[array.Length * sizeof(ulong)];
        Buffer.BlockCopy(array, 0, bytesArray, 0, bytesArray.Length);
        return bytesArray;
    }

    /// <summary>
    /// Converts a <see cref="uint"/> array to its byte representation.
    /// </summary>
    /// <param name="array">The array to convert.</param>
    /// <returns>A byte array containing the raw bytes of the input array.</returns>
    public static byte[] GetBytes(this uint[] array)
    {
        var bytesArray = new byte[array.Length * sizeof(uint)];
        Buffer.BlockCopy(array, 0, bytesArray, 0, bytesArray.Length);
        return bytesArray;
    }

    /// <summary>
    /// Converts a string array to its byte representation by joining all strings.
    /// </summary>
    /// <param name="strings">The string array to convert.</param>
    /// <returns>A byte array containing the UTF-8 bytes of the concatenated strings.</returns>
    public static byte[] GetBytes(this string[] strings)
        => string.Join("", strings).GetBytes();

    /// <summary>
    /// Sums multiple double arrays element-wise, padding shorter arrays with zero.
    /// </summary>
    /// <param name="arrays">The arrays to sum.</param>
    /// <returns>An array where each element is the sum of the corresponding elements across all input arrays.</returns>
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
