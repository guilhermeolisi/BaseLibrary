using BaseLibrary.Numbers;
using static System.Math;

namespace BaseLibrary.Math;

public static class PointExtension
{
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
    public static double Length(this Point3D v)
    {
        return Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    }
    public static Point3D Normalize(this Point3D v)
    {
        double n = Length(v);
        if (n < 1e-14)
            throw new InvalidOperationException("Vetor of length zero.");

        return new Point3D(v.X / n, v.Y / n, v.Z / n);
    }
    public static Point3D TryInteger(this Point3D v, int until = int.MinValue)
    {

        if (until == int.MinValue)
        {
            until = 19;
        }

        double min = Abs(v.X) < 1E-10 ? double.MaxValue : Abs(v.X);
        if (Abs(v.Y) < min && Abs(v.Y) > 1E-10)
            min = Abs(v.Y);
        if (Abs(v.Z) < min && Abs(v.Z) > 1E-10)
            min = Abs(v.Z);
        v = new(v.X / min, v.Y / min, v.Z / min);

        Point3D result = new(Round(v.X, 12), Round(v.Y, 12), Round(v.Z, 12));

        int integer = 2;
        //bool cont = !(v.X.IsInteger() && v.X.IsInteger() && v.X.IsInteger());
        while ((!result.X.IsInteger() || !result.Y.IsInteger() || !result.Z.IsInteger()) && integer <= until)
        {
            result = new(v.X * integer, v.Y * integer, v.Z * integer);
            integer++;
        }
        if (integer > until)
            return new(Round(v.X, 12), Round(v.Y, 12), Round(v.Z, 12));
        return result;
    }
    public static Point3D RemoveZeroNegative(this Point3D v)
    {
        if ((v.X != 0 || double.IsPositive(v.X)) && (v.Y != 0 || double.IsPositive(v.Y)) && (v.Z != 0 && double.IsPositive(v.Z)))
            return v;

        return new(v.X.RemoveZeroNegative(), v.Y.RemoveZeroNegative(), v.Z.RemoveZeroNegative());
    }
}
