namespace Sindarin.Math.Matrix;

/// <summary>
/// Matriz triangular superior (zeros estritamente abaixo da diagonal principal), armazenada de forma
/// compacta: apenas os <c>n·(n+1)/2</c> elementos do triângulo superior, em ordem por linha.
/// Multiplicação, inversão (back-substitution) e solução de sistema custam ~metade de uma matriz densa
/// e ignoram completamente a metade nula. Aparece naturalmente como o fator U de uma decomposição LU/QR.
/// </summary>
public sealed class MatrixTriangularUpper : iMatrix
{
    /// <summary>Triângulo superior empacotado por linha: linha i ocupa [rowStart[i] .. rowStart[i]+(n-i)).</summary>
    internal readonly double[] data;
    private readonly int[] rowStart;
    private readonly int n;

    public MatrixTriangularUpper(int order)
    {
        if (order <= 0)
            throw new Exception("The order must be greater than zero.");
        n = order;
        rowStart = BuildRowStart(order);
        data = new double[order * (order + 1) / 2];
    }

    private MatrixTriangularUpper(int order, double[] packed)
    {
        n = order;
        rowStart = BuildRowStart(order);
        data = packed;
    }

    private static int[] BuildRowStart(int order)
    {
        int[] starts = new int[order];
        int acc = 0;
        for (int i = 0; i < order; i++)
        {
            starts[i] = acc;
            acc += order - i; // linha i tem (n - i) elementos: colunas i..n-1
        }
        return starts;
    }

    public double this[int row, int column]
    {
        get => column >= row ? data[rowStart[row] + (column - row)] : 0.0;
        set
        {
            if (column >= row)
                data[rowStart[row] + (column - row)] = value;
            else if (value != 0.0)
                throw new Exception("Cannot set a non-zero element below the main diagonal of an upper-triangular matrix.");
        }
    }

    public int RowCount => n;
    public int ColumnCount => n;
    public MatrixStructure Structure => MatrixStructure.UpperTriangular;

    public double GetValue(int row, int column) => column >= row ? data[rowStart[row] + (column - row)] : 0.0;
    public double GetValueTransposed(int row, int column) => row >= column ? data[rowStart[column] + (row - column)] : 0.0;

    public iVector GetRow(int index) => new Vector(GetRowArray(index));

    public double[] GetRowArray(int index)
    {
        double[] row = new double[n];
        int start = rowStart[index];
        for (int c = index; c < n; c++)
            row[c] = data[start + (c - index)];
        return row;
    }

    public void SetRow(int index, double[] value)
    {
        if (value.Length != n)
            throw new Exception("The length of the row must be equal to the number of columns.");
        for (int c = 0; c < index; c++)
            if (value[c] != 0.0)
                throw new Exception("Cannot set a non-zero element below the main diagonal of an upper-triangular matrix.");
        int start = rowStart[index];
        for (int c = index; c < n; c++)
            data[start + (c - index)] = value[c];
    }

    /// <summary>Determinante = produto da diagonal.</summary>
    public double Determinant()
    {
        double p = 1.0;
        for (int i = 0; i < n; i++) p *= data[rowStart[i]]; // (i,i) = rowStart[i] + 0
        return p;
    }

    /// <summary>
    /// Produto de duas triangulares superiores → triangular superior. O(n³/6), ignora a metade nula.
    /// </summary>
    public MatrixTriangularUpper Multiply(MatrixTriangularUpper other)
    {
        if (n != other.n)
            throw new Exception("The matrices cannot be multiplied.");
        MatrixTriangularUpper result = new MatrixTriangularUpper(n);
        double[] a = data, b = other.data, r = result.data;
        int[] rs = rowStart;
        for (int i = 0; i < n; i++)
        {
            int rsi = rs[i];
            for (int j = i; j < n; j++)
            {
                double sum = 0.0;
                // k vai de i até j: A[i,k] (k>=i) e B[k,j] (j>=k) ambos no triângulo superior.
                for (int k = i; k <= j; k++)
                    sum += a[rsi + (k - i)] * b[rs[k] + (j - k)];
                r[rsi + (j - i)] = sum;
            }
        }
        return result;
    }

    /// <summary>
    /// Resolve U·x = b por substituição reversa. O(n²).
    /// </summary>
    public double[] SolveUpper(double[] b)
    {
        if (b.Length != n)
            throw new Exception("The right-hand side has the wrong length.");
        double[] x = new double[n];
        double[] a = data;
        int[] rs = rowStart;
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = b[i];
            int rsi = rs[i];
            for (int j = i + 1; j < n; j++)
                sum -= a[rsi + (j - i)] * x[j];
            double diag = a[rsi];
            if (diag == 0.0)
                throw new Exception("The matrix is not invertible (zero on the diagonal).");
            x[i] = sum / diag;
        }
        return x;
    }

    /// <summary>
    /// Inversa de uma triangular superior → triangular superior, por substituição reversa. O(n³/6).
    /// </summary>
    public MatrixTriangularUpper Inverse()
    {
        MatrixTriangularUpper inv = new MatrixTriangularUpper(n);
        double[] a = data, r = inv.data;
        int[] rs = rowStart;

        for (int i = 0; i < n; i++)
        {
            double diag = a[rs[i]];
            if (diag == 0.0)
                throw new Exception("The matrix is not invertible (zero on the diagonal).");
        }

        // Resolve coluna a coluna: U · inv[:,j] = e_j, mas só as linhas 0..j são não-nulas.
        for (int j = 0; j < n; j++)
        {
            r[rs[j] + 0] = 1.0 / a[rs[j]]; // inv[j,j]
            for (int i = j - 1; i >= 0; i--)
            {
                double sum = 0.0;
                int rsi = rs[i];
                for (int k = i + 1; k <= j; k++)
                    sum += a[rsi + (k - i)] * r[rs[k] + (j - k)];
                r[rsi + (j - i)] = -sum / a[rsi];
            }
        }
        return inv;
    }

    /// <summary>Extrai o triângulo superior de uma matriz quadrada qualquer.</summary>
    public static MatrixTriangularUpper FromMatrix(iMatrix matrix)
    {
        if (matrix.RowCount != matrix.ColumnCount)
            throw new Exception("The matrix must be square.");
        int order = matrix.RowCount;
        MatrixTriangularUpper u = new MatrixTriangularUpper(order);
        double[] d = u.data;
        int[] rs = u.rowStart;
        for (int i = 0; i < order; i++)
            for (int j = i; j < order; j++)
                d[rs[i] + (j - i)] = matrix[i, j];
        return u;
    }

    /// <summary>Empacota um array já no formato compacto (sem cópia).</summary>
    public static MatrixTriangularUpper FromPacked(int order, double[] packed)
    {
        if (packed is null || packed.Length != order * (order + 1) / 2)
            throw new Exception("The packed array has the wrong length.");
        return new MatrixTriangularUpper(order, packed);
    }

    /// <summary>Converte para <see cref="MatrixArrays"/> densa (uso pontual / interop).</summary>
    public MatrixArrays ToArrays()
    {
        MatrixArrays m = new MatrixArrays(n, n);
        for (int i = 0; i < n; i++)
        {
            int start = rowStart[i];
            for (int j = i; j < n; j++)
                m[i, j] = data[start + (j - i)];
        }
        return m;
    }
}
