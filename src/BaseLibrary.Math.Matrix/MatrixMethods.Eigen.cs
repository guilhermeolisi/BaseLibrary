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
    /// Decomposição em autovalores de uma matriz SIMÉTRICA pelo método cíclico de Jacobi (rotações de Givens),
    /// na variante de Numerical Recipes: itera até zerar os elementos fora da diagonal ATÉ O PISO de
    /// arredondamento (critério <c>sm == 0</c> + descarte dos off-diagonais desprezíveis relativos às
    /// diagonais), atingindo precisão de máquina (~1e-15 na reconstrução <c>V·Λ·Vᵀ</c>), não uma tolerância
    /// absoluta frouxa. Devolve os autovalores em ordem CRESCENTE e os autovetores como COLUNAS de
    /// <paramref name="eigenvectors"/> (<c>eigenvectors[a, p]</c> = componente a do p-ésimo autovetor), na mesma
    /// convenção do <c>Evd(Symmetricity.Symmetric)</c> do MathNet. Converge quadraticamente (~6–10 varreduras);
    /// só o triângulo superior+diagonal é lido (a matriz é assumida simétrica). Devolve <c>false</c> se não
    /// convergir em <paramref name="maxSweeps"/> (resultado ainda utilizável).
    /// </summary>
    public static bool SymmetricEigen(iMatrix matrix, out double[] eigenvalues, out MatrixArrays eigenvectors,
        int maxSweeps = 50)
    {
        int n = matrix.RowCount;
        if (n != matrix.ColumnCount)
            throw new Exception("The matrix must be square.");

        // Triângulo SUPERIOR de trabalho (o algoritmo só toca o triângulo superior) e acumulador de autovetores V.
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
                a[i][j] = matrix[i, j];
        }

        bool converged = false;
        for (int sweep = 0; sweep < maxSweeps; sweep++)
        {
            // sm = soma dos |off-diagonais| (triângulo superior). sm == 0 ⇒ tudo zerado até o piso de máquina.
            double sm = 0.0;
            for (int p = 0; p < n - 1; p++)
                for (int q = p + 1; q < n; q++)
                    sm += Abs(a[p][q]);
            if (sm == 0.0)
            {
                converged = true;
                break;
            }

            // Nas 3 primeiras varreduras só gira off-diagonais acima de um limiar (acelera); depois, todos.
            double tresh = sweep < 3 ? 0.2 * sm / (n * n) : 0.0;

            for (int p = 0; p < n - 1; p++)
            {
                for (int q = p + 1; q < n; q++)
                {
                    double apq = a[p][q];
                    double g = 100.0 * Abs(apq);
                    // Após algumas varreduras, zera off-diagonais DESPREZÍVEIS relativo às duas diagonais.
                    if (sweep > 3 && Abs(a[p][p]) + g == Abs(a[p][p]) && Abs(a[q][q]) + g == Abs(a[q][q]))
                    {
                        a[p][q] = 0.0;
                    }
                    else if (Abs(apq) > tresh)
                    {
                        double h = a[q][q] - a[p][p];
                        double t;
                        if (Abs(h) + g == Abs(h))
                            t = apq / h; // theta grande: aproximação estável
                        else
                        {
                            double theta = 0.5 * h / apq;
                            t = 1.0 / (Abs(theta) + Sqrt(1.0 + theta * theta));
                            if (theta < 0.0) t = -t;
                        }
                        double c = 1.0 / Sqrt(1.0 + t * t);
                        double s = t * c;
                        double tau = s / (1.0 + c);
                        h = t * apq;
                        a[p][p] -= h;
                        a[q][q] += h;
                        a[p][q] = 0.0;
                        // Rotaciona os demais elementos (só triângulo superior), forma ROTATE de NR (menos erro).
                        for (int r = 0; r < p; r++) JacobiRotate(a, r, p, r, q, s, tau);
                        for (int r = p + 1; r < q; r++) JacobiRotate(a, p, r, r, q, s, tau);
                        for (int r = q + 1; r < n; r++) JacobiRotate(a, p, r, q, r, s, tau);
                        for (int r = 0; r < n; r++) JacobiRotate(v, r, p, r, q, s, tau); // autovetores nas colunas
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

    /// <summary>Rotação de Jacobi (ROTATE de Numerical Recipes) sobre dois pares de elementos.</summary>
    private static void JacobiRotate(double[][] m, int i, int j, int k, int l, double s, double tau)
    {
        double g = m[i][j];
        double h = m[k][l];
        m[i][j] = g - s * (h + g * tau);
        m[k][l] = h + s * (g - h * tau);
    }
}
