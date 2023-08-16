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
}
