using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;
using System.Security.Cryptography;

namespace Rinzler78.NetExtension.FootPrint;

public static class SHA512Helper
{
    public static byte[] GenerateSHA512(this ulong[] data)
        => data.GetBytes().GenerateSHA512();

    public static byte[] GenerateSHA512(this uint[] data)
        => data.GetBytes().GenerateSHA512();

    public static byte[] GenerateSHA512(this string[] data)
        => data.GetBytes().GenerateSHA512();

    public static byte[] GenerateSHA512(this string str)
        => str.GetBytes().GenerateSHA512();

    public static byte[] GenerateSHA512(this byte[] data)
    {
        var tool = SHA512.Create();

        var result = tool.ComputeHash(data);

        return result;
    }
}