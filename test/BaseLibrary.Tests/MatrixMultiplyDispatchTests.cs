using FluentAssertions;
using Sindarin.Math.Matrix;

namespace BaseLibrary.Tests;

/// <summary>
/// Verifica que os caminhos de multiplicação otimizados (cache-friendly e despacho por estrutura)
/// concordam com a multiplicação ingênua de referência, e cobre a correção de <c>DeterminantLU</c>.
/// </summary>
public class MatrixMultiplyDispatchTests
{
    private static MatrixArrays RandomArrays(Random rnd, int rows, int cols)
    {
        var m = new MatrixArrays(rows, cols);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                m[i, j] = rnd.NextDouble() * 20 - 10;
        return m;
    }

    private static MatrixDense ToDense(iMatrix src)
    {
        var d = new MatrixDense(src.RowCount, src.ColumnCount);
        for (int i = 0; i < src.RowCount; i++)
            for (int j = 0; j < src.ColumnCount; j++)
                d[i, j] = src[i, j];
        return d;
    }

    private static double[,] NaiveMultiply(iMatrix a, iMatrix b)
    {
        var r = new double[a.RowCount, b.ColumnCount];
        for (int i = 0; i < a.RowCount; i++)
            for (int j = 0; j < b.ColumnCount; j++)
            {
                double s = 0;
                for (int k = 0; k < a.ColumnCount; k++) s += a[i, k] * b[k, j];
                r[i, j] = s;
            }
        return r;
    }

    private static void AssertEquals(iMatrix actual, double[,] expected)
    {
        for (int i = 0; i < actual.RowCount; i++)
            for (int j = 0; j < actual.ColumnCount; j++)
                actual[i, j].Should().BeApproximately(expected[i, j], 1e-8);
    }

    [Fact]
    public void Arrays_TimesArrays_ShouldMatchNaive_Rectangular()
    {
        var rnd = new Random(1);
        var a = RandomArrays(rnd, 7, 5);
        var b = RandomArrays(rnd, 5, 9);

        AssertEquals(a.Multiply(b), NaiveMultiply(a, b));
    }

    [Fact]
    public void Dense_TimesDense_ShouldMatchNaive_Rectangular()
    {
        var rnd = new Random(2);
        var a = RandomArrays(rnd, 7, 5);
        var b = RandomArrays(rnd, 5, 9);
        var da = ToDense(a);
        var db = ToDense(b);

        AssertEquals(da.Multiply(db), NaiveMultiply(a, b));
    }

    [Fact]
    public void Arrays_TimesDense_ShouldMatchNaive()
    {
        var rnd = new Random(3);
        var a = RandomArrays(rnd, 6, 4);
        var b = RandomArrays(rnd, 4, 6);

        AssertEquals(a.Multiply(ToDense(b)), NaiveMultiply(a, b));
    }

    [Fact]
    public void Dense_TimesArrays_ShouldMatchNaive()
    {
        var rnd = new Random(4);
        var a = RandomArrays(rnd, 6, 4);
        var b = RandomArrays(rnd, 4, 6);

        AssertEquals(ToDense(a).Multiply(b), NaiveMultiply(a, b));
    }

    [Fact]
    public void GenericDispatch_ShouldMatchNaive()
    {
        var rnd = new Random(5);
        iMatrix a = RandomArrays(rnd, 8, 6);
        iMatrix b = RandomArrays(rnd, 6, 8);

        AssertEquals(a.Multiply(b), NaiveMultiply(a, b));
    }

    [Fact]
    public void IdentityDispatch_ShouldReturnOtherOperand()
    {
        var rnd = new Random(6);
        var a = RandomArrays(rnd, 5, 5);
        iMatrix id = new Identity(5);

        AssertEquals(id.Multiply(a), NaiveMultiply(id, a));
        AssertEquals(((iMatrix)a).Multiply(id), NaiveMultiply(a, id));
    }

    [Fact]
    public void DeterminantLU_ShouldMatchCofactorDeterminant()
    {
        // det = 3 (conferido à mão). Antes da correção, DeterminantLU multiplicava a diagonal da
        // matriz ORIGINAL (2*3*1 = 6) em vez da diagonal de U.
        var m = new MatrixArrays(new[]
        {
            new[] { 2.0, 0.0, 1.0 },
            new[] { 1.0, 3.0, 2.0 },
            new[] { 0.0, 1.0, 1.0 },
        });

        double detLu = ((iMatrix)m.Copy()).DeterminantLU();
        double detCofactor = ((iMatrix)m).Determinant();

        detCofactor.Should().BeApproximately(3.0, 1e-10);
        detLu.Should().BeApproximately(3.0, 1e-10);
    }
}
