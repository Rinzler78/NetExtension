using System;
using System.Numerics;

namespace Rinzler78.NetExtension.Math
{
    public static class BigIntegerExt
    {
        public static ulong ToULong(this BigInteger bigInteger)
        {
            try
            {
                return (ulong)bigInteger;
            }
            catch
            {
            }

            ulong result = 0;
            if (bigInteger < 0)
                result = ulong.MinValue;
            else
                result = ulong.MaxValue;

            //Console.WriteLine($"BigInteger convert failed : From {bigInteger} to {result}");
            return result;
        }
    }
}
