using BaseLibrary.Math;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// A distribuição F, usada pelo teste de Hamilton (razão de R) do Sindarin.Analysis para o valor crítico.
/// </summary>
public class DistributionsTests
{
    [Theory]
    // Valores críticos das tabelas F usuais (quatro algarismos significativos).
    [InlineData(1.0, 10.0, 0.95, 4.9646)]
    [InlineData(2.0, 20.0, 0.95, 3.4928)]
    [InlineData(5.0, 30.0, 0.99, 3.6990)]
    [InlineData(10.0, 120.0, 0.95, 1.9105)]
    public void FInverseCumulative_BateComATabelaF(double d1, double d2, double p, double expected)
        => Distributions.FInverseCumulative(d1, d2, p).Should().BeApproximately(expected, 1e-3);

    [Theory]
    // Com d1 = 2 a acumulada tem forma fechada: P(F ≤ x) = 1 − (1 + 2x/d2)^(−d2/2).
    [InlineData(20.0, 3.4928)]
    [InlineData(7.0, 0.8)]
    [InlineData(150.0, 12.0)]
    public void FCumulative_ComD1Dois_BateComAFormaFechada(double d2, double x)
    {
        double expected = 1.0 - System.Math.Pow(1.0 + 2.0 * x / d2, -d2 / 2.0);
        Distributions.FCumulative(2.0, d2, x).Should().BeApproximately(expected, 1e-12);
    }

    [Theory]
    [InlineData(1.0, 10.0, 0.95)]
    [InlineData(3.0, 1500.0, 0.05)]
    [InlineData(25.0, 8.0, 0.999)]
    public void FInverseCumulative_EAInversaDaAcumulada(double d1, double d2, double p)
        => Distributions.FCumulative(d1, d2, Distributions.FInverseCumulative(d1, d2, p)).Should().BeApproximately(p, 1e-9);

    [Fact]
    public void FCumulative_ForaDoSuporte_DaZero()
        => Distributions.FCumulative(3.0, 9.0, -1.0).Should().Be(0.0);

    [Theory]
    [InlineData(0.0, 10.0, 0.5)]
    [InlineData(1.0, -2.0, 0.5)]
    [InlineData(1.0, 10.0, 0.0)]
    [InlineData(1.0, 10.0, 1.0)]
    public void FInverseCumulative_ComArgumentoInvalido_DeveRecusar(double d1, double d2, double p)
        => FluentActions.Invoking(() => Distributions.FInverseCumulative(d1, d2, p))
            .Should().Throw<ArgumentOutOfRangeException>();
}
