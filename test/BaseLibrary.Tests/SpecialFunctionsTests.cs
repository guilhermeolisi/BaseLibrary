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

    private static double Abs(double v) => System.Math.Abs(v);
}
