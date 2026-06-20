using static System.Math;

namespace BaseLibrary.Math;

/// <summary>
/// Funções especiais (Gama, Gama incompleta, função de erro) implementadas internamente para substituir
/// o <c>MathNet.Numerics.SpecialFunctions</c>. As definições e assinaturas seguem as do MathNet:
/// <list type="bullet">
/// <item><see cref="Gamma"/> = Γ(x).</item>
/// <item><see cref="GammaUpperIncomplete"/> = Γ(a,x) = ∫ₓ^∞ t^(a-1)e^(-t) dt (NÃO regularizada).</item>
/// <item><see cref="Erf"/>/<see cref="Erfc"/> = função de erro e seu complemento.</item>
/// </list>
/// Gama por aproximação de Lanczos (g=7); gama incompleta regularizada por série (x &lt; a+1) ou fração
/// contínua (x ≥ a+1), conforme Numerical Recipes. Precisão ~1e-12 ou melhor.
/// </summary>
public static class SpecialFunctions
{
    private const double Epsilon = 1e-15;
    private const double FpMin = 1e-300; // menor positivo p/ evitar divisão por zero na fração contínua
    private static readonly double SqrtPi = Sqrt(PI);

    // Coeficientes de Lanczos (g = 7, 9 termos) — ~15 dígitos.
    private static readonly double[] LanczosG7 =
    {
        0.99999999999980993,
        676.5203681218851,
        -1259.1392167224028,
        771.32342877765313,
        -176.61502916214059,
        12.507343278686905,
        -0.13857109526572012,
        9.9843695780195716e-6,
        1.5056327351493116e-7
    };

    /// <summary>Logaritmo natural da função Gama, ln Γ(x), para x &gt; 0 (Lanczos).</summary>
    public static double GammaLn(double x)
    {
        if (x <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(x), "GammaLn requires x > 0.");
        double z = x - 1.0;
        double a = LanczosG7[0];
        for (int i = 1; i < LanczosG7.Length; i++)
            a += LanczosG7[i] / (z + i);
        double t = z + 7.5; // g + 0.5
        return 0.5 * Log(2.0 * PI) + (z + 0.5) * Log(t) - t + Log(a);
    }

    /// <summary>Função Gama Γ(x) (Lanczos, com reflexão para x &lt; 0.5).</summary>
    public static double Gamma(double x)
    {
        if (x < 0.5)
        {
            // Fórmula de reflexão: Γ(x)·Γ(1-x) = π / sin(πx)
            return PI / (Sin(PI * x) * Gamma(1.0 - x));
        }
        double z = x - 1.0;
        double a = LanczosG7[0];
        for (int i = 1; i < LanczosG7.Length; i++)
            a += LanczosG7[i] / (z + i);
        double t = z + 7.5;
        return Sqrt(2.0 * PI) * Pow(t, z + 0.5) * Exp(-t) * a;
    }

    /// <summary>Gama incompleta inferior REGULARIZADA P(a,x) = γ(a,x)/Γ(a), com a &gt; 0, x ≥ 0.</summary>
    public static double GammaLowerRegularized(double a, double x)
    {
        if (a <= 0.0) throw new ArgumentOutOfRangeException(nameof(a), "a must be > 0.");
        if (x < 0.0) throw new ArgumentOutOfRangeException(nameof(x), "x must be >= 0.");
        if (x == 0.0) return 0.0;
        return x < a + 1.0 ? GammaSeries(a, x) : 1.0 - GammaContinuedFraction(a, x);
    }

    /// <summary>Gama incompleta superior REGULARIZADA Q(a,x) = Γ(a,x)/Γ(a) = 1 − P(a,x).</summary>
    public static double GammaUpperRegularized(double a, double x)
    {
        if (a <= 0.0) throw new ArgumentOutOfRangeException(nameof(a), "a must be > 0.");
        if (x < 0.0) throw new ArgumentOutOfRangeException(nameof(x), "x must be >= 0.");
        if (x == 0.0) return 1.0;
        return x < a + 1.0 ? 1.0 - GammaSeries(a, x) : GammaContinuedFraction(a, x);
    }

    /// <summary>Gama incompleta superior NÃO regularizada Γ(a,x) = Q(a,x)·Γ(a) (mesma definição do MathNet).</summary>
    public static double GammaUpperIncomplete(double a, double x)
        => GammaUpperRegularized(a, x) * Gamma(a);

    /// <summary>Gama incompleta inferior NÃO regularizada γ(a,x) = P(a,x)·Γ(a).</summary>
    public static double GammaLowerIncomplete(double a, double x)
        => GammaLowerRegularized(a, x) * Gamma(a);

    /// <summary>Função de erro erf(x) = (2/√π)∫₀ˣ e^(-t²) dt. Usa P(1/2, x²).</summary>
    public static double Erf(double x)
    {
        if (x == 0.0) return 0.0;
        // Limites em ±∞: erf(±∞)=±1. Necessário pois GammaLowerRegularized(0.5, +∞) avalia
        // Exp(-∞ + 0.5·Log(∞) - …) = Exp(-∞+∞) = NaN; sem este guarda, erf(±∞) retornaria NaN.
        if (double.IsInfinity(x)) return x > 0.0 ? 1.0 : -1.0;
        double p = GammaLowerRegularized(0.5, x * x);
        return x > 0.0 ? p : -p;
    }

