namespace Sindarin.Math.Matrix;

/// <summary>
/// Matriz esparsa no formato CSR (Compressed Sparse Row): armazena apenas os elementos não-nulos.
/// Para matrizes com muitos zeros (ex.: matrizes de vínculo/constraint, blocos por fase) o custo de
/// multiplicação cai de O(linhas·colunas·k) para O(nnz·k), e o armazenamento de O(n²) para O(nnz).
/// É construída uma vez (read-mostly): o padrão de esparsidade é fixo após a construção.
/// </summary>
public sealed class MatrixSparse : iMatrix
{
    private readonly double[] values;   // valores não-nulos, agrupados por linha
    private readonly int[] colIndex;    // coluna de cada valor
    private readonly int[] rowPtr;      // rowPtr[i]..rowPtr[i+1) = intervalo da linha i; length = rows+1
    private readonly int rows;
    private readonly int cols;

    private MatrixSparse(double[] values, int[] colIndex, int[] rowPtr, int rows, int cols)
    {
        this.values = values;
        this.colIndex = colIndex;
        this.rowPtr = rowPtr;
        this.rows = rows;
        this.cols = cols;
    }

    public int RowCount => rows;
    public int ColumnCount => cols;
    public MatrixStructure Structure => MatrixStructure.Sparse;

    /// <summary>Número de elementos não-nulos armazenados.</summary>
    public int NonZeroCount => values.Length;
    /// <summary>Fração de elementos não-nulos (0..1).</summary>
    public double Density => (double)values.Length / ((long)rows * cols);

    public double this[int row, int column]
    {
        get => GetValue(row, column);
        set
        {
            int idx = FindIndex(row, column);
            if (idx >= 0)
                values[idx] = value;
            else if (value != 0.0)
                throw new NotSupportedException("Cannot introduce a new non-zero into a CSR sparse matrix; rebuild it from the dense data instead.");
        }
    }

    public double GetValue(int row, int column)
    {
        int idx = FindIndex(row, column);
        return idx >= 0 ? values[idx] : 0.0;
    }

    public double GetValueTransposed(int row, int column) => GetValue(column, row);

    private int FindIndex(int row, int column)
    {
        // Busca binária dentro da linha (colIndex está ordenado por coluna em cada linha).
        int lo = rowPtr[row];
        int hi = rowPtr[row + 1] - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            int c = colIndex[mid];
            if (c == column) return mid;
            if (c < column) lo = mid + 1;
            else hi = mid - 1;
        }
        return -1;
    }

    public iVector GetRow(int index) => new Vector(GetRowArray(index));

    public double[] GetRowArray(int index)
    {
        double[] row = new double[cols];
        for (int p = rowPtr[index]; p < rowPtr[index + 1]; p++)
            row[colIndex[p]] = values[p];
        return row;
    }

    public void SetRow(int index, double[] value)
        => throw new NotSupportedException("CSR sparse matrices are immutable in structure; rebuild from dense data.");

    /// <summary>
    /// Multiplica esta esparsa por uma matriz densa qualquer: <c>this · dense</c>. O(nnz · dense.ColumnCount).
    /// </summary>
    public MatrixArrays Multiply(iMatrix dense)
    {
        if (cols != dense.RowCount)
            throw new Exception("The matrices cannot be multiplied.");
        int outCols = dense.ColumnCount;
        MatrixArrays result = new MatrixArrays(rows, outCols);
        double[][] r = result.data;

        // Caminho rápido quando o RHS é MatrixArrays (linhas contíguas).
        double[][]? rhs = (dense as MatrixArrays)?.data;

        Parallel.For(0, rows, i =>
        {
            double[] ri = r[i];
            int end = rowPtr[i + 1];
            for (int p = rowPtr[i]; p < end; p++)
            {
                double v = values[p];
                int k = colIndex[p];
                if (rhs is not null)
                {
                    double[] rowK = rhs[k];
                    for (int j = 0; j < outCols; j++)
                        ri[j] += v * rowK[j];
                }
                else
                {
                    for (int j = 0; j < outCols; j++)
                        ri[j] += v * dense[k, j];
                }
            }
        });
        return result;
    }

    /// <summary>Produto matriz-vetor <c>this · x</c>. O(nnz).</summary>
    public double[] Multiply(double[] x)
    {
        if (x.Length != cols)
            throw new Exception("The vector has the wrong length.");
        double[] y = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double sum = 0.0;
            int end = rowPtr[i + 1];
            for (int p = rowPtr[i]; p < end; p++)
                sum += values[p] * x[colIndex[p]];
            y[i] = sum;
        }
        return y;
    }

    /// <summary>Produto transposta-vetor <c>thisᵀ · x</c>. O(nnz).</summary>
    public double[] TransposeMultiply(double[] x)
    {
        if (x.Length != rows)
            throw new Exception("The vector has the wrong length.");
        double[] y = new double[cols];
        for (int i = 0; i < rows; i++)
        {
            double xi = x[i];
            int end = rowPtr[i + 1];
            for (int p = rowPtr[i]; p < end; p++)
                y[colIndex[p]] += values[p] * xi;
        }
        return y;
    }

    /// <summary>
    /// Constrói uma matriz esparsa a partir de uma matriz densa qualquer, descartando |valor| ≤ tolerance.
    /// </summary>
    public static MatrixSparse FromMatrix(iMatrix matrix, double tolerance = 0.0)
    {
        int rows = matrix.RowCount;
        int cols = matrix.ColumnCount;
        int[] rowPtr = new int[rows + 1];

        // 1ª passada: conta não-nulos por linha.
        int nnz = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                double v = matrix[i, j];
                if (v > tolerance || v < -tolerance) nnz++;
            }
            rowPtr[i + 1] = nnz;
        }

        double[] values = new double[nnz];
        int[] colIndex = new int[nnz];
        int p = 0;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                double v = matrix[i, j];
                if (v > tolerance || v < -tolerance)
                {
                    values[p] = v;
                    colIndex[p] = j;
                    p++;
                }
            }
        return new MatrixSparse(values, colIndex, rowPtr, rows, cols);
    }

    /// <summary>
    /// Constrói diretamente do formato CSR (sem cópia). <paramref name="colIndex"/> deve estar ordenado por linha.
    /// </summary>
    public static MatrixSparse FromCsr(double[] values, int[] colIndex, int[] rowPtr, int cols)
    {
        if (rowPtr is null || rowPtr.Length < 2)
            throw new Exception("rowPtr must have at least 2 entries.");
        if (values.Length != colIndex.Length || values.Length != rowPtr[^1])
            throw new Exception("CSR arrays are inconsistent.");
        return new MatrixSparse(values, colIndex, rowPtr, rowPtr.Length - 1, cols);
    }

    /// <summary>Converte para <see cref="MatrixArrays"/> densa (uso pontual / interop).</summary>
    public MatrixArrays ToArrays()
    {
        MatrixArrays m = new MatrixArrays(rows, cols);
        double[][] d = m.data;
        for (int i = 0; i < rows; i++)
            for (int pp = rowPtr[i]; pp < rowPtr[i + 1]; pp++)
                d[i][colIndex[pp]] = values[pp];
        return m;
    }
}
