namespace Sindarin.Math.Matrix;

public partial class MatrixArrays : iMatrix
{
    public double this[int row, int column]
    {
        get
        {
            return data[row][column];
        }
        set
        {
            data[row][column] = value;
        }
    }
    public int RowCount => data.Length;
    public int ColumnCount => data[0].Length;

    public double GetValue(int row, int column)
    {
        return data[row][column];

    }
    public double GetValueTransposed(int row, int column)
    {
        return data[column][row];
    }
    public iVector GetRow(int index)
    {
        return new Vector(GetRowArray(index));
    }
    public double[] GetRowArray(int index)
    {
        return data[index];
    }
    public void SetRow(int index, double[] rowValue)
    {
        if (rowValue.Length != ColumnCount)
        {
            throw new Exception("The length of the row must be equal to the number of columns.");
        }
        data[index] = rowValue;
    }
    public MatrixArrays(int rows, int column)
    {
        // Check if the rows and columns are valid.
        if (rows <= 0 || column <= 0)
        {
            throw new Exception("The rows and columns must be greater than zero.");
        }
        createMatrix(rows, column);
    }
    public MatrixArrays(double[][] values)
    {
        // Check if the rows and columns are valid.
        if (values.Length <= 0 || values[0].Length <= 0)
        {
            throw new Exception("The rows and columns must be greater than zero.");
        }
        this.data = values;
    }
    /// <summary>
    /// Constrói uma matriz 1×n (uma LINHA) a partir de um vetor. Equivale ao
    /// <c>Matrix&lt;double&gt;.Build.DenseOfRowArrays(singleRow)</c> do MathNet.
    /// </summary>
    public MatrixArrays(double[] singleRow)
    {
        if (singleRow is null || singleRow.Length == 0)
            throw new Exception("The row must have at least one element.");
        data = new double[1][];
        data[0] = singleRow;
    }
    /// <summary>
    /// Constrói a partir de um array 2D (row-major). Equivale ao <c>Matrix&lt;double&gt;.Build.DenseOfArray</c>
    /// do MathNet.
    /// </summary>
    public MatrixArrays(double[,] values)
    {
        int rows = values.GetLength(0);
        int cols = values.GetLength(1);
        if (rows <= 0 || cols <= 0)
            throw new Exception("The rows and columns must be greater than zero.");
        data = new double[rows][];
        for (int i = 0; i < rows; i++)
        {
            data[i] = new double[cols];
            for (int j = 0; j < cols; j++)
                data[i][j] = values[i, j];
        }
    }

    /// <summary>
    /// Constrói a partir de arrays de COLUNAS (cada <c>columns[c]</c> é a coluna c). Equivale ao
    /// <c>Matrix&lt;double&gt;.Build.DenseOfColumnArrays</c> do MathNet.
    /// </summary>
    public static MatrixArrays FromColumnArrays(params double[][] columns)
    {
        int cols = columns.Length;
        if (cols <= 0 || columns[0].Length <= 0)
            throw new Exception("The rows and columns must be greater than zero.");
        int rows = columns[0].Length;
        MatrixArrays m = new MatrixArrays(rows, cols);
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
                m.data[r][c] = columns[c][r];
        return m;
    }

    public double[][] GetData() => data;
    public void SetData(double[][] data) => this.data = data;
    /// <summary>
    /// Values of the matrix. Rows, columns, etc.
    /// </summary>
    internal double[][] data;
    private void createMatrix(int row, int column)
    {
        data = new double[row][];
        for (int i = 0; i < row; i++)
        {
            data[i] = new double[column];
        }
    }
    public static MatrixArrays Identity(int length)
    {
        // Check if the rows and columns are valid.
        if (length <= 0)
        {
            throw new Exception("The length must be greater than zero.");
        }
        MatrixArrays matrix = new MatrixArrays(length, length);
        for (int i = 0; i < length; i++)
        {
            matrix.data[i][i] = 1;
        }
        return matrix;
    }

}
