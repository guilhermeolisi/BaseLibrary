using static System.Math;
namespace BaseLibrary.Math;

public class MathServices : IMathServices
{
    public double[] DerivativeFivePoint(double[] y, double step, int order)
    {
        //https://en.wikipedia.org/wiki/Five-point_stencil

        if (y.Length < 5 /*|| x.Length < 5 || x.Length != y.Length*/)
        {
            throw new ArgumentException("length of y array must be more than 5, inclusive", nameof(y));
        }

        if (order < 1 || order > 4)
        {
            throw new ArgumentException("order must be between 1 and 4, inclusive", nameof(order));
        }

        double[] result = new double[y.Length - 4];

        switch (order)
        {
            case 1:
                for (int i = 0; i < result.Length; i++)
                {
                    for (int j = -2; j < 2; j++)
                    {
                        result[i] =
                            (j == -2 ? +1 :
                            j == -1 ? -8 :
                            j == 0 ? 0 :
                            j == 1 ? +8 :
                            -1)
                            * y[i + j]
                            / (12 * step);
                    }
                }
                break;
            case 2:
                for (int i = 0; i < result.Length; i++)
                {
                    for (int j = -2; j < 2; j++)
                    {
                        result[i] =
                            (j == -2 ? -1 :
                            j == -1 ? +16 :
                            j == 0 ? -30 :
                            j == 1 ? +16 :
                            -1)
                            * y[i + j]
                            / (12 * Pow(step, 2));
                    }
                }
                break;
            case 3:
                for (int i = 0; i < result.Length; i++)
                {
                    for (int j = -2; j < 2; j++)
                    {
                        if (j == 0)
                            continue;
                        result[i] =
                           (j == -2 ? -1 :
                           j == -1 ? +2 :
                           j == 0 ? 0 :
                           j == 1 ? -2 :
                           +1)
                           * y[i + j]
                           / (2 * Pow(step, 3));
                    }
                }
                break;
            case 4:
                for (int i = 0; i < result.Length; i++)
                {
                    for (int j = -2; j < 2; j++)
                    {
                        if (j == 0)
                            continue;
                        result[i] =
                           (j == -2 ? +1 :
                           j == -1 ? -4 :
                           j == 0 ? +6 :
                           j == 1 ? -4 :
                           +1)
                           * y[i + j]
                           / (Pow(step, 4));
                    }
                }
                break;
        }

        return result;
    }
    public (double[] min, double[] max) FindMinMax(double[] y, double xMin, double step)
    {
        List<double> min = new();
        List<double> max = new();

        for (int i = 1; i < y.Length - 1; i++)
        {
            if (y[i] > y[i - 1] && y[i] > y[i + 1])
            {
                max.Add(xMin + Round((i + 2) * step, 12));
            }
            else if (y[i] < y[i - 1] && y[i] < y[i + 1])
            {
                min.Add(xMin + Round((i + 2) * step, 12));
            }
        }

        return (min.ToArray(), max.ToArray());
    }
    public (double[] min, double[] max) FindMinMaxFromDerivative(double[] y, double xMin, double step)
    {
        double[] derivate = DerivativeFivePoint(y, step, 1);

        List<double> min = new();
        List<double> max = new();

        bool? isTheLastPositive;

        isTheLastPositive = derivate[0] == 0 ? null : derivate[0] < 0 ? false : true;

        double xLast = xMin + Round(2 * step, 12);

        for (int i = 0; i < derivate.Length; i++)
        {
            if (isTheLastPositive == false && derivate[i] >= 0)
            {
                max.Add(xMin + Round((i + 2) * step, 12));
            }
            if (isTheLastPositive == true && derivate[0] <= 0)
            {
                min.Add(xMin + Round((i + 2) * step, 12));
            }
            isTheLastPositive = derivate[i] == 0 ? null : derivate[i] < 0 ? false : true;
        }

        return (min.ToArray(), max.ToArray());
    }
    public double[] FindChangingConcavityFromDerivative(double[] y, double xMin, double step)
    {
        double[] derivate = DerivativeFivePoint(y, step, 2);

        List<double> result = new();

        bool? isTheLastPositive;

        isTheLastPositive = derivate[0] == 0 ? null : derivate[0] < 0 ? false : true;

        double xLast = xMin + Round(2 * step, 12);

        for (int i = 0; i < derivate.Length; i++)
        {
            if ((isTheLastPositive == false && derivate[i] >= 0) || (isTheLastPositive == true && derivate[0] <= 0))
            {
                result.Add(xMin + Round((i + 2) * step, 12));
            }
            isTheLastPositive = derivate[i] == 0 ? null : derivate[i] < 0 ? false : true;
        }

        return result.ToArray();
    }
    public double Interpolation(in double x1, in double y1, in double x2, in double y2, in double x)
    {
        return y1 + (y2 - y1) * (x - x1) / (x2 - x1);
    }
}