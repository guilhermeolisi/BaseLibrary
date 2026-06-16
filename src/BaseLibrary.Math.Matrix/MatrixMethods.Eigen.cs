using static System.Math;

namespace Sindarin.Math.Matrix;

public static partial class MatrixMethods
{
    /// <summary>
    /// Produto <c>thisᵀ · other</c>: resultado (this.ColumnCount × other.ColumnCount), somando sobre as linhas.
    /// Para vetores-coluna (n×1) devolve a matriz 1×1 com o produto interno. Substitui o
    /// <c>Matrix.TransposeThisAndMultiply</c> do MathNet.
    /// </summary>
    public static iMatrix TransposeThisAndMultiply(this iMatrix a, iMatrix b)
    {
        if (a.RowCount != b.RowCount)
            throw new Exception("The matrices cannot be multiplied.");
        int p = a.ColumnCount, q = b.ColumnCount, inner = a.RowCount;
        MatrixArrays result = new MatrixArrays(p, q);
        double[][] r = result.data;
        for (int i = 0; i < p; i++)
        {
            double[] ri = r[i];
            for (int k = 0; k < inner; k++)
            {
                double aki = a[k, i];
                if (aki == 0.0) continue;
                for (int j = 0; j < q; j++)
                    ri[j] += aki * b[k, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Decomposição em autovalores de uma matriz SIMÉTRICA pelo método cíclico de Jacobi (rotações).
    /// Devolve os autovalores em ordem CRESCENTE e os autovetores como COLUNAS de
    /// <paramref name="eigenvectors"/> (<c>eigenvectors[a, p]</c> = componente a do p-ésimo autovetor),
    /// na mesma convenção do <c>Evd(Symmetricity.Symmetric)</c> do MathNet. Robusto e O(n³) por varredura;
    /// adequado a matrizes pequenas (nº de parâmetros do refinamento). Só o triângulo superior+diagonal é lido
    /// (a matriz é assumida simétrica).
    /// </summary>
    public static bool SymmetricEigen(iMatrix matrix, out double[] eigenvalues, out MatrixArrays eigenvectors,
        int maxSweeps = 100, double tolerance = 1e-15)
    {
        int n = matrix.RowCount;
        if (n != matrix.ColumnCount)
            throw new Exception("The matrix must be square.");

        // Cópia de trabalho (simetrizada a partir do triângulo superior) e acumulador de autovetores V.
        double[][] a = new double[n][];
        double[][] v = new double[n][];
        for (int i = 0; i < n; i++)
        {
            a[i] = new double[n];
            v[i] = new double[n];
            v[i][i] = 1.0;
        }
        for (int i = 0; i < n; i++)
        {
            a[i][i] = matrix[i, i];
            for (int j = i + 1; j < n; j++)
            {
                double val = matrix[i, j];
                a[i][j] = val;
                a[j][i] = val;
            }
        }

        bool converged = false;
        for (int sweep = 0; sweep < maxSweeps; sweep++)
        {
            // Soma dos quadrados dos elementos fora da diagonal.
            double off = 0.0;
            for (int p = 0; p < n; p++)
                for (int q = p + 1; q < n; q++)
                    off += a[p][q] * a[p][q];
            if (off <= tolerance)
            {
                converged = true;
                break;
            }

            for (int p = 0; p < n; p++)
            {
                for (int q = p + 1; q < n; q++)
                {
                    double apq = a[p][q];
                    if (apq == 0.0) continue;

                    // Ângulo de rotação que zera a[p][q].
                    double theta = (a[q][q] - a[p][p]) / (2.0 * apq);
                    double t = Sign(theta) / (Abs(theta) + Sqrt(theta * theta + 1.0));
                    if (theta == 0.0) t = 1.0;
                    double c = 1.0 / Sqrt(t * t + 1.0);
                    double s = t * c;

                    // Aplica a rotação de Givens nas linhas/colunas p,q de A.
                    double app = a[p][p], aqq = a[q][q];
                    a[p][p] = c * c * app - 2.0 * s * c * apq + s * s * aqq;
                    a[q][q] = s * s * app + 2.0 * s * c * apq + c * c * aqq;
                    a[p][q] = 0.0;
                    a[q][p] = 0.0;
                    for (int k = 0; k < n; k++)
                    {
                        if (k == p || k == q) continue;
                        double akp = a[k][p], akq = a[k][q];
                        a[k][p] = c * akp - s * akq;
                        a[p][k] = a[k][p];
                        a[k][q] = s * akp + c * akq;
                        a[q][k] = a[k][q];
                    }
                    // Acumula a rotação em V (autovetores como colunas).
                    for (int k = 0; k < n; k++)
                    {
                        double vkp = v[k][p], vkq = v[k][q];
                        v[k][p] = c * vkp - s * vkq;
                        v[k][q] = s * vkp + c * vkq;
                    }
                }
            }
        }

        // Autovalores = diagonal; ordena CRESCENTE e reordena as colunas de V.
        var idx = new int[n];
        var diag = new double[n];
        for (int i = 0; i < n; i++) { idx[i] = i; diag[i] = a[i][i]; }
        Array.Sort(idx, (x, y) => diag[x].CompareTo(diag[y]));

        eigenvalues = new double[n];
        eigenvectors = new MatrixArrays(n, n);
        double[][] vec = eigenvectors.data;
        for (int col = 0; col < n; col++)
        {
            int src = idx[col];
            eigenvalues[col] = diag[src];
            for (int row = 0; row < n; row++)
                vec[row][col] = v[row][src];
        }
        return converged;
    }
}
