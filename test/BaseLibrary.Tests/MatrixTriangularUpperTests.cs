using FluentAssertions;
using Sindarin.Math.Matrix;

namespace BaseLibrary.Tests;

public class MatrixTriangularUpperTests
{
    private static MatrixTriangularUpper Build3x3()
    {
        // [ 2 1 1 ]
        // [ 0 3 2 ]
        // [ 0 0 4 ]
        var u = new MatrixTriangularUpper(3);
        u[0, 0] = 2; u[0, 1] = 1; u[0, 2] = 1;
        u[1, 1] = 3; u[1, 2] = 2;
        u[2, 2] = 4;
        return u;
    }

    [Fact]
    public void Indexer_ShouldBeZeroBelowDiagonal_AndStoreUpper()
    {
        var u = Build3x3();

        u[2, 0].Should().Be(0.0);
        u[1, 0].Should().Be(0.0);
        u[0, 2].Should().Be(1.0);
        u.Structure.Should().Be(MatrixStructure.UpperTriangular);
    }

    [Fact]
    public void SetBelowDiagonalNonZero_ShouldThrow()
    {
        var u = new MatrixTriangularUpper(3);

        Action act = () => u[2, 0] = 1.0;

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Determinant_ShouldBeProductOfDiagonal()
    {
        Build3x3().Determinant().Should().BeApproximately(24.0, 1e-12);
    }

    [Fact]
    public void Inverse_TimesOriginal_ShouldBeIdentity()
    {
        var u = Build3x3();
        var inv = u.Inverse();

        iMatrix product = ((iMatrix)u).Multiply(inv);

        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                product[i, j].Should().BeApproximately(i == j ? 1.0 : 0.0, 1e-10);
    }

    [Fact]
    public void Multiply_TwoUpper_ShouldStayUpper_AndMatchDense()
    {
        var u1 = Build3x3();
        var u2 = Build3x3();

        var product = u1.Multiply(u2);
        iMatrix dense = ((iMatrix)u1.ToArrays()).Multiply(u2.ToArrays());

        product[2, 0].Should().Be(0.0);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                product[i, j].Should().BeApproximately(dense[i, j], 1e-10);
    }

    [Fact]
    public void SolveUpper_ShouldRecoverKnownSolution()
    {
        var u = Build3x3();
        double[] x = { 1.0, -2.0, 3.0 };
        // b = U * x
        double[] b =
        {
            2 * 1 + 1 * -2 + 1 * 3,
            3 * -2 + 2 * 3,
            4 * 3,
        };

        double[] solved = u.SolveUpper(b);

        solved[0].Should().BeApproximately(1.0, 1e-10);
        solved[1].Should().BeApproximately(-2.0, 1e-10);
        solved[2].Should().BeApproximately(3.0, 1e-10);
    }
}
