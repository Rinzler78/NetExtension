namespace Rinzler78.NetExtension.Geo;

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
    public static double FromMilesToKiloMeters(this long distanceInMile)
    {
        return distanceInMile * MilesToMetersConversionFactor / MetersToKilometersConversionFactor;
    }

    public static double FromMeterToKiloMeters(this long distanceInMeter)
    {
        return distanceInMeter / MetersToKilometersConversionFactor;
    }
}
