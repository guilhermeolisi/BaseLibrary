using static System.Math;

namespace BaseLibrary.Math;

public static class Functions
{
    public static double LogDistributionMeanSD(double mean, double sd, double x)
    {
        (double mu, double sigma) = LogDistributionMuSigma(mean, sd);
        return LogDistribution(mu, sigma, x);
    }

    public static double LogDistribution(double mu, double sigma, double x)
    {
#if DEBUG
        double trash = 1 / Pow(2 * PI, 2); //0.025330295910584444
        double trash2 = Pow(2 * PI, 2); //39.478417604357432
#endif

        return Exp(-Pow(Log(x) - mu, 2) / (2 * Pow(sigma, 2))) / (39.478417604357432 * sigma * x);
    }
    public static double LogDistributionMean(double mu, double sigma) => Exp(mu + 0.5 * Pow(sigma, 2));

    public static double LogDistributionSD(double mu, double sigma) => Exp(2 * mu + Pow(sigma, 2)) * (Exp(Pow(sigma, 2)) - 1);
    public static (double mu, double sigma) LogDistributionMuSigma(double mean, double sd)
    {
        double sigma = Sqrt(Log(Pow(sd / mean, 2) + 1));
        double mu = Log(mean) - Pow(sigma, 2.0) / 2.0;
        return (mu, sigma);
    }
}
