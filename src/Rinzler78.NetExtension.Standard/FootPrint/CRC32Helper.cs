using System;
using System.IO.Hashing;
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
        return Crc32.HashToUInt32(data);
    }
}
