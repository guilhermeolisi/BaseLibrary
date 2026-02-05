using System.Runtime.CompilerServices;
using static System.Math;

namespace BaseLibrary.Math;

public static class MathMethods
{
    static MathServices mathServices = new MathServices();
    //public static double Mean(this IEnumerable<double> x) => mathServices.Mean(x);
    //public static double StandardDeviation(this IEnumerable<double> x) => mathServices.StandardDeviation(x);
    //public static double StandardDeviation(IEnumerable<double> x, double mean) => mathServices.StandardDeviation(x, mean);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SphereVolumeRadius(double radius) => mathServices.SphereVolumeRadius(radius);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SphereVolumeDiameter(double diameter) => mathServices.SphereVolumeDiameter(diameter);
    //public static double Hypotenuse(double x, double y) => mathServices.Hypotenuse(x, y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DistancePoints(double x1, double y1, double x2, double y2) => mathServices.DistancePoints(x1, y1, x2, y2);
    //public static double ClausenIntegralSerie(double x, double error = 1E-10) => mathServices.ClausenIntegralSerie(x, error);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ClausenIntegral(double x) => mathServices.ClausenIntegral(x);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Radians(double degrees)
    {
        return degrees * (PI / 180.0);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Degrees(double radians)
    {
        return radians * (180.0 / PI);

    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RadiusFromPointCartesian(double x, double y, double z)
    {
        return Sqrt(x * x + y * y + z * z);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double PolarFromPointCartesian(double z, double radius)
    {
        return Acos(z / radius);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double AzimuthFromPointCartesian(double x, double y)
    {
        return Atan2(y, x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double XFromPointSpherical(double radius, double theta, double phi)
    {
        return radius * Sin(theta) * Cos(phi);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double YFromPointSpherical(double radius, double theta, double phi)
    {
        return radius * Sin(theta) * Sin(phi);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ZFromPointSpherical(double radius, double theta)
    {
        return radius * Cos(theta);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CosPhi2Directions(double x0, double y0, double z0, double x1, double y1, double z1)
    {
        return (x0 * x1 + y0 * y1 + z0 * z1) / (Sqrt(x0 * x0 + y0 * y0 + z0 * z0) * Sqrt(x1 * x1 + y1 * y1 + z1 * z1));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RemoveZeroNegative(this double value)
    {
        if (value != 0 || double.IsPositive(value))
            return value;
        return 0.0;
    }

}
