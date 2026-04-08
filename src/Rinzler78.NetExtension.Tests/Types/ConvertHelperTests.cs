using System;
using FluentAssertions;
using Rinzler78.NetExtension.Types;

namespace Rinzler78.NetExtension.Tests.Types;

[Trait("Category", "Unit")]
public class ConvertHelperTests
{
    [Fact]
    public void ConvertValue_IntegerString_ShouldConvertToInt()
    {
        "42".ConvertValue<int>().Should().Be(42);
    }

    /// <summary>
    /// ConvertHelper uses Convert.ChangeType with InvariantCulture.
    /// "3.14" with invariant culture parses the dot as decimal separator → 3.14.
    /// </summary>
    [Fact]
    public void ConvertValue_DecimalString_ShouldConvertToDouble()
    {
        "3.14".ConvertValue<double>().Should().BeApproximately(3.14, 0.0001);
    }

    [Fact]
    public void ConvertValue_BooleanString_ShouldConvertToBool()
    {
        "true".ConvertValue<bool>().Should().BeTrue();
    }

    /// <summary>
    /// "2024-01-15" is recognized by Convert.ChangeType(InvariantCulture) as January 15, 2024.
    /// </summary>
    [Fact]
    public void ConvertValue_DateString_ShouldConvertToDateTime()
    {
        "2024-01-15".ConvertValue<DateTime>().Should().Be(new DateTime(2024, 1, 15));
    }

    /// <summary>
    /// Convert.ChangeType("invalid", typeof(int)) calls int.Parse internally which
    /// throws FormatException for non-numeric input.
    /// </summary>
    [Fact]
    public void ConvertValue_InvalidStringToInt_ShouldThrowFormatException()
    {
        var ex = Record.Exception(() => "invalid".ConvertValue<int>());
        ex.Should().BeOfType<FormatException>(
            "Convert.ChangeType delegates to int.Parse which throws FormatException for non-numeric strings");
    }

    /// <summary>
    /// Convert.ChangeType(null, typeof(int)) throws InvalidCastException for non-nullable
    /// value types because null cannot be represented as an int.
    /// </summary>
    [Fact]
    public void ConvertValue_NullToValueType_ShouldThrowInvalidCastException()
    {
        object? nullValue = null;
        var ex = Record.Exception(() => nullValue!.ConvertValue<int>());
        ex.Should().BeOfType<InvalidCastException>(
            "Convert.ChangeType throws InvalidCastException when converting null to a non-nullable value type");
    }

    // ─────────────────────────────────────────────────────────────────
    // Additional type conversions
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ConvertValue_GuidString_ShouldThrowInvalidCastException()
    {
        // Convert.ChangeType does not support Guid conversion from string.
        var guid = Guid.NewGuid();

        var ex = Record.Exception(() => guid.ToString().ConvertValue<Guid>());

        ex.Should().BeOfType<InvalidCastException>();
    }

    [Fact]
    public void ConvertValue_IntToLong_ShouldWiden()
    {
        42.ConvertValue<long>().Should().Be(42L);
    }

    [Fact]
    public void ConvertValue_DoubleToDecimal_ShouldConvert()
    {
        3.14.ConvertValue<decimal>().Should().BeApproximately(3.14m, 0.0001m);
    }
}
