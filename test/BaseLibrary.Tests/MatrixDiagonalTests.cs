using FluentAssertions;
using Sindarin.Math.Matrix;

namespace BaseLibrary.Tests;

public class MatrixDiagonalTests
{
    [Fact]
    public void Indexer_ShouldReturnDiagonalAndZeroOffDiagonal_WhenAccessed()
    {
        var d = new MatrixDiagonal(new[] { 2.0, 3.0, 5.0 });

        d[0, 0].Should().Be(2.0);
        d[1, 1].Should().Be(3.0);
        d[0, 1].Should().Be(0.0);
        d[2, 0].Should().Be(0.0);
        d.Structure.Should().Be(MatrixStructure.Diagonal);
    }

    [Fact]
    public void SetOffDiagonalNonZero_ShouldThrow_WhenAttempted()
    {
        var d = new MatrixDiagonal(3);

        Action act = () => d[0, 1] = 4.0;

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Inverse_ShouldReturnReciprocalDiagonal_WhenInvertible()
    {
        var d = new MatrixDiagonal(new[] { 2.0, 4.0, 0.5 });

        var inv = d.Inverse();

        inv[0, 0].Should().BeApproximately(0.5, 1e-12);
        inv[1, 1].Should().BeApproximately(0.25, 1e-12);
        inv[2, 2].Should().BeApproximately(2.0, 1e-12);
    }

    [Fact]
    public void Inverse_ShouldThrow_WhenZeroOnDiagonal()
    {
        var d = new MatrixDiagonal(new[] { 2.0, 0.0 });

        Action act = () => d.Inverse();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Determinant_ShouldBeProductOfDiagonal()
    {
        var d = new MatrixDiagonal(new[] { 2.0, 3.0, 5.0 });

        d.Determinant().Should().BeApproximately(30.0, 1e-12);
    }

    [Fact]
    public void MultiplyDispatch_DiagonalTimesGeneral_ShouldScaleRows()
    {
        var d = new MatrixDiagonal(new[] { 2.0, 3.0 });
        var b = new MatrixArrays(new[] { new[] { 1.0, 2.0 }, new[] { 4.0, 5.0 } });

        iMatrix result = ((iMatrix)d).Multiply(b);

        result[0, 0].Should().BeApproximately(2.0, 1e-12);  // 2*1
        result[0, 1].Should().BeApproximately(4.0, 1e-12);  // 2*2
        result[1, 0].Should().BeApproximately(12.0, 1e-12); // 3*4
        result[1, 1].Should().BeApproximately(15.0, 1e-12); // 3*5
    }

    [Fact]
    public void MultiplyDispatch_GeneralTimesDiagonal_ShouldScaleColumns()
    {
        var a = new MatrixArrays(new[] { new[] { 1.0, 2.0 }, new[] { 4.0, 5.0 } });
        var d = new MatrixDiagonal(new[] { 2.0, 3.0 });

        iMatrix result = ((iMatrix)a).Multiply(d);

        result[0, 0].Should().BeApproximately(2.0, 1e-12);  // 1*2
        result[0, 1].Should().BeApproximately(6.0, 1e-12);  // 2*3
        result[1, 0].Should().BeApproximately(8.0, 1e-12);  // 4*2
        result[1, 1].Should().BeApproximately(15.0, 1e-12); // 5*3
    }
}
