namespace Rinzler78.NetExtension.Geo;

public static class DistanceHelper
{
    public static double FromMilesToKiloMeters(this long distanceInMile)
    {
        return distanceInMile / 1609.34d;
    }

    public static double FromMeterToKiloMeters(this long distanceInMeter)
    {
        return distanceInMeter / 1000;
    }
}