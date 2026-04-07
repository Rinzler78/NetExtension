using System.IO.Hashing;
using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.FootPrint;

/// <summary>
/// Provides extension methods for computing CRC-64 checksums.
/// </summary>
public static class Crc64Helper
{
    /// <summary>
    /// Computes the CRC-64 checksum of a <see cref="ulong"/> array.
    /// </summary>
    public static ulong GenerateCrc64(this ulong[] data) => data.GetBytes().GenerateCrc64();

    /// <summary>
    /// Computes the CRC-64 checksum of a <see cref="uint"/> array.
    /// </summary>
    public static ulong GenerateCrc64(this uint[] data) => data.GetBytes().GenerateCrc64();

    /// <summary>
    /// Computes the CRC-64 checksum of a string array.
    /// </summary>
    public static ulong GenerateCrc64(this string[] data) => data.GetBytes().GenerateCrc64();

    /// <summary>
    /// Computes the CRC-64 checksum of a string.
    /// </summary>
    public static ulong GenerateCrc64(this string str) => str.GetBytes().GenerateCrc64();

    /// <summary>
    /// Computes the CRC-64 checksum of a byte array.
    /// </summary>
    public static ulong GenerateCrc64(this byte[] data)
    {
        return Crc64.HashToUInt64(data);
    }
}
