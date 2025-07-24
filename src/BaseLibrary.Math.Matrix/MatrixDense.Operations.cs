using static System.Math;

namespace Sindarin.Math.Matrix;

public partial class MatrixDense
{
    /// <summary>
    /// Multiplies two matrices together.
    /// </summary>
    /// <param name="matrix1"></param>
    /// <param name="matrix2"></param>
    /// <returns></returns>
    public iMatrix Multiply(MatrixDense matrix2)
    {
        // Check if the matrices can be multiplied.
        // Número de colunas da primeira matriz deve ser igual ao número de linhas da segunda matriz.
        if (ColumnCount != matrix2.RowCount)
        {
            throw new Exception("The matrices cannot be multiplied.");
        }

        // Create the result 
        MatrixDense result = new MatrixDense(RowCount, matrix2.ColumnCount);

        double[] data = this.data; // To improve performance
        double[] data2 = matrix2.data;
        double[] dataResult = result.data;
        _ = data[data.Length - 1]; // To improve performance
        _ = data2[data2.Length - 1]; // To improve performance
        _ = dataResult[dataResult.Length - 1]; // To improve performance

        int n1 = RowCount;

        // Loop parallel through the rows of the first 
        Parallel.For(0, RowCount, i =>
        {
            // Loop through the columns of the second 
            for (int j = 0; j < matrix2.ColumnCount; j++)
            {
                int index2Colj = j * matrix2.RowCount;
                // Loop through the rows of the second 
                for (int k = 0; k < matrix2.RowCount; k++)
                {
                    // Add the product of the two matrices to the result 
                    dataResult[i + index2Colj] += data[i + k * n1] * data2[k + index2Colj];
                }
            }
        });

        // Return the result 
        return result;
    }
    public iMatrix Multiply(MatrixArrays matrix2)
    {
        // Check if the matrices can be multiplied.
        // Número de colunas da primeira matriz deve ser igual ao número de linhas da segunda matriz.
        if (ColumnCount != matrix2.RowCount)
        {
            throw new Exception("The matrices cannot be multiplied.");
        }

        // Create the result 
        MatrixDense result = new MatrixDense(RowCount, matrix2.ColumnCount);

        double[] data = this.data; // To improve performance
        double[][] data2 = matrix2.data;
        double[] dataResult = result.data;
        _ = data[data.Length - 1]; // To improve performance
        _ = dataResult[dataResult.Length - 1]; // To improve performance

        int n1 = RowCount;

        // Loop parallel through the rows of the first 
        Parallel.For(0, RowCount, i =>
        {
            // Loop through the columns of the second 
            for (int j = 0; j < matrix2.ColumnCount; j++)
            {
                int index2Colj = j * matrix2.RowCount;
                // Loop through the rows of the second 
                for (int k = 0; k < matrix2.RowCount; k++)
                {
                    // Add the product of the two matrices to the result 
                    dataResult[i + index2Colj] += data[i + k * n1] * data2[k][j];
                }
            }
        });

        // Return the result 
        return result;
    }
    public iMatrix TransposeAndMultiply(iMatrix matrix2)
    {
        //TODO fazer adaptações para calcular a partir do data

        // Check if the matrices can be multiplied.
        // Número de linhas da primeira matriz deve ser igual ao número de linhas da segunda matriz.
        if (RowCount != matrix2.RowCount)
        {
            throw new Exception("The matrices cannot be multiplied.");
        }

        // Create the result 
        iMatrix result = new MatrixArrays(ColumnCount, matrix2.ColumnCount);

        // Loop parallel through the rows of the first 
        Parallel.For(0, ColumnCount, i =>
        {
            // Loop through the columns of the second 
            for (int j = 0; j < matrix2.ColumnCount; j++)
            {
                // Loop through the rows of the second 
                for (int k = 0; k < matrix2.RowCount; k++)
                {
                    // Add the product of the two matrices to the result 
                    result[i, j] += this[k, i] * matrix2[k, j];
                }
            }
        });

        // Return the result 
        return result;
    }
    public iMatrix InverseLU()
    {
        // Check if the matrix is square.
        if (RowCount != ColumnCount)
        {
            throw new Exception("The matrix must be square.");
        }

        int toggle;
        int[] perm;  // out parameter
        double[] lum = DecomposeLU(out perm, out toggle); //ignore toggle

        // Check if the determinant is zero.
        if (DeterminantWithLU(lum, toggle) == 0)
        {
            throw new Exception("The matrix is not invertible.");
        }

        int nTotal = data.Length;
        int n = RowCount;

        // Create the result 
        double[] result = new double[nTotal];

        for (int j = 0; j < n; ++j) // col
        {
            int indexColj = j * n;
            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
                if (j == perm[i])
                {
                    b[i] = 1.0;
                    break;
                }

            double[] x = SolveLinearLU(lum, b);
            for (int i = 0; i < n; ++i) // row
                result[i + indexColj] = x[i];
        }
        return new MatrixDense(result, n);
    }

