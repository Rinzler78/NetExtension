using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;
using System;
using System.Data.HashFunction.CRC;

namespace Rinzler78.NetExtension.FootPrint;

public static class Crc32Helper
{
    public static uint GenerateCrc32(this ulong[] data)
    {
        return data.GetBytes().GenerateCrc32();
    }

    public static uint GenerateCrc32(this uint[] data)
    {
        return data.GetBytes().GenerateCrc32();
    }

    public static uint GenerateCrc32(this string[] data)
    {
        return data.GetBytes().GenerateCrc32();
    }

    public static uint GenerateCrc32(this string str)
    {
        return str.GetBytes().GenerateCrc32();
    }

    public static uint GenerateCrc32(this byte[] data)
    {
        var result = CRCFactory.Instance.Create(CRCConfig.CRC32).ComputeHash(data);

        return BitConverter.ToUInt32(result.Hash);
    }
}