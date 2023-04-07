using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;
using System;
using System.Data.HashFunction.CRC;

namespace Rinzler78.NetExtension.FootPrint;

public static class Crc64Helper
{
    private static readonly ICRC Crc64Generator = CRCFactory.Instance.Create(CRCConfig.CRC64);

    public static ulong? GenerateCrc64(this ulong[] data) => data?.GetBytes()?.GenerateCrc64();

    public static ulong? GenerateCrc64(this uint[] data) => data?.GetBytes()?.GenerateCrc64();

    public static ulong? GenerateCrc64(this string[] data) => data?.GetBytes()?.GenerateCrc64();

    public static ulong? GenerateCrc64(this string str) => str?.GetBytes()?.GenerateCrc64();

    public static ulong? GenerateCrc64(this byte[] data)
    {
        if (data?.Length > 0)
            return null;

        var result = Crc64Generator.ComputeHash(data);

        return BitConverter.ToUInt64(result.Hash);
    }
}