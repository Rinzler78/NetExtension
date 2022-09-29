using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;
using System;
using System.Data.HashFunction.CRC;

namespace Rinzler78.NetExtension.FootPrint;

public static class CRC32Helper
{
    public static uint GenerateCRC32(this ulong[] data)
        => data.GetBytes().GenerateCRC32();

    public static uint GenerateCRC32(this uint[] data)
        => data.GetBytes().GenerateCRC32();

    public static uint GenerateCRC32(this string[] data)
        => data.GetBytes().GenerateCRC32();

    public static uint GenerateCRC32(this string str)
        => str.GetBytes().GenerateCRC32();

    public static uint GenerateCRC32(this byte[] data)
    {
        var result = CRCFactory.Instance.Create(CRCConfig.CRC32).ComputeHash(data);

        return BitConverter.ToUInt32(result.Hash);
    }
}