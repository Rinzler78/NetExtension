using System;
using Rinzler78.NetExtension.Enums;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Enums;

public class EnumsHelperTests
{
    public enum TestEnum1
    {
        Value1,
        Value2,
        Value3
    }

    public enum TestEnum2
    {
        Value1 = 10,
        Value2 = 20,
        Value3 = 30
    }

    public enum TestEnum3
    {
        DifferentValue1,
        DifferentValue2,
        Value1 // Same name as in TestEnum1
    }

    [Fact]
    public void Convert_EnumToEnum_SameNames_ShouldReturnCorrectValue()
    {
        // Arrange
        var source = TestEnum1.Value1;

        // Act
        var result = source.Convert<TestEnum1, TestEnum3>();

        // Assert
        Assert.Equal(TestEnum3.Value1, result); // Should match by name "Value1"
    }

    [Fact]
    public void Convert_EnumToEnum_NoMatchingNames_ShouldReturnDefault()
    {
        // Arrange
        var source = TestEnum1.Value3;

        // Act
        var result = source.Convert<TestEnum1, TestEnum3>();

        // Assert
        Assert.Equal(TestEnum3.DifferentValue1, result); // Should return default (first value) since Value3 doesn't exist in TestEnum3
    }

    [Fact]
    public void Convert_EnumToEnum_MatchingNames_ShouldReturnCorrectValue()
    {
        // Arrange
        var source = TestEnum1.Value1;

        // Act
        var result = source.Convert<TestEnum1, TestEnum2>();

        // Assert
        Assert.Equal(TestEnum2.Value1, result); // Both have Value1
    }

    [Theory]
    [InlineData("Value1", TestEnum1.Value1)]
    [InlineData("Value2", TestEnum1.Value2)]
    [InlineData("Value3", TestEnum1.Value3)]
    [InlineData("value1", TestEnum1.Value1)] // Case insensitive
    [InlineData("VALUE2", TestEnum1.Value2)] // Case insensitive
    public void Convert_StringToEnum_ValidValues_ShouldReturnCorrectEnum(string input, TestEnum1 expected)
    {
        // Act
        var result = input.Convert<TestEnum1>();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("InvalidValue")]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("NotAnEnumValue")]
    public void Convert_StringToEnum_InvalidValues_ShouldReturnDefault(string input)
    {
        // Act
        var result = input.Convert<TestEnum1>();

        // Assert
        Assert.Equal(TestEnum1.Value1, result); // Should return default (first value)
    }

    [Fact]
    public void Convert_StringToEnum_NullString_ShouldReturnDefault()
    {
        // Arrange
        string? nullString = null;

        // Act
        var result = nullString!.Convert<TestEnum1>();

        // Assert
        Assert.Equal(TestEnum1.Value1, result); // Should return default (first value)
    }

    [Theory]
    [InlineData("10", TestEnum2.Value1)]
    [InlineData("20", TestEnum2.Value2)]
    [InlineData("30", TestEnum2.Value3)]
    public void Convert_NumericStringToEnum_ValidValues_ShouldReturnCorrectEnum(string input, TestEnum2 expected)
    {
        // Act
        var result = input.Convert<TestEnum2>();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Value1", TestEnum1.Value1)]
    [InlineData("Value2", TestEnum1.Value2)]
    [InlineData("value3", TestEnum1.Value3)] // Case insensitive
    public void Parse_ValidEnumString_ShouldReturnCorrectEnum(string input, TestEnum1 expected)
    {
        // Act
        var result = input.Parse<TestEnum1>();

        // Assert
        Assert.Equal(expected, result);
    }

    // Note: Testing invalid parse scenarios is difficult due to type constraints
    // These would be better tested with integration tests using actual Enum types

    [Fact]
    public void Convert_EnumToEnum_SameEnum_ShouldReturnSameValue()
    {
        // Arrange
        var source = TestEnum1.Value2;

        // Act
        var result = source.Convert<TestEnum1, TestEnum1>();

        // Assert
        Assert.Equal(TestEnum1.Value2, result);
    }
}
