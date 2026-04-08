using FluentAssertions;
using Newtonsoft.Json;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Math;

namespace Rinzler78.NetExtension.Tests.Math;

[Trait("Category", "Unit")]
public class DecimalHelperTests
{
    // ── Pow ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Pow_PositiveExponent_ShouldReturnExpectedPower()
    {
        DecimalHelper.Pow(2m, 3m).Should().Be(8m);
    }

    /// <summary>
    /// Specification: x^0 = 1 for any x, including 0 (matches System.Math.Pow(0,0) == 1.0).
    /// </summary>
    [Fact]
    public void Pow_ZeroBase_ZeroExponent_ShouldReturnOne()
    {
        DecimalHelper.Pow(0m, 0m).Should().Be(1m);
    }

    /// <summary>
    /// 2^(-1) = 0.5 — negative exponents produce fractional results.
    /// </summary>
    [Fact]
    public void Pow_NegativeExponent_ShouldReturnFraction()
    {
        DecimalHelper.Pow(2m, -1m).Should().Be(0.5m);
    }

    /// <summary>
    /// 2^0.5 = √2 ≈ 1.4142 — fractional exponents produce irrational results.
    /// </summary>
    [Fact]
    public void Pow_FractionalExponent_ShouldApproximateSqrt()
    {
        DecimalHelper.Pow(2m, 0.5m).Should().BeApproximately(1.4142m, 0.0001m);
    }

    // ── Log ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Log_OfOne_ShouldReturnZero()
    {
        1m.Log().Should().Be(0m);
    }

    /// <summary>
    /// log(e) = 1 by definition of the natural logarithm.
    /// </summary>
    [Fact]
    public void Log_OfE_ShouldApproximateOne()
    {
        ((decimal)System.Math.E).Log().Should().BeApproximately(1m, 0.0001m);
    }

    /// <summary>
    /// System.Math.Log(0) returns -Infinity; casting -Infinity to decimal throws
    /// OverflowException because decimal has no representation for infinity.
    /// </summary>
    [Fact]
    public void Log_OfZero_ShouldThrowOverflowException()
    {
        var ex = Record.Exception(() => 0m.Log());
        ex.Should().BeOfType<OverflowException>(
            "Math.Log(0) == -Infinity, which cannot be represented as a decimal");
    }

    /// <summary>
    /// System.Math.Log(-1) returns NaN; casting NaN to decimal throws
    /// OverflowException because decimal has no representation for NaN.
    /// </summary>
    [Fact]
    public void Log_OfNegative_ShouldThrowOverflowException()
    {
        var ex = Record.Exception(() => (-1m).Log());
        ex.Should().BeOfType<OverflowException>(
            "Math.Log(-1) == NaN, which cannot be represented as a decimal");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Pow_IntegerExponent_ShouldBeExact()
    {
        // 2^10 = 1024 — must be exact in decimal
        DecimalHelper.Pow(2m, 10m).Should().Be(1024m);
    }

    // ─────────────────────────────────────────────────────────────────
    // Additional Pow / Log edge cases
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Pow_WithOneBase_AnyExponent_ShouldReturnOne()
    {
        // 1^x = 1 for any x
        DecimalHelper.Pow(1m, 100m).Should().Be(1m);
        DecimalHelper.Pow(1m, -50m).Should().Be(1m);
        DecimalHelper.Pow(1m, 0.5m).Should().BeApproximately(1m, 0.0001m);
    }

    [Fact]
    public void Pow_NegativeBase_EvenExponent_ShouldReturnPositive()
    {
        // (-2)^2 = 4
        DecimalHelper.Pow(-2m, 2m).Should().Be(4m);
    }

    [Fact]
    public void Pow_NegativeBase_OddExponent_ShouldReturnNegative()
    {
        // (-2)^3 = -8
        DecimalHelper.Pow(-2m, 3m).Should().Be(-8m);
    }

    [Fact]
    public void Log_OfLargeValue_ShouldReturnCorrectApproximation()
    {
        // log(1000000) ≈ 13.8155
        var result = 1000000m.Log();
        result.Should().BeApproximately(13.8155m, 0.001m);
    }
}

public static class DecimalHelperAdditionalTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public static void Log_OfVerySmallPositiveValue_ShouldReturnLargeNegativeValue()
    {
        // Log(epsilon) = very large negative value — no exception expected
        var result = (0.0000001m).Log();
        result.Should().BeLessThan(-10m);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public static void DeSerialize_WithSettings_OnNullString_ShouldReturnDefault()
    {
        // Covers the early-return branch of Deserialize<T>(string?, settings)
        string? nullStr = null;
        var settings = new Newtonsoft.Json.JsonSerializerSettings();
        var result = nullStr.Deserialize<int>(settings);
        result.Should().Be(default(int));
    }
}
