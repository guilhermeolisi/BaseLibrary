using BaseLibrary.Math;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// E270: as copias privadas de mediana do motor e do SindarinAI trocadas por <see cref="NumericMethods.Median"/> e
/// <see cref="NumericMethods.MedianOrZeroWhenEmpty"/>, bit a bit. Cada copia antiga vai escrita aqui como estava no
/// commit de partida (Sindarin 9d1b3106, SindarinAI 0b3b9a4), e o teste a compara com a casa em casos a mao (par,
/// impar, ±0, subnormal, infinito, NaN) e em 1000 vetores sorteados com semente fixa.
/// </summary>
public class NumericMethodsMedianCopiesTests
{
    // Oraculo: regressao, copia de Sindarin/src/Analysis/PhaseResidualDiagnostics.cs:320 no commit 9d1b3106
    private static double PhaseResidualDiagnosticsMedian(List<double> values)
    {
        double[] sorted = [.. values];
        Array.Sort(sorted);
        int middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2.0;
    }

    // Oraculo: regressao, copia de Sindarin/src/Math/Sindarin.Math/Statistic.cs:262 no commit 9d1b3106
    private static double StatisticMedian(double[] values)
    {
        var copy = (double[])values.Clone();
        Array.Sort(copy);
        int m = copy.Length;
        return m % 2 == 1 ? copy[m / 2] : 0.5 * (copy[m / 2 - 1] + copy[m / 2]);
    }

    // Oraculo: regressao, copia de SindarinAI/src/SindarinAI.GR2R/Data/NoiseEstimator.cs:34 no commit 0b3b9a4
    private static double NoiseEstimatorMedian(double[] values)
    {
        var copy = (double[])values.Clone();
        Array.Sort(copy);
        int m = copy.Length;
        return m % 2 == 1 ? copy[m / 2] : 0.5 * (copy[m / 2 - 1] + copy[m / 2]);
    }

    // Oraculo: regressao, copia de SindarinAI/src/SindarinAI.XrdSim.Trainer/SimCommands.cs:743 no commit 0b3b9a4
    private static double SimCommandsMedian(double[] v)
    {
        if (v.Length == 0) return 0;
        var c = (double[])v.Clone(); Array.Sort(c);
        return c.Length % 2 == 1 ? c[c.Length / 2] : 0.5 * (c[c.Length / 2 - 1] + c[c.Length / 2]);
    }

    // Oraculo: regressao, copia de SindarinAI/src/SindarinAI.GR2R.Trainer/Training/PatternFeatures.cs:89 no commit 0b3b9a4
    private static double PatternFeaturesMedian(IReadOnlyList<double> v)
    {
        int n = v.Count;
        if (n == 0) return 0;
        var a = new double[n];
        for (int i = 0; i < n; i++) a[i] = v[i];
        Array.Sort(a);
        return n % 2 == 1 ? a[n / 2] : 0.5 * (a[n / 2 - 1] + a[n / 2]);
    }

    private static long Bits(double x) => BitConverter.DoubleToInt64Bits(x);

    /// <summary>Casos a mao (nao vazios) mais 1000 sorteios com tamanho de 1 a 24 e valores de varias escalas.</summary>
    private static List<double[]> Inputs()
    {
        var list = new List<double[]>
        {
            new[] { 3.0 },
            new[] { 1.0, 2.0 },
            new[] { -0.0, 0.0 },
            new[] { 0.0, -0.0 },
            new[] { -0.0, 0.0, -0.0 },
            new[] { double.Epsilon, 3 * double.Epsilon },
            new[] { double.MaxValue, double.MaxValue },
            new[] { double.NegativeInfinity, 1.0, double.PositiveInfinity, 2.0 },
            new[] { double.NaN, 1.0, 2.0 },
            new[] { 0.1, 0.2, 0.3, 0.4 },
            new[] { 5.0, -1.0, 4.0, 2.5, 2.5, 7.0 },
        };
        var random = new Random(270);
        for (int i = 0; i < 1000; i++)
        {
            int n = 1 + random.Next(24);
            var v = new double[n];
            double scale = System.Math.Pow(10, random.Next(-300, 300));
            for (int j = 0; j < n; j++)
                v[j] = (random.NextDouble() - 0.5) * scale;
            list.Add(v);
        }
        return list;
    }

    [Fact]
    public void Median_ShouldMatchThePhaseResidualDiagnosticsCopyBitForBit()
    {
        int compared = 0;
        foreach (double[] v in Inputs())
        {
            Bits(NumericMethods.Median([.. v])).Should().Be(Bits(PhaseResidualDiagnosticsMedian([.. v])));
            compared++;
        }
        compared.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Median_ShouldMatchTheStatisticCopyBitForBit()
    {
        int compared = 0;
        foreach (double[] v in Inputs())
        {
            Bits(NumericMethods.Median((double[])v.Clone())).Should().Be(Bits(StatisticMedian(v)));
            compared++;
        }
        compared.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Median_ShouldMatchTheNoiseEstimatorCopyBitForBit()
    {
        int compared = 0;
        foreach (double[] v in Inputs())
        {
            Bits(NumericMethods.Median((double[])v.Clone())).Should().Be(Bits(NoiseEstimatorMedian(v)));
            compared++;
        }
        compared.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void MedianOrZeroWhenEmpty_ShouldMatchTheSimCommandsCopyBitForBit()
    {
        int compared = 0;
        foreach (double[] v in Inputs().Append([]))
        {
            Bits(NumericMethods.MedianOrZeroWhenEmpty((double[])v.Clone())).Should().Be(Bits(SimCommandsMedian(v)));
            compared++;
        }
        compared.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void MedianOrZeroWhenEmpty_ShouldMatchThePatternFeaturesCopyBitForBit()
    {
        int compared = 0;
        foreach (double[] v in Inputs().Append([]))
        {
            Bits(NumericMethods.MedianOrZeroWhenEmpty([.. v])).Should().Be(Bits(PatternFeaturesMedian(v)));
            compared++;
        }
        compared.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void MedianOrZeroWhenEmpty_ShouldReturnZero_WhenEmpty()
    {
        // Oraculo: a mao, o guarda das copias do SindarinAI devolve 0 para vetor vazio
        NumericMethods.MedianOrZeroWhenEmpty([]).Should().Be(0);
    }

    [Fact]
    public void MedianOrZeroWhenEmpty_ShouldBeTheMiddleOrTheMeanOfTheTwoMiddles()
    {
        // Oraculo: a mao, {1, 3, 2} ordenado da 2; {4, 1, 3, 2} ordenado da (2 + 3) / 2 = 2,5; valores exatos em double
        NumericMethods.MedianOrZeroWhenEmpty([1.0, 3.0, 2.0]).Should().Be(2.0);
        NumericMethods.MedianOrZeroWhenEmpty([4.0, 1.0, 3.0, 2.0]).Should().Be(2.5);
    }
}
