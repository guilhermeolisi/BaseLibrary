using System.Text;
using static System.Math;

namespace Sindarin.Math.Matrix;

public static class MatrixMethods
{
    static Random random = new Random();


    //public static double[] GetData(this iMatrix) { data; }
    //public void SetData(double[] data) => this.data = data;
    public static iMatrix Add(this iMatrix matrix1, iMatrix matrix2)
    {
        //Verify if the matrices have the same size.
        if (matrix1.RowCount != matrix2.RowCount || matrix1.ColumnCount != matrix2.ColumnCount)
        {
            throw new Exception("The matrices must have the same size.");
        }

        iMatrix result = new MatrixArrays(matrix1.RowCount, matrix1.ColumnCount);
        // Loop parallel through the rows of the first matrix.
        Parallel.For(0, matrix1.RowCount, i =>
        {
            // Loop through the columns of the second matrix.
            for (int j = 0; j < matrix1.ColumnCount; j++)
            {
                // Add the values of the matrices.
                result[i, j] = matrix1[i, j] + matrix2[i, j];
            }
        });
        return result;
    }
    /// <summary>
    /// Subtract two matrices.
    /// </summary>
    /// <param name="matrix1"></param>
    /// <param name="matrix2"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static iMatrix Subtract(this iMatrix matrix1, iMatrix matrix2)
    {
        //Verify if the matrices have the same size.
        if (matrix1.RowCount != matrix2.RowCount || matrix1.ColumnCount != matrix2.ColumnCount)
        {
            throw new Exception("The matrices must have the same size.");
        }

        iMatrix result = new MatrixArrays(matrix1.RowCount, matrix1.ColumnCount);

        // Loop parallel through the rows of the first matrix.
        Parallel.For(0, matrix1.RowCount, i =>
        {
            // Loop through the columns of the second matrix.
            for (int j = 0; j < matrix1.ColumnCount; j++)
            {
                // Subtract the two matrices.
                result[i, j] = matrix1[i, j] - matrix2[i, j];
            }
        });
        return result;
    }

    /// <summary>
    /// Multiplies two matrices together.
    /// </summary>
    /// <param name="matrix1"></param>
    /// <param name="matrix2"></param>
    /// <returns></returns>
    public static iMatrix Multiply(this iMatrix matrix1, iMatrix matrix2)
    {
        // Check if the matrices can be multiplied.
        // Número de colunas da primeira matriz deve ser igual ao número de linhas da segunda matriz.
        if (matrix1.ColumnCount != matrix2.RowCount)
        {
            throw new Exception("The matrices cannot be multiplied.");
        }

        // Create the result matrix.
        iMatrix result = new MatrixArrays(matrix1.RowCount, matrix2.ColumnCount);

        // Loop parallel through the rows of the first matrix.
        Parallel.For(0, matrix1.RowCount, i =>
        {
            // Loop through the columns of the second matrix.
            for (int j = 0; j < matrix2.ColumnCount; j++)
            {
                // Loop through the rows of the second matrix.
                for (int k = 0; k < matrix2.RowCount; k++)
                {
                    // Add the product of the two matrices to the result matrix.
                    result[i, j] += matrix1[i, k] * matrix2[k, j];
                }
            }
        });

        // Return the result matrix.
        return result;
    }

