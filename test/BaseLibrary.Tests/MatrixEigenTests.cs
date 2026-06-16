using FluentAssertions;
using Sindarin.Math.Matrix;

namespace BaseLibrary.Tests;

public class MatrixEigenTests
{
    [Fact]
    public void SymmetricEigen_ShouldReturnAscendingEigenvalues_For2x2()
    {
        // [[2,1],[1,2]] → autovalores 1 e 3.
        var a = new MatrixArrays(new[] { new[] { 2.0, 1.0 }, new[] { 1.0, 2.0 } });

        MatrixMethods.SymmetricEigen(a, out double[] eig, out MatrixArrays vec).Should().BeTrue();

        eig[0].Should().BeApproximately(1.0, 1e-10);
        eig[1].Should().BeApproximately(3.0, 1e-10);
        // autovetores ortonormais
        (vec[0, 0] * vec[0, 1] + vec[1, 0] * vec[1, 1]).Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void SymmetricEigen_ShouldReconstructMatrix_VLambdaVt()
    {
        var rnd = new Random(7);
        int n = 6;
        // matriz simétrica aleatória
        var a = new MatrixArrays(n, n);
        for (int i = 0; i < n; i++)
            for (int j = i; j < n; j++)
            {
                double v = rnd.NextDouble() * 2 - 1;
                a[i, j] = v; a[j, i] = v;
            }

        MatrixMethods.SymmetricEigen(a, out double[] eig, out MatrixArrays vec);

        // reconstruir A = V·Λ·Vᵀ
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int k = 0; k < n; k++) sum += vec[i, k] * eig[k] * vec[j, k];
                sum.Should().BeApproximately(a[i, j], 1e-9);
            }
    }

    [Fact]
    public void SymmetricEigen_ShouldDetectNearZeroEigenvalue_ForSingularNormal()
    {
        // x3 = x1 + x2 (escalonada): autovalor ~0 (direção singular).
        double s = 1.0 / System.Math.Sqrt(2.0);
        var a = new MatrixArrays(new[]
        {
            new[] { 1.0, 0.0, s },
            new[] { 0.0, 1.0, s },
            new[] { s,   s,   1.0 },
        });

        MatrixMethods.SymmetricEigen(a, out double[] eig, out _);

        eig[0].Should().BeApproximately(0.0, 1e-9);     // menor autovalor ~0
        eig[2].Should().BeApproximately(2.0, 1e-9);     // maior autovalor 2
    }

    [Fact]
    public void TransposeThisAndMultiply_ShouldComputeColumnDotProduct()
    {
        var a = new MatrixArrays(new[] { new[] { 1.0 }, new[] { 2.0 }, new[] { 3.0 } }); // 3×1
        var b = new MatrixArrays(new[] { new[] { 4.0 }, new[] { 5.0 }, new[] { 6.0 } }); // 3×1

        iMatrix r = a.TransposeThisAndMultiply(b);

        r.RowCount.Should().Be(1);
        r.ColumnCount.Should().Be(1);
        r[0, 0].Should().BeApproximately(32.0, 1e-12); // 4+10+18
    }
}
