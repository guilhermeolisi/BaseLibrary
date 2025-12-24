using static System.Math;

namespace BaseLibrary.Math;

public readonly record struct Point2D(double X, double Y)
{
    public static Point2D operator +(Point2D a, Point2D b)
    {
        return new Point2D(a.X + b.X, a.Y + b.Y);
    }
    public static Point2D operator -(Point2D a, Point2D b)
    {
        return new Point2D(a.X - b.X, a.Y - b.Y);
    }
    public static Point2D operator *(Point2D a, double scalar)
    {
        return new Point2D(a.X * scalar, a.Y * scalar);
    }
    public static Point2D operator /(Point2D a, double scalar)
    {
        return new Point2D(a.X / scalar, a.Y / scalar);
    }
    public static double Dot(Point2D a, Point2D b)
    {
        return a.X * b.X + a.Y * b.Y;
    }
    public static double Length(Point2D v)
    {
        return Sqrt(v.X * v.X + v.Y * v.Y);
    }
    public static Point2D Normalize(Point2D v)
    {
        double n = Length(v);
        if (n < 1e-14)
            throw new InvalidOperationException("Vetor of length zero.");

        return new Point2D(v.X / n, v.Y / n);
    }
    public static double DistanceBetweenPoints(Point2D a, Point2D b)
    {
        return Length(b - a);
    }
}
