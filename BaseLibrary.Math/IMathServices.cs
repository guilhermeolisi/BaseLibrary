namespace BaseLibrary.Math;

public interface IMathServices
{
    double ClausenIntegralSerie(double x, double error = 1E-10);
    public double[] DerivativeFivePoint(double[] y, double step, int order);
    double DistancePoints(double x1, double y1, double x2, double y2);
    double[] FindChangingConcavityFromDerivative(double[] y, double xMin, double step);
    (double[] min, double[] max) FindMinMax(double[] y, double xMin, double step);
    (double[] min, double[] max) FindMinMaxFromDerivative(double[] y, double xMin, double step);
    double Hypotenuse(double x, double y);
    double Interpolation(in double x1, in double y1, in double x2, in double y2, in double x);
}