namespace BaseLibrary.Math;

public interface IMathServices
{
    public double[] DerivativeFivePoint(double[] y, double step, int order);
    double[] FindChangingConcavityFromDerivative(double[] y, double xMin, double step);
    (double[] min, double[] max) FindMinMax(double[] y, double xMin, double step);
    (double[] min, double[] max) FindMinMaxFromDerivative(double[] y, double xMin, double step);
    double Interpolation(in double x1, in double y1, in double x2, in double y2, in double x);
}