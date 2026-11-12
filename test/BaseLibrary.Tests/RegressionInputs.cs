using System.Numerics;

namespace BaseLibrary.Tests;

/// <summary>
/// Entradas fixas da regressao bit a bit da E269. So soma, produto e divisao (sem funcao transcendente), para a
/// entrada ser a mesma em qualquer maquina; a saida foi capturada de Sindarin.Math.GeneralMethods e
/// Sindarin.Math.GeometryMethods no master antes da mudanca de casa.
/// </summary>
internal static class RegressionInputs
{
    /// <summary>Sinal de 128 pontos: dois picos lorentzianos, fundo linear e um serrilhado inteiro.</summary>
    public static double[] Signal(int n = 128)
    {
        var y = new double[n];
        for (int i = 0; i < n; i++)
        {
            double d1 = (i - 40) / 5.0;
            double d2 = (i - 90) / 8.0;
            y[i] = 100.0 / (1.0 + d1 * d1) + 30.0 / (1.0 + d2 * d2) + 10.0 + 0.05 * i + ((i * 37) % 11 - 5) * 0.25;
        }
        return y;
    }

    /// <summary>Grade regular de 128 pontos de 10 a 73.5 graus (passo 0.5).</summary>
    public static double[] Grid(int n = 128)
    {
        var x = new double[n];
        for (int i = 0; i < n; i++) x[i] = 10.0 + 0.5 * i;
        return x;
    }

    /// <summary>Contagens positivas (para a deviance de Poisson e o Whittaker ponderado).</summary>
    public static double[] Weights(int n = 128)
    {
        var w = new double[n];
        for (int i = 0; i < n; i++) w[i] = 1.0 + (i % 7) * 0.125;
        return w;
    }

    /// <summary>Matriz 4x4 simetrica positiva definida, em forma densa.</summary>
    public static double[,] DenseSpd() => new double[,]
    {
        { 4.0, 1.0, 0.5, 0.25 },
        { 1.0, 5.0, 1.0, 0.5 },
        { 0.5, 1.0, 6.0, 1.0 },
        { 0.25, 0.5, 1.0, 7.0 },
    };

    public static double[] DenseRhs() => new[] { 1.0, -2.0, 3.0, 0.5 };

    /// <summary>Banda simetrica positiva definida de n = 12 e meia-largura 2.</summary>
    public static (double[] diagonal, double[][] superDiagonals) Band(int n = 12)
    {
        var diag = new double[n];
        var s1 = new double[n - 1];
        var s2 = new double[n - 2];
        for (int i = 0; i < n; i++) diag[i] = 6.0 + (i % 3) * 0.5;
        for (int i = 0; i < n - 1; i++) s1[i] = -1.0 - (i % 2) * 0.25;
        for (int i = 0; i < n - 2; i++) s2[i] = 0.5;
        return (diag, new[] { s1, s2 });
    }

    public static double[] BandRhs(int n = 12)
    {
        var b = new double[n];
        for (int i = 0; i < n; i++) b[i] = (i % 5) - 2.0;
        return b;
    }

    /// <summary>Tensor metrico de uma cela monoclinica (a=5, b=6, c=7, beta com cos = -0.25), montado sem raiz nem cosseno.</summary>
    public static double[,] MonoclinicG() => new double[,]
    {
        { 25.0, 0.0, 5.0 * 7.0 * -0.25 },
        { 0.0, 36.0, 0.0 },
        { 5.0 * 7.0 * -0.25, 0.0, 49.0 },
    };

    public static Complex[] ComplexSeries() => new[] { new Complex(1.5, 0.25), new Complex(4.0, -1.0), new Complex(-2.0, 3.0) };
}
