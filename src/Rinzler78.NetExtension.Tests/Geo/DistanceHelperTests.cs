using Rinzler78.NetExtension.Geo;

namespace Rinzler78.NetExtension.Tests.Geo;

[Trait("Category", "Unit")]
public class DistanceHelperTests
{
    [Fact]
    public void FromMilesToKiloMeters_ShouldReturn1Point60934_WhenInputIs1Mile()
    {
        var result = 1L.FromMilesToKiloMeters();

        // 1 mile * 1609.34 m/mile / 1000 m/km = 1.60934 km
        result.Should().BeApproximately(1.60934d, 0.001d);
    }

    [Fact]
    public void FromMilesToKiloMeters_ShouldReturnZero_WhenInputIsZeroMiles()
    {
        var result = 0L.FromMilesToKiloMeters();

        result.Should().Be(0.0d);
    }

    [Fact]
    public void FromMilesToKiloMeters_ShouldReturn16Point0934_WhenInputIs10Miles()
    {
        var result = 10L.FromMilesToKiloMeters();

        // 10 miles * 1609.34 m/mile / 1000 m/km = 16.0934 km
        result.Should().BeApproximately(16.0934d, 0.001d);
    }

    [Fact]
    public void FromMeterToKiloMeters_ShouldConvertMetersToKilometers()
    {
        var result = 2500L.FromMeterToKiloMeters();

        result.Should().Be(2.5d);
    }

    // ─────────────────────────────────────────────────────────────────
    // Boundary and negative values
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void FromMilesToKiloMeters_WithNegativeValue_ShouldReturnNegative()
    {
        var result = (-5L).FromMilesToKiloMeters();

        result.Should().BeNegative();
        result.Should().BeApproximately(-8.0467d, 0.001d);
    }

    [Fact]
    public void FromMilesToKiloMeters_WithMaxLong_ShouldNotThrow()
    {
        var result = long.MaxValue.FromMilesToKiloMeters();

        result.Should().BePositive();
    }

    [Fact]
    public void FromMeterToKiloMeters_WithZero_ShouldReturnZero()
    {
        var result = 0L.FromMeterToKiloMeters();

        result.Should().Be(0.0d);
    }

    [Fact]
    public void FromMeterToKiloMeters_WithNegativeValue_ShouldReturnNegative()
    {
        var result = (-1500L).FromMeterToKiloMeters();

        result.Should().Be(-1.5d);
    }

    [Fact]
    public void FromMeterToKiloMeters_With1000_ShouldReturnExactlyOne()
    {
        var result = 1000L.FromMeterToKiloMeters();

        result.Should().Be(1.0d);
    }

    [Fact]
    public void FromMilesToKiloMeters_With100_ShouldBeAccurate()
    {
        var result = 100L.FromMilesToKiloMeters();

        result.Should().BeApproximately(160.934d, 0.001d);
    }
}
