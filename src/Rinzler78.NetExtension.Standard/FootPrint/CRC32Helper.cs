using System;
using System.Data.HashFunction.CRC;
using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.FootPrint;

public static class Crc32Helper
{
    public static uint GenerateCrc32(this ulong[] data) => data.GetBytes().GenerateCrc32();

    public static uint GenerateCrc32(this uint[] data) => data.GetBytes().GenerateCrc32();

    public static uint GenerateCrc32(this string[] data) => data.GetBytes().GenerateCrc32();

    public static uint GenerateCrc32(this string str) => str.GetBytes().GenerateCrc32();

    public static uint GenerateCrc32(this byte[] data)
    {
        var result = CRCFactory.Instance.Create(CRCConfig.CRC32).ComputeHash(data);

        return BitConverter.ToUInt32(result.Hash);
    }
}
