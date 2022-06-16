using System.Linq;

namespace Rinzler78.NetExtension.Array;

public static class ArrayExtension
{
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

    public static int ComputeHashCode<ArrayType>(this ArrayType[] array)
    {
        int hash = 0;
        for (int i = 0; i < array.Length; i++)
            hash ^= array[i].GetHashCode();
        return hash;
    }
}