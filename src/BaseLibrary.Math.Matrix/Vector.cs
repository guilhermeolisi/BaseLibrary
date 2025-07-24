namespace Sindarin.Math.Matrix;

public class Vector : iVector
{
    public double this[int row, int column]
    {
        get
        {

            if (row < 0 || row >= RowCount)
                throw new IndexOutOfRangeException("Index is outside the class boundaries");
            return values[column];
        }
        set
        {

            if (row < 0 || row >= RowCount)
                throw new IndexOutOfRangeException("Index is outside the class boundaries");
            values[column] = value;
        }
    }
    public double this[int index]
    {
        get
        {
            return values[index];
        }
        set
        {
            values[index] = value;
        }
    }
    public int RowCount => 1;

    public int ColumnCount => values.Length;

    public double GetValue(int row, int column)
    {
        return values[column];
    }
    public double GetValueTransposed(int row, int column)
    {

        if (row != 1)
        {
            throw new IndexOutOfRangeException("Index is outside the class boundaries");
        }
        return values[column];
    }
    public iVector GetRow(int row)
    {
        return new Vector(GetRowArray(row));
    }

    public double[] GetRowArray(int row)
    {

        return values;

    }

    public void SetRow(int row, double[] rowArray)
    {
        if (row != 1)
        {
            throw new IndexOutOfRangeException("Index is outside the class boundaries");
        }
        values = rowArray;
    }

    public Vector(int length)
    {
        // Check if the rows and columns are valid.
        if (length <= 0)
        {
            throw new Exception("The columns must be greater than zero.");
        }
        values = new double[length];
    }
    public Vector(double[] values)
    {
        this.values = values;
    }
    internal double[] values;
}