    /// <summary>Função de erro complementar erfc(x) = 1 − erf(x), sem cancelamento para x grande.</summary>
    public static double Erfc(double x)
    {
        if (x == 0.0) return 1.0;
        // Limites em ±∞: erfc(+∞)=0, erfc(-∞)=2. Necessário pois GammaUpperRegularized(0.5, +∞)
        // avalia Exp(-∞ + 0.5·Log(∞) - …) = Exp(-∞+∞) = NaN; sem este guarda, erfc(±∞) retornaria NaN
        // (ex.: SizeDistribution lognormal em L=0 faz Log(0)=-∞ → erfc(-∞)).
        if (double.IsInfinity(x)) return x > 0.0 ? 0.0 : 2.0;
        double t = x * x;
        // x>0: erfc = Q(1/2,x²); x<0: erfc = 2 − Q(1/2,x²) (= 1 + P, evita cancelamento)
        return x > 0.0 ? GammaUpperRegularized(0.5, t) : 2.0 - GammaUpperRegularized(0.5, t);
    }

    // ===================================================================================
    //  Bessel modificada I₀/I₁ e Struve modificada L₀/L₁ (usadas na absorção de capilar de Sabine).
    //  Séries de potências de TERMOS POSITIVOS (sem cancelamento interno) → precisão de máquina;
    //  convergem em ~x termos. A combinação I_ν(x)−L_ν(x) (a única usada) tem o mesmo condicionamento
    //  que no MathNet.
    // ===================================================================================

    /// <summary>Função de Bessel modificada de 1ª espécie, ordem 0: I₀(x).</summary>
    public static double BesselI0(double x) => BesselISeries(System.Math.Abs(x), 0); // par

    /// <summary>Função de Bessel modificada de 1ª espécie, ordem 1: I₁(x).</summary>
    public static double BesselI1(double x)
    {
        double v = BesselISeries(System.Math.Abs(x), 1);
        return x < 0.0 ? -v : v; // ímpar
    }

    /// <summary>Função de Struve modificada, ordem 0: L₀(x).</summary>
    public static double StruveL0(double x)
    {
        double v = StruveLSeries(System.Math.Abs(x), 0);
        return x < 0.0 ? -v : v; // ímpar
    }

    /// <summary>Função de Struve modificada, ordem 1: L₁(x).</summary>
    public static double StruveL1(double x) => StruveLSeries(System.Math.Abs(x), 1); // par

    // I_ν(x) = Σ_{k≥0} (x/2)^(2k+ν) / (k!·Γ(ν+k+1)); razão t_k/t_{k-1} = (x/2)²/(k(k+ν)). x ≥ 0.
    private static double BesselISeries(double x, int nu)
    {
        double half = x / 2.0;
        double half2 = half * half;
        double term = nu == 0 ? 1.0 : half; // (x/2)^ν / ν!
        double sum = term;
        for (int k = 1; k < 2000; k++)
        {
            term *= half2 / (k * (k + nu));
            sum += term;
            if (term <= sum * 1e-17 && k > half) break; // só após o pico (k≈x/2)
        }
        return sum;
    }

    // L_ν(x) = Σ_{m≥0} (x/2)^(2m+ν+1) / (Γ(m+3/2)·Γ(m+ν+3/2)); razão = (x/2)²/((m+½)(m+ν+½)). x ≥ 0.
    private static double StruveLSeries(double x, int nu)
    {
        if (x == 0.0) return 0.0;
        double half = x / 2.0;
        double half2 = half * half;
        double term = Pow(half, nu + 1) / (Gamma(1.5) * Gamma(nu + 1.5)); // m=0
        double sum = term;
        for (int m = 1; m < 2000; m++)
        {
            term *= half2 / ((m + 0.5) * (m + nu + 0.5));
            sum += term;
            if (term <= sum * 1e-17 && m > half) break;
        }
        return sum;
    }

    // P(a,x) por série (convergente para x < a+1).
    private static double GammaSeries(double a, double x)
    {
        double ap = a;
        double sum = 1.0 / a;
        double del = sum;
        for (int n = 0; n < 1000; n++)
        {
            ap += 1.0;
            del *= x / ap;
            sum += del;
            if (Abs(del) < Abs(sum) * Epsilon)
                break;
        }
        return sum * Exp(-x + a * Log(x) - GammaLn(a));
    }

    // Q(a,x) por fração contínua (convergente para x >= a+1) — Lentz modificado.
    private static double GammaContinuedFraction(double a, double x)
    {
        double b = x + 1.0 - a;
        double c = 1.0 / FpMin;
        double d = 1.0 / b;
        double h = d;
        for (int i = 1; i <= 1000; i++)
        {
            double an = -i * (i - a);
            b += 2.0;
            d = an * d + b;
            if (Abs(d) < FpMin) d = FpMin;
            c = b + an / c;
            if (Abs(c) < FpMin) c = FpMin;
            d = 1.0 / d;
            double del = d * c;
            h *= del;
            if (Abs(del - 1.0) < Epsilon)
                break;
        }
        return Exp(-x + a * Log(x) - GammaLn(a)) * h;
    }
}
