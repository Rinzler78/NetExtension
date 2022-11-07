using System;
using System.Data.HashFunction.CRC;
using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.FootPrint;

public static class Crc64Helper
{
    public static ulong GenerateCrc64(this ulong[] data)
    {
        return data.GetBytes().GenerateCrc64();
    }

    public static ulong GenerateCrc64(this uint[] data)
    {
        return data.GetBytes().GenerateCrc64();
    }

    public static ulong GenerateCrc64(this string[] data)
    {
        return data.GetBytes().GenerateCrc64();
    }

    public static ulong GenerateCrc64(this string str)
    {
        return str.GetBytes().GenerateCrc64();
    }

    public static ulong GenerateCrc64(this byte[] data)
    {
        var result = CRCFactory.Instance.Create(CRCConfig.CRC64).ComputeHash(data);

        return BitConverter.ToUInt64(result.Hash);
    }
}