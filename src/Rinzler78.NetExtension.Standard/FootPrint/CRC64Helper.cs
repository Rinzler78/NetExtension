using System;
using System.IO.Hashing;
using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.FootPrint;

public static class Crc64Helper
{
    public static ulong GenerateCrc64(this ulong[] data) => data.GetBytes().GenerateCrc64();

    public static ulong GenerateCrc64(this uint[] data) => data.GetBytes().GenerateCrc64();

    public static ulong GenerateCrc64(this string[] data) => data.GetBytes().GenerateCrc64();

    public static ulong GenerateCrc64(this string str) => str.GetBytes().GenerateCrc64();

    public static ulong GenerateCrc64(this byte[] data)
    {
        return Crc64.HashToUInt64(data);
    }
}
