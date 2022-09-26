
using System;
using System.Data.HashFunction.CRC;
using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.FootPrint;

public static class CRC64Helper
{
    public static ulong GenerateCRC64(this ulong[] data)
        => data.GetBytes().GenerateCRC64();

    public static ulong GenerateCRC64(this uint[] data)
        => data.GetBytes().GenerateCRC64();

    public static ulong GenerateCRC64(this string[] data)
        => data.GetBytes().GenerateCRC64();

    public static ulong GenerateCRC64(this string str)
        => str.GetBytes().GenerateCRC64();

    public static ulong GenerateCRC64(this byte[] data)
    {
        var result = CRCFactory.Instance.Create(CRCConfig.CRC64).ComputeHash(data);

        return BitConverter.ToUInt64(result.Hash);
    }
}