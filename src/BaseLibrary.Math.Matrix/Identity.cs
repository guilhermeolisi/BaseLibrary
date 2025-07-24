namespace Sindarin.Math.Matrix;

public class Identity : iMatrix
{
    public double this[int row, int column]
    {
        get
        {
            //if (row < 0 || row >= length || column < 0 || column >= length)
            //    throw new IndexOutOfRangeException("Index is outside the class boundaries");
            //if (row == column)
            //    return 1;
            //else
            //    return 0;
            return row == column ? 1 : 0;
        }
        set
        {
            throw new Exception("The identity matrix is read-only.");
        }
    }

    public int RowCount => length;

    public int ColumnCount => length;
    public double GetValue(int row, int column)
    {
        if (row < 0 || row >= RowCount || column < 0 || column >= ColumnCount)
            throw new IndexOutOfRangeException("Index is outside the class boundaries");
        return row == column ? 1 : 0;
    }
    public double GetValueTransposed(int row, int column)
    {
        if (row < 0 || row >= RowCount || column < 0 || column >= ColumnCount)
            throw new IndexOutOfRangeException("Index is outside the class boundaries");
        return row == column ? 1 : 0;
    }
    public iVector GetRow(int row)
    {

        return new VectorWithOne(row, length);
    }

    public double[] GetRowArray(int row)
    {
        throw new NotImplementedException();
    }

    public void SetRow(int row, double[] value)
    {
        throw new NotImplementedException();
    }

    public Identity(int length)
    {
        // Check if the rows and columns are valid.
        if (length <= 0)
        {
            throw new Exception("The length must be greater than zero.");
        }
        this.length = length;
    }

    private int length;
}
