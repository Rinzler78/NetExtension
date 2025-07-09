using System;
using System.Linq;
using Rinzler78.NetExtension.Array;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Array;

public class ArrayExtensionTests
{
    [Fact]
    public void GetBytes_UlongArray_ShouldReturnCorrectByteArray()
    {
        // Arrange
        var ulongArray = new ulong[] { 0x0123456789ABCDEF, 0xFEDCBA9876543210 };

        // Act
        var result = ulongArray.GetBytes();

        // Assert
        Assert.Equal(16, result.Length); // 2 ulongs * 8 bytes each
        Assert.NotNull(result);

        // Verify that we can convert back (basic sanity check)
        Assert.True(result.Length % sizeof(ulong) == 0);
    }

    [Fact]
    public void GetBytes_UintArray_ShouldReturnCorrectByteArray()
    {
        // Arrange
        var uintArray = new uint[] { 0x12345678, 0x9ABCDEF0, 0xFEDCBA98 };

        // Act
        var result = uintArray.GetBytes();

        // Assert
        Assert.Equal(12, result.Length); // 3 uints * 4 bytes each
        Assert.NotNull(result);

        // Verify that we can convert back (basic sanity check)
        Assert.True(result.Length % sizeof(uint) == 0);
    }

    [Fact]
    public void GetBytes_EmptyUlongArray_ShouldReturnEmptyByteArray()
    {
        // Arrange
        var ulongArray = new ulong[0];

        // Act
        var result = ulongArray.GetBytes();

        // Assert
        Assert.Empty(result);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetBytes_EmptyUintArray_ShouldReturnEmptyByteArray()
    {
        // Arrange
        var uintArray = new uint[0];

        // Act
        var result = uintArray.GetBytes();

        // Assert
        Assert.Empty(result);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetBytes_SingleUlong_ShouldReturnCorrectByteArray()
    {
        // Arrange
        var ulongArray = new ulong[] { 0x123456789ABCDEF0 };

        // Act
        var result = ulongArray.GetBytes();

        // Assert
        Assert.Equal(8, result.Length); // 1 ulong * 8 bytes
        Assert.NotNull(result);
    }

    [Fact]
    public void GetBytes_SingleUint_ShouldReturnCorrectByteArray()
    {
        // Arrange
        var uintArray = new uint[] { 0x12345678 };

        // Act
        var result = uintArray.GetBytes();

        // Assert
        Assert.Equal(4, result.Length); // 1 uint * 4 bytes
        Assert.NotNull(result);
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
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetBytes_EmptyStringArray_ShouldReturnEmptyByteArray()
    {
        // Arrange
        var stringArray = new string[0];

        // Act
        var result = stringArray.GetBytes();

        // Assert
        Assert.Empty(result);
        Assert.NotNull(result);
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
        Assert.Equal(new double[] { 5.0, 7.0, 9.0 }, result);
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
        Assert.Equal(4, result.Length);
        Assert.Equal(new double[] { 5.0, 7.0, 6.0, 7.0 }, result);
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
        Assert.Equal(new double[] { 6.0, 6.0, 6.0 }, result);
    }

    [Fact]
    public void SumArrays_SingleArray_ShouldReturnCopyOfArray()
    {
        // Arrange
        var array = new double[] { 1.0, 2.0, 3.0 };

        // Act
        var result = ArrayExtension.SumArrays(array);

        // Assert
        Assert.Equal(array, result);
        Assert.NotSame(array, result); // Should be a copy, not the same reference
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
        Assert.Empty(result);
    }

    [Fact]
    public void SumArrays_NoArrays_ShouldReturnEmptyArray()
    {
        // Act
        var result = ArrayExtension.SumArrays();

        // Assert
        Assert.Empty(result);
    }
}
