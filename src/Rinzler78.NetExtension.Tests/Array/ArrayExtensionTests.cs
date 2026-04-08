using System;
using Rinzler78.NetExtension.Array;

namespace Rinzler78.NetExtension.Tests.Array;

[Trait("Category", "Unit")]
public class ArrayExtensionTests
{
    [Fact]
    public void GetBytes_UlongArray_ShouldReturnCorrectByteArray()
    {
        // Arrange
        var ulongArray = new ulong[] { 0x0123456789ABCDEF, 0xFEDCBA9876543210 };

        // Act
        var result = ulongArray.GetBytes();

        // Assert – raw bytes in platform byte order (little-endian on x86/ARM)
        var expected = new byte[16];
        BitConverter.GetBytes(0x0123456789ABCDEFul).CopyTo(expected, 0);
        BitConverter.GetBytes(0xFEDCBA9876543210ul).CopyTo(expected, 8);
        result.Should().Equal(expected);
    }

    [Fact]
    public void GetBytes_UintArray_ShouldReturnCorrectByteArray()
    {
        // Arrange
        var uintArray = new uint[] { 0x12345678, 0x9ABCDEF0, 0xFEDCBA98 };

        // Act
        var result = uintArray.GetBytes();

        // Assert – raw bytes in platform byte order (little-endian on x86/ARM)
        var expected = new byte[12];
        BitConverter.GetBytes(0x12345678u).CopyTo(expected, 0);
        BitConverter.GetBytes(0x9ABCDEF0u).CopyTo(expected, 4);
        BitConverter.GetBytes(0xFEDCBA98u).CopyTo(expected, 8);
        result.Should().Equal(expected);
    }

    [Fact]
    public void GetBytes_EmptyUlongArray_ShouldReturnEmptyByteArray()
    {
        // Arrange
        var ulongArray = new ulong[0];

        // Act
        var result = ulongArray.GetBytes();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetBytes_EmptyUintArray_ShouldReturnEmptyByteArray()
    {
        // Arrange
        var uintArray = new uint[0];

        // Act
        var result = uintArray.GetBytes();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetBytes_SingleUlong_ShouldReturnCorrectByteArray()
    {
        // Arrange
        var ulongArray = new ulong[] { 0x123456789ABCDEF0 };

        // Act
        var result = ulongArray.GetBytes();

        // Assert – raw bytes in platform byte order (little-endian on x86/ARM)
        var expected = BitConverter.GetBytes(0x123456789ABCDEF0ul);
        result.Should().Equal(expected);
    }

    [Fact]
    public void GetBytes_SingleUint_ShouldReturnCorrectByteArray()
    {
        // Arrange
        var uintArray = new uint[] { 0x12345678 };

        // Act
        var result = uintArray.GetBytes();

        // Assert – raw bytes in platform byte order (little-endian on x86/ARM)
        var expected = BitConverter.GetBytes(0x12345678u);
        result.Should().Equal(expected);
    }

    [Theory]
    [InlineData(new string[] { "hello", "world" }, new byte[] { 104, 101, 108, 108, 111, 119, 111, 114, 108, 100 })]
    [InlineData(new string[] { "a", "b", "c" }, new byte[] { 97, 98, 99 })]
    [InlineData(new string[] { "" }, new byte[0])]
    public void GetBytes_StringArray_ShouldReturnCorrectByteArray(string[] input, byte[] expected)
    {
        // Act
        var result = input.GetBytes();

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void GetBytes_EmptyStringArray_ShouldReturnEmptyByteArray()
    {
        // Arrange
        var stringArray = new string[0];

        // Act
        var result = stringArray.GetBytes();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SumArrays_TwoArraysSameLength_ShouldReturnCorrectSum()
    {
        // Arrange
        var array1 = new double[] { 1.0, 2.0, 3.0 };
        var array2 = new double[] { 4.0, 5.0, 6.0 };

        // Act
        var result = ArrayExtension.SumArrays(array1, array2);

        // Assert
        result.Should().Equal(new double[] { 5.0, 7.0, 9.0 });
    }

    [Fact]
    public void SumArrays_TwoArraysDifferentLength_ShouldReturnSumWithLongerLength()
    {
        // Arrange
        var array1 = new double[] { 1.0, 2.0 };
        var array2 = new double[] { 4.0, 5.0, 6.0, 7.0 };

        // Act
        var result = ArrayExtension.SumArrays(array1, array2);

        // Assert
        result.Should().HaveCount(4);
        result.Should().Equal(new double[] { 5.0, 7.0, 6.0, 7.0 });
    }

    [Fact]
    public void SumArrays_MultipleArrays_ShouldReturnCorrectSum()
    {
        // Arrange
        var array1 = new double[] { 1.0, 1.0, 1.0 };
        var array2 = new double[] { 2.0, 2.0, 2.0 };
        var array3 = new double[] { 3.0, 3.0, 3.0 };

        // Act
        var result = ArrayExtension.SumArrays(array1, array2, array3);

        // Assert
        result.Should().Equal(new double[] { 6.0, 6.0, 6.0 });
    }

    [Fact]
    public void SumArrays_SingleArray_ShouldReturnCopyOfArray()
    {
        // Arrange
        var array = new double[] { 1.0, 2.0, 3.0 };

        // Act
        var result = ArrayExtension.SumArrays(array);

        // Assert
        result.Should().Equal(array);
        result.Should().NotBeSameAs(array); // Should be a copy, not the same reference
    }

    [Fact]
    public void SumArrays_EmptyArrays_ShouldReturnEmptyArray()
    {
        // Arrange
        var array1 = new double[0];
        var array2 = new double[0];

        // Act
        var result = ArrayExtension.SumArrays(array1, array2);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SumArrays_NoArrays_ShouldReturnEmptyArray()
    {
        // Act
        var result = ArrayExtension.SumArrays();

        // Assert
        result.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────────────
    // Additional edge cases
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void GetBytes_StringArray_WithUnicodeStrings_ShouldUseAsciiEncoding()
    {
        // string[].GetBytes() → string.Join → StringHelper.GetBytes() → Encoding.ASCII
        // 'é' (U+00E9) is outside ASCII range → replaced by 0x3F ('?')
        var result = new[] { "café" }.GetBytes();

        result.Should().HaveCount(4, "ASCII treats each char as one byte");
        result[3].Should().Be(0x3F, "'é' is replaced by '?' in ASCII");
    }

    [Fact]
    public void SumArrays_WithNegativeValues_ShouldSumCorrectly()
    {
        var a = new double[] { -1.0, 2.0, -3.0 };
        var b = new double[] { 1.0, -2.0, 3.0 };

        var result = ArrayExtension.SumArrays(a, b);

        result.Should().Equal(new double[] { 0.0, 0.0, 0.0 });
    }

    [Fact]
    public void GetBytes_UlongArray_WithMaxValue_ShouldReturnCorrectBytes()
    {
        var result = new[] { ulong.MaxValue }.GetBytes();

        result.Should().HaveCount(8);
        result.Should().AllSatisfy(b => b.Should().Be(0xFF));
    }

    [Fact]
    public void GetBytes_StringArray_WithMultipleEmptyStrings_ShouldReturnEmpty()
    {
        var result = new[] { "", "", "" }.GetBytes();

        result.Should().BeEmpty();
    }
}
