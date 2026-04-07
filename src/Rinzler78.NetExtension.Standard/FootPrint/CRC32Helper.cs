using System.IO.Hashing;
using Rinzler78.NetExtension.Array;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.FootPrint;

/// <summary>
/// Provides extension methods for computing CRC-32 checksums.
/// </summary>
public static class Crc32Helper
{
    /// <summary>
    /// Computes the CRC-32 checksum of a <see cref="ulong"/> array.
    /// </summary>
    public static uint GenerateCrc32(this ulong[] data) => data.GetBytes().GenerateCrc32();

    /// <summary>
    /// Computes the CRC-32 checksum of a <see cref="uint"/> array.
    /// </summary>
    public static uint GenerateCrc32(this uint[] data) => data.GetBytes().GenerateCrc32();

    /// <summary>
    /// Computes the CRC-32 checksum of a string array.
    /// </summary>
    public static uint GenerateCrc32(this string[] data) => data.GetBytes().GenerateCrc32();

    /// <summary>
    /// Computes the CRC-32 checksum of a string.
    /// </summary>
    public static uint GenerateCrc32(this string str) => str.GetBytes().GenerateCrc32();

    /// <summary>
    /// Computes the CRC-32 checksum of a byte array.
    /// </summary>
    public static uint GenerateCrc32(this byte[] data)
    {
        return Crc32.HashToUInt32(data);
    }
}