    public double[] DecomposeLU(out int[] perm, out int toggle)
    {
        // Crout's LU decomposition for matrix determinant and inverse
        // stores combined lower & upper in lum[][]
        // stores row permuations into perm[]
        // returns +1 or -1 according to even or odd number of row permutations
        // lower gets dummy 1.0s on diagonal (0.0s above)
        // upper gets lum values on diagonal (0.0s below)

        double[] data = this.data; //para tornar o acesso mais rápido
        _ = data[data.Length - 1]; //para tornar o acesso mais rápido

        toggle = +1; // even (+1) or odd (-1) row permutatuions
        int nTotal = data.Length;
        int n = RowCount;
        // make a copy of m[][] into result lu[][]
        double[] lum = new double[nTotal];
        //for (int j = 0; j < n; ++j) //colunas
        //{
        //    int indexColj = j * n;
        //    for (int i = 0; i < n; ++i) //linhas
        //        lum[i + indexColj] = data[i + indexColj];
        //}
        data.AsSpan().CopyTo(lum); // copy contents as a single dimensional array

        // make perm[]
        perm = new int[n];
        for (int i = 0; i < n; ++i)
            perm[i] = i;

        for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
        {
            int indexColj = j * n;
            double max = Abs(lum[j + indexColj]);
            int piv = j;

            for (int i = j + 1; i < n; ++i) // find pivot index
            {
                double xij = Abs(lum[i + indexColj]);
                if (xij > max)
                {
                    max = xij;
                    piv = i;
                }
            } // i

            if (piv != j)
            {
                for (int k = 0; k < n; k++)
                {
                    (lum[piv + k * n], lum[j + k * n]) = (lum[j + k * n], lum[piv + k * n]); // swap rows j, piv
                }

                (perm[piv], perm[j]) = (perm[j], perm[piv]); // swap perm elements

                //double[] tmp = lum[piv]; // swap rows j, piv
                //lum[piv] = lum[j];
                //lum[j] = tmp;

                //int t = perm[piv]; // swap perm elements
                //perm[piv] = perm[j];
                //perm[j] = t;

                toggle = -toggle;
            }

            double xjj = lum[j + indexColj];
            if (xjj != 0.0)
            {
                for (int i = j + 1; i < n; ++i)
                {
                    double xij = lum[i + indexColj] / xjj;
                    lum[i + indexColj] = xij;
                    for (int k = j + 1; k < n; ++k)
                        lum[i + k * n] -= xij * lum[j + k * n];
                }
            }

        } // j
        return lum;
    }
    public double[] SolveLinearLU(double[] lum, double[] b)
    {
        int n = RowCount;

        double[] x = new double[n];

        //for (int i = 0; i < n; ++i)
        //    x[i] = b[i];
        b.AsSpan().CopyTo(x); // copy contents as a single dimensional array

        for (int i = 1; i < n; ++i)
        {
            double sum = x[i];
            for (int j = 0; j < i; ++j)
            {
                sum -= lum[i + j * n] * x[j];
            }
            x[i] = sum;
        }

        x[n - 1] /= lum[n - 1 + (n - 1) * n];
        for (int i = n - 2; i >= 0; --i)
        {
            double sum = x[i];
            for (int j = i + 1; j < n; ++j)
                sum -= lum[i + j * n] * x[j];
            x[i] = sum / lum[i + i * n];
        }

        return x;
    }
    public iMatrix InverseLUInPlace()
    {
        // Check if the matrix is square.
        if (RowCount != ColumnCount)
        {
            throw new Exception("The matrix must be square.");
        }

        int toggle;
        int[] perm;  // out parameter
        DecomposeLUInPlace(out perm, out toggle); //ignore toggle

        // Check if the determinant is zero.
        if (DeterminantWithLU(data, toggle) == 0)
        {
            throw new Exception("The matrix is not invertible.");
        }

        int n = RowCount;

        double[] b = new double[n * n];
        for (int j = 0; j < n; j++) //colunas
        {
            int indexColj = j * n;
            for (int i = 0; i < n; ++i) //linhas
            {
                if (j == perm[i])
                {
                    b[i + indexColj] = 1.0;
                    break;
                }
            }
        }
        SolveLinearLUInPlace(b);
        this.data = b;
        return this;
    }
    public double[] DecomposeLUInPlace(out int[] perm, out int toggle)
    {
        // Crout's LU decomposition for matrix determinant and inverse
        // stores combined lower & upper in lum[][]
        // stores row permuations into perm[]
        // returns +1 or -1 according to even or odd number of row permutations
        // lower gets dummy 1.0s on diagonal (0.0s above)
        // upper gets lum values on diagonal (0.0s below)

        double[] data = this.data; //para tornar o acesso mais rápido
        _ = data[data.Length - 1]; //para tornar o acesso mais rápido

        toggle = +1; // even (+1) or odd (-1) row permutatuions
        int n = RowCount;

        // make perm[]
        perm = new int[n];
        for (int i = 0; i < n; ++i)
            perm[i] = i;

        for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
        {
            int indexColj = j * n;
            double max = Abs(data[j + indexColj]);
            int piv = j;

            for (int i = j + 1; i < n; ++i) // find pivot index
            {
                double xij = Abs(data[i + indexColj]);
                if (xij > max)
                {
                    max = xij;
                    piv = i;
                }
            }

            if (piv != j)
            {
                for (int k = 0; k < n; k++)
                {
                    (data[piv + k * n], data[j + k * n]) = (data[j + k * n], data[piv + k * n]); // swap rows j, piv
                }

                (perm[piv], perm[j]) = (perm[j], perm[piv]); // swap perm elements

                toggle = -toggle;
            }

            double xjj = data[j + indexColj];
            if (xjj != 0.0)
            {
                for (int i = j + 1; i < n; ++i)
                {
                    double xij = data[i + indexColj] / xjj;
                    data[i + indexColj] = xij;
                    for (int k = j + 1; k < n; ++k)
                        data[i + k * n] -= xij * data[j + k * n];
                }
            }

        }
        return data;
    }
    internal double[] SolveLinearLUInPlace(double[] b)
    {

        double[] data = this.data; //para tornar o acesso mais rápido
        int nTotal = data.Length;
        _ = data[nTotal - 1]; //para tornar o acesso mais rápido

        int n = RowCount;

        for (int c = 0; c < n; c++)
        {
            int indexColc = c * n;
            for (int i = 1; i < n; ++i)
            {
                double sum = b[i + indexColc];
                for (int j = 0; j < i; ++j)
                {
                    sum -= data[i + j * n] * b[j + indexColc];
                }
                b[i + indexColc] = sum;
            }

            b[n - 1 + indexColc] /= data[n - 1 + (n - 1) * n];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = b[i + indexColc];
                for (int j = i + 1; j < n; ++j)
                {
                    sum -= data[i + j * n] * b[j + indexColc];
                }
                b[i + indexColc] = sum / data[i + i * n];
            }
        }
        return b;
    }
    public double DeterminantWithLU(double[] matrixLU, double toggleLU)
    {
        int n = RowCount;
        for (int i = 0; i < n; ++i)
        {
            //int indexColi = i * n;
            toggleLU *= matrixLU[i + i * n];
        }
        return toggleLU;
    }

    #region From MathNet.Numerics.LinearAlgebra.Double.Factorization.LU
    /// <summary>
    /// Computes the LUP factorization of A. P*A = L*U.
    /// </summary>
    /// <param name="data">An <paramref name="order"/> by <paramref name="order"/> matrix. The matrix is overwritten with the
    /// the LU factorization on exit. The lower triangular factor L is stored in under the diagonal of <paramref name="data"/> (the diagonal is always 1.0
    /// for the L factor). The upper triangular factor U is stored on and above the diagonal of <paramref name="data"/>.</param>
    /// <param name="order">The order of the square matrix <paramref name="data"/>.</param>
    /// <param name="ipiv">On exit, it contains the pivot indices. The size of the array must be <paramref name="order"/>.</param>
    /// <remarks>This is equivalent to the GETRF LAPACK routine.</remarks>
    public void LUFactor(double[] data, int order, int[] ipiv)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (ipiv == null)
        {
            throw new ArgumentNullException(nameof(ipiv));
        }

        if (data.Length != order * order)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(data));
        }

        if (ipiv.Length != order)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
        }

        // Initialize the pivot matrix to the identity permutation.
        for (var i = 0; i < order; i++)
        {
            ipiv[i] = i;
        }

        var vecLUcolj = new double[order];

        // Outer loop.
        for (var j = 0; j < order; j++)
        {
            var indexj = j * order;
            var indexjj = indexj + j;

            // Make a copy of the j-th column to localize references.
            for (var i = 0; i < order; i++)
            {
                vecLUcolj[i] = data[indexj + i];
            }

            // Apply previous transformations.
            for (var i = 0; i < order; i++)
            {
                // Most of the time is spent in the following dot product.
                var kmax = Min(i, j);
                var s = 0.0;
                for (var k = 0; k < kmax; k++)
                {
                    s += data[(k * order) + i] * vecLUcolj[k];
                }

                data[indexj + i] = vecLUcolj[i] -= s;
            }

            // Find pivot and exchange if necessary.
            var p = j;
            for (var i = j + 1; i < order; i++)
            {
                if (Abs(vecLUcolj[i]) > Abs(vecLUcolj[p]))
                {
                    p = i;
                }
            }

            if (p != j)
            {
                for (var k = 0; k < order; k++)
                {
                    var indexk = k * order;
                    var indexkp = indexk + p;
                    var indexkj = indexk + j;
                    (data[indexkp], data[indexkj]) = (data[indexkj], data[indexkp]);
                }

                ipiv[j] = p;
            }

            // Compute multipliers.
            if (j < order & data[indexjj] != 0.0)
            {
                for (var i = j + 1; i < order; i++)
                {
                    data[indexj + i] /= data[indexjj];
                }
            }
        }
    }

    /// <summary>
    /// Computes the inverse of matrix using LU factorization.
    /// </summary>
    /// <param name="a">The N by N matrix to invert. Contains the inverse On exit.</param>
    /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
    /// <remarks>This is equivalent to the GETRF and GETRI LAPACK routines.</remarks>
    public void LUInverse(double[] a, int order)
    {
        if (a == null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        if (a.Length != order * order)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(a));
        }

        var ipiv = new int[order];
        LUFactor(a, order, ipiv);
        LUInverseFactored(a, order, ipiv);
    }

    /// <summary>
    /// Computes the inverse of a previously factored matrix.
    /// </summary>
    /// <param name="a">The LU factored N by N matrix.  Contains the inverse On exit.</param>
    /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
    /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
    /// <remarks>This is equivalent to the GETRI LAPACK routine.</remarks>
    public void LUInverseFactored(double[] a, int order, int[] ipiv)
    {
        if (a == null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        if (ipiv == null)
        {
            throw new ArgumentNullException(nameof(ipiv));
        }

        if (a.Length != order * order)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(a));
        }

        if (ipiv.Length != order)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
        }

        var inverse = new double[a.Length];
        for (var i = 0; i < order; i++)
        {
            inverse[i + (order * i)] = 1.0;
        }

        LUSolveFactored(order, a, order, ipiv, inverse);
        inverse.Copy(a);
    }

    /// <summary>
    /// Solves A*X=B for X using LU factorization.
    /// </summary>
    /// <param name="columnsOfB">The number of columns of B.</param>
    /// <param name="a">The square matrix A.</param>
    /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
    /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
    /// <remarks>This is equivalent to the GETRF and GETRS LAPACK routines.</remarks>
    public void LUSolve(int columnsOfB, double[] a, int order, double[] b)
    {
        if (a == null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        if (b == null)
        {
            throw new ArgumentNullException(nameof(b));
        }

        if (a.Length != order * order)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(a));
        }

        if (b.Length != order * columnsOfB)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(b));
        }

        if (ReferenceEquals(a, b))
        {
            throw new ArgumentException("Arguments must be different objects.");
        }

        var ipiv = new int[order];
        var clone = new double[a.Length];
        a.Copy(clone);
        LUFactor(clone, order, ipiv);
        LUSolveFactored(columnsOfB, clone, order, ipiv, b);
    }

    /// <summary>
    /// Solves A*X=B for X using a previously factored A matrix.
    /// </summary>
    /// <param name="columnsOfB">The number of columns of B.</param>
    /// <param name="a">The factored A matrix.</param>
    /// <param name="order">The order of the square matrix <paramref name="a"/>.</param>
    /// <param name="ipiv">The pivot indices of <paramref name="a"/>.</param>
    /// <param name="b">On entry the B matrix; on exit the X matrix.</param>
    /// <remarks>This is equivalent to the GETRS LAPACK routine.</remarks>
    public void LUSolveFactored(int columnsOfB, double[] a, int order, int[] ipiv, double[] b)
    {
        if (a == null)
        {
            throw new ArgumentNullException(nameof(a));
        }

        if (ipiv == null)
        {
            throw new ArgumentNullException(nameof(ipiv));
        }

        if (b == null)
        {
            throw new ArgumentNullException(nameof(b));
        }

        if (a.Length != order * order)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(a));
        }

        if (ipiv.Length != order)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(ipiv));
        }

        if (b.Length != order * columnsOfB)
        {
            throw new ArgumentException("The array arguments must have the same length.", nameof(b));
        }

        if (ReferenceEquals(a, b))
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
            for (var j = 0; j < columnsOfB; j++)
            {
                var indexk = j * order;
                var indexkp = indexk + p;
                var indexkj = indexk + i;
                (b[indexkp], b[indexkj]) = (b[indexkj], b[indexkp]);
            }
        }

        // Solve L*Y = P*B
        for (var k = 0; k < order; k++)
        {
            var korder = k * order;
            for (var i = k + 1; i < order; i++)
            {
                for (var j = 0; j < columnsOfB; j++)
                {
                    var index = j * order;
                    b[i + index] -= b[k + index] * a[i + korder];
                }
            }
        }

        // Solve U*X = Y;
        for (var k = order - 1; k >= 0; k--)
        {
            var korder = k + (k * order);
            for (var j = 0; j < columnsOfB; j++)
            {
                b[k + (j * order)] /= a[korder];
            }

            korder = k * order;
            for (var i = 0; i < k; i++)
            {
                for (var j = 0; j < columnsOfB; j++)
                {
                    var index = j * order;
                    b[i + index] -= b[k + index] * a[i + korder];
                }
            }
        }
    }
    #endregion

}
