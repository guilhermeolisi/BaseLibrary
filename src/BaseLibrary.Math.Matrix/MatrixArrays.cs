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
        if (rowValue.Length != RowCount)
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
