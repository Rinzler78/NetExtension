using Rinzler78.NetExtension.Strings;
using System.Linq;

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
    {
        return string.Join("", strings).GetBytes();
        // strings.SelectMany(str => str.GetBytes()).ToArray();
    }

    public static double[] SumArrays(params double[][] arrays)
    {
        var count = arrays.Max(arg => arg.Length);
        var result = new double[count];

        for (var i = 0; i < count; ++i)
        {
            double sum = default;

            for (var y = 0; y < arrays.Length; ++y)
                sum += arrays[y][i];

            result[i] = sum;
        }

        return result;
    }

/*
    public static int ComputeHashCode<ArrayType>(this ArrayType[] array)
    {
        var hash = 0;
        for (var i = 0; i < array.Length; i++)
            hash ^= array[i].GetHashCode();
        return hash;
    }
*/
}