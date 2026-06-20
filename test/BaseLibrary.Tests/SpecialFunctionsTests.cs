using FluentAssertions;
using BaseLibrary.Math;

namespace BaseLibrary.Tests;

/// <summary>
/// Valida as funções especiais próprias (substitutas do MathNet.Numerics.SpecialFunctions) contra
/// valores de referência conhecidos (alta precisão).
/// </summary>
public class SpecialFunctionsTests
{
    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(2.0, 1.0)]
    [InlineData(5.0, 24.0)]                 // Γ(5) = 4! = 24
    [InlineData(0.5, 1.7724538509055160)]   // Γ(1/2) = √π
    [InlineData(4.5, 11.631728396567448)]
    [InlineData(0.1, 9.5135076986687319)]
    public void Gamma_ShouldMatchReference(double x, double expected)
    {
        SpecialFunctions.Gamma(x).Should().BeApproximately(expected, Abs(expected) * 1e-11 + 1e-12);
    }

    [Fact]
    public void Gamma_ShouldHandleNegativeViaReflection()
    {
        // Γ(-0.5) = -2√π ≈ -3.5449077018
        SpecialFunctions.Gamma(-0.5).Should().BeApproximately(-3.5449077018110318, 1e-10);
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(0.5, 0.5204998778130465)]
    [InlineData(1.0, 0.8427007929497149)]
    [InlineData(2.0, 0.9953222650189527)]
    [InlineData(-1.0, -0.8427007929497149)]
    public void Erf_ShouldMatchReference(double x, double expected)
    {
        SpecialFunctions.Erf(x).Should().BeApproximately(expected, 1e-11);
    }

    [Theory]
    [InlineData(0.0, 1.0)]
    [InlineData(1.0, 0.15729920705028513)]
    [InlineData(2.0, 0.0046777349810472651)]
    [InlineData(-1.0, 1.8427007929497149)]
    public void Erfc_ShouldMatchReference(double x, double expected)
    {
        SpecialFunctions.Erfc(x).Should().BeApproximately(expected, 1e-11 + Abs(expected) * 1e-11);
    }

    [Fact]
    public void ErfAndErfc_ShouldSumToOne()
    {
        foreach (double x in new[] { -2.0, -0.3, 0.7, 1.5, 3.0 })
            (SpecialFunctions.Erf(x) + SpecialFunctions.Erfc(x)).Should().BeApproximately(1.0, 1e-12);
    }

    // Regressão: erf/erfc em ±∞ devolviam NaN (GammaRegularized(0.5, +∞) avalia Exp(-∞+∞)=NaN).
    // Quebrava o WPPM SizeDistribution lognormal em L=0 (Log(0)=-∞ → erfc(-∞)), gerando NaN em todo o Yc.
    [Theory]
    [InlineData(double.PositiveInfinity, 1.0)]
    [InlineData(double.NegativeInfinity, -1.0)]
    public void Erf_ShouldReturnLimit_AtInfinity(double x, double expected)
    {
        SpecialFunctions.Erf(x).Should().Be(expected);
    }

    [Theory]
    [InlineData(double.PositiveInfinity, 0.0)]
    [InlineData(double.NegativeInfinity, 2.0)]
    public void Erfc_ShouldReturnLimit_AtInfinity(double x, double expected)
    {
        SpecialFunctions.Erfc(x).Should().Be(expected);
    }

    [Theory]
    // Γ(1,x) = e^{-x}
    [InlineData(1.0, 2.0, 0.1353352832366127)]
    // Γ(2,1) = (1+1)e^{-1} = 2/e
    [InlineData(2.0, 1.0, 0.7357588823428847)]
    // Γ(3,2) = (x²+2x+2)e^{-x} = 10 e^{-2}
    [InlineData(3.0, 2.0, 1.3533528323661270)]
    public void GammaUpperIncomplete_ShouldMatchReference(double a, double x, double expected)
    {
        SpecialFunctions.GammaUpperIncomplete(a, x).Should().BeApproximately(expected, Abs(expected) * 1e-10 + 1e-12);
    }

    [Fact]
    public void GammaRegularized_LowerPlusUpper_ShouldBeOne()
    {
        foreach ((double a, double x) in new[] { (0.5, 0.3), (2.0, 1.0), (5.0, 7.0), (3.0, 2.5) })
            (SpecialFunctions.GammaLowerRegularized(a, x) + SpecialFunctions.GammaUpperRegularized(a, x))
                .Should().BeApproximately(1.0, 1e-12);
    }

    [Theory]
    [InlineData(1.0, 1.2660658777520084)]
    [InlineData(2.0, 2.2795853023360673)]
    [InlineData(5.0, 27.239871823604442)]
    [InlineData(10.0, 2815.7166284662544)]
    public void BesselI0_ShouldMatchReference(double x, double expected)
        => SpecialFunctions.BesselI0(x).Should().BeApproximately(expected, Abs(expected) * 1e-10);

    [Theory]
    [InlineData(1.0, 0.5651591039924850)]
    [InlineData(2.0, 1.5906368546373291)]
    [InlineData(5.0, 24.335642142450530)]
    [InlineData(10.0, 2670.9883037012543)]
    public void BesselI1_ShouldMatchReference(double x, double expected)
        => SpecialFunctions.BesselI1(x).Should().BeApproximately(expected, Abs(expected) * 1e-9);

    [Fact]
    public void BesselI1_ShouldBeOdd()
        => SpecialFunctions.BesselI1(-3.0).Should().BeApproximately(-SpecialFunctions.BesselI1(3.0), 1e-12);

    [Theory]
    // Termo dominante (x→0): L_ν(x) ≈ (x/2)^(ν+1)/(Γ(3/2)Γ(ν+3/2)) — independente dos termos altos.
    [InlineData(0.01, 0.0063661977)]   // L0: (0.005)/(Γ1.5²)
    public void StruveL0_LeadingTerm_ShouldMatch(double x, double expected)
        => SpecialFunctions.StruveL0(x).Should().BeApproximately(expected, 1e-7);

    [Theory]
    [InlineData(1.0, 0.71024348)]      // série DLMF (verificada à mão)
    [InlineData(2.0, 1.93743)]
    public void StruveL0_ShouldMatchReference(double x, double expected)
        => SpecialFunctions.StruveL0(x).Should().BeApproximately(expected, 1e-4);

    [Theory]
    [InlineData(0.01, 2.1220659e-5)]   // L1 termo dominante (0.005)²/(Γ1.5·Γ2.5)
    [InlineData(1.0, 0.22676438)]
    [InlineData(2.0, 1.10275)]
    public void StruveL1_ShouldMatchReference(double x, double expected)
        => SpecialFunctions.StruveL1(x).Should().BeApproximately(expected, Abs(expected) * 1e-4 + 1e-9);

    [Fact]
    public void BesselMinusStruve_ShouldBePositive_AndApproachTwoOverPi()
    {
        // I_ν(x) − L_ν(x) > 0 e, assintoticamente, x·(I0−L0) → 2/π e (I1−L1) → 2/π.
        foreach (double x in new[] { 2.0, 4.0, 8.0 })
        {
            double d0 = SpecialFunctions.BesselI0(x) - SpecialFunctions.StruveL0(x);
            double d1 = SpecialFunctions.BesselI1(x) - SpecialFunctions.StruveL1(x);
            d0.Should().BeGreaterThan(0);
            d1.Should().BeGreaterThan(0);
        }
        // x grande o suficiente p/ se aproximar, sem cancelamento severo
        (8.0 * (SpecialFunctions.BesselI0(8.0) - SpecialFunctions.StruveL0(8.0)))
            .Should().BeApproximately(2.0 / System.Math.PI, 0.05);
        (SpecialFunctions.BesselI1(8.0) - SpecialFunctions.StruveL1(8.0))
            .Should().BeApproximately(2.0 / System.Math.PI, 0.05);
    }

    private static double Abs(double v) => System.Math.Abs(v);
}
