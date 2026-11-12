using Sindarin.Math.Matrix;
using static BaseLibrary.Math.MathMethods;
using static System.Math;

namespace BaseLibrary.Math;

public static class GeometryMethods
{
    public static double PositiveAngle(double angle)
    {
        if (angle < 0)
        {
            return 2 * PI + angle;
        }
        return angle;
    }
    public static double Length(this int[] hkl1, iMatrix G) => Length(hkl1.ToDouble(), G);
    public static double Length(this double[] vector, iMatrix G)
    {
        iMatrix point1;
        point1 = MatrixArrays.FromColumnArrays(vector);
        return Sqrt(point1.TransposeThisAndMultiply(G).Multiply(point1)[0, 0]);
    }
    public static double CosPhi(int[] hkl1, int[] hkl2, iMatrix GS) => CosPhi(hkl1.ToDouble(), hkl2.ToDouble(), GS);
    public static double CosPhi(double[] vetor1, double[] vetor2, iMatrix G)
    {
        iMatrix point1;
        //point1 = MatrixArrays.FromColumnArrays(new double[] { vetor1S[0], vetor1S[1], vetor1S[2] });
        point1 = MatrixArrays.FromColumnArrays(vetor1);
        iMatrix point2;
        //point2 = MatrixArrays.FromColumnArrays(new double[] { vetor2S[0], vetor2S[1], vetor2S[2] });
        point2 = MatrixArrays.FromColumnArrays(vetor2);

        double r1r2 = (point1.TransposeThisAndMultiply(G)).Multiply(point2)[0, 0];
        double r1 = Sqrt((point1.TransposeThisAndMultiply(G)).Multiply(point1)[0, 0]);
        double r2 = Sqrt((point2.TransposeThisAndMultiply(G)).Multiply(point2)[0, 0]);
#if DEBUG
        iMatrix trash = point1.TransposeThisAndMultiply(G);
        iMatrix trash2 = trash.Multiply(point2);
        iMatrix trash3 = point1.TransposeThisAndMultiply(G);
        iMatrix trash4 = trash3.Multiply(point1);
        double trash5 = Sqrt(trash4[0, 0]);
        //TARSH
        double SCoss = r1r2 / (r1 * r2);
        double angle = Degrees(Acos(SCoss));
#endif
        return r1r2 / (r1 * r2);
    }
    public static double CalculeScalarProduct(int[] vector1, int[] vector2, iMatrix G)
    {
        // Conta está no "Geometria Cristalgoráfica.docx"

        iMatrix mVector1 = new MatrixArrays(vector1.ToDouble());
        iMatrix mVector2 = MatrixArrays.FromColumnArrays(vector2.ToDouble());

        var mResult = mVector1.Multiply(G).Multiply(mVector2);

        return mResult[0, 0];
    }
    public static double Dot(this Point3D a, Point3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    public static Point3D Cross(this Point3D a, Point3D b)
    {
        return new Point3D(
            Round(a.Y * b.Z - a.Z * b.Y, 14),
            Round(a.Z * b.X - a.X * b.Z, 14),
            Round(a.X * b.Y - a.Y * b.X, 14)
        );
    }
    public static double Dot(Point2D a, Point2D b)
    {
        return a.X * b.X + a.Y * b.Y;
    }
}
