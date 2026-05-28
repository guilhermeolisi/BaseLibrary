using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using static System.Math;
namespace BaseLibrary.Math;

public class MathServices : IMathServices
{
    /// <summary>
    /// Calculate the mean of a set of numbers
    /// </summary>
    /// <param name="x">A set o numbers</param>
    /// <returns>Return the mean</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Mean(IEnumerable<double> x)
    {
        double sum = 0;
        int count = 0;
        foreach (var item in x)
        {
            sum += item;
            count++;
        }

        return sum / count;
    }
    /// <summary>
    /// Calculate the standard deviation of a set of numbers
    /// </summary>
    /// <param name="x">A set o numbers</param>
    /// <returns>The standard deviation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double StandardDeviation(IEnumerable<double> x)
    {
        double mean = Mean(x);
        return StandardDeviation(x, mean);
    }
    /// <summary>
    /// Calculate the standard deviation of a set of numbers
    /// </summary>
    /// <param name="x">A set o numbers</param>
    /// <param name="mean">The mean of the set of numbers</param>
    /// <returns>The standard deviation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double StandardDeviation(IEnumerable<double> x, double mean)
    {
        double sum = 0;
        int count = 0;
        foreach (var item in x)
        {
            double temp = item - mean;
            sum += temp * temp; //Pow(item - mean, 2);
            count++;
        }

        return Sqrt(sum / (count));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double SphereVolumeFromRadius(double radius)
    {
        return (4.0 / 3.0) * PI * radius * radius * radius;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double SphereVolumeFromDiameter(double diameter)
    {
        return (1.0 / 6.0) * PI * diameter * diameter * diameter;
    }
    /// <summary>
    /// Calculate the derivative of a set of numbers using the five-point stencil
    /// </summary>
    /// <param name="y"></param>
    /// <param name="step"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                            // / (12 * Pow(step, 2));
                            / (12 * step * step);
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
                            // / (2 * Pow(step, 3));
                            / (2 * step * step * step);
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
                            // / (Pow(step, 4));
                            / (step * step * step * step);
                    }
                }
                break;
        }

        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    /// <summary>
    /// Find the minimum and maximum of a function using the derivative
    /// </summary>
    /// <param name="y"></param>
    /// <param name="xMin"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    /// <summary>
    /// Find the points where the concavity of a function changes
    /// </summary>
    /// <param name="y"></param>
    /// <param name="xMin"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    /// <summary>
    /// Calculate the interpolation of a point between two points
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Interpolation(in double x1, in double y1, in double x2, in double y2, in double x)
    {
        return y1 + (y2 - y1) * (x - x1) / (x2 - x1);
    }
    /// <summary>
    /// Calculate the hypotenuse of a right triangle
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Hypotenuse(double x, double y)
    {
        return Sqrt(x * x + y * y);
    }
    /// <summary>
    /// Calculate the distance between two points in a plane
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    /// <returns></returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double DistancePoints(double x1, double y1, double x2, double y2)
    {
        return Hypotenuse(x2 - x1, y2 - y1);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ClausenIntegralSerie(double x, double error = 1E-10)
    {
        // Lima 2017 - Clausen Integral
        // https://en.wikipedia.org/wiki/Clausen_function
        double result = 0;
        double resultOld = 1E10;
        int n = 1;
        while (Abs(result - resultOld) > error)
        {
            resultOld = result;
            result += Sin(n * x) / (n * n);
            n++;
        }
        //chega a precisar de 3000 iterações para convergir
        Debug.WriteLine($"Clausen IntegralSerie n = {n}");
        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ClausenIntegral(double x)
    {
        // Implementar o cálculo de ClausenIntegral por polinomio de Chebyshev dado por Kolbig 1995
        // enquanto isso fazer os calculos com ClausenIntegralSerie
        return ClausenIntegralSerie(x);
        return 0;
    }
    ////////
    // This computes an in-place complex-to-complex FFT  
    // x and y are the real and imaginary arrays of 2^m points. 
    // see http://astronomy.swin.edu.au/~pbourke/analysis/dft/ 
    //https://code.msdn.microsoft.com/FFTLibrary-e1942867
    //Fiz modificações do código original para receber um array complexo
    /// <summary>
    /// This computes an in-place complex-to-complex FFT
    /// </summary>
    /// <param name="y"></param>
    /// <param name="isReverse"></param>
    public void FFTCalcule(in Complex[] y, bool isReverse)
    {
        int n, m, i, i1, j, k, i2, l, l1, l2;
        double c1, c2, u1, u2, z;
        Complex ty, t12;

        // Calculate the number of points 
        n = y.Length;
        m = (int)Log(n, 2);
        // Do the bit reversal 
        i2 = n >> 1;
        j = 0;
        for (i = 0; i < n - 1; i++)
        {
            if (i < j)
            {
                ty = y[i];
                y[i] = y[j];
                y[j] = ty;
            }
            k = i2;
            while (k <= j)
            {
                j -= k;
                k >>= 1;
            }
            j += k;
        }
        // Compute the FFT 
        c1 = -1.0;
        c2 = 0.0;
        l2 = 1;
        for (l = 0; l < m; l++)
        {
            l1 = l2;
            l2 <<= 1;
            u1 = 1.0;
            u2 = 0.0;
            for (j = 0; j < l1; j++)
            {
                for (i = j; i < n; i += l2)
                {
                    i1 = i + l1;
                    t12 = new Complex(u1 * y[i1].Real - u2 * y[i1].Imaginary, u1 * y[i1].Imaginary + u2 * y[i1].Real);
                    y[i1] = y[i] - t12;
                    y[i] += t12;
                }
                z = u1 * c1 - u2 * c2;
                u2 = u1 * c2 + u2 * c1;
                u1 = z;
            }
            c2 = Sqrt((1.0 - c1) / 2.0);
            if (!isReverse)
                c2 = -c2;
            c1 = Sqrt((1.0 + c1) / 2.0);
        }
#if DEBUG
        for (i = 0; i < y.Length; i++)
            if (double.IsNaN(y[i].Real))
            { }
#endif
        // Scaling for forward transform 
        //if (!reverse)
        //    for (i = 0; i < n; i++) y[i] /= n;
    }

    //        /* Performs a Bit Reversal Algorithm on a postive integer 
    //         * for given number of bits
    //         * e.g. 011 with 3 bits is reversed to 110 
    //         * https://rosettacode.org/wiki/Fast_Fourier_transform#C.23 */
    //        public static int BitReverse(int n, int bits) {
    //            int reversedN = n;
    //            int count = bits - 1;

    //            n >>= 1;
    //            while (n > 0) {
    //                reversedN = (reversedN << 1) | (n & 1);
    //                count--;
    //                n >>= 1;
    //            }

    //            return ((reversedN << count) & ((1 << bits) - 1));
    //        }

    //        /* Uses Cooley-Tukey iterative in-place algorithm with radix-2 DIT case
    //         * assumes no of points provided are a power of 2 */
    //        public static void FFTCT(Complex[] buffer) {
    //            int bits = (int)Log(buffer.Length, 2);
    //#if false

    //        for (int j = 1; j < buffer.Length / 2; j++) {

    //            int swapPos = BitReverse(j, bits);
    //            var temp = buffer[j];
    //            buffer[j] = buffer[swapPos];
    //            buffer[swapPos] = temp;
    //        }
    //// Said Zandian
    //// The above section of the code is incorrect and does not work correctly and has two bugs.
    //// BUG 1
    //// The bug is that when you reach and index that was swapped previously it does swap it again
    //// Ex. binary value n = 0010 and Bits = 4 as input to BitReverse routine and  returns 4. The code section above //     swaps it. Cells 2 and 4 are swapped. just fine.
    ////     now binary value n = 0010 and Bits = 4 as input to BitReverse routine and returns 2. The code Section
    ////     swap it. Cells 4 and 2 are swapped.     WROOOOONG
    //// 
    //// Bug 2
    //// The code works on the half section of the cells. In the case of Bits = 4 it means that we are having 16 cells
    //// The code works on half the cells        for (int j = 1; j < buffer.Length / 2; j++) buffer.Length returns 16
    //// and divide by 2 makes 8, so j goes from 1 to 7. This covers almost everything but what happened to 1011 value
    //// which must be swap with 1101. and this is the second bug.
    //// 
    //// use the following corrected section of the code. I have seen this bug in other languages that uses bit
    //// reversal routine. 

    //#else
    //            for (int j = 1; j < buffer.Length; j++) {
    //                int swapPos = BitReverse(j, bits);
    //                if (swapPos <= j) {
    //                    continue;
    //                }
    //                var temp = buffer[j];
    //                buffer[j] = buffer[swapPos];
    //                buffer[swapPos] = temp;
    //            }

    //            // First the full length is used and 1011 value is swapped with 1101. Second if new swapPos is less than j
    //            // then it means that swap was happen when j was the swapPos.

    //#endif

    //            for (int N = 2; N <= buffer.Length; N <<= 1) {
    //                for (int i = 0; i < buffer.Length; i += N) {
    //                    for (int k = 0; k < N / 2; k++) {

    //                        int evenIndex = i + k;
    //                        int oddIndex = i + k + (N / 2);
    //                        var even = buffer[evenIndex];
    //                        var odd = buffer[oddIndex];

    //                        double term = -2 * PI * k / (double)N;
    //                        Complex exp = new Complex(Cos(term), Sin(term)) * odd;

    //                        buffer[evenIndex] = even + exp;
    //                        buffer[oddIndex] = even - exp;

    //                    }
    //                }
    //            }
    //        }
}