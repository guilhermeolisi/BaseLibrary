namespace Sindarin.Math.Matrix;

/// <summary>
/// Kernels otimizados que exploram a estrutura das matrizes (diagonal, esparsa, …) e a operação
/// de mínimos quadrados Mᵀ·W·M. Complementa o despacho de <see cref="MatrixMethods.Multiply(iMatrix, iMatrix)"/>.
/// </summary>
public static partial class MatrixMethods
{
    /// <summary>
    /// Alias de <see cref="InverseLU(iMatrix)"/> para compatibilidade com o <c>Matrix.Inverse()</c> do MathNet.
    /// Lança em matriz singular (as matrizes de geometria/métrica são sempre invertíveis); para o caminho do
    /// otimizador use <see cref="TryInverseLU"/>.
    /// </summary>
    public static iMatrix Inverse(this iMatrix matrix) => matrix.InverseLU();

    /// <summary>Cópia densa (em <see cref="MatrixArrays"/>) de qualquer matriz, via indexador.</summary>
    internal static MatrixArrays DensifyCopy(iMatrix matrix)
    {
        int rows = matrix.RowCount, cols = matrix.ColumnCount;
        MatrixArrays result = new MatrixArrays(rows, cols);
        double[][] r = result.data;
        for (int i = 0; i < rows; i++)
        {
            double[] ri = r[i];
            for (int j = 0; j < cols; j++)
                ri[j] = matrix[i, j];
        }
        return result;
    }

    /// <summary>D·B com D diagonal: escala a linha i de B por d[i]. O(linhas·colunas).</summary>
    internal static MatrixArrays MultiplyDiagonalLeft(MatrixDiagonal d, iMatrix b)
    {
        double[] diag = d.diagonal;
        int rows = b.RowCount, cols = b.ColumnCount;
        MatrixArrays result = new MatrixArrays(rows, cols);
        double[][] r = result.data;
        double[][]? bRows = (b as MatrixArrays)?.data;
        Parallel.For(0, rows, i =>
        {
            double di = diag[i];
            double[] ri = r[i];
            if (bRows is not null)
            {
                double[] bi = bRows[i];
                for (int j = 0; j < cols; j++) ri[j] = di * bi[j];
            }
            else
            {
                for (int j = 0; j < cols; j++) ri[j] = di * b[i, j];
            }
        });
        return result;
    }

    /// <summary>A·D com D diagonal: escala a coluna j de A por d[j]. O(linhas·colunas).</summary>
    internal static MatrixArrays MultiplyDiagonalRight(iMatrix a, MatrixDiagonal d)
    {
        double[] diag = d.diagonal;
        int rows = a.RowCount, cols = a.ColumnCount;
        MatrixArrays result = new MatrixArrays(rows, cols);
        double[][] r = result.data;
        double[][]? aRows = (a as MatrixArrays)?.data;
        Parallel.For(0, rows, i =>
        {
            double[] ri = r[i];
            if (aRows is not null)
            {
                double[] ai = aRows[i];
                for (int j = 0; j < cols; j++) ri[j] = ai[j] * diag[j];
            }
            else
            {
                for (int j = 0; j < cols; j++) ri[j] = a[i, j] * diag[j];
            }
        });
        return result;
    }

    // ===================================================================================
    //  Mᵀ · W · M  (equações normais dos mínimos quadrados ponderados)
    // ===================================================================================

