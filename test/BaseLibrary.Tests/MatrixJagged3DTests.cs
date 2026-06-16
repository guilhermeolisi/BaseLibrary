using FluentAssertions;
using Sindarin.Math.Matrix;

namespace BaseLibrary.Tests;

public class MatrixJagged3DTests
{
    private static MatrixJagged3D Sample()
    {
        // exp0: file0=[1,2], file1=[3]; exp1: file0=[4,5]  → flat = 1,2,3,4,5
        double[][][] data =
        {
            new[] { new[] { 1.0, 2.0 }, new[] { 3.0 } },
            new[] { new[] { 4.0, 5.0 } },
        };
        return new MatrixJagged3D(data);
    }

    [Fact]
    public void Shape_ShouldBeTotalByOne()
    {
        var m = Sample();

        m.RowCount.Should().Be(5);
        m.ColumnCount.Should().Be(1);
        m.Length.Should().Be(5);
    }

    [Fact]
    public void ToFlat_ShouldEnumerateInExperimentalFilePointOrder()
    {
        var m = Sample();

        m.ToFlat().Should().Equal(1.0, 2.0, 3.0, 4.0, 5.0);
    }

    [Fact]
    public void Indexer_ShouldLocateAcrossExperimentalsAndFiles()
    {
        var m = Sample();

        m[0, 0].Should().Be(1.0);
        m[2, 0].Should().Be(3.0); // exp0, file1, point0
        m[3, 0].Should().Be(4.0); // exp1, file0, point0
        m[4, 0].Should().Be(5.0);
    }

    [Fact]
    public void FillFromFlat_ThenToFlat_ShouldRoundTrip()
    {
        var m = MatrixJagged3D.CreateLike(Sample());

        m.FillFromFlat(new[] { 9.0, 8.0, 7.0, 6.0, 5.0 });

        m.ToFlat().Should().Equal(9.0, 8.0, 7.0, 6.0, 5.0);
        m.GetData()[0][1][0].Should().Be(7.0); // exp0,file1,point0 = flat index 2
    }

    [Fact]
    public void Subtract_ShouldComputeResidualElementwise()
    {
        var y = Sample();
        var yc = MatrixJagged3D.CreateLike(y);
        yc.FillFromFlat(new[] { 1.0, 1.0, 1.0, 1.0, 1.0 });

        var residual = y.Subtract(yc);

        residual.ToFlat().Should().Equal(0.0, 1.0, 2.0, 3.0, 4.0);
    }

    [Fact]
    public void SolveNormalEquations_ShouldRecoverLeastSquaresSlope()
    {
        // Modelo yc = a*x, x = [1,2,3], y = [2,4,6] → a = 2.
        double[][][] xData = { new[] { new[] { 1.0, 2.0, 3.0 } } };
        double[][][] yData = { new[] { new[] { 2.0, 4.0, 6.0 } } };
        double[][][] wData = { new[] { new[] { 1.0, 1.0, 1.0 } } };

        var weights = new MatrixJagged3D(wData);
        var residual = new MatrixJagged3D(yData); // resíduo a partir de a=0 é o próprio y
        double[][] jacobian = { xData[0][0] };     // ∂yc/∂a = x

        double[] delta = weights.SolveNormalEquations(jacobian, residual);

        delta.Should().HaveCount(1);
        delta[0].Should().BeApproximately(2.0, 1e-10);
    }
}
