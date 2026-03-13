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
    /// <summary>
    /// Calculates the maximum possible radius of a cross-section of a cylinder, given its height, diameter, and
    /// orientation parameters.
    /// </summary>
    /// <remarks>This method is useful for determining the largest possible cross-sectional radius of a
    /// cylinder when it is arbitrarily oriented in space. The calculation accounts for both the cylinder's dimensions
    /// and its rotation angles.</remarks>
    /// <param name="H">The height of the cylinder. Must be a positive value.</param>
    /// <param name="D">The diameter of the cylinder. Must be a positive value.</param>
    /// <param name="polar">The polar angle, in radians, specifying the orientation of the cross-section relative to the cylinder's axis.</param>
    /// <param name="azimutal">The azimuthal angle, in radians, specifying the orientation of the cross-section around the cylinder's axis.</param>
    /// <param name="polarRot">The polar rotation angle, in radians, representing the cylinder's rotation about its axis.</param>
    /// <param name="azimutalRot">The azimuthal rotation angle, in radians, representing the cylinder's rotation around its axis.</param>
    /// <returns>The maximum radius, as a double, of the cross-section of the cylinder for the specified orientation. The value
    /// is constrained by the cylinder's height and diameter.</returns>
    public static double CylinderRadius(double H, double D, double polar, double azimutal, double polarRot, double azimutalRot)
    {
        // https://chatgpt.com/c/69922192-0a30-832c-8070-6b042d303232
        //equação sem rotação:
        // r = Min(D / 2 / Sin(polar), H / 2 / Abs(Cos(polar)));
        double cosGamma = Cos(polar) * Cos(polarRot) + Sin(polar) * Sin(polarRot) * Cos(azimutal - azimutalRot);
        double sinGamma = Sqrt(1 - Pow(cosGamma, 2));
        return Min(D / 2 / sinGamma, H / 2 / Abs(cosGamma));
    }
}
