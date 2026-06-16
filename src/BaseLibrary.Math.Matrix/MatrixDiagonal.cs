namespace Sindarin.Math.Matrix;

/// <summary>
/// Matriz diagonal: armazena apenas a diagonal principal (<c>n</c> valores) em vez de <c>n²</c>.
/// Usada como a matriz de pesos <c>W</c> nos mínimos quadrados (Mᵀ·W·M) e em escalonamentos de
/// linha/coluna. As operações têm custo O(n) ou O(n²) em vez de O(n³).
/// </summary>
public sealed class MatrixDiagonal : iMatrix
{
    /// <summary>Valores da diagonal principal. <c>length == n</c>.</summary>
    internal double[] diagonal;

    public MatrixDiagonal(int length)
    {
        if (length <= 0)
            throw new Exception("The length must be greater than zero.");
        diagonal = new double[length];
    }

    /// <summary>Cria a matriz diagonal assumindo a posse do array (sem cópia).</summary>
    public MatrixDiagonal(double[] diagonal)
    {
        if (diagonal is null || diagonal.Length == 0)
            throw new Exception("The diagonal must have at least one element.");
        this.diagonal = diagonal;
    }

    public double this[int row, int column]
    {
        get => row == column ? diagonal[row] : 0.0;
        set
        {
            if (row == column)
                diagonal[row] = value;
            else if (value != 0.0)
                throw new Exception("Cannot set a non-zero off-diagonal element on a diagonal matrix.");
        }
    }

    public int RowCount => diagonal.Length;
    public int ColumnCount => diagonal.Length;
    public MatrixStructure Structure => MatrixStructure.Diagonal;

    public double GetValue(int row, int column) => row == column ? diagonal[row] : 0.0;
    public double GetValueTransposed(int row, int column) => row == column ? diagonal[row] : 0.0;

    public double[] GetDiagonal() => diagonal;

    public iVector GetRow(int index) => new Vector(GetRowArray(index));

    public double[] GetRowArray(int index)
    {
        double[] row = new double[diagonal.Length];
        row[index] = diagonal[index];
        return row;
    }

    public void SetRow(int index, double[] value)
        => throw new NotSupportedException("A diagonal matrix only stores its main diagonal; use the indexer for the diagonal element.");

    /// <summary>Soma dos elementos da diagonal.</summary>
    public double Trace()
    {
        double s = 0.0;
        double[] d = diagonal;
        for (int i = 0; i < d.Length; i++) s += d[i];
        return s;
    }

    /// <summary>Determinante = produto da diagonal.</summary>
    public double Determinant()
    {
        double p = 1.0;
        double[] d = diagonal;
        for (int i = 0; i < d.Length; i++) p *= d[i];
        return p;
    }

    /// <summary>Inversa = recíproco de cada elemento da diagonal. O(n).</summary>
    public MatrixDiagonal Inverse()
    {
        double[] d = diagonal;
        double[] inv = new double[d.Length];
        for (int i = 0; i < d.Length; i++)
        {
            if (d[i] == 0.0)
                throw new Exception("The matrix is not invertible (zero on the diagonal).");
            inv[i] = 1.0 / d[i];
        }
        return new MatrixDiagonal(inv);
    }

    /// <summary>Produto de duas diagonais (elemento a elemento). O(n).</summary>
    public MatrixDiagonal Multiply(MatrixDiagonal other)
    {
        double[] a = diagonal, b = other.diagonal;
        if (a.Length != b.Length)
            throw new Exception("The matrices cannot be multiplied.");
        double[] r = new double[a.Length];
        for (int i = 0; i < a.Length; i++) r[i] = a[i] * b[i];
        return new MatrixDiagonal(r);
    }

    /// <summary>
    /// Extrai a diagonal de uma matriz qualquer como <see cref="MatrixDiagonal"/>.
    /// </summary>
    public static MatrixDiagonal FromMatrix(iMatrix matrix)
    {
        if (matrix.RowCount != matrix.ColumnCount)
            throw new Exception("The matrix must be square.");
        int n = matrix.RowCount;
        double[] d = new double[n];
        for (int i = 0; i < n; i++) d[i] = matrix[i, i];
        return new MatrixDiagonal(d);
    }

    /// <summary>Converte para <see cref="MatrixArrays"/> densa (uso pontual / interop).</summary>
    public MatrixArrays ToArrays()
    {
        int n = diagonal.Length;
        MatrixArrays m = new MatrixArrays(n, n);
        for (int i = 0; i < n; i++) m[i, i] = diagonal[i];
        return m;
    }
}