    public static iMatrix Multiply(this iMatrix matrix1, double value)
    {
        iMatrix result = new MatrixArrays(matrix1.RowCount, matrix1.ColumnCount);
        // Loop parallel through the rows of the first matrix.
        Parallel.For(0, matrix1.RowCount, i =>
        {
            // Loop through the columns of the second matrix.
            for (int j = 0; j < matrix1.ColumnCount; j++)
            {
                // Add the values of the matrices.
                result[i, j] = matrix1[i, j] * value;
            }
        });
        return result;
    }
    public static iMatrix MultiplyScale(this iMatrix matrix1, iMatrix matrix2)
    {
        //Verify if the matrices have the same size.
        if (matrix1.RowCount != matrix2.RowCount || matrix1.ColumnCount != matrix2.ColumnCount)
        {
            throw new Exception("The matrices must have the same size.");
        }

        iMatrix result = new MatrixArrays(matrix1.RowCount, matrix1.ColumnCount);
        // Loop parallel through the rows of the first matrix.
        Parallel.For(0, matrix1.RowCount, i =>
        {
            // Loop through the columns of the second matrix.
            for (int j = 0; j < matrix1.ColumnCount; j++)
            {
                // Add the values of the matrices.
                result[i, j] = matrix1[i, j] * matrix2[i, j];
            }
        });
        return result;
    }
    public static iMatrix DivideScale(this iMatrix matrix1, iMatrix matrix2)
    {
        //Verify if the matrices have the same size.
        if (matrix1.RowCount != matrix2.RowCount || matrix1.ColumnCount != matrix2.ColumnCount)
        {
            throw new Exception("The matrices must have the same size.");
        }

        iMatrix result = new MatrixArrays(matrix1.RowCount, matrix1.ColumnCount);
        // Loop parallel through the rows of the first matrix.
        Parallel.For(0, matrix1.RowCount, i =>
        {
            // Loop through the columns of the second matrix.
            for (int j = 0; j < matrix1.ColumnCount; j++)
            {
                // Add the values of the matrices.
                result[i, j] = matrix1[i, j] / matrix2[i, j];
            }
        });
        return result;
    }
    public static iMatrix Divide(this iMatrix matrix1, double value)
    {
        iMatrix result = new MatrixArrays(matrix1.RowCount, matrix1.ColumnCount);
        // Loop parallel through the rows of the first matrix.
        Parallel.For(0, matrix1.RowCount, i =>
        {
            // Loop through the columns of the second matrix.
            for (int j = 0; j < matrix1.ColumnCount; j++)
            {
                // Add the values of the matrices.
                result[i, j] = matrix1[i, j] / value;
            }
        });
        return result;
    }
    public static iMatrix TransposeAndMultiply(this iMatrix matrix1, iMatrix matrix2)
    {
        // Check if the matrices can be multiplied.
        // Número de linhas da primeira matriz deve ser igual ao número de linhas da segunda matriz.
        if (matrix1.RowCount != matrix2.RowCount)
        {
            throw new Exception("The matrices cannot be multiplied.");
        }

        // Create the result matrix.
        iMatrix result = new MatrixArrays(matrix1.ColumnCount, matrix2.ColumnCount);

        // Loop parallel through the rows of the first matrix.
        Parallel.For(0, matrix1.ColumnCount, i =>
        {
            // Loop through the columns of the second matrix.
            for (int j = 0; j < matrix2.ColumnCount; j++)
            {
                // Loop through the rows of the second matrix.
                for (int k = 0; k < matrix2.RowCount; k++)
                {
                    // Add the product of the two matrices to the result matrix.
                    result[i, j] += matrix1[k, i] * matrix2[k, j];
                }
            }
        });

        // Return the result matrix.
        return result;
    }
    public static double Determinant(this iMatrix matrix)
    {
        // Check if the matrix is square.
        if (matrix.RowCount != matrix.ColumnCount)
        {
            throw new Exception("The matrix must be square.");
        }

        // Check if the matrix is 2x2.
        if (matrix.RowCount == 2)
        {
            return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
        }

        // Create the result variable.
        double result = 0;

        // Loop through the columns of the matrix.
        for (int i = 0; i < matrix.ColumnCount; i++)
        {
            // Create the submatrix.
            MatrixArrays subMatrix = new MatrixArrays(matrix.RowCount - 1, matrix.ColumnCount - 1);

            // Loop through the rows of the matrix.
            for (int j = 1; j < matrix.RowCount; j++)
            {
                // Loop through the columns of the matrix.
                for (int k = 0; k < matrix.ColumnCount; k++)
                {
                    // Check if the column is the same as the current column.
                    if (k < i)
                    {
                        // Set the value of the submatrix.
                        subMatrix[j - 1, k] = matrix[j, k];
                    }
                    // Check if the column is greater than the current column.
                    else if (k > i)
                    {
                        // Set the value of the submatrix.
                        subMatrix[j - 1, k - 1] = matrix[j, k];
                    }
                }
            }

            // Add the product of the matrix and the determinant of the submatrix to the result.
            result += matrix[0, i] * Pow(-1, i) * Determinant(subMatrix);
        }

        // Return the result.
        return result;
    }
    /// <summary>
    /// Calcule the determinant of a matrix using the LU decomposition method.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static double DeterminantLU(this iMatrix matrix)
    {
        int toggle;
        iMatrix lum = matrix.DecomposeLU(out _, out toggle);
        double result = toggle;

        for (int i = 0; i < matrix.RowCount; ++i)
            result *= matrix[i, i];
        return result;
    }
    /// <summary>
    /// Calcule the inverse of a matrix using the Gauss-Jordan method with.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static iMatrix InverseGJ(this iMatrix matrix)
    {
        //TODO implementar paralelismo

        // Check if the matrix is square.
        if (matrix.RowCount != matrix.ColumnCount)
        {
            throw new Exception("The matrix must be square.");
        }

        // Create the result matrix.
        iMatrix result = new Identity(matrix.RowCount);


        int n = matrix.RowCount;

        // Criar uma matriz estendida com a identidade
        double[,] matrizEstendida = new double[n, 2 * n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                matrizEstendida[i, j] = matrix[i, j];
                matrizEstendida[i, j + n] = (i == j) ? 1 : 0; // Adicionar a identidade
            }
        }

