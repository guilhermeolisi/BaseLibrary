namespace Sindarin.Math.Matrix;

public class VectorWithOne : iVector
{
    public double this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= RowCount)
                throw new IndexOutOfRangeException("Index is outside the class boundaries");
            return indexValueOne == column ? 1 : 0;
        }
        set
        {
            throw new Exception("The VectorWithOne is read-only.");
        }
    }
    public double this[int index]
    {
        get
        {
            return indexValueOne == index ? 1 : 0;
        }
        set
        {
            throw new Exception("The VectorWithOne is read-only.");
        }
    }
    public int RowCount
    {
        get
        {
            return 1;
        }
    }

    public int ColumnCount
    {
        get
        {
            return length;
        }
    }

    public double GetValue(int row, int column)
    {

        return row == indexValueOne ? 1 : 0;

    }
    public double GetValueTransposed(int row, int column)
    {

        if (row != 1)
        {
            throw new IndexOutOfRangeException("Index is outside the class boundaries");
        }
        return column == indexValueOne ? 1 : 0;

    }
    public iVector GetRow(int row)
    {
        return new Vector(GetRowArray(row));
    }

    public double[] GetRowArray(int row)
    {

        double[] result = new double[length];
        result[indexValueOne] = 1;
        return result;

    }

    public void SetRow(int row, double[] value)
    {
        throw new Exception("The VectorWithOne is read-only.");
    }

    public VectorWithOne(int length, int indexOne)
    {
        // Check if the rows and columns are valid.
        if (length <= 0)
        {
            throw new Exception("The columns must be greater than zero.");
        }
        if (indexOne >= length)
        {
            throw new Exception("The index must be less than the length.");
        }
        indexValueOne = indexOne;
        this.length = length;
    }

    int indexValueOne;
    int length;
}
