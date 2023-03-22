using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;
using System.Security.Cryptography;

namespace Rinzler78.NetExtension.FootPrint;

public static class Sha512Helper
{
    public static byte[] GenerateSha512(this ulong[] data)
    {
        return data.GetBytes().GenerateSha512();
    }

    public static byte[] GenerateSha512(this uint[] data)
    {
        return data.GetBytes().GenerateSha512();
    }

    public static byte[] GenerateSha512(this string[] data)
    {
        return data.GetBytes().GenerateSha512();
    }

    public static byte[] GenerateSha512(this string str)
    {
        return str.GetBytes().GenerateSha512();
    }

    public static byte[] GenerateSha512(this byte[] data)
    {
        var result = SHA512.HashData(data);

        return result;
    }
}