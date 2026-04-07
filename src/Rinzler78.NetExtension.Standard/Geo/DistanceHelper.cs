namespace Rinzler78.NetExtension.Geo;

/// <summary>
/// Provides extension methods for converting distance units.
/// </summary>
public static class DistanceHelper
{
    #region Constants

    /// <summary>
    /// Conversion factor from miles to meters.
    /// </summary>
    private const double MilesToMetersConversionFactor = 1609.34d;

    /// <summary>
    /// Conversion factor from meters to kilometers.
    /// </summary>
    private const int MetersToKilometersConversionFactor = 1000;

    #endregion
    /// <summary>
    /// Converts a distance in miles to kilometers.
    /// </summary>
    /// <param name="distanceInMile">The distance in miles.</param>
    /// <returns>The equivalent distance in kilometers.</returns>
    public static double FromMilesToKiloMeters(this long distanceInMile)
    {
        return distanceInMile * MilesToMetersConversionFactor / MetersToKilometersConversionFactor;
    }

    /// <summary>
    /// Converts a distance in meters to kilometers.
    /// </summary>
    /// <param name="distanceInMeter">The distance in meters.</param>
    /// <returns>The equivalent distance in kilometers.</returns>
    public static double FromMeterToKiloMeters(this long distanceInMeter)
    {
        return distanceInMeter / (double)MetersToKilometersConversionFactor;
    }
}