        // Eliminação de Gauss-Jordan
        for (int i = 0; i < n; i++)
        {
            double pivot = matrizEstendida[i, i];

            // Dividir a linha pelo pivô
            for (int j = 0; j < 2 * n; j++)
            {
                matrizEstendida[i, j] /= pivot;
            }

            // Reduzir as outras linhas
            for (int k = 0; k < n; k++)
            {
                if (k != i)
                {
                    double factor = matrizEstendida[k, i];
                    for (int j = 0; j < 2 * n; j++)
                    {
                        matrizEstendida[k, j] -= factor * matrizEstendida[i, j];
                    }
                }
            }
        }

        // Extrair a matriz inversa
        iMatrix matrizInversa = new MatrixArrays(n, n);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                matrizInversa[i, j] = matrizEstendida[i, j + n];
            }
        }

        return matrizInversa;

    }
    #region LU from MathNet

    // from class DenseColumnMajorMatrixStorage<T>
    //public override T At(int row, int column)
    //{
    //    return Data[(column * RowCount) + row];
    //}

    /// <summary>
    /// Computes the LUP factorization of A. P*A = L*U.
    /// </summary>
    /// <param name="m">An <paramref name="order"/> by <paramref name="order"/> matrix. The matrix is overwritten with the
    /// the LU factorization on exit. The lower triangular factor L is stored in under the diagonal of <paramref name="m"/> (the diagonal is always 1.0
    /// for the L factor). The upper triangular factor U is stored on and above the diagonal of <paramref name="data"/>.</param>
    /// <param name="order">The order of the square matrix <paramref name="data"/>.</param>
    /// <param name="ipiv">On exit, it contains the pivot indices. The size of the array must be <paramref name="order"/>.</param>
    /// <remarks>This is equivalent to the GETRF LAPACK routine.</remarks>
    public static iMatrix LUFactor(this iMatrix m, int[] ipiv)
    {
        if (m == null)
        {
            throw new ArgumentNullException(nameof(m));
        }

        if (ipiv == null)
        {
            throw new ArgumentNullException(nameof(ipiv));
        }

        if (m.RowCount != m.ColumnCount)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(m));
        }

        if (ipiv.Length != m.RowCount)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
        }

        // Initialize the pivot matrix to the identity permutation.
        for (var i = 0; i < m.RowCount; i++)
        {
            ipiv[i] = i;
        }

        //var vecLUcolj = new double[order];
        var vecLUcolj = new double[m.RowCount];

        // Outer loop.
        // for (var j = 0; j < order; j++)
        for (var j = 0; j < m.RowCount; j++)
        {
            //var indexj = j * order;
            //var indexjj = indexj + j;

            // Make a copy of the j-th column to localize references.
            //for (var i = 0; i < order; i++)
            //{
            //    vecLUcolj[i] = data[indexj + i];
            //}
            for (var i = 0; i < m.ColumnCount; i++)
            {
                vecLUcolj[i] = m[i, j];
            }

            // Apply previous transformations.
            //for (var i = 0; i < order; i++)
            for (var i = 0; i < m.ColumnCount; i++)
            {
                // Most of the time is spent in the following dot product.
                var kmax = Min(i, j);
                var s = 0.0;
                for (var k = 0; k < kmax; k++)
                {
                    //s += data[(k * order) + i] * vecLUcolj[k];
                    s += m[i, k] * vecLUcolj[k];
                }

                //data[indexj + i] = vecLUcolj[i] -= s;
                m[i, j] = vecLUcolj[i] -= s;
            }

            // Find pivot and exchange if necessary.
            var p = j;
            //for (var i = j + 1; i < order; i++)
            for (var i = j + 1; i < m.ColumnCount; i++)
            {
                if (Abs(vecLUcolj[i]) > Abs(vecLUcolj[p]))
                {
                    p = i;
                }
            }

            if (p != j)
            {
                //for (var k = 0; k < order; k++)
                for (var k = 0; k < m.ColumnCount; k++)
                {
                    //var indexk = k * order;
                    //var indexkp = indexk + p;
                    //var indexkj = indexk + j;
                    //(data[indexkp], data[indexkj]) = (data[indexkj], data[indexkp]);
                    //(m[p, k], m[j, k]) = (m[j, k], m[p, k]);

                    double temp = m[p, k];
                    m[p, k] = m[j, k];
                    m[j, k] = temp;
                }

                ipiv[j] = p;
            }

            // Compute multipliers.
            //if (j < order & data[indexjj] != 0.0)
            if (j < m.ColumnCount & m[j, j] != 0.0)
            {
                //for (var i = j + 1; i < order; i++)
                for (var i = j + 1; i < m.ColumnCount; i++)
                {
                    //data[indexj + i] /= data[indexjj];
                    m[i, j] /= m[j, j];
                }
            }
        }
        return m;
    }
    /// <summary>
    /// Computes the inverse of matrix using LU factorization.
    /// </summary>
    /// <param name="m">The N by N matrix to invert. Contains the inverse On exit.</param>
    /// <remarks>This is equivalent to the GETRF and GETRI LAPACK routines.</remarks>
    public static iMatrix LUInverse(this iMatrix m)
    {
        if (m == null)
        {
            throw new ArgumentNullException(nameof(m));
        }

        if (m.RowCount != m.ColumnCount)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(m));
        }

        var ipiv = new int[m.RowCount];
        LUFactor(m, ipiv);
        LUInverseFactored(m, ipiv);
        return m;
    }

    /// <summary>
    /// Computes the inverse of a previously factored matrix.
    /// </summary>
    /// <param name="m">The LU factored N by N matrix.  Contains the inverse On exit.</param>
    /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
    /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
    /// <remarks>This is equivalent to the GETRI LAPACK routine.</remarks>
    public static iMatrix LUInverseFactored(this iMatrix m, int[] ipiv)
    {
        if (m == null)
        {
            throw new ArgumentNullException(nameof(m));
        }

        if (ipiv == null)
        {
            throw new ArgumentNullException(nameof(ipiv));
        }

        if (m.RowCount != m.ColumnCount)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(m));
        }

        if (ipiv.Length != m.RowCount)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
        }

        //var inverse = new double[m.Length]; //Comprimento total da matriz
        //for (var i = 0; i < order; i++)
        //{
        //    inverse[i + (order * i)] = 1.0;
        //}

        iMatrix inverse = new MatrixArrays(m.RowCount, m.ColumnCount);
        for (var i = 0; i < m.RowCount; i++)
        {
            inverse[i, i] = 1.0;
        }

        LUSolveFactored(m, ipiv, inverse);
        inverse.CopyTo(m);
        return m;
    }

    /// <summary>
    /// Solves A*X=B for X using LU factorization.
    /// </summary>
    /// <param name="columnsOfB">The number of columns of B.</param>
    /// <param name="a">The square matrix A.</param>
    /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
    /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
    /// <remarks>This is equivalent to the GETRF and GETRS LAPACK routines.</remarks>
    //public void LUSolve(int columnsOfB, double[] a, int order, double[] b)
    //{
    //    if (a == null)
    //    {
    //        throw new ArgumentNullException(nameof(a));
    //    }

    //    if (b == null)
    //    {
    //        throw new ArgumentNullException(nameof(b));
    //    }

    //    if (a.Length != order * order)
    //    {
    //        throw new ArgumentException("The array arguments must have the same length.", nameof(a));
    //    }

    //    if (b.Length != order * columnsOfB)
    //    {
    //        throw new ArgumentException("The array arguments must have the same length.", nameof(b));
    //    }

    //    if (ReferenceEquals(a, b))
    //    {
    //        throw new ArgumentException("Arguments must be different objects.");
    //    }

    //    var ipiv = new int[order];
    //    var clone = new double[a.Length];
    //    a.Copy(clone);
    //    LUFactor(clone, order, ipiv);
    //    LUSolveFactored(columnsOfB, clone, order, ipiv, b);
    //}

    /// <summary>
    /// Solves A*X=B for X using a previously factored A matrix.
    /// </summary>
    /// <param name="columnsOfB">The number of columns of B.</param>
    /// <param name="m">The factored A matrix.</param>
    /// <param name="order">The order of the square matrix <paramref name="m"/>.</param>
    /// <param name="ipiv">The pivot indices of <paramref name="m"/>.</param>
    /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
    /// <remarks>This is equivalent to the GETRS LAPACK routine.</remarks>
    public static iMatrix LUSolveFactored(this iMatrix m, int[] ipiv, iMatrix b)
    {
        if (m == null)
        {
            throw new ArgumentNullException(nameof(m));
        }

        if (ipiv == null)
        {
            throw new ArgumentNullException(nameof(ipiv));
        }

        if (b == null)
        {
            throw new ArgumentNullException(nameof(b));
        }

        if (m.RowCount != m.ColumnCount)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(m));
        }

        if (ipiv.Length != m.RowCount)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
        }

        if (b.RowCount != m.RowCount || b.ColumnCount != m.ColumnCount)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(b));
        }

        if (ReferenceEquals(m, b))
        {
            throw new ArgumentException("Arguments must be different objects.");
        }

        // Compute the column vector  P*B
        for (var i = 0; i < ipiv.Length; i++)
        {
            if (ipiv[i] == i)
            {
                continue;
            }

            var p = ipiv[i];
            for (var j = 0; j < b.ColumnCount; j++)
            {
                //var indexk = j * order;
                //var indexkp = indexk + p;
                //var indexkj = indexk + i;
                //(b[indexkp], b[indexkj]) = (b[indexkj], b[indexkp]);
                (b[j, p], b[j, i]) = (b[j, i], b[j, p]);
            }
        }

        // Solve L*Y = P*B
        for (var k = 0; k < m.RowCount; k++)
        {
            //var korder = k * order;
            for (var i = k + 1; i < m.RowCount; i++)
            {
                for (var j = 0; j < b.ColumnCount; j++)
                {
                    //var index = j * order;
                    //b[i + index] -= b[k + index] * m[i + korder];
                    b[j, i] -= b[j, k] * m[k, i];
                }
            }
        }

        // Solve U*X = Y;
        for (var k = m.RowCount - 1; k >= 0; k--)
        {
            //var korder = k + (k * order);
            for (var j = 0; j < b.ColumnCount; j++)
            {
                //b[k + (j * order)] /= m[korder];
                b[j, k] /= m[k, k];
            }

            //korder = k * order;
            for (var i = 0; i < k; i++)
            {
                for (var j = 0; j < b.ColumnCount; j++)
                {
                    //var index = j * order;
                    //b[i + index] -= b[k + index] * m[i + korder];
                    b[j, i] -= b[j, k] * m[k, j];
                }
            }
        }
        return b;
    }
    #endregion
    #region LU from site data
    public static iMatrix InverseLUData(this iMatrix matrix)
    {
        if (matrix is MatrixArrays ma)
        {
            //double[][] data = ma.data; //ma.GetData();
            double[][] result = InverseLU(ma.data);
            return new MatrixArrays(result);
        }

        return null;
    }
    public static double[][] InverseLU(this double[][] matrix)
    {
        // Check if the matrix is square.
        if (matrix.Length != matrix[0].Length)
        {
            throw new Exception("The matrix must be square.");
        }

        int toggle;
        int[] perm;  // out parameter
        double[][] lum = matrix.DecomposeLU(out perm, out toggle); //ignore toggle

        // Check if the determinant is zero.
        if (lum.DeterminantWithLU(toggle) == 0)
        {
            throw new Exception("The matrix is not invertible.");
        }

        int n = matrix.Length;

        // Create the result matrix.
        double[][] result = new double[n][];
        for (int i = 0; i < n; i++)
        {
            result[i] = new double[n];
        }

        for (int i = 0; i < n; ++i)
        {

            double[] b = new double[n];
            for (int j = 0; j < n; ++j)
                if (i == perm[j])
                {
                    b[j] = 1.0;
                    break;
                }

            double[] x = lum.SolveLinearLU(b); // 
            //TODO fazer um metodo substitua a coluna com um array
            for (int j = 0; j < n; ++j)
                result[j][i] = x[j];
        }
        return result;
    }
    public static double[][] DecomposeLU(this double[][] matrix, out int[] perm, out int toggle)
    {
        // Crout's LU decomposition for matrix determinant and inverse
        // stores combined lower & upper in lum[][]
        // stores row permuations into perm[]
        // returns +1 or -1 according to even or odd number of row permutations
        // lower gets dummy 1.0s on diagonal (0.0s above)
        // upper gets lum values on diagonal (0.0s below)

        toggle = +1; // even (+1) or odd (-1) row permutatuions
        int n = matrix.Length;

        // make a copy of m[][] into result lu[][]
        double[][] lum = new double[n][];
        for (int i = 0; i < n; ++i)
        {
            lum[i] = new double[n];
            for (int j = 0; j < n; ++j)
                lum[i][j] = matrix[i][j];
        }

        // make perm[]
        perm = new int[n];
        for (int i = 0; i < n; ++i)
            perm[i] = i;

        for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
        {
            double max = Abs(lum[j][j]);
            int piv = j;

            for (int i = j + 1; i < n; ++i) // find pivot index
            {
                double xij = Abs(lum[i][j]);
                if (xij > max)
                {
                    max = xij;
                    piv = i;
                }
            } // i

            if (piv != j)
            {
                double[] tmp = lum[piv]; // swap rows j, piv
                lum[piv] = lum[j];
                lum[j] = tmp;

                int t = perm[piv]; // swap perm elements
                perm[piv] = perm[j];
                perm[j] = t;

                toggle = -toggle;
            }

            double xjj = lum[j][j];
            if (xjj != 0.0)
            {
                for (int i = j + 1; i < n; ++i)
                {
                    double xij = lum[i][j] / xjj;
                    lum[i][j] = xij;
                    for (int k = j + 1; k < n; ++k)
                        lum[i][k] -= xij * lum[j][k];
                }
            }

        } // j
        return lum;
    }
    public static double DeterminantWithLU(this double[][] matrixLU, double toggleLU)
    {
        for (int i = 0; i < matrixLU.Length; ++i)
            toggleLU *= matrixLU[i][i];
        return toggleLU;
    }
    internal static double[] SolveLinearLU(this double[][] lum, double[] b)
    {
        int n = lum.Length;
        double[] x = new double[n];
        //b.CopyTo(x, 0);
        for (int i = 0; i < n; ++i)
            x[i] = b[i];

        for (int i = 1; i < n; ++i)
        {
            double sum = x[i];
            for (int j = 0; j < i; ++j)
                sum -= lum[i][j] * x[j];
            x[i] = sum;
        }

        x[n - 1] /= lum[n - 1][n - 1];
        for (int i = n - 2; i >= 0; --i)
        {
            double sum = x[i];
            for (int j = i + 1; j < n; ++j)
                sum -= lum[i][j] * x[j];
            x[i] = sum / lum[i][i];
        }

        return x;
    }
    #endregion
    #region LU from site
    /// <summary>
    /// Calcule the inverse of a matrix using the Crout's LU decomposition.
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static iMatrix InverseLU(this iMatrix matrix)
    {
        //TODO implementar paralelismo
        //TODO implementar inplace

        // Check if the matrix is square.
        if (matrix.RowCount != matrix.ColumnCount)
        {
            throw new Exception("The matrix must be square.");
        }

        int toggle;
        int[] perm;  // out parameter
        iMatrix lum = matrix.DecomposeLU(out perm, out toggle); //ignore toggle

        // Check if the determinant is zero.
        if (lum.DeterminantWithLU(toggle) == 0)
        {
            throw new Exception("The matrix is not invertible.");
        }

        int n = matrix.ColumnCount;

        // Create the result matrix.
        iMatrix result = new MatrixArrays(n, n);


        for (int i = 0; i < n; ++i)
        {
            int ind = -1;
            for (int j = 0; j < n; ++j)
                if (i == perm[j])
                    ind = j;

            iVector b = new VectorWithOne(n, ind);
            //iVector b2 = new Vector(n);
            //for (int j = 0; j < n; ++j)
            //    if (i == perm[j])
            //        b2[j] = 1.0;
            //    else
            //        b2[j] = 0.0;
            iVector x = lum.SolveLinearLU(b); // 
            //TODO fazer um metodo substitua a coluna com um array
            for (int j = 0; j < n; ++j)
                result[j, i] = x[j];
        }
        return result;
    }
    public static iMatrix InverseLU(this iMatrix matrix, iMatrix lum, int[] perm, int toggle)
    {
        // Check if the matrix is square.
        if (matrix.RowCount != matrix.ColumnCount)
        {
            throw new Exception("The matrix must be square.");
        }

        // Check if the determinant is zero.
        if (lum.DeterminantWithLU(toggle) == 0)
        {
            throw new Exception("The matrix is not invertible.");
        }

        int n = matrix.ColumnCount;

        // Create the result matrix.
        iMatrix result = new MatrixArrays(n, n);


        for (int i = 0; i < n; ++i)
        {
            int ind = -1;
            for (int j = 0; j < n; ++j)
                if (i == perm[j])
                    ind = j;

            iVector b = new VectorWithOne(n, ind);
#if DEBUG
            iVector b2 = new Vector(n);
            for (int j = 0; j < n; ++j)
                if (i == perm[j])
                    b2[j] = 1.0;
                else
                    b2[j] = 0.0;
#endif
            iVector x = lum.SolveLinearLU(b); // 
            //TODO fazer um metodo substitua a coluna com um array
            for (int j = 0; j < n; ++j)
                result[j, i] = x[j];
        }
        return result;
    }
    /// <summary>
    /// Calcule the inverse of a matrix using the Crout's LU decomposition in place
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static iMatrix InverseLUInPlace(this iMatrix matrix)
    {

        // Check if the matrix is square.
        if (matrix.RowCount != matrix.ColumnCount)
        {
            throw new Exception("The matrix must be square.");
        }

        int toggle;
        int[] perm;  // out parameter
        matrix.DecomposeLUInPlace(out perm, out toggle); //ignore toggle

        // Check if the determinant is zero.
        if (matrix.DeterminantWithLU(toggle) == 0)
        {
            throw new Exception("The matrix is not invertible.");
        }

        int n = matrix.ColumnCount;

        iMatrix b = new MatrixArrays(n, n);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; ++j)
            {
                if (i == perm[j])
                {
                    b[j, i] = 1.0;
                    break;
                }
            }
        }
        matrix.SolveLinearLUInPlace(b); // 
        b.CopyToReference(matrix);
        return matrix;
        //for (int i = 0; i < n; ++i)
        //{
        //    iVector b = new Vector(n);
        //    for (int j = 0; j < n; ++j)
        //        if (i == perm[j])
        //        {
        //            b[j] = 1.0;
        //            break;
        //        }
        //    matrix.SolveLinearLUInPlace(b); // 
        //    for (int j = 0; j < n; ++j)
        //        matrix[j, i] = b[j];
        //}
        //return matrix;
    }
    internal static iVector SolveLinearLU(this iMatrix lum, iVector b)
    {
        int n = lum.RowCount;
        double[] x = new double[n];
        //b.CopyTo(x, 0);
        for (int i = 0; i < n; ++i)
            x[i] = b[i];

        //x[0] = b[0];
        //for (int i = 1; i < n; ++i)
        //{
        //    double sum = b[i];
        //    for (int j = 0; j < i; ++j)
        //        sum -= lum[i, j] * b[j];
        //    x[i] = sum;
        //}

        for (int i = 1; i < n; ++i)
        {
            double sum = x[i];
            for (int j = 0; j < i; ++j)
                sum -= lum[i, j] * x[j];
            x[i] = sum;
        }

        x[n - 1] /= lum[n - 1, n - 1];
        for (int i = n - 2; i >= 0; --i)
        {
            double sum = x[i];
            for (int j = i + 1; j < n; ++j)
                sum -= lum[i, j] * x[j];
            x[i] = sum / lum[i, i];
        }

        return new Vector(x);
    }
    internal static iMatrix SolveLinearLUInPlace(this iMatrix lum, iMatrix b)
    {
        int n = lum.RowCount;

        for (int c = 0; c < b.ColumnCount; c++)
        {


            for (int i = 1; i < n; ++i)
            {
                double sum = b[i, c];
                for (int j = 0; j < i; ++j)
                    sum -= lum[i, j] * b[j, c];
                b[i, c] = sum;
            }

            b[n - 1, c] /= lum[n - 1, n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = b[i, c];
                for (int j = i + 1; j < n; ++j)
                    sum -= lum[i, j] * b[j, c];
                b[i, c] = sum / lum[i, i];
            }
        }
        return b;
    }
    internal static iVector SolveLinearLUInPlace(this iMatrix lum, iVector b)
    {
        int n = lum.RowCount;

        for (int i = 1; i < n; ++i)
        {
            double sum = b[i];
            for (int j = 0; j < i; ++j)
                sum -= lum[i, j] * b[j];
            b[i] = sum;
        }

        b[n - 1] /= lum[n - 1, n - 1];
        for (int i = n - 2; i >= 0; --i)
        {
            double sum = b[i];
            for (int j = i + 1; j < n; ++j)
                sum -= lum[i, j] * b[j];
            b[i] = sum / lum[i, i];
        }

        return b;
    }
    public static iMatrix DecomposeLU(this iMatrix matrix, out int[] perm, out int toggle)
    {
        // Crout's LU decomposition for matrix determinant and inverse
        // stores combined lower & upper in lum[][]
        // stores row permuations into perm[]
        // returns +1 or -1 according to even or odd number of row permutations
        // lower gets dummy 1.0s on diagonal (0.0s above)
        // upper gets lum values on diagonal (0.0s below)

        toggle = +1; // even (+1) or odd (-1) row permutatuions
        int n = matrix.RowCount;

        // make a copy of m[][] into result lu[][]
        iMatrix lum = new MatrixArrays(n, n);
        for (int i = 0; i < n; ++i)
            for (int j = 0; j < n; ++j)
                lum[i, j] = matrix[i, j];

        // make perm[]
        perm = new int[n];
        for (int i = 0; i < n; ++i)
            perm[i] = i;

        for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
        {
            double max = Abs(lum[j, j]);
            int piv = j;

            for (int i = j + 1; i < n; ++i) // find pivot index
            {
                double xij = Abs(lum[i, j]);
                if (xij > max)
                {
                    max = xij;
                    piv = i;
                }
            } // i

            if (piv != j)
            {
                iMatrix tmp = lum.GetRow(piv); // swap rows j, piv
                lum.SetRow(piv, lum.GetRowArray(j));
                lum.SetRow(j, tmp.GetRowArray(0));

                int t = perm[piv]; // swap perm elements
                perm[piv] = perm[j];
                perm[j] = t;

                toggle = -toggle;
            }

            double xjj = lum[j, j];
            if (xjj != 0.0)
            {
                for (int i = j + 1; i < n; ++i)
                {
                    double xij = lum[i, j] / xjj;
                    lum[i, j] = xij;
                    for (int k = j + 1; k < n; ++k)
                        lum[i, k] -= xij * lum[j, k];
                }
            }

        } // j
        return lum;
    }
    public static iMatrix DecomposeLUInPlace(this iMatrix matrix, out int[] perm, out int toggle)
    {
        // Crout's LU decomposition for matrix determinant and inverse
        // stores combined lower & upper in lum[][]
        // stores row permuations into perm[]
        // returns +1 or -1 according to even or odd number of row permutations
        // lower gets dummy 1.0s on diagonal (0.0s above)
        // upper gets lum values on diagonal (0.0s below)

        int toggleTemp = +1; // even (+1) or odd (-1) row permutatuions
        int n = matrix.RowCount;

        // make perm[]
        int[] permTem = new int[n];
        for (int i = 0; i < n; ++i)
            permTem[i] = i;

        for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
        {
            double max = Abs(matrix[j, j]);
            int piv = j;

            for (int i = j + 1; i < n; ++i) // find pivot index
            {
                double xij = Abs(matrix[i, j]);
                if (xij > max)
                {
                    max = xij;
                    piv = i;
                }
            } // i

            if (piv != j)
            {
                iMatrix tmp = matrix.GetRow(piv); // swap rows j, piv
                matrix.SetRow(piv, matrix.GetRowArray(j));
                matrix.SetRow(j, tmp.GetRowArray(0));

                int t = permTem[piv]; // swap perm elements
                permTem[piv] = permTem[j];
                permTem[j] = t;

                toggleTemp = -toggleTemp;
            }

            double xjj = matrix[j, j];
            if (xjj != 0.0)
            {
                for (int i = j + 1; i < n; ++i)
                {
                    double xij = matrix[i, j] / xjj;
                    matrix[i, j] = xij;
                    for (int k = j + 1; k < n; ++k)
                        matrix[i, k] -= xij * matrix[j, k];
                }
            }

        }
        perm = permTem;
        toggle = toggleTemp;
        return matrix;
    }
    #endregion
    public static double DeterminantWithLU(this iMatrix matrixLU, double toggleLU)
    {
        for (int i = 0; i < matrixLU.RowCount; ++i)
            toggleLU *= matrixLU[i, i];
        return toggleLU;
    }
    public static iMatrix ExtractLower(this iMatrix lum)
    {
        // lower part of an LU Crout's decomposition
        // (dummy 1.0s on diagonal, 0.0s above)
        int n = lum.RowCount;
        iMatrix result = new MatrixArrays(n, n);
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < n; ++j)
            {
                if (i == j)
                    result[i, j] = 1.0;
                else if (i > j)
                    result[i, j] = lum[i, j];
            }
        }
        return result;
    }
    public static iMatrix ExtractUpper(this iMatrix lum)
    {
        // upper part of an LU (lu values on diagional and above, 0.0s below)
        int n = lum.RowCount;
        iMatrix result = new MatrixArrays(n, n);
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < n; ++j)
            {
                if (i <= j)
                    result[i, j] = lum[i, j];
            }
        }
        return result;
    }
    public static iMatrix ReconstructFromLU(this iMatrix lum, int[] perm)
    {

        iMatrix lower = lum.ExtractLower();
        iMatrix upper = lum.ExtractUpper();

        return ReconstructFromLU(lower, upper, perm);
    }
    public static iMatrix ReconstructFromLU(iMatrix lower, iMatrix upper, int[] perm)
    {
        iMatrix tmp = lower.Multiply(upper);  // scrambled rows
        iMatrix result = new MatrixArrays(lower.RowCount, lower.ColumnCount);
        // suppose perm = [1, 3, 0, 2]
        // row 0 of tmp goes to row 1 of result,
        // row 1 of tmp goes to row 3 of result
        // row 2 of tmp goes to row 0 of result
        // etc.
        for (int i = 0; i < lower.RowCount; ++i)
        {
            int r = perm[i];
            for (int j = 0; j < lower.ColumnCount; ++j)
                result[r, j] = tmp[i, j];

        }
        return result;
    }
    public static iMatrix Copy(this iMatrix matrix)
    {
        if (matrix is MatrixArrays)
        {
            iMatrix copy = new MatrixArrays(matrix.RowCount, matrix.ColumnCount);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    copy[i, j] = matrix[i, j];
                }
            }
            return copy;
        }
        else if (matrix is Identity)
        {
            return new Identity(matrix.RowCount);
        }
        throw new NotImplementedException($"Not implemented copy method to matrix type {matrix.GetType().Name}");
    }
    public static iMatrix CopyTo(this iMatrix source, iMatrix dest)
    {
        if (source is MatrixArrays)
        {
            for (int i = 0; i < source.RowCount; i++)
            {
                for (int j = 0; j < source.ColumnCount; j++)
                {
                    dest[i, j] = source[i, j];
                }
            }
            return dest;
        }
        else if (source is Identity)
        {
            return new Identity(source.RowCount);
        }
        throw new NotImplementedException($"Not implemented copy method to matrix type {source.GetType().Name}");
    }
    public static iMatrix CopyToReference(this iMatrix source, iMatrix dest)
    {
        if (source is MatrixArrays ms && dest is MatrixArrays md)
        {
            md.SetData(ms.GetData());
            return md;
        }
        else if (source is Identity)
        {
            return new Identity(source.RowCount);
        }
        throw new NotImplementedException($"Not implemented copy method to matrix type {source.GetType().Name}");
    }
    public static void Copy(this double[] source, double[] dest)
    {
        Buffer.BlockCopy(source, 0, dest, 0, source.Length * sizeof(double));
    }
    public static string MatrixToString(this iMatrix matrix, int dec = 4, int wid = 10)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < matrix.RowCount; ++i)
        {
            for (int j = 0; j < matrix.ColumnCount; ++j)
            {
                double v = matrix[i, j];
                if (Abs(v) < 1.0e-5) v = 0.0;  // avoid "-0.00"
                sb.Append(v.ToString("G" + dec).PadLeft(wid));
            }
            if (i < matrix.ColumnCount - 1)
                sb.AppendLine();
        }

        return sb.ToString();
    }
    public static void FillMatrixRandon(this iMatrix matrix, double min = -1000, double max = 1000)
    {
        for (int i = 0; i < matrix.RowCount; i++)
        {
            for (int j = 0; j < matrix.ColumnCount; j++)
            {
                matrix[i, j] = random.NextDouble() * (max - min) + min;
            }
        }
    }
}
