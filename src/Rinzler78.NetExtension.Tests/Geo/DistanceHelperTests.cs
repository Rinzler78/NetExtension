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

        result.Should().Be(2d);
    }
}
