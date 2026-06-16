using FluentAssertions;
using Sindarin.Math.Matrix;

namespace BaseLibrary.Tests;

public class MatrixSparseTests
{
    private static MatrixArrays DenseSample()
    {
        // [ 0 2 0 ]
        // [ 1 0 0 ]
        // [ 0 0 3 ]
        return new MatrixArrays(new[]
        {
            new[] { 0.0, 2.0, 0.0 },
            new[] { 1.0, 0.0, 0.0 },
            new[] { 0.0, 0.0, 3.0 },
        });
    }

    [Fact]
    public void FromMatrix_ShouldStoreOnlyNonZeros()
    {
        var sparse = MatrixSparse.FromMatrix(DenseSample());

        sparse.NonZeroCount.Should().Be(3);
        sparse.Structure.Should().Be(MatrixStructure.Sparse);
        sparse[0, 1].Should().Be(2.0);
        sparse[1, 0].Should().Be(1.0);
        sparse[2, 2].Should().Be(3.0);
        sparse[0, 0].Should().Be(0.0);
    }

    [Fact]
    public void MultiplyVector_ShouldMatchDense()
    {
        var dense = DenseSample();
        var sparse = MatrixSparse.FromMatrix(dense);
        double[] x = { 1.0, 2.0, 3.0 };

        double[] y = sparse.Multiply(x);

        y[0].Should().BeApproximately(4.0, 1e-12);  // 2*2
        y[1].Should().BeApproximately(1.0, 1e-12);  // 1*1
        y[2].Should().BeApproximately(9.0, 1e-12);  // 3*3
    }

    [Fact]
    public void TransposeMultiply_ShouldMatchDefinition()
    {
        var sparse = MatrixSparse.FromMatrix(DenseSample());
        double[] x = { 1.0, 1.0, 1.0 };

        double[] y = sparse.TransposeMultiply(x);

        // columns: col0 has 1 (row1), col1 has 2 (row0), col2 has 3 (row2)
        y[0].Should().BeApproximately(1.0, 1e-12);
        y[1].Should().BeApproximately(2.0, 1e-12);
        y[2].Should().BeApproximately(3.0, 1e-12);
    }

    [Fact]
    public void MultiplyDispatch_SparseTimesDense_ShouldMatchDenseProduct()
    {
        var dense = DenseSample();
        var sparse = MatrixSparse.FromMatrix(dense);
        var rhs = new MatrixArrays(new[]
        {
            new[] { 1.0, 0.0 },
            new[] { 0.0, 1.0 },
            new[] { 2.0, 3.0 },
        });

        iMatrix viaSparse = ((iMatrix)sparse).Multiply(rhs);
        iMatrix viaDense = ((iMatrix)dense).Multiply(rhs);

        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 2; j++)
                viaSparse[i, j].Should().BeApproximately(viaDense[i, j], 1e-12);
    }

    [Fact]
    public void IntroducingNewNonZero_ShouldThrow()
    {
        var sparse = MatrixSparse.FromMatrix(DenseSample());

        Action act = () => sparse[0, 0] = 5.0;

        act.Should().Throw<NotSupportedException>();
    }
}
