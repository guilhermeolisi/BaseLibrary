using FluentAssertions;
using Sindarin.Math.Matrix;

namespace BaseLibrary.Tests;

/// <summary>Cobre Mᵀ·W·M (equações normais) em <see cref="MatrixMethods"/>.</summary>
public class MatrixNormalEquationsTests
{
    private static readonly double[][] Jacobian =
    {
        new[] { 1.0, 2.0, 3.0 }, // parâmetro 0 sobre 3 pontos
        new[] { 4.0, 5.0, 6.0 }, // parâmetro 1
    };

    [Fact]
    public void NormalEquations_Unweighted_ShouldMatchHandComputation()
    {
        var a = MatrixMethods.NormalEquations(Jacobian);

        a.RowCount.Should().Be(2);
        a[0, 0].Should().BeApproximately(14.0, 1e-10); // 1+4+9
        a[0, 1].Should().BeApproximately(32.0, 1e-10); // 4+10+18
        a[1, 0].Should().BeApproximately(32.0, 1e-10); // simétrica
        a[1, 1].Should().BeApproximately(77.0, 1e-10); // 16+25+36
    }

    [Fact]
    public void NormalEquations_Weighted_ShouldApplyDiagonalWeights()
    {
        double[] w = { 2.0, 1.0, 0.5 };

        var a = MatrixMethods.NormalEquations(Jacobian, w);

        a[0, 0].Should().BeApproximately(10.5, 1e-10);
        a[0, 1].Should().BeApproximately(27.0, 1e-10);
        a[1, 1].Should().BeApproximately(75.0, 1e-10);
    }

    [Fact]
    public void NormalEquations_GeneralMatrixForm_ShouldEqualJacobianForm()
    {
        // M = Jᵀ (data × params), W diagonal. Mᵀ·W·M deve igualar a forma por-parâmetro.
        var m = new MatrixArrays(new[]
        {
            new[] { 1.0, 4.0 },
            new[] { 2.0, 5.0 },
            new[] { 3.0, 6.0 },
        });
        var w = new MatrixDiagonal(new[] { 2.0, 1.0, 0.5 });

        var viaGeneral = MatrixMethods.NormalEquations(m, w);
        var viaJacobian = MatrixMethods.NormalEquations(Jacobian, new[] { 2.0, 1.0, 0.5 });

        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                viaGeneral[i, j].Should().BeApproximately(viaJacobian[i, j], 1e-10);
    }

    [Fact]
    public void TransposeWeightedResidual_ShouldComputeMtWr()
    {
        double[] w = { 1.0, 1.0, 1.0 };
        double[] r = { 1.0, 1.0, 1.0 };

        double[] b = MatrixMethods.TransposeWeightedResidual(Jacobian, w, r);

        b[0].Should().BeApproximately(6.0, 1e-10);  // 1+2+3
        b[1].Should().BeApproximately(15.0, 1e-10); // 4+5+6
    }

    [Fact]
    public void NormalEquations_ShouldEqualNaiveDenseMtWM_ForRandomData()
    {
        var rnd = new Random(42);
        int p = 5, m = 200;
        double[][] j = new double[p][];
        for (int k = 0; k < p; k++)
        {
            j[k] = new double[m];
            for (int n = 0; n < m; n++) j[k][n] = rnd.NextDouble() * 2 - 1;
        }
        double[] w = new double[m];
        for (int n = 0; n < m; n++) w[n] = rnd.NextDouble() + 0.1;

        var a = MatrixMethods.NormalEquations(j, w);

        for (int k = 0; k < p; k++)
            for (int jj = 0; jj < p; jj++)
            {
                double expected = 0.0;
                for (int n = 0; n < m; n++) expected += w[n] * j[k][n] * j[jj][n];
                a[k, jj].Should().BeApproximately(expected, 1e-8);
            }
    }
}
