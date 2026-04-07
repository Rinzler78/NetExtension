using System.Security.Cryptography;
using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.FootPrint;

/// <summary>
/// Provides extension methods for computing SHA-512 hashes.
/// </summary>
public static class Sha512Helper
{
    /// <summary>
    /// Computes the SHA-512 hash of a <see cref="ulong"/> array.
    /// </summary>
    public static byte[] GenerateSha512(this ulong[] data) => data.GetBytes().GenerateSha512();

    /// <summary>
    /// Computes the SHA-512 hash of a <see cref="uint"/> array.
    /// </summary>
    public static byte[] GenerateSha512(this uint[] data) => data.GetBytes().GenerateSha512();

    /// <summary>
    /// Computes the SHA-512 hash of a string array.
    /// </summary>
    public static byte[] GenerateSha512(this string[] data) => data.GetBytes().GenerateSha512();

    /// <summary>
    /// Computes the SHA-512 hash of a string.
    /// </summary>
    public static byte[] GenerateSha512(this string str) => str.GetBytes().GenerateSha512();

    /// <summary>
    /// Computes the SHA-512 hash of a byte array.
    /// </summary>
    public static byte[] GenerateSha512(this byte[] data)
    {
        var result = SHA512.HashData(data);

        return result;
    }
}
