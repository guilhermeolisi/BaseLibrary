using static System.Math;

namespace BaseLibrary.Math;

/// <summary>
/// Geradores de numeros aleatorios de proposito geral (uniforme e gaussiano por Box-Muller). Movidos de
/// Sindarin.Math (a classe de funcoes gerais do motor, Onda 2.5; o resto dela virou <see cref="NumericMethods"/>
/// na E269) para BaseLibrary.Math como casa canonica compartilhada
/// entre os nos Randon/RandonGauss e a geracao de ruido do DataXYE. Usa <see cref="Random.Shared"/>
/// (nao semeado e seguro entre threads, portanto nao-deterministico entre execucoes, como o original).
/// </summary>
public static class RandomMethods
{
    // Random.Shared e seguro entre threads (E318); a instancia unica de System.Random nao e.

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
        var u1 = Random.Shared.NextDouble(); //uniform(0,1] random doubles
        var u2 = Random.Shared.NextDouble();

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
        return Random.Shared.NextDouble() * 2 * range - range;
    }
}
