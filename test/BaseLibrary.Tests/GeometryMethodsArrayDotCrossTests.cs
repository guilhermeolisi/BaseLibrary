using BaseLibrary.Math;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// E270: <see cref="GeometryMethods.Dot(double[], double[])"/> e <see cref="GeometryMethods.Cross(double[], double[])"/>,
/// que substituem as copias do <c>LatticeGeometry</c> e do <c>ConvexHull3D</c> do motor. Bit a bit contra as copias
/// antigas (Sindarin 9d1b3106) em casos a mao e em 1000 pares sorteados, e os invariantes do produto vetorial.
/// </summary>
public class GeometryMethodsArrayDotCrossTests
{
    // Oraculo: regressao, copia de Sindarin/src/CIF/LatticeGeometry.cs:48 no commit 9d1b3106
    private static double LatticeGeometryDot(double[] a, double[] b) => a[0] * b[0] + a[1] * b[1] + a[2] * b[2];

    // Oraculo: regressao, copia de Sindarin/src/CIF/LatticeGeometry.cs:50 no commit 9d1b3106
    private static double[] LatticeGeometryCross(double[] a, double[] b) =>
        [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];

    // Oraculo: regressao, copia de Sindarin/src/Math/Sindarin.Math/Polyhedra/ConvexHull3D.cs:273 no commit 9d1b3106
    private static double ConvexHullDot(double[] a, double[] b) => a[0] * b[0] + a[1] * b[1] + a[2] * b[2];

    // Oraculo: regressao, copia de Sindarin/src/Math/Sindarin.Math/Polyhedra/ConvexHull3D.cs:275 no commit 9d1b3106
    private static double[] ConvexHullCross(double[] a, double[] b)
        => [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];

    private static long Bits(double x) => BitConverter.DoubleToInt64Bits(x);

    private static List<(double[] A, double[] B)> Pairs()
    {
        var list = new List<(double[], double[])>
        {
            ([1, 0, 0], [0, 1, 0]),
            ([-0.0, 0.0, 1.0], [0.0, -0.0, 1.0]),
            ([double.Epsilon, 1, 2], [3, double.Epsilon, -1]),
            ([1e300, 1e300, 1e300], [1e10, -1e10, 1e10]),
            ([0.1, 0.2, 0.3], [0.3, 0.2, 0.1]),
        };
        var random = new Random(270);
        for (int i = 0; i < 1000; i++)
        {
            double scale = System.Math.Pow(10, random.Next(-150, 150));
            double[] a = [(random.NextDouble() - 0.5) * scale, (random.NextDouble() - 0.5) * scale, (random.NextDouble() - 0.5) * scale];
            double[] b = [(random.NextDouble() - 0.5) * scale, (random.NextDouble() - 0.5) * scale, (random.NextDouble() - 0.5) * scale];
            list.Add((a, b));
        }
        return list;
    }

    [Fact]
    public void Dot_ShouldMatchTheLatticeGeometryAndConvexHullCopiesBitForBit()
    {
        int compared = 0;
        foreach (var (a, b) in Pairs())
        {
            long house = Bits(GeometryMethods.Dot(a, b));
            house.Should().Be(Bits(LatticeGeometryDot(a, b)));
            house.Should().Be(Bits(ConvexHullDot(a, b)));
            compared++;
        }
        compared.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Cross_ShouldMatchTheLatticeGeometryAndConvexHullCopiesBitForBit()
    {
        int compared = 0;
        foreach (var (a, b) in Pairs())
        {
            double[] house = GeometryMethods.Cross(a, b), lattice = LatticeGeometryCross(a, b), hull = ConvexHullCross(a, b);
            for (int k = 0; k < 3; k++)
            {
                Bits(house[k]).Should().Be(Bits(lattice[k]));
                Bits(house[k]).Should().Be(Bits(hull[k]));
            }
            compared++;
        }
        compared.Should().BeGreaterThan(1000);
    }

    [Fact]
    public void Cross_ShouldGiveTheThirdAxis_ForTheFirstTwoAxesOfARightHandedBasis()
    {
        // Oraculo: caso limite, e1 × e2 = e3, e2 × e3 = e1, e3 × e1 = e2 numa base destra (valores exatos)
        GeometryMethods.Cross([1, 0, 0], [0, 1, 0]).Should().Equal(0, 0, 1);
        GeometryMethods.Cross([0, 1, 0], [0, 0, 1]).Should().Equal(1, 0, 0);
        GeometryMethods.Cross([0, 0, 1], [1, 0, 0]).Should().Equal(0, 1, 0);
    }

    [Fact]
    public void Cross_ShouldBePerpendicularToBothFactors()
    {
        // Oraculo: caso limite, (a × b)·a = (a × b)·b = 0; folga 1e-12 relativa a |a||b|², o arredondamento de tres
        // produtos e duas subtracoes em vetores de modulo ~1
        double[] a = [0.3, -1.2, 0.7], b = [2.1, 0.4, -0.9];
        double[] c = GeometryMethods.Cross(a, b);
        GeometryMethods.Dot(c, a).Should().BeApproximately(0, 1e-12);
        GeometryMethods.Dot(c, b).Should().BeApproximately(0, 1e-12);
        GeometryMethods.Dot(a, b).Should().BeApproximately(0.3 * 2.1 - 1.2 * 0.4 - 0.7 * 0.9, 1e-15);
    }
}
