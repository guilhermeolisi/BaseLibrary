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

    /// <summary>
    /// Estrutura conhecida da matriz. O default é <see cref="MatrixStructure.General"/>;
    /// tipos especializados (diagonal, triangular, esparsa…) sobrescrevem para habilitar
    /// kernels otimizados no despacho de <see cref="MatrixMethods.Multiply(iMatrix, iMatrix)"/>.
    /// </summary>
    MatrixStructure Structure => MatrixStructure.General;

    double GetValue(int row, int column);
    double GetValueTransposed(int row, int column);
    iVector GetRow(int index);
    double[] GetRowArray(int row);
    void SetRow(int row, double[] value);
}