    /// <summary>
    /// Equações normais a partir do Jacobiano em layout "por parâmetro": <paramref name="jacobianByParam"/>[k]
    /// é o vetor de derivadas do parâmetro k sobre todos os <c>m</c> pontos. Calcula a matriz simétrica
    /// <c>A[k,j] = Σ_n w[n]·J[k][n]·J[j][n]</c> (= J·diag(w)·Jᵀ, equivalente a Mᵀ·W·M com M = Jᵀ).
    /// Este é exatamente o layout do Jacobiano usado no NLS do Sindarin — sem transposição/cópia.
    /// </summary>
    public static MatrixArrays NormalEquations(double[][] jacobianByParam, double[]? weights = null)
    {
        int p = jacobianByParam.Length;
        if (p == 0) throw new Exception("The Jacobian must have at least one parameter row.");
        int m = jacobianByParam[0].Length;
        if (weights is not null && weights.Length != m)
            throw new Exception("The weights length must match the number of data points.");

        MatrixArrays result = new MatrixArrays(p, p);
        double[][] a = result.data;

        Parallel.For(0, p, k =>
        {
            double[] jk = jacobianByParam[k];
            double[] ak = a[k];
            for (int j = k; j < p; j++)
            {
                double[] jj = jacobianByParam[j];
                double sum = 0.0;
                if (weights is null)
                {
                    for (int n = 0; n < m; n++) sum += jk[n] * jj[n];
                }
                else
                {
                    for (int n = 0; n < m; n++) sum += weights[n] * jk[n] * jj[n];
                }
                ak[j] = sum;
            }
        });

        // Completa o triângulo inferior (simetria).
        for (int k = 0; k < p; k++)
            for (int j = k + 1; j < p; j++)
                a[j][k] = a[k][j];

        return result;
    }

    /// <summary>
    /// Lado direito das equações normais: <c>b[k] = Σ_n J[k][n]·w[n]·residual[n]</c> (= Mᵀ·W·r).
    /// <paramref name="residual"/> é tipicamente (y − yc).
    /// </summary>
    public static double[] TransposeWeightedResidual(double[][] jacobianByParam, double[]? weights, double[] residual)
    {
        int p = jacobianByParam.Length;
        int m = residual.Length;
        double[] b = new double[p];
        Parallel.For(0, p, k =>
        {
            double[] jk = jacobianByParam[k];
            double sum = 0.0;
            if (weights is null)
                for (int n = 0; n < m; n++) sum += jk[n] * residual[n];
            else
                for (int n = 0; n < m; n++) sum += jk[n] * weights[n] * residual[n];
            b[k] = sum;
        });
        return b;
    }

    /// <summary>
    /// Equações normais para uma matriz de projeto <paramref name="m"/> genérica (linhas = pontos,
    /// colunas = parâmetros) com pesos diagonais <paramref name="w"/> (ou nulos = identidade).
    /// Calcula <c>A = Mᵀ·W·M</c> (p×p simétrica) por acumulação rank-1 sobre as linhas (cache-friendly),
    /// reduzindo por thread.
    /// </summary>
    public static MatrixArrays NormalEquations(iMatrix m, MatrixDiagonal? w = null)
    {
        int nData = m.RowCount;
        int p = m.ColumnCount;
        if (w is not null && w.RowCount != nData)
            throw new Exception("The weight matrix size must match the number of rows of M.");

        double[]? weights = w?.diagonal;
        double[][]? mRows = (m as MatrixArrays)?.data;

        MatrixArrays result = new MatrixArrays(p, p);
        double[][] a = result.data;
        object gate = new object();

        Parallel.For(0, nData,
            () => new double[p * p],            // acumulador local por thread (triângulo superior preenchido)
            (n, _, local) =>
            {
                double scale = weights is null ? 1.0 : weights[n];
                if (scale == 0.0) return local;
                double[] row = mRows is not null ? mRows[n] : m.GetRowArray(n);
                for (int k = 0; k < p; k++)
                {
                    double s = scale * row[k];
                    if (s == 0.0) continue;
                    int basek = k * p;
                    for (int j = k; j < p; j++)
                        local[basek + j] += s * row[j];
                }
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    for (int k = 0; k < p; k++)
                    {
                        double[] ak = a[k];
                        int basek = k * p;
                        for (int j = k; j < p; j++)
                            ak[j] += local[basek + j];
                    }
                }
            });

        // Completa o triângulo inferior (simetria).
        for (int k = 0; k < p; k++)
            for (int j = k + 1; j < p; j++)
                a[j][k] = a[k][j];

        return result;
    }
}
