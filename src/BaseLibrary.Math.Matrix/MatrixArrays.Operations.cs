using static System.Math;

namespace Sindarin.Math.Matrix;

public partial class MatrixArrays
{
    /// <summary>
    /// Multiplies two matrices together.
    /// </summary>
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
        MatrixArrays result = new MatrixArrays(RowCount, matrix2.ColumnCount);

        double[][] data = this.data; // To improve performance
        double[] data2 = matrix2.data;
        double[][] dataResult = result.data;

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
                    dataResult[i][j] += data[i][k] * data2[k + index2Colj];
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
        MatrixArrays result = new MatrixArrays(RowCount, matrix2.ColumnCount);

        double[][] data = this.data; // To improve performance
        double[][] data2 = matrix2.data;
        double[][] dataResult = result.data;

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
                    dataResult[i][j] += data[i][k] * data2[k][j];
                }
            }
        });

        // Return the result 
        return result;
    }
    public iMatrix TransposeAndMultiply(MatrixDense matrix2)
    {
        //TODO fazer adaptações para calcular a partir do data

        // Check if the matrices can be multiplied.
        // Número de linhas da primeira matriz deve ser igual ao número de linhas da segunda matriz.
        if (RowCount != matrix2.RowCount)
        {
            throw new Exception("The matrices cannot be multiplied.");
        }

        // Create the result 
        MatrixArrays result = new MatrixArrays(ColumnCount, matrix2.ColumnCount);

        double[][] data = this.data; // To improve performance
        double[] data2 = matrix2.data;
        double[][] dataResult = result.data;

        // Loop parallel through the rows of the first 
        Parallel.For(0, ColumnCount, i =>
        {
            // Loop through the columns of the second 
            for (int j = 0; j < matrix2.ColumnCount; j++)
            {
                int indexColj = j * matrix2.RowCount;
                // Loop through the rows of the second 
                for (int k = 0; k < matrix2.RowCount; k++)
                {
                    // Add the product of the two matrices to the result 
                    dataResult[i][j] += data[k][i] * data2[k + indexColj];
                }
            }
        });

        // Return the result 
        return result;
    }
    public iMatrix TransposeAndMultiply(MatrixArrays matrix2)
    {
        //TODO fazer adaptações para calcular a partir do data

        // Check if the matrices can be multiplied.
        // Número de linhas da primeira matriz deve ser igual ao número de linhas da segunda matriz.
        if (RowCount != matrix2.RowCount)
        {
            throw new Exception("The matrices cannot be multiplied.");
        }

        // Create the result 
        MatrixArrays result = new MatrixArrays(ColumnCount, matrix2.ColumnCount);

        double[][] data = this.data; // To improve performance
        double[][] data2 = matrix2.data;
        double[][] dataResult = result.data;

        // Loop parallel through the rows of the first 
        Parallel.For(0, ColumnCount, i =>
        {
            // Loop through the columns of the second 
            for (int j = 0; j < matrix2.ColumnCount; j++)
            {
                int indexColj = j * matrix2.RowCount;
                // Loop through the rows of the second 
                for (int k = 0; k < matrix2.RowCount; k++)
                {
                    // Add the product of the two matrices to the result 
                    dataResult[i][j] += data[k][i] * data2[k][j];
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
        double[][] lum = DecomposeLU(out perm, out toggle); //ignore toggle

        // Check if the determinant is zero.
        if (lum.DeterminantWithLU(toggle) == 0)
        {
            throw new Exception("The matrix is not invertible.");
        }

        int n = data.Length;

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
        return new MatrixArrays(result);
    }
    public double[][] DecomposeLU(out int[] perm, out int toggle)
    {
        // Crout's LU decomposition for matrix determinant and inverse
        // stores combined lower & upper in lum[][]
        // stores row permuations into perm[]
        // returns +1 or -1 according to even or odd number of row permutations
        // lower gets dummy 1.0s on diagonal (0.0s above)
        // upper gets lum values on diagonal (0.0s below)

        double[][] data = this.data; //para tornar o acesso mais rápido

        toggle = +1; // even (+1) or odd (-1) row permutatuions
        int n = data.Length;

        // make a copy of m[][] into result lu[][]
        double[][] lum = new double[n][];
        for (int i = 0; i < n; ++i)
        {
            lum[i] = new double[n];
            for (int j = 0; j < n; ++j)
                lum[i][j] = data[i][j];
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
    public double[] SolveLinearLU(double[][] lum, double[] b)
    {
        int n = lum.Length;
        double[] x = new double[n];

        for (int i = 0; i < n; ++i)
            x[i] = b[i];
        //b.AsSpan().CopyTo(x);

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
    public double DeterminantWithLU(double[][] matrixLU, double toggleLU)
    {
        int n = RowCount;
        for (int i = 0; i < n; ++i)
        {
            //int indexColi = i * n;
            toggleLU *= matrixLU[i][i];
        }
        return toggleLU;
    }

}
