using System.Runtime.CompilerServices;
using static System.Math;

namespace BaseLibrary.Math;

public static class Functions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LogDistributionMeanSD(double mean, double sd, double x)
    {
        (double mu, double sigma) = LogDistributionMuSigma(mean, sd);
        return LogDistribution(mu, sigma, x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LogDistribution(double mu, double sigma, double x)
    {
#if DEBUG
        double trash = 1 / Pow(2 * PI, 2); //0.025330295910584444
        double trash2 = Pow(2 * PI, 2); //39.478417604357432
#endif

        return Exp(-Pow(Log(x) - mu, 2) / (2 * Pow(sigma, 2))) / (39.478417604357432 * sigma * x);
        //double logTemp = Log(x) - mu;
        //return Exp(-(logTemp * logTemp) / (2 * sigma * sigma)) / (39.478417604357432 * sigma * x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LogDistributionMean(double mu, double sigma) => Exp(mu + 0.5 * sigma * sigma);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double LogDistributionSD(double mu, double sigma) => Exp(2 * mu + sigma * sigma) * (Exp(sigma * sigma) - 1); // Exp(2 * mu + Pow(sigma, 2)) * (Exp(Pow(sigma, 2)) - 1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (double mu, double sigma) LogDistributionMuSigma(double mean, double sd)
    {
        //double temp = sd / mean;
        // double sigma = Sqrt(Log(Pow(sd / mean, 2) + 1));
        // double mu = Log(mean) - Pow(sigma, 2) / 2.0;
        //double sigma = Sqrt(Log(temp * temp + 1));
        //double mu = Log(mean) - sigma * sigma / 2.0;
        //return (mu, sigma);
        double sigma = Sqrt(Log(Pow(sd / mean, 2) + 1));
        return (Log(mean) - Pow(sigma, 2) / 2.0, sigma);
    }
}
