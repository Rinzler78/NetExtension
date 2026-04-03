using Rinzler78.NetExtension.FootPrint;

namespace Rinzler78.NetExtension.Tests.FootPrint;

[Trait("Category", "Unit")]
public class HashHelperTests
{
    // ─────────────────────────────────────────────────────────────────
    // CRC-32 (ISO/IEC 3309 / PKZIP polynomial)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Known-good reference pinned from a single authoritative run of
    /// <c>System.IO.Hashing.Crc32.HashToUInt32(new byte[]{1,2,3})</c>.
    /// Re-compute only if the hashing algorithm is deliberately changed.
    /// Python cross-check: binascii.crc32(bytes([1,2,3])) &amp; 0xFFFFFFFF == 0x55BC801D.
    /// </summary>
    private const uint KnownCrc32ForBytes123 = 0x55BC801Du; // 1438416925u

    [Fact]
    public void GenerateCrc32_KnownVector_ByteArray123_ShouldMatchExpected()
    {
        var bytes = new byte[] { 1, 2, 3 };

        bytes.GenerateCrc32().Should().Be(KnownCrc32ForBytes123);
    }

    [Fact]
    public void GenerateCrc32_Determinism_ByteArray_ShouldReturnSameResultTwice()
    {
        var bytes = new byte[] { 10, 20, 30, 40, 50 };

        var first = bytes.GenerateCrc32();
        var second = bytes.GenerateCrc32();

        first.Should().Be(second);
    }

    [Fact]
    public void GenerateCrc32_Determinism_String_ShouldReturnSameResultTwice()
    {
        var first = "hello world".GenerateCrc32();
        var second = "hello world".GenerateCrc32();

        first.Should().Be(second);
    }

    [Fact]
    public void GenerateCrc32_EmptyByteArray_ShouldNotThrowAndBeConsistent()
    {
        var empty = new byte[] { };

        var first = empty.GenerateCrc32();
        var second = empty.GenerateCrc32();

        // CRC-32 of empty input is 0 (Init XOR XorOut = 0xFFFFFFFF ^ 0xFFFFFFFF).
        first.Should().Be(0u);
        first.Should().Be(second);
    }

    [Fact]
    public void GenerateCrc32_EmptyString_ShouldNotThrowAndBeConsistent()
    {
        var first = string.Empty.GenerateCrc32();
        var second = string.Empty.GenerateCrc32();

        // ASCII bytes of "" → empty byte array → CRC32 = 0u.
        first.Should().Be(0u);
        first.Should().Be(second);
    }

    [Fact]
    public void GenerateCrc32_AllInputShapes_ShouldBeDeterministic()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var uints = new uint[] { 1, 2, 3 };
        var ulongs = new ulong[] { 1, 2, 3 };
        var strarr = new[] { "a", "b" };
        const string str = "abc";

        // byte[] matches known vector
        bytes.GenerateCrc32().Should().Be(KnownCrc32ForBytes123);

        // All other shapes: determinism is the key assertion (specific value
        // depends on GetBytes() serialisation; we pin it via round-trip equality).
        uints.GenerateCrc32().Should().Be(uints.GenerateCrc32());
        ulongs.GenerateCrc32().Should().Be(ulongs.GenerateCrc32());
        strarr.GenerateCrc32().Should().Be(strarr.GenerateCrc32());
        str.GenerateCrc32().Should().Be(str.GenerateCrc32());
    }

    // ─────────────────────────────────────────────────────────────────
    // CRC-64 (ECMA-182)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateCrc64_Determinism_ByteArray_ShouldReturnSameResultTwice()
    {
        var bytes = new byte[] { 10, 20, 30, 40, 50 };

        var first = bytes.GenerateCrc64();
        var second = bytes.GenerateCrc64();

        first.Should().Be(second);
    }

    [Fact]
    public void GenerateCrc64_EmptyByteArray_ShouldNotThrowAndBeConsistent()
    {
        var first = new byte[] { }.GenerateCrc64();
        var second = new byte[] { }.GenerateCrc64();

        first.Should().Be(second);
    }

    [Fact]
    public void GenerateCrc64_EmptyString_ShouldNotThrowAndBeConsistent()
    {
        var first = string.Empty.GenerateCrc64();
        var second = string.Empty.GenerateCrc64();

        first.Should().Be(second);
    }

    [Fact]
    public void GenerateCrc64_AllInputShapes_ShouldBeDeterministic()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var uints = new uint[] { 1, 2, 3 };
        var ulongs = new ulong[] { 1, 2, 3 };
        var strarr = new[] { "a", "b" };
        const string str = "abc";

        var bytesHash = bytes.GenerateCrc64();
        bytesHash.Should().BeGreaterThan(0ul,
            "a non-trivial input should yield a non-zero CRC-64");

        bytes.GenerateCrc64().Should().Be(bytesHash);
        uints.GenerateCrc64().Should().Be(uints.GenerateCrc64());
        ulongs.GenerateCrc64().Should().Be(ulongs.GenerateCrc64());
        strarr.GenerateCrc64().Should().Be(strarr.GenerateCrc64());
        str.GenerateCrc64().Should().Be(str.GenerateCrc64());
    }

    // ─────────────────────────────────────────────────────────────────
    // SHA-512
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateSha512_Determinism_ByteArray_ShouldReturnSameResultTwice()
    {
        var bytes = new byte[] { 10, 20, 30 };

        bytes.GenerateSha512().Should().Equal(bytes.GenerateSha512());
    }

    [Fact]
    public void GenerateSha512_EmptyByteArray_ShouldNotThrowAndReturn64Bytes()
    {
        var result = new byte[] { }.GenerateSha512();

        // SHA-512 always produces a 512-bit (64-byte) digest, even for empty input.
        result.Should().HaveCount(64);
        result.Should().Equal(new byte[] { }.GenerateSha512(),
            "empty-input hash must be deterministic");
    }

    [Fact]
    public void GenerateSha512_EmptyString_ShouldNotThrowAndReturn64Bytes()
    {
        var result = string.Empty.GenerateSha512();

        result.Should().HaveCount(64);
        result.Should().Equal(string.Empty.GenerateSha512());
    }

    [Fact]
    public void GenerateSha512_AllInputShapes_ShouldReturn64BytesAndBeDeterministic()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var uints = new uint[] { 1, 2, 3 };
        var ulongs = new ulong[] { 1, 2, 3 };
        var strarr = new[] { "a", "b" };
        const string str = "abc";

        bytes.GenerateSha512().Should().HaveCount(64).And.Equal(bytes.GenerateSha512());
        uints.GenerateSha512().Should().HaveCount(64).And.Equal(uints.GenerateSha512());
        ulongs.GenerateSha512().Should().HaveCount(64).And.Equal(ulongs.GenerateSha512());
        strarr.GenerateSha512().Should().HaveCount(64).And.Equal(strarr.GenerateSha512());
        str.GenerateSha512().Should().HaveCount(64).And.Equal(str.GenerateSha512());
    }
}
