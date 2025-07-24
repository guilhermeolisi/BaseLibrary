namespace Sindarin.Math.Matrix;

public interface iMatrix
{
    double this[int row, int column] { get; set; }
    /// <summary>
    /// Lines of the matrix.
    /// </summary>
    int RowCount { get; }
    /// <summary>
    /// Columns of the matrix.
    /// </summary>
    int ColumnCount { get; }

    bool isSquare { get => ColumnCount == RowCount; }

    double GetValue(int row, int column);
    double GetValueTransposed(int row, int column);
    iVector GetRow(int index);
    double[] GetRowArray(int row);
    void SetRow(int row, double[] value);
}
