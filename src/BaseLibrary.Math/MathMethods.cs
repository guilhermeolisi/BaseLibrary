namespace BaseLibrary.Math;

public static class MathMethods
{
    static MathServices mathServices = new MathServices();
    public static double Mean(this IEnumerable<double> x) => mathServices.Mean(x);
    public static double StandardDeviation(this IEnumerable<double> x) => mathServices.StandardDeviation(x);
    public static double StandardDeviation(this IEnumerable<double> x, double mean) => mathServices.StandardDeviation(x, mean);
    //public static double Mean(this double[] x) => mathServices.Mean(x);
    //public static double StandardDeviation(this double[] x) => mathServices.StandardDeviation(x);
    //public static double StandardDeviation(this double[] x, double mean) => mathServices.StandardDeviation(x, mean);
    public static double SphereVolumeRadius(double radius) => mathServices.SphereVolumeRadius(radius);
    public static double SphereVolumeDiameter(double diameter) => mathServices.SphereVolumeDiameter(diameter);
    public static double Hypotenuse(double x, double y) => mathServices.Hypotenuse(x, y);
    public static double DistancePoints(double x1, double y1, double x2, double y2) => mathServices.DistancePoints(x1, y1, x2, y2);
    public static double ClausenIntegralSerie(double x, double error = 1E-10) => mathServices.ClausenIntegralSerie(x, error);
    public static double ClausenIntegral(double x) => mathServices.ClausenIntegral(x);
}
