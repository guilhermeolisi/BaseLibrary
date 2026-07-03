using static System.Math;

namespace BaseLibrary.Math;

/// <summary>
/// Geradores de numeros aleatorios de proposito geral (uniforme e gaussiano por Box-Muller). Movidos de
/// <c>Sindarin.Math.GeneralMethods</c> para BaseLibrary.Math (Onda 2.5) como casa canonica compartilhada
/// entre os nos Randon/RandonGauss e a geracao de ruido do DataXYE. Instancia unica de <see cref="Random"/>
/// (nao semeada, portanto nao-deterministica entre execucoes, como no original).
/// </summary>
public static class RandomMethods
{
    private static readonly Random random = new Random();

    /// <summary>
    /// Calcule a random number with gaussian probability with a given mean and standard deviation.
    /// </summary>
    /// <param name="mean">Mean of the distribution.</param>
    /// <param name="sd">Standard deviation.</param>
    public static double RandonGaussian(this double mean, double sd)
    {
        var rand_normal = mean + RandonGaussian(sd);

        return rand_normal;
    }
    public static double RandonGaussian(double sd)
    {
        var u1 = random.NextDouble(); //uniform(0,1] random doubles
        var u2 = random.NextDouble();

        var rand_std_normal = Sqrt(-2.0 * Log(u1)) * Sin(2.0 * PI * u2); //random normal(0,1)

        return sd * rand_std_normal;
    }
    public static double Randon(double mean, double range)
    {
        return mean + Randon(range);
    }
    /// <summary>
    /// Returns a random number between -range and +range.
    /// </summary>
    public static double Randon(double range)
    {
        return random.NextDouble() * 2 * range - range;
    }
}
