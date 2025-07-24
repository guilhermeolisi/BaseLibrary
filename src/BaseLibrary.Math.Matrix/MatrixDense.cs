namespace Sindarin.Math.Matrix;

public partial class MatrixDense : iMatrix
{
    //LU from MathNet from class DenseColumnMajorMatrixStorage<T>
    //public override T At(int row, int column)
    //{
    //    return Data[(column * RowCount) + row];
    //}

    public double this[int row, int column]
    {
        get
        {
            return data[row + column * RowCount];
        }
        set
        {
            data[row + column * RowCount] = value;
        }
    }
    public int RowCount { get; private set; }
    public int ColumnCount { get; private set; }

    public double GetValue(int row, int column)
    {
        return data[row + column * RowCount];
    }
    public double GetValueTransposed(int row, int column)
    {
        return data[column + row * RowCount]; //TODO: check
    }
    public iVector GetRow(int index)
    {
        return new Vector(GetRowArray(index));
    }
    public double[] GetRowArray(int index)
    {
        return null;
        //return data[index]; //TODO: check
    }
    public double[] GetData()
    {
        return data;
    }
    public void SetRow(int index, double[] rowValue)
    {
        if (rowValue.Length != RowCount)
        {
            throw new Exception("The length of the row must be equal to the number of columns.");
        }
        //data[index] = rowValue; //TODO: check
    }
    public MatrixDense(int rows, int columns)
    {
        // Check if the rows and columns are valid.
        if (rows <= 0 || columns <= 0)
        {
            throw new Exception("The rows and columns must be greater than zero.");
        }
        createMatrix(rows, columns);
    }
    public MatrixDense(double[] data, int rows)
    {
        // Check if the rows and columns are valid.
        double colDouble = data.Length / ((double)rows);
        int col = (int)colDouble;
        if (colDouble != col)
        {
            throw new Exception("The length of data must be multiple of columnLength.");
        }
        RowCount = rows;
        ColumnCount = col;
        this.data = data;
    }

    /// <summary>
    /// Values of the matrix. Rows, columns, etc.
    /// </summary>
    internal double[] data;
    private void createMatrix(int rows, int columns)
    {
        RowCount = rows;
        ColumnCount = columns;

        data = new double[rows * columns];
    }
    public static MatrixDense Identity(int length)
    {
        // Check if the rows and columns are valid.
        if (length <= 0)
        {
            throw new Exception("The length must be greater than zero.");
        }
        MatrixDense matrix = new MatrixDense(length, length);
        for (int i = 0; i < length; i++)
        {
            matrix.data[i * length + i] = 1;
        }
        return matrix;
    }

}
