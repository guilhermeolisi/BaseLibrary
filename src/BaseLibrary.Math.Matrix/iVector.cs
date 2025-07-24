namespace Sindarin.Math.Matrix;

public interface iVector : iMatrix
{
    double this[int index] { get; set; }
}
