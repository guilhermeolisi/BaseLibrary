namespace BaseLibrary.Math;

/// <summary>
/// Distribuições de probabilidade usadas em testes estatísticos (hoje: a distribuição F de Snedecor, que o teste de
/// Hamilton do Sindarin usa para o valor crítico da razão de R).
/// </summary>
public static class Distributions
{
    /// <summary>
    /// Função de distribuição acumulada da F(d1, d2): P(F ≤ x) = I_{d1·x/(d1·x + d2)}(d1/2, d2/2).
    /// </summary>
    public static double FCumulative(double d1, double d2, double x)
    {
        if (d1 <= 0.0) throw new ArgumentOutOfRangeException(nameof(d1), "d1 must be > 0.");
        if (d2 <= 0.0) throw new ArgumentOutOfRangeException(nameof(d2), "d2 must be > 0.");
        if (double.IsNaN(x)) throw new ArgumentOutOfRangeException(nameof(x), "x must be a number.");
        if (x <= 0.0) return 0.0;
        if (double.IsPositiveInfinity(x)) return 1.0;
        double dx = d1 * x;
        return SpecialFunctions.BetaRegularized(d1 / 2.0, d2 / 2.0, dx / (dx + d2));
    }

    /// <summary>
    /// Inversa da acumulada da F(d1, d2): o x com P(F ≤ x) = p, para 0 &lt; p &lt; 1. Bissecção sobre
    /// <see cref="FCumulative"/>, com o limite superior dobrado até cobrir p; tolerância relativa de 1e-12 em x.
    /// </summary>
    public static double FInverseCumulative(double d1, double d2, double p)
    {
        if (d1 <= 0.0) throw new ArgumentOutOfRangeException(nameof(d1), "d1 must be > 0.");
        if (d2 <= 0.0) throw new ArgumentOutOfRangeException(nameof(d2), "d2 must be > 0.");
        if (!(p > 0.0 && p < 1.0)) throw new ArgumentOutOfRangeException(nameof(p), "p must be in (0, 1).");
        double lo = 0.0;
        double hi = 1.0;
        while (FCumulative(d1, d2, hi) < p)
        {
            lo = hi;
            hi *= 2.0;
            if (hi > 1e300) return double.PositiveInfinity;
        }
        for (int i = 0; i < 2000 && hi - lo > 1e-12 * hi; i++)
        {
            double mid = 0.5 * (lo + hi);
            if (FCumulative(d1, d2, mid) < p) lo = mid;
            else hi = mid;
        }
        return 0.5 * (lo + hi);
    }
}
