using System;
using System.Numerics;
using Rinzler78.NetExtension.Math;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Math;

public class BigIntegerExtTests
{
    [Theory]
    [InlineData(0, 0UL)]
    [InlineData(100, 100UL)]
    [InlineData(1000, 1000UL)]
    public void ToULong_ValidPositiveValues_ShouldReturnCorrectValue(long input, ulong expected)
    {
        // Arrange
        var bigInteger = new BigInteger(input);

        // Act
        var result = bigInteger.ToULong();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToULong_MaxULongValue_ShouldReturnMaxValue()
    {
        // Arrange
        var bigInteger = new BigInteger(ulong.MaxValue);

        // Act
        var result = bigInteger.ToULong();

        // Assert
        Assert.Equal(ulong.MaxValue, result);
    }

    [Fact]
    public void ToULong_NegativeValue_ShouldReturnMinValue()
    {
        // Arrange
        var bigInteger = new BigInteger(-100);

        // Act
        var result = bigInteger.ToULong();

        // Assert
        Assert.Equal(ulong.MinValue, result);
    }

    [Fact]
    public void ToULong_ValueTooLarge_ShouldReturnMaxValue()
    {
        // Arrange
        var bigInteger = BigInteger.Parse("18446744073709551616"); // ulong.MaxValue + 1

        // Act
        var result = bigInteger.ToULong();

        // Assert
        Assert.Equal(ulong.MaxValue, result);
    }

    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(100, 100.0)]
    [InlineData(-100, -100.0)]
    [InlineData(1000000, 1000000.0)]
    public void ToDouble_ValidValues_ShouldReturnCorrectValue(long input, double expected)
    {
        // Arrange
        var bigInteger = new BigInteger(input);

        // Act
        var result = bigInteger.ToDouble();

        // Assert
        Assert.Equal(expected, result, 10); // 10 decimal places precision
    }

    [Fact]
    public void ToDouble_VeryLargePositiveValue_ShouldReturnMaxValue()
    {
        // Arrange
        // Create a BigInteger that's larger than double.MaxValue
        var bigInteger = BigInteger.Parse("1" + new string('0', 400)); // Very large number

        // Act
        var result = bigInteger.ToDouble();

        // Assert
        Assert.Equal(double.MaxValue, result);
    }

    [Fact]
    public void ToDouble_VeryLargeNegativeValue_ShouldReturnMinValue()
    {
        // Arrange
        // Create a BigInteger that's smaller than double.MinValue
        var bigInteger = BigInteger.Parse("-1" + new string('0', 400)); // Very large negative number

        // Act
        var result = bigInteger.ToDouble();

        // Assert
        Assert.Equal(double.MinValue, result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 100)]
    [InlineData(-100, -100)]
    [InlineData(123456, 123456)]
    public void ToDecimal_ValidValues_ShouldReturnCorrectValue(long input, decimal expected)
    {
        // Arrange
        var bigInteger = new BigInteger(input);

        // Act
        var result = bigInteger.ToDecimal();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDecimal_MaxDecimalValue_ShouldReturnMaxValue()
    {
        // Arrange
        var bigInteger = new BigInteger(decimal.MaxValue);

        // Act
        var result = bigInteger.ToDecimal();

        // Assert
        Assert.Equal(decimal.MaxValue, result);
    }

    [Fact]
    public void ToDecimal_MinDecimalValue_ShouldReturnMinValue()
    {
        // Arrange
        var bigInteger = new BigInteger(decimal.MinValue);

        // Act
        var result = bigInteger.ToDecimal();

        // Assert
        Assert.Equal(decimal.MinValue, result);
    }

    [Fact]
    public void ToDecimal_ValueTooLarge_ShouldReturnMaxValue()
    {
        // Arrange
        // Create a BigInteger larger than decimal.MaxValue
        var bigInteger = BigInteger.Parse("79228162514264337593543950336"); // decimal.MaxValue + 1

        // Act
        var result = bigInteger.ToDecimal();

        // Assert
        Assert.Equal(decimal.MaxValue, result);
    }

    [Fact]
    public void ToDecimal_ValueTooSmall_ShouldReturnMinValue()
    {
        // Arrange
        // Create a BigInteger smaller than decimal.MinValue
        var bigInteger = BigInteger.Parse("-79228162514264337593543950336"); // decimal.MinValue - 1

        // Act
        var result = bigInteger.ToDecimal();

        // Assert
        Assert.Equal(decimal.MinValue, result);
    }

    [Fact]
    public void ToULong_Zero_ShouldReturnZero()
    {
        // Arrange
        var bigInteger = BigInteger.Zero;

        // Act
        var result = bigInteger.ToULong();

        // Assert
        Assert.Equal(0UL, result);
    }

    [Fact]
    public void ToDouble_Zero_ShouldReturnZero()
    {
        // Arrange
        var bigInteger = BigInteger.Zero;

        // Act
        var result = bigInteger.ToDouble();

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void ToDecimal_Zero_ShouldReturnZero()
    {
        // Arrange
        var bigInteger = BigInteger.Zero;

        // Act
        var result = bigInteger.ToDecimal();

        // Assert
        Assert.Equal(0m, result);
    }
}
