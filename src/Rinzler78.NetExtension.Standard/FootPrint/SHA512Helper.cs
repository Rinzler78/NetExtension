using System.Security.Cryptography;
using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.FootPrint;

public static class Sha512Helper
{
    public static byte[] GenerateSha512(this ulong[] data) => data.GetBytes().GenerateSha512();

    public static byte[] GenerateSha512(this uint[] data) => data.GetBytes().GenerateSha512();

    public static byte[] GenerateSha512(this string[] data) => data.GetBytes().GenerateSha512();

    public static byte[] GenerateSha512(this string str) => str.GetBytes().GenerateSha512();

    public static byte[] GenerateSha512(this byte[] data)
    {
        var result = SHA512.HashData(data);

        return result;
    }
}
