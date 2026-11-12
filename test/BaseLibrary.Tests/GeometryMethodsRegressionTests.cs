using FluentAssertions;
using BaseLibrary.Math;
using Sindarin.Math.Matrix;
using static BaseLibrary.Tests.RegressionHash;

namespace BaseLibrary.Tests;

/// <summary>
/// Regressao bit a bit de <see cref="GeometryMethods"/> contra a saida da classe de origem no motor (antes da E269) e
/// casos limite com valor analitico, inclusive para Dot e Cross de <see cref="Point3D"/> e Dot de <see cref="Point2D"/>,
/// que vieram do PointExtension e do Point2D. Captura: Sindarin 5a0fd38e (master de 12/11/2026), win-x64, .NET 10.
/// </summary>
public class GeometryMethodsRegressionTests
{
    [Fact]
    public void LatticeVectorFunctions_ShouldMatchTheEngineOutputBitForBit()
    {
        iMatrix gm = new MatrixArrays(RegressionInputs.MonoclinicG());

        // Oraculo: regressao, saida de Sindarin.Math.GeometryMethods em Sindarin 5a0fd38e
        Bits(GeometryMethods.PositiveAngle(-1.25)).Should().Be(0x401421FB54442D18UL);
        Bits(GeometryMethods.PositiveAngle(0.75)).Should().Be(0x3FE8000000000000UL);
        Bits(new[] { 1, 2, -1 }.Length(gm)).Should().Be(0x402EB1281FF56922UL);
        Bits(new[] { 0.5, -1.25, 2.0 }.Length(gm)).Should().Be(0x402F0C60A033A7B3UL);
        Bits(GeometryMethods.CosPhi(new[] { 1, 0, 0 }, new[] { 0, 0, 1 }, gm)).Should().Be(0xBFD0000000000000UL);
        Bits(GeometryMethods.CosPhi(new[] { 0.5, -1.25, 2.0 }, new[] { 1.0, 0.25, -0.5 }, gm)).Should().Be(0xBFE2B6C13D3BEE44UL);
        Bits(GeometryMethods.CalculeScalarProduct(new[] { 1, 2, 3 }, new[] { -1, 0, 2 }, gm)).Should().Be(0x40715C0000000000UL);
    }

    [Fact]
    public void PositiveAngle_ShouldAddATurn_ToANegativeAngle()
    {
        // Oraculo: a mao, -pi/2 + 2 pi = 3 pi / 2; folga de 1e-15, um ulp de 4,7 e 8,9e-16
        GeometryMethods.PositiveAngle(-System.Math.PI / 2).Should().BeApproximately(1.5 * System.Math.PI, 1e-15);
    }

    [Fact]
    public void Length_ShouldBeTheCellEdge_ForTheUnitVectorOfACubicCell()
    {
        iMatrix cubic = new MatrixArrays(new double[,] { { 9.0, 0.0, 0.0 }, { 0.0, 9.0, 0.0 }, { 0.0, 0.0, 9.0 } });

        // Oraculo: caso limite, |[100]| = a = 3 numa cela cubica (G = a² I); sqrt(9) e exato
        new[] { 1, 0, 0 }.Length(cubic).Should().Be(3.0);
    }

    [Fact]
    public void Point3DDotAndCross_ShouldFollowTheCanonicalBasis()
    {
        Point3D e1 = new(1, 0, 0), e2 = new(0, 1, 0), e3 = new(0, 0, 1);

        // Oraculo: caso limite, base ortonormal destra: e1.e2 = 0, e1.e1 = 1, e1 x e2 = e3, e2 x e1 = -e3
        e1.Dot(e2).Should().Be(0.0);
        e1.Dot(e1).Should().Be(1.0);
        e1.Cross(e2).Should().Be(e3);
        e2.Cross(e1).Should().Be(new Point3D(0, 0, -1));
        new Point3D(1, 2, 3).Dot(new Point3D(4, -5, 6)).Should().Be(12.0);
    }

    [Fact]
    public void Point2DDot_ShouldBeZero_ForOrthogonalVectors()
    {
        // Oraculo: a mao, (3, 4).(-4, 3) = -12 + 12 = 0; (3, 4).(3, 4) = 25
        GeometryMethods.Dot(new Point2D(3, 4), new Point2D(-4, 3)).Should().Be(0.0);
        GeometryMethods.Dot(new Point2D(3, 4), new Point2D(3, 4)).Should().Be(25.0);
    }
}
